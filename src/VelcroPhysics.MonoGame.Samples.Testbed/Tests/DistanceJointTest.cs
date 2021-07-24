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

// This tests distance joints, body destruction, and joint destruction.

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
    internal class DistanceJointTest : Test
    {
        private DistanceJoint _joint;
        private readonly float _length;
        private float _minLength;
        private float _maxLength;
        private readonly float _hertz;
        private readonly float _dampingRatio;

        private DistanceJointTest()
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
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.AngularDamping = 0.1f;

                bd.Position = new Vector2(0.0f, 5.0f);
                Body body = BodyFactory.CreateFromDef(World, bd);

                PolygonShape shape = new PolygonShape(5.0f);
                shape.SetAsBox(0.5f, 0.5f);
                body.AddFixture(shape);

                _hertz = 1.0f;
                _dampingRatio = 0.7f;

                DistanceJointDef jd = new DistanceJointDef();
                jd.Initialize(ground, body, new Vector2(0.0f, 15.0f), bd.Position);
                jd.CollideConnected = true;
                _length = jd.Length;
                _minLength = _length;
                _maxLength = _length;
                JointHelper.LinearStiffness(_hertz, _dampingRatio, jd.BodyA, jd.BodyB, out float stiffness, out float damping);
                jd.Stiffness = stiffness;
                jd.Damping = damping;

                _joint = (DistanceJoint)JointFactory.CreateFromDef(World, jd);
            }
        }

        //private void UpdateUI()
        //{
        //    if (ImGui::SliderFloat("Length", _length, 0.0f, 20.0f, "%.0f"))
        //    {
        //        _length = _joint.SetLength(_length);
        //    }

        //    if (ImGui::SliderFloat("Min Length", _minLength, 0.0f, 20.0f, "%.0f"))
        //    {
        //        _minLength = _joint.SetMinLength(_minLength);
        //    }

        //    if (ImGui::SliderFloat("Max Length", _maxLength, 0.0f, 20.0f, "%.0f"))
        //    {
        //        _maxLength = _joint.SetMaxLength(_maxLength);
        //    }

        //    if (ImGui::SliderFloat("Hertz", _hertz, 0.0f, 10.0f, "%.1f"))
        //    {
        //        float stiffness;
        //        float damping;
        //        b2LinearStiffness(stiffness, damping, _hertz, _dampingRatio, _joint.GetBodyA(), _joint.GetBodyB());
        //        _joint.SetStiffness(stiffness);
        //        _joint.SetDamping(damping);
        //    }

        //    if (ImGui::SliderFloat("Damping Ratio", _dampingRatio, 0.0f, 2.0f, "%.1f"))
        //    {
        //        float stiffness;
        //        float damping;
        //        b2LinearStiffness(stiffness, damping, _hertz, _dampingRatio, _joint.GetBodyA(), _joint.GetBodyB());
        //        _joint.SetStiffness(stiffness);
        //        _joint.SetDamping(damping);
        //    }
        //}

        internal static Test Create()
        {
            return new DistanceJointTest();
        }
    }
}