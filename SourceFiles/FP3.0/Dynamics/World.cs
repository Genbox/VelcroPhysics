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
using System.Collections.Generic;
using System.Diagnostics;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

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
        internal Queue<Contact> ContactPool = new Queue<Contact>(256);

        /// <summary>
        /// Called whenever a Fixture is removed
        /// </summary>
        public FixtureRemovedDelegate FixtureRemoved;

        internal WorldFlags Flags;

        /// <summary>
        /// Called whenever a Joint is removed
        /// </summary>
        public JointRemovedDelegate JointRemoved;

        private float _inv_dt0;
        private Island _island = new Island();
        private Func<FixtureProxy, bool> _queryAABBCallback;
        private Func<int, bool> _queryAABBCallbackWrapper;
        private RayCastCallback _rayCastCallback;
        private RayCastCallbackInternal _rayCastCallbackWrapper;
        private Body[] _stack = new Body[64];
        private Contact[] _toiContacts = new Contact[Settings.MaxTOIContacts];
        private TOISolver _toiSolver = new TOISolver();
        private Stopwatch _watch = new Stopwatch();

        /// ruct a world object.
        /// @param gravity the world gravity vector.
        /// @param doSleep improve performance by not simulating inactive bodies.
        public World(Vector2 gravity)
        {
            ContactManager = new ContactManager();
            Gravity = gravity;

            Flags = WorldFlags.ClearForces;

            _queryAABBCallbackWrapper = QueryAABBCallbackWrapper;
            _rayCastCallbackWrapper = RayCastCallbackWrapper;

            new DefaultContactFilter(this);

            Controllers = new List<Controller>();
            BreakableBodyList = new List<BreakableBody>();
        }

        public List<Controller> Controllers { get; private set; }

        public List<BreakableBody> BreakableBodyList { get; private set; }

        public float UpdateTime { get; private set; }

        public float ContinuousPhysicsTime { get; private set; }

        public float NewContactsTime { get; private set; }

        public float ControllersUpdateTime { get; private set; }

        public float ContactsUpdateTime { get; private set; }

        public float SolveUpdateTime { get; private set; }

        /// Get the number of broad-phase proxies.
        public int ProxyCount
        {
            get { return ContactManager.BroadPhase.ProxyCount; }
        }

        /// Get the number of bodies.
        public int BodyCount { get; private set; }

        /// Get the number of joints.
        public int JointCount { get; private set; }

        /// Get the number of contacts (each may have 0 or more contact points).
        public int ContactCount
        {
            get { return ContactManager.ContactCount; }
        }

        /// Change the global gravity vector.
        public Vector2 Gravity { get; set; }

        /// Is the world locked (in the middle of a time step).
        public bool IsLocked
        {
            get { return (Flags & WorldFlags.Locked) == WorldFlags.Locked; }
            set
            {
                if (value)
                {
                    Flags |= WorldFlags.Locked;
                }
                else
                {
                    Flags &= ~WorldFlags.Locked;
                }
            }
        }

        /// Set flag to control automatic clearing of forces after each time step.
        private bool AutoClearForces
        {
            set
            {
                if (value)
                {
                    Flags |= WorldFlags.ClearForces;
                }
                else
                {
                    Flags &= ~WorldFlags.ClearForces;
                }
            }
            get { return (Flags & WorldFlags.ClearForces) == WorldFlags.ClearForces; }
        }

        /// Get the contact manager for testing.
        public ContactManager ContactManager { get; set; }

        /// Get the world body list. With the returned body, use Body.GetNext to get
        /// the next body in the world list. A null body indicates the end of the list.
        /// @return the head of the world body list.
        public Body BodyList { get; set; }

        /// Get the world joint list. With the returned joint, use Joint.GetNext to get
        /// the next joint in the world list. A null joint indicates the end of the list.
        /// @return the head of the world joint list.
        public Joint JointList { get; set; }

        /// Get the world contact list. With the returned contact, use Contact.GetNext to get
        /// the next contact in the world list. A null contact indicates the end of the list.
        /// @return the head of the world contact list.
        /// @warning contacts are 
        public Contact ContactList
        {
            get { return ContactManager.ContactList; }
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

            // Add to world doubly linked list.
            b.Prev = null;
            b.Next = BodyList;
            if (BodyList != null)
            {
                BodyList.Prev = b;
            }
            BodyList = b;
            ++BodyCount;

            return b;
        }

        /// Destroy a rigid body given a definition. No reference to the definition
        /// is retained. This function is locked during callbacks.
        /// @warning This automatically deletes all associated shapes and joints.
        /// @warning This function is locked during callbacks.
        public void DestroyBody(Body b)
        {
            Debug.Assert(BodyCount > 0);
            Debug.Assert(!IsLocked);
            if (IsLocked)
            {
                return;
            }

            // Delete the attached joints.
            JointEdge je = b.JointList;
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
            b.JointList = null;

            // Delete the attached contacts.
            ContactEdge ce = b.ContactList;
            while (ce != null)
            {
                ContactEdge ce0 = ce;
                ce = ce.Next;
                ContactManager.Destroy(ce0.Contact);
            }
            b.ContactList = null;

            // Delete the attached fixtures. This destroys broad-phase proxies.
            Fixture f = b.FixtureList;
            while (f != null)
            {
                Fixture f0 = f;
                f = f.Next;

                if (FixtureRemoved != null)
                {
                    FixtureRemoved(f0);
                }

                f0.DestroyProxies(ContactManager.BroadPhase);
                f0.Destroy();
            }
            b.FixtureList = null;
            b.FixtureCount = 0;

            // Remove world body list.
            if (b.Prev != null)
            {
                b.Prev.Next = b.Next;
            }

            if (b.Next != null)
            {
                b.Next.Prev = b.Prev;
            }

            if (b == BodyList)
            {
                BodyList = b.Next;
            }

            --BodyCount;
        }

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
            j._next = JointList;
            if (JointList != null)
            {
                JointList._prev = j;
            }
            JointList = j;
            ++JointCount;

            // Connect to the bodies' doubly linked lists.
            j._edgeA.Joint = j;
            j._edgeA.Other = j.BodyB;
            j._edgeA.Prev = null;
            j._edgeA.Next = j.BodyA.JointList;

            if (j.BodyA.JointList != null)
                j.BodyA.JointList.Prev = j._edgeA;

            j.BodyA.JointList = j._edgeA;

            // WIP David
            if (!j.IsFixedType())
            {
                j._edgeB.Joint = j;
                j._edgeB.Other = j.BodyA;
                j._edgeB.Prev = null;
                j._edgeB.Next = j.BodyB.JointList;

                if (j.BodyB.JointList != null)
                    j.BodyB.JointList.Prev = j._edgeB;

                j.BodyB.JointList = j._edgeB;
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

            if (j == JointList)
            {
                JointList = j._next;
            }

            // Disconnect from island graph.
            Body bodyA = j.BodyA;
            Body bodyB = j.BodyB;

            // Wake up connected bodies.
            bodyA.Awake = true;

            // WIP David
            if (!j.IsFixedType())
            {
                bodyB.Awake = true;
            }

            // Remove from body 1.
            if (j._edgeA.Prev != null)
            {
                j._edgeA.Prev.Next = j._edgeA.Next;
            }

            if (j._edgeA.Next != null)
            {
                j._edgeA.Next.Prev = j._edgeA.Prev;
            }

            if (j._edgeA == bodyA.JointList)
            {
                bodyA.JointList = j._edgeA.Next;
            }

            j._edgeA.Prev = null;
            j._edgeA.Next = null;

            // WIP David
            if (!j.IsFixedType())
            {
                // Remove from body 2
                if (j._edgeB.Prev != null)
                {
                    j._edgeB.Prev.Next = j._edgeB.Next;
                }

                if (j._edgeB.Next != null)
                {
                    j._edgeB.Next.Prev = j._edgeB.Prev;
                }

                if (j._edgeB == bodyB.JointList)
                {
                    bodyB.JointList = j._edgeB.Next;
                }

                j._edgeB.Prev = null;
                j._edgeB.Next = null;
            }

            Debug.Assert(JointCount > 0);
            --JointCount;

            // WIP David
            if (!j.IsFixedType())
            {
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
        }

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
            if ((Flags & WorldFlags.NewFixture) == WorldFlags.NewFixture)
            {
                ContactManager.FindNewContacts();
                Flags &= ~WorldFlags.NewFixture;
            }
            if (Settings.EnableDiagnostics)
                NewContactsTime = _watch.ElapsedTicks;

            Flags |= WorldFlags.Locked;

            TimeStep step;
            step.dt = dt;
            if (dt > 0.0f)
            {
                step.inv_dt = 1.0f/dt;
            }
            else
            {
                step.inv_dt = 0.0f;
            }

            step.dtRatio = _inv_dt0*dt;

            //Update controllers
            foreach (Controller controller in Controllers)
            {
                controller.Update(dt);
            }

            if (Settings.EnableDiagnostics)
                ControllersUpdateTime = _watch.ElapsedTicks - NewContactsTime;

            // Update contacts. This is where some contacts are destroyed.
            ContactManager.Collide();

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

            if ((Flags & WorldFlags.ClearForces) != 0)
            {
                ClearForces();
            }

            Flags &= ~WorldFlags.Locked;

            if (Settings.EnableDiagnostics)
            {
                _watch.Stop();
                UpdateTime = _watch.ElapsedTicks;
                _watch.Reset();
            }

            //TODO: introduce timing
            foreach (BreakableBody breakableBody in BreakableBodyList)
            {
                breakableBody.Update();
            }
        }

        /// Call this after you are done with time steps to clear the forces. You normally
        /// call this after each call to Step, unless you are performing sub-steps. By default,
        /// forces will be automatically cleared, so you don't need to call this function.
        /// @see SetAutoClearForces
        public void ClearForces()
        {
            for (Body body = BodyList; body != null; body = body.Next)
            {
                body.Force = Vector2.Zero;
                body.Torque = 0.0f;
            }
        }

        /// Query the world for all fixtures that potentially overlap the
        /// provided AABB.
        /// @param callback a user implemented callback class.
        /// @param aabb the query box.
        public void QueryAABB(Func<FixtureProxy, bool> callback, ref AABB aabb)
        {
            _queryAABBCallback = callback;
            ContactManager.BroadPhase.Query(_queryAABBCallbackWrapper, ref aabb);
            _queryAABBCallback = null;
        }

        private bool QueryAABBCallbackWrapper(int proxyId)
        {
            FixtureProxy proxy = ContactManager.BroadPhase.GetUserData<FixtureProxy>(proxyId);
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
            ContactManager.BroadPhase.RayCast(_rayCastCallbackWrapper, ref input);
            _rayCastCallback = null;
        }

        private float RayCastCallbackWrapper(ref RayCastInput input, int proxyId)
        {
            FixtureProxy proxy = ContactManager.BroadPhase.GetUserData<FixtureProxy>(proxyId);
            Fixture fixture = proxy.Fixture;
            int index = proxy.ChildIndex;
            RayCastOutput output;
            bool hit = fixture.RayCast(out output, ref input, index);

            if (hit)
            {
                float fraction = output.Fraction;
                Vector2 point = (1.0f - fraction)*input.Point1 + fraction*input.Point2;
                return _rayCastCallback(fixture, point, output.Normal, fraction);
            }

            return input.MaxFraction;
        }

        private void Solve(ref TimeStep step)
        {
            // Size the island for the worst case.
            _island.Reset(BodyCount,
                          ContactManager.ContactCount,
                          JointCount,
                          ContactManager);

            // Clear all the island flags.
            for (Body b = BodyList; b != null; b = b.Next)
            {
                b.Flags &= ~BodyFlags.Island;
            }
            for (Contact c = ContactManager.ContactList; c != null; c = c.Next)
            {
                c.Flags &= ~ContactFlags.Island;
            }
            for (Joint j = JointList; j != null; j = j._next)
            {
                j._islandFlag = false;
            }

            // Build and simulate all awake islands.
            int stackSize = BodyCount;
            if (stackSize > _stack.Length)
                _stack = new Body[Math.Max(_stack.Length*2, stackSize)];

            for (Body seed = BodyList; seed != null; seed = seed.Next)
            {
                if ((seed.Flags & (BodyFlags.Island)) != BodyFlags.None)
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
                _stack[stackCount++] = seed;
                seed.Flags |= BodyFlags.Island;

                // Perform a depth first search (DFS) on the raint graph.
                while (stackCount > 0)
                {
                    // Grab the next body off the stack and add it to the island.
                    Body b = _stack[--stackCount];
                    Debug.Assert(b.Active);
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
                    for (ContactEdge ce = b.ContactList; ce != null; ce = ce.Next)
                    {
                        Contact contact = ce.Contact;

                        // Has this contact already been added to an island?
                        if ((contact.Flags & ContactFlags.Island) != ContactFlags.None)
                        {
                            continue;
                        }

                        // Is this contact solid and touching?
                        if (!ce.Contact.Enabled || !ce.Contact.IsTouching())
                        {
                            continue;
                        }

                        // Skip sensors.
                        bool sensorA = contact.FixtureA.IsSensor;
                        bool sensorB = contact.FixtureB.IsSensor;
                        if (sensorA || sensorB)
                        {
                            continue;
                        }

                        _island.Add(contact);
                        contact.Flags |= ContactFlags.Island;

                        Body other = ce.Other;

                        // Was the other body already added to this island?
                        if ((other.Flags & BodyFlags.Island) != BodyFlags.None)
                        {
                            continue;
                        }

                        Debug.Assert(stackCount < stackSize);
                        _stack[stackCount++] = other;
                        other.Flags |= BodyFlags.Island;
                    }

                    // Search all joints connect to this body.
                    for (JointEdge je = b.JointList; je != null; je = je.Next)
                    {
                        if (je.Joint._islandFlag)
                        {
                            continue;
                        }

                        Body other = je.Other;

                        // WIP David
                        //Enter here when it's a non-fixed joint. Non-fixed joints have a other body.
                        if (other != null)
                        {
                            // Don't simulate joints connected to inactive bodies.
                            if (other.Active == false)
                            {
                                continue;
                            }

                            _island.Add(je.Joint);
                            je.Joint._islandFlag = true;

                            if ((other.Flags & BodyFlags.Island) != BodyFlags.None)
                            {
                                continue;
                            }

                            Debug.Assert(stackCount < stackSize);
                            _stack[stackCount++] = other;
                            other.Flags |= BodyFlags.Island;
                        }
                        else
                        {
                            _island.Add(je.Joint);
                            je.Joint._islandFlag = true;
                        }
                    }
                }

                _island.Solve(ref step, Gravity);

                // Post solve cleanup.
                for (int i = 0; i < _island.BodyCount; ++i)
                {
                    // Allow static bodies to participate in other islands.
                    Body b = _island.Bodies[i];
                    if (b.BodyType == BodyType.Static)
                    {
                        b.Flags &= ~BodyFlags.Island;
                    }
                }
            }

            // Synchronize fixtures, check for out of range bodies.
            for (Body b = BodyList; b != null; b = b.Next)
            {
                // If a body was not in an island then it did not move.
                if ((b.Flags & BodyFlags.Island) != BodyFlags.Island)
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
            ContactManager.FindNewContacts();
        }

        // Advance a dynamic body to its first time of contact
        // and adjust the position to ensure clearance.
        private void SolveTOI(Body body)
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
                for (ContactEdge ce = body.ContactList; ce != null; ce = ce.Next)
                {
                    if (ce.Contact == toiContact)
                    {
                        continue;
                    }

                    Body other = ce.Other;
                    BodyType type = other.BodyType;

                    // Only bullets perform TOI with dynamic bodies.
                    if (bullet)
                    {
                        // Bullets only perform TOI with bodies that have their TOI resolved.
                        if ((other.Flags & BodyFlags.Toi) == 0)
                        {
                            continue;
                        }

                        // No repeated hits on non-static bodies
                        if (type != BodyType.Static && (ce.Contact.Flags & ContactFlags.BulletHit) != 0)
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
                    if (contact.TOICount > 10)
                    {
                        continue;
                    }

                    Fixture fixtureA = contact.FixtureA;
                    Fixture fixtureB = contact.FixtureB;
                    int indexA = contact.IndexA;
                    int indexB = contact.IndexB;

                    // Cull sensors.
                    if (fixtureA.IsSensor || fixtureB.IsSensor)
                    {
                        continue;
                    }

                    Body bodyA = fixtureA.Body;
                    Body bodyB = fixtureB.Body;

                    // Compute the time of impact in interval [0, minTOI]
                    TOIInput input = new TOIInput();
                    input.ProxyA.Set(fixtureA.Shape, indexA);
                    input.ProxyB.Set(fixtureB.Shape, indexB);
                    input.SweepA = bodyA.Sweep;
                    input.SweepB = bodyB.Sweep;
                    input.TMax = toi;

                    TOIOutput output;
                    TimeOfImpact.CalculateTimeOfImpact(out output, ref input);

                    if (output.State == TOIOutputState.Touching && output.T < toi)
                    {
                        toiContact = contact;
                        toi = output.T;
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

            Sweep backup = body.Sweep;
            body.Advance(toi);
            toiContact.Update(ContactManager);
            if (toiContact.Enabled == false)
            {
                // Contact disabled. Backup and recurse.
                body.Sweep = backup;
                SolveTOI(body);
            }

            ++toiContact.TOICount;

            // Update all the valid contacts on this body and build a contact island.
            count = 0;
            for (ContactEdge ce = body.ContactList; (ce != null) && (count < Settings.MaxTOIContacts); ce = ce.Next)
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

                Fixture fixtureA = contact.FixtureA;
                Fixture fixtureB = contact.FixtureB;

                // Cull sensors.
                if (fixtureA.IsSensor || fixtureB.IsSensor)
                {
                    continue;
                }

                // The contact likely has some new contact points. The listener
                // gives the user a chance to disable the contact.
                if (contact != toiContact)
                {
                    contact.Update(ContactManager);
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

            const float k_toiBaumgarte = 0.75f;
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
                toiContact.Flags |= ContactFlags.BulletHit;
            }
        }

        // Sequentially solve TOIs for each body. We bring each
        // body to the time of contact and perform some position correction.
        // Time is not conserved.
        private void SolveTOI()
        {
            // Prepare all contacts.
            for (Contact c = ContactManager.ContactList; c != null; c = c.Next)
            {
                // Enable the contact
                c.Flags |= ContactFlags.Enabled;

                // Set the number of TOI events for this contact to zero.
                c.TOICount = 0;
            }

            // Initialize the TOI flag.
            for (Body body = BodyList; body != null; body = body.Next)
            {
                // Kinematic, and static bodies will not be affected by the TOI event.
                // If a body was not in an island then it did not move.
                if ((body.Flags & BodyFlags.Island) == 0 || body.BodyType == BodyType.Kinematic ||
                    body.BodyType == BodyType.Static)
                {
                    body.Flags |= BodyFlags.Toi;
                }
                else
                {
                    body.Flags &= ~BodyFlags.Toi;
                }
            }

            // Collide non-bullets.
            for (Body body = BodyList; body != null; body = body.Next)
            {
                if ((body.Flags & BodyFlags.Toi) != BodyFlags.None)
                {
                    continue;
                }

                if (body.IsBullet)
                {
                    continue;
                }

                SolveTOI(body);

                body.Flags |= BodyFlags.Toi;
            }

            // Collide bullets.
            for (Body body = BodyList; body != null; body = body.Next)
            {
                if ((body.Flags & BodyFlags.Toi) != BodyFlags.None)
                {
                    continue;
                }

                if (body.IsBullet == false)
                {
                    continue;
                }

                SolveTOI(body);

                body.Flags |= BodyFlags.Toi;
            }
        }

        public void AddController(Controller controller)
        {
            Debug.Assert(!Controllers.Contains(controller));

            controller.World = this;
            Controllers.Add(controller);
        }

        public void RemoveController(Controller controller)
        {
            if (Controllers.Contains(controller))
                Controllers.Remove(controller);
        }

        public void AddBreakableBody(BreakableBody breakableBody)
        {
            BreakableBodyList.Add(breakableBody);
        }
    }
}