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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Framework
{
    public static class Rand
    {
        public static Random Random = new Random(0x2eed2eed);

        /// Random number in range [-1,1]
        public static float RandomFloat()
        {
            return (float)(Random.NextDouble() * 2.0 - 1.0);
        }

        /// Random floating point number in range [lo, hi]
        public static float RandomFloat(float lo, float hi)
        {
            float r = (float)Random.NextDouble();
            r = (hi - lo) * r + lo;
            return r;
        }
    }

    public class Settings
    {
        public uint DrawAABBs;
        public uint DrawCOMs;
        public uint DrawContactForces;
        public uint DrawContactNormals;
        public uint DrawContactPoints;
        public uint DrawFrictionForces;
        public uint DrawJoints;
        public uint DrawPairs;
        public uint DrawShapes;
        public uint DrawStats;
        public uint EnableContinuous;
        public uint EnableWarmStarting;
        public float Hz;
        public uint Pause;
        public int PositionIterations;
        public uint SingleStep;
        public int VelocityIterations;

        public Settings()
        {
            Hz = 60.0f;
            VelocityIterations = 8; // 10;
            PositionIterations = 3; // 8;
            DrawShapes = 1;
            DrawJoints = 1;
            EnableWarmStarting = 1;
            EnableContinuous = 1;
            //DrawAABBs = 1;
            //DrawStats = 1;
            //DrawCOMs = 1;
        }
    }

    public struct TestEntry
    {
        public Func<Test> CreateFcn;
        public string Name;
    }

    public struct ContactPoint
    {
        public Fixture FixtureA;
        public Fixture FixtureB;
        public Vector2 Normal;
        public Vector2 Position;
        public PointState State;
    }

    public class Test
    {
        private const int k_MaxContactPoints = 2048;
        internal int TextLine;
        internal World World;
        internal DebugViewXNA.DebugViewXNA DebugView;
        private Body _groundBody;
        private MouseJoint _mouseJoint;
        internal int PointCount;
        internal ContactPoint[] Points = new ContactPoint[k_MaxContactPoints];
        internal int StepCount;

        protected Test()
        {
            World = new World(new Vector2(0.0f, -10.0f), false);
                       
            TextLine = 30;

            World.JointRemoved += JointRemoved;
            World.ContactManager.PreSolve += PreSolve;
            World.ContactManager.PostSolve += PostSolve;
            World.ContactManager.BeginContact += BeginContact;
            World.ContactManager.EndContact += EndContact;

            StepCount = 0;

            _groundBody = World.Add();
        }

        public Game1 GameInstance { protected get; set; }

        public virtual void Initialize()
        {
            if (GameInstance.DebugViewEnabled)
                DebugView = new DebugViewXNA.DebugViewXNA(World);
        }

        private void JointRemoved(Joint joint)
        {
            if (_mouseJoint == joint)
            {
                _mouseJoint = null;
            }
        }

        public void SetTextLine(int line)
        {
            TextLine = line;
        }

        public void DrawTitle(int x, int y, string title)
        {
            DebugView.DrawString(x, y, title);
        }

        public virtual void Update(Settings settings)
        {
            float timeStep = settings.Hz > 0.0f ? 1.0f / settings.Hz : 0.0f;

            if (settings.Pause > 0)
            {
                if (settings.SingleStep > 0)
                {
                    settings.SingleStep = 0;
                }
                else
                {
                    timeStep = 0.0f;
                }

                DebugView.DrawString(50, TextLine, "****PAUSED****");
                TextLine += 15;
            }

            if (GameInstance.DebugViewEnabled)
            {
                uint flags = 0;
                flags += settings.DrawShapes * (uint)DebugViewFlags.Shape;
                flags += settings.DrawJoints * (uint)DebugViewFlags.Joint;
                flags += settings.DrawAABBs * (uint)DebugViewFlags.AABB;
                flags += settings.DrawPairs * (uint)DebugViewFlags.Pair;
                flags += settings.DrawCOMs * (uint)DebugViewFlags.CenterOfMass;
                DebugView.Flags = (DebugViewFlags)flags;
            }

            World.WarmStarting = (settings.EnableWarmStarting > 0);
            World.ContinuousPhysics = (settings.EnableContinuous > 0);

            PointCount = 0;

            World.Step(timeStep, settings.VelocityIterations, settings.PositionIterations);

            if (timeStep > 0.0f)
            {
                ++StepCount;
            }

            if (GameInstance.DebugViewEnabled)
            {
                DebugView.DrawDebugData();

                if (settings.DrawStats > 0)
                {
                    DebugView.DrawString(50, TextLine, "bodies/contacts/joints/proxies = {0:n}/{1:n}/{2:n}",
                                         World.BodyCount, World.ContactCount, World.JointCount,
                                         World.ProxyCount);
                    TextLine += 15;
                }

                if (_mouseJoint != null)
                {
                    Vector2 p1 = _mouseJoint.WorldAnchorB;
                    Vector2 p2 = _mouseJoint.Target;

                    DebugView.DrawPoint(p1, 0.5f, new Color(0.0f, 1.0f, 0.0f));
                    DebugView.DrawPoint(p1, 0.5f, new Color(0.0f, 1.0f, 0.0f));
                    DebugView.DrawSegment(p1, p2, new Color(0.8f, 0.8f, 0.8f));
                }

                if (settings.DrawContactPoints > 0)
                {
                    const float k_axisScale = 0.3f;

                    for (int i = 0; i < PointCount; ++i)
                    {
                        ContactPoint point = Points[i];

                        if (point.State == PointState.Add)
                        {
                            // Add
                            DebugView.DrawPoint(point.Position, 1.5f, new Color(0.3f, 0.95f, 0.3f));
                        }
                        else if (point.State == PointState.Persist)
                        {
                            // Persist
                            DebugView.DrawPoint(point.Position, 0.65f, new Color(0.3f, 0.3f, 0.95f));
                        }

                        if (settings.DrawContactNormals == 1)
                        {
                            Vector2 p1 = point.Position;
                            Vector2 p2 = p1 + k_axisScale * point.Normal;
                            DebugView.DrawSegment(p1, p2, new Color(0.4f, 0.9f, 0.4f));
                        }
                        else if (settings.DrawContactForces == 1)
                        {
                            //Vector2 p1 = point.position;
                            //Vector2 p2 = p1 + k_forceScale * point.normalForce * point.normal;
                            //DrawSegment(p1, p2, Color(0.9f, 0.9f, 0.3f));
                        }

                        if (settings.DrawFrictionForces == 1)
                        {
                            //Vector2 tangent = b2Cross(point.normal, 1.0f);
                            //Vector2 p1 = point.position;
                            //Vector2 p2 = p1 + k_forceScale * point.tangentForce * tangent;
                            //DrawSegment(p1, p2, Color(0.9f, 0.9f, 0.3f));
                        }
                    }
                }
            }
        }

        public virtual void Keyboard(KeyboardState state, KeyboardState oldState)
        {
        }

        public virtual void Gamepad(GamePadState state, GamePadState oldState)
        {
        }

        public virtual void Mouse(MouseState state, MouseState oldState)
        {
            Vector2 position;// = new Vector2(state.X,state.Y );
            position = GameInstance.ConvertScreenToWorld(state.X, state.Y);

            if (state.LeftButton == ButtonState.Released && oldState.LeftButton == ButtonState.Pressed)
            {
                MouseUp();
            }
            else if (state.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
            {
                MouseDown(position);
            }

            MouseMove(position);
        }

        private void MouseDown(Vector2 p)
        {
            if (_mouseJoint != null)
            {
                return;
            }

            // Make a small box.
            AABB aabb;
            Vector2 d = new Vector2(0.001f, 0.001f);
            aabb.LowerBound = p - d;
            aabb.UpperBound = p + d;

            Fixture myFixture = null;

            // Query the world for overlapping shapes.
            World.QueryAABB(
                fixture =>
                {
                    Body body = fixture.Body;
                    if (body.BodyType == BodyType.Dynamic)
                    {
                        bool inside = fixture.TestPoint(ref p);
                        if (inside)
                        {
                            myFixture = fixture;

                            // We are done, terminate the query.
                            return false;
                        }
                    }

                    // Continue the query.
                    return true;
                }, ref aabb);

            if (myFixture != null)
            {
                Body body = myFixture.Body;
                _mouseJoint = new MouseJoint(_groundBody, body, p);
                _mouseJoint.MaxForce = 1000.0f * body.Mass;
                World.Add(_mouseJoint);
                body.Awake = true;
            }
        }

        private void MouseUp()
        {
            if (_mouseJoint != null)
            {
                World.Remove(_mouseJoint);
                _mouseJoint = null;
            }
        }

        private void MouseMove(Vector2 p)
        {
            if (_mouseJoint != null)
            {
                _mouseJoint.Target = p;
            }
        }

        // Let derived tests know that a joint was destroyed.
        public virtual void JointDestroyed(Joint joint)
        {
        }

        // Callbacks for derived classes.
        public virtual void BeginContact(Contact contact)
        {
        }

        public virtual void EndContact(Contact contact)
        {
        }

        public virtual void PreSolve(Contact contact, ref Manifold oldManifold)
        {
            Manifold manifold = contact.Manifold;

            if (manifold.PointCount == 0)
            {
                return;
            }

            Fixture fixtureA = contact.FixtureA;
            Fixture fixtureB = contact.FixtureB;

            FixedArray2<PointState> state1, state2;
            Collision.GetPointStates(out state1, out state2, ref oldManifold, ref manifold);

            WorldManifold worldManifold;
            contact.GetWorldManifold(out worldManifold);

            for (int i = 0; i < manifold.PointCount && PointCount < k_MaxContactPoints; ++i)
            {
                if (fixtureA == null)
                {
                    Points[i] = new ContactPoint();
                }
                ContactPoint cp = Points[PointCount];
                cp.FixtureA = fixtureA;
                cp.FixtureB = fixtureB;
                cp.Position = worldManifold.Points[i];
                cp.Normal = worldManifold.Normal;
                cp.State = state2[i];
                Points[PointCount] = cp;
                ++PointCount;
            }
        }

        public virtual void PostSolve(Contact contact, ref ContactImpulse impulse)
        {
        }
    }
}