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
	public class VerticalStack : Test
	{
		Body _bullet;

		public VerticalStack()
		{
			{
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(50.0f, 10.0f, new Vec2(0.0f, -10.0f), 0.0f);

				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, 0.0f);
				Body ground = _world.CreateBody(bd);
				ground.CreateShape(sd);

				sd.SetAsBox(0.1f, 10.0f, new Vec2(20.0f, 10.0f), 0.0f);
				ground.CreateShape(sd);
			}

			float[] xs = new float[5] { 0.0f, -10.0f, -5.0f, 5.0f, 10.0f };

			for (int j = 0; j < 5; ++j)
			{
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(0.5f, 0.5f);
				sd.Density = 1.0f;
				sd.Friction = 0.3f;

				for (int i = 0; i < 12; ++i)
				{
					BodyDef bd = new BodyDef();

					// For this test we are using continuous physics for all boxes.
					// This is a stress test, you normally wouldn't do this for
					// performance reasons.
					bd.AllowSleep = true;
					bd.Position.Set(xs[j], 0.752f + 1.54f * i);
					Body body = _world.CreateBody(bd);

					body.CreateShape(sd);
					body.SetMassFromShapes();
				}
			}

			_bullet = null;
		}

		public static Test Create()
		{
			return new VerticalStack();
		}

		public override void Keyboard(System.Windows.Forms.Keys key)
		{
			switch (key)
			{
				case System.Windows.Forms.Keys.S:
					if (_bullet != null)
					{
						_world.DestroyBody(_bullet);
						_bullet = null;
					}
					{
						CircleDef sd = new CircleDef();
						sd.Density = 20.0f;
						sd.Radius = 0.25f;
						sd.Restitution = 0.05f;

						BodyDef bd = new BodyDef();
						bd.IsBullet = true;
						bd.AllowSleep = false;
						bd.Position.Set(-31.0f, 5.0f);

						_bullet = _world.CreateBody(bd);
						_bullet.CreateShape(sd);
						_bullet.SetMassFromShapes();

						_bullet.SetLinearVelocity(new Vec2(400.0f, 0.0f));
					}
					break;
			}
		}
	}
}
