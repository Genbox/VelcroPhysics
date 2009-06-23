/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.Com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2008 Erin Catto http://www.gphysics.Com

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
	public class TimeOfImpact : Test
	{
		Body _body1;
		Body _body2;
		Shape _shape1;
		PolygonShape _shape2;

		public TimeOfImpact()
		{
			{
				PolygonDef sd = new PolygonDef();
				sd.Density = 0.0f;

				sd.SetAsBox(0.1f, 10.0f, new Vec2(10.0f, 0.0f), 0.0f);

				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, 20.0f);
				bd.Angle = 0.0f;
				_body1 = _world.CreateBody(bd);
				_shape1 = _body1.CreateShape(sd);
			}

			{
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(0.25f, 0.25f);
				sd.Density = 1.0f;

				BodyDef bd = new BodyDef();
				bd.Position.Set(9.6363468f, 28.050615f);
				bd.Angle = 1.6408679f;
				_body2 = _world.CreateBody(bd);
				_shape2 = (PolygonShape)_body2.CreateShape(sd);
				_body2.SetMassFromShapes();
			}
		}

		public override void Step(Settings settings)
		{			
			settings.pause = 1;
			base.Step(settings);
			settings.pause = 0;

			Sweep sweep1 = new Sweep();
			sweep1.C0.Set(0.0f, 20.0f);
			sweep1.A0 = 0.0f;
			sweep1.C = sweep1.C0;
			sweep1.A = sweep1.A0;
			sweep1.T0 = 0.0f;
			sweep1.LocalCenter = _body1.GetLocalCenter();

			Sweep sweep2 = new Sweep();
			sweep2.C0.Set(9.6363468f, 28.050615f);
			sweep2.A0 = 1.6408679f;
			sweep2.C = sweep2.C0 + new Vec2(-0.075121880f, 0.27358246f);
			sweep2.A = sweep2.A0 - 10.434675f;
			sweep2.T0 = 0.0f;
			sweep2.LocalCenter = _body2.GetLocalCenter();

			float toi = Collision.TimeOfImpact(_shape1, sweep1, _shape2, sweep2);
			
			OpenGLDebugDraw.DrawString(5, _textLine, "toi = " + toi.ToString());
			_textLine += 15;

			XForm xf2 = new XForm();
			sweep2.GetXForm(out xf2, toi);
			int vertexCount = _shape2.VertexCount;
			Vec2[] vertices = new Vec2[Box2DX.Common.Settings.MaxPolygonVertices];
			Vec2[] localVertices = _shape2.GetVertices();
			for (int i = 0; i < vertexCount; ++i)
			{
				vertices[i] = Box2DX.Common.Math.Mul(xf2, localVertices[i]);
			}
			_debugDraw.DrawPolygon(vertices, vertexCount, new Color(0.5f, 0.7f, 0.9f));

			localVertices = _shape2.GetCoreVertices();
			for (int i = 0; i < vertexCount; ++i)
			{
				vertices[i] = Box2DX.Common.Math.Mul(xf2, localVertices[i]);
			}
			_debugDraw.DrawPolygon(vertices, vertexCount, new Color(0.5f, 0.7f, 0.9f));
		}

		public static Test Create()
		{
			return new TimeOfImpact();
		}	
	}
}