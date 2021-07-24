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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class CompoundShapesTest : Test
    {
        private readonly Body _table1;
        private readonly Body _table2;
        private readonly Body _ship1;
        private readonly Body _ship2;

        private CompoundShapesTest()
        {
            {
                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(0.0f, 0.0f);
                Body body = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape();
                shape.SetTwoSided(new Vector2(50.0f, 0.0f), new Vector2(-50.0f, 0.0f));

                body.AddFixture(shape);
            }

            // Table 1
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-15.0f, 1.0f);
                _table1 = BodyFactory.CreateFromDef(World, bd);

                PolygonShape top = new PolygonShape(2.0f);
                top.SetAsBox(3.0f, 0.5f, new Vector2(0.0f, 3.5f), 0.0f);

                PolygonShape leftLeg = new PolygonShape(2.0f);
                leftLeg.SetAsBox(0.5f, 1.5f, new Vector2(-2.5f, 1.5f), 0.0f);

                PolygonShape rightLeg = new PolygonShape(2.0f);
                rightLeg.SetAsBox(0.5f, 1.5f, new Vector2(2.5f, 1.5f), 0.0f);

                _table1.AddFixture(top);
                _table1.AddFixture(leftLeg);
                _table1.AddFixture(rightLeg);
            }

            // Table 2
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-5.0f, 1.0f);
                _table2 = BodyFactory.CreateFromDef(World, bd);

                PolygonShape top = new PolygonShape(2.0f);
                top.SetAsBox(3.0f, 0.5f, new Vector2(0.0f, 3.5f), 0.0f);

                PolygonShape leftLeg = new PolygonShape(2.0f);
                leftLeg.SetAsBox(0.5f, 2.0f, new Vector2(-2.5f, 2.0f), 0.0f);

                PolygonShape rightLeg = new PolygonShape(2.0f);
                rightLeg.SetAsBox(0.5f, 2.0f, new Vector2(2.5f, 2.0f), 0.0f);

                _table2.AddFixture(top);
                _table2.AddFixture(leftLeg);
                _table2.AddFixture(rightLeg);
            }

            // Spaceship 1
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(5.0f, 1.0f);
                _ship1 = BodyFactory.CreateFromDef(World, bd);

                Vertices vertices = new Vertices(3);

                PolygonShape left = new PolygonShape(2.0f);
                vertices.Add(new Vector2(-2.0f, 0.0f));
                vertices.Add(new Vector2(0.0f, 4.0f / 3.0f));
                vertices.Add(new Vector2(0.0f, 4.0f));
                left.Vertices = vertices;

                PolygonShape right = new PolygonShape(2.0f);
                vertices[0] = new Vector2(2.0f, 0.0f);
                vertices[1] = new Vector2(0.0f, 4.0f / 3.0f);
                vertices[2] = new Vector2(0.0f, 4.0f);
                right.Vertices = vertices;

                _ship1.AddFixture(left);
                _ship1.AddFixture(right);
            }

            // Spaceship 2
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(15.0f, 1.0f);
                _ship2 = BodyFactory.CreateFromDef(World, bd);

                Vertices vertices = new Vertices(3);

                PolygonShape left = new PolygonShape(2.0f);
                vertices.Add(new Vector2(-2.0f, 0.0f));
                vertices.Add(new Vector2(1.0f, 2.0f));
                vertices.Add(new Vector2(0.0f, 4.0f));
                left.Vertices = vertices;

                PolygonShape right = new PolygonShape(2.0f);
                vertices[0] = new Vector2(2.0f, 0.0f);
                vertices[1] = new Vector2(-1.0f, 2.0f);
                vertices[2] = new Vector2(0.0f, 4.0f);
                right.Vertices = vertices;

                _ship2.AddFixture(left);
                _ship2.AddFixture(right);
            }
        }

        private void Spawn()
        {
            // Table 1 obstruction
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = _table1.Position;
                bd.Angle = _table1.Rotation;

                Body body = BodyFactory.CreateFromDef(World, bd);

                PolygonShape box = new PolygonShape(2.0f);
                box.SetAsBox(4.0f, 0.1f, new Vector2(0.0f, 3.0f), 0.0f);

                body.AddFixture(box);
            }

            // Table 2 obstruction
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = _table2.Position;
                bd.Angle = _table2.Rotation;

                Body body = BodyFactory.CreateFromDef(World, bd);

                PolygonShape box = new PolygonShape(2.0f);
                box.SetAsBox(4.0f, 0.1f, new Vector2(0.0f, 3.0f), 0.0f);

                body.AddFixture(box);
            }

            // Ship 1 obstruction
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = _ship1.Position;
                bd.Angle = _ship1.Rotation;
                bd.GravityScale = 0.0f;

                Body body = BodyFactory.CreateFromDef(World, bd);

                CircleShape circle = new CircleShape(0.5f, 2.0f, new Vector2(0.0f, 2.0f));
                body.AddFixture(circle);
            }

            // Ship 2 obstruction
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = _ship2.Position;
                bd.Angle = _ship2.Rotation;
                bd.GravityScale = 0.0f;

                Body body = BodyFactory.CreateFromDef(World, bd);

                CircleShape circle = new CircleShape(2.0f);
                circle.Radius = 0.5f;
                circle.Position = new Vector2(0.0f, 2.0f);

                body.AddFixture(circle);
            }
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsNewKeyPress(Keys.S))
                Spawn();

            base.Keyboard(keyboard);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("Press S to spawn");

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new CompoundShapesTest();
        }
    }
}