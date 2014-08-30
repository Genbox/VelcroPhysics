/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Testbed.Tests
{
    public class PolyCollisionTest : Test
    {
        private float _angleB;
        private PolygonShape _polygonA = new PolygonShape(1);
        private PolygonShape _polygonB = new PolygonShape(1);
        private Vector2 _positionB;

        private Transform _transformA;
        private Transform _transformB;

        private PolyCollisionTest()
        {
            {
                _polygonA.Vertices = PolygonTools.CreateRectangle(0.2f, 0.4f);
                _transformA.Set(Vector2.Zero, 0.0f);
            }

            {
                _polygonB.Vertices = PolygonTools.CreateRectangle(0.5f, 0.5f);
                //_positionB = new Vector2(19.345284f, 1.5632932f);
                _positionB = new Vector2(0.345284f, 0.5632932f);

                _angleB = 1.9160721f;
                _transformB.Set(_positionB, _angleB);
            }
        }

        internal static Test Create()
        {
            return new PolyCollisionTest();
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            Manifold manifold = new Manifold();
            Collision.Collision.CollidePolygons(ref manifold, _polygonA, ref _transformA, _polygonB, ref _transformB);

            Vector2 normal;
            FixedArray2<Vector2> points;
            ContactSolver.WorldManifold.Initialize(ref manifold, ref _transformA, _polygonA.Radius, ref _transformB, _polygonB.Radius, out normal, out points);

            DrawString("Point count = " + manifold.PointCount);

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
            {
                Color color = new Color(0.9f, 0.9f, 0.9f);
                Vector2[] v = new Vector2[Settings.MaxPolygonVertices];
                for (int i = 0; i < _polygonA.Vertices.Count; ++i)
                {
                    v[i] = MathUtils.Mul(ref _transformA, _polygonA.Vertices[i]);
                }
                DebugView.DrawPolygon(v, _polygonA.Vertices.Count, color);

                for (int i = 0; i < _polygonB.Vertices.Count; ++i)
                {
                    v[i] = MathUtils.Mul(ref _transformB, _polygonB.Vertices[i]);
                }
                DebugView.DrawPolygon(v, _polygonB.Vertices.Count, color);
            }

            for (int i = 0; i < manifold.PointCount; ++i)
            {
                DebugView.DrawPoint(points[i], 0.1f, new Color(0.9f, 0.3f, 0.3f));
                DrawString(points[i].ToString());
            }
            DebugView.EndCustomDraw();
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsKeyDown(Keys.A))
            {
                _positionB.X -= 0.1f;
            }
            if (keyboardManager.IsKeyDown(Keys.D))
            {
                _positionB.X += 0.1f;
            }
            if (keyboardManager.IsKeyDown(Keys.S))
            {
                _positionB.Y -= 0.1f;
            }
            if (keyboardManager.IsKeyDown(Keys.W))
            {
                _positionB.Y += 0.1f;
            }
            if (keyboardManager.IsKeyDown(Keys.Q))
            {
                _angleB += 0.1f * Settings.Pi;
            }
            if (keyboardManager.IsKeyDown(Keys.E))
            {
                _angleB -= 0.1f * Settings.Pi;
            }

            _transformB.Set(_positionB, _angleB);
        }
    }
}