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

// Inspired by a contribution by roman_m
// Dimensions scooped from APE (http://www.cove.org/ape/index.htm)

using System;
using System.Collections.Generic;
using System.Text;

using Box2DX.Common;
using Box2DX.Collision;
using Box2DX.Dynamics;

namespace TestBed
{
	public class TheoJansen : Test
	{
		Vec2 _offset;
		Body _chassis;
		Body _wheel;
		RevoluteJoint _motorJoint;
		bool _motorOn;
		float _motorSpeed;

		public TheoJansen()
		{
			_offset.Set(0.0f, 8.0f);
			_motorSpeed = 2.0f;
			_motorOn = true;
			Vec2 pivot = new Vec2(0.0f, 0.8f);

			{
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(50.0f, 10.0f);

				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, -10.0f);
				Body ground = _world.CreateBody(bd);
				ground.CreateShape(sd);

				sd.SetAsBox(0.5f, 5.0f, new Vec2(-50.0f, 15.0f), 0.0f);
				ground.CreateShape(sd);

				sd.SetAsBox(0.5f, 5.0f, new Vec2(50.0f, 15.0f), 0.0f);
				ground.CreateShape(sd);
			}

			for (int i = 0; i < 40; ++i)
			{
				CircleDef sd = new CircleDef();
				sd.Density = 1.0f;
				sd.Radius = 0.25f;

				BodyDef bd = new BodyDef();
				bd.Position.Set(-40.0f + 2.0f * i, 0.5f);

				Body body = _world.CreateBody(bd);
				body.CreateShape(sd);
				body.SetMassFromShapes();
			}

			{
				PolygonDef sd = new PolygonDef();
				sd.Density = 1.0f;
				sd.SetAsBox(2.5f, 1.0f);
				sd.Filter.GroupIndex = -1;
				BodyDef bd = new BodyDef();
				bd.Position = pivot + _offset;
				_chassis = _world.CreateBody(bd);
				_chassis.CreateShape(sd);
				_chassis.SetMassFromShapes();
			}

			{
				CircleDef sd = new CircleDef();
				sd.Density = 1.0f;
				sd.Radius = 1.6f;
				sd.Filter.GroupIndex = -1;
				BodyDef bd = new BodyDef();
				bd.Position = pivot + _offset;
				_wheel = _world.CreateBody(bd);
				_wheel.CreateShape(sd);
				_wheel.SetMassFromShapes();
			}

			{
				RevoluteJointDef jd = new RevoluteJointDef();
				jd.Initialize(_wheel, _chassis, pivot + _offset);
				jd.CollideConnected = false;
				jd.MotorSpeed = _motorSpeed;
				jd.MaxMotorTorque = 400.0f;
				jd.EnableMotor = _motorOn;
				_motorJoint = (RevoluteJoint)_world.CreateJoint(jd);
			}

			Vec2 wheelAnchor;
			
			wheelAnchor = pivot + new Vec2(0.0f, -0.8f);

			CreateLeg(-1.0f, wheelAnchor);
			CreateLeg(1.0f, wheelAnchor);

			_wheel.SetXForm(_wheel.GetPosition(), 120.0f * Box2DX.Common.Settings.Pi / 180.0f);
			CreateLeg(-1.0f, wheelAnchor);
			CreateLeg(1.0f, wheelAnchor);

			_wheel.SetXForm(_wheel.GetPosition(), -120.0f * Box2DX.Common.Settings.Pi / 180.0f);
			CreateLeg(-1.0f, wheelAnchor);
			CreateLeg(1.0f, wheelAnchor);
		}

		public void CreateLeg(float s, Vec2 wheelAnchor)
		{
			Vec2 p1 = new Vec2(5.4f * s, -6.1f);
			Vec2 p2 = new Vec2(7.2f * s, -1.2f);
			Vec2 p3 = new Vec2(4.3f * s, -1.9f);
			Vec2 p4 = new Vec2(3.1f * s, 0.8f);
			Vec2 p5 = new Vec2(6.0f * s, 1.5f);
			Vec2 p6 = new Vec2(2.5f * s, 3.7f);

			PolygonDef sd1 = new PolygonDef(), sd2 = new PolygonDef();
			sd1.VertexCount = 3;
			sd2.VertexCount = 3;
			sd1.Filter.GroupIndex = -1;
			sd2.Filter.GroupIndex = -1;
			sd1.Density = 1.0f;
			sd2.Density = 1.0f;

			if (s > 0.0f)
			{
				sd1.Vertices[0] = p1;
				sd1.Vertices[1] = p2;
				sd1.Vertices[2] = p3;

				sd2.Vertices[0] = Vec2.Zero;
				sd2.Vertices[1] = p5 - p4;
				sd2.Vertices[2] = p6 - p4;
			}
			else
			{
				sd1.Vertices[0] = p1;
				sd1.Vertices[1] = p3;
				sd1.Vertices[2] = p2;

				sd2.Vertices[0] = Vec2.Zero;
				sd2.Vertices[1] = p6 - p4;
				sd2.Vertices[2] = p5 - p4;
			}

			BodyDef bd1 = new BodyDef(), bd2 = new BodyDef();
			bd1.Position = _offset;
			bd2.Position = p4 + _offset;

			bd1.AngularDamping = 10.0f;
			bd2.AngularDamping = 10.0f;

			Body body1 = _world.CreateBody(bd1);
			Body body2 = _world.CreateBody(bd2);

			body1.CreateShape(sd1);
			body2.CreateShape(sd2);

			body1.SetMassFromShapes();
			body2.SetMassFromShapes();

			DistanceJointDef djd = new DistanceJointDef();

			// Using a soft distance constraint can reduce some jitter.
			// It also makes the structure seem a bit more fluid by
			// acting like a suspension system.
			djd.DampingRatio = 0.5f;
			djd.FrequencyHz = 10.0f;

			djd.Initialize(body1, body2, p2 + _offset, p5 + _offset);
			_world.CreateJoint(djd);

			djd.Initialize(body1, body2, p3 + _offset, p4 + _offset);
			_world.CreateJoint(djd);

			djd.Initialize(body1, _wheel, p3 + _offset, wheelAnchor + _offset);
			_world.CreateJoint(djd);

			djd.Initialize(body2, _wheel, p6 + _offset, wheelAnchor + _offset);
			_world.CreateJoint(djd);

			RevoluteJointDef rjd = new RevoluteJointDef();

			rjd.Initialize(body2, _chassis, p4 + _offset);
			_world.CreateJoint(rjd);
		}

		public override void Step(Settings settings)
		{
			OpenGLDebugDraw.DrawString(5, _textLine, "Keys: left = a, brake = s, right = d, toggle motor = m");
			_textLine += 15;

			base.Step(settings);
		}

		public override void Keyboard(System.Windows.Forms.Keys key)
		{
			switch (key)
			{
				case System.Windows.Forms.Keys.A:
					_chassis.WakeUp();
					_motorJoint.MotorSpeed = -_motorSpeed;
					break;

				case System.Windows.Forms.Keys.S:
					_chassis.WakeUp();
					_motorJoint.MotorSpeed = (0.0f);
					break;

				case System.Windows.Forms.Keys.D:
					_chassis.WakeUp();
					_motorJoint.MotorSpeed = (_motorSpeed);
					break;

				case System.Windows.Forms.Keys.M:
					_chassis.WakeUp();
					_motorJoint.EnableMotor(!_motorJoint.IsMotorEnabled);
					break;
			}
		}

		public static Test Create()
		{
			return new TheoJansen();
		}
	}
}