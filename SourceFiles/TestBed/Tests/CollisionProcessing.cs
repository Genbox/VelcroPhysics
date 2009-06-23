/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.Google.Com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2008 Erin Catto http://www.Gphysics.Com

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
	// This test shows collision processing and tests
	// deferred body destruction.
	public class CollisionProcessing : Test
	{
		public CollisionProcessing()
		{
			// Ground body
			{
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(50.0f, 10.0f);
				sd.Friction = 0.3f;

				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, -10.0f);

				Body ground = _world.CreateBody(bd);
				ground.CreateShape(sd);
			}

			float xLo = -5.0f, xHi = 5.0f;
			float yLo = 2.0f, yHi = 35.0f;

			// Small triangle
			PolygonDef triangleShapeDef = new PolygonDef();
			triangleShapeDef.VertexCount = 3;
			triangleShapeDef.Vertices[0].Set(-1.0f, 0.0f);
			triangleShapeDef.Vertices[1].Set(1.0f, 0.0f);
			triangleShapeDef.Vertices[2].Set(0.0f, 2.0f);
			triangleShapeDef.Density = 1.0f;

			BodyDef triangleBodyDef = new BodyDef();
			//triangleBodyDef.Position.Set(Box2DX.Common.Math.Random(xLo, xHi), Box2DX.Common.Math.Random(yLo, yHi));
			triangleBodyDef.Position.Set(Box2DX.Common.Math.Random(xLo, xHi), 35f);

			Body body1 = _world.CreateBody(triangleBodyDef);
			body1.CreateShape(triangleShapeDef);
			body1.SetMassFromShapes();

			// Large triangle (recycle definitions)
			triangleShapeDef.Vertices[0] *= 2.0f;
			triangleShapeDef.Vertices[1] *= 2.0f;
			triangleShapeDef.Vertices[2] *= 2.0f;
			//triangleBodyDef.Position.Set(Box2DX.Common.Math.Random(xLo, xHi), Box2DX.Common.Math.Random(yLo, yHi));
			triangleBodyDef.Position.Set(Box2DX.Common.Math.Random(xLo, xHi), 30f);

			Body body2 = _world.CreateBody(triangleBodyDef);
			body2.CreateShape(triangleShapeDef);
			body2.SetMassFromShapes();

			// Small box
			PolygonDef boxShapeDef = new PolygonDef();
			boxShapeDef.SetAsBox(1.0f, 0.5f);
			boxShapeDef.Density = 1.0f;

			BodyDef boxBodyDef = new BodyDef();
			//boxBodyDef.Position.Set(Box2DX.Common.Math.Random(xLo, xHi), Box2DX.Common.Math.Random(yLo, yHi));
			boxBodyDef.Position.Set(Box2DX.Common.Math.Random(xLo, xHi), 25f);

			Body body3 = _world.CreateBody(boxBodyDef);
			body3.CreateShape(boxShapeDef);
			body3.SetMassFromShapes();

			// Large box (recycle definitions)
			boxShapeDef.SetAsBox(2.0f, 1.0f);
			//boxBodyDef.Position.Set(Box2DX.Common.Math.Random(xLo, xHi), Box2DX.Common.Math.Random(yLo, yHi));
			boxBodyDef.Position.Set(Box2DX.Common.Math.Random(xLo, xHi), 15f);

			Body body4 = _world.CreateBody(boxBodyDef);
			body4.CreateShape(boxShapeDef);
			body4.SetMassFromShapes();

			// Small circle
			CircleDef circleShapeDef = new CircleDef();
			circleShapeDef.Radius = 1.0f;
			circleShapeDef.Density = 1.0f;

			BodyDef circleBodyDef = new BodyDef();
			//circleBodyDef.Position.Set(Box2DX.Common.Math.Random(xLo, xHi), Box2DX.Common.Math.Random(yLo, yHi));
			circleBodyDef.Position.Set(Box2DX.Common.Math.Random(xLo, xHi), 10f);

			Body body5 = _world.CreateBody(circleBodyDef);
			body5.CreateShape(circleShapeDef);
			body5.SetMassFromShapes();

			// Large circle
			circleShapeDef.Radius *= 2.0f;
			//circleBodyDef.Position.Set(Box2DX.Common.Math.Random(xLo, xHi), Box2DX.Common.Math.Random(yLo, yHi));
			circleBodyDef.Position.Set(Box2DX.Common.Math.Random(xLo, xHi), 5f);

			Body body6 = _world.CreateBody(circleBodyDef);
			body6.CreateShape(circleShapeDef);
			body6.SetMassFromShapes();
		}

		public override void Step(Settings settings)
		{
			base.Step(settings);
			// We are going to destroy some bodies according to contact
			// points. We must buffer the bodies that should be destroyed
			// because they may belong to multiple contact points.
			const int k_maxNuke = 6;
			Dictionary<int, Body> nuke = new Dictionary<int, Body>(k_maxNuke);
			int nukeCount = 0;

			// Traverse the contact results. Destroy bodies that
			// are touching heavier bodies.
			for (int i = 0; i < _pointCount; ++i)
			{
				MyContactPoint point = _points[i];

				Body body1 = point.shape1.GetBody();
				Body body2 = point.shape2.GetBody();
				float mass1 = body1.GetMass();
				float mass2 = body2.GetMass();

				if (mass1 > 0.0f && mass2 > 0.0f)
				{
					if (mass2 > mass1)
					{
						int hc = body1.GetHashCode();
						if (!nuke.ContainsKey(hc))
							nuke.Add(hc, body1);//nuke[nukeCount++] = body1;
					}
					else
					{
						int hc = body2.GetHashCode();
						if (!nuke.ContainsKey(hc))
							nuke.Add(hc, body2);//nuke[nukeCount++] = body2;
					}

					if (nukeCount == k_maxNuke)
					{
						break;
					}
				}
			}

			// Sort the nuke array to group duplicates.
			//Array.Sort(nuke);

			// Destroy the bodies, skipping duplicates.
			/*int i_ = 0;
			while (i_ < nukeCount)
			{
				Body b = nuke[i_++];
				while (i_ < nukeCount && nuke[i_] == b)
				{
					++i_;
				}

				_world.DestroyBody(b);
			}*/

			foreach (KeyValuePair<int, Body> kvp in nuke)
				_world.DestroyBody(nuke[kvp.Key]);
		}

		public static Test Create()
		{
			return new CollisionProcessing();
		}
	}
}