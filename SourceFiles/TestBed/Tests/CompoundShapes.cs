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
	public class CompoundShapes : Test
	{
		public CompoundShapes()
		{
			{
				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, -10.0f);
				Body body = _world.CreateBody(bd);

				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(50.0f, 10.0f);
				body.CreateShape(sd);
			}

			{
				CircleDef sd1 = new CircleDef();
				sd1.Radius = 0.5f;
				sd1.LocalPosition.Set(-0.5f, 0.5f);
				sd1.Density = 2.0f;

				CircleDef sd2 = new CircleDef();
				sd2.Radius = 0.5f;
				sd2.LocalPosition.Set(0.5f, 0.5f);
				sd2.Density = 0.0f; // massless

				for (int i = 0; i < 10; ++i)
				{
					float x = Box2DX.Common.Math.Random(-0.1f, 0.1f);
					BodyDef bd = new BodyDef();
					bd.Position.Set(x + 5.0f, 1.05f + 2.5f * i);
					bd.Angle = Box2DX.Common.Math.Random(-Box2DX.Common.Settings.Pi, Box2DX.Common.Settings.Pi);
					Body body = _world.CreateBody(bd);
					body.CreateShape(sd1);
					body.CreateShape(sd2);
					body.SetMassFromShapes();
				}
			}

			{
				PolygonDef sd1 = new PolygonDef();
				sd1.SetAsBox(0.25f, 0.5f);
				sd1.Density = 2.0f;

				PolygonDef sd2 = new PolygonDef();
				sd2.SetAsBox(0.25f, 0.5f, new Vec2(0.0f, -0.5f), 0.5f * Box2DX.Common.Settings.Pi);
				sd2.Density = 2.0f;

				for (int i = 0; i < 10; ++i)
				{
					float x = Box2DX.Common.Math.Random(-0.1f, 0.1f);
					BodyDef bd = new BodyDef();
					bd.Position.Set(x - 5.0f, 1.05f + 2.5f * i);
					bd.Angle = Box2DX.Common.Math.Random(-Box2DX.Common.Settings.Pi, Box2DX.Common.Settings.Pi);
					Body body = _world.CreateBody(bd);
					body.CreateShape(sd1);
					body.CreateShape(sd2);
					body.SetMassFromShapes();
				}
			}

			{
				XForm xf1 = new XForm();
				xf1.R.Set(0.3524f * Box2DX.Common.Settings.Pi);
				xf1.Position = Box2DX.Common.Math.Mul(xf1.R, new Vec2(1.0f, 0.0f));

				PolygonDef sd1 = new PolygonDef();
				sd1.VertexCount = 3;
				sd1.Vertices[0] = Box2DX.Common.Math.Mul(xf1, new Vec2(-1.0f, 0.0f));
				sd1.Vertices[1] = Box2DX.Common.Math.Mul(xf1, new Vec2(1.0f, 0.0f));
				sd1.Vertices[2] = Box2DX.Common.Math.Mul(xf1, new Vec2(0.0f, 0.5f));
				sd1.Density = 2.0f;

				XForm xf2 = new XForm();
				xf2.R.Set(-0.3524f * Box2DX.Common.Settings.Pi);
				xf2.Position = Box2DX.Common.Math.Mul(xf2.R, new Vec2(-1.0f, 0.0f));

				PolygonDef sd2 = new PolygonDef();
				sd2.VertexCount = 3;
				sd2.Vertices[0] = Box2DX.Common.Math.Mul(xf2, new Vec2(-1.0f, 0.0f));
				sd2.Vertices[1] = Box2DX.Common.Math.Mul(xf2, new Vec2(1.0f, 0.0f));
				sd2.Vertices[2] = Box2DX.Common.Math.Mul(xf2, new Vec2(0.0f, 0.5f));
				sd2.Density = 2.0f;

				for (int i = 0; i < 10; ++i)
				{
					float x = Box2DX.Common.Math.Random(-0.1f, 0.1f);
					BodyDef bd = new BodyDef();
					bd.Position.Set(x, 2.05f + 2.5f * i);
					bd.Angle = 0.0f;
					Body body = _world.CreateBody(bd);
					body.CreateShape(sd1);
					body.CreateShape(sd2);
					body.SetMassFromShapes();
				}
			}

			{
				PolygonDef sd_bottom = new PolygonDef();
				sd_bottom.SetAsBox(1.5f, 0.15f);
				sd_bottom.Density = 4.0f;

				PolygonDef sd_left = new PolygonDef();
				sd_left.SetAsBox(0.15f, 2.7f, new Vec2(-1.45f, 2.35f), 0.2f);
				sd_left.Density = 4.0f;

				PolygonDef sd_right = new PolygonDef();
				sd_right.SetAsBox(0.15f, 2.7f, new Vec2(1.45f, 2.35f), -0.2f);
				sd_right.Density = 4.0f;

				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, 2.0f);
				Body body = _world.CreateBody(bd);
				body.CreateShape(sd_bottom);
				body.CreateShape(sd_left);
				body.CreateShape(sd_right);
				body.SetMassFromShapes();
			}
		}

		public static Test Create()
		{
			return new CompoundShapes();
		}
	}
}