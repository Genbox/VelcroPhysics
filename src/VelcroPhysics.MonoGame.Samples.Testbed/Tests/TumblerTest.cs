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
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class TumblerTest : Test
    {
        private const int _eCount = 800;
        private RevoluteJoint _joint;
        private int _count2;

        private TumblerTest()
        {
            Body ground;
            {
                BodyDef bd = new BodyDef();
                ground = BodyFactory.CreateFromDef(World, bd);
            }

            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.AllowSleep = false;
                bd.Position = new Vector2(0.0f, 10.0f);
                Body body = BodyFactory.CreateFromDef(World, bd);

                PolygonShape shape = new PolygonShape(5.0f);
                shape.SetAsBox(0.5f, 10.0f, new Vector2(10.0f, 0.0f), 0.0f);
                body.AddFixture(shape);
                shape.SetAsBox(0.5f, 10.0f, new Vector2(-10.0f, 0.0f), 0.0f);
                body.AddFixture(shape);
                shape.SetAsBox(10.0f, 0.5f, new Vector2(0.0f, 10.0f), 0.0f);
                body.AddFixture(shape);
                shape.SetAsBox(10.0f, 0.5f, new Vector2(0.0f, -10.0f), 0.0f);
                body.AddFixture(shape);

                RevoluteJointDef jd = new RevoluteJointDef();
                jd.BodyA = ground;
                jd.BodyB = body;
                jd.LocalAnchorA = new Vector2(0.0f, 10.0f);
                jd.LocalAnchorB = new Vector2(0.0f, 0.0f);
                jd.ReferenceAngle = 0.0f;
                jd.MotorSpeed = 0.05f * MathConstants.Pi;
                jd.MaxMotorTorque = 1e8f;
                jd.EnableMotor = true;
                _joint = (RevoluteJoint)JointFactory.CreateFromDef(World, jd);
            }

            _count2 = 0;
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            if (_count2 < _eCount)
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(0.0f, 10.0f);
                Body body = BodyFactory.CreateFromDef(World, bd);

                PolygonShape shape = new PolygonShape(1.0f);
                shape.SetAsBox(0.125f, 0.125f);
                body.AddFixture(shape);

                ++_count2;
            }
        }

        internal static Test Create()
        {
            return new TumblerTest();
        }
    }
}