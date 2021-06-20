// MIT License

// Copyright (c) 2019 Erin Catto

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Genbox.VelcroPhysics.Collision.Narrowphase;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Shared.Optimization;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class PolygonCollisionTest : Test
    {
        private PolygonShape _polygonA;
        private PolygonShape _polygonB;

        private Transform _transformA;
        private Transform _transformB;

        private Vector2 _positionB;
        private float _angleB;

        private PolygonCollisionTest()
        {
            {
                _polygonA = new PolygonShape(1.0f);
                _polygonA.SetAsBox(0.2f, 0.4f);
                _transformA.Set(new Vector2(0.0f, 0.0f), 0.0f);
            }

            {
                _polygonB = new PolygonShape(1.0f);
                _polygonB.SetAsBox(0.5f, 0.5f);
                _positionB = new Vector2(19.345284f, 1.5632932f);
                _angleB = 1.9160721f;
                _transformB.Set(_positionB, _angleB);
            }
        }

        internal static Test Create()
        {
            return new PolygonCollisionTest();
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            Manifold manifold = new Manifold();
            CollidePolygon.CollidePolygons(ref manifold, _polygonA, ref _transformA, _polygonB, ref _transformB);

            WorldManifold.Initialize(ref manifold, ref _transformA, _polygonA.Radius, ref _transformB, _polygonB.Radius, out Vector2 normal, out FixedArray2<Vector2> points, out FixedArray2<float> separations);

            DrawString("point count = " + manifold.PointCount);

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);

            {
                Color color = new Color(0.9f, 0.9f, 0.9f);
                Vector2[] v = new Vector2[Settings.MaxPolygonVertices];
                for (int i = 0; i < _polygonA.Vertices.Count; ++i)
                    v[i] = MathUtils.Mul(ref _transformA, _polygonA.Vertices[i]);
                DebugView.DrawPolygon(v, _polygonA.Vertices.Count, color);

                for (int i = 0; i < _polygonB.Vertices.Count; ++i)
                    v[i] = MathUtils.Mul(ref _transformB, _polygonB.Vertices[i]);
                DebugView.DrawPolygon(v, _polygonB.Vertices.Count, color);
            }

            for (int i = 0; i < manifold.PointCount; ++i)
                DebugView.DrawPoint(points[i], 4.0f, new Color(0.9f, 0.3f, 0.3f));

            DebugView.EndCustomDraw();

            base.Update(settings, gameTime);
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsNewKeyPress(Keys.A))
                _positionB.X -= 0.1f;
            else if (keyboard.IsNewKeyPress(Keys.D))
                _positionB.X += 0.1f;
            else if (keyboard.IsNewKeyPress(Keys.S))
                _positionB.Y -= 0.1f;
            else if (keyboard.IsNewKeyPress(Keys.W))
                _positionB.Y += 0.1f;
            else if (keyboard.IsNewKeyPress(Keys.Q))
                _angleB += 0.1f * MathConstants.Pi;
            else if (keyboard.IsNewKeyPress(Keys.E))
                _angleB -= 0.1f * MathConstants.Pi;

            _transformB.Set(_positionB, _angleB);

            base.Keyboard(keyboard);
        }
    }
}