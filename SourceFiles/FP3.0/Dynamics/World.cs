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
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FarseerPhysics.Dynamics
{
    [Flags]
    public enum WorldFlags
    {
        NewFixture = (1 << 0),
        Locked = (1 << 1),
        ClearForces = (1 << 2),
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

        /// ruct a world object.
        /// @param gravity the world gravity vector.
        /// @param doSleep improve performance by not simulating inactive bodies.
        public World(Vector2 gravity)
        {
            Gravity = gravity;

            _flags = WorldFlags.ClearForces;

            _queryAABBCallbackWrapper = QueryAABBCallbackWrapper;
            _rayCastCallbackWrapper = RayCastCallbackWrapper;

            new DefaultContactFilter(this);
        }

        /// Create a rigid body given a definition. No reference to the definition
        /// is retained.
        /// @warning This function is locked during callbacks.
        public Body CreateBody()
        {
            Debug.Assert(!IsLocked);
            if (IsLocked)
            {
                return null;
            }

            Body b = new Body(this);
            b.Active = true;

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

                f0.DestroyProxies(_contactManager.BroadPhase);
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

        public float UpdateTime { get; private set; }

        public float ContinuousPhysicsTime { get; private set; }

        public float NewContactsTime { get; private set; }

        public float ControllersUpdateTime { get; private set; }

        public float ContactsUpdateTime { get; private set; }

        public float SolveUpdateTime { get; private set; }

        /// Create a joint to rain bodies together. No reference to the definition
        /// is retained. This may cause the connected bodies to cease colliding.
        /// @warning This function is locked during callbacks.
        public void AddJoint(Joint j)
        {
            Debug.Assert(!IsLocked);
            if (IsLocked)
            {
                return;
            }

            // Connect to the world list.
            j._prev = null;
            j._next = _jointList;
            if (_jointList != null)
            {
                _jointList._prev = j;
            }
            _jointList = j;
            ++_jointCount;

            // Connect to the bodies' doubly linked lists.
            j._edgeA.Joint = j;
            j._edgeA.Other = j.BodyB;
            j._edgeA.Prev = null;
            j._edgeA.Next = j.BodyA._jointList;

            if (j.BodyA._jointList != null)
                j.BodyA._jointList.Prev = j._edgeA;

            j.BodyA._jointList = j._edgeA;

            // WIP David
            if (!j.IsFixedType())
            {
                j._edgeB.Joint = j;
                j._edgeB.Other = j.BodyA;
                j._edgeB.Prev = null;
                j._edgeB.Next = j.BodyB._jointList;

                if (j.BodyB._jointList != null)
                    j.BodyB._jointList.Prev = j._edgeB;

                j.BodyB._jointList = j._edgeB;
            }

            // WIP David
            if (!j.IsFixedType())
            {
                Body bodyA = j.BodyA;
                Body bodyB = j.BodyB;

                // If the joint prevents collisions, then flag any contacts for filtering.
                if (j.CollideConnected == false)
                {
                    ContactEdge edge = bodyB.ContactList;
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
            // Note: creating a joint doesn't wake the bodies.
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
            if (j._prev != null)
            {
                j._prev._next = j._next;
            }

            if (j._next != null)
            {
                j._next._prev = j._prev;
            }

            if (j == _jointList)
            {
                _jointList = j._next;
            }

            // Disconnect from island graph.
            Body bodyA = j.BodyA;
            Body bodyB = j.BodyB;

            // Wake up connected bodies.
            bodyA.Awake = true;
            bodyB.Awake = true;

            // Remove from body 1.
            if (j._edgeA.Prev != null)
            {
                j._edgeA.Prev.Next = j._edgeA.Next;
            }

            if (j._edgeA.Next != null)
            {
                j._edgeA.Next.Prev = j._edgeA.Prev;
            }

            if (j._edgeA == bodyA._jointList)
            {
                bodyA._jointList = j._edgeA.Next;
            }

            j._edgeA.Prev = null;
            j._edgeA.Next = null;

            // Remove from body 2
            if (j._edgeB.Prev != null)
            {
                j._edgeB.Prev.Next = j._edgeB.Next;
            }

            if (j._edgeB.Next != null)
            {
                j._edgeB.Next.Prev = j._edgeB.Prev;
            }

            if (j._edgeB == bodyB._jointList)
            {
                bodyB._jointList = j._edgeB.Next;
            }

            j._edgeB.Prev = null;
            j._edgeB.Next = null;

            Debug.Assert(_jointCount > 0);
            --_jointCount;

            // If the joint prevents collisions, then flag any contacts for filtering.
            if (collideConnected == false)
            {
                ContactEdge edge = bodyB.ContactList;
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

        Stopwatch _watch = new Stopwatch();

        /// Take a time step. This performs collision detection, integration,
        /// and raint solution.
        /// @param timeStep the amount of time to simulate, this should not vary.
        /// @param velocityIterations for the velocity raint solver.
        /// @param positionIterations for the position raint solver.
        public void Step(float dt)
        {
            if (Settings.EnableDiagnostics)
                _watch.Start();

            // If new fixtures were added, we need to find the new contacts.
            if ((_flags & WorldFlags.NewFixture) == WorldFlags.NewFixture)
            {
                _contactManager.FindNewContacts();
                _flags &= ~WorldFlags.NewFixture;
            }
            if (Settings.EnableDiagnostics)
                NewContactsTime = _watch.ElapsedTicks;

            _flags |= WorldFlags.Locked;

            TimeStep step;
            step.dt = dt;
            if (dt > 0.0f)
            {
                step.inv_dt = 1.0f / dt;
            }
            else
            {
                step.inv_dt = 0.0f;
            }

            step.dtRatio = _inv_dt0 * dt;

            //Update controllers
            //TODO:
            //foreach (Controller controller in Controllers)
            //{
            //    controller.Update(dt);
            //}

            //if (Settings.EnableDiagnostics)
            //    ControllersUpdateTime = _watch.ElapsedTicks - NewContactsTime;




            // Update contacts. This is where some contacts are destroyed.
            _contactManager.Collide();

            if (Settings.EnableDiagnostics)
                ContactsUpdateTime = _watch.ElapsedTicks - (NewContactsTime + ControllersUpdateTime);

            // Integrate velocities, solve velocity raints, and integrate positions.
            if (step.dt > 0.0f)
            {
                Solve(ref step);
            }

            if (Settings.EnableDiagnostics)
                SolveUpdateTime = _watch.ElapsedTicks - (NewContactsTime + ControllersUpdateTime + ContactsUpdateTime);

            // Handle TOI events.
            if (Settings.ContinuousPhysics && step.dt > 0.0f)
            {
                SolveTOI();
            }

            if (Settings.EnableDiagnostics)
                ContinuousPhysicsTime = _watch.ElapsedTicks -
                                        (NewContactsTime + ControllersUpdateTime + ContactsUpdateTime + SolveUpdateTime);

            if (step.dt > 0.0f)
            {
                _inv_dt0 = step.inv_dt;
            }

            if ((_flags & WorldFlags.ClearForces) != 0)
            {
                ClearForces();
            }

            _flags &= ~WorldFlags.Locked;

            if (Settings.EnableDiagnostics)
            {
                _watch.Stop();
                UpdateTime = _watch.ElapsedTicks;
                _watch.Reset();
            }

            //TODO: Fix and introduce timing
            //foreach (BreakableBody breakableBody in BreakableBodyList)
            //{
            //breakableBody.Update();
            //}
        }

        /// Call this after you are done with time steps to clear the forces. You normally
        /// call this after each call to Step, unless you are performing sub-steps. By default,
        /// forces will be automatically cleared, so you don't need to call this function.
        /// @see SetAutoClearForces
        public void ClearForces()
        {
            for (Body body = _bodyList; body != null; body = body.GetNext())
            {
                body._force = Vector2.Zero;
                body._torque = 0.0f;
            }
        }

        /// Set flag to control automatic clearing of forces after each time step.
        void SetAutoClearForces(bool flag)
        {
            if (flag)
            {
                _flags |= WorldFlags.ClearForces;
            }
            else
            {
                _flags &= ~WorldFlags.ClearForces;
            }
        }

        /// Get the flag that controls automatic clearing of forces after each time step.
        bool GetAutoClearForces()
        {
            return (_flags & WorldFlags.ClearForces) == WorldFlags.ClearForces;
        }

        /// Get the contact manager for testing.
        public ContactManager GetContactManager()
        {
            return _contactManager;
        }

        /// Query the world for all fixtures that potentially overlap the
        /// provided AABB.
        /// @param callback a user implemented callback class.
        /// @param aabb the query box.
        public void QueryAABB(Func<FixtureProxy, bool> callback, ref AABB aabb)
        {
            _queryAABBCallback = callback;
            _contactManager.BroadPhase.Query(_queryAABBCallbackWrapper, ref aabb);
            _queryAABBCallback = null;
        }

        Func<FixtureProxy, bool> _queryAABBCallback;
        Func<int, bool> _queryAABBCallbackWrapper;

        bool QueryAABBCallbackWrapper(int proxyId)
        {
            FixtureProxy proxy = _contactManager.BroadPhase.GetUserData<FixtureProxy>(proxyId);
            return _queryAABBCallback(proxy);
        }

        /// Ray-cast the world for all fixtures in the path of the ray. Your callback
        /// controls whether you get the closest point, any point, or n-points.
        /// The ray-cast ignores shapes that contain the starting point.
        /// @param callback a user implemented callback class.
        /// @param point1 the ray starting point
        /// @param point2 the ray ending point
        public void RayCast(RayCastCallback callback, Vector2 point1, Vector2 point2)
        {
            RayCastInput input = new RayCastInput();
            input.MaxFraction = 1.0f;
            input.Point1 = point1;
            input.Point2 = point2;

            _rayCastCallback = callback;
            _contactManager.BroadPhase.RayCast(_rayCastCallbackWrapper, ref input);
            _rayCastCallback = null;
        }

        RayCastCallback _rayCastCallback;
        RayCastCallbackInternal _rayCastCallbackWrapper;

        float RayCastCallbackWrapper(ref RayCastInput input, int proxyId)
        {
            FixtureProxy proxy = _contactManager.BroadPhase.GetUserData<FixtureProxy>(proxyId);
            Fixture fixture = proxy.Fixture;
            int index = proxy.ChildIndex;
            RayCastOutput output;
            bool hit = fixture.RayCast(out output, ref input, index);

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
            return _contactManager.ContactList;
        }

        /// Get the number of broad-phase proxies.
        public int ProxyCount
        {
            get
            {
                return _contactManager.BroadPhase.ProxyCount;
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
                return _contactManager.ContactCount;
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

        void Solve(ref TimeStep step)
        {
            // Size the island for the worst case.
            _island.Reset(_bodyCount,
                          _contactManager.ContactCount,
                          _jointCount,
                          _contactManager);

            // Clear all the island flags.
            for (Body b = _bodyList; b != null; b = b._next)
            {
                b._flags &= ~BodyFlags.Island;
            }
            for (Contact c = _contactManager.ContactList; c != null; c = c._next)
            {
                c._flags &= ~ContactFlags.Island;
            }
            for (Joint j = _jointList; j != null; j = j._next)
            {
                j._islandFlag = false;
            }

            // Build and simulate all awake islands.
            int stackSize = _bodyCount;
            if (stackSize > stack.Length)
                stack = new Body[Math.Max(stack.Length * 2, stackSize)];

            for (Body seed = _bodyList; seed != null; seed = seed._next)
            {
                if ((seed._flags & (BodyFlags.Island)) != BodyFlags.None)
                {
                    continue;
                }

                if (seed.Awake == false || seed.Active == false)
                {
                    continue;
                }

                // The seed can be dynamic or kinematic.
                if (seed.BodyType == BodyType.Static)
                {
                    continue;
                }

                // Reset island and stack.
                _island.Clear();
                int stackCount = 0;
                stack[stackCount++] = seed;
                seed._flags |= BodyFlags.Island;

                // Perform a depth first search (DFS) on the raint graph.
                while (stackCount > 0)
                {
                    // Grab the next body off the stack and add it to the island.
                    Body b = stack[--stackCount];
                    Debug.Assert(b.Active == true);
                    _island.Add(b);

                    // Make sure the body is awake.
                    b.Awake = true;

                    // To keep islands as small as possible, we don't
                    // propagate islands across static bodies.
                    if (b.BodyType == BodyType.Static)
                    {
                        continue;
                    }

                    // Search all contacts connected to this body.
                    for (ContactEdge ce = b._contactList; ce != null; ce = ce.Next)
                    {
                        Contact contact = ce.Contact;

                        // Has this contact already been added to an island?
                        if ((contact._flags & ContactFlags.Island) != ContactFlags.None)
                        {
                            continue;
                        }

                        // Is this contact solid and touching?
                        if (!ce.Contact.Enabled || !ce.Contact.IsTouching())
                        {
                            continue;
                        }

                        // Skip sensors.
                        bool sensorA = contact._fixtureA._isSensor;
                        bool sensorB = contact._fixtureB._isSensor;
                        if (sensorA || sensorB)
                        {
                            continue;
                        }

                        _island.Add(contact);
                        contact._flags |= ContactFlags.Island;

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
                        if (je.Joint._islandFlag == true)
                        {
                            continue;
                        }

                        Body other = je.Other;

                        // Don't simulate joints connected to inactive bodies.
                        if (other.Active == false)
                        {
                            continue;
                        }

                        _island.Add(je.Joint);
                        je.Joint._islandFlag = true;

                        if ((other._flags & BodyFlags.Island) != BodyFlags.None)
                        {
                            continue;
                        }

                        Debug.Assert(stackCount < stackSize);
                        stack[stackCount++] = other;
                        other._flags |= BodyFlags.Island;
                    }
                }

                _island.Solve(ref step, Gravity);

                // Post solve cleanup.
                for (int i = 0; i < _island._bodyCount; ++i)
                {
                    // Allow static bodies to participate in other islands.
                    Body b = _island._bodies[i];
                    if (b.BodyType == BodyType.Static)
                    {
                        b._flags &= ~BodyFlags.Island;
                    }
                }
            }

            // Synchronize fixtures, check for out of range bodies.
            for (Body b = _bodyList; b != null; b = b.GetNext())
            {
                // If a body was not in an island then it did not move.
                if ((b._flags & BodyFlags.Island) != BodyFlags.Island)
                {
                    continue;
                }

                if (b.BodyType == BodyType.Static)
                {
                    continue;
                }

                // Update fixtures (for broad-phase).
                b.SynchronizeFixtures();
            }

            // Look for new contacts.
            _contactManager.FindNewContacts();
        }

        // Advance a dynamic body to its first time of contact
        // and adjust the position to ensure clearance.
        void SolveTOI(Body body)
        {
            // Find the minimum contact.
            Contact toiContact = null;
            float toi = 1.0f;
            Body toiOther = null;
            bool found;
            int count;
            int iter = 0;

            bool bullet = body.IsBullet;

            // Iterate until all contacts agree on the minimum TOI. We have
            // to iterate because the TOI algorithm may skip some intermediate
            // collisions when objects rotate through each other.
            do
            {
                count = 0;
                found = false;
                for (ContactEdge ce = body._contactList; ce != null; ce = ce.Next)
                {
                    if (ce.Contact == toiContact)
                    {
                        continue;
                    }

                    Body other = ce.Other;
                    BodyType type = other.BodyType;

                    // Only bullets perform TOI with dynamic bodies.
                    if (bullet == true)
                    {
                        // Bullets only perform TOI with bodies that have their TOI resolved.
                        if ((other._flags & BodyFlags.Toi) == 0)
                        {
                            continue;
                        }

                        // No repeated hits on non-static bodies
                        if (type != BodyType.Static && (ce.Contact._flags & ContactFlags.BulletHit) != 0)
                        {
                            continue;
                        }
                    }
                    else if (type == BodyType.Dynamic)
                    {
                        continue;
                    }

                    // Check for a disabled contact.
                    Contact contact = ce.Contact;
                    if (contact.Enabled == false)
                    {
                        continue;
                    }

                    // Prevent infinite looping.
                    if (contact._toiCount > 10)
                    {
                        continue;
                    }

                    Fixture fixtureA = contact._fixtureA;
                    Fixture fixtureB = contact._fixtureB;
                    int indexA = contact._indexA;
                    int indexB = contact._indexB;

                    // Cull sensors.
                    if (fixtureA.IsSensor || fixtureB.IsSensor)
                    {
                        continue;
                    }

                    Body bodyA = fixtureA._body;
                    Body bodyB = fixtureB._body;

                    // Compute the time of impact in interval [0, minTOI]
                    TOIInput input = new TOIInput();
                    input.proxyA.Set(fixtureA.Shape, indexA);
                    input.proxyB.Set(fixtureB.Shape, indexB);
                    input.sweepA = bodyA._sweep;
                    input.sweepB = bodyB._sweep;
                    input.tMax = toi;

                    TOIOutput output;
                    TimeOfImpact.CalculateTimeOfImpact(out output, ref input);

                    if (output.State == TOIOutputState.Touching && output.t < toi)
                    {
                        toiContact = contact;
                        toi = output.t;
                        toiOther = other;
                        found = true;
                    }

                    ++count;
                }

                ++iter;
            } while (found && count > 1 && iter < 50);

            if (toiContact == null)
            {
                body.Advance(1.0f);
                return;
            }

            Sweep backup = body._sweep;
            body.Advance(toi);
            toiContact.Update(_contactManager);
            if (toiContact.Enabled == false)
            {
                // Contact disabled. Backup and recurse.
                body._sweep = backup;
                SolveTOI(body);
            }

            ++toiContact._toiCount;

            // Update all the valid contacts on this body and build a contact island.
            count = 0;
            for (ContactEdge ce = body._contactList; (ce != null) && (count < Settings.MaxTOIContacts); ce = ce.Next)
            {
                Body other = ce.Other;
                BodyType type = other.BodyType;

                // Only perform correction with static bodies, so the
                // body won't get pushed out of the world.
                if (type == BodyType.Dynamic)
                {
                    continue;
                }

                // Check for a disabled contact.
                Contact contact = ce.Contact;
                if (contact.Enabled == false)
                {
                    continue;
                }

                Fixture fixtureA = contact._fixtureA;
                Fixture fixtureB = contact._fixtureB;

                // Cull sensors.
                if (fixtureA.IsSensor || fixtureB.IsSensor)
                {
                    continue;
                }

                // The contact likely has some new contact points. The listener
                // gives the user a chance to disable the contact.
                if (contact != toiContact)
                {
                    contact.Update(_contactManager);
                }

                // Did the user disable the contact?
                if (contact.Enabled == false)
                {
                    // Skip this contact.
                    continue;
                }

                if (contact.IsTouching() == false)
                {
                    continue;
                }

                _toiContacts[count] = contact;
                ++count;
            }

            // Reduce the TOI body's overlap with the contact island.
            _toiSolver.Initialize(_toiContacts, count, body);

            float k_toiBaumgarte = 0.75f;
            //bool solved = false;
            for (int i = 0; i < 20; ++i)
            {
                bool contactsOkay = _toiSolver.Solve(k_toiBaumgarte);
                if (contactsOkay)
                {
                    //solved = true;
                    break;
                }
            }

            if (toiOther.BodyType != BodyType.Static)
            {
                toiContact._flags |= ContactFlags.BulletHit;
            }
        }

        // Sequentially solve TOIs for each body. We bring each
        // body to the time of contact and perform some position correction.
        // Time is not conserved.
        void SolveTOI()
        {
            // Prepare all contacts.
            for (Contact c = _contactManager.ContactList; c != null; c = c._next)
            {
                // Enable the contact
                c._flags |= ContactFlags.Enabled;

                // Set the number of TOI events for this contact to zero.
                c._toiCount = 0;
            }

            // Initialize the TOI flag.
            for (Body body = _bodyList; body != null; body = body._next)
            {
                // Kinematic, and static bodies will not be affected by the TOI event.
                // If a body was not in an island then it did not move.
                if ((body._flags & BodyFlags.Island) == 0 || body.BodyType == BodyType.Kinematic || body.BodyType == BodyType.Static)
                {
                    body._flags |= BodyFlags.Toi;
                }
                else
                {
                    body._flags &= ~BodyFlags.Toi;
                }
            }

            // Collide non-bullets.
            for (Body body = _bodyList; body != null; body = body._next)
            {
                if ((body._flags & BodyFlags.Toi) != BodyFlags.None)
                {
                    continue;
                }

                if (body.IsBullet == true)
                {
                    continue;
                }

                SolveTOI(body);

                body._flags |= BodyFlags.Toi;
            }

            // Collide bullets.
            for (Body body = _bodyList; body != null; body = body._next)
            {
                if ((body._flags & BodyFlags.Toi) != BodyFlags.None)
                {
                    continue;
                }

                if (body.IsBullet == false)
                {
                    continue;
                }

                SolveTOI(body);

                body._flags |= BodyFlags.Toi;
            }
        }

        TOISolver _toiSolver = new TOISolver();
        Contact[] _toiContacts = new Contact[Settings.MaxTOIContacts];
        internal Island _island = new Island();
        internal WorldFlags _flags;

        public ContactManager _contactManager = new ContactManager();
        internal Queue<Contact> _contactPool = new Queue<Contact>(256);

        public Body _bodyList;
        public Joint _jointList;

        public int _bodyCount;
        public int _jointCount;

        // This is used to compute the time step ratio to
        // support a variable time step.
        internal float _inv_dt0;

        Body[] stack = new Body[64];
    }
}
