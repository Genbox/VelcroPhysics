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
	public class SensorTest : Test
	{
		Shape _sensor;

		public SensorTest()
		{
			{
				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, -10.0f);

				Body ground = _world.CreateBody(bd);

				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(50.0f, 10.0f);
				ground.CreateShape(sd);

				CircleDef cd = new CircleDef();
				cd.IsSensor = true;
				cd.Radius = 5.0f;
				cd.LocalPosition.Set(0.0f, 20.0f);
				_sensor = ground.CreateShape(cd);
			}

			{
				CircleDef sd = new CircleDef();
				sd.Radius = 1.0f;
				sd.Density = 1.0f;

				for (int i = 0; i < 7; ++i)
				{
					BodyDef bd = new BodyDef();
					bd.Position.Set(-10.0f + 3.0f * i, 20.0f);

					Body body = _world.CreateBody(bd);

					body.CreateShape(sd);
					body.SetMassFromShapes();
				}
			}
		}

		public override void Step(Settings settings)
		{
			base.Step(settings);
			// Traverse the contact results. Apply a force on shapes
			// that overlap the sensor.
			for (int i = 0; i < _pointCount; ++i)
			{
				MyContactPoint point = _points[i];

				if ((int)point.state == 2)
				{
					continue;
				}

				Shape shape1 = point.shape1;
				Shape shape2 = point.shape2;
				Body other;

				if (shape1 == _sensor)
				{
					other = shape2.GetBody();
				}
				else if (shape2 == _sensor)
				{
					other = shape1.GetBody();
				}
				else
				{
					continue;
				}

				Body ground = _sensor.GetBody();

				CircleShape circle = (CircleShape)_sensor;
				Vec2 center = ground.GetWorldPoint(circle.GetLocalPosition());

				Vec2 d = center - point.position;
				if (d.LengthSquared() < Box2DX.Common.Settings.FLT_EPSILON * Box2DX.Common.Settings.FLT_EPSILON)
				{
					continue;
				}

				d.Normalize();
				Vec2 F = 100.0f * d;
				other.ApplyForce(F, point.position);
			}
		}

		public static Test Create()
		{
			return new SensorTest();
		}
	}
}