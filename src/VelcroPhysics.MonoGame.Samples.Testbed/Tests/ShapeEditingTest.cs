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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class ShapeEditingTest : Test
    {
        private readonly Body _body;
        private Fixture _fixture1;
        private Fixture _fixture2;
        private bool _sensor;

        private ShapeEditingTest()
        {
            {
                BodyDef bd = new BodyDef();
                Body ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape();
                shape.SetTwoSided(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.AddFixture(shape);
            }

            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(0.0f, 10.0f);
                _body = BodyFactory.CreateFromDef(World, bd);

                PolygonShape shape = new PolygonShape(10.0f);
                shape.SetAsBox(4.0f, 4.0f, new Vector2(0.0f, 0.0f), 0.0f);
                _fixture1 = _body.AddFixture(shape);

                _fixture2 = null;

                _sensor = false;
            }
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsNewKeyPress(Keys.C))
            {
                if (_fixture2 == null)
                {
                    CircleShape shape = new CircleShape(10.0f);
                    shape.Radius = 3.0f;
                    shape.Position = new Vector2(0.5f, -4.0f);
                    _fixture2 = _body.AddFixture(shape);
                    _body.Awake = true;
                }
            }
            else if (keyboard.IsNewKeyPress(Keys.D))
            {
                if (_fixture2 != null)
                {
                    _body.RemoveFixture(_fixture2);
                    _fixture2 = null;
                    _body.Awake = true;
                }
            }
            else if (keyboard.IsNewKeyPress(Keys.S))
            {
                if (_fixture2 != null)
                {
                    _sensor = !_sensor;
                    _fixture2.IsSensor = _sensor;
                }
            }

            base.Keyboard(keyboard);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            DrawString("Press: (c) create a shape, (d) destroy a shape.");
            DrawString("sensor = " + _sensor);
        }

        internal static Test Create()
        {
            return new ShapeEditingTest();
        }
    }
}