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
        public int drawCoreShapes;
        public int drawAABBs;
        public int drawOBBs;
        public int drawPairs;
        public int drawContactPoints;
        public int drawContactNormals;
        public int drawContactForces;
        public int drawFrictionForces;
        public int drawController;
        public int drawCOMs;
        public int drawStats;
        public int enableWarmStarting;
        public int enableContinuous;
        public int pause;
        public int singleStep;

        public Settings()
        {
            hz = 60.0f;
            velocityIterations = 10;
            positionIterations = 8;
            //TODO: Set this to 0 - It is not in Box2D
            drawStats = 1;
            drawShapes = 1;
            drawJoints = 1;
            drawCoreShapes = 0;
            drawAABBs = 0;
            drawOBBs = 0;
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
            drawController = 1;
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

    public class BoundaryListener : Box2DX.Dynamics.BoundaryListener
    {
        public override void Violation(Body body)
        {
            if (test._bomb != body)
            {
                test.BoundaryViolated(body);
            }
        }

        public Test test;
    }

    public struct ContactPoint
    {
        public Fixture fixtureA;
        public Fixture fixtureB;
        public Vec2 normal;
        public Vec2 position;
        public PointState state;
    }

    public class Test : ContactListener
    {
        public static TestEntry[] g_testEntries = new TestEntry[]
		{			
			new TestEntry("Sensor Test", SensorTest.Create),
	        new TestEntry("CCD Test", CCDTest.Create),
	        new TestEntry("SphereStack", SphereStack.Create),
	        new TestEntry("Vertical Stack", VerticalStack.Create),
	        new TestEntry("Time of Impact", TimeOfImpact.Create),
	        new TestEntry("Distance Test", DistanceTest.Create),
	        new TestEntry("Static Edges", StaticEdges.Create),            
	        new TestEntry("Pyramid And Static Edges", PyramidStaticEdges.Create),
	        new TestEntry("PolyCollision", PolyCollision.Create),
	        //new TestEntry("Dynamic Tree", DynamicTreeTest.Create),        // TODO - Finish porting DynamicTree
	        new TestEntry("Dynamic Edges", DynamicEdges.Create),
	        new TestEntry("Line Joint", LineJoint.Create),
	        new TestEntry("Pyramid", Pyramid.Create),
	        new TestEntry("Prismatic", Prismatic.Create),
	        new TestEntry("Revolute", Revolute.Create),
	        new TestEntry("Bridge", Bridge.Create),
	        //new TestEntry("Breakable Body", BreakableBody.Create),        // TODO - Port TriangleMesh
	        new TestEntry("Polygon Shapes", PolyShapes.Create),
	        new TestEntry("Theo Jansen's Walker", TheoJansen.Create),
	        new TestEntry("Web", Web.Create),
	        new TestEntry("Varying Friction", VaryingFriction.Create),
	        new TestEntry("Varying Restitution", VaryingRestitution.Create),
	        new TestEntry("Dominos", Dominos.Create),
	        new TestEntry("Biped Test", BipedTest.Create),
	        new TestEntry("Car", Car.Create),
	        new TestEntry("Gears", Gears.Create),
	        new TestEntry("Slider Crank", SliderCrank.Create),
	        new TestEntry("Compound Shapes", CompoundShapes.Create),
	        new TestEntry("Chain", Chain.Create),
	        new TestEntry("Collision Processing", CollisionProcessing.Create),
	        new TestEntry("Collision Filtering", CollisionFiltering.Create),
	        new TestEntry("Apply Force", ApplyForce.Create),
	        new TestEntry("Pulleys", Pulleys.Create),
	        new TestEntry("Shape Editing", ShapeEditing.Create),
	        new TestEntry("Broad Phase", BroadPhaseTest.Create),
	        new TestEntry("Elastic Body", ElasticBody.Create),
	        new TestEntry("Raycast Test", RaycastTest.Create),
	        new TestEntry("Buoyancy", Buoyancy.Create),
            new TestEntry("DominoTower", DominoTower.Create),
            new TestEntry("Washing Machine", WashingMachine.Create)
		};

        public const int k_maxContactPoints = 2048;

        protected AABB _worldAABB;
        internal ContactPoint[] _points = new ContactPoint[k_maxContactPoints];
        internal int _pointCount;
        protected DestructionListener _destructionListener = new DestructionListener();
        protected BoundaryListener _boundaryListener = new BoundaryListener();
        internal DebugDraw _debugDraw = new OpenGLDebugDraw();
        protected int _textLine;
        internal World _world;
        internal Body _bomb;
        internal MouseJoint _mouseJoint;
        internal bool _bombSpawning;
        internal Vec2 _bombSpawnPoint;
        internal Vec2 _mouseWorld;
        internal int _stepCount;

        public Test()
        {
            _worldAABB = new AABB();
            _worldAABB.LowerBound.Set(-200.0f, -100.0f);
            _worldAABB.UpperBound.Set(200.0f, 200.0f);
            Vec2 gravity = new Vec2();
            gravity.Set(0.0f, -10.0f);
            bool doSleep = false;
            _world = new World(_worldAABB, gravity, doSleep);
            _bomb = null;
            _textLine = 30;
            _mouseJoint = null;
            _pointCount = 0;

            _destructionListener.test = this;
            _boundaryListener.test = this;
            _world.SetDestructionListener(_destructionListener);
            _world.SetBoundaryListener(_boundaryListener);
            _world.SetContactListener(this);
            _world.SetDebugDraw(_debugDraw);

            _bombSpawning = false;

            _stepCount = 0;
        }

        public override void PreSolve(Contact contact, Manifold oldManifold)
        {
            Manifold manifold = contact.GetManifold();

            if (manifold.PointCount == 0)
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

            for (int i = 0; i < manifold.PointCount && _pointCount < k_maxContactPoints; ++i)
            {
                ContactPoint cp = _points[_pointCount];
                cp.fixtureA = fixtureA;
                cp.fixtureB = fixtureB;
                cp.position = WorldManifold.Points[i];
                cp.normal = worldManifold.Normal;
                cp.state = state2[i];
                _points[_pointCount] = cp;
                ++_pointCount;
            }
        }

        public void DrawTitle(int x, int y, string text)
        {
            OpenGLDebugDraw.DrawString(x, y, text);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool state)
        {
            if (state)
            {
                // By deleting the world, we delete the bomb, mouse joint, etc.
                _world.Dispose();
                _world = null;
            }
        }

        public void SetTextLine(int line) { _textLine = line; }
        public virtual void Keyboard(System.Windows.Forms.Keys key) { ; }
        // Let derived tests know that a joint was destroyed.
        public virtual void JointDestroyed(Joint joint) { ; }
        public virtual void BoundaryViolated(Body body) { ; }

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
            const int k_maxCount = 10;
            Fixture[] fixtures = new Fixture[k_maxCount];
            int count = _world.Query(aabb, fixtures, k_maxCount);
            Body body = null;
            for (int i = 0; i < count; ++i)
            {
                Body b = fixtures[i].GetBody();
                if (b.IsStatic() == false && b.GetMass() > 0.0f)
                {
                    bool inside = fixtures[i].TestPoint(p);
                    if (inside)
                    {
                        body = b;
                        break;
                    }
                }
            }

            if (body != null)
            {
                MouseJointDef md = new MouseJointDef();
                md.Body1 = _world.GetGroundBody();
                md.Body2 = body;
                md.Target = p;
#if TARGET_FLOAT32_IS_FIXED
		        md.maxForce = (body->GetMass() < 16.0)? 
			        (1000.0f * body->GetMass()) : float32(16000.0);
#else
                md.MaxForce = 1000.0f * body.GetMass();
#endif
                _mouseJoint = (MouseJoint)_world.CreateJoint(md);
                body.WakeUp();
            }
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

        public void ShiftMouseDown(Vec2 p)
        {
            _mouseWorld = p;

            if (_mouseJoint != null)
            {
                return;
            }

            SpawnBomb(p);
        }

        public void MouseUp(Vec2 p)
        {
            if (_mouseJoint != null)
            {
                _world.DestroyJoint(_mouseJoint);
                _mouseJoint = null;
            }

            if (_bombSpawning != null)
            {
                CompleteBombSpawn(p);
            }
        }

        public void MouseMove(Vec2 p)
        {
            if (_mouseJoint != null)
            {
                _mouseJoint.SetTarget(p);
            }
        }

        public void LaunchBomb()
        {
            Vec2 p = new Vec2(Math.Random(-15.0f, 15.0f), 30.0f);
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
            bd.AllowSleep = true;
            bd.Position = position;

            bd.IsBullet = true;
            _bomb = _world.CreateBody(bd);
            _bomb.SetLinearVelocity(velocity);

            CircleDef sd = new CircleDef();
            sd.Radius = 0.3f;
            sd.Density = 20.0f;
            sd.Restitution = 0.1f;

            Vec2 minV = position - new Vec2(0.3f, 0.3f);
            Vec2 maxV = position + new Vec2(0.3f, 0.3f);

            AABB aabb = new AABB();
            aabb.LowerBound = minV;
            aabb.UpperBound = maxV;

            bool inRange = _world.InRange(aabb);

            if (inRange)
            {
                _bomb.CreateFixture(sd);
                _bomb.SetMassFromShapes();
            }
        }

        public virtual void Step(Settings settings)
        {
            float timeStep = settings.hz > 0.0f ? 1.0f / settings.hz : 0.0f;

            if (settings.pause == 1)
            {
                if (settings.singleStep > 1)
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
            flags += settings.drawController * (int)DebugDraw.DrawFlags.Controller;
            flags += settings.drawCoreShapes * (int)DebugDraw.DrawFlags.CoreShape;
            flags += settings.drawAABBs * (int)DebugDraw.DrawFlags.Aabb;
            flags += settings.drawOBBs * (int)DebugDraw.DrawFlags.Obb;
            flags += settings.drawPairs * (int)DebugDraw.DrawFlags.Pair;
            flags += settings.drawCOMs * (int)DebugDraw.DrawFlags.CenterOfMass;
            _debugDraw.Flags = (DebugDraw.DrawFlags)flags;

            _world.SetWarmStarting(settings.enableWarmStarting > 0);
            _world.SetContinuousPhysics(settings.enableContinuous > 0);

            _pointCount = 0;

            _world.Step(timeStep, settings.velocityIterations, settings.positionIterations);

            if (timeStep > 0.0f)
            {
                ++_stepCount;
            }

            _world.Validate();

            if (_bomb != null && _bomb.IsFrozen())
            {
                _world.DestroyBody(_bomb);
                _bomb = null;
            }

            if (settings.drawStats == 1)
            {
                OpenGLDebugDraw.DrawString(5, _textLine, string.Format("proxies(max) = {0}({1}), pairs(max) = {2}({3})",
                                                         _world.GetProxyCount(), Box2DX.Common.Settings.MaxProxies,
                                                         _world.GetPairCount(), Box2DX.Common.Settings.MaxPairs));

                _textLine += 15;

                OpenGLDebugDraw.DrawString(5, _textLine, string.Format("bodies/contacts/joints = {0}/{1}/{2}",
                                                        _world.GetBodyCount(), _world.GetContactCount(), _world.GetJointCount()));
                _textLine += 15;

                //OpenGLDebugDraw.DrawString(5, _textLine, "heap bytes = %d", b2_byteCount);
                _textLine += 15;
            }

            if (_mouseJoint != null)
            {
                Body body = _mouseJoint.GetBody2();
                Vec2 p1 = body.GetWorldPoint(_mouseJoint._localAnchor);
                Vec2 p2 = _mouseJoint._target;

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
    }
}
