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
using Genbox.VelcroPhysics.Definitions.Joints;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class DominosTest : Test
    {
        private DominosTest()
        {
            Body b1;
            {
                EdgeShape shape = new EdgeShape();
                shape.SetTwoSided(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

                BodyDef bd = new BodyDef();
                b1 = BodyFactory.CreateFromDef(World, bd);
                b1.AddFixture(shape);
            }

            {
                PolygonShape shape = new PolygonShape(0.0f);
                shape.SetAsBox(6.0f, 0.25f);

                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(-1.5f, 10.0f);
                Body ground = BodyFactory.CreateFromDef(World, bd);
                ground.AddFixture(shape);
            }

            {
                PolygonShape shape = new PolygonShape(20.0f);
                shape.SetAsBox(0.1f, 1.0f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Friction = 0.1f;

                for (int i = 0; i < 10; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(-6.0f + 1.0f * i, 11.25f);
                    Body body = BodyFactory.CreateFromDef(World, bd);
                    body.AddFixture(fd);
                }
            }

            {
                PolygonShape shape = new PolygonShape(0.0f);
                shape.SetAsBox(7.0f, 0.25f, Vector2.Zero, 0.3f);

                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(1.0f, 6.0f);
                Body ground = BodyFactory.CreateFromDef(World, bd);
                ground.AddFixture(shape);
            }

            Body b2;
            {
                PolygonShape shape = new PolygonShape(0.02f);
                shape.SetAsBox(0.25f, 1.5f);

                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(-7.0f, 4.0f);
                b2 = BodyFactory.CreateFromDef(World, bd);
                b2.AddFixture(shape);
            }

            Body b3;
            {
                PolygonShape shape = new PolygonShape(10.0f);
                shape.SetAsBox(6.0f, 0.125f);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-0.9f, 1.0f);
                bd.Angle = -0.15f;

                b3 = BodyFactory.CreateFromDef(World, bd);
                b3.AddFixture(shape);
            }

            RevoluteJointDef jd = new RevoluteJointDef();
            Vector2 anchor;

            anchor = new Vector2(-2.0f, 1.0f);
            jd.Initialize(b1, b3, anchor);
            jd.CollideConnected = true;
            JointFactory.CreateFromDef(World, jd);

            Body b4;
            {
                PolygonShape shape = new PolygonShape(10.0f);
                shape.SetAsBox(0.25f, 0.25f);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-10.0f, 15.0f);
                b4 = BodyFactory.CreateFromDef(World, bd);
                b4.AddFixture(shape);
            }

            anchor = new Vector2(-7.0f, 15.0f);
            jd.Initialize(b2, b4, anchor);
            JointFactory.CreateFromDef(World, jd);

            Body b5;
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(6.5f, 3.0f);
                b5 = BodyFactory.CreateFromDef(World, bd);

                PolygonShape shape = new PolygonShape(10.0f);
                FixtureDef fd = new FixtureDef();

                fd.Shape = shape;
                fd.Friction = 0.1f;

                shape.SetAsBox(1.0f, 0.1f, new Vector2(0.0f, -0.9f), 0.0f);
                b5.AddFixture(fd);

                shape.SetAsBox(0.1f, 1.0f, new Vector2(-0.9f, 0.0f), 0.0f);
                b5.AddFixture(fd);

                shape.SetAsBox(0.1f, 1.0f, new Vector2(0.9f, 0.0f), 0.0f);
                b5.AddFixture(fd);
            }

            anchor = new Vector2(6.0f, 2.0f);
            jd.Initialize(b1, b5, anchor);
            JointFactory.CreateFromDef(World, jd);

            Body b6;
            {
                PolygonShape shape = new PolygonShape(30.0f);
                shape.SetAsBox(1.0f, 0.1f);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(6.5f, 4.1f);
                b6 = BodyFactory.CreateFromDef(World, bd);
                b6.AddFixture(shape);
            }

            anchor = new Vector2(7.5f, 4.0f);
            jd.Initialize(b5, b6, anchor);
            JointFactory.CreateFromDef(World, jd);

            Body b7;
            {
                PolygonShape shape = new PolygonShape(10.0f);
                shape.SetAsBox(0.1f, 1.0f);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(7.4f, 1.0f);

                b7 = BodyFactory.CreateFromDef(World, bd);
                b7.AddFixture(shape);
            }

            DistanceJointDef djd = new DistanceJointDef();
            djd.BodyA = b3;
            djd.BodyB = b7;
            djd.LocalAnchorA = new Vector2(6.0f, 0.0f);
            djd.LocalAnchorB = new Vector2(0.0f, -1.0f);
            Vector2 d = djd.BodyB.GetWorldPoint(djd.LocalAnchorB) - djd.BodyA.GetWorldPoint(djd.LocalAnchorA);
            djd.Length = d.Length();

            JointHelper.LinearStiffness(1.0f, 1.0f, djd.BodyA, djd.BodyB, out float stiffness, out float damping);
            djd.Stiffness = stiffness;
            djd.Damping = damping;

            JointFactory.CreateFromDef(World, djd);

            {
                float radius = 0.2f;

                CircleShape shape = new CircleShape(10.0f);
                shape.Radius = radius;

                for (int i = 0; i < 4; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(5.9f + 2.0f * radius * i, 2.4f);
                    Body body = BodyFactory.CreateFromDef(World, bd);
                    body.AddFixture(shape);
                }
            }
        }

        internal static Test Create()
        {
            return new DominosTest();
        }
    }
}