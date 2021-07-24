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
using Genbox.VelcroPhysics.Dynamics.Joints;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class GearJointTest : Test
    {
        private readonly RevoluteJoint _joint1;
        private readonly RevoluteJoint _joint2;
        private readonly PrismaticJoint _joint3;
        private readonly GearJoint _joint4;
        private readonly GearJoint _joint5;

        private GearJointTest()
        {
            Body ground;
            {
                BodyDef bd = new BodyDef();
                ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape();
                shape.SetTwoSided(new Vector2(50.0f, 0.0f), new Vector2(-50.0f, 0.0f));
                ground.AddFixture(shape);
            }

            {
                CircleShape circle1 = new CircleShape(5.0f);
                circle1.Radius = 1.0f;

                PolygonShape box = new PolygonShape(5.0f);
                box.SetAsBox(0.5f, 5.0f);

                CircleShape circle2 = new CircleShape(5.0f);
                circle2.Radius = 2.0f;

                BodyDef bd1 = new BodyDef();
                bd1.Type = BodyType.Static;
                bd1.Position = new Vector2(10.0f, 9.0f);
                Body body1 = BodyFactory.CreateFromDef(World, bd1);
                body1.AddFixture(circle1);

                BodyDef bd2 = new BodyDef();
                bd2.Type = BodyType.Dynamic;
                bd2.Position = new Vector2(10.0f, 8.0f);
                Body body2 =BodyFactory.CreateFromDef(World, bd2);
                body2.AddFixture(box);

                BodyDef bd3 = new BodyDef();
                bd3.Type = BodyType.Dynamic;
                bd3.Position = new Vector2(10.0f, 6.0f);
                Body body3 = BodyFactory.CreateFromDef(World, bd3);
                body3.AddFixture(circle2);

                RevoluteJointDef jd1 = new RevoluteJointDef();
                jd1.Initialize(body1, body2, bd1.Position);
                Joint joint1 = JointFactory.CreateFromDef(World, jd1);

                RevoluteJointDef jd2 = new RevoluteJointDef();
                jd2.Initialize(body2, body3, bd3.Position);
                Joint joint2 = JointFactory.CreateFromDef(World, jd2);

                GearJointDef jd4 = new GearJointDef();
                jd4.BodyA = body1;
                jd4.BodyB = body3;
                jd4.JointA = joint1;
                jd4.JointB = joint2;
                jd4.Ratio = circle2.Radius / circle1.Radius;
                JointFactory.CreateFromDef(World, jd4);
            }

            {
                CircleShape circle1 = new CircleShape(5.0f);
                circle1.Radius = 1.0f;

                CircleShape circle2 = new CircleShape(5.0f);
                circle2.Radius = 2.0f;

                PolygonShape box = new PolygonShape(5.0f);
                box.SetAsBox(0.5f, 5.0f);

                BodyDef bd1 = new BodyDef();
                bd1.Type = BodyType.Dynamic;
                bd1.Position = new Vector2(-3.0f, 12.0f);
                Body body1 = BodyFactory.CreateFromDef(World, bd1);
                body1.AddFixture(circle1);

                RevoluteJointDef jd1 = new RevoluteJointDef();
                jd1.BodyA = ground;
                jd1.BodyB = body1;
                jd1.LocalAnchorA = ground.GetLocalPoint(bd1.Position);
                jd1.LocalAnchorB = body1.GetLocalPoint(bd1.Position);
                jd1.ReferenceAngle = body1.Rotation - ground.Rotation;
                _joint1 = (RevoluteJoint)JointFactory.CreateFromDef(World, jd1);

                BodyDef bd2 = new BodyDef();
                bd2.Type = BodyType.Dynamic;
                bd2.Position = new Vector2(0.0f, 12.0f);
                Body body2 = BodyFactory.CreateFromDef(World, bd2);
                body2.AddFixture(circle2);

                RevoluteJointDef jd2 = new RevoluteJointDef();
                jd2.Initialize(ground, body2, bd2.Position);
                _joint2 = (RevoluteJoint)JointFactory.CreateFromDef(World, jd2);

                BodyDef bd3 = new BodyDef();
                bd3.Type = BodyType.Dynamic;
                bd3.Position = new Vector2(2.5f, 12.0f);
                Body body3 = BodyFactory.CreateFromDef(World, bd3);
                body3.AddFixture(box);

                PrismaticJointDef jd3 = new PrismaticJointDef();
                jd3.Initialize(ground, body3, bd3.Position, new Vector2(0.0f, 1.0f));
                jd3.LowerTranslation = -5.0f;
                jd3.UpperTranslation = 5.0f;
                jd3.EnableLimit = true;

                _joint3 = (PrismaticJoint)JointFactory.CreateFromDef(World, jd3);

                GearJointDef jd4 = new GearJointDef();
                jd4.BodyA = body1;
                jd4.BodyB = body2;
                jd4.JointA = _joint1;
                jd4.JointB = _joint2;
                jd4.Ratio = circle2.Radius / circle1.Radius;
                _joint4 = (GearJoint)JointFactory.CreateFromDef(World, jd4);

                GearJointDef jd5 = new GearJointDef();
                jd5.BodyA = body2;
                jd5.BodyB = body3;
                jd5.JointA = _joint2;
                jd5.JointB = _joint3;
                jd5.Ratio = -1.0f / circle2.Radius;
                _joint5 = (GearJoint)JointFactory.CreateFromDef(World, jd5);
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            float ratio, value;

            ratio = _joint4.Ratio;
            value = _joint1.JointAngle + ratio * _joint2.JointAngle;
            DrawString($"theta1 + {ratio} * theta2 = {value}");

            ratio = _joint5.Ratio;
            value = _joint2.JointAngle + ratio * _joint3.JointTranslation;
            DrawString($"theta2 + {ratio} * delta = {value}");
        }

        internal static Test Create()
        {
            return new GearJointTest();
        }
    }
}