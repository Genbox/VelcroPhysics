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
using FarseerPhysics.Controllers;
using Microsoft.Xna.Framework;

namespace FarseerPhysics
{
    [Flags]
    public enum WorldFlags
    {
        NewFixture = (1 << 0),
        Locked = (1 << 1),
        ClearForces = (1 << 2),
    }

    /// <summary>
    /// The world class manages all physics entities, dynamic simulation,
    /// and asynchronous queries. The world also contains efficient memory
    /// management facilities.
    /// </summary>
    public class World
    {
        /// <summary>
        /// Called whenever a Fixture is removed
        /// </summary>
        public FixtureRemovedDelegate FixtureRemoved;

        internal WorldFlags Flags;

        /// <summary>
        /// Called whenever a Joint is removed
        /// </summary>
        public JointRemovedDelegate JointRemoved;

        private float _invDt0;
        private Island _island = new Island();
        private Func<Fixture, bool> _queryAABBCallback;
        private Func<int, bool> _queryAABBCallbackWrapper;
        private TOISolver _toiSolver = new TOISolver();
        private Contact[] _toiContacts = new Contact[Settings.MaxTOIContactsPerIsland];

        private WorldRayCastCallback _rayCastCallback;
        private RayCastCallback _rayCastCallbackWrapper;

        private Stopwatch _watch;

        private List<Controller> _controllers = new List<Controller>();

        /// <summary>
        /// Construct a world object.
        /// </summary>
        /// <param name="gravity">the world gravity vector.</param>
        /// <param name="allowSleep">improve performance by not simulating inactive bodies.</param>
        public World(Vector2 gravity, bool allowSleep)
        {
            WarmStarting = true;
            ContinuousPhysics = true;

            AllowSleep = allowSleep;
            Gravity = gravity;

            Flags = WorldFlags.ClearForces;

            _queryAABBCallbackWrapper = QueryAABBCallbackWrapper;
            _rayCastCallbackWrapper = RayCastCallbackWrapper;

            ContactManager = new ContactManager();

            //Create the default contact filter
            new DefaultContactFilter(this);

            // init the watch
            _watch = new Stopwatch();
        }

        /// <summary>
        /// Enable/disable warm starting. For testing.
        /// </summary>
        /// <value><c>true</c> if [warm starting]; otherwise, <c>false</c>.</value>
        public bool WarmStarting { get; set; }

        /// <summary>
        /// Enable/disable continuous physics. For testing.
        /// </summary>
        /// <value><c>true</c> if [continuous physics]; otherwise, <c>false</c>.</value>
        public bool ContinuousPhysics { get; set; }

        /// <summary>
        /// Get the number of broad-phase proxies.
        /// </summary>
        /// <value>The proxy count.</value>
        public int ProxyCount
        {
            get { return ContactManager._broadPhase.ProxyCount; }
        }

        /// <summary>
        /// Get the number of bodies.
        /// </summary>
        /// <value>The body count.</value>
        public int BodyCount { get; private set; }

        /// <summary>
        /// Get the number of joints.
        /// </summary>
        /// <value>The joint count.</value>
        public int JointCount { get; private set; }

        /// <summary>
        /// Get the number of contacts (each may have 0 or more contact points).
        /// </summary>
        /// <value>The contact count.</value>
        public int ContactCount
        {
            get { return ContactManager._contactCount; }
        }

        /// <summary>
        /// Change the global gravity vector.
        /// </summary>
        /// <value>The gravity.</value>
        public Vector2 Gravity { get; set; }

        /// <summary>
        /// Is the world locked (in the middle of a time step).
        /// </summary>
        /// <value><c>true</c> if this instance is locked; otherwise, <c>false</c>.</value>
        public bool Locked
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

        /// <summary>
        /// Get the world body list. With the returned body, use Body.GetNext to get
        /// the next body in the world list. A null body indicates the end of the list.
        /// </summary>
        /// <returns>the head of the world body list.</returns>
        public Body BodyList { get; private set; }

        public ContactManager ContactManager { get; private set; }

        /// <summary>
        /// Get the world joint list. With the returned joint, use Joint.GetNext to get
        /// the next joint in the world list. A null joint indicates the end of the list.
        /// </summary>
        /// <returns>the head of the world joint list.</returns>
        public Joint JointList { get; private set; }

        public bool AllowSleep { get; set; }

        public TimeSpan UpdateTime { get; private set; }

        /// <summary>
        /// Get the world contact list. With the returned contact, use Contact.GetNext to get
        /// the next contact in the world list. A null contact indicates the end of the list.
        /// </summary>
        /// <returns>the head of the world contact list.</returns>
        public Contact ContactList
        {
            get { return ContactManager._contactList; }
        }

        public void AddController(Controller controller)
        {
            Debug.Assert(!_controllers.Contains(controller));

            controller.World = this;
            _controllers.Add(controller);
        }

        public Body CreateBody(Body body)
        {
            Debug.Assert(!Locked);
            if (Locked)
            {
                return null;
            }

            Body clone = body.Clone(this);

            // Add to world doubly linked list.
            clone._prev = null;
            clone._next = BodyList;
            if (BodyList != null)
            {
                BodyList._prev = clone;
            }
            BodyList = clone;
            ++BodyCount;

            return clone;
        }

        public Body CreateBody()
        {
            Debug.Assert(!Locked);
            if (Locked)
            {
                return null;
            }

            Body b = new Body(this);

            // Add to world doubly linked list.
            b._prev = null;
            b._next = BodyList;
            if (BodyList != null)
            {
                BodyList._prev = b;
            }
            BodyList = b;
            ++BodyCount;

            return b;
        }

        /// <summary>
        /// Destroy a rigid body given a definition. No reference to the definition
        /// is retained. This function is locked during callbacks.
        /// @warning This automatically deletes all associated shapes and joints.
        /// @warning This function is locked during callbacks.
        /// </summary>
        /// <param name="b">The b.</param>
        public void DestroyBody(Body b)
        {
            Debug.Assert(BodyCount > 0);
            Debug.Assert(!Locked);
            if (Locked)
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
                ContactManager.Destroy(ce0.Contact);
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

                f0.DestroyProxy(ContactManager._broadPhase);
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

            if (b == BodyList)
            {
                BodyList = b._next;
            }

            --BodyCount;
        }

        /// <summary>
        /// Create a joint to rain bodies together. No reference to the definition
        /// is retained. This may cause the connected bodies to cease colliding.
        /// @warning This function is locked during callbacks.
        /// </summary>
        /// <param name="joint">The joint.</param>
        /// <returns></returns>
        public void CreateJoint(Joint joint)
        {
            Debug.Assert(!Locked);
            if (Locked)
            {
                return;
            }

            // Connect to the world list.
            joint.Prev = null;
            joint.Next = JointList;
            if (JointList != null)
            {
                JointList.Prev = joint;
            }
            JointList = joint;
            ++JointCount;

            // Connect to the bodies' doubly linked lists.
            joint._edgeA.Joint = joint;
            joint._edgeA.Other = joint.BodyB;

            joint._edgeA.Prev = null;
            joint._edgeA.Next = joint.BodyA._jointList;

            if (joint.BodyA._jointList != null)
                joint.BodyA._jointList.Prev = joint._edgeA;

            joint.BodyA._jointList = joint._edgeA;

            // WIP David
            if (!joint.IsFixedType())
            {
                joint._edgeB.Joint = joint;
                joint._edgeB.Other = joint.BodyA;
                joint._edgeB.Prev = null;
                joint._edgeB.Next = joint.BodyB._jointList;

                if (joint.BodyB._jointList != null)
                    joint.BodyB._jointList.Prev = joint._edgeB;

                joint.BodyB._jointList = joint._edgeB;
            }

            // WIP David
            if (!joint.IsFixedType())
            {
                Body bodyA = joint.BodyA;
                Body bodyB = joint.BodyB;

                // If the joint prevents collisions, then flag any contacts for filtering.
                if (joint.CollideConnected == false)
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

        /// <summary>
        /// Destroy a joint. This may cause the connected bodies to begin colliding.
        /// @warning This function is locked during callbacks.
        /// </summary>
        /// <param name="j">The j.</param>
        public void DestroyJoint(Joint j)
        {
            Debug.Assert(!Locked);
            if (Locked)
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

            if (j == JointList)
            {
                JointList = j.Next;
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

            if (j._edgeA == bodyA._jointList)
            {
                bodyA._jointList = j._edgeA.Next;
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

                if (j._edgeB == bodyB._jointList)
                {
                    bodyB._jointList = j._edgeB.Next;
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

        /// <summary>
        /// Take a time step. This performs collision detection, integration,
        /// and constraint solution.
        /// </summary>
        /// <param name="dt">the amount of time to simulate, this should not vary.</param>
        /// <param name="velocityIterations">for the velocity constraint solver.</param>
        /// <param name="positionIterations">for the position constraint solver.</param>
        public void Step(float dt, int velocityIterations, int positionIterations)
        {
            _watch.Start();

            // If new fixtures were added, we need to find the new contacts.
            if ((Flags & WorldFlags.NewFixture) == WorldFlags.NewFixture)
            {
                ContactManager.FindNewContacts();
                Flags &= ~WorldFlags.NewFixture;
            }

            //Lock the world
            Flags |= WorldFlags.Locked;

            TimeStep step;
            step.DeltaTime = dt;
            step.VelocityIterations = velocityIterations;
            step.PositionIterations = positionIterations;
            if (dt > 0.0f)
            {
                step.Inv_DeltaTime = 1.0f / dt;
            }
            else
            {
                step.Inv_DeltaTime = 0.0f;
            }

            step.DtRatio = _invDt0 * dt;

            step.WarmStarting = WarmStarting;

            //Update controllers
            foreach (Controller controller in _controllers)
            {
                controller.Update(dt);
            }

            // Update contacts. This is where some contacts are destroyed.
            ContactManager.Collide();

            // Integrate velocities, solve velocity constraints, and integrate positions.
            if (step.DeltaTime > 0.0f)
            {
                Solve(ref step);
            }

            // Handle TOI events.
            if (ContinuousPhysics && step.DeltaTime > 0.0f)
            {
                SolveTOI();
            }

            if (step.DeltaTime > 0.0f)
            {
                _invDt0 = step.Inv_DeltaTime;
            }

            if ((Flags & WorldFlags.ClearForces) != 0)
            {
                ClearForces();
            }

            Flags &= ~WorldFlags.Locked;

            _watch.Stop();
            UpdateTime = _watch.Elapsed;
            _watch.Reset();
        }

        /// Call this after you are done with time steps to clear the forces. You normally
        /// call this after each call to Step, unless you are performing sub-steps. By default,
        /// forces will be automatically cleared, so you don't need to call this function.
        /// @see SetAutoClearForces
        public void ClearForces()
        {
            for (Body body = BodyList; body != null; body = body.NextBody)
            {
                body._force = Vector2.Zero;
                body._torque = 0.0f;
            }
        }

        /// Set flag to control automatic clearing of forces after each time step.
        public void SetAutoClearForces(bool flag)
        {
            if (flag)
            {
                Flags |= WorldFlags.ClearForces;
            }
            else
            {
                Flags &= ~WorldFlags.ClearForces;
            }
        }

        /// Get the flag that controls automatic clearing of forces after each time step.
        public bool GetAutoClearForces()
        {
            return (Flags & WorldFlags.ClearForces) == WorldFlags.ClearForces;
        }

        /// <summary>
        /// Query the world for all fixtures that potentially overlap the
        /// provided AABB.
        /// </summary>
        /// <param name="callback">a user implemented callback class.</param>
        /// <param name="aabb">the query box.</param>
        public void QueryAABB(Func<Fixture, bool> callback, ref AABB aabb)
        {
            _queryAABBCallback = callback;
            ContactManager._broadPhase.Query(_queryAABBCallbackWrapper, ref aabb);
            _queryAABBCallback = null;
        }

        private bool QueryAABBCallbackWrapper(int proxyId)
        {
            Fixture fixture = ContactManager._broadPhase.GetUserData<Fixture>(proxyId);
            return _queryAABBCallback(fixture);
        }

        /// <summary>
        /// Ray-cast the world for all fixtures in the path of the ray. Your callback
        /// controls whether you get the closest point, any point, or n-points.
        /// The ray-cast ignores shapes that contain the starting point.
        /// </summary>
        /// <param name="callback">a user implemented callback class.</param>
        /// <param name="point1">the ray starting point</param>
        /// <param name="point2">the ray ending point</param>
        public void RayCast(WorldRayCastCallback callback, Vector2 point1, Vector2 point2)
        {
            RayCastInput input = new RayCastInput();
            input.MaxFraction = 1.0f;
            input.Point1 = point1;
            input.Point2 = point2;

            _rayCastCallback = callback;
            ContactManager._broadPhase.RayCast(_rayCastCallbackWrapper, ref input);
            _rayCastCallback = null;
        }

        private float RayCastCallbackWrapper(ref RayCastInput input, int proxyId)
        {
            Fixture fixture = ContactManager._broadPhase.GetUserData<Fixture>(proxyId);
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

        private Body[] _stack;

        private void Solve(ref TimeStep step)
        {
            // Size the island for the worst case.
            _island.Reset(BodyCount,
                          ContactManager._contactCount,
                          JointCount,
                          ContactManager);

            // Clear all the island flags.
            for (Body b = BodyList; b != null; b = b._next)
            {
                b._flags &= ~BodyFlags.Island;
            }
            for (Contact c = ContactManager._contactList; c != null; c = c.NextContact)
            {
                c.Flags &= ~ContactFlags.Island;
            }
            for (Joint j = JointList; j != null; j = j.Next)
            {
                j._islandFlag = false;
            }

            // Build and simulate all awake islands.
            int stackSize = BodyCount;

            if (_stack == null || _stack.Length < stackSize)
                _stack = new Body[BodyCount];

            for (Body seed = BodyList; seed != null; seed = seed._next)
            {
                if ((seed._flags & (BodyFlags.Island)) != BodyFlags.None)
                {
                    continue;
                }

                if (seed.Awake == false || seed.Enabled == false)
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
                seed._flags |= BodyFlags.Island;

                // Perform a depth first search (DFS) on the constraint graph.
                while (stackCount > 0)
                {
                    // Grab the next body off the stack and add it to the island.
                    Body b = _stack[--stackCount];
                    Debug.Assert(b.Enabled);
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
                        if ((contact.Flags & ContactFlags.Island) != ContactFlags.None)
                        {
                            continue;
                        }

                        // Is this contact solid and touching?
                        if (!contact.Enabled || !contact.IsTouching())
                        {
                            continue;
                        }

                        // Skip sensors.
                        bool sensorA = contact.FixtureA.Sensor;
                        bool sensorB = contact.FixtureB.Sensor;
                        if (sensorA || sensorB)
                        {
                            continue;
                        }

                        _island.Add(contact);
                        contact.Flags |= ContactFlags.Island;

                        Body other = ce.Other;

                        // Was the other body already added to this island?
                        if ((other._flags & BodyFlags.Island) != BodyFlags.None)
                        {
                            continue;
                        }

                        Debug.Assert(stackCount < stackSize);
                        _stack[stackCount++] = other;
                        other._flags |= BodyFlags.Island;
                    }

                    // Search all joints connect to this body.
                    for (JointEdge je = b._jointList; je != null; je = je.Next)
                    {
                        if (je.Joint._islandFlag)
                        {
                            continue;
                        }

                        Body other = je.Other;

                        // Enter here when it's a non-fixed joint. Non-fixed joints have a other body.
                        if (other != null)
                        {
                            // Don't simulate joints connected to inactive bodies.
                            if (other.Enabled == false)
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
                            _stack[stackCount++] = other;
                            other._flags |= BodyFlags.Island;
                        }
                        else
                        {
                            _island.Add(je.Joint);
                            je.Joint._islandFlag = true;
                        }
                    }
                }

                _island.Solve(ref step, Gravity, AllowSleep);

                // Post solve cleanup.
                for (int i = 0; i < _island.BodyCount; ++i)
                {
                    // Allow static bodies to participate in other islands.
                    Body b = _island.Bodies[i];
                    if (b.BodyType == BodyType.Static)
                    {
                        b._flags &= ~BodyFlags.Island;
                    }
                }
            }

            // Synchronize fixtures, check for out of range bodies.
            for (Body b = BodyList; b != null; b = b.NextBody)
            {
                if (!b.Awake || !b.Enabled)
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

        private Body[] _queue;

        // Advance a dynamic body to its first time of contact
        // and adjust the position to ensure clearance.
        private void SolveTOI(Body body)
        {
            // Find the minimum contact.
            Contact toiContact = null;
            float toi = 1.0f;
            bool found;
            int count;
            int iter = 0;

            bool bullet = body.Bullet;

            // Iterate until all contacts agree on the minimum TOI. We have
            // to iterate because the TOI algorithm may skip some intermediate
            // collisions when objects rotate through each other.
            do
            {
                count = 0;
                found = false;
                for (ContactEdge ce = body._contactList; ce != null; ce = ce.Next)
                {
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
                    if (contact.ToiCount > 10)
                    {
                        continue;
                    }

                    Fixture fixtureA = contact.FixtureA;
                    Fixture fixtureB = contact.FixtureB;

                    // Cull sensors.
                    if (fixtureA.Sensor || fixtureB.Sensor)
                    {
                        continue;
                    }

                    Body bodyA = fixtureA._body;
                    Body bodyB = fixtureB._body;

                    // Compute the time of impact in interval [0, minTOI]
                    TOIInput input = new TOIInput();
                    input.ProxyA.Set(fixtureA.Shape);
                    input.ProxyB.Set(fixtureB.Shape);
                    input.SweepA = bodyA._sweep;
                    input.SweepB = bodyB._sweep;
                    input.TMax = toi;

                    TOIOutput output;
                    TimeOfImpact.CalculateTimeOfImpact(out output, ref input);

                    if (output.State == TOIOutputState.Touching && output.t < toi)
                    {
                        toiContact = contact;
                        toi = output.t;
                        found = true;
                    }

                    ++count;
                }

                ++iter;
            } while (found && count > 1 && iter < 50);

            if (toiContact == null)
            {
                return;
            }

            // Advance the body to its safe time.
            Sweep backup = body._sweep;
            body.Advance(toi);

            ++toiContact.ToiCount;

            // Update all the valid contacts on this body and build a contact island.
            count = 0;
            for (ContactEdge ce = body._contactList; (ce != null) && (count < Settings.MaxTOIContactsPerIsland); ce = ce.Next)
            {
                Body other = ce.Other;
                BodyType type = other.BodyType;

                // Only perform correction with static bodies, so the
                // body won't get pushed out of the world.
                if (type != BodyType.Static)
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
                if (fixtureA.Sensor || fixtureB.Sensor)
                {
                    continue;
                }

                // The contact likely has some new contact points. The listener
                // gives the user a chance to disable the contact;
                contact.Update(ContactManager);

                // Did the user disable the contact?
                if (contact.Enabled == false)
                {
                    if (contact == toiContact)
                    {
                        // Restore the body's sweep.
                        body._sweep = backup;
                        body.SynchronizeTransform();

                        // Recurse because the TOI has been invalidated.
                        SolveTOI(body);
                        return;
                    }

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
            bool solved = false;
            for (int i = 0; i < 20; ++i)
            {
                bool contactsOkay = _toiSolver.Solve(k_toiBaumgarte);
                if (contactsOkay)
                {
                    solved = true;
                    break;
                }
            }
        }

        // Sequentially solve TOIs for each body. We bring each
        // body to the time of contact and perform some position correction.
        // Time is not conserved.
        private void SolveTOI()
        {
            // Prepare all contacts.
            for (Contact c = ContactManager._contactList; c != null; c = c.NextContact)
            {
                // Enable the contact
                c.Flags |= ContactFlags.Enabled;

                // Set the number of TOI events for this contact to zero.
                c.ToiCount = 0;
            }

            // Initialize the TOI flag.
            for (Body body = BodyList; body != null; body = body._next)
            {
                // Sleeping, kinematic, and static bodies will not be affected by the TOI event.
                if (body.Awake == false || body.BodyType == BodyType.Kinematic || body.BodyType == BodyType.Static)
                {
                    body._flags |= BodyFlags.Toi;
                }
                else
                {
                    body._flags &= ~BodyFlags.Toi;
                }
            }

            // Collide non-bullets.
            for (Body body = BodyList; body != null; body = body._next)
            {
                if (body.BodyType != BodyType.Dynamic || body.Awake == false)
                {
                    continue;
                }

                if (body.Bullet == true)
                {
                    continue;
                }

                SolveTOI(body);

                body._flags |= BodyFlags.Toi;
            }

            // Collide bullets.
            for (Body body = BodyList; body != null; body = body._next)
            {
                if (body.BodyType != BodyType.Dynamic || body.Awake == false)
                {
                    continue;
                }

                if (body.Bullet == false)
                {
                    continue;
                }

                SolveTOI(body);

                body._flags |= BodyFlags.Toi;
            }
        }
    }
}