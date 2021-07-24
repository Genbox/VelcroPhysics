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

// Inspired by a contribution from roman_m
// Dimensions scooped from APE (http://www.cove.org/ape/index.htm)

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
    internal class TheoJansenTest : Test
    {
        private readonly Vector2 _offset;
        private readonly Body _chassis;
        private readonly Body _wheel;
        private readonly RevoluteJoint _motorJoint;
        private readonly bool _motorOn;
        private readonly float _motorSpeed;

        private void CreateLeg(float s, Vector2 wheelAnchor)
        {
            Vector2 p1 = new Vector2(5.4f * s, -6.1f);
            Vector2 p2 = new Vector2(7.2f * s, -1.2f);
            Vector2 p3 = new Vector2(4.3f * s, -1.9f);
            Vector2 p4 = new Vector2(3.1f * s, 0.8f);
            Vector2 p5 = new Vector2(6.0f * s, 1.5f);
            Vector2 p6 = new Vector2(2.5f * s, 3.7f);

            FixtureDef fd1 = new FixtureDef(), fd2 = new FixtureDef();
            fd1.Filter.Group = -1;
            fd2.Filter.Group = -1;

            PolygonShape poly1 = new PolygonShape(1.0f), poly2 = new PolygonShape(1.0f);

            if (s > 0.0f)
            {
                Vertices vertices = new Vertices(3);

                vertices.Add(p1);
                vertices.Add(p2);
                vertices.Add(p3);
                poly1.Vertices = vertices;

                vertices[0] = Vector2.Zero;
                vertices[1] = p5 - p4;
                vertices[2] = p6 - p4;
                poly2.Vertices = vertices;
            }
            else
            {
                Vertices vertices = new Vertices(3);

                vertices.Add(p1);
                vertices.Add(p3);
                vertices.Add(p2);
                poly1.Vertices = vertices;

                vertices[0] = Vector2.Zero;
                vertices[1] = p6 - p4;
                vertices[2] = p5 - p4;
                poly2.Vertices = vertices;
            }

            fd1.Shape = poly1;
            fd2.Shape = poly2;

            BodyDef bd1 = new BodyDef(), bd2 = new BodyDef();
            bd1.Type = BodyType.Dynamic;
            bd2.Type = BodyType.Dynamic;
            bd1.Position = _offset;
            bd2.Position = p4 + _offset;

            bd1.AngularDamping = 10.0f;
            bd2.AngularDamping = 10.0f;

            Body body1 = BodyFactory.CreateFromDef(World, bd1);
            Body body2 = BodyFactory.CreateFromDef(World, bd2);

            body1.AddFixture(fd1);
            body2.AddFixture(fd2);

            {
                DistanceJointDef jd = new DistanceJointDef();

                // Using a soft distance constraint can reduce some jitter.
                // It also makes the structure seem a bit more fluid by
                // acting like a suspension system.
                float dampingRatio = 0.5f;
                float frequencyHz = 10.0f;

                jd.Initialize(body1, body2, p2 + _offset, p5 + _offset);
                JointHelper.LinearStiffness(frequencyHz, dampingRatio, jd.BodyA, jd.BodyB, out float stiffness, out float damping);
                jd.Stiffness = stiffness;
                jd.Damping = damping;
                JointFactory.CreateFromDef(World, jd);

                jd.Initialize(body1, body2, p3 + _offset, p4 + _offset);
                JointHelper.LinearStiffness(frequencyHz, dampingRatio, jd.BodyA, jd.BodyB, out stiffness, out damping);
                jd.Stiffness = stiffness;
                jd.Damping = damping;
                JointFactory.CreateFromDef(World, jd);

                jd.Initialize(body1, _wheel, p3 + _offset, wheelAnchor + _offset);
                JointHelper.LinearStiffness(frequencyHz, dampingRatio, jd.BodyA, jd.BodyB, out stiffness, out damping);
                jd.Stiffness = stiffness;
                jd.Damping = damping;
                JointFactory.CreateFromDef(World, jd);

                jd.Initialize(body2, _wheel, p6 + _offset, wheelAnchor + _offset);
                JointHelper.LinearStiffness(frequencyHz, dampingRatio, jd.BodyA, jd.BodyB, out stiffness, out damping);
                jd.Stiffness = stiffness;
                jd.Damping = damping;
                JointFactory.CreateFromDef(World, jd);
            }

            {
                RevoluteJointDef jd = new RevoluteJointDef();
                jd.Initialize(body2, _chassis, p4 + _offset);
                JointFactory.CreateFromDef(World, jd);
            }
        }

        private TheoJansenTest()
        {
            _offset = new Vector2(0.0f, 8.0f);
            _motorSpeed = 2.0f;
            _motorOn = true;
            Vector2 pivot = new Vector2(0.0f, 0.8f);

            // Ground
            {
                BodyDef bd = new BodyDef();
                Body ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape();
                shape.SetTwoSided(new Vector2(-50.0f, 0.0f), new Vector2(50.0f, 0.0f));
                ground.AddFixture(shape);

                shape.SetTwoSided(new Vector2(-50.0f, 0.0f), new Vector2(-50.0f, 10.0f));
                ground.AddFixture(shape);

                shape.SetTwoSided(new Vector2(50.0f, 0.0f), new Vector2(50.0f, 10.0f));
                ground.AddFixture(shape);
            }

            // Balls
            for (int i = 0; i < 40; ++i)
            {
                CircleShape shape = new CircleShape(1.0f);
                shape.Radius = 0.25f;

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-40.0f + 2.0f * i, 0.5f);

                Body body = BodyFactory.CreateFromDef(World, bd);
                body.AddFixture(shape);
            }

            // Chassis
            {
                PolygonShape shape = new PolygonShape(1.0f);
                shape.SetAsBox(2.5f, 1.0f);

                FixtureDef sd = new FixtureDef();
                sd.Shape = shape;
                sd.Filter.Group = -1;
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = pivot + _offset;
                _chassis = BodyFactory.CreateFromDef(World, bd);
                _chassis.AddFixture(sd);
            }

            {
                CircleShape shape = new CircleShape(1.0f);
                shape.Radius = 1.6f;

                FixtureDef sd = new FixtureDef();
                sd.Shape = shape;
                sd.Filter.Group = -1;
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = pivot + _offset;
                _wheel = BodyFactory.CreateFromDef(World, bd);
                _wheel.AddFixture(sd);
            }

            {
                RevoluteJointDef jd = new RevoluteJointDef();
                jd.Initialize(_wheel, _chassis, pivot + _offset);
                jd.CollideConnected = false;
                jd.MotorSpeed = _motorSpeed;
                jd.MaxMotorTorque = 400.0f;
                jd.EnableMotor = _motorOn;
                _motorJoint = (RevoluteJoint)JointFactory.CreateFromDef(World, jd);
            }

            Vector2 wheelAnchor;

            wheelAnchor = pivot + new Vector2(0.0f, -0.8f);

            CreateLeg(-1.0f, wheelAnchor);
            CreateLeg(1.0f, wheelAnchor);

            _wheel.SetTransform(_wheel.Position, 120.0f * MathConstants.Pi / 180.0f);
            CreateLeg(-1.0f, wheelAnchor);
            CreateLeg(1.0f, wheelAnchor);

            _wheel.SetTransform(_wheel.Position, -120.0f * MathConstants.Pi / 180.0f);
            CreateLeg(-1.0f, wheelAnchor);
            CreateLeg(1.0f, wheelAnchor);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("Keys: left = a, brake = s, right = d, toggle motor = m");

            base.Update(settings, gameTime);
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsNewKeyPress(Keys.A))
                _motorJoint.MotorSpeed = -_motorSpeed;
            else if (keyboard.IsNewKeyPress(Keys.S))
                _motorJoint.MotorSpeed = 0.0f;
            else if (keyboard.IsNewKeyPress(Keys.D))
                _motorJoint.MotorSpeed = _motorSpeed;
            else if (keyboard.IsNewKeyPress(Keys.M))
                _motorJoint.MotorEnabled = !_motorJoint.MotorEnabled;

            base.Keyboard(keyboard);
        }

        internal static Test Create()
        {
            return new TheoJansenTest();
        }
    }
}