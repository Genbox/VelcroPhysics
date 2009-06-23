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
	// A motor driven slider crank with joint friction.
	public class SliderCrank : Test
	{
		public SliderCrank()
		{
			Body ground = null;
			{
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(50.0f, 10.0f);

				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, -10.0f);
				ground = _world.CreateBody(bd);
				ground.CreateShape(sd);
			}

			{
				// Define crank.
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(0.5f, 2.0f);
				sd.Density = 1.0f;

				RevoluteJointDef rjd = new RevoluteJointDef();

				Body prevBody = ground;

				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, 7.0f);
				Body body = _world.CreateBody(bd);
				body.CreateShape(sd);
				body.SetMassFromShapes();

				rjd.Initialize(prevBody, body, new Vec2(0.0f, 5.0f));
				rjd.MotorSpeed = 1.0f * Box2DX.Common.Settings.Pi;
				rjd.MaxMotorTorque = 10000.0f;
				rjd.EnableMotor = true;
				_joint1 = (RevoluteJoint)_world.CreateJoint(rjd);

				prevBody = body;

				// Define follower.
				sd.SetAsBox(0.5f, 4.0f);
				bd.Position.Set(0.0f, 13.0f);
				body = _world.CreateBody(bd);
				body.CreateShape(sd);
				body.SetMassFromShapes();

				rjd.Initialize(prevBody, body, new Vec2(0.0f, 9.0f));
				rjd.EnableMotor = false;
				_world.CreateJoint(rjd);

				prevBody = body;

				// Define piston
				sd.SetAsBox(1.5f, 1.5f);
				bd.Position.Set(0.0f, 17.0f);
				body = _world.CreateBody(bd);
				body.CreateShape(sd);
				body.SetMassFromShapes();

				rjd.Initialize(prevBody, body, new Vec2(0.0f, 17.0f));
				_world.CreateJoint(rjd);

				PrismaticJointDef pjd = new PrismaticJointDef();
				pjd.Initialize(ground, body, new Vec2(0.0f, 17.0f), new Vec2(0.0f, 1.0f));

				pjd.MaxMotorForce = 1000.0f;
				pjd.EnableMotor = true;

				_joint2 = (PrismaticJoint)_world.CreateJoint(pjd);

				// Create a payload
				sd.Density = 2.0f;
				bd.Position.Set(0.0f, 23.0f);
				body = _world.CreateBody(bd);
				body.CreateShape(sd);
				body.SetMassFromShapes();
			}
		}

		public override void Keyboard(System.Windows.Forms.Keys key)
		{
			switch (key)
			{
				case System.Windows.Forms.Keys.F:
					_joint2._enableMotor = !_joint2._enableMotor;
					_joint2.GetBody2().WakeUp();
					break;

				case System.Windows.Forms.Keys.M:
					_joint1._enableMotor = !_joint1._enableMotor;
					_joint1.GetBody2().WakeUp();
					break;
			}
		}

		public override void Step(Settings settings)
		{
			base.Step(settings);
			OpenGLDebugDraw.DrawString(5, _textLine, "Keys: (f) toggle friction, (m) toggle motor");
			_textLine += 15;
			float torque = _joint1.MotorTorque;
			StringBuilder strBld = new StringBuilder();
			strBld.AppendFormat("Motor Torque = {0}",
				new object[] { torque });
			OpenGLDebugDraw.DrawString(5, _textLine, strBld.ToString());
			_textLine += 15;
		}

		public static Test Create()
		{
			return new SliderCrank();
		}

		RevoluteJoint _joint1;
		PrismaticJoint _joint2;
	}
}