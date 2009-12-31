/*
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics
{
    [Flags]
    public enum WorldFlags
    {
        NewFixture = (1 << 0),
        Locked = (1 << 1),
    }

    /// The world class manages all physics entities, dynamic simulation,
    /// and asynchronous queries. The world also contains efficient memory
    /// management facilities.
    public class World
    {
        /// <summary>
        /// Called whenever a Joint is removed
        /// </summary>
        public JointRemovedDelegate JointRemoved;

        /// <summary>
        /// Called whenever a Fixture is removed
        /// </summary>
        public FixtureRemovedDelegate FixtureRemoved;

        /// Construct a world object.
        /// @param gravity the world gravity vector.
        /// @param doSleep improve performance by not simulating inactive bodies.
        public World(Vector2 gravity, bool doSleep)
        {
            WarmStarting = true;
            ContinuousPhysics = true;

            _allowSleep = doSleep;
            Gravity = gravity;

            _queryAABBCallbackWrapper = QueryAABBCallbackWrapper;
            _rayCastCallbackWrapper = RayCastCallbackWrapper;

            new DefaultContactFilter(this);
        }

        public Body CreateBody(BodyDef def)
        {
            Debug.Assert(!IsLocked);
            if (IsLocked)
            {
                return null;
            }

            var b = new Body(def, this);

            // Add to world doubly linked list.
            b._prev = null;
            b._next = _bodyList;
            if (_bodyList != null)
            {
                _bodyList._prev = b;
            }
            _bodyList = b;
            ++_bodyCount;

            return b;
        }

        /// Destroy a rigid body given a definition. No reference to the definition
        /// is retained. This function is locked during callbacks.
        /// @warning This automatically deletes all associated shapes and joints.
        /// @warning This function is locked during callbacks.
        public void DestroyBody(Body b)
        {
            Debug.Assert(_bodyCount > 0);
            Debug.Assert(!IsLocked);
            if (IsLocked)
            {
                return;
            }

            // Delete the attached joints.
            JointEdge je = b._jointList;
            while (je != null)
            {
                JointEdge je0 = je;
                je = je.Next;

                if (JointRemoved != null)
                {
                    JointRemoved(je0.Joint);
                }

                DestroyJoint(je0.Joint);
            }
            b._jointList = null;

            // Delete the attached contacts.
            ContactEdge ce = b._contactList;
            while (ce != null)
            {
                ContactEdge ce0 = ce;
                ce = ce.Next;
                _contactManager.Destroy(ce0.Contact);
            }
            b._contactList = null;

            // Delete the attached fixtures. This destroys broad-phase proxies.
            Fixture f = b._fixtureList;
            while (f != null)
            {
                Fixture f0 = f;
                f = f._next;

                if (FixtureRemoved != null)
                {
                    FixtureRemoved(f0);
                }

                f0.DestroyProxy(_contactManager._broadPhase);
                f0.Destroy();
            }
            b._fixtureList = null;
            b._fixtureCount = 0;

            // Remove world body list.
            if (b._prev != null)
            {
                b._prev._next = b._next;
            }

            if (b._next != null)
            {
                b._next._prev = b._prev;
            }

            if (b == _bodyList)
            {
                _bodyList = b._next;
            }

            --_bodyCount;
        }

        /// Create a joint to rain bodies together. No reference to the definition
        /// is retained. This may cause the connected bodies to cease colliding.
        /// @warning This function is locked during callbacks.
        public Joint CreateJoint(JointDef def)
        {
            Debug.Assert(!IsLocked);
            if (IsLocked)
            {
                return null;
            }

            Joint j = Joint.Create(def);

            // Connect to the world list.
            j.Prev = null;
            j.Next = _jointList;
            if (_jointList != null)
            {
                _jointList.Prev = j;
            }
            _jointList = j;
            ++_jointCount;

            // Connect to the bodies' doubly linked lists.
            j.EdgeA.Joint = j;
            j.EdgeA.Other = j.BodyB;
            j.EdgeA.Prev = null;
            j.EdgeA.Next = j.BodyA._jointList;

            if (j.BodyA._jointList != null)
                j.BodyA._jointList.Prev = j.EdgeA;

            j.BodyA._jointList = j.EdgeA;

            j.EdgeB.Joint = j;
            j.EdgeB.Other = j.BodyA;
            j.EdgeB.Prev = null;
            j.EdgeB.Next = j.BodyB._jointList;

            if (j.BodyB._jointList != null)
                j.BodyB._jointList.Prev = j.EdgeB;

            j.BodyB._jointList = j.EdgeB;

            Body bodyA = def.BodyA;
            Body bodyB = def.BodyB;

            // If the joint prevents collisions, then flag any contacts for filtering.
            if (def.CollideConnected == false)
            {
                ContactEdge edge = bodyB.GetContactList();
                while (edge != null)
                {
                    if (edge.Other == bodyA)
                    {
                        // Flag the contact for filtering at the next time step (where either
                        // body is awake).
                        edge.Contact.FlagForFiltering();
                    }

                    edge = edge.Next;
                }
            }

            // Note: creating a joint doesn't wake the bodies.

            return j;
        }

        /// Destroy a joint. This may cause the connected bodies to begin colliding.
        /// @warning This function is locked during callbacks.
        public void DestroyJoint(Joint j)
        {
            Debug.Assert(!IsLocked);
            if (IsLocked)
            {
                return;
            }

            bool collideConnected = j.CollideConnected;

            // Remove from the doubly linked list.
            if (j.Prev != null)
            {
                j.Prev.Next = j.Next;
            }

            if (j.Next != null)
            {
                j.Next.Prev = j.Prev;
            }

            if (j == _jointList)
            {
                _jointList = j.Next;
            }

            // Disconnect from island graph.
            Body bodyA = j.BodyA;
            Body bodyB = j.BodyB;

            // Wake up connected bodies.
            bodyA.SetAwake(true);
            bodyB.SetAwake(true);

            // Remove from body 1.
            if (j.EdgeA.Prev != null)
            {
                j.EdgeA.Prev.Next = j.EdgeA.Next;
            }

            if (j.EdgeA.Next != null)
            {
                j.EdgeA.Next.Prev = j.EdgeA.Prev;
            }

            if (j.EdgeA == bodyA._jointList)
            {
                bodyA._jointList = j.EdgeA.Next;
            }

            j.EdgeA.Prev = null;
            j.EdgeA.Next = null;

            // Remove from body 2
            if (j.EdgeB.Prev != null)
            {
                j.EdgeB.Prev.Next = j.EdgeB.Next;
            }

            if (j.EdgeB.Next != null)
            {
                j.EdgeB.Next.Prev = j.EdgeB.Prev;
            }

            if (j.EdgeB == bodyB._jointList)
            {
                bodyB._jointList = j.EdgeB.Next;
            }

            j.EdgeB.Prev = null;
            j.EdgeB.Next = null;

            Debug.Assert(_jointCount > 0);
            --_jointCount;

            // If the joint prevents collisions, then flag any contacts for filtering.
            if (collideConnected == false)
            {
                ContactEdge edge = bodyB.GetContactList();
                while (edge != null)
                {
                    if (edge.Other == bodyA)
                    {
                        // Flag the contact for filtering at the next time step (where either
                        // body is awake).
                        edge.Contact.FlagForFiltering();
                    }

                    edge = edge.Next;
                }
            }
        }

        /// Take a time step. This performs collision detection, integration,
        /// and constraint solution.
        /// @param timeStep the amount of time to simulate, this should not vary.
        /// @param velocityIterations for the velocity constraint solver.
        /// @param positionIterations for the position constraint solver.
        public void Step(float dt, int velocityIterations, int positionIterations)
        {
            // If new fixtures were added, we need to find the new contacts.
            if ((_flags & WorldFlags.NewFixture) == WorldFlags.NewFixture)
            {
                _contactManager.FindNewContacts();
                _flags &= ~WorldFlags.NewFixture;
            }

            //Lock the world
            _flags |= WorldFlags.Locked;

            TimeStep step;
            step.dt = dt;
            step.velocityIterations = velocityIterations;
            step.positionIterations = positionIterations;
            if (dt > 0.0f)
            {
                step.inv_dt = 1.0f / dt;
            }
            else
            {
                step.inv_dt = 0.0f;
            }

            step.dtRatio = _inv_dt0 * dt;

            step.warmStarting = WarmStarting;

            // Update contacts. This is where some contacts are destroyed.
            _contactManager.Collide();

            // Integrate velocities, solve velocity constraints, and integrate positions.
            if (step.dt > 0.0f)
            {
                Solve(ref step);
            }

            // Handle TOI events.
            if (ContinuousPhysics && step.dt > 0.0f)
            {
                SolveTOI(ref step);
            }

            if (step.dt > 0.0f)
            {
                _inv_dt0 = step.inv_dt;
            }

            _flags &= ~WorldFlags.Locked;
        }

        /// Call this after you are done with time steps to clear the forces. You normally
        /// call this after each call to Step, unless you are performing sub-steps.
        public void ClearForces()
        {
            for (Body body = _bodyList; body != null; body = body.GetNext())
            {
                body._force = Vector2.Zero;
                body._torque = 0.0f;
            }
        }

        /// Query the world for all fixtures that potentially overlap the
        /// provided AABB.
        /// @param callback a user implemented callback class.
        /// @param aabb the query box.
        public void QueryAABB(Func<Fixture, bool> callback, ref AABB aabb)
        {
            _queryAABBCallback = callback;
            _contactManager._broadPhase.Query(_queryAABBCallbackWrapper, ref aabb);
            _queryAABBCallback = null;
        }

        Func<Fixture, bool> _queryAABBCallback;
        Func<int, bool> _queryAABBCallbackWrapper;

        bool QueryAABBCallbackWrapper(int proxyId)
        {
            Fixture fixture = (Fixture)_contactManager._broadPhase.GetUserData(proxyId);
            return _queryAABBCallback(fixture);
        }

        /// Ray-cast the world for all fixtures in the path of the ray. Your callback
        /// controls whether you get the closest point, any point, or n-points.
        /// The ray-cast ignores shapes that contain the starting point.
        /// @param callback a user implemented callback class.
        /// @param point1 the ray starting point
        /// @param point2 the ray ending point
        public void RayCast(WorldRayCastCallback callback, Vector2 point1, Vector2 point2)
        {
            RayCastInput input = new RayCastInput();
            input.MaxFraction = 1.0f;
            input.Point1 = point1;
            input.Point2 = point2;

            _rayCastCallback = callback;
            _contactManager._broadPhase.RayCast(_rayCastCallbackWrapper, ref input);
            _rayCastCallback = null;
        }

        WorldRayCastCallback _rayCastCallback;
        RayCastCallback _rayCastCallbackWrapper;

        private float RayCastCallbackWrapper(ref RayCastInput input, int proxyId)
        {
            object userData = _contactManager._broadPhase.GetUserData(proxyId);
            Fixture fixture = (Fixture)userData;
            RayCastOutput output;
            bool hit = fixture.RayCast(out output, ref input);

            if (hit)
            {
                float fraction = output.Fraction;
                Vector2 point = (1.0f - fraction) * input.Point1 + fraction * input.Point2;
                return _rayCastCallback(fixture, point, output.Normal, fraction);
            }

            return input.MaxFraction;
        }

        /// Get the world body list. With the returned body, use Body.GetNext to get
        /// the next body in the world list. A null body indicates the end of the list.
        /// @return the head of the world body list.
        public Body GetBodyList()
        {
            return _bodyList;
        }

        /// Get the world joint list. With the returned joint, use Joint.GetNext to get
        /// the next joint in the world list. A null joint indicates the end of the list.
        /// @return the head of the world joint list.
        public Joint GetJointList()
        {
            return _jointList;
        }

        /// Get the world contact list. With the returned contact, use Contact.GetNext to get
        /// the next contact in the world list. A null contact indicates the end of the list.
        /// @return the head of the world contact list.
        /// @warning contacts are 
        public Contact GetContactList()
        {
            return _contactManager._contactList;
        }

        /// Enable/disable warm starting. For testing.
        public bool WarmStarting { get; set; }

        /// Enable/disable continuous physics. For testing.
        public bool ContinuousPhysics { get; set; }

        /// Get the number of broad-phase proxies.
        public int ProxyCount
        {
            get
            {
                return _contactManager._broadPhase.ProxyCount;
            }
        }

        /// Get the number of bodies.
        public int BodyCount
        {
            get
            {
                return _bodyCount;
            }
        }

        /// Get the number of joints.
        public int JointCount
        {
            get
            {
                return _jointCount;
            }
        }

        /// Get the number of contacts (each may have 0 or more contact points).
        public int ContactCount
        {
            get
            {
                return _contactManager._contactCount;
            }
        }

        /// Change the global gravity vector.
        public Vector2 Gravity { get; set; }

        /// Is the world locked (in the middle of a time step).
        public bool IsLocked
        {
            get
            {
                return (_flags & WorldFlags.Locked) == WorldFlags.Locked;
            }
            set
            {
                if (value)
                {
                    _flags |= WorldFlags.Locked;
                }
                else
                {
                    _flags &= ~WorldFlags.Locked;
                }
            }
        }

        public Body BodyList
        {
            get { return _bodyList; }
        }

        public ContactManager ContactManager
        {
            get { return _contactManager; }
        }

        public Joint JointList
        {
            get { return _jointList; }
        }

        private void Solve(ref TimeStep step)
        {
            // Size the island for the worst case.
            _island.Reset(_bodyCount,
                          _contactManager._contactCount,
                          _jointCount,
                          _contactManager);

            // Clear all the island flags.
            for (Body b = _bodyList; b != null; b = b._next)
            {
                b._flags &= ~BodyFlags.Island;
            }
            for (Contact c = _contactManager._contactList; c != null; c = c.Next)
            {
                c.Flags &= ~ContactFlags.Island;
            }
            for (Joint j = _jointList; j != null; j = j.Next)
            {
                j.IslandFlag = false;
            }

            // Build and simulate all awake islands.
#warning Remove extra allocs

            int stackSize = _bodyCount;
            Body[] stack = new Body[_bodyCount];
            for (Body seed = _bodyList; seed != null; seed = seed._next)
            {
                if ((seed._flags & (BodyFlags.Island)) != BodyFlags.None)
                {
                    continue;
                }

                if (seed.IsAwake() == false || seed.IsActive() == false)
                {
                    continue;
                }

                // The seed can be dynamic or kinematic.
                if (seed.GetBodyType() == BodyType.Static)
                {
                    continue;
                }

                // Reset island and stack.
                _island.Clear();
                int stackCount = 0;
                stack[stackCount++] = seed;
                seed._flags |= BodyFlags.Island;

                // Perform a depth first search (DFS) on the constraint graph.
                while (stackCount > 0)
                {
                    // Grab the next body off the stack and add it to the island.
                    Body b = stack[--stackCount];
                    Debug.Assert(b.IsActive());
                    _island.Add(b);

                    // Make sure the body is awake.
                    if (b.IsAwake() == false)
                    {
                        b.SetAwake(true);
                    }

                    // To keep islands as small as possible, we don't
                    // propagate islands across static bodies.
                    if (b.GetBodyType() == BodyType.Static)
                    {
                        continue;
                    }

                    // Search all contacts connected to this body.
                    for (ContactEdge ce = b._contactList; ce != null; ce = ce.Next)
                    {
                        // Has this contact already been added to an island?
                        if ((ce.Contact.Flags & ContactFlags.Island) != ContactFlags.None)
                        {
                            continue;
                        }

                        // Is this contact solid and touching?
                        if (ce.Contact.IsSensor() || !ce.Contact.IsEnabled() || !ce.Contact.IsTouching())
                        {
                            continue;
                        }

                        _island.Add(ce.Contact);
                        ce.Contact.Flags |= ContactFlags.Island;

                        Body other = ce.Other;

                        // Was the other body already added to this island?
                        if ((other._flags & BodyFlags.Island) != BodyFlags.None)
                        {
                            continue;
                        }

                        Debug.Assert(stackCount < stackSize);
                        stack[stackCount++] = other;
                        other._flags |= BodyFlags.Island;
                    }

                    // Search all joints connect to this body.
                    for (JointEdge je = b._jointList; je != null; je = je.Next)
                    {
                        if (je.Joint.IslandFlag)
                        {
                            continue;
                        }

                        Body other = je.Other;

                        // Don't simulate joints connected to inactive bodies.
                        if (other.IsActive() == false)
                        {
                            continue;
                        }

                        _island.Add(je.Joint);
                        je.Joint.IslandFlag = true;

                        if ((other._flags & BodyFlags.Island) != BodyFlags.None)
                        {
                            continue;
                        }

                        Debug.Assert(stackCount < stackSize);
                        stack[stackCount++] = other;
                        other._flags |= BodyFlags.Island;
                    }
                }

                _island.Solve(ref step, Gravity, _allowSleep);

                // Post solve cleanup.
                for (int i = 0; i < _island._bodyCount; ++i)
                {
                    // Allow static bodies to participate in other islands.
                    Body b = _island._bodies[i];
                    if (b.GetBodyType() == BodyType.Static)
                    {
                        b._flags &= ~BodyFlags.Island;
                    }
                }
            }

            // Synchronize fixtures, check for out of range bodies.
            for (Body b = _bodyList; b != null; b = b.GetNext())
            {
                if (!b.IsAwake() || !b.IsActive())
                {
                    continue;
                }

                if (b.GetBodyType() == BodyType.Static)
                {
                    continue;
                }

                // Update fixtures (for broad-phase).
                b.SynchronizeFixtures();
            }

            // Look for new contacts.
            _contactManager.FindNewContacts();
        }

        private void SolveTOI(ref TimeStep step)
        {
            // Reserve an island and a queue for TOI island solution.
            _island.Reset(_bodyCount,
                            Settings.MaxTOIContactsPerIsland,
                            Settings.MaxTOIJointsPerIsland,
                            _contactManager);

            //Simple one pass queue
            //Relies on the fact that we're only making one pass
            //through and each body can only be pushed/popped once.
            //To push: 
            //  queue[queueStart+queueSize++] = newElement;
            //To pop: 
            //	poppedElement = queue[queueStart++];
            //  --queueSize;
#warning More Body array Allocs
            int queueCapacity = _bodyCount;
            Body[] queue = new Body[_bodyCount];

            for (Body b = _bodyList; b != null; b = b._next)
            {
                b._flags &= ~BodyFlags.Island;
                b._sweep.t0 = 0.0f;
            }

            for (Contact c = _contactManager._contactList; c != null; c = c.Next)
            {
                // Invalidate TOI
                c.Flags &= ~(ContactFlags.Toi | ContactFlags.Island);
            }

            for (Joint j = _jointList; j != null; j = j.Next)
            {
                j.IslandFlag = false;
            }

            // Find TOI events and solve them.
            for (; ; )
            {
                // Find the first TOI.
                Contact minContact = null;
                float minTOI = 1.0f;

                for (Contact c = _contactManager._contactList; c != null; c = c.Next)
                {
                    // Can this contact generate a solid TOI contact?
                    if (c.IsSensor() || c.IsEnabled() == false || c.IsContinuous() == false)
                    {
                        continue;
                    }

                    // TODO_ERIN keep a counter on the contact, only respond to M TOIs per contact.

                    float toi;
                    if ((c.Flags & ContactFlags.Toi) != ContactFlags.None)
                    {
                        // This contact has a valid cached TOI.
                        toi = c.Toi;
                    }
                    else
                    {
                        // Compute the TOI for this contact.
                        Fixture s1 = c.GetFixtureA();
                        Fixture s2 = c.GetFixtureB();
                        Body b1 = s1.GetBody();
                        Body b2 = s2.GetBody();

                        if ((b1.GetBodyType() != BodyType.Dynamic || !b1.IsAwake()) &&
                            (b2.GetBodyType() != BodyType.Dynamic || !b2.IsAwake()))
                        {
                            continue;
                        }

                        // Put the sweeps onto the same time interval.
                        float t0 = b1._sweep.t0;

                        if (b1._sweep.t0 < b2._sweep.t0)
                        {
                            t0 = b2._sweep.t0;
                            b1._sweep.Advance(t0);
                        }
                        else if (b2._sweep.t0 < b1._sweep.t0)
                        {
                            t0 = b1._sweep.t0;
                            b2._sweep.Advance(t0);
                        }

                        Debug.Assert(t0 < 1.0f);

                        // Compute the time of impact.
                        toi = c.ComputeTOI(ref b1._sweep, ref b2._sweep);

                        Debug.Assert(0.0f <= toi && toi <= 1.0f);

                        // If the TOI is in range ...
                        if (0.0f < toi && toi < 1.0f)
                        {
                            // Interpolate on the actual range.
                            toi = Math.Min((1.0f - toi) * t0 + toi, 1.0f);
                        }


                        c.Toi = toi;
                        c.Flags |= ContactFlags.Toi;
                    }

                    if (Settings.Epsilon < toi && toi < minTOI)
                    {
                        // This is the minimum TOI found so far.
                        minContact = c;
                        minTOI = toi;
                    }
                }

                if (minContact == null || 1.0f - 100.0f * Settings.Epsilon < minTOI)
                {
                    // No more TOI events. Done!
                    break;
                }

                // Advance the bodies to the TOI.
                Fixture s1_2 = minContact.GetFixtureA();
                Fixture s2_2 = minContact.GetFixtureB();
                Body b1_2 = s1_2.GetBody();
                Body b2_2 = s2_2.GetBody();

                Sweep backup1 = b1_2._sweep;
                Sweep backup2 = b2_2._sweep;

                b1_2.Advance(minTOI);
                b2_2.Advance(minTOI);

                // The TOI contact likely has some new contact points.
                minContact.Update(_contactManager);
                minContact.Flags &= ~ContactFlags.Toi;

                // Is the contact solid?
                if (minContact.IsSensor() || !minContact.IsEnabled())
                {
                    // Restore the sweeps.
                    b1_2._sweep = backup1;
                    b2_2._sweep = backup2;
                    b1_2.SynchronizeTransform();
                    b2_2.SynchronizeTransform();
                    continue;
                }

                // Did numerical issues prevent a contact point from being generated?
                if (!minContact.IsTouching())
                {
                    // Give up on this TOI.
                    continue;
                }

                // Build the TOI island. We need a dynamic seed.
                Body seed = b1_2;
                if (seed.GetBodyType() != BodyType.Dynamic)
                {
                    seed = b2_2;
                }

                // Reset island and queue.
                _island.Clear();

                int queueStart = 0; // starting index for queue
                int queueSize = 0;  // elements in queue
                queue[queueStart + queueSize++] = seed;
                seed._flags |= BodyFlags.Island;

                // Perform a breadth first search (BFS) on the contact/joint graph.
                while (queueSize > 0)
                {
                    // Grab the next body off the stack and add it to the island.
                    Body b = queue[queueStart++];
                    --queueSize;

                    _island.Add(b);

                    // Make sure the body is awake.
                    if (b.IsAwake() == false)
                    {
                        b.SetAwake(true);
                    }


                    // To keep islands as small as possible, we don't
                    // propagate islands across static or kinematic bodies.
                    if (b.GetBodyType() != BodyType.Dynamic)
                    {
                        continue;
                    }

                    // Search all contacts connected to this body.
                    for (ContactEdge cEdge = b._contactList; cEdge != null; cEdge = cEdge.Next)
                    {
                        // Does the TOI island still have space for contacts?
                        if (_island._contactCount == _island._contactCapacity)
                        {
                            break;
                        }

                        // Has this contact already been added to an island?
                        if ((cEdge.Contact.Flags & ContactFlags.Island) != ContactFlags.None)
                        {
                            continue;
                        }

                        // Skip separate, sensor, or disabled contacts.
                        if (cEdge.Contact.IsSensor() ||
                            cEdge.Contact.IsEnabled() == false ||
                            cEdge.Contact.IsTouching() == false)
                        {
                            continue;
                        }

                        _island.Add(cEdge.Contact);
                        cEdge.Contact.Flags |= ContactFlags.Island;

                        // Update other body.
                        Body other = cEdge.Other;

                        // Was the other body already added to this island?
                        if ((other._flags & BodyFlags.Island) != BodyFlags.None)
                        {
                            continue;
                        }

                        // Synchronize the connected body.
                        if (other.GetBodyType() != BodyType.Static)
                        {
                            other.Advance(minTOI);
                            other.SetAwake(true);
                        }

                        Debug.Assert(queueStart + queueSize < queueCapacity);
                        queue[queueStart + queueSize] = other;
                        ++queueSize;
                        other._flags |= BodyFlags.Island;
                    }

                    for (JointEdge jEdge = b._jointList; jEdge != null; jEdge = jEdge.Next)
                    {
                        if (_island._jointCount == _island._jointCapacity)
                        {
                            continue;
                        }

                        if (jEdge.Joint.IslandFlag)
                        {
                            continue;
                        }

                        Body other = jEdge.Other;
                        if (other.IsActive() == false)
                        {
                            continue;
                        }

                        _island.Add(jEdge.Joint);

                        jEdge.Joint.IslandFlag = true;

                        if ((other._flags & BodyFlags.Island) != BodyFlags.None)
                        {
                            continue;
                        }

                        // Synchronize the connected body.
                        if (other.GetBodyType() != BodyType.Static)
                        {
                            other.Advance(minTOI);
                            other.SetAwake(true);
                        }

                        Debug.Assert(queueStart + queueSize < queueCapacity);
                        queue[queueStart + queueSize] = other;
                        ++queueSize;
                        other._flags |= BodyFlags.Island;
                    }
                }

                TimeStep subStep;
                subStep.warmStarting = false;
                subStep.dt = (1.0f - minTOI) * step.dt;
                subStep.inv_dt = 1.0f / subStep.dt;
                subStep.dtRatio = 0.0f;
                subStep.velocityIterations = step.velocityIterations;
                subStep.positionIterations = step.positionIterations;

                _island.SolveTOI(ref subStep);

                // Post solve cleanup.
                for (int i = 0; i < _island._bodyCount; ++i)
                {
                    // Allow bodies to participate in future TOI islands.
                    Body b = _island._bodies[i];
                    b._flags &= ~BodyFlags.Island;

                    if (b.IsAwake() == false)
                    {
                        continue;
                    }

                    if (b.GetBodyType() == BodyType.Static)
                    {
                        continue;
                    }

                    // Update fixtures (for broad-phase).
                    b.SynchronizeFixtures();

                    // Invalidate all contact TOIs associated with this body. Some of these
                    // may not be in the island because they were not touching.
                    for (ContactEdge ce = b._contactList; ce != null; ce = ce.Next)
                    {
                        ce.Contact.Flags &= ~ContactFlags.Toi;
                    }
                }

                int contactCount = _island._contactCount;
                for (int i = 0; i < contactCount; ++i)
                {
                    // Allow contacts to participate in future TOI islands.
                    Contact c = _island._contacts[i];
                    c.Flags &= ~(ContactFlags.Toi | ContactFlags.Island);
                }

                for (int i = 0; i < _island._jointCount; ++i)
                {
                    // Allow joints to participate in future TOI islands.
                    Joint j = _island._joints[i];
                    j.IslandFlag = false;
                }

                // Commit fixture proxy movements to the broad-phase so that new contacts are created.
                // Also, some contacts can be destroyed.
                _contactManager.FindNewContacts();
            }
        }

        private Island _island = new Island();
        internal WorldFlags _flags;

        internal ContactManager _contactManager = new ContactManager();

        private Body _bodyList;
        private Joint _jointList;

        private int _bodyCount;
        private int _jointCount;

        private bool _allowSleep;

        // This is used to compute the time step ratio to
        // support a variable time step.
        private float _inv_dt0;
    }
}
