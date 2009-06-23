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

#define v1

using System;
using System.Collections.Generic;
using System.Text;

using Box2DX.Common;
using Box2DX.Collision;
using Box2DX.Dynamics;

namespace TestBed
{
	public class CCDTest : Test
	{
		public CCDTest()
		{
			const float k_restitution = 1.4f;

			{
				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, 20.0f);
				Body body = _world.CreateBody(bd);

				PolygonDef sd = new PolygonDef();
				sd.Density = 0.0f;
				sd.Restitution = k_restitution;

				sd.SetAsBox(0.1f, 10.0f, new Vec2(-10.0f, 0.0f), 0.0f);
				body.CreateShape(sd);

				sd.SetAsBox(0.1f, 10.0f, new Vec2(10.0f, 0.0f), 0.0f);
				body.CreateShape(sd);

				sd.SetAsBox(0.1f, 10.0f, new Vec2(0.0f, -10.0f), 0.5f * Box2DX.Common.Settings.Pi);
				body.CreateShape(sd);

				sd.SetAsBox(0.1f, 10.0f, new Vec2(0.0f, 10.0f), -0.5f * Box2DX.Common.Settings.Pi);
				body.CreateShape(sd);
			}

			{
				PolygonDef sd_bottom = new PolygonDef();
				sd_bottom.SetAsBox( 1.5f, 0.15f );
				sd_bottom.Density = 4.0f;

				PolygonDef sd_left = new PolygonDef();
				sd_left.SetAsBox(0.15f, 2.7f, new Vec2(-1.45f, 2.35f), 0.2f);
				sd_left.Density = 4.0f;

				PolygonDef sd_right = new PolygonDef();
				sd_right.SetAsBox(0.15f, 2.7f, new Vec2(1.45f, 2.35f), -0.2f);
				sd_right.Density = 4.0f;

				BodyDef bd = new BodyDef();
				bd.Position.Set( 0.0f, 15.0f );
				Body body = _world.CreateBody(bd);
				body.CreateShape(sd_bottom);
				body.CreateShape(sd_left);
				body.CreateShape(sd_right);
				body.SetMassFromShapes();
			}

			for (int i = 0; i < 0; ++i)
			{
				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, 15.0f + i);
				bd.IsBullet = true;
				Body body = _world.CreateBody(bd);
				body.SetAngularVelocity(Box2DX.Common.Math.Random(-50.0f, 50.0f));

				CircleDef sd = new CircleDef();
				sd.Radius = 0.25f;
				sd.Density = 1.0f;
				sd.Restitution = 0.0f;
				body.CreateShape(sd);
				body.SetMassFromShapes();
			}
		}

		public static Test Create()
		{
			return new CCDTest();
		}
	}
}