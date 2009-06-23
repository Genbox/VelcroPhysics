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
	public class PolyCollision : Test
	{
		MyContactPoint[] _localPoints = new MyContactPoint[2];

		Body _body1;
		Body _body2;

		public PolyCollision()
		{
			_localPoints[0].state = ContactState.ContactRemoved;
			_localPoints[1].state = ContactState.ContactRemoved;

			{
				PolygonDef sd = new PolygonDef();
				sd.Vertices[0].Set(-9.0f, -1.1f);
				sd.Vertices[1].Set(7.0f, -1.1f);
				sd.Vertices[2].Set(5.0f, -0.9f);
				sd.Vertices[3].Set(-11.0f, -0.9f);
				sd.VertexCount = 4;
				sd.Density = 0.0f;

				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, 10.0f);
				_body1 = _world.CreateBody(bd);
				_body1.CreateShape(sd);
			}

			{
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(0.5f, 0.5f);
				sd.Density = 1.0f;

				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, 10.0f);
				_body2 = _world.CreateBody(bd);
				_body2.CreateShape(sd);
				_body2.SetMassFromShapes();
			}

			_world.Gravity = Vec2.Zero;
		}

		public override void Step(Settings settings)
		{
			settings.pause = 1;
			base.Step(settings);
			settings.pause = 0;
		}

		public override void Keyboard(System.Windows.Forms.Keys key)
		{
			Vec2 p = _body2.GetPosition();
			float a = _body2.GetAngle();

			switch (key)
			{
				case System.Windows.Forms.Keys.A:
					p.X -= 0.1f;
					break;

				case System.Windows.Forms.Keys.D:
					p.X += 0.1f;
					break;

				case System.Windows.Forms.Keys.S:
					p.Y -= 0.1f;
					break;

				case System.Windows.Forms.Keys.W:
					p.Y += 0.1f;
					break;

				case System.Windows.Forms.Keys.Q:
					a += 0.1f * Box2DX.Common.Settings.Pi;
					break;

				case System.Windows.Forms.Keys.E:
					a -= 0.1f * Box2DX.Common.Settings.Pi;
					break;
			}

			_body2.SetXForm(p, a);
		}

		public static Test Create()
		{
			return new PolyCollision();
		}
	}
}