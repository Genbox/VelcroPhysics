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

// A basic slider crank created for GDC tutorial: Understanding Constraints

using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Definitions.Joints;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class SliderCrank1Test : Test
    {
        private SliderCrank1Test()
        {
            Body ground;
            {
                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(0.0f, 17.0f);
                ground = BodyFactory.CreateFromDef(World, bd);
            }

            {
                Body prevBody = ground;

                // Define crank.
                {
                    PolygonShape shape = new PolygonShape(2.0f);
                    shape.SetAsBox(4.0f, 1.0f);

                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(-8.0f, 20.0f);
                    Body body = BodyFactory.CreateFromDef(World, bd);
                    body.AddFixture(shape);

                    RevoluteJointDef rjd = new RevoluteJointDef();
                    rjd.Initialize(prevBody, body, new Vector2(-12.0f, 20.0f));
                    JointFactory.CreateFromDef(World, rjd);

                    prevBody = body;
                }

                // Define connecting rod
                {
                    PolygonShape shape = new PolygonShape(2.0f);
                    shape.SetAsBox(8.0f, 1.0f);

                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(4.0f, 20.0f);
                    Body body = BodyFactory.CreateFromDef(World, bd);
                    body.AddFixture(shape);

                    RevoluteJointDef rjd = new RevoluteJointDef();
                    rjd.Initialize(prevBody, body, new Vector2(-4.0f, 20.0f));
                    JointFactory.CreateFromDef(World, rjd);

                    prevBody = body;
                }

                // Define piston
                {
                    PolygonShape shape = new PolygonShape(2.0f);
                    shape.SetAsBox(3.0f, 3.0f);

                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.FixedRotation = true;
                    bd.Position = new Vector2(12.0f, 20.0f);
                    Body body = BodyFactory.CreateFromDef(World, bd);
                    body.AddFixture(shape);

                    RevoluteJointDef rjd = new RevoluteJointDef();
                    rjd.Initialize(prevBody, body, new Vector2(12.0f, 20.0f));
                    JointFactory.CreateFromDef(World, rjd);

                    PrismaticJointDef pjd = new PrismaticJointDef();
                    pjd.Initialize(ground, body, new Vector2(12.0f, 17.0f), new Vector2(1.0f, 0.0f));
                    JointFactory.CreateFromDef(World, pjd);
                }
            }
        }

        internal static Test Create()
        {
            return new SliderCrank1Test();
        }
    }
}