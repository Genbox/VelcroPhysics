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
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    /// <summary>This test shows how to use a motor joint. A motor joint can be used to animate a dynamic body. With finite
    /// motor forces the body can be blocked by collision with other bodies.</summary>
    internal class MotorJointTest : Test
    {
        private readonly MotorJoint _joint;
        private float _time;
        private bool _go;

        private MotorJointTest()
        {
            Body ground;
            {
                BodyDef bd = new BodyDef();
                ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape();
                shape.SetTwoSided(new Vector2(-20.0f, 0.0f), new Vector2(20.0f, 0.0f));

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;

                ground.AddFixture(fd);
            }

            // Define motorized body
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(0.0f, 8.0f);
                Body body = BodyFactory.CreateFromDef(World, bd);

                PolygonShape shape = new PolygonShape(2.0f);
                shape.SetAsBox(2.0f, 0.5f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Friction = 0.6f;
                body.AddFixture(fd);

                MotorJointDef mjd = new MotorJointDef();
                mjd.Initialize(ground, body);
                mjd.MaxForce = 1000.0f;
                mjd.MaxTorque = 1000.0f;
                _joint = (MotorJoint)JointFactory.CreateFromDef(World, mjd);
            }

            _go = false;
            _time = 0.0f;
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsNewKeyPress(Keys.S))
                _go = !_go;

            base.Keyboard(keyboard);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            if (_go && settings.Hz > 0.0f)
                _time += 1.0f / settings.Hz;

            Vector2 linearOffset = new Vector2();
            linearOffset.X = 6.0f * MathUtils.Sinf(2.0f * _time);
            linearOffset.Y = 8.0f + 4.0f * MathUtils.Sinf(1.0f * _time);

            float angularOffset = 4.0f * _time;

            _joint.LinearOffset = linearOffset;
            _joint.AngularOffset = angularOffset;

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
            DebugView.DrawPoint(linearOffset, 4.0f, new Color(0.9f, 0.9f, 0.9f));
            DebugView.EndCustomDraw();

            DrawString("Keys: (s) pause");

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new MotorJointTest();
        }
    }
}