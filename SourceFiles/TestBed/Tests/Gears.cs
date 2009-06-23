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

namespace TestBed
{
	public class Gears : Test
	{
		RevoluteJoint _joint1;
		RevoluteJoint _joint2;
		PrismaticJoint _joint3;
		GearJoint _joint4;
		GearJoint _joint5;

		public Gears()
		{
			Body ground = null;
			{
				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, -10.0f);
				ground = _world.CreateBody(bd);

				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(50.0f, 10.0f);
				ground.CreateShape(sd);
			}

			{
				CircleDef circle1 = new CircleDef();
				circle1.Radius = 1.0f;
				circle1.Density = 5.0f;

				CircleDef circle2 = new CircleDef();
				circle2.Radius = 2.0f;
				circle2.Density = 5.0f;

				PolygonDef box = new PolygonDef();
				box.SetAsBox(0.5f, 5.0f);
				box.Density = 5.0f;

				BodyDef bd1 = new BodyDef();
				bd1.Position.Set(-3.0f, 12.0f);
				Body body1 = _world.CreateBody(bd1);
				body1.CreateShape(circle1);
				body1.SetMassFromShapes();

				RevoluteJointDef jd1 = new RevoluteJointDef();
				jd1.Body1 = ground;
				jd1.Body2 = body1;
				jd1.LocalAnchor1 = ground.GetLocalPoint(bd1.Position);
				jd1.LocalAnchor2 = body1.GetLocalPoint(bd1.Position);
				jd1.ReferenceAngle = body1.GetAngle() - ground.GetAngle();
				_joint1 = (RevoluteJoint)_world.CreateJoint(jd1);

				BodyDef bd2 = new BodyDef();
				bd2.Position.Set(0.0f, 12.0f);
				Body body2 = _world.CreateBody(bd2);
				body2.CreateShape(circle2);
				body2.SetMassFromShapes();

				RevoluteJointDef jd2 = new RevoluteJointDef();
				jd2.Initialize(ground, body2, bd2.Position);
				_joint2 = (RevoluteJoint)_world.CreateJoint(jd2);

				BodyDef bd3 = new BodyDef();
				bd3.Position.Set(2.5f, 12.0f);
				Body body3 = _world.CreateBody(bd3);
				body3.CreateShape(box);
				body3.SetMassFromShapes();

				PrismaticJointDef jd3 = new PrismaticJointDef();
				jd3.Initialize(ground, body3, bd3.Position, new Vec2(0.0f, 1.0f));
				jd3.LowerTranslation = -5.0f;
				jd3.UpperTranslation = 5.0f;
				jd3.EnableLimit = true;

				_joint3 = (PrismaticJoint)_world.CreateJoint(jd3);

				GearJointDef jd4 = new GearJointDef();
				jd4.Body1 = body1;
				jd4.Body2 = body2;
				jd4.Joint1 = _joint1;
				jd4.Joint2 = _joint2;
				jd4.Ratio = circle2.Radius / circle1.Radius;
				_joint4 = (GearJoint)_world.CreateJoint(jd4);

				GearJointDef jd5 = new GearJointDef();
				jd5.Body1 = body2;
				jd5.Body2 = body3;
				jd5.Joint1 = _joint2;
				jd5.Joint2 = _joint3;
				jd5.Ratio = -1.0f / circle2.Radius;
				_joint5 = (GearJoint)_world.CreateJoint(jd5);
			}
		}

		public static Test Create()
		{
			return new Gears();
		}

		public override void Step(Settings settings)
		{
			base.Step(settings);

			float ratio, value;

			ratio = _joint4.Ratio;
			value = _joint1.JointAngle + ratio * _joint2.JointAngle;
			StringBuilder strBld = new StringBuilder();
			strBld.AppendFormat("theta1 + {0} * theta2 = {1}", new object[]{ratio,value});
			OpenGLDebugDraw.DrawString(5, _textLine, strBld.ToString());
			_textLine += 15;

			ratio = _joint5.Ratio;
			value = _joint2.JointAngle + ratio * _joint3.JointTranslation;
			strBld = new StringBuilder();
			strBld.AppendFormat("theta2 + {0} * delta = {1}", new object[] { ratio, value });
			OpenGLDebugDraw.DrawString(5, _textLine, strBld.ToString());
			_textLine += 15;
		}
	}
}
