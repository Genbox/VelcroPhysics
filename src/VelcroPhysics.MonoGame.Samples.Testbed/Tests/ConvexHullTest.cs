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

using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class ConvexHullTest : Test
    {
        private readonly int _ecount = Settings.MaxPolygonVertices;
        private readonly Vector2[] _points = new Vector2[Settings.MaxPolygonVertices];
        private int _count;
        private bool _auto;

        private ConvexHullTest()
        {
            Generate();
            _auto = false;
        }

        private void Generate()
        {
            Vector2 lowerBound = new Vector2(-8.0f, -8.0f);
            Vector2 upperBound = new Vector2(8.0f, 8.0f);

            for (int i = 0; i < _ecount; ++i)
            {
                float x = 10.0f * Rand.RandomFloat();
                float y = 10.0f * Rand.RandomFloat();

                // Clamp onto a square to help create collinearities.
                // This will stress the convex hull algorithm.
                Vector2 v = new Vector2(x, y);
                v = MathUtils.Clamp(v, lowerBound, upperBound);
                _points[i] = v;
            }

            _count = _ecount;
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsNewKeyPress(Keys.A))
                _auto = !_auto;
            else if (keyboard.IsNewKeyPress(Keys.G))
                Generate();

            base.Keyboard(keyboard);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            PolygonShape shape = new PolygonShape(new Vertices(_points), 0);

            DrawString("Press g to generate a new random convex hull");

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);

            DebugView.DrawPolygon(shape.Vertices.ToArray(), shape.Vertices.Count, new Color(0.9f, 0.9f, 0.9f));

            for (int i = 0; i < _count; ++i)
            {
                DebugView.DrawPoint(_points[i], 3.0f, new Color(0.3f, 0.9f, 0.3f));
                DebugView.DrawString(_points[i] + new Vector2(0.05f, 0.05f), i.ToString());
            }

            DebugView.EndCustomDraw();

            if (_auto)
                Generate();
        }

        internal static Test Create()
        {
            return new ConvexHullTest();
        }
    }
}