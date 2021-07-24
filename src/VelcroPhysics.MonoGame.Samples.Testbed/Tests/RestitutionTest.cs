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

// Note: even with a restitution of 1.0, there is some energy change
// due to position correction.

using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    public class RestitutionTest : Test
    {
        private RestitutionTest()
        {
            const float threshold = 10.0f;

            {
                BodyDef bd = new BodyDef();
                Body ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape();
                shape.SetTwoSided(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.RestitutionThreshold = threshold;
                ground.AddFixture(fd);
            }

            {
                CircleShape shape = new CircleShape(1.0f);
                shape.Radius = 1.0f;

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;

                float[] restitution = { 0.0f, 0.1f, 0.3f, 0.5f, 0.75f, 0.9f, 1.0f };

                for (int i = 0; i < 7; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(-10.0f + 3.0f * i, 20.0f);

                    Body body = BodyFactory.CreateFromDef(World, bd);

                    fd.Restitution = restitution[i];
                    fd.RestitutionThreshold = threshold;
                    body.AddFixture(fd);
                }
            }
        }

        public static Test Create()
        {
            return new RestitutionTest();
        }
    }
}