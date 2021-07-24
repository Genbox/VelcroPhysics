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

// A motor driven slider crank with joint friction.

using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Definitions.Joints;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class SliderCrank2Test : Test
    {
        private readonly RevoluteJoint _joint1;
        private readonly PrismaticJoint _joint2;

        private SliderCrank2Test()
        {
            Body ground;
            {
                BodyDef bd = new BodyDef();
                ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape();
                shape.SetTwoSided(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.AddFixture(shape);
            }

            {
                Body prevBody = ground;

                // Define crank.
                {
                    PolygonShape shape = new PolygonShape(2.0f);
                    shape.SetAsBox(0.5f, 2.0f);

                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(0.0f, 7.0f);
                    Body body = BodyFactory.CreateFromDef(World, bd);
                    body.AddFixture(shape);

                    RevoluteJointDef rjd = new RevoluteJointDef();
                    rjd.Initialize(prevBody, body, new Vector2(0.0f, 5.0f));
                    rjd.MotorSpeed = 1.0f * MathConstants.Pi;
                    rjd.MaxMotorTorque = 10000.0f;
                    rjd.EnableMotor = true;
                    _joint1 = (RevoluteJoint)JointFactory.CreateFromDef(World, rjd);

                    prevBody = body;
                }

                // Define follower.
                {
                    PolygonShape shape = new PolygonShape(2.0f);
                    shape.SetAsBox(0.5f, 4.0f);

                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(0.0f, 13.0f);
                    Body body = BodyFactory.CreateFromDef(World, bd);
                    body.AddFixture(shape);

                    RevoluteJointDef rjd = new RevoluteJointDef();
                    rjd.Initialize(prevBody, body, new Vector2(0.0f, 9.0f));
                    rjd.EnableMotor = false;
                    JointFactory.CreateFromDef(World, rjd);

                    prevBody = body;
                }

                // Define piston
                {
                    PolygonShape shape = new PolygonShape(2.0f);
                    shape.SetAsBox(1.5f, 1.5f);

                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.FixedRotation = true;
                    bd.Position = new Vector2(0.0f, 17.0f);
                    Body body = BodyFactory.CreateFromDef(World, bd);
                    body.AddFixture(shape);

                    RevoluteJointDef rjd = new RevoluteJointDef();
                    rjd.Initialize(prevBody, body, new Vector2(0.0f, 17.0f));
                    JointFactory.CreateFromDef(World, rjd);

                    PrismaticJointDef pjd = new PrismaticJointDef();
                    pjd.Initialize(ground, body, new Vector2(0.0f, 17.0f), new Vector2(0.0f, 1.0f));

                    pjd.MaxMotorForce = 1000.0f;
                    pjd.EnableMotor = true;

                    _joint2 = (PrismaticJoint)JointFactory.CreateFromDef(World, pjd);
                }

                // Create a payload
                {
                    PolygonShape shape = new PolygonShape(2.0f);
                    shape.SetAsBox(1.5f, 1.5f);

                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(0.0f, 23.0f);
                    Body body = BodyFactory.CreateFromDef(World, bd);
                    body.AddFixture(shape);
                }
            }
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsNewKeyPress(Keys.F))
            {
                _joint2.MotorEnabled = !_joint2.MotorEnabled;
                _joint2.BodyB.Awake = true;
            }
            else if (keyboard.IsNewKeyPress(Keys.M))
            {
                _joint1.MotorEnabled = !_joint1.MotorEnabled;
                _joint1.BodyB.Awake = true;
            }

            base.Keyboard(keyboard);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            DrawString("Keys: (f) toggle friction, (m) toggle motor");
            float torque = _joint1.GetMotorTorque(settings.Hz);
            DrawString("Motor Torque = " + torque);
        }

        internal static Test Create()
        {
            return new SliderCrank2Test();
        }
    }
}