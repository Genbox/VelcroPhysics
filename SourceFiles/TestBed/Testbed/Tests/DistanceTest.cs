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

using Tao.OpenGl;

using Box2DX.Common;
using Box2DX.Collision;
using Box2DX.Dynamics;

namespace TestBed
{
	public class DistanceTest : Test
	{
        Vec2 _positionB;
        float _angleB;

        XForm _transformA;
        XForm _transformB;
        PolygonShape _polygonA = new PolygonShape();
        PolygonShape _polygonB = new PolygonShape();

		public DistanceTest()
		{
            {
                _transformA = new XForm();
                _transformA.SetIdentity();
                _transformA.Position.Set(0.0f, -0.2f);
                _polygonA.SetAsBox(10.0f, 0.2f);
            }

            {
                _positionB.Set(12.017401f, 0.13678508f);
                _angleB = -0.0109265f;
                _transformB = new XForm(_positionB, new Mat22(_angleB));

                _polygonB.SetAsBox(2.0f, 0.1f);
            }
		}

		public override void Step(Settings settings)
		{			
			settings.pause = 1;
			base.Step(settings);
			settings.pause = 0;

			Collision.DistanceInput input;
		    input.TransformA = _transformA;
		    input.TransformB = _transformB;
		    input.UseRadii = true;
		    Collision.SimplexCache cache = new Collision.SimplexCache();
		    cache.Count = 0;
		    Collision.DistanceOutput output;
		    Collision.Distance(out output, ref cache, ref input, _polygonA, _polygonB);

		    //_debugDraw.DrawString(5, m_textLine, "distance = %g", output.distance);
		    _textLine += 15;

		    //_debugDraw.DrawString(5, m_textLine, "iterations = %d", output.iterations);
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

		    Vec2 x1 = output.PointA;
		    Vec2 x2 = output.PointB;

		    Gl.glPointSize(4.0f);
            Gl.glColor4f(1.0f, 0.0f, 0.0f, 1);
            Gl.glBegin(Gl.GL_POINTS);
            Gl.glVertex2f(x1.X, x1.Y);
            Gl.glVertex2f(x2.X, x2.Y);
            Gl.glEnd();
            Gl.glPointSize(1.0f);

            Gl.glColor4f(1.0f, 1.0f, 0.0f, 1);
            Gl.glBegin(Gl.GL_LINES);
            Gl.glVertex2f(x1.X, x1.Y);
            Gl.glVertex2f(x2.X, x2.Y);
            Gl.glEnd();
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
			return new DistanceTest();
		}
	}
}