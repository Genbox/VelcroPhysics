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
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    /// <summary>This tests bullet collision and provides an example of a gameplay scenario. This also uses a loop shape.</summary>
    internal class PinballTest : Test
    {
        private readonly RevoluteJoint _leftJoint;
        private readonly RevoluteJoint _rightJoint;
        private readonly Body _ball;
        private bool _button;

        private PinballTest()
        {
            // Ground body
            Body ground;
            {
                BodyDef bd = new BodyDef();
                ground = BodyFactory.CreateFromDef(World, bd);

                Vertices vs = new Vertices(5);
                vs.Add(new Vector2(-8.0f, 6.0f));
                vs.Add(new Vector2(-8.0f, 20.0f));
                vs.Add(new Vector2(8.0f, 20.0f));
                vs.Add(new Vector2(8.0f, 6.0f));
                vs.Add(new Vector2(0.0f, -2.0f));

                ChainShape loop = new ChainShape(vs, true);
                FixtureDef fd = new FixtureDef();
                fd.Shape = loop;
                ground.AddFixture(fd);
            }

            // Flippers
            {
                Vector2 p1 = new Vector2(-2.0f, 0.0f), p2 = new Vector2(2.0f, 0.0f);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;

                bd.Position = p1;
                Body leftFlipper = BodyFactory.CreateFromDef(World, bd);

                bd.Position = p2;
                Body rightFlipper = BodyFactory.CreateFromDef(World, bd);

                PolygonShape box = new PolygonShape(1.0f);
                box.SetAsBox(1.75f, 0.1f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = box;

                leftFlipper.AddFixture(fd);
                rightFlipper.AddFixture(fd);

                RevoluteJointDef jd = new RevoluteJointDef();
                jd.BodyA = ground;
                jd.LocalAnchorB = Vector2.Zero;
                jd.EnableMotor = true;
                jd.MaxMotorTorque = 1000.0f;
                jd.EnableLimit = true;

                jd.MotorSpeed = 0.0f;
                jd.LocalAnchorA = p1;
                jd.BodyB = leftFlipper;
                jd.LowerAngle = -30.0f * MathConstants.Pi / 180.0f;
                jd.UpperAngle = 5.0f * MathConstants.Pi / 180.0f;
                _leftJoint = (RevoluteJoint)JointFactory.CreateFromDef(World, jd);

                jd.MotorSpeed = 0.0f;
                jd.LocalAnchorA = p2;
                jd.BodyB = rightFlipper;
                jd.LowerAngle = -5.0f * MathConstants.Pi / 180.0f;
                jd.UpperAngle = 30.0f * MathConstants.Pi / 180.0f;
                _rightJoint = (RevoluteJoint)JointFactory.CreateFromDef(World, jd);
            }

            // Circle character
            {
                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(1.0f, 15.0f);
                bd.Type = BodyType.Dynamic;
                bd.IsBullet = true;

                _ball = BodyFactory.CreateFromDef(World, bd);

                CircleShape shape = new CircleShape(1.0f);
                shape.Radius = 0.2f;

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                _ball.AddFixture(fd);
            }

            _button = false;
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            if (_button)
            {
                _leftJoint.MotorSpeed = 20.0f;
                _rightJoint.MotorSpeed = -20.0f;
            }
            else
            {
                _leftJoint.MotorSpeed = -10.0f;
                _rightJoint.MotorSpeed = 10.0f;
            }

            base.Update(settings, gameTime);

            DrawString("Press 'a' to control the flippers");
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsKeyDown(Keys.A))
                _button = true;
            else
                _button = false;

            base.Keyboard(keyboard);
        }

        internal static Test Create()
        {
            return new PinballTest();
        }
    }
}