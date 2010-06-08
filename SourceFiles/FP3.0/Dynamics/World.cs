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
        AutoClearForces = (1 << 2),
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

        private RayCastCallback _rayCastCallback;
        private RayCastCallbackInternal _rayCastCallbackWrapper;
        private Body[] _stack = new Body[64];
        private Contact[] _toiContacts = new Contact[Settings.MaxTOIContacts];
        private TOISolver _toiSolver = new TOISolver();

        private Stopwatch _watch;

        /// <summary>
        /// Construct a world object.
        /// Warmstarting and continuous collision detection is on by default.
        /// </summary>
        /// <param name="gravity">the world gravity vector.</param>
        public World(Vector2 gravity)
        {
            Gravity = gravity;

            Flags = WorldFlags.AutoClearForces;

            _queryAABBCallbackWrapper = QueryAABBCallbackWrapper;
            _rayCastCallbackWrapper = RayCastCallbackWrapper;

            ContactManager = new ContactManager();

            //Create the default contact filter
            new DefaultContactFilter(this);

            // init the watch
            _watch = new Stopwatch();

            Controllers = new List<Controller>();
            BodyList = new List<Body>(32);
            BreakableBodyList = new List<BreakableBody>(8);
            JointList = new List<Joint>(8);
        }

        /// <summary>
        /// Get the number of broad-phase proxies.
        /// </summary>
        /// <value>The proxy count.</value>
        public int ProxyCount
        {
            get { return ContactManager.BroadPhase.ProxyCount; }
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
                    Flags |= WorldFlags.Locked;
                else
                    Flags &= ~WorldFlags.Locked;
            }
        }

        /// Get the flag that controls automatic clearing of forces after each time step.
        public bool AutoClearForces
        {
            get { return (Flags & WorldFlags.AutoClearForces) == WorldFlags.AutoClearForces; }
            set
            {
                if (value)
                {
                    Flags |= WorldFlags.AutoClearForces;
                }
                else
                {
                    Flags &= ~WorldFlags.AutoClearForces;
                }
            }
        }

        /// <summary>
        /// Get the world body list. With the returned body, use Body.GetNext to get
        /// the next body in the world list. A null body indicates the end of the list.
        /// </summary>
        /// <returns>the head of the world body list.</returns>
        public List<Body> BodyList { get; private set; }

        public List<BreakableBody> BreakableBodyList { get; private set; }

        public ContactManager ContactManager { get; private set; }

        /// <summary>
        /// Get the world joint list. With the returned joint, use Joint.GetNext to get
        /// the next joint in the world list. A null joint indicates the end of the list.
        /// </summary>
        /// <returns>the head of the world joint list.</returns>
        public List<Joint> JointList { get; private set; }

        /// <summary>
        /// Get the world contact list. With the returned contact, use Contact.GetNext to get
        /// the next contact in the world list. A null contact indicates the end of the list.
        /// </summary>
        /// <returns>the head of the world contact list.</returns>
        public List<Contact> ContactList
        {
            get { return ContactManager.ContactList; }
        }

        public List<Controller> Controllers { get; private set; }

        public float UpdateTime { get; private set; }

        public float ContinuousPhysicsTime { get; private set; }

        public float NewContactsTime { get; private set; }

        public float ControllersUpdateTime { get; private set; }

        public float ContactsUpdateTime { get; private set; }

        public float SolveUpdateTime { get; private set; }

        public void Add(Controller controller)
        {
            Debug.Assert(!Controllers.Contains(controller));

            controller.World = this;
            Controllers.Add(controller);
        }

        public void Remove(Controller controller)
        {
            if (Controllers.Contains(controller))
                Controllers.Remove(controller);
        }

        public void Add(Body body)
        {
            Debug.Assert(!Locked);
            if (Locked)
            {
                return;
            }

            BodyList.Add(body);
        }

        public void Add(BreakableBody body)
        {
            Debug.Assert(!Locked);
            if (Locked)
            {
                return;
            }

            Add(body.MainBody);

            BreakableBodyList.Add(body);
        }

        /// <summary>
        /// Destroy a rigid body given a definition. No reference to the definition
        /// is retained. This function is locked during callbacks.
        /// @warning This automatically deletes all associated shapes and joints.
        /// @warning This function is locked during callbacks.
        /// </summary>
        /// <param name="body">The body.</param>
        public void Remove(Body body)
        {
            Debug.Assert(BodyList.Count > 0);
            Debug.Assert(!Locked);
            if (Locked)
            {
                return;
            }

            // Delete the attached joints.
            JointEdge je = body._jointList;
            while (je != null)
            {
                JointEdge je0 = je;
                je = je.Next;

                if (JointRemoved != null)
                {
                    JointRemoved(je0.Joint);
                }

                Remove(je0.Joint);
            }
            body._jointList = null;

            // Delete the attached contacts.
            ContactEdge ce = body._contactList;
            while (ce != null)
            {
                ContactEdge ce0 = ce;
                ce = ce.Next;
                ContactManager.Destroy(ce0.Contact);
            }
            body._contactList = null;

            // Delete the attached fixtures. This destroys broad-phase proxies.

            for (int i = 0; i < body._fixtureList.Count; i++)
            {
                Fixture f = body._fixtureList[i];
                if (FixtureRemoved != null)
                {
                    FixtureRemoved(f);
                }

                f.DestroyProxy(ContactManager.BroadPhase);
                f.Destroy();
            }

            body._fixtureList = null;

            BodyList.Remove(body);
        }

        public void Remove(BreakableBody body)
        {
            Debug.Assert(BreakableBodyList.Count > 0);
            Debug.Assert(!Locked);
            if (Locked)
            {
                return;
            }

            //Remove all the parts of the breakable body.
            for (int i = 0; i < body.Parts.Count; i++)
            {
                Remove(body.Parts[i].Body);
            }

            BreakableBodyList.Remove(body);
        }

        /// <summary>
        /// Create a joint to rain bodies together. No reference to the definition
        /// is retained. This may cause the connected bodies to cease colliding.
        /// @warning This function is locked during callbacks.
        /// </summary>
        /// <param name="joint">The joint.</param>
        /// <returns></returns>
        public void Add(Joint joint)
        {
            Debug.Assert(!Locked);
            if (Locked)
            {
                return;
            }

            JointList.Add(joint);

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
        /// <param name="joint">The joint.</param>
        public void Remove(Joint joint)
        {
            Debug.Assert(!Locked);
            if (Locked)
            {
                return;
            }

            Debug.Assert(JointList.Count > 0);

            bool collideConnected = joint.CollideConnected;

            // Remove from the doubly linked list.
            JointList.Remove(joint);

            // Disconnect from island graph.
            Body bodyA = joint.BodyA;
            Body bodyB = joint.BodyB;

            // Wake up connected bodies.
            bodyA.Awake = true;

            // WIP David
            if (!joint.IsFixedType())
            {
                bodyB.Awake = true;
            }

            // Remove from body 1.
            if (joint._edgeA.Prev != null)
            {
                joint._edgeA.Prev.Next = joint._edgeA.Next;
            }

            if (joint._edgeA.Next != null)
            {
                joint._edgeA.Next.Prev = joint._edgeA.Prev;
            }

            if (joint._edgeA == bodyA._jointList)
            {
                bodyA._jointList = joint._edgeA.Next;
            }

            joint._edgeA.Prev = null;
            joint._edgeA.Next = null;

            // WIP David
            if (!joint.IsFixedType())
            {
                // Remove from body 2
                if (joint._edgeB.Prev != null)
                {
                    joint._edgeB.Prev.Next = joint._edgeB.Next;
                }

                if (joint._edgeB.Next != null)
                {
                    joint._edgeB.Next.Prev = joint._edgeB.Prev;
                }

                if (joint._edgeB == bodyB._jointList)
                {
                    bodyB._jointList = joint._edgeB.Next;
                }

                joint._edgeB.Prev = null;
                joint._edgeB.Next = null;
            }

            // WIP David
            if (!joint.IsFixedType())
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
        public void Step(float dt)
        {
            TimeStep step;
            step.DeltaTime = dt;

            if (dt > 0.0f)
                step.Inv_DeltaTime = 1.0f / dt;
            else
                step.Inv_DeltaTime = 0.0f;

            step.DtRatio = _invDt0 * dt;

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

            //Lock the world
            Flags |= WorldFlags.Locked;

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

            // Integrate velocities, solve velocity constraints, and integrate positions.
            if (step.DeltaTime > 0.0f)
            {
                Solve(ref step);
            }

            if (Settings.EnableDiagnostics)
                SolveUpdateTime = _watch.ElapsedTicks - (NewContactsTime + ControllersUpdateTime + ContactsUpdateTime);

            // Handle TOI events.
            if (Settings.EnableContinuousPhysics && step.DeltaTime > 0.0f)
            {
                SolveTOI();
            }

            if (Settings.EnableDiagnostics)
                ContinuousPhysicsTime = _watch.ElapsedTicks -
                                        (NewContactsTime + ControllersUpdateTime + ContactsUpdateTime + SolveUpdateTime);

            if (step.DeltaTime > 0.0f)
            {
                _invDt0 = step.Inv_DeltaTime;
            }

            if ((Flags & WorldFlags.AutoClearForces) != 0)
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
            for (int i = 0; i < BodyList.Count; i++)
            {
                BodyList[i]._force = Vector2.Zero;
                BodyList[i]._torque = 0.0f;
            }
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
            ContactManager.BroadPhase.Query(_queryAABBCallbackWrapper, ref aabb);
            _queryAABBCallback = null;
        }

        private bool QueryAABBCallbackWrapper(int proxyId)
        {
            Fixture fixture = ContactManager.BroadPhase.GetUserData<Fixture>(proxyId);
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
            Fixture fixture = ContactManager.BroadPhase.GetUserData<Fixture>(proxyId);
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

        private void Solve(ref TimeStep step)
        {
            // Size the island for the worst case.
            _island.Reset(BodyList.Count,
                          ContactManager.ContactList.Count,
                          JointList.Count,
                          ContactManager);

            // Clear all the island flags.
            for (int i = 0; i < BodyList.Count; i++)
            {
                BodyList[i]._flags &= ~BodyFlags.Island;
            }
            for (int i = 0; i < ContactList.Count; i++)
            {
                ContactList[i].Flags &= ~ContactFlags.Island;
            }
            for (int i = 0; i < JointList.Count; i++)
            {
                JointList[i]._islandFlag = false;
            }

            // Build and simulate all awake islands.
            int stackSize = BodyList.Count;

            if (_stack.Length < stackSize)
                _stack = new Body[BodyList.Count];

            for (int j = 0; j < BodyList.Count; j++)
            {
                Body seed = BodyList[j];
                if ((seed._flags & (BodyFlags.Island)) == BodyFlags.Island)
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
                        if (!contact.IsEnabled || !contact.IsTouching())
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
                        if ((other._flags & BodyFlags.Island) == BodyFlags.Island)
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

                            if ((other._flags & BodyFlags.Island) == BodyFlags.Island)
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

                _island.Solve(ref step, Gravity, Settings.EnableSleeping);

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
            for (int i = 0; i < BodyList.Count; i++)
            {
                Body b = BodyList[i];

                // If a body was not in an island then it did not move.
                if ((b._flags & BodyFlags.Island) == 0)
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
                for (ContactEdge ce = body._contactList; ce != null; ce = ce.Next)
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
                        if ((other._flags & BodyFlags.Toi) == 0)
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
                    if (contact.IsEnabled == false)
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
                    if (fixtureA.IsSensor || fixtureB.IsSensor)
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
            toiContact.Update(ContactManager);
            if (toiContact.IsEnabled == false)
            {
                // Contact disabled. Backup and recurse.
                body._sweep = backup;
                SolveTOI(body);
            }

            ++toiContact.ToiCount;

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
                if (contact.IsEnabled == false)
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
                // gives the user a chance to disable the contact;
                if (contact != toiContact)
                {
                    contact.Update(ContactManager);
                }

                // Did the user disable the contact?
                if (contact.IsEnabled == false)
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
            for (int i = 0; i < ContactList.Count; i++)
            {
                Contact c = ContactList[i];

                // Enable the contact
                c.Flags |= ContactFlags.Enabled;

                // Set the number of TOI events for this contact to zero.
                c.ToiCount = 0;
            }

            // Initialize the TOI flag.
            for (int i = 0; i < BodyList.Count; i++)
            {
                Body body = BodyList[i];
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
            for (int i = 0; i < BodyList.Count; i++)
            {
                Body body = BodyList[i];
                if ((body._flags & BodyFlags.Toi) == BodyFlags.Toi)
                {
                    continue;
                }

                if (body.IsBullet)
                {
                    continue;
                }

                SolveTOI(body);

                body._flags |= BodyFlags.Toi;
            }

            // Collide bullets.
            for (int i = 0; i < BodyList.Count; i++)
            {
                Body body = BodyList[i];
                if ((body._flags & BodyFlags.Toi) == BodyFlags.Toi)
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
    }
}