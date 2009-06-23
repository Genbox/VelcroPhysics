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
	// A line joint with a limit and friction.
	public class LineJoint : Test
	{
		public LineJoint()
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
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(0.5f, 2.0f);
				sd.Density = 1.0f;


				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, 7.0f);
				Body body = _world.CreateBody(bd);
				body.CreateShape(sd);
				body.SetMassFromShapes();

				LineJointDef jd = new LineJointDef();
				Vec2 axis = new Vec2(2.0f, 1.0f);
				axis.Normalize();
				jd.Initialize(ground, body, new Vec2(0.0f, 8.5f), axis);
				jd.motorSpeed = 0.0f;
				jd.maxMotorForce = 100.0f;
				jd.enableMotor = true;
				jd.lowerTranslation = -4.0f;
				jd.upperTranslation = 4.0f;
				jd.enableLimit = true;
				_world.CreateJoint(jd);
			}
		}

		public static Test Create()
		{
			return new LineJoint();
		}
	}
}