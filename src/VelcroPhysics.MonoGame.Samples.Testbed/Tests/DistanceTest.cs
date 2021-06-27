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

using Genbox.VelcroPhysics.Collision.Distance;
using Genbox.VelcroPhysics.Collision.Narrowphase;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class DistanceTest : Test
    {
        private Vector2 _positionB;
        private float _angleB;

        private Transform _transformA;
        private Transform _transformB;
        private PolygonShape _polygonA = new PolygonShape(0.0f);
        private PolygonShape _polygonB = new PolygonShape(0.0f);

        private DistanceTest()
        {
            {
                _transformA.SetIdentity();
                _transformA.p = new Vector2(0.0f, -0.2f);
                _polygonA.SetAsBox(10.0f, 0.2f);
            }

            {
                _positionB = new Vector2(12.017401f, 0.13678508f);
                _angleB = -0.0109265f;
                _transformB.Set(_positionB, _angleB);

                _polygonB.SetAsBox(2.0f, 0.1f);
            }
        }

        internal static Test Create()
        {
            return new DistanceTest();
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            DistanceInput input = new DistanceInput();
            input.ProxyA = new DistanceProxy(_polygonA, 0);
            input.ProxyB = new DistanceProxy(_polygonB, 0);
            input.TransformA = _transformA;
            input.TransformB = _transformB;
            input.UseRadii = true;
            SimplexCache cache = new SimplexCache();
            cache.Count = 0;
            DistanceOutput output;
            DistanceGJK.ComputeDistance(ref input, out output, out cache);

            DrawString($"distance = {output.Distance}");

            DrawString($"iterations = {output.Iterations}");

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

            Vector2 x1 = output.PointA;
            Vector2 x2 = output.PointB;

            Color c1 = new Color(1.0f, 0.0f, 0.0f);
            DebugView.DrawPoint(x1, 4.0f, c1);

            Color c2 = new Color(1.0f, 1.0f, 0.0f);
            DebugView.DrawPoint(x2, 4.0f, c2);

            DebugView.EndCustomDraw();
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsKeyDown(Keys.A))
                _positionB.X -= 0.1f;
            else if (keyboard.IsKeyDown(Keys.D))
                _positionB.X += 0.1f;
            else if (keyboard.IsKeyDown(Keys.S))
                _positionB.Y -= 0.1f;
            else if (keyboard.IsKeyDown(Keys.W))
                _positionB.Y += 0.1f;
            else if (keyboard.IsKeyDown(Keys.Q))
                _angleB += 0.1f * MathConstants.Pi;
            else if (keyboard.IsKeyDown(Keys.E))
                _angleB -= 0.1f * MathConstants.Pi;

            _transformB.Set(_positionB, _angleB);

            base.Keyboard(keyboard);
        }
    }
}