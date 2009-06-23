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
	public class DistanceTest : Test
	{
		Body _body1;
		Body _body2;
		Shape _shape1;
		Shape _shape2;

		public DistanceTest()
		{
			PolygonDef sd = new PolygonDef();
			sd.SetAsBox(1.0f, 1.0f);
			sd.Density = 0.0f;

			BodyDef bd = new BodyDef();
			bd.Position.Set(0.0f, 10.0f);
			_body1 = _world.CreateBody(bd);
			_shape1 = _body1.CreateShape(sd);

			PolygonDef sd2 = new PolygonDef();
			sd2.VertexCount = 3;
			sd2.Vertices[0].Set(-1.0f, 0.0f);
			sd2.Vertices[1].Set(1.0f, 0.0f);
			sd2.Vertices[2].Set(0.0f, 15.0f);
			sd2.Density = 1.0f;

			BodyDef bd2 = new BodyDef();

			bd2.Position.Set(0.0f, 10.0f);

			_body2 = _world.CreateBody(bd2);
			_shape2 = _body2.CreateShape(sd2);
			_body2.SetMassFromShapes();

			_world.Gravity = new Vec2(0.0f, 0.0f);
		}

		public override void Step(Settings settings)
		{			
			settings.pause = 1;
			base.Step(settings);
			settings.pause = 0;

			Vec2 x1, x2;
			float distance = Collision.Distance(out x1, out x2, _shape1, _body1.GetXForm(), _shape2, _body2.GetXForm());

			StringBuilder strBld = new StringBuilder();
			strBld.AppendFormat("distance = {0}", new object[] { distance });
			OpenGLDebugDraw.DrawString(5, _textLine, strBld.ToString());
			_textLine += 15;

			strBld = new StringBuilder();
			strBld.AppendFormat("iterations = {0}", new object[] { Collision.GJKIterations });
			OpenGLDebugDraw.DrawString(5, _textLine, strBld.ToString());
			_textLine += 15;

			OpenGLDebugDraw.DrawPoint(x1, 4.0f, new Color(1, 0, 0));
			OpenGLDebugDraw.DrawSegment(x1, x2, new Color(1, 0, 0));
			OpenGLDebugDraw.DrawPoint(x2, 4.0f, new Color(1, 0, 0));
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
			return new DistanceTest();
		}
	}
}