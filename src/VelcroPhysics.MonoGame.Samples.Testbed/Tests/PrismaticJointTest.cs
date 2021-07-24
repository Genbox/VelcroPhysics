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

// Test the prismatic joint with limits and motor options.

using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Definitions.Joints;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class PrismaticJointTest : Test
    {
        private readonly PrismaticJoint _joint;
        private readonly float _motorSpeed;
        private readonly bool _enableMotor;
        private readonly bool _enableLimit;

        private PrismaticJointTest()
        {
            Body ground;
            {
                BodyDef bd = new BodyDef();
                ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape();
                shape.SetTwoSided(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.AddFixture(shape);
            }

            _enableLimit = true;
            _enableMotor = false;
            _motorSpeed = 10.0f;

            {
                PolygonShape shape = new PolygonShape(5.0f);
                shape.SetAsBox(1.0f, 1.0f);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(0.0f, 10.0f);
                bd.Angle = 0.5f * MathConstants.Pi;
                bd.AllowSleep = false;
                Body body = BodyFactory.CreateFromDef(World, bd);
                body.AddFixture(shape);

                PrismaticJointDef pjd = new PrismaticJointDef();

                // Horizontal
                pjd.Initialize(ground, body, bd.Position, new Vector2(1.0f, 0.0f));

                pjd.MotorSpeed = _motorSpeed;
                pjd.MaxMotorForce = 10000.0f;
                pjd.EnableMotor = _enableMotor;
                pjd.LowerTranslation = -10.0f;
                pjd.UpperTranslation = 10.0f;
                pjd.EnableLimit = _enableLimit;

                _joint = (PrismaticJoint)JointFactory.CreateFromDef(World, pjd);
            }
        }

        //void UpdateUI()
        //{
        //	ImGui::SetNextWindowPos(ImVec2(10.0f, 100.0f));
        //	ImGui::SetNextWindowSize(ImVec2(200.0f, 100.0f));
        //	ImGui::Begin("Joint Controls", nullptr, ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoResize);

        //	if (ImGui::Checkbox("Limit", _enableLimit))
        //	{
        //		_joint.EnableLimit(_enableLimit);
        //	}

        //	if (ImGui::Checkbox("Motor", _enableMotor))
        //	{
        //		_joint.EnableMotor(_enableMotor);
        //	}

        //	if (ImGui::SliderFloat("Speed", _motorSpeed, -100.0f, 100.0f, "%.0f"))
        //	{
        //		_joint.SetMotorSpeed(_motorSpeed);
        //	}

        //	ImGui::End();
        //}

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            float force = _joint.GetMotorForce(settings.Hz);
            DrawString("Motor Force = " + force);
        }

        internal static Test Create()
        {
            return new PrismaticJointTest();
        }
    }
}