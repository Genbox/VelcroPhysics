/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
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
        /// <summary>
        /// Flag that indicates a new fixture has been added to the world.
        /// </summary>
        NewFixture = (1 << 0),

        /// <summary>
        /// Flag that determines if the world is locked.
        /// </summary>
        Locked = (1 << 1),

        /// <summary>
        /// Flag that clear the forces after each time step.
        /// </summary>
        ClearForces = (1 << 2),
    }

    /// <summary>
    /// The world class manages all physics entities, dynamic simulation,
    /// and asynchronous queries.
    /// </summary>
    public class World
    {
        /// <summary>
        /// Fires whenever a body has been added
        /// </summary>
        public BodyDelegate BodyAdded;

        /// <summary>
        /// Fires whenever a body has been removed
        /// </summary>
        public BodyDelegate BodyRemoved;

        internal Queue<Contact> ContactPool = new Queue<Contact>(256);

        /// <summary>
        /// Fires whenever a fixture has been added
        /// </summary>
        public FixtureDelegate FixtureAdded;

        /// <summary>
        /// Fires whenever a fixture has been removed
        /// </summary>
        public FixtureDelegate FixtureRemoved;

        internal WorldFlags Flags;

        /// <summary>
        /// Fires whenever a joint has been added
        /// </summary>
        public JointDelegate JointAdded;

        /// <summary>
        /// Fires whenever a joint has been removed
        /// </summary>
        public JointDelegate JointRemoved;

        private float _invDt0;
        private Island _island = new Island();
        private Func<FixtureProxy, bool> _queryAABBCallback;
        private Func<int, bool> _queryAABBCallbackWrapper;
        private RayCastCallback _rayCastCallback;
        private RayCastCallbackInternal _rayCastCallbackWrapper;
        private Body[] _stack = new Body[64];
        private Contact[] _toiContacts = new Contact[Settings.MaxTOIContacts];
        private TOISolver _toiSolver = new TOISolver();

#if (!SILVERLIGHT)
        private Stopwatch _watch = new Stopwatch();
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="World"/> class.
        /// </summary>
        /// <param name="gravity">The gravity.</param>
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
            BodyList = new List<Body>(32);
            JointList = new List<Joint>(32);
        }

        public List<Controller> Controllers { get; private set; }

        public List<BreakableBody> BreakableBodyList { get; private set; }

        public float UpdateTime { get; private set; }

        public float ContinuousPhysicsTime { get; private set; }

        public float NewContactsTime { get; private set; }

        public float ControllersUpdateTime { get; private set; }

        public float ContactsUpdateTime { get; private set; }

        public float SolveUpdateTime { get; private set; }

        public float BreakableBodyTime { get; private set; }

        /// <summary>
        /// Get the number of broad-phase proxies.
        /// </summary>
        /// <value>The proxy count.</value>
        public int ProxyCount
        {
            get { return ContactManager.BroadPhase.ProxyCount; }
        }

        /// <summary>
        /// Get the number of contacts (each may have 0 or more contact points).
        /// </summary>
        /// <value>The contact count.</value>
        public int ContactCount
        {
            get { return ContactManager.ContactCount; }
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

        /// <summary>
        /// Set flag to control automatic clearing of forces after each time step.
        /// </summary>
        /// <value><c>true</c> if it should auto clear forces; otherwise, <c>false</c>.</value>
        public bool AutoClearForces
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

        /// <summary>
        /// Get the contact manager for testing.
        /// </summary>
        /// <value>The contact manager.</value>
        public ContactManager ContactManager { get; private set; }

        /// <summary>
        /// Get the world body list. With the returned body, use Body.Next to get
        /// the next body in the world list. A null body indicates the end of the list.
        /// </summary>
        /// <value>Thehead of the world body list.</value>
        public List<Body> BodyList { get; private set; }

        public List<Joint> JointList { get; private set; }

        /// <summary>
        /// Get the world contact list. With the returned contact, use Contact.GetNext to get
        /// the next contact in the world list. A null contact indicates the end of the list.
        /// </summary>
        /// <value>The head of the world contact list.</value>
        public Contact ContactList
        {
            get { return ContactManager.ContactList; }
        }

        /// <summary>
        /// Create a rigid body.
        /// Warning: This function is locked during callbacks.
        /// </summary>
        /// <returns></returns>
        public Body CreateBody()
        {
            Debug.Assert(!IsLocked);
            if (IsLocked)
            {
                return null;
            }

            Body body = new Body(this);

            // Add to world doubly linked list.
            BodyList.Add(body);

            if (BodyAdded != null)
                BodyAdded(body);

            return body;
        }

        /// <summary>
        /// Destroy a rigid body.
        /// Warning: This function is locked during callbacks.
        /// Warning: This automatically deletes all associated shapes and joints.
        /// </summary>
        /// <param name="body">The body.</param>
        public void RemoveBody(Body body)
        {
            Debug.Assert(BodyList.Count > 0);
            Debug.Assert(!IsLocked);

            if (IsLocked)
            {
                return;
            }

            // You tried to remove a body that is not contained in the BodyList.
            // Are you removing the body more than once?
            Debug.Assert(BodyList.Contains(body));

            // Delete the attached joints.
            JointEdge je = body.JointList;
            while (je != null)
            {
                JointEdge je0 = je;
                je = je.Next;

                RemoveJoint(je0.Joint);
            }
            body.JointList = null;

            // Delete the attached contacts.
            ContactEdge ce = body.ContactList;
            while (ce != null)
            {
                ContactEdge ce0 = ce;
                ce = ce.Next;
                ContactManager.Destroy(ce0.Contact);
            }
            body.ContactList = null;

            // Delete the attached fixtures. This destroys broad-phase proxies.

            foreach (Fixture fixture in body.FixtureList)
            {
                fixture.DestroyProxies(ContactManager.BroadPhase);
                fixture.Destroy();
            }

            body.FixtureList = null;

            // Remove world body list.
            BodyList.Remove(body);

            if (BodyRemoved != null)
                BodyRemoved(body);
        }

        /// <summary>
        /// Create a joint to constrain bodies together. This may cause the connected bodies to cease colliding.
        /// Warning: This function is locked during callbacks.
        /// </summary>
        /// <param name="joint">The joint.</param>
        public void AddJoint(Joint joint)
        {
            Debug.Assert(!IsLocked);
            if (IsLocked)
            {
                return;
            }

            // Connect to the world list.
            JointList.Add(joint);

            // Connect to the bodies' doubly linked lists.
            joint.EdgeA.Joint = joint;
            joint.EdgeA.Other = joint.BodyB;
            joint.EdgeA.Prev = null;
            joint.EdgeA.Next = joint.BodyA.JointList;

            if (joint.BodyA.JointList != null)
                joint.BodyA.JointList.Prev = joint.EdgeA;

            joint.BodyA.JointList = joint.EdgeA;

            // WIP David
            if (!joint.IsFixedType())
            {
                joint.EdgeB.Joint = joint;
                joint.EdgeB.Other = joint.BodyA;
                joint.EdgeB.Prev = null;
                joint.EdgeB.Next = joint.BodyB.JointList;

                if (joint.BodyB.JointList != null)
                    joint.BodyB.JointList.Prev = joint.EdgeB;

                joint.BodyB.JointList = joint.EdgeB;
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

            if (JointAdded != null)
                JointAdded(joint);

            // Note: creating a joint doesn't wake the bodies.
        }

        /// <summary>
        /// Destroy a joint. This may cause the connected bodies to begin colliding.
        /// Warning: This function is locked during callbacks.
        /// </summary>
        /// <param name="joint">The joint.</param>
        public void RemoveJoint(Joint joint)
        {
            Debug.Assert(!IsLocked);
            if (IsLocked)
            {
                return;
            }

            bool collideConnected = joint.CollideConnected;

            // Remove from the world list.
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
            if (joint.EdgeA.Prev != null)
            {
                joint.EdgeA.Prev.Next = joint.EdgeA.Next;
            }

            if (joint.EdgeA.Next != null)
            {
                joint.EdgeA.Next.Prev = joint.EdgeA.Prev;
            }

            if (joint.EdgeA == bodyA.JointList)
            {
                bodyA.JointList = joint.EdgeA.Next;
            }

            joint.EdgeA.Prev = null;
            joint.EdgeA.Next = null;

            // WIP David
            if (!joint.IsFixedType())
            {
                // Remove from body 2
                if (joint.EdgeB.Prev != null)
                {
                    joint.EdgeB.Prev.Next = joint.EdgeB.Next;
                }

                if (joint.EdgeB.Next != null)
                {
                    joint.EdgeB.Next.Prev = joint.EdgeB.Prev;
                }

                if (joint.EdgeB == bodyB.JointList)
                {
                    bodyB.JointList = joint.EdgeB.Next;
                }

                joint.EdgeB.Prev = null;
                joint.EdgeB.Next = null;
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

            if (JointRemoved != null)
            {
                JointRemoved(joint);
            }
        }

        /// <summary>
        /// Take a time step. This performs collision detection, integration,
        /// and consraint solution.
        /// </summary>
        /// <param name="dt">The amount of time to simulate, this should not vary.</param>
        public void Step(float dt)
        {
#if (!SILVERLIGHT)
            if (Settings.EnableDiagnostics)
                _watch.Start();
#endif

            // If new fixtures were added, we need to find the new contacts.
            if ((Flags & WorldFlags.NewFixture) == WorldFlags.NewFixture)
            {
                ContactManager.FindNewContacts();
                Flags &= ~WorldFlags.NewFixture;
            }

#if (!SILVERLIGHT)
            if (Settings.EnableDiagnostics)
                NewContactsTime = _watch.ElapsedTicks;
#endif

            Flags |= WorldFlags.Locked;

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

            step.dtRatio = _invDt0 * dt;

            //Update controllers
            foreach (Controller controller in Controllers)
            {
                controller.Update(dt);
            }

#if (!SILVERLIGHT)
            if (Settings.EnableDiagnostics)
                ControllersUpdateTime = _watch.ElapsedTicks - NewContactsTime;
#endif

            // Update contacts. This is where some contacts are destroyed.
            ContactManager.Collide();

#if (!SILVERLIGHT)
            if (Settings.EnableDiagnostics)
                ContactsUpdateTime = _watch.ElapsedTicks - (NewContactsTime + ControllersUpdateTime);
#endif
            // Integrate velocities, solve velocity raints, and integrate positions.
            if (step.dt > 0.0f)
            {
                Solve(ref step);
            }

#if (!SILVERLIGHT)
            if (Settings.EnableDiagnostics)
                SolveUpdateTime = _watch.ElapsedTicks - (NewContactsTime + ControllersUpdateTime + ContactsUpdateTime);
#endif

            // Handle TOI events.
            if (Settings.ContinuousPhysics && step.dt > 0.0f)
            {
                SolveTOI();
            }

#if (!SILVERLIGHT)
            if (Settings.EnableDiagnostics)
                ContinuousPhysicsTime = _watch.ElapsedTicks -
                                        (NewContactsTime + ControllersUpdateTime + ContactsUpdateTime + SolveUpdateTime);
#endif
            if (step.dt > 0.0f)
            {
                _invDt0 = step.inv_dt;
            }

            if ((Flags & WorldFlags.ClearForces) != 0)
            {
                ClearForces();
            }

            //We have to unlock the world here to support breakable bodies.
            Flags &= ~WorldFlags.Locked;

            foreach (BreakableBody breakableBody in BreakableBodyList)
            {
                breakableBody.Update();
            }

#if (!SILVERLIGHT)
            if (Settings.EnableDiagnostics)
                BreakableBodyTime = _watch.ElapsedTicks -
                                    (NewContactsTime + ControllersUpdateTime + ContactsUpdateTime + SolveUpdateTime +
                                     ContinuousPhysicsTime);

            if (Settings.EnableDiagnostics)
            {
                _watch.Stop();
                UpdateTime = _watch.ElapsedTicks;
                _watch.Reset();
            }
#endif
        }

        /// <summary>
        /// Call this after you are done with time steps to clear the forces. You normally
        /// call this after each call to Step, unless you are performing sub-steps. By default,
        /// forces will be automatically cleared, so you don't need to call this function.
        /// </summary>
        public void ClearForces()
        {
            foreach (Body body in BodyList)
            {
                body.Force = Vector2.Zero;
                body.Torque = 0.0f;
            }
        }

        /// <summary>
        /// Query the world for all fixtures that potentially overlap the
        /// provided AABB.
        /// </summary>
        /// <param name="callback">A user implemented callback class.</param>
        /// <param name="aabb">The aabb query box.</param>
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

        /// <summary>
        /// Ray-cast the world for all fixtures in the path of the ray. Your callback
        /// controls whether you get the closest point, any point, or n-points.
        /// The ray-cast ignores shapes that contain the starting point.
        /// </summary>
        /// <param name="callback">A user implemented callback class.</param>
        /// <param name="point1">The ray starting point.</param>
        /// <param name="point2">The ray ending point.</param>
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
                Vector2 point = (1.0f - fraction) * input.Point1 + fraction * input.Point2;
                return _rayCastCallback(fixture, point, output.Normal, fraction);
            }

            return input.MaxFraction;
        }

        private void Solve(ref TimeStep step)
        {
            // Size the island for the worst case.
            _island.Reset(BodyList.Count,
                          ContactManager.ContactCount,
                          JointList.Count,
                          ContactManager);

            // Clear all the island flags.
            foreach (Body b in BodyList)
            {
                b.Flags &= ~BodyFlags.Island;
            }
            for (Contact c = ContactManager.ContactList; c != null; c = c.Next)
            {
                c.Flags &= ~ContactFlags.Island;
            }
            foreach (Joint j in JointList)
            {
                j.IslandFlag = false;
            }

            // Build and simulate all awake islands.
            int stackSize = BodyList.Count;
            if (stackSize > _stack.Length)
                _stack = new Body[Math.Max(_stack.Length * 2, stackSize)];

            for (int index = BodyList.Count - 1; index >= 0; index--)
            {
                Body seed = BodyList[index];
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

                // Perform a depth first search (DFS) on the constraint graph.
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
                        if (je.Joint.IslandFlag)
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
                            je.Joint.IslandFlag = true;

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
                            je.Joint.IslandFlag = true;
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
            foreach (Body b in BodyList)
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

        /// <summary>
        /// Advance a dynamic body to its first time of contact
        /// and adjust the position to ensure clearance.
        /// </summary>
        /// <param name="body">The body.</param>
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
                    int indexA = contact.ChildIndexA;
                    int indexB = contact.ChildIndexB;

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

        /// <summary>
        /// Sequentially solve TOIs for each body. We bring each
        /// body to the time of contact and perform some position correction.
        /// Time is not conserved.
        /// </summary>
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
            foreach (Body body in BodyList)
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
            foreach (Body body in BodyList)
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
            foreach (Body body in BodyList)
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

        public void RemoveBreakableBody(BreakableBody breakableBody)
        {
            //The breakable body list does not contain the body you tried to remove.
            Debug.Assert(BreakableBodyList.Contains(breakableBody));

            BreakableBodyList.Remove(breakableBody);
        }
    }
}
