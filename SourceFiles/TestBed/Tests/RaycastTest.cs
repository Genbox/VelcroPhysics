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
	public class RaycastTest : Test
	{
		Body laserBody;

		public RaycastTest()
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
				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, 1.0f);
				laserBody = _world.CreateBody(bd);

				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(5.0f, 1.0f);
				sd.Density = 4.0f;
				laserBody.CreateShape(sd);
				laserBody.SetMassFromShapes();

				Body body;
				//Create a few shapes
				bd.Position.Set(-5.0f, 10.0f);
				body = _world.CreateBody(bd);

				CircleDef cd = new CircleDef();
				cd.Radius = 3;
				body.CreateShape(cd);

				bd.Position.Set(5.0f, 10.0f);
				body = _world.CreateBody(bd);

				body.CreateShape(cd);
			}
		}

		public override void Keyboard(System.Windows.Forms.Keys key)
		{
			base.Keyboard(key);
		}

		public override void Step(Settings settings)
		{
			base.Step(settings);

			float segmentLength = 30.0f;

			Segment segment;
			Vec2 laserStart = new Vec2(5.0f - 0.1f, 0.0f);
			Vec2 laserDir = new Vec2(segmentLength, 0.0f);
			segment.P1 = laserBody.GetWorldPoint(laserStart);
			segment.P2 = laserBody.GetWorldVector(laserDir);
			segment.P2 += segment.P1;

			for (int rebounds = 0; rebounds < 10; rebounds++)
			{

				float lambda = 1;
				Vec2 normal;
				Shape shape = _world.RaycastOne(segment, out lambda, out normal, false, null);

				Color laserColor = new Color(255, 0, 0);

				if (shape != null)
				{
					_debugDraw.DrawSegment(segment.P1, (1 - lambda) * segment.P1 + lambda * segment.P2, laserColor);
				}
				else
				{
					_debugDraw.DrawSegment(segment.P1, segment.P2, laserColor);
					break;
				}
				//Bounce
				segmentLength *= (1 - lambda);
				if (segmentLength <= Box2DX.Common.Settings.FLT_EPSILON)
					break;
				laserStart = (1 - lambda) * segment.P1 + lambda * segment.P2;
				laserDir = segment.P2 - segment.P1;
				laserDir.Normalize();
				laserDir = laserDir - 2 * Vec2.Dot(laserDir, normal) * normal;
				segment.P1 = laserStart - 0.1f * laserDir;
				segment.P2 = laserStart + segmentLength * laserDir;
			}
		}

		public static Test Create()
		{
			return new RaycastTest();
		}
	}
}