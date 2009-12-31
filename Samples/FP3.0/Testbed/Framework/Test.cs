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
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.TestBed.Framework
{
    public class Rand
    {
        public static int RAND_LIMIT = 32767;
        public static Random rand = new Random(0x2eed2eed);

        /// Random number in range [-1,1]
        public static float RandomFloat()
        {
            return (float)(rand.NextDouble() * 2.0 - 1.0);
        }

        /// Random floating point number in range [lo, hi]
        public static float RandomFloat(float lo, float hi)
        {
            float r = (float)rand.NextDouble();
            r = (hi - lo) * r + lo;
            return r;
        }
    }

    public class Settings
    {
        public Settings()
        {
            hz = 60.0f;
            velocityIterations = 8; // 10;
            positionIterations = 3; // 8;
            drawShapes = 1;
            drawJoints = 1;
            enableWarmStarting = 1;
            enableContinuous = 1;
        }

        public float hz;
        public int velocityIterations;
        public int positionIterations;
        public uint drawShapes;
        public uint drawJoints;
        public uint drawAABBs;
        public uint drawPairs;
        public uint drawContactPoints;
        public uint drawContactNormals;
        public uint drawContactForces;
        public uint drawFrictionForces;
        public uint drawCOMs;
        public uint drawStats;
        public uint enableWarmStarting;
        public uint enableContinuous;
        public uint pause;
        public uint singleStep;
    }

    public struct TestEntry
    {
        public string name;
        public Func<Test> createFcn;
    }

    public struct ContactPoint
    {
        public Fixture fixtureA;
        public Fixture fixtureB;
        public Vector2 normal;
        public Vector2 position;
        public PointState state;
    }

    public class Test
    {
        protected Test()
        {
            _world = new World(new Vector2(0.0f, -10.0f), true);
            _debugView = new DebugViewXNA.DebugViewXNA(_world);
            _textLine = 30;
            
            _world.JointRemoved += JointRemoved;
            _world.ContactManager.PreSolve += PreSolve;
            _world.ContactManager.PostSolve += PostSolve;
            _world.ContactManager.BeginContact += BeginContact;
            _world.ContactManager.EndContact += EndContact;

            _bombSpawning = false;

            BodyDef bodyDef = new BodyDef();
            _groundBody = _world.CreateBody(bodyDef);
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
            _textLine = line;
        }

        public void DrawTitle(int x, int y, string title)
        {
            _debugView.DrawString(x, y, title);
        }

        public virtual void Step(Settings settings)
        {
            float timeStep = settings.hz > 0.0f ? 1.0f / settings.hz : 0.0f;

            if (settings.pause > 0)
            {
                if (settings.singleStep > 0)
                {
                    settings.singleStep = 0;
                }
                else
                {
                    timeStep = 0.0f;
                }

                _debugView.DrawString(50, _textLine, "****PAUSED****");
                _textLine += 15;
            }

            uint flags = 0;
            flags += settings.drawShapes * (uint)DebugViewFlags.Shape;
            flags += settings.drawJoints * (uint)DebugViewFlags.Joint;
            flags += settings.drawAABBs * (uint)DebugViewFlags.AABB;
            flags += settings.drawPairs * (uint)DebugViewFlags.Pair;
            flags += settings.drawCOMs * (uint)DebugViewFlags.CenterOfMass;
            _debugView.Flags = (DebugViewFlags)flags;

            _world.WarmStarting = (settings.enableWarmStarting > 0);
            _world.ContinuousPhysics = (settings.enableContinuous > 0);

            _pointCount = 0;

            _world.Step(timeStep, settings.velocityIterations, settings.positionIterations);
            _world.ClearForces();

            _debugView.DrawDebugData();

            if (settings.drawStats > 0)
            {
                _debugView.DrawString(50, _textLine, "bodies/contacts/joints/proxies = {0:n}/{1:n}/{2:n}",
                                      _world.BodyCount, _world.ContactCount, _world.JointCount, _world.ProxyCount);
                _textLine += 15;
            }

            if (_mouseJoint != null)
            {
                Vector2 p1 = _mouseJoint.GetAnchorB();
                Vector2 p2 = _mouseJoint.GetTarget();

                _debugView.DrawPoint(p1, 0.5f, new Color(0.0f, 1.0f, 0.0f));
                _debugView.DrawPoint(p1, 0.5f, new Color(0.0f, 1.0f, 0.0f));
                _debugView.DrawSegment(p1, p2, new Color(0.8f, 0.8f, 0.8f));
            }

            if (_bombSpawning)
            {
                _debugView.DrawPoint(_bombSpawnPoint, 0.5f, new Color(0.0f, 0.0f, 1.0f));
                _debugView.DrawSegment(_mouseWorld, _bombSpawnPoint, new Color(0.8f, 0.8f, 0.8f));
            }

            if (settings.drawContactPoints > 0)
            {
                const float k_axisScale = 0.3f;

                for (int i = 0; i < _pointCount; ++i)
                {
                    ContactPoint point = _points[i];

                    if (point.state == PointState.Add)
                    {
                        // Add
                        _debugView.DrawPoint(point.position, 1.5f, new Color(0.3f, 0.95f, 0.3f));
                    }
                    else if (point.state == PointState.Persist)
                    {
                        // Persist
                        _debugView.DrawPoint(point.position, 0.65f, new Color(0.3f, 0.3f, 0.95f));
                    }

                    if (settings.drawContactNormals == 1)
                    {
                        Vector2 p1 = point.position;
                        Vector2 p2 = p1 + k_axisScale * point.normal;
                        _debugView.DrawSegment(p1, p2, new Color(0.4f, 0.9f, 0.4f));
                    }
                    else if (settings.drawContactForces == 1)
                    {
                        //Vector2 p1 = point.position;
                        //Vector2 p2 = p1 + k_forceScale * point.normalForce * point.normal;
                        //DrawSegment(p1, p2, Color(0.9f, 0.9f, 0.3f));
                    }

                    if (settings.drawFrictionForces == 1)
                    {
                        //Vector2 tangent = b2Cross(point.normal, 1.0f);
                        //Vector2 p1 = point.position;
                        //Vector2 p2 = p1 + k_forceScale * point.tangentForce * tangent;
                        //DrawSegment(p1, p2, Color(0.9f, 0.9f, 0.3f));
                    }
                }
            }
        }

        public Game1 Game { get; set; }
        
        public virtual void Keyboard(KeyboardState state, KeyboardState oldState)
        {
        }

        public virtual void Mouse(MouseState state, MouseState oldState)
        {
            Vector2 position;// = new Vector2(state.X,state.Y );
            position = Game.ConvertScreenToWorld(state.X, state.Y);

            if (state.LeftButton == ButtonState.Released && oldState.LeftButton == ButtonState.Pressed)
            {
                MouseUp(position);
            }
            else if (state.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
            {
                MouseDown(position);
            }

            MouseMove(position);
        }

        private void MouseDown(Vector2 p)
        {
            _mouseWorld = p;

            if (_mouseJoint != null)
            {
                return;
            }

            // Make a small box.
            AABB aabb;
            Vector2 d = new Vector2(0.001f, 0.001f);
            aabb.LowerBound = p - d;
            aabb.UpperBound = p + d;

            Fixture _fixture = null;

            // Query the world for overlapping shapes.
            _world.QueryAABB(
                (fixture) =>
                {
                    Body body = fixture.GetBody();
                    if (body.GetBodyType() == BodyType.Dynamic)
                    {
                        bool inside = fixture.TestPoint(p);
                        if (inside)
                        {
                            _fixture = fixture;

                            // We are done, terminate the query.
                            return false;
                        }
                    }

                    // Continue the query.
                    return true;
                }, ref aabb);

            if (_fixture != null)
            {
                Body body = _fixture.GetBody();
                MouseJointDef md = new MouseJointDef();
                md.BodyA = _groundBody;
                md.BodyB = body;
                md.Target = p;
                md.MaxForce = 1000.0f * body.GetMass();
                _mouseJoint = (MouseJoint)_world.CreateJoint(md);
                body.SetAwake(true);
            }
        }

        private void MouseUp(Vector2 p)
        {
            if (_mouseJoint != null)
            {
                _world.DestroyJoint(_mouseJoint);
                _mouseJoint = null;
            }

            if (_bombSpawning)
            {
                CompleteBombSpawn(p);
            }
        }

        private void MouseMove(Vector2 p)
        {
            _mouseWorld = p;

            if (_mouseJoint != null)
            {
                _mouseJoint.SetTarget(p);
            }
        }

        public void LaunchBomb()
        {
            Vector2 p = new Vector2(Rand.RandomFloat(-15.0f, 15.0f), 30.0f);
            Vector2 v = -5.0f * p;
            LaunchBomb(p, v);
        }

        private void LaunchBomb(Vector2 position, Vector2 velocity)
        {
            if (_bomb != null)
            {
                _world.DestroyBody(_bomb);
                _bomb = null;
            }

            BodyDef bd = new BodyDef();
            bd.Type = BodyType.Dynamic;
            bd.Position = position;

            bd.Bullet = true;
            _bomb = _world.CreateBody(bd);
            _bomb.SetLinearVelocity(velocity);

            CircleShape circle = new CircleShape(0.3f);

            FixtureDef fd = new FixtureDef();
            fd.Shape = circle;
            fd.Density = 20.0f;
            fd.Restitution = 0.1f;

            Vector2 minV = position - new Vector2(0.3f, 0.3f);
            Vector2 maxV = position + new Vector2(0.3f, 0.3f);

            AABB aabb;
            aabb.LowerBound = minV;
            aabb.UpperBound = maxV;

            _bomb.CreateFixture(fd);
        }

        public void SpawnBomb(Vector2 worldPt)
        {
            _bombSpawnPoint = worldPt;
            _bombSpawning = true;
        }

        private void CompleteBombSpawn(Vector2 p)
        {
            if (_bombSpawning == false)
            {
                return;
            }

            const float multiplier = 30.0f;
            Vector2 vel = _bombSpawnPoint - p;
            vel *= multiplier;
            LaunchBomb(_bombSpawnPoint, vel);
            _bombSpawning = false;
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
            Manifold manifold;
            contact.GetManifold(out manifold);

            if (manifold._pointCount == 0)
            {
                return;
            }

            Fixture fixtureA = contact.GetFixtureA();
            Fixture fixtureB = contact.GetFixtureB();

            FixedArray2<PointState> state1, state2;
            Collision.GetPointStates(out state1, out state2, ref oldManifold, ref manifold);

            WorldManifold worldManifold;
            contact.GetWorldManifold(out worldManifold);

            for (int i = 0; i < manifold._pointCount && _pointCount < k_maxContactPoints; ++i)
            {
                if (fixtureA == null)
                {
                    _points[i] = new ContactPoint();
                }
                ContactPoint cp = _points[_pointCount];
                cp.fixtureA = fixtureA;
                cp.fixtureB = fixtureB;
                cp.position = worldManifold.Points[i];
                cp.normal = worldManifold.Normal;
                cp.state = state2[i];
                _points[_pointCount] = cp;
                ++_pointCount;
            }
        }

        public virtual void PostSolve(Contact contact, ref ContactImpulse impulse)
        {
        }

        private Body _groundBody;
        internal ContactPoint[] _points = new ContactPoint[k_maxContactPoints];
        internal int _pointCount;
        internal DebugViewXNA.DebugViewXNA _debugView;
        internal int _textLine;
        internal World _world;
        private Body _bomb;
        private MouseJoint _mouseJoint;
        private Vector2 _bombSpawnPoint;
        private bool _bombSpawning;
        private Vector2 _mouseWorld;

        private const int k_maxContactPoints = 2048;
    }
}