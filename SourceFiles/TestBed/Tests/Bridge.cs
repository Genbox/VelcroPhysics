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
	public class Bridge : Test
	{
		public Bridge()
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
				sd.SetAsBox(0.5f, 0.125f);
				sd.Density = 20.0f;
				sd.Friction = 0.2f;

				RevoluteJointDef jd = new RevoluteJointDef();
				const int numPlanks = 30;

				Body prevBody = ground;
				for (int i = 0; i < numPlanks; ++i)
				{
					BodyDef bd = new BodyDef();
					bd.Position.Set(-14.5f + 1.0f * i, 5.0f);
					Body body = _world.CreateBody(bd);
					body.CreateShape(sd);
					body.SetMassFromShapes();

					Vec2 anchor = new Vec2(-15.0f + 1.0f * i, 5.0f);
					jd.Initialize(prevBody, body, anchor);
					_world.CreateJoint(jd);

					prevBody = body;
				}

				Vec2 anchor_ = new Vec2(-15.0f + 1.0f * numPlanks, 5.0f);
				jd.Initialize(prevBody, ground, anchor_);
				_world.CreateJoint(jd);
			}
		}

		public static Test Create()
		{
			return new Bridge();
		}
	}
}
