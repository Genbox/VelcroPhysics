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
    internal class EdgeTest : Test
    {
        private readonly Vector2 _offset1;
        private readonly Vector2 _offset2;
        private Body _body1;
        private Body _body2;
        private bool _boxes;

        private EdgeTest()
        {
            Vertices vertices = new Vertices
            {
                new Vector2(10.0f, -4.0f),
                new Vector2(10.0f, 0.0f),
                new Vector2(6.0f, 0.0f),
                new Vector2(4.0f, 2.0f),
                new Vector2(2.0f, 0.0f),
                new Vector2(-2.0f, 0.0f),
                new Vector2(-6.0f, 0.0f),
                new Vector2(-8.0f, -3.0f),
                new Vector2(-10.0f, 0.0f),
                new Vector2(-10.0f, -4.0f)
            };

            _offset1 = new Vector2(0.0f, 8.0f);
            _offset2 = new Vector2(0.0f, 16.0f);

            {
                Vector2 v1 = vertices[0] + _offset1;
                Vector2 v2 = vertices[1] + _offset1;
                Vector2 v3 = vertices[2] + _offset1;
                Vector2 v4 = vertices[3] + _offset1;
                Vector2 v5 = vertices[4] + _offset1;
                Vector2 v6 = vertices[5] + _offset1;
                Vector2 v7 = vertices[6] + _offset1;
                Vector2 v8 = vertices[7] + _offset1;
                Vector2 v9 = vertices[8] + _offset1;
                Vector2 v10 = vertices[9] + _offset1;

                BodyDef bd = new BodyDef();
                Body ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape();

                shape.SetOneSided(v10, v1, v2, v3);
                ground.AddFixture(shape);

                shape.SetOneSided(v1, v2, v3, v4);
                ground.AddFixture(shape);

                shape.SetOneSided(v2, v3, v4, v5);
                ground.AddFixture(shape);

                shape.SetOneSided(v3, v4, v5, v6);
                ground.AddFixture(shape);

                shape.SetOneSided(v4, v5, v6, v7);
                ground.AddFixture(shape);

                shape.SetOneSided(v5, v6, v7, v8);
                ground.AddFixture(shape);

                shape.SetOneSided(v6, v7, v8, v9);
                ground.AddFixture(shape);

                shape.SetOneSided(v7, v8, v9, v10);
                ground.AddFixture(shape);

                shape.SetOneSided(v8, v9, v10, v1);
                ground.AddFixture(shape);

                shape.SetOneSided(v9, v10, v1, v2);
                ground.AddFixture(shape);
            }

            {
                Vector2 v1 = vertices[0] + _offset2;
                Vector2 v2 = vertices[1] + _offset2;
                Vector2 v3 = vertices[2] + _offset2;
                Vector2 v4 = vertices[3] + _offset2;
                Vector2 v5 = vertices[4] + _offset2;
                Vector2 v6 = vertices[5] + _offset2;
                Vector2 v7 = vertices[6] + _offset2;
                Vector2 v8 = vertices[7] + _offset2;
                Vector2 v9 = vertices[8] + _offset2;
                Vector2 v10 = vertices[9] + _offset2;

                BodyDef bd = new BodyDef();
                Body ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape();

                shape.SetTwoSided(v1, v2);
                ground.AddFixture(shape);

                shape.SetTwoSided(v2, v3);
                ground.AddFixture(shape);

                shape.SetTwoSided(v3, v4);
                ground.AddFixture(shape);

                shape.SetTwoSided(v4, v5);
                ground.AddFixture(shape);

                shape.SetTwoSided(v5, v6);
                ground.AddFixture(shape);

                shape.SetTwoSided(v6, v7);
                ground.AddFixture(shape);

                shape.SetTwoSided(v7, v8);
                ground.AddFixture(shape);

                shape.SetTwoSided(v8, v9);
                ground.AddFixture(shape);

                shape.SetTwoSided(v9, v10);
                ground.AddFixture(shape);

                shape.SetTwoSided(v10, v1);
                ground.AddFixture(shape);
            }

            _body1 = null;
            _body2 = null;
            CreateBoxes();
            _boxes = true;
        }

        private void CreateBoxes()
        {
            if (_body1 != null)
            {
                World.RemoveBody(_body1);
                _body1 = null;
            }

            if (_body2 != null)
            {
                World.RemoveBody(_body2);
                _body2 = null;
            }

            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(8.0f, 2.6f) + _offset1;
                bd.AllowSleep = false;
                _body1 = BodyFactory.CreateFromDef(World, bd);

                PolygonShape shape = new PolygonShape(1.0f);
                shape.SetAsBox(0.5f, 1.0f);

                _body1.AddFixture(shape);
            }

            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(8.0f, 2.6f) + _offset2;
                bd.AllowSleep = false;
                _body2 = BodyFactory.CreateFromDef(World, bd);

                PolygonShape shape = new PolygonShape(1.0f);
                shape.SetAsBox(0.5f, 1.0f);

                _body2.AddFixture(shape);
            }
        }

        private void CreateCircles()
        {
            if (_body1 != null)
            {
                World.RemoveBody(_body1);
                _body1 = null;
            }

            if (_body2 != null)
            {
                World.RemoveBody(_body2);
                _body2 = null;
            }

            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-0.5f, 0.6f) + _offset1;
                bd.AllowSleep = false;
                _body1 = BodyFactory.CreateFromDef(World, bd);

                CircleShape shape = new CircleShape(1.0f);
                shape.Radius = 0.5f;

                _body1.AddFixture(shape);
            }

            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-0.5f, 0.6f) + _offset2;
                bd.AllowSleep = false;
                _body2 = BodyFactory.CreateFromDef(World, bd);

                CircleShape shape = new CircleShape(1.0f);
                shape.Radius = 0.5f;

                _body2.AddFixture(shape);
            }
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsKeyDown(Keys.A))
            {
                _body1.ApplyForce(new Vector2(-10.0f, 0.0f));
                _body2.ApplyForce(new Vector2(-10.0f, 0.0f));
            }

            if (keyboard.IsKeyDown(Keys.D))
            {
                _body1.ApplyForce(new Vector2(10.0f, 0.0f));
                _body2.ApplyForce(new Vector2(10.0f, 0.0f));
            }

            base.Keyboard(keyboard);
        }

        internal static Test Create()
        {
            return new EdgeTest();
        }
    }
}