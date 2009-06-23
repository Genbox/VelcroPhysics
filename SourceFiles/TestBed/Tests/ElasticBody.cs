/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2008 Erin Catto http://www.gphysics.com

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

namespace TestBed
{
	public class ElasticBody : Test
	{
		Body[] bodies = new Body[64];
		Body _ground;
		Body _elev;
		PrismaticJoint _joint_elev;

		// Main...
		public ElasticBody()
		{
			// Bottom static body
			{
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(50.0f, 2.0f);
				sd.Friction = 0.1f;
				sd.Restitution = 0.1f;
				BodyDef bd = new BodyDef();
				bd.Position.Set(-1.0f, -7.5f);
				_ground = _world.CreateBody(bd);
				_ground.CreateShape(sd);
			}
			// Upper static body
			{
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(20.0f, 0.50f, new Vec2(0.0f, 0.0f), 0.047f * Box2DX.Common.Settings.Pi);
				sd.Friction = 0.01f;
				sd.Restitution = 0.001f;
				BodyDef bd = new BodyDef();
				bd.Position.Set(-20.0f, 93.0f);
				Body g = _world.CreateBody(bd);
				g.CreateShape(sd);
				sd.SetAsBox(15.0f, 0.50f, new Vec2(-15.0f, 12.5f), 0.0f);
				g.CreateShape(sd);

				sd.SetAsBox(20.0f, 0.5f, new Vec2(0.0f, -25.0f), -0.5f);
				g.CreateShape(sd);
			}
			// Left channel left wall
			{
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(0.7f, 55.0f);
				sd.Friction = 0.1f;
				sd.Restitution = 0.1f;
				BodyDef bd = new BodyDef();
				bd.Position.Set(-49.3f, 50.0f);
				Body g = _world.CreateBody(bd);
				g.CreateShape(sd);
			}
			// Right wall
			{
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(0.7f, 55.0f);
				sd.Friction = 0.1f;
				sd.Restitution = 0.1f;
				BodyDef bd = new BodyDef();
				bd.Position.Set(45.0f, 50.0f);
				Body g = _world.CreateBody(bd);
				g.CreateShape(sd);
			}
			// Left channel right upper wall  
			{
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(0.5f, 20.0f);
				sd.Friction = 0.05f;
				sd.Restitution = 0.01f;
				BodyDef bd = new BodyDef();
				bd.Position.Set(-42.0f, 70.0f);
				bd.Angle = -0.03f * Box2DX.Common.Settings.Pi;
				Body g = _world.CreateBody(bd);
				g.CreateShape(sd);
			}
			// Left channel right lower wall
			{
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(0.50f, 23.0f);
				sd.Friction = 0.05f;
				sd.Restitution = 0.01f;
				BodyDef bd = new BodyDef();
				bd.Position.Set(-44.0f, 27.0f);
				Body g = _world.CreateBody(bd);
				g.CreateShape(sd);
				// Bottom motors
				CircleDef cd = new CircleDef();
				cd.Radius = 3.0f;
				cd.Density = 15.0f;
				cd.Friction = 1.0f;
				cd.Restitution = 0.2f;
				// 1. 
				bd.Position.Set(-40.0f, 2.5f);
				Body body = _world.CreateBody(bd);
				body.CreateShape(cd);
				body.SetMassFromShapes();
				RevoluteJointDef jr = new RevoluteJointDef();
				jr.Initialize(g, body, body.GetWorldCenter() + new Vec2(0.0f, 1.0f));
				jr.MaxMotorTorque = 30000.0f;
				jr.EnableMotor = true;
				jr.MotorSpeed = 20.0f;
				_world.CreateJoint(jr);
				// 1. left down
				bd.Position.Set(-46.0f, -2.5f);
				cd.Radius = 1.5f; jr.MotorSpeed = -20.0f;
				body = _world.CreateBody(bd);
				body.CreateShape(cd);
				sd.SetAsBox(2.0f, 0.50f);
				body.CreateShape(sd);
				body.SetMassFromShapes();
				jr.Initialize(g, body, body.GetWorldCenter());
				_world.CreateJoint(jr);
				// 2.
				cd.Radius = 3.0f; jr.MotorSpeed = 20.0f;
				bd.Position.Set(-32.0f, 2.5f);
				body = _world.CreateBody(bd);
				body.CreateShape(cd);
				body.SetMassFromShapes();
				jr.Initialize(g, body, body.GetWorldCenter() + new Vec2(0.0f, 1.0f));
				_world.CreateJoint(jr);
				// 3.
				jr.MotorSpeed = 20.0f;
				bd.Position.Set(-24.0f, 1.5f);
				body = _world.CreateBody(bd);
				body.CreateShape(cd);
				body.SetMassFromShapes();
				jr.Initialize(g, body, body.GetWorldCenter() + new Vec2(0.0f, 1.0f));
				_world.CreateJoint(jr);
				// 4.
				bd.Position.Set(-16.0f, 0.8f);
				body = _world.CreateBody(bd);
				body.CreateShape(cd);
				body.SetMassFromShapes();
				jr.Initialize(g, body, body.GetWorldCenter() + new Vec2(0.0f, 1.0f));
				_world.CreateJoint(jr);
				// 5.
				bd.Position.Set(-8.0f, 0.5f);
				body = _world.CreateBody(bd);
				body.CreateShape(cd);
				body.SetMassFromShapes();
				jr.Initialize(g, body, body.GetWorldCenter() + new Vec2(0.0f, 1.0f));
				_world.CreateJoint(jr);
				// 6.
				bd.Position.Set(0.0f, 0.1f);
				body = _world.CreateBody(bd);
				body.CreateShape(cd);
				body.SetMassFromShapes();
				jr.Initialize(g, body, body.GetWorldCenter() + new Vec2(0.0f, 1.0f));
				_world.CreateJoint(jr);
				// 7.
				bd.Position.Set(8.0f, -0.5f);
				body = _world.CreateBody(bd);
				body.CreateShape(cd);
				sd.SetAsBox(3.7f, 0.5f);
				body.CreateShape(sd);
				body.SetMassFromShapes();
				jr.Initialize(g, body, body.GetWorldCenter() + new Vec2(0.0f, 1.0f));
				_world.CreateJoint(jr);
				// 8. right rotator
				sd.SetAsBox(5.0f, 0.5f);
				sd.Density = 2.0f;
				bd.Position.Set(18.0f, 1.0f);
				Body rightmotor = _world.CreateBody(bd);
				rightmotor.CreateShape(sd);
				sd.SetAsBox(4.5f, 0.5f, new Vec2(0.0f, 0.0f), Box2DX.Common.Settings.Pi / 3.0f);
				rightmotor.CreateShape(sd);
				sd.SetAsBox(4.5f, 0.5f, new Vec2(0.0f, 0.0f), Box2DX.Common.Settings.Pi * 2.0f / 3.0f);
				rightmotor.CreateShape(sd);
				cd.Radius = 4.2f;
				rightmotor.CreateShape(cd);
				rightmotor.SetMassFromShapes();
				jr.Initialize(g, rightmotor, rightmotor.GetWorldCenter());
				jr.MaxMotorTorque = 70000.0f;
				jr.MotorSpeed = -4.0f;
				_world.CreateJoint(jr);
				// 9. left rotator
				sd.SetAsBox(8.5f, 0.5f);
				sd.Density = 2.0f;
				bd.Position.Set(-34.0f, 17.0f);
				body = _world.CreateBody(bd);
				body.CreateShape(sd);
				sd.SetAsBox(8.5f, 0.5f, new Vec2(0.0f, 0.0f), Box2DX.Common.Settings.Pi * .5f);
				body.CreateShape(sd);
				cd.Radius = 7.0f;
				cd.Friction = 0.9f;
				body.CreateShape(cd);
				body.SetMassFromShapes();
				jr.Initialize(g, body, body.GetWorldCenter());
				jr.MaxMotorTorque = 100000.0f;
				jr.MotorSpeed = -5.0f;
				_world.CreateJoint(jr);
				// big compressor
				sd.SetAsBox(3.0f, 4.0f);
				sd.Density = 10.0f;
				bd.Position.Set(-16.0f, 17.0f);
				Body hammerleft = _world.CreateBody(bd);
				hammerleft.CreateShape(sd);
				hammerleft.SetMassFromShapes();
				DistanceJointDef jd = new DistanceJointDef();
				jd.Initialize(body, hammerleft, body.GetWorldCenter() + new Vec2(0.0f, 6.0f), hammerleft.GetWorldCenter());
				_world.CreateJoint(jd);

				bd.Position.Set(4.0f, 17.0f);
				Body hammerright = _world.CreateBody(bd);
				hammerright.CreateShape(sd);
				hammerright.SetMassFromShapes();
				jd.Initialize(body, hammerright, body.GetWorldCenter() - new Vec2(0.0f, 6.0f), hammerright.GetWorldCenter());
				_world.CreateJoint(jd);
				// pusher
				sd.SetAsBox(6.0f, 0.75f);
				bd.Position.Set(-21.0f, 9.0f);
				Body pusher = _world.CreateBody(bd);
				pusher.CreateShape(sd);
				sd.SetAsBox(2.0f, 1.5f, new Vec2(-5.0f, 0.0f), 0.0f);
				pusher.SetMassFromShapes();
				pusher.CreateShape(sd);
				jd.Initialize(rightmotor, pusher, rightmotor.GetWorldCenter() + new Vec2(-8.0f, 0.0f),
							  pusher.GetWorldCenter() + new Vec2(5.0f, 0.0f));
				_world.CreateJoint(jd);
			}
			// Static bodies above motors
			{
				PolygonDef sd = new PolygonDef();
				CircleDef cd = new CircleDef();
				sd.SetAsBox(9.0f, 0.5f);
				sd.Friction = 0.05f;
				sd.Restitution = 0.01f;
				BodyDef bd = new BodyDef();
				bd.Position.Set(-15.5f, 12.0f);
				bd.Angle = 0.0f;
				Body g = _world.CreateBody(bd);
				g.CreateShape(sd);

				sd.SetAsBox(8.0f, 0.5f, new Vec2(23.0f, 0.0f), 0.0f);
				g.CreateShape(sd);
				// compressor statics  
				sd.SetAsBox(7.0f, 0.5f, new Vec2(-2.0f, 9.0f), 0.0f);
				g.CreateShape(sd);
				sd.SetAsBox(9.0f, 0.5f, new Vec2(22.0f, 9.0f), 0.0f);
				g.CreateShape(sd);

				sd.SetAsBox(19.0f, 0.5f, new Vec2(-9.0f, 15.0f), -0.05f);
				g.CreateShape(sd);
				sd.SetAsBox(4.7f, 0.5f, new Vec2(15.0f, 11.5f), -0.5f);
				g.CreateShape(sd);
				// below compressor
				sd.SetAsBox(26.0f, 0.3f, new Vec2(17.0f, -4.4f), -0.02f);
				g.CreateShape(sd);
				cd.Radius = 1.0f; cd.Friction = 1.0f;
				cd.LocalPosition = new Vec2(29.0f, -6.0f);
				g.CreateShape(cd);
				cd.Radius = 0.7f;
				cd.LocalPosition = new Vec2(-2.0f, -4.5f);
				g.CreateShape(cd);
			}
			// Elevator
			{
				BodyDef bd = new BodyDef();
				CircleDef cd = new CircleDef();
				PolygonDef sd = new PolygonDef();

				bd.Position.Set(40.0f, 4.0f);
				_elev = _world.CreateBody(bd);

				sd.SetAsBox(0.5f, 2.5f, new Vec2(3.0f, -3.0f), 0.0f);
				sd.Density = 1.0f;
				sd.Friction = 0.01f;
				_elev.CreateShape(sd);
				sd.SetAsBox(7.0f, 0.5f, new Vec2(-3.5f, -5.5f), 0.0f);
				_elev.CreateShape(sd);
				sd.SetAsBox(0.5f, 2.5f, new Vec2(-11.0f, -3.5f), 0.0f);
				_elev.CreateShape(sd);
				_elev.SetMassFromShapes();

				PrismaticJointDef jp = new PrismaticJointDef();
				jp.Initialize(_ground, _elev, bd.Position, new Vec2(0.0f, 1.0f));
				jp.LowerTranslation = 0.0f;
				jp.UpperTranslation = 100.0f;
				jp.EnableLimit = true;
				jp.EnableMotor = true;
				jp.MaxMotorForce = 10000.0f;
				jp.MotorSpeed = 0.0f;
				_joint_elev = (PrismaticJoint)_world.CreateJoint(jp);

				// Korb
				sd.SetAsBox(2.3f, 0.5f, new Vec2(1.0f, 0.0f), 0.0f);
				sd.Density = 0.5f;
				bd.Position.Set(29.0f, 6.5f);
				Body body = _world.CreateBody(bd);
				body.CreateShape(sd);
				sd.SetAsBox(2.5f, 0.5f, new Vec2(3.0f, -2.0f), Box2DX.Common.Settings.Pi / 2.0f);
				body.CreateShape(sd);
				sd.SetAsBox(4.6f, 0.5f, new Vec2(7.8f, -4.0f), 0.0f);
				body.CreateShape(sd);
				sd.SetAsBox(0.5f, 4.5f, new Vec2(12.0f, 0.0f), 0.0f);
				body.CreateShape(sd);

				sd.SetAsBox(0.5f, 0.5f, new Vec2(13.0f, 4.0f), 0.0f);
				body.CreateShape(sd);

				cd.Radius = 0.7f; cd.Density = 1.0f; cd.Friction = 0.01f;
				cd.LocalPosition = new Vec2(0.0f, 0.0f);
				body.CreateShape(cd);
				body.SetMassFromShapes();

				RevoluteJointDef jr = new RevoluteJointDef();
				jr.Initialize(_elev, body, bd.Position);
				jr.EnableLimit = true;
				jr.LowerAngle = -0.2f;
				jr.UpperAngle = Box2DX.Common.Settings.Pi * 1.1f;
				jr.CollideConnected = true;
				_world.CreateJoint(jr);
				// upper body exit
				sd.SetAsBox(14.0f, 0.5f, new Vec2(-3.5f, -10.0f), 0.0f);
				bd.Position.Set(17.5f, 96.0f);
				body = _world.CreateBody(bd);
				body.CreateShape(sd);
			}
			// "Elastic body" 64 bodies - something like a lin. elastic compound
			// connected via dynamic forces (springs) 
			{
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(0.55f, 0.55f);
				sd.Density = 1.5f;
				sd.Friction = 0.01f;
				sd.Filter.GroupIndex = -1;
				Vec2 startpoint = new Vec2(30.0f, 20.0f);
				BodyDef bd = new BodyDef();
				bd.IsBullet = false;
				bd.AllowSleep = false;
				for (int i = 0; i < 8; ++i)
				{
					for (int j = 0; j < 8; ++j)
					{
						bd.Position.Set(j * 1.02f, 2.51f + 1.02f * i);
						bd.Position += startpoint;
						Body body = _world.CreateBody(bd);
						bodies[8 * i + j] = body;
						body.CreateShape(sd);
						body.SetMassFromShapes();
					}
				}
			}
		}

		/// <summary>
		/// Add a spring force
		/// </summary>
		void AddSpringForce(Body bA, Vec2 localA, Body bB, Vec2 localB, float k, float friction, float desiredDist)
		{
			Vec2 pA = bA.GetWorldPoint(localA);
			Vec2 pB = bB.GetWorldPoint(localB);
			Vec2 diff = pB - pA;
			//Find velocities of attach points
			Vec2 vA = bA.GetLinearVelocity() - Vec2.Cross(bA.GetWorldVector(localA), bA.GetAngularVelocity());
			Vec2 vB = bB.GetLinearVelocity() - Vec2.Cross(bB.GetWorldVector(localB), bB.GetAngularVelocity());
			Vec2 vdiff = vB - vA;
			float dx = diff.Normalize(); //normalizes diff and puts length into dx
			float vrel = vdiff.X * diff.X + vdiff.Y * diff.Y;
			float forceMag = -k * (dx - desiredDist) - friction * vrel;
			diff *= forceMag; // diff *= forceMag
			bB.ApplyForce(diff, bA.GetWorldPoint(localA));
			diff *= -1.0f;
			bA.ApplyForce(diff, bB.GetWorldPoint(localB));
		}

		/// <summary>
		///  Apply dynamic forces (springs) and check elevator state
		/// </summary>
		/// <param name="settings"></param>
		public override void Step(Settings settings)
		{
			base.Step(settings);
			for (int i = 0; i < 8; ++i)
			{
				for (int j = 0; j < 8; ++j)
				{
					Vec2 zero = new Vec2(0.0f, 0.0f);
					Vec2 down = new Vec2(0.0f, -0.5f);
					Vec2 up = new Vec2(0.0f, 0.5f);
					Vec2 right = new Vec2(0.5f, 0.0f);
					Vec2 left = new Vec2(-0.5f, 0.0f);
					int ind = i * 8 + j;
					int indr = ind + 1;
					int indd = ind + 8;
					float spring = 500.0f;
					float damp = 5.0f;
					if (j < 7)
					{
						AddSpringForce(bodies[ind], zero, bodies[indr], zero, spring, damp, 1.0f);
						AddSpringForce(bodies[ind], right, bodies[indr], left, 0.5f * spring, damp, 0.0f);
					}
					if (i < 7)
					{
						AddSpringForce(bodies[ind], zero, bodies[indd], zero, spring, damp, 1.0f);
						AddSpringForce(bodies[ind], up, bodies[indd], down, 0.5f * spring, damp, 0.0f);
					}
					int inddr = indd + 1;
					int inddl = indd - 1;
					float drdist = (float)System.Math.Sqrt(2.0f);
					if (i < 7 && j < 7)
					{
						AddSpringForce(bodies[ind], zero, bodies[inddr], zero, spring, damp, drdist);
					}
					if (i < 7 && j > 0)
					{
						AddSpringForce(bodies[ind], zero, bodies[inddl], zero, spring, damp, drdist);
					}

					indr = ind + 2;
					indd = ind + 8 * 2;
					if (j < 6)
					{
						AddSpringForce(bodies[ind], zero, bodies[indr], zero, spring, damp, 2.0f);
					}
					if (i < 6)
					{
						AddSpringForce(bodies[ind], zero, bodies[indd], zero, spring, damp, 2.0f);
					}

					inddr = indd + 2;
					inddl = indd - 2;
					drdist = (float)System.Math.Sqrt(2.0f) * 2.0f;
					if (i < 6 && j < 6)
					{
						AddSpringForce(bodies[ind], zero, bodies[inddr], zero, spring, damp, drdist);
					}
					if (i < 6 && j > 1)
					{
						AddSpringForce(bodies[ind], zero, bodies[inddl], zero, spring, damp, drdist);
					}
				}
			}
			// Check if bodies are near elevator
			// Look if the body to lift is near the elevator
			Vec2 p1 = bodies[0].GetWorldCenter();
			Vec2 p2 = bodies[63].GetWorldCenter();
			// _elev:   elevator prism. joint
			Vec2 e = _elev.GetWorldCenter() + new Vec2(0.0f, 7.0f);
			// maybe not the best way to do it...
			// Bodies reached the elevator side 
			if (p1.X > e.X || p2.X > e.X)
			{
				// go up
				if ((p1.Y < e.Y || p2.Y < e.Y) &&
					 (_joint_elev.JointTranslation <= _joint_elev.LowerLimit + 1.0f))
				{
					_joint_elev.MotorSpeed = (20.0f);
					//printf("lift goes up trans: %G\n",_joint_elev.GetJointTranslation());
				}
			}
			// go down
			if ((_joint_elev.JointTranslation >= _joint_elev.UpperLimit - 2.0f))
			{
				_joint_elev.MotorSpeed = (-15.0f);
				//printf("lift goes down: %G\n",_joint_elev.GetJointTranslation());
			}
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <returns></returns>
		public static Test Create()
		{
			return new ElasticBody();
		}
	}
}