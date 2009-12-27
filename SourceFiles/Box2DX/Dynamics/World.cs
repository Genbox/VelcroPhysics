/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2007 Erin Catto http://www.gphysics.com

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

using System;

using Box2DX.Common;
using Box2DX.Collision;
using Box2DX.Stuff;
using Math = Box2DX.Common.Math;

namespace Box2DX.Dynamics
{
    /// <summary>
    /// The world class manages all physics entities, dynamic simulation,
    /// and asynchronous queries.
    /// </summary>
    public class World : IDisposable
    {
        [Flags]
        public enum WorldFlags
        {
            NewFixture = 0x0001,
            Locked = 0x0002,
        };

        public WorldFlags _flags;

        public ContactManager _contactManager;

        private Body _bodyList;
        private Joint _jointList;

        private int _bodyCount;
        private int _jointCount;

        private Vec2 _gravity;
        private bool _allowSleep;

        private Body _groundBody;

        private DestructionListener _destructionListener;
        private DebugDraw _debugDraw;

        // This is used to compute the time step ratio to
        // support a variable time step.
        private float _inv_dt0;

        // This is for debugging the solver.
        private bool _warmStarting;

        // This is for debugging the solver.
        private bool _continuousPhysics;

        /// <summary>
        /// Construct a world object.
        /// </summary>
        /// <param name="gravity">The world gravity vector.</param>
        /// <param name="doSleep">Improve performance by not simulating inactive bodies.</param>
        public World(Vec2 gravity, bool doSleep)
        {
            _destructionListener = null;
            _debugDraw = null;

            _bodyList = null;
            _jointList = null;

            _bodyCount = 0;
            _jointCount = 0;

            _warmStarting = true;
            _continuousPhysics = true;

            _allowSleep = doSleep;
            _gravity = gravity;

            _flags = 0;

            _inv_dt0 = 0.0f;

            _contactManager = new ContactManager();
        }

        /// <summary>
        /// Register a destruction listener.
        /// </summary>
        /// <param name="listener"></param>
        public void SetDestructionListener(DestructionListener listener)
        {
            _destructionListener = listener;
        }

        /// <summary>
        /// Register a contact filter to provide specific control over collision.
        /// Otherwise the default filter is used (b2_defaultFilter).
        /// </summary>
        /// <param name="filter"></param>
        public void SetContactFilter(ContactFilter filter)
        {
            _contactManager._contactFilter = filter;
        }

        /// <summary>
        /// Register a contact event listener
        /// </summary>
        /// <param name="listener"></param>
        public void SetContactListener(ContactListener listener)
        {
            _contactManager._contactListener = listener;
        }

        /// <summary>
        /// Register a routine for debug drawing. The debug draw functions are called
        /// inside the World.Step method, so make sure your renderer is ready to
        /// consume draw commands when you call Step().
        /// </summary>
        /// <param name="debugDraw"></param>
        public void SetDebugDraw(DebugDraw debugDraw)
        {
            _debugDraw = debugDraw;
        }

        /// <summary>
        /// Create a rigid body given a definition. No reference to the definition
        /// is retained.
        /// @warning This function is locked during callbacks.
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public Body CreateBody(BodyDef def)
        {
            Box2DXDebug.Assert(IsLocked() == false);
            if (IsLocked())
            {
                return null;
            }

            Body b = new Body(def, this);

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

        public void DestroyBody(Body b)
        {
            Box2DXDebug.Assert(_bodyCount > 0);
            Box2DXDebug.Assert(IsLocked() == false);
            if (IsLocked())
            {
                return;
            }

            // Delete the attached joints.
            JointEdge je = b._jointList;
            while (je != null)
            {
                JointEdge je0 = je;
                je = je.Next;

                if (_destructionListener != null)
                {
                    _destructionListener.SayGoodbye(je0.Joint);
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

                if (_destructionListener != null)
                {
                    _destructionListener.SayGoodbye(f0);
                }

                f0.DestroyProxy(_contactManager._broadPhase);
                f0.Destroy();
                f0 = null;
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
            b = null;
        }

        /// <summary>
        /// Create a joint to constrain bodies together. No reference to the definition
        /// is retained. This may cause the connected bodies to cease colliding.
        /// @warning This function is locked during callbacks.
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public Joint CreateJoint(JointDef def)
        {
            Box2DXDebug.Assert(IsLocked() == false);
            if (IsLocked())
            {
                return null;
            }

            Joint j = Joint.Create(def);

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
            j._edgeA.Other = j._bodyB;
            j._edgeA.Prev = null;
            j._edgeA.Next = j._bodyA._jointList;
            if (j._bodyA._jointList != null)
                j._bodyA._jointList.Prev = j._edgeA;
            j._bodyA._jointList = j._edgeA;

            j._edgeB.Joint = j;
            j._edgeB.Other = j._bodyA;
            j._edgeB.Prev = null;
            j._edgeB.Next = j._bodyB._jointList;
            if (j._bodyB._jointList != null)
                j._bodyB._jointList.Prev = j._edgeB;
            j._bodyB._jointList = j._edgeB;

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

        /// <summary>
        /// Destroy a joint. This may cause the connected bodies to begin colliding.
        /// @warning This function is locked during callbacks.
        /// </summary>
        /// <param name="j"></param>
        public void DestroyJoint(Joint j)
        {
            Box2DXDebug.Assert(IsLocked() == false);
            if (IsLocked())
            {
                return;
            }

            bool collideConnected = j._collideConnected;

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
            Body bodyA = j._bodyA;
            Body bodyB = j._bodyB;

            // Wake up connected bodies.
            bodyA.SetAwake(true);
            bodyB.SetAwake(true);

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

            Joint.Destroy(j);

            Box2DXDebug.Assert(_jointCount > 0);
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

        /// <summary>
        /// Take a time step. This performs collision detection, integration,
        /// and constraint solution.
        /// </summary>
        /// <param name="dt">The amount of time to simulate, this should not vary.</param>
        /// <param name="velocityIterations">For the velocity constraint solver.</param>
        /// <param name="positionIteration">For the positionconstraint solver.</param>
        public void Step(float dt, int velocityIterations, int positionIteration, bool resetForces)
        {
            // If new fixtures were added, we need to find the new contacts.
            if ((_flags & WorldFlags.NewFixture) != 0)
            {
                _contactManager.FindNewContacts();
                _flags &= ~WorldFlags.NewFixture;
            }

            _flags |= WorldFlags.Locked;

            TimeStep step = new TimeStep();
            step.Dt = dt;
            step.VelocityIterations = velocityIterations;
            step.PositionIterations = positionIteration;
            step.ResetForces = resetForces;

            if (dt > 0.0f)
            {
                step.Inv_Dt = 1.0f / dt;
            }
            else
            {
                step.Inv_Dt = 0.0f;
            }

            step.DtRatio = _inv_dt0 * dt;

            step.WarmStarting = _warmStarting;

            // Update contacts. This is where some contacts are destroyed.
            _contactManager.Collide();

            // Integrate velocities, solve velocity constraints, and integrate positions.
            if (step.Dt > 0.0f)
            {
                Solve(step);
            }

            // Handle TOI events.
            if (_continuousPhysics && step.Dt > 0.0f)
            {
                SolveTOI(step);
            }

            if (step.Dt > 0.0f)
            {
                _inv_dt0 = step.Inv_Dt;
            }

            _flags &= ~WorldFlags.Locked;
        }

        public class WorldQueryWrapper : IQueryEnabled
        {
            public bool QueryCallback(int proxyId)
            {
                Fixture fixture = (Fixture)BroadPhase.GetUserData(proxyId);
                return Callback.ReportFixture(fixture);
            }

            public BroadPhase BroadPhase;
            public QueryCallback Callback;
        };

        public void DrawDebugData()
        {
            if (_debugDraw == null)
            {
                return;
            }

            DebugDraw.DrawFlags flags = _debugDraw.Flags;

            if ((flags & DebugDraw.DrawFlags.Shape) != 0)
            {
                for (Body b = _bodyList; b != null; b = b.GetNext())
                {
                    Transform xf = b.GetTransform();
                    for (Fixture f = b.GetFixtureList(); f != null; f = f.GetNext())
                    {
                        if (b.IsActive() == false)
                        {
                            DrawShape(f, xf, new Color(0.5f, 0.5f, 0.3f));
                        }
                        else if (b.GetType() == Body.BodyType.Static)
                        {
                            DrawShape(f, xf, new Color(0.5f, 0.5f, 0.9f));
                        }
                        else if (b.GetType() == Body.BodyType.Kinematic)
                        {
                            DrawShape(f, xf, new Color(0.9f, 0.9f, 0.9f));
                        }
                        else if (b.IsAwake() == false)
                        {
                            DrawShape(f, xf, new Color(0.6f, 0.6f, 0.6f));
                        }
                        else
                        {
                            DrawShape(f, xf, new Color(0.9f, 0.7f, 0.7f));
                        }
                    }
                }
            }

            if ((flags & DebugDraw.DrawFlags.Joint) != 0)
            {
                for (Joint j = _jointList; j != null; j = j.GetNext())
                {
                    DrawJoint(j);
                }
            }

            if ((flags & DebugDraw.DrawFlags.Pair) != 0)
            {
                Color color = new Color(0.3f, 0.9f, 0.9f);
                for (Contact c = _contactManager._contactList; c != null; c = c.GetNext())
                {
                    Fixture fixtureA = c.GetFixtureA();
                    Fixture fixtureB = c.GetFixtureB();

                    Vec2 cA = fixtureA.GetAABB().GetCenter();
                    Vec2 cB = fixtureB.GetAABB().GetCenter();

                    _debugDraw.DrawSegment(cA, cB, color);
                }
            }

            if ((flags & DebugDraw.DrawFlags.Aabb) != 0)
            {
                Color color = new Color(0.9f, 0.3f, 0.9f);
                BroadPhase bp = _contactManager._broadPhase;

                for (Body b = _bodyList; b != null; b = b.GetNext())
                {
                    if (b.IsActive() == false)
                    {
                        continue;
                    }

                    for (Fixture f = b.GetFixtureList(); f != null; f = f.GetNext())
                    {
                        AABB aabb = bp.GetFatAABB(f._proxyId);
                        Vec2[] vs = new Vec2[4];
                        vs[0].Set(aabb.LowerBound.X, aabb.LowerBound.Y);
                        vs[1].Set(aabb.UpperBound.X, aabb.LowerBound.Y);
                        vs[2].Set(aabb.UpperBound.X, aabb.UpperBound.Y);
                        vs[3].Set(aabb.LowerBound.X, aabb.UpperBound.Y);

                        _debugDraw.DrawPolygon(vs, 4, color);
                    }
                }
            }

            if ((flags & DebugDraw.DrawFlags.CenterOfMass) != 0)
            {
                for (Body b = _bodyList; b != null; b = b.GetNext())
                {
                    Transform xf = b.GetTransform();
                    xf.Position = b.GetWorldCenter();
                    _debugDraw.DrawTransform(xf);
                }
            }
        }

        public void QueryAABB(QueryCallback callback, AABB aabb)
        {
            WorldQueryWrapper wrapper = new WorldQueryWrapper();
            wrapper.BroadPhase = _contactManager._broadPhase;
            wrapper.Callback = callback;
            _contactManager._broadPhase.Query(wrapper, aabb);
        }

        public class WorldRayCastWrapper : IRayCastEnabled
        {
            public float RayCastCallback(RayCastInput input, int proxyId)
            {
                object userData = broadPhase.GetUserData(proxyId);
                Fixture fixture = (Fixture)userData;
                RayCastOutput output;
                bool hit = fixture.RayCast(out output, ref input);

                if (hit)
                {
                    float fraction = output.Fraction;
                    Vec2 point = (1.0f - fraction) * input.P1 + fraction * input.P2;
                    return callback.ReportFixture(fixture, point, output.Normal, fraction);
                }

                return input.MaxFraction;
            }

            public BroadPhase broadPhase;
            public RayCastCallback callback;
        };

        /// Ray-cast the world for all fixtures in the path of the ray. Your callback
        /// controls whether you get the closest point, any point, or n-points.
        /// The ray-cast ignores shapes that contain the starting point.
        /// @param callback a user implemented callback class.
        /// @param point1 the ray starting point
        /// @param point2 the ray ending point
        public void RayCast(RayCastCallback callback, Vec2 point1, Vec2 point2)
        {
            WorldRayCastWrapper wrapper = new WorldRayCastWrapper();
            wrapper.broadPhase = _contactManager._broadPhase;
            wrapper.callback = callback;
            RayCastInput input = new RayCastInput();
            input.MaxFraction = 1.0f;
            input.P1 = point1;
            input.P2 = point2;
            _contactManager._broadPhase.RayCast(wrapper, input);
        }

        /// <summary>
        /// Get the world body list. With the returned body, use Body.GetNext to get
        /// the next body in the world list. A null body indicates the end of the list.
        /// </summary>
        /// <returns>The head of the world body list.</returns>
        public Body GetBodyList()
        {
            return _bodyList;
        }

        /// <summary>
        /// Get the world joint list. With the returned joint, use Joint.GetNext to get
        /// the next joint in the world list. A null joint indicates the end of the list.
        /// </summary>
        /// <returns>The head of the world joint list.</returns>
        public Joint GetJointList()
        {
            return _jointList;
        }

        public Contact GetContactList()
        {
            return _contactManager._contactList;
        }

        /// <summary>
        /// Enable/disable warm starting. For testing.
        /// </summary>		
        public void SetWarmStarting(bool flag) { _warmStarting = flag; }

        /// <summary>
        /// Enable/disable continuous physics. For testing.
        /// </summary>		
        public void SetContinuousPhysics(bool flag) { _continuousPhysics = flag; }

        /// <summary>
        /// Get the number of broad-phase proxies.
        /// </summary>
        public int GetProxyCount() { return _contactManager._broadPhase.GetProxyCount(); }

        /// <summary>
        /// Get the number of bodies.
        /// </summary>
        /// <returns></returns>
        public int GetBodyCount() { return _bodyCount; }

        /// <summary>
        /// Get the number joints.
        /// </summary>
        /// <returns></returns>
        public int GetJointCount() { return _jointCount; }

        /// <summary>
        /// Get the number of contacts (each may have 0 or more contact points).
        /// </summary>
        /// <returns></returns>
        public int GetContactCount() { return _contactManager._contactCount; }

        /// <summary>
        /// Get\Set global gravity vector.
        /// </summary>
        public Vec2 Gravity { get { return _gravity; } set { _gravity = value; } }

        public bool IsLocked()
        {
            return (_flags & WorldFlags.Locked) == WorldFlags.Locked;
        }

        /// <summary>
        /// Destruct the world. All physics entities are destroyed.
        /// </summary>
        public void Dispose()
        {
        }

        // Find islands, integrate and solve constraints, solve position constraints
        private void Solve(TimeStep step)
        {
            // Size the island for the worst case.
            Island island = new Island(_bodyCount,
                            _contactManager._contactCount,
                            _jointCount,
                            _contactManager._contactListener);

            // Clear all the island flags.
            for (Body b = _bodyList; b != null; b = b._next)
            {
                b._flags &= ~Body.BodyFlags.IslandFlag;
            }
            for (Contact c = _contactManager._contactList; c != null; c = c._next)
            {
                c._flags &= ~ContactFlag.IslandFlag;
            }
            for (Joint j = _jointList; j != null; j = j._next)
            {
                j._islandFlag = false;
            }


            // Build and simulate all awake islands.
            int stackSize = _bodyCount;
            Body[] stack = new Body[stackSize];
            for (Body seed = _bodyList; seed != null; seed = seed._next)
            {
                //NOTE: Correct?
                if ((seed._flags & Body.BodyFlags.IslandFlag) == Body.BodyFlags.IslandFlag)
                {
                    continue;
                }

                if (seed.IsAwake() == false || seed.IsActive() == false)
                {
                    continue;
                }

                // The seed can be dynamic or kinematic.
                if (seed.GetType() == Body.BodyType.Static)
                {
                    continue;
                }

                // Reset island and stack.
                island.Clear();
                int stackCount = 0;
                stack[stackCount++] = seed;
                seed._flags |= Body.BodyFlags.IslandFlag;

                // Perform a depth first search (DFS) on the constraint graph.
                while (stackCount > 0)
                {
                    // Grab the next body off the stack and add it to the island.
                    Body b = stack[--stackCount];
                    Box2DXDebug.Assert(b.IsActive() == true);
                    island.Add(ref b);

                    // Make sure the body is awake.
                    if (b.IsAwake() == false)
                    {
                        b.SetAwake(true);
                    }

                    // To keep islands as small as possible, we don't
                    // propagate islands across static bodies.
                    if (b.GetType() == Body.BodyType.Static)
                    {
                        continue;
                    }

                    // Search all contacts connected to this body.
                    for (ContactEdge ce = b._contactList; ce != null; ce = ce.Next)
                    {
                        // Has this contact already been added to an island?
                        if ((ce.Contact._flags & ContactFlag.IslandFlag) != 0)
                        {
                            continue;
                        }

                        // Is this contact solid and touching?
                        if (ce.Contact.IsSensor() == true ||
                            ce.Contact.IsEnabled() == false ||
                            ce.Contact.IsTouching() == false)
                        {
                            continue;
                        }

                        island.Add(ref ce.Contact);
                        ce.Contact._flags |= ContactFlag.IslandFlag;

                        Body other = ce.Other;

                        // Was the other body already added to this island?
                        if ((other._flags & Body.BodyFlags.IslandFlag) != 0)
                        {
                            continue;
                        }

                        Box2DXDebug.Assert(stackCount < stackSize);
                        stack[stackCount++] = other;
                        other._flags |= Body.BodyFlags.IslandFlag;
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
                        if (other.IsActive() == false)
                        {
                            continue;
                        }

                        island.Add(je.Joint);
                        je.Joint._islandFlag = true;

                        if ((other._flags & Body.BodyFlags.IslandFlag) != 0)
                        {
                            continue;
                        }

                        Box2DXDebug.Assert(stackCount < stackSize);
                        stack[stackCount++] = other;
                        other._flags |= Body.BodyFlags.IslandFlag;
                    }
                }

                island.Solve(step, _gravity, _allowSleep);

                // Post solve cleanup.
                for (int i = 0; i < island._bodyCount; ++i)
                {
                    // Allow static bodies to participate in other islands.
                    Body b = island._bodies[i];
                    if (b.GetType() == Body.BodyType.Static)
                    {
                        b._flags &= ~Body.BodyFlags.IslandFlag;
                    }
                }
            }

            stack = null;

            // Synchronize shapes, check for out of range bodies.
            for (Body b = _bodyList; b != null; b = b.GetNext())
            {
                if (b.IsAwake() == false || b.IsActive() == false)
                {
                    continue;
                }

                if (b.GetType() == Body.BodyType.Static)
                {
                    continue;
                }

                // Update fixtures (for broad-phase).
                b.SynchronizeFixtures();
            }

            // Look for new contacts.
            _contactManager.FindNewContacts();
        }

        // Find TOI contacts and solve them.
        private void SolveTOI(TimeStep step)
        {
            // Reserve an island and a queue for TOI island solution.
            Island island = new Island(_bodyCount,
                            Settings.MaxTOIContactsPerIsland,
                             Settings.MaxTOIJointsPerIsland,
                            _contactManager._contactListener);

            //Simple one pass queue
            //Relies on the fact that we're only making one pass
            //through and each body can only be pushed/popped once.
            //To push: 
            //  queue[queueStart+queueSize++] = newElement;
            //To pop: 
            //	poppedElement = queue[queueStart++];
            //  --queueSize;
            int queueCapacity = _bodyCount;
            Body[] queue = new Body[queueCapacity];

            for (Body b = _bodyList; b != null; b = b._next)
            {
                b._flags &= ~Body.BodyFlags.IslandFlag;
                b._sweep.T0 = 0.0f;
            }

            for (Contact c = _contactManager._contactList; c != null; c = c._next)
            {
                // Invalidate TOI
                c._flags &= ~(ContactFlag.ToiFlag | ContactFlag.IslandFlag);
            }

            for (Joint j = _jointList; j != null; j = j._next)
            {
                j._islandFlag = false;
            }

            // Find TOI events and solve them.
            for (; ; )
            {
                // Find the first TOI.
                Contact minContact = null;
                float minTOI = 1.0f;

                for (Contact c = _contactManager._contactList; c != null; c = c._next)
                {
                    // Can this contact generate a solid TOI contact?
                    if (c.IsSensor() == true ||
                        c.IsEnabled() == false ||
                        c.IsContinuous() == false)
                    {
                        continue;
                    }

                    // TODO_ERIN keep a counter on the contact, only respond to M TOIs per contact.

                    float toi = 1.0f;
                    if ((c._flags & ContactFlag.ToiFlag) != 0)
                    {
                        // This contact has a valid cached TOI.
                        toi = c._toi;
                    }
                    else
                    {
                        // Compute the TOI for this contact.
                        Fixture s1 = c.GetFixtureA();
                        Fixture s2 = c.GetFixtureB();
                        Body b1 = s1.GetBody();
                        Body b2 = s2.GetBody();

                        if ((b1.GetType() != Body.BodyType.Dynamic || b1.IsAwake() == false) &&
                            (b2.GetType() != Body.BodyType.Dynamic || b2.IsAwake() == false))
                        {
                            continue;
                        }

                        // Put the sweeps onto the same time interval.
                        float t0 = b1._sweep.T0;

                        if (b1._sweep.T0 < b2._sweep.T0)
                        {
                            t0 = b2._sweep.T0;
                            b1._sweep.Advance(t0);
                        }
                        else if (b2._sweep.T0 < b1._sweep.T0)
                        {
                            t0 = b1._sweep.T0;
                            b2._sweep.Advance(t0);
                        }

                        Box2DXDebug.Assert(t0 < 1.0f);

                        // Compute the time of impact.
                        toi = c.ComputeTOI(b1._sweep, b2._sweep);

                        Box2DXDebug.Assert(0.0f <= toi && toi <= 1.0f);

                        // If the TOI is in range ...
                        if (0.0f < toi && toi < 1.0f)
                        {
                            // Interpolate on the actual range.
                            toi = Math.Min((1.0f - toi) * t0 + toi, 1.0f);
                        }


                        c._toi = toi;
                        c._flags |= ContactFlag.ToiFlag;
                    }

                    if (Settings.epsilon < toi && toi < minTOI)
                    {
                        // This is the minimum TOI found so far.
                        minContact = c;
                        minTOI = toi;
                    }
                }

                if (minContact == null || 1.0f - 100.0f * Settings.epsilon < minTOI)
                {
                    // No more TOI events. Done!
                    break;
                }

                // Advance the bodies to the TOI.
                Fixture f1 = minContact.GetFixtureA();
                Fixture f2 = minContact.GetFixtureB();
                Body b3 = f1.GetBody();
                Body b4 = f2.GetBody();

                Sweep backup1 = b3._sweep;
                Sweep backup2 = b4._sweep;

                b3.Advance(minTOI);
                b4.Advance(minTOI);

                // The TOI contact likely has some new contact points.
                minContact.Update(_contactManager._contactListener);
                minContact._flags &= ~ContactFlag.ToiFlag;

                // Is the contact solid?
                if (minContact.IsSensor() == true || minContact.IsEnabled() == false)
                {
                    // Restore the sweeps.
                    b3._sweep = backup1;
                    b4._sweep = backup2;
                    b3.SynchronizeTransform();
                    b4.SynchronizeTransform();
                    continue;
                }

                // Did numerical issues prevent a contact point from being generated?
                if (minContact.IsTouching() == false)
                {
                    // Give up on this TOI.
                    continue;
                }

                // Build the TOI island. We need a dynamic seed.
                Body seed = b3;
                if (seed.GetType() != Body.BodyType.Dynamic)
                {
                    seed = b4;
                }

                // Reset island and queue.
                island.Clear();

                int queueStart = 0; // starting index for queue
                int queueSize = 0;  // elements in queue
                queue[queueStart + queueSize++] = seed;
                seed._flags |= Body.BodyFlags.IslandFlag;

                // Perform a breadth first search (BFS) on the contact/joint graph.
                while (queueSize > 0)
                {
                    // Grab the next body off the stack and add it to the island.
                    Body b = queue[queueStart++];
                    --queueSize;

                    island.Add(ref b);

                    // Make sure the body is awake.
                    if (b.IsAwake() == false)
                    {
                        b.SetAwake(true);
                    }

                    // To keep islands as small as possible, we don't
                    // propagate islands across static or kinematic bodies.
                    if (b.GetType() != Body.BodyType.Dynamic)
                    {
                        continue;
                    }

                    // Search all contacts connected to this body.
                    for (ContactEdge cEdge = b._contactList; cEdge != null; cEdge = cEdge.Next)
                    {
                        // Does the TOI island still have space for contacts?
                        if (island._contactCount == island._contactCapacity)
                        {
                            break;
                        }

                        // Has this contact already been added to an island? Skip slow or non-solid contacts.
                        if ((cEdge.Contact._flags & ContactFlag.IslandFlag) != 0)
                        {
                            continue;
                        }

                        // Skip separate, sensor, or disabled contacts.
                        if (cEdge.Contact.IsSensor() == true ||
                            cEdge.Contact.IsEnabled() == false ||
                            cEdge.Contact.IsTouching() == false)
                        {
                            continue;
                        }

                        island.Add(ref cEdge.Contact);
                        cEdge.Contact._flags |= ContactFlag.IslandFlag;

                        // Update other body.
                        Body other = cEdge.Other;

                        // Was the other body already added to this island?
                        if ((other._flags & Body.BodyFlags.IslandFlag) != 0)
                        {
                            continue;
                        }

                        // Synchronize the connected body.
                        if (other.GetType() != Body.BodyType.Static)
                        {
                            other.Advance(minTOI);
                            other.SetAwake(true);
                        }

                        Box2DXDebug.Assert(queueStart + queueSize < queueCapacity);
                        queue[queueStart + queueSize] = other;
                        ++queueSize;
                        other._flags |= Body.BodyFlags.IslandFlag;
                    }

                    for (JointEdge jEdge = b._jointList; jEdge != null; jEdge = jEdge.Next)
                    {
                        if (island._jointCount == island._jointCapacity)
                        {
                            continue;
                        }

                        if (jEdge.Joint._islandFlag == true)
                        {
                            continue;
                        }

                        Body other = jEdge.Other;
                        if (other.IsActive() == false)
                        {
                            continue;
                        }

                        island.Add(jEdge.Joint);

                        jEdge.Joint._islandFlag = true;

                        if ((other._flags & Body.BodyFlags.IslandFlag) != 0)
                        {
                            continue;
                        }

                        // Synchronize the connected body.
                        if (other.GetType() != Body.BodyType.Static)
                        {
                            other.Advance(minTOI);
                            other.SetAwake(true);
                        }

                        Box2DXDebug.Assert(queueStart + queueSize < queueCapacity);
                        queue[queueStart + queueSize] = other;
                        ++queueSize;
                        other._flags |= Body.BodyFlags.IslandFlag;
                    }
                }

                TimeStep subStep = new TimeStep();
                subStep.WarmStarting = false;
                subStep.Dt = (1.0f - minTOI) * step.Dt;
                subStep.Inv_Dt = 1.0f / subStep.Dt;
                subStep.DtRatio = 0.0f;
                subStep.VelocityIterations = step.VelocityIterations;
                subStep.PositionIterations = step.PositionIterations;

                island.SolveTOI(ref subStep);

                // Post solve cleanup.
                for (int i = 0; i < island._bodyCount; ++i)
                {
                    // Allow bodies to participate in future TOI islands.
                    Body b = island._bodies[i];
                    b._flags &= ~Body.BodyFlags.IslandFlag;

                    if (b.IsAwake() == false)
                    {
                        continue;
                    }

                    if (b.GetType() == Body.BodyType.Static)
                    {
                        continue;
                    }

                    b.SynchronizeFixtures();

                    // Invalidate all contact TOIs associated with this body. Some of these
                    // may not be in the island because they were not touching.
                    for (ContactEdge ce = b._contactList; ce != null; ce = ce.Next)
                    {
                        ce.Contact._flags &= ~ContactFlag.ToiFlag;
                    }
                }

                for (int i = 0; i < island._contactCount; ++i)
                {
                    // Allow contacts to participate in future TOI islands.
                    Contact c = island._contacts[i];
                    c._flags &= ~(ContactFlag.ToiFlag | ContactFlag.IslandFlag);
                }

                for (int i = 0; i < island._jointCount; ++i)
                {
                    // Allow joints to participate in future TOI islands.
                    Joint j = island._joints[i];
                    j._islandFlag = false;
                }

                // Commit fixture proxy movements to the broad-phase so that new contacts are created.
                // Also, some contacts can be destroyed.
                _contactManager.FindNewContacts();
            }

            queue = null;
        }

        private void DrawJoint(Joint joint)
        {
            Body b1 = joint.GetBodyA();
            Body b2 = joint.GetBodyB();
            Transform xf1 = b1.GetTransform();
            Transform xf2 = b2.GetTransform();
            Vec2 x1 = xf1.Position;
            Vec2 x2 = xf2.Position;
            Vec2 p1 = joint.GetAnchorA();
            Vec2 p2 = joint.GetAnchorB();

            Color color = new Color(0.5f, 0.8f, 0.8f);

            switch (joint.GetType())
            {
                case JointType.DistanceJoint:
                    _debugDraw.DrawSegment(p1, p2, color);
                    break;

                case JointType.PulleyJoint:
                    {
                        PulleyJoint pulley = (PulleyJoint)joint;
                        Vec2 s1 = pulley.GroundAnchor1;
                        Vec2 s2 = pulley.GroundAnchor2;
                        _debugDraw.DrawSegment(s1, p1, color);
                        _debugDraw.DrawSegment(s2, p2, color);
                        _debugDraw.DrawSegment(s1, s2, color);
                    }
                    break;

                case JointType.MouseJoint:
                    // don't draw this
                    break;

                default:
                    _debugDraw.DrawSegment(x1, p1, color);
                    _debugDraw.DrawSegment(p1, p2, color);
                    _debugDraw.DrawSegment(x2, p2, color);
                    break;
            }
        }

        private void DrawShape(Fixture fixture, Transform xf, Color color)
        {
            switch (fixture.GetType())
            {
                case ShapeType.CircleShape:
                    {
                        CircleShape circle = (CircleShape)fixture.GetShape();

                        Vec2 center = Math.Mul(xf, circle._p);
                        float radius = circle._radius;
                        Vec2 axis = xf.R.Col1;

                        _debugDraw.DrawSolidCircle(center, radius, axis, color);
                    }
                    break;

                case ShapeType.PolygonShape:
                    {
                        PolygonShape poly = (PolygonShape)fixture.GetShape();
                        int vertexCount = poly._vertexCount;
                        Box2DXDebug.Assert(vertexCount <= Settings.MaxPolygonVertices);
                        Vec2[] vertices = new Vec2[Settings.MaxPolygonVertices];

                        for (int i = 0; i < vertexCount; ++i)
                        {
                            vertices[i] = Math.Mul(xf, poly._vertices[i]);
                        }

                        _debugDraw.DrawSolidPolygon(vertices, vertexCount, color);
                    }
                    break;
            }
        }
    }
}
