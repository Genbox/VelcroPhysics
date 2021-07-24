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
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class FrictionTest : Test
    {
        private FrictionTest()
        {
            {
                BodyDef bd = new BodyDef();
                Body ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape();
                shape.SetTwoSided(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.AddFixture(shape);
            }

            {
                PolygonShape shape = new PolygonShape(0.0f);
                shape.SetAsBox(13.0f, 0.25f);

                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(-4.0f, 22.0f);
                bd.Angle = -0.25f;

                Body ground = BodyFactory.CreateFromDef(World, bd);
                ground.AddFixture(shape);
            }

            {
                PolygonShape shape = new PolygonShape(0.0f);
                shape.SetAsBox(0.25f, 1.0f);

                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(10.5f, 19.0f);

                Body ground = BodyFactory.CreateFromDef(World, bd);
                ground.AddFixture(shape);
            }

            {
                PolygonShape shape = new PolygonShape(0.0f);
                shape.SetAsBox(13.0f, 0.25f);

                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(4.0f, 14.0f);
                bd.Angle = 0.25f;

                Body ground = BodyFactory.CreateFromDef(World, bd);
                ground.AddFixture(shape);
            }

            {
                PolygonShape shape = new PolygonShape(0.0f);
                shape.SetAsBox(0.25f, 1.0f);

                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(-10.5f, 11.0f);

                Body ground = BodyFactory.CreateFromDef(World, bd);
                ground.AddFixture(shape);
            }

            {
                PolygonShape shape = new PolygonShape(0.0f);
                shape.SetAsBox(13.0f, 0.25f);

                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(-4.0f, 6.0f);
                bd.Angle = -0.25f;

                Body ground = BodyFactory.CreateFromDef(World, bd);
                ground.AddFixture(shape);
            }

            {
                PolygonShape shape = new PolygonShape(25.0f);
                shape.SetAsBox(0.5f, 0.5f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;

                float[] friction = { 0.75f, 0.5f, 0.35f, 0.1f, 0.0f };

                for (int i = 0; i < 5; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(-15.0f + 4.0f * i, 28.0f);
                    Body body = BodyFactory.CreateFromDef(World, bd);

                    fd.Friction = friction[i];
                    body.AddFixture(fd);
                }
            }
        }

        internal static Test Create()
        {
            return new FrictionTest();
        }
    }
}