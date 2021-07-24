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

using Genbox.VelcroPhysics.Collision.Filtering;
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
    internal class RevoluteJointTest : Test
    {
        private readonly Body _ball;
        private readonly RevoluteJoint _joint1;
        private readonly RevoluteJoint _joint2;
        private readonly float _motorSpeed;
        private readonly bool _enableMotor;
        private readonly bool _enableLimit;

        private RevoluteJointTest()
        {
            Body ground;
            {
                BodyDef bd = new BodyDef();
                ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape();
                shape.SetTwoSided(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;

                //fd.Filter.Category = 2;

                ground.AddFixture(fd);
            }

            _enableLimit = true;
            _enableMotor = false;
            _motorSpeed = 1.0f;

            {
                PolygonShape shape = new PolygonShape(5.0f);
                shape.SetAsBox(0.25f, 3.0f, new Vector2(0.0f, 3.0f), 0.0f);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-10.0f, 20.0f);
                Body body = BodyFactory.CreateFromDef(World, bd);
                body.AddFixture(shape);

                RevoluteJointDef jd = new RevoluteJointDef();
                jd.Initialize(ground, body, new Vector2(-10.0f, 20.5f));
                jd.MotorSpeed = _motorSpeed;
                jd.MaxMotorTorque = 10000.0f;
                jd.EnableMotor = _enableMotor;
                jd.LowerAngle = -0.25f * MathConstants.Pi;
                jd.UpperAngle = 0.5f * MathConstants.Pi;
                jd.EnableLimit = _enableLimit;

                _joint1 = (RevoluteJoint)JointFactory.CreateFromDef(World, jd);
            }

            {
                CircleShape circle_shape = new CircleShape(5.0f);
                circle_shape.Radius = 2.0f;

                BodyDef circle_bd = new BodyDef();
                circle_bd.Type = BodyType.Dynamic;
                circle_bd.Position = new Vector2(5.0f, 30.0f);

                FixtureDef fd = new FixtureDef();
                fd.Filter.CategoryMask = Category.Cat1;
                fd.Shape = circle_shape;

                _ball = BodyFactory.CreateFromDef(World, circle_bd);
                _ball.AddFixture(fd);

                PolygonShape polygon_shape = new PolygonShape(2.0f);
                polygon_shape.SetAsBox(10.0f, 0.5f, new Vector2(-10.0f, 0.0f), 0.0f);

                BodyDef polygon_bd = new BodyDef();
                polygon_bd.Position = new Vector2(20.0f, 10.0f);
                polygon_bd.Type = BodyType.Dynamic;
                polygon_bd.IsBullet = true;
                Body polygon_body = BodyFactory.CreateFromDef(World, polygon_bd);
                polygon_body.AddFixture(polygon_shape);

                RevoluteJointDef jd = new RevoluteJointDef();
                jd.Initialize(ground, polygon_body, new Vector2(19.0f, 10.0f));
                jd.LowerAngle = -0.25f * MathConstants.Pi;
                jd.UpperAngle = 0.0f * MathConstants.Pi;
                jd.EnableLimit = true;
                jd.EnableMotor = true;
                jd.MotorSpeed = 0.0f;
                jd.MaxMotorTorque = 10000.0f;

                _joint2 = (RevoluteJoint)JointFactory.CreateFromDef(World, jd);
            }
        }

        //void UpdateUI()
        //{
        //	ImGui::SetNextWindowPos(ImVec2(10.0f, 100.0f));
        //	ImGui::SetNextWindowSize(ImVec2(200.0f, 100.0f));
        //	ImGui::Begin("Joint Controls", nullptr, ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoResize);

        //	if (ImGui::Checkbox("Limit", _enableLimit))
        //	{
        //		_joint1.EnableLimit(_enableLimit);
        //	}

        //	if (ImGui::Checkbox("Motor", _enableMotor))
        //	{
        //		_joint1.EnableMotor(_enableMotor);
        //	}

        //	if (ImGui::SliderFloat("Speed", _motorSpeed, -20.0f, 20.0f, "%.0f"))
        //	{
        //		_joint1.SetMotorSpeed(_motorSpeed);
        //	}

        //	ImGui::End();
        //}

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            float torque1 = _joint1.GetMotorTorque(settings.Hz);
            DrawString("Motor Torque 1= " + torque1);

            float torque2 = _joint2.GetMotorTorque(settings.Hz);
            DrawString("Motor Torque 2= " + torque2);
        }

        internal static Test Create()
        {
            return new RevoluteJointTest();
        }
    }
}