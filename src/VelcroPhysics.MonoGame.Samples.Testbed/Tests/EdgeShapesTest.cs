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
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class EdgeShapesTest : Test
    {
        private class EdgeShapesCallback
        {
            public float ReportFixture(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
            {
                _fixture = fixture;
                _point = point;
                _normal = normal;

                return fraction;
            }

            public Fixture _fixture;
            public Vector2 _point;
            public Vector2 _normal;
        };

        private const int _maxBodies = 256;

        private int _bodyIndex;
        private readonly Body[] _bodies = new Body[_maxBodies];
        private readonly PolygonShape[] _polygons = new PolygonShape[4];
        private readonly CircleShape _circle = new CircleShape(1.0f);

        private float _angle;

        private EdgeShapesTest()
        {
            // Ground body
            {
                BodyDef bd = new BodyDef();
                Body ground = BodyFactory.CreateFromDef(World, bd);

                float x1 = -20.0f;
                float y1 = 2.0f * MathUtils.Cosf(x1 / 10.0f * MathConstants.Pi);
                for (int i = 0; i < 80; ++i)
                {
                    float x2 = x1 + 0.5f;
                    float y2 = 2.0f * MathUtils.Cosf(x2 / 10.0f * MathConstants.Pi);

                    EdgeShape shape = new EdgeShape();
                    shape.SetTwoSided(new Vector2(x1, y1), new Vector2(x2, y2));
                    ground.AddFixture(shape);

                    x1 = x2;
                    y1 = y2;
                }
            }

            {
                Vertices vertices = new Vertices(3);
                vertices.Add(new Vector2(-0.5f, 0.0f));
                vertices.Add(new Vector2(0.5f, 0.0f));
                vertices.Add(new Vector2(0.0f, 1.5f));
                _polygons[0] = new PolygonShape(vertices, 20);
            }

            {
                Vertices vertices = new Vertices(3);
                vertices.Add(new Vector2(-0.1f, 0.0f));
                vertices.Add(new Vector2(0.1f, 0.0f));
                vertices.Add(new Vector2(0.0f, 1.5f));
                _polygons[1] = new PolygonShape(vertices, 20);
            }

            {
                float w = 1.0f;
                float b = w / (2.0f + MathUtils.Sqrt(2.0f));
                float s = MathUtils.Sqrt(2.0f) * b;

                Vertices vertices = new Vertices(8);
                vertices.Add(new Vector2(0.5f * s, 0.0f));
                vertices.Add(new Vector2(0.5f * w, b));
                vertices.Add(new Vector2(0.5f * w, b + s));
                vertices.Add(new Vector2(0.5f * s, w));
                vertices.Add(new Vector2(-0.5f * s, w));
                vertices.Add(new Vector2(-0.5f * w, b + s));
                vertices.Add(new Vector2(-0.5f * w, b));
                vertices.Add(new Vector2(-0.5f * s, 0.0f));

                _polygons[2] = new PolygonShape(vertices, 20);
            }

            {
                _polygons[3] = new PolygonShape(1.0f);
                _polygons[3].SetAsBox(0.5f, 0.5f);
            }

            {
                _circle.Radius = 0.5f;
            }

            _bodyIndex = 0;

            _angle = 0.0f;
        }

        private void Create(int index)
        {
            if (_bodies[_bodyIndex] != null)
            {
                World.RemoveBody(_bodies[_bodyIndex]);
                _bodies[_bodyIndex] = null;
            }

            BodyDef bd = new BodyDef();

            float x = Rand.RandomFloat(-10.0f, 10.0f);
            float y = Rand.RandomFloat(10.0f, 20.0f);
            bd.Position = new Vector2(x, y);
            bd.Angle = Rand.RandomFloat(-MathConstants.Pi, MathConstants.Pi);
            bd.Type = BodyType.Dynamic;

            if (index == 4)
                bd.AngularDamping = 0.02f;

            _bodies[_bodyIndex] = BodyFactory.CreateFromDef(World, bd);

            if (index < 4)
            {
                FixtureDef fd = new FixtureDef();
                fd.Shape = _polygons[index];
                fd.Friction = 0.3f;
                _bodies[_bodyIndex].AddFixture(fd);
            }
            else
            {
                FixtureDef fd = new FixtureDef();
                fd.Shape = _circle;
                fd.Friction = 0.3f;
                _bodies[_bodyIndex].AddFixture(fd);
            }

            _bodyIndex = (_bodyIndex + 1) % _maxBodies;
        }

        private void DestroyBody()
        {
            for (int i = 0; i < _maxBodies; ++i)
                if (_bodies[i] != null)
                {
                    World.RemoveBody(_bodies[i]);
                    _bodies[i] = null;
                    return;
                }
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsNewKeyPress(Keys.D1))
                Create(1);
            else if (keyboard.IsNewKeyPress(Keys.D2))
                Create(2);
            else if (keyboard.IsNewKeyPress(Keys.D3))
                Create(3);
            else if (keyboard.IsNewKeyPress(Keys.D4))
                Create(4);
            else if (keyboard.IsNewKeyPress(Keys.D5))
                Create(5);
            else if (keyboard.IsNewKeyPress(Keys.D))
                DestroyBody();

            base.Keyboard(keyboard);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            bool advanceRay = !settings.Pause || settings.SingleStep;

            base.Update(settings, gameTime);
            DrawString("Press 1-5 to drop stuff");

            float L = 25.0f;
            Vector2 point1 = new Vector2(0.0f, 10.0f);
            Vector2 d = new Vector2(L * MathUtils.Cosf(_angle), -L * MathUtils.Abs(MathUtils.Sinf(_angle)));
            Vector2 point2 = point1 + d;

            EdgeShapesCallback callback = new EdgeShapesCallback();

            World.RayCast(callback.ReportFixture, point1, point2);

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);

            if (callback._fixture != null)
            {
                DebugView.DrawPoint(callback._point, 5f, new Color(0.4f, 0.9f, 0.4f));

                DebugView.DrawSegment(point1, callback._point, new Color(0.8f, 0.8f, 0.8f));

                Vector2 head = callback._point + 0.5f * callback._normal;
                DebugView.DrawSegment(callback._point, head, new Color(0.9f, 0.9f, 0.4f));
            }
            else
            {
                DebugView.DrawSegment(point1, point2, new Color(0.8f, 0.8f, 0.8f));
            }

            DebugView.EndCustomDraw();

            if (advanceRay)
                _angle += 0.25f * MathConstants.Pi / 180.0f;
        }

        internal static Test Create()
        {
            return new EdgeShapesTest();
        }
    }
}