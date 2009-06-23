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
using System.Collections.Generic;
using System.Text;

using Box2DX.Common;
using Box2DX.Collision;
using Box2DX.Dynamics;

using Tao.OpenGl;
using Tao.FreeGlut;

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
		public int enableTOI;
		public int pause;
		public int singleStep;		

		public Settings()
		{
			hz = 60.0f;
			velocityIterations = 10;
			positionIterations = 8;
			drawStats = 0;
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
			enableTOI = 1;
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

	public enum ContactState
	{
		ContactAdded,
		ContactPersisted,
		ContactRemoved
	}

	public struct MyContactPoint
	{
		public Shape shape1;
		public Shape shape2;
		public Vec2 normal;
		public Vec2 position;
		public Vec2 velocity;
		public ContactID id;
		public ContactState state;
	}

	// This is called when a joint in the world is implicitly destroyed
	// because an attached body is destroyed. This gives us a chance to
	// nullify the mouse joint.
	public class MyDestructionListener : DestructionListener
	{
		public override void SayGoodbye(Shape shape) { ; }
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

	public class MyBoundaryListener : BoundaryListener
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

	public class MyContactListener : ContactListener
	{
		public override void Add(ContactPoint point)
		{
			if (test._pointCount == Test.k_maxContactPoints)
			{
				return;
			}

			MyContactPoint cp = new MyContactPoint();
			cp.shape1 = point.Shape1;
			cp.shape2 = point.Shape2;
			cp.position = point.Position;
			cp.normal = point.Normal;
			cp.id = point.ID;
			cp.state = ContactState.ContactAdded;
			test._points[test._pointCount] = cp;
			++test._pointCount;
		}

		public override void Persist(ContactPoint point)
		{
			if (test._pointCount == Test.k_maxContactPoints)
			{
				return;
			}

			MyContactPoint cp = new MyContactPoint();
			cp.shape1 = point.Shape1;
			cp.shape2 = point.Shape2;
			cp.position = point.Position;
			cp.normal = point.Normal;
			cp.id = point.ID;
			cp.state = ContactState.ContactPersisted;
			test._points[test._pointCount] = cp;
			++test._pointCount;
		}

		public override void Remove(ContactPoint point)
		{
			if (test._pointCount == Test.k_maxContactPoints)
			{
				return;
			}

			MyContactPoint cp = new MyContactPoint();
			cp.shape1 = point.Shape1;
			cp.shape2 = point.Shape2;
			cp.position = point.Position;
			cp.normal = point.Normal;
			cp.id = point.ID;
			cp.state = ContactState.ContactRemoved;
			test._points[test._pointCount] = cp;
			++test._pointCount;
		}

		public Test test;
	}

	public class Test : IDisposable
	{
		public static TestEntry[] g_testEntries = new TestEntry[]
		{			
			new TestEntry("Simple Test", SimpleTest.Create),
			new TestEntry("Line Joint Test", LineJoint.Create),
			new TestEntry("Pyramid", Pyramid.Create),
			new TestEntry("Prismatic", Prismatic.Create),
			new TestEntry("Revolute", Revolute.Create),
			new TestEntry("Theo Jansen's Walker", TheoJansen.Create),
			//new TestEntry("Contact Callback Test", ContactCB.Create),
			new TestEntry("Polygon Shapes", PolyShapes.Create),
			new TestEntry("Web", Web.Create),
			new TestEntry("Vertical Stack", VerticalStack.Create),
			new TestEntry("Varying Friction", VaryingFriction.Create),
			new TestEntry("Varying Restitution", VaryingRestitution.Create),
			new TestEntry("Bridge", Bridge.Create),
			new TestEntry("Dominos", Dominos.Create),
			new TestEntry("CCD Test", CCDTest.Create),
			new TestEntry("Biped Test", BipedTest.Create),
			new TestEntry("Sensor Test", SensorTest.Create),
			new TestEntry("Car", Car.Create),
			new TestEntry("Gears", Gears.Create),
			new TestEntry("Slider Crank", SliderCrank.Create),
			new TestEntry("Compound Shapes", CompoundShapes.Create),
			new TestEntry("Chain", Chain.Create),
			new TestEntry("Collision Processing", CollisionProcessing.Create),
			new TestEntry("Collision Filtering", CollisionFiltering.Create),
			new TestEntry("Motors and Limits", MotorsAndLimits.Create),
			new TestEntry("Apply Force", ApplyForce.Create),
			new TestEntry("Pulleys", Pulleys.Create),
			new TestEntry("Shape Editing", ShapeEditing.Create),
			new TestEntry("Time of Impact", TimeOfImpact.Create),
			new TestEntry("Distance Test", DistanceTest.Create),
			new TestEntry("Broad Phase", BroadPhaseTest.Create),
			new TestEntry("PolyCollision", PolyCollision.Create),
			new TestEntry("Elastic Body", ElasticBody.Create),
			new TestEntry("Raycast Test", RaycastTest.Create),
			new TestEntry("Buoyancy", Buoyancy.Create)
		};

		public static int k_maxContactPoints = 2048;

		protected AABB _worldAABB;
		internal MyContactPoint[] _points = new MyContactPoint[k_maxContactPoints];
		internal int _pointCount;
		protected MyDestructionListener _destructionListener = new MyDestructionListener();
		protected MyBoundaryListener _boundaryListener = new MyBoundaryListener();
		protected MyContactListener _contactListener = new MyContactListener();
		internal DebugDraw _debugDraw = new OpenGLDebugDraw();
		protected int _textLine;
		internal World _world;
		internal Body _bomb;
		internal MouseJoint _mouseJoint;

		public Test()
		{
			_worldAABB = new AABB();
			_worldAABB.LowerBound.Set(-200.0f, -100.0f);
			_worldAABB.UpperBound.Set(200.0f, 200.0f);
			Vec2 gravity = new Vec2();
			gravity.Set(0.0f, -10.0f);
			bool doSleep = true;
			_world = new World(_worldAABB, gravity, doSleep);
			_bomb = null;
			_textLine = 30;
			_mouseJoint = null;
			_pointCount = 0;

			_destructionListener.test = this;
			_boundaryListener.test = this;
			_contactListener.test = this;
			_world.SetDestructionListener(_destructionListener);
			_world.SetBoundaryListener(_boundaryListener);
			_world.SetContactListener(_contactListener);
			_world.SetDebugDraw(_debugDraw);
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
			int k_maxCount = 10;
			Shape[] shapes = new Shape[k_maxCount];
			int count = _world.Query(aabb, shapes, k_maxCount);
			Body body = null;
			for (int i = 0; i < count; ++i)
			{
				Body shapeBody = shapes[i].GetBody();
				if (shapeBody.IsStatic() == false && shapeBody.GetMass() > 0.0f)
				{
					bool inside = shapes[i].TestPoint(shapeBody.GetXForm(), p);
					if (inside)
					{
						body = shapes[i].GetBody();
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
				md.MaxForce = (body.GetMass() < 16.0f)? 
					(1000.0f * body.GetMass()) : 16000.0f;
#else
				md.MaxForce = 1000.0f * body.GetMass();
#endif
				_mouseJoint = (MouseJoint)_world.CreateJoint(md);
				body.WakeUp();
			}
		}

		public void MouseUp()
		{
			if (_mouseJoint != null)
			{
				_world.DestroyJoint(_mouseJoint);
				_mouseJoint = null;
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
			if (_bomb != null)
			{
				_world.DestroyBody(_bomb);
				_bomb = null;
			}

			BodyDef bd = new BodyDef();
			bd.AllowSleep = true;
			bd.Position.Set(Box2DX.Common.Math.Random(-15.0f, 15.0f), 30.0f);
			bd.IsBullet = true;
			_bomb = _world.CreateBody(bd);
			_bomb.SetLinearVelocity(-5.0f * bd.Position);

			CircleDef sd = new CircleDef();
			sd.Radius = 0.3f;
			sd.Density = 20.0f;
			sd.Restitution = 0.1f;
			_bomb.CreateShape(sd);

			_bomb.SetMassFromShapes();
		}

		public virtual void Step(Settings settings)
		{
			float timeStep = settings.hz > 0.0f ? 1.0f / settings.hz : 0.0f;

			if (settings.pause != 0)
			{
				if (settings.singleStep != 0)
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

			uint flags = 0;
			flags += (uint)settings.drawShapes * (uint)DebugDraw.DrawFlags.Shape;
			flags += (uint)settings.drawJoints * (uint)DebugDraw.DrawFlags.Joint;
			flags += (uint)settings.drawCoreShapes * (uint)DebugDraw.DrawFlags.CoreShape;
			flags += (uint)settings.drawAABBs * (uint)DebugDraw.DrawFlags.Aabb;
			flags += (uint)settings.drawOBBs * (uint)DebugDraw.DrawFlags.Obb;
			flags += (uint)settings.drawPairs * (uint)DebugDraw.DrawFlags.Pair;
			flags += (uint)settings.drawCOMs * (uint)DebugDraw.DrawFlags.CenterOfMass;
			flags += (uint)settings.drawController * (uint)DebugDraw.DrawFlags.Controller;
			_debugDraw.Flags = (DebugDraw.DrawFlags)flags;

			_world.SetWarmStarting(settings.enableWarmStarting > 0);
			_world.SetContinuousPhysics(settings.enableTOI > 0);

			_pointCount = 0;

			_world.Step(timeStep, settings.velocityIterations, settings.positionIterations);

			_world.Validate();

			if (_bomb != null && _bomb.IsFrozen())
			{
				_world.DestroyBody(_bomb);
				_bomb = null;
			}

			if (settings.drawStats != 0)
			{
				OpenGLDebugDraw.DrawString(5, _textLine, String.Format("proxies(max) = {0}({1}), pairs(max) = {2}({3})",
					new object[]{_world.GetProxyCount(), Box2DX.Common.Settings.MaxProxies,
						_world.GetPairCount(), Box2DX.Common.Settings.MaxProxies}));
				_textLine += 15;

				OpenGLDebugDraw.DrawString(5, _textLine, String.Format("bodies/contacts/joints = {0}/{1}/{2}",
					new object[] { _world.GetBodyCount(), _world.GetContactCount(), _world.GetJointCount() }));
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

			if (settings.drawContactPoints != 0)
			{
				//float k_forceScale = 0.01f;
				float k_axisScale = 0.3f;

				for (int i = 0; i < _pointCount; ++i)
				{
					MyContactPoint point = _points[i];

					if (point.state == ContactState.ContactAdded)
					{
						// Add
						OpenGLDebugDraw.DrawPoint(point.position, 10.0f, new Color(0.3f, 0.95f, 0.3f));
					}
					else if (point.state == ContactState.ContactPersisted)
					{
						// Persist
						OpenGLDebugDraw.DrawPoint(point.position, 5.0f, new Color(0.3f, 0.3f, 0.95f));
					}
					else
					{
						// Remove
						OpenGLDebugDraw.DrawPoint(point.position, 10.0f, new Color(0.95f, 0.3f, 0.3f));
					}

					if (settings.drawContactNormals == 1)
					{
						Vec2 p1 = point.position;
						Vec2 p2 = p1 + k_axisScale * point.normal;
						OpenGLDebugDraw.DrawSegment(p1, p2, new Color(0.4f, 0.9f, 0.4f));
					}
					else if (settings.drawContactForces == 1)
					{
						/*Vector2 p1 = point.position;
						Vector2 p2 = p1 + k_forceScale * point.normalForce * point.normal;
						OpenGLDebugDraw.DrawSegment(p1, p2, new Color(0.9f, 0.9f, 0.3f));*/
					}

					if (settings.drawFrictionForces == 1)
					{
						/*Vector2 tangent = Vector2.Cross(point.normal, 1.0f);
						Vector2 p1 = point.position;
						Vector2 p2 = p1 + k_forceScale * point.tangentForce * tangent;
						OpenGLDebugDraw.DrawSegment(p1, p2, new Color(0.9f, 0.9f, 0.3f));*/
					}
				}
			}
		}

		public override string ToString()
		{
			return GetType().Name;
		}
	}
}
