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
        PolygonShape _polygonA = new PolygonShape();
        PolygonShape _polygonB = new PolygonShape();

        XForm _transformA;
        XForm _transformB;

        Vec2 _positionB;
        float _angleB;

		public PolyCollision()
		{
            {
			    Vec2[] vertices = new Vec2[4];
			    vertices[0].Set(-9.0f, -1.1f);
			    vertices[1].Set(7.0f, -1.1f);
			    vertices[2].Set(5.0f, -0.9f);
			    vertices[3].Set(-11.0f, -0.9f);
			    _polygonA.Set(vertices, 4);
                _transformA = new XForm(new Vec2(0.0f, 10.0f), new Mat22(0.0f));
            }

		    {
			    _polygonB.SetAsBox(0.5f, 0.5f);
			    _positionB.SetZero();
			    _angleB = 0.0f;
                _transformB = new XForm(_positionB, new Mat22(_angleB));
		    }
		}

		public override void Step(Settings settings)
		{
			settings.pause = 1;
			base.Step(settings);
			settings.pause = 0;

            //B2_NOT_USED(settings);

		    Manifold manifold = new Manifold();
		    Collision.CollidePolygons(out manifold, _polygonA, _transformA, _polygonB, _transformB);

		    WorldManifold worldManifold = new WorldManifold();
		    worldManifold.Initialize(manifold, _transformA, _polygonA.Radius, _transformB, _polygonB.Radius);

		    //_debugDraw.DrawString(5, m_textLine, "point count = %d", manifold.m_pointCount);
		    _textLine += 15;

		    {
			    Color color = new Color(0.9f, 0.9f, 0.9f);
			    Vec2[] v = new Vec2[Box2DX.Common.Settings.MaxPolygonVertices];
			    for (int i = 0; i < _polygonA.VertexCount; ++i)
			    {
                    v[i] = Box2DX.Common.Math.Mul(_transformA, _polygonA.Vertices[i]);
			    }
			    _debugDraw.DrawPolygon(v, _polygonA.VertexCount, color);

			    for (int i = 0; i < _polygonB.VertexCount; ++i)
			    {
                    v[i] = Box2DX.Common.Math.Mul(_transformB, _polygonB.Vertices[i]);
			    }
			    _debugDraw.DrawPolygon(v, _polygonB.VertexCount, color);
		    }

		    for (int i = 0; i < manifold.PointCount; ++i)
		    {
			    OpenGLDebugDraw.DrawPoint(WorldManifold.Points[i], 4.0f, new Color(0.9f, 0.3f, 0.3f));
		    }
		}

		public override void Keyboard(System.Windows.Forms.Keys key)
		{

			switch (key)
			{
				case System.Windows.Forms.Keys.A:
                    _positionB.X -= 0.1f;
					break;

				case System.Windows.Forms.Keys.D:
                    _positionB.X += 0.1f;
					break;

				case System.Windows.Forms.Keys.S:
                    _positionB.Y -= 0.1f;
					break;

				case System.Windows.Forms.Keys.W:
                    _positionB.Y += 0.1f;
					break;

				case System.Windows.Forms.Keys.Q:
                    _angleB += 0.1f * Box2DX.Common.Settings.Pi;
					break;

				case System.Windows.Forms.Keys.E:
                    _angleB -= 0.1f * Box2DX.Common.Settings.Pi;
					break;
			}

            _transformB = new XForm(_positionB, new Mat22(_angleB));
		}

		public static Test Create()
		{
			return new PolyCollision();
		}
	}
}