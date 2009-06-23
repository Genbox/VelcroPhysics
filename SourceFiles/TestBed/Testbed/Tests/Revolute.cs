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
	public class Revolute : Test
	{
		private RevoluteJoint _joint;

		public Revolute()
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
				CircleDef sd = new CircleDef();
				sd.Radius = 0.5f;
				sd.Density = 5.0f;

				BodyDef bd = new BodyDef();

				RevoluteJointDef rjd = new RevoluteJointDef();

				bd.Position.Set(0.0f, 20.0f);
				Body body = _world.CreateBody(bd);
				body.CreateShape(sd);
				body.SetMassFromShapes();

				float w = 100.0f;
				body.SetAngularVelocity(w);
				body.SetLinearVelocity(new Vec2(-8.0f * w, 0.0f));

				rjd.Initialize(ground, body, new Vec2(0.0f, 12.0f));
				rjd.MotorSpeed = 1.0f * Box2DX.Common.Settings.Pi;
				rjd.MaxMotorTorque = 10000.0f;
				rjd.EnableMotor = false;
				rjd.LowerAngle = -0.25f * Box2DX.Common.Settings.Pi;
				rjd.UpperAngle = 0.5f * Box2DX.Common.Settings.Pi;
				rjd.EnableLimit = true;
				rjd.CollideConnected = true;

				_joint = (RevoluteJoint)_world.CreateJoint(rjd);
			}
		}

		public override void Step(Settings settings)
		{
			base.Step(settings);
			OpenGLDebugDraw.DrawString(5, _textLine, "Keys: (l) limits, (s) motor");
			_textLine += 15;

		}

		public override void Keyboard(System.Windows.Forms.Keys key)
		{
			switch (key)
			{
				case System.Windows.Forms.Keys.L:
					_joint.EnableLimit(!_joint.IsLimitEnabled);
					break;
				case System.Windows.Forms.Keys.S:
					_joint.EnableMotor(!_joint.IsMotorEnabled);
					break;
				default:
					return;
			}
		}

		public static Test Create()
		{
			return new Revolute();
		}
	}
}