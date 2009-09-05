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

using Box2DX.Common;
using Box2DX.Collision;
using Box2DX.Dynamics;

namespace TestBed
{
    public class PolyCollision : Test
    {
        private PolygonShape _polygonA = new PolygonShape();
        private PolygonShape _polygonB = new PolygonShape();

        private Transform _transformA;
        private Transform _transformB;

        private Vec2 _positionB;
        private float _angleB;

        public PolyCollision()
        {
            {
                _polygonA.SetAsBox(1.0f, 1.0f, new Vec2(0.0f, 0.0f), Box2DX.Common.Settings.PI * 0.25f);
                _transformA.Set(new Vec2(0.0f, 5.0f), 0.0f);
            }

            {
                _polygonB.SetAsBox(0.25f, 0.25f);
                _positionB.Set(-1.7793884f, 5.0326509f);
                _angleB = 2.2886343f;
                _transformB.Set(_positionB, _angleB);
            }

        }

        public override void Step(Settings settings)
        {
            //B2_NOT_USED(settings);

            Manifold manifold;
            Collision.CollidePolygons(out manifold, _polygonA, _transformA, _polygonB, _transformB);

            WorldManifold worldManifold = new WorldManifold();
            worldManifold.Initialize(manifold, _transformA, _polygonA._radius, _transformB, _polygonB._radius);

            OpenGLDebugDraw.DrawString(5, _textLine, string.Format("point count = {0}", manifold.PointCount));
            _textLine += 15;

            {
                Color color = new Color(0.9f, 0.9f, 0.9f);
                Vec2[] v = new Vec2[Box2DX.Common.Settings.MaxPolygonVertices];
                for (int i = 0; i < _polygonA.VertexCount; ++i)
                {
                    v[i] = Math.Mul(_transformA, _polygonA.Vertices[i]);
                }
                _debugDraw.DrawPolygon(v, _polygonA.VertexCount, color);

                for (int i = 0; i < _polygonB.VertexCount; ++i)
                {
                    v[i] = Math.Mul(_transformB, _polygonB.Vertices[i]);
                }
                _debugDraw.DrawPolygon(v, _polygonB.VertexCount, color);
            }

            for (int i = 0; i < manifold.PointCount; ++i)
            {
                OpenGLDebugDraw.DrawPoint(worldManifold.Points[i], 4.0f, new Color(0.9f, 0.3f, 0.3f));
            }

            base.Step(settings);
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
                    _angleB += 0.1f * Box2DX.Common.Settings.PI;
                    break;

                case System.Windows.Forms.Keys.E:
                    _angleB -= 0.1f * Box2DX.Common.Settings.PI;
                    break;
            }

            _transformB.Set(_positionB, _angleB);
        }

        public static Test Create()
        {
            return new PolyCollision();
        }
    }
}