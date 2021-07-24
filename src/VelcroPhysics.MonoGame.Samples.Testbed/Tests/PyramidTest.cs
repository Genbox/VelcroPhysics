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
    internal class PyramidTest : Test
    {
        private const int _count = 20;

        private PyramidTest()
        {
            {
                BodyDef bd = new BodyDef();
                Body ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape();
                shape.SetTwoSided(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.AddFixture(shape);
            }

            {
                float a = 0.5f;
                PolygonShape shape = new PolygonShape(5.0f);
                shape.SetAsBox(a, a);

                Vector2 x = new Vector2(-7.0f, 0.75f);
                Vector2 y;
                Vector2 deltaX = new Vector2(0.5625f, 1.25f);
                Vector2 deltaY = new Vector2(1.125f, 0.0f);

                for (int i = 0; i < _count; ++i)
                {
                    y = x;

                    for (int j = i; j < _count; ++j)
                    {
                        BodyDef bd = new BodyDef();
                        bd.Type = BodyType.Dynamic;
                        bd.Position = y;
                        Body body = BodyFactory.CreateFromDef(World, bd);
                        body.AddFixture(shape);

                        y += deltaY;
                    }

                    x += deltaX;
                }
            }
        }

        internal static Test Create()
        {
            return new PyramidTest();
        }
    }
}