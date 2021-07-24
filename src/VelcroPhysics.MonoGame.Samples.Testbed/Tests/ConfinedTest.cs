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
    internal class ConfinedTest : Test
    {
        private const int _columnCount = 0;
        private const int _rowCount = 0;

        private ConfinedTest()
        {
            {
                BodyDef bd = new BodyDef();
                Body ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape();

                // Floor
                shape.SetTwoSided(new Vector2(-10.0f, 0.0f), new Vector2(10.0f, 0.0f));
                ground.AddFixture(shape);

                // Left wall
                shape.SetTwoSided(new Vector2(-10.0f, 0.0f), new Vector2(-10.0f, 20.0f));
                ground.AddFixture(shape);

                // Right wall
                shape.SetTwoSided(new Vector2(10.0f, 0.0f), new Vector2(10.0f, 20.0f));
                ground.AddFixture(shape);

                // Roof
                shape.SetTwoSided(new Vector2(-10.0f, 20.0f), new Vector2(10.0f, 20.0f));
                ground.AddFixture(shape);
            }

            {
                float radius = 0.5f;
                CircleShape shape = new CircleShape(1.0f);
                shape.Position = Vector2.Zero;
                shape.Radius = radius;

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Friction = 0.1f;

                for (int j = 0; j < _columnCount; ++j)
                for (int i = 0; i < _rowCount; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(-10.0f + (2.1f * j + 1.0f + 0.01f * i) * radius, (2.0f * i + 1.0f) * radius);
                    Body body = BodyFactory.CreateFromDef(World, bd);

                    body.AddFixture(fd);
                }

                World.Gravity = new Vector2(0.0f, 0.0f);
            }
        }

        private void CreateCircle()
        {
            float radius = 2.0f;
            CircleShape shape = new CircleShape(1.0f);
            shape.Position = Vector2.Zero;
            shape.Radius = radius;

            FixtureDef fd = new FixtureDef();
            fd.Shape = shape;
            fd.Friction = 0.0f;

            Vector2 p = new Vector2(Rand.RandomFloat(), 3.0f + Rand.RandomFloat());
            BodyDef bd = new BodyDef();
            bd.Type = BodyType.Dynamic;
            bd.Position = p;

            //bd.AllowSleep = false;
            Body body = BodyFactory.CreateFromDef(World, bd);

            body.AddFixture(fd);
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsNewKeyPress(Keys.C))
                CreateCircle();

            base.Keyboard(keyboard);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("Press 'c' to create a circle.");

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new ConfinedTest();
        }
    }
}