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
	public class PolyShapes : Test
	{
		const int k_maxBodies = 256;

		int bodyIndex;
		Body[] bodies = new Body[k_maxBodies];
		PolygonDef[] sds = new PolygonDef[4];
		CircleDef circleDef = new CircleDef();

		public PolyShapes()
		{
			// Ground body
			{
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(50.0f, 10.0f);
				sd.Friction = 0.3f;
				sd.Filter.CategoryBits = 0x0001;

				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, -10.0f);
				Body ground = _world.CreateBody(bd);
				ground.CreateShape(sd);
			}

			for (int i = 0; i < 4; i++)
			{
				sds[i] = new PolygonDef();
			}

			sds[0].VertexCount = 3;
			sds[0].Vertices[0].Set(-0.5f, 0.0f);
			sds[0].Vertices[1].Set(0.5f, 0.0f);
			sds[0].Vertices[2].Set(0.0f, 1.5f);
			sds[0].Density = 1.0f;
			sds[0].Friction = 0.3f;
			sds[0].Filter.CategoryBits = 0x0002;
			//sds[0].MaskBits = 0x0003;

			sds[1].VertexCount = 3;
			sds[1].Vertices[0].Set(-0.1f, 0.0f);
			sds[1].Vertices[1].Set(0.1f, 0.0f);
			sds[1].Vertices[2].Set(0.0f, 1.5f);
			sds[1].Density = 1.0f;
			sds[1].Friction = 0.3f;
			sds[1].Filter.CategoryBits = 0x0004;

			sds[2].VertexCount = 8;
			float w = 1.0f;
			float b = w / (2.0f + (float)System.Math.Sqrt(2.0f));
			float s = (float)System.Math.Sqrt(2.0f) * b;
			sds[2].Vertices[0].Set(0.5f * s, 0.0f);
			sds[2].Vertices[1].Set(0.5f * w, b);
			sds[2].Vertices[2].Set(0.5f * w, b + s);
			sds[2].Vertices[3].Set(0.5f * s, w);
			sds[2].Vertices[4].Set(-0.5f * s, w);
			sds[2].Vertices[5].Set(-0.5f * w, b + s);
			sds[2].Vertices[6].Set(-0.5f * w, b);
			sds[2].Vertices[7].Set(-0.5f * s, 0.0f);
			sds[2].Density = 1.0f;
			sds[2].Friction = 0.3f;
			sds[2].Filter.CategoryBits = 0x0004;

			sds[3].VertexCount = 4;
			sds[3].Vertices[0].Set(-0.5f, 0.0f);
			sds[3].Vertices[1].Set(0.5f, 0.0f);
			sds[3].Vertices[2].Set(0.5f, 1.0f);
			sds[3].Vertices[3].Set(-0.5f, 1.0f);
			sds[3].Density = 1.0f;
			sds[3].Friction = 0.3f;
			sds[3].Filter.CategoryBits = 0x0004;

			circleDef.Radius = 0.5f;
			circleDef.Density = 1.0f;

			bodyIndex = 0;
			//memset(bodies, 0, sizeof(bodies));
		}

		public void Create(int index)
		{
			if (bodies[bodyIndex] != null)
			{
				_world.DestroyBody(bodies[bodyIndex]);
				bodies[bodyIndex] = null;
			}

			BodyDef bd = new BodyDef();

			float x = Box2DX.Common.Math.Random(-2.0f, 2.0f);
			bd.Position.Set(x, 10.0f);
			bd.Angle = Box2DX.Common.Math.Random(-Box2DX.Common.Settings.Pi, Box2DX.Common.Settings.Pi);

			if (index == 4)
			{
				bd.AngularDamping = 0.02f;
			}

			bodies[bodyIndex] = _world.CreateBody(bd);

			if (index < 4)
			{
				bodies[bodyIndex].CreateShape(sds[index]);
			}
			else
			{
				bodies[bodyIndex].CreateShape(circleDef);
			}
			bodies[bodyIndex].SetMassFromShapes();

			bodyIndex = (bodyIndex + 1) % k_maxBodies;
		}

		public void DestroyBody()
		{
			for (int i = 0; i < k_maxBodies; ++i)
			{
				if (bodies[i] != null)
				{
					_world.DestroyBody(bodies[i]);
					bodies[i] = null;
					return;
				}
			}
		}

		public override void Step(Settings settings)
		{
			base.Step(settings);
			OpenGLDebugDraw.DrawString(5, _textLine, "Press 1-5 to drop stuff");
			_textLine += 15;
		}

		public override void Keyboard(System.Windows.Forms.Keys key)
		{
			int keyKode = 0;
			switch (key)
			{
				case System.Windows.Forms.Keys.D1:
					keyKode = 1;
					break;
				case System.Windows.Forms.Keys.D2:
					keyKode = 2;
					break;
				case System.Windows.Forms.Keys.D3:
					keyKode = 3;
					break;
				case System.Windows.Forms.Keys.D4:
					keyKode = 4;
					break;
				case System.Windows.Forms.Keys.D5:
					keyKode = 5;					
					break;
				case System.Windows.Forms.Keys.D:
					DestroyBody();
					return;
				default:
					return;
			}
			Create(keyKode - 1);
			return;
		}

		public static Test Create()
		{
			return new PolyShapes();
		}
	}
}