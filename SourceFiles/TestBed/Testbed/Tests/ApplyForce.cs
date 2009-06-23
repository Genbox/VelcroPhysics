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
	public class ApplyForce : Test
	{
		Body _body;

		public ApplyForce()
		{
			_world.Gravity = new Vec2(0.0f, 0.0f);

			const float k_restitution = 0.4f;

			{
				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, 20.0f);
				Body ground = _world.CreateBody(bd);

				PolygonDef sd = new PolygonDef();
				sd.Density = 0.0f;
				sd.Restitution = k_restitution;

				sd.SetAsBox(0.2f, 20.0f, new Vec2(-20.0f, 0.0f), 0.0f);
				ground.CreateShape(sd);

				sd.SetAsBox(0.2f, 20.0f, new Vec2(20.0f, 0.0f), 0.0f);
				ground.CreateShape(sd);

				sd.SetAsBox(0.2f, 20.0f, new Vec2(0.0f, -20.0f), 0.5f * Box2DX.Common.Settings.Pi);
				ground.CreateShape(sd);

				sd.SetAsBox(0.2f, 20.0f, new Vec2(0.0f, 20.0f), -0.5f * Box2DX.Common.Settings.Pi);
				ground.CreateShape(sd);
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

				BodyDef bd = new BodyDef();
				bd.AngularDamping = 2.0f;
				bd.LinearDamping = 0.1f;

				bd.Position.Set(0.0f, 1.05f);
				bd.Angle = Box2DX.Common.Settings.Pi;
				_body = _world.CreateBody(bd);
				_body.CreateShape(sd1);
				_body.CreateShape(sd2);
				_body.SetMassFromShapes();
			}
		}

		public override void Keyboard(System.Windows.Forms.Keys key)
		{
			switch (key)
			{
				case System.Windows.Forms.Keys.W:
					{
						Vec2 f = _body.GetWorldVector(new Vec2(0.0f, -200.0f));
						Vec2 p = _body.GetWorldPoint(new Vec2(0.0f, 2.0f));
						_body.ApplyForce(f, p);
					}
					break;

				case System.Windows.Forms.Keys.A:
					{
						_body.ApplyTorque(20.0f);
					}
					break;

				case System.Windows.Forms.Keys.D:
					{
						_body.ApplyTorque(-20.0f);
					}
					break;
			}
		}

		public static Test Create()
		{
			return new ApplyForce();
		}
	}
}