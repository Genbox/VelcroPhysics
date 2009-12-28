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

using Box2DX.Common;
using Box2DX.Collision;
using Box2DX.Dynamics;

using Tao.OpenGl;

namespace TestBed
{
    public delegate Test TestCreateFcn();

    public class Settings
    {
        public float hz;
        public int velocityIterations;
        public int positionIterations;
        public int drawShapes;
        public int drawJoints;
        public int drawAABBs;
        public int drawPairs;
        public int drawContactPoints;
        public int drawContactNormals;
        public int drawContactForces;
        public int drawFrictionForces;
        public int drawCOMs;
        public int drawStats;
        public int enableWarmStarting;
        public int enableContinuous;
        public int pause;
        public int singleStep;

        public Settings()
        {
            hz = 60.0f;
            velocityIterations = 8;
            positionIterations = 3;
            drawStats = 0;
            drawShapes = 1;
            drawJoints = 1;
            drawAABBs = 0;
            drawPairs = 0;
            drawContactPoints = 0;
            drawContactNormals = 0;
            drawContactForces = 0;
            drawFrictionForces = 0;
            drawCOMs = 0;
            enableWarmStarting = 1;
            enableContinuous = 1;
            pause = 0;
            singleStep = 0;
        }
    }

    public class TestEntry
    {
        public TestEntry(string name, TestCreateFcn fcn)
        {
            Name = name;
            CreateFcn = fcn;
        }

        public string Name;
        public TestCreateFcn CreateFcn;

        public override string ToString()
        {
            return Name;
        }
    }

    // This is called when a joint in the world is implicitly destroyed
    // because an attached body is destroyed. This gives us a chance to
    // nullify the mouse joint.
    public class DestructionListener : Box2DX.Dynamics.DestructionListener
    {
        public override void SayGoodbye(Fixture fixture) { /*B2_NOT_USED(fixture);*/ }
        public override void SayGoodbye(Joint joint)
        {
            if (test._mouseJoint == joint)
            {
                test._mouseJoint = null;
            }
            else
            {
                test.JointDestroyed(joint);
            }
        }

        public Test test;
    }

    public class ContactPoint
    {
        public Fixture fixtureA;
        public Fixture fixtureB;
        public Vec2 normal;
        public Vec2 position;
        public PointState state;
    }

    public class Test : ContactListener
    {


        public const int k_maxContactPoints = 2048;

        protected Body _groundBody;
        protected AABB _worldAABB;
        protected ContactPoint[] _points = new ContactPoint[k_maxContactPoints];
        protected int _pointCount;
        protected DestructionListener _destructionListener = new DestructionListener();
        protected DebugDraw _debugDraw = new OpenGLDebugDraw();
        protected int _textLine;
        protected World _world;
        protected Body _bomb;
        public MouseJoint _mouseJoint;
        protected Vec2 _bombSpawnPoint;
        protected bool _bombSpawning;
        protected Vec2 _mouseWorld;
        protected int _stepCount;

        public Test()
        {
            Vec2 gravity = new Vec2();
            gravity.Set(0.0f, -10.0f);
            bool doSleep = true;
            _world = new World(gravity, doSleep);
            _bomb = null;
            _textLine = 30;
            _mouseJoint = null;
            _pointCount = 0;

            _destructionListener.test = this;
            _world.SetDestructionListener(_destructionListener);
            _world.SetContactListener(this);
            _world.SetDebugDraw(_debugDraw);

            _bombSpawning = false;

            _stepCount = 0;

            BodyDef bodyDef = new BodyDef();
            _groundBody = _world.CreateBody(bodyDef);

            for (int i = 0; i < _points.Length; i++)
            {
                _points[i] = new ContactPoint();
            }
        }

        public void SetTextLine(int line) { _textLine = line; }

        public void DrawTitle(int x, int y, string text)
        {
            OpenGLDebugDraw.DrawString(x, y, text);
        }

        public virtual void Step(Settings settings)
        {
            float timeStep = settings.hz > 0.0f ? 1.0f / settings.hz : 0.0f;

            if (settings.pause == 1)
            {
                if (settings.singleStep == 1)
                {
                    settings.singleStep = 0;
                }
                else
                {
                    timeStep = 0.0f;
                }

                OpenGLDebugDraw.DrawString(5, _textLine, "****PAUSED****");
                _textLine += 15;
            }

            int flags = 0;
            flags += settings.drawShapes * (int)DebugDraw.DrawFlags.Shape;
            flags += settings.drawJoints * (int)DebugDraw.DrawFlags.Joint;
            flags += settings.drawAABBs * (int)DebugDraw.DrawFlags.Aabb;
            flags += settings.drawPairs * (int)DebugDraw.DrawFlags.Pair;
            flags += settings.drawCOMs * (int)DebugDraw.DrawFlags.CenterOfMass;
            _debugDraw.SetFlags((DebugDraw.DrawFlags)flags);

            _world.SetWarmStarting(settings.enableWarmStarting > 0);
            _world.SetContinuousPhysics(settings.enableContinuous > 0);

            _pointCount = 0;

            _world.Step(timeStep, settings.velocityIterations, settings.positionIterations);
            _world.ClearForces();

            _world.DrawDebugData();

            if (timeStep > 0.0f)
            {
                ++_stepCount;
            }

            if (settings.drawStats == 1)
            {
                OpenGLDebugDraw.DrawString(5, _textLine, string.Format("bodies/contacts/joints/proxies = {0}/{1}/{2}",
                                                         _world.GetBodyCount(), _world.GetContactCount(),
                                                         _world.GetJointCount(), _world.GetProxyCount()));
                _textLine += 15;

                //OpenGLDebugDraw.DrawString(5, _textLine, "heap bytes = %d", b2_byteCount);
                //_textLine += 15;
            }

            if (_mouseJoint != null)
            {
                Vec2 p1 = _mouseJoint.GetAnchorB();
                Vec2 p2 = _mouseJoint.GetTarget();

                Gl.glPointSize(4.0f);
                Gl.glColor3f(0.0f, 1.0f, 0.0f);
                Gl.glBegin(Gl.GL_POINTS);
                Gl.glVertex2f(p1.X, p1.Y);
                Gl.glVertex2f(p2.X, p2.Y);
                Gl.glEnd();
                Gl.glPointSize(1.0f);

                Gl.glColor3f(0.8f, 0.8f, 0.8f);
                Gl.glBegin(Gl.GL_LINES);
                Gl.glVertex2f(p1.X, p1.Y);
                Gl.glVertex2f(p2.X, p2.Y);
                Gl.glEnd();
            }

            if (_bombSpawning)
            {
                Gl.glPointSize(4.0f);
                Gl.glColor3f(0.0f, 0.0f, 1.0f);
                Gl.glBegin(Gl.GL_POINTS);
                Gl.glColor3f(0.0f, 0.0f, 1.0f);
                Gl.glVertex2f(_bombSpawnPoint.X, _bombSpawnPoint.Y);
                Gl.glEnd();

                Gl.glColor3f(0.8f, 0.8f, 0.8f);
                Gl.glBegin(Gl.GL_LINES);
                Gl.glVertex2f(_mouseWorld.X, _mouseWorld.Y);
                Gl.glVertex2f(_bombSpawnPoint.X, _bombSpawnPoint.Y);
                Gl.glEnd();
            }

            if (settings.drawContactPoints == 1)
            {
                //const float32 k_impulseScale = 0.1f;
                const float k_axisScale = 0.3f;

                for (int i = 0; i < _pointCount; ++i)
                {
                    ContactPoint point = _points[i];

                    if (point.state == PointState.AddState)
                    {
                        // Add
                        OpenGLDebugDraw.DrawPoint(point.position, 10.0f, new Color(0.3f, 0.95f, 0.3f));
                    }
                    else if (point.state == PointState.PersistState)
                    {
                        // Persist
                        OpenGLDebugDraw.DrawPoint(point.position, 5.0f, new Color(0.3f, 0.3f, 0.95f));
                    }

                    if (settings.drawContactNormals == 1)
                    {
                        Vec2 p1 = point.position;
                        Vec2 p2 = p1 + k_axisScale * point.normal;
                        _debugDraw.DrawSegment(p1, p2, new Color(0.4f, 0.9f, 0.4f));
                    }
                    else if (settings.drawContactForces == 1)
                    {
                        //Vec2 p1 = point.position;
                        //Vec2 p2 = p1 + k_forceScale * point.normalForce * point.normal;
                        //DrawSegment(p1, p2, b2Color(0.9f, 0.9f, 0.3f));
                    }

                    if (settings.drawFrictionForces == 1)
                    {
                        //Vec2 tangent = Vec2.Cross(point.normal, 1.0f);
                        //Vec2 p1 = point.position;
                        //Vec2 p2 = p1 + k_forceScale * point.tangentForce * tangent;
                        //DrawSegment(p1, p2, Color(0.9f, 0.9f, 0.3f));
                    }
                }
            }
        }

        public virtual void Keyboard(System.Windows.Forms.Keys key) { }

        public void ShiftMouseDown(Vec2 p)
        {
            _mouseWorld = p;

            if (_mouseJoint != null)
            {
                return;
            }

            SpawnBomb(p);
        }

        public class MyQueryCallback : QueryCallback
        {
            public MyQueryCallback(Vec2 point)
            {
                _point = point;
                _fixture = null;
            }

            public Vec2 _point;
            public Fixture _fixture;

            public override bool ReportFixture(Fixture fixture)
            {
                Body body = fixture.GetBody();
                if (body.GetType() == Body.BodyType.Dynamic)
                {
                    bool inside = fixture.TestPoint(_point);
                    if (inside)
                    {
                        _fixture = fixture;

                        // We are done, terminate the query.
                        return false;
                    }
                }

                // Continue the query.
                return true;
            }
        }

        public void MouseDown(Vec2 p)
        {
            _mouseWorld = p;

            if (_mouseJoint != null)
            {
                return;
            }

            // Make a small box.
            AABB aabb = new AABB();
            Vec2 d = new Vec2();
            d.Set(0.001f, 0.001f);
            aabb.LowerBound = p - d;
            aabb.UpperBound = p + d;

            // Query the world for overlapping shapes.
            MyQueryCallback callback = new MyQueryCallback(p);
            _world.QueryAABB(callback, aabb);

            if (callback._fixture != null)
            {
                Body body = callback._fixture.GetBody();
                MouseJointDef md = new MouseJointDef();
                md.BodyA = _groundBody;
                md.BodyB = body;
                md.Target = p;
                md.MaxForce = 1000.0f * body.GetMass();
                _mouseJoint = (MouseJoint)_world.CreateJoint(md);
                body.SetAwake(true);
            }
        }

        public void MouseUp(Vec2 p)
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

        public void MouseMove(Vec2 p)
        {
            _mouseWorld = p;

            if (_mouseJoint != null)
            {
                _mouseJoint.SetTarget(p);
            }
        }

        public void LaunchBomb()
        {
            Vec2 p = new Vec2(Math.RandomFloat(-15.0f, 15.0f), 30.0f);
            Vec2 v = -5.0f * p;
            LaunchBomb(p, v);
        }

        public void LaunchBomb(Vec2 position, Vec2 velocity)
        {
            if (_bomb != null)
            {
                _world.DestroyBody(_bomb);
                _bomb = null;
            }

            BodyDef bd = new BodyDef();
            bd.Type = Body.BodyType.Dynamic;
            bd.Position = position;
            bd.Bullet = true;
            _bomb = _world.CreateBody(bd);
            _bomb.SetLinearVelocity(velocity);

            CircleShape circle = new CircleShape();
            circle._radius = 0.3f;

            FixtureDef fd = new FixtureDef();
            fd.Shape = circle;
            fd.Density = 20.0f;
            fd.Restitution = 0.1f;

            Vec2 minV = position - new Vec2(0.3f, 0.3f);
            Vec2 maxV = position + new Vec2(0.3f, 0.3f);

            AABB aabb = new AABB();
            aabb.LowerBound = minV;
            aabb.UpperBound = maxV;

            _bomb.CreateFixture(fd);
        }

        public void SpawnBomb(Vec2 worldPt)
        {
            _bombSpawnPoint = worldPt;
            _bombSpawning = true;
        }

        public void CompleteBombSpawn(Vec2 p)
        {
            if (_bombSpawning == false)
            {
                return;
            }

            const float multiplier = 30.0f;
            Vec2 vel = _bombSpawnPoint - p;
            vel *= multiplier;
            LaunchBomb(_bombSpawnPoint, vel);
            _bombSpawning = false;
        }

        // Let derived tests know that a joint was destroyed.
        public virtual void JointDestroyed(Joint joint) { }

        // Callbacks for derived classes.
        public override void PreSolve(Contact contact, Manifold oldManifold)
        {
            Manifold manifold = contact.GetManifold();

            if (manifold._pointCount == 0)
            {
                return;
            }

            Fixture fixtureA = contact.GetFixtureA();
            Fixture fixtureB = contact.GetFixtureB();

            PointState[] state1 = new PointState[Box2DX.Common.Settings.MaxManifoldPoints];
            PointState[] state2 = new PointState[Box2DX.Common.Settings.MaxManifoldPoints];

            Collision.GetPointStates(state1, state2, oldManifold, manifold);

            WorldManifold worldManifold;
            contact.GetWorldManifold(out worldManifold);

            for (int i = 0; i < manifold._pointCount && _pointCount < k_maxContactPoints; ++i)
            {
                ContactPoint cp = _points[_pointCount];
                cp.fixtureA = fixtureA;
                cp.fixtureB = fixtureB;
                cp.position = worldManifold.Points[i];
                cp.normal = worldManifold.Normal;
                cp.state = state2[i];
                ++_pointCount;
            }
        }

        //public void Dispose()
        //{
        //    Dispose(true);
        //}

        //protected virtual void Dispose(bool state)
        //{
        //    if (state)
        //    {
        //        // By deleting the world, we delete the bomb, mouse joint, etc.
        //        _world.Dispose();
        //        _world = null;
        //    }
        //}
    }
}
