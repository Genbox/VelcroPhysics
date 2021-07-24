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
    internal class PulleyJointTest : Test
    {
        private readonly PulleyJoint _joint1;

        private PulleyJointTest()
        {
            float y = 16.0f;
            float L = 12.0f;
            float a = 1.0f;
            float b = 2.0f;

            Body ground;
            {
                BodyDef bd = new BodyDef();
                ground = BodyFactory.CreateFromDef(World, bd);

                CircleShape circle = new CircleShape(0.0f);
                circle.Radius = 2.0f;

                circle.Position = new Vector2(-10.0f, y + b + L);
                ground.AddFixture(circle);

                circle.Position = new Vector2(10.0f, y + b + L);
                ground.AddFixture(circle);
            }

            {
                PolygonShape shape = new PolygonShape(5.0f);
                shape.SetAsBox(a, b);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;

                //bd.FixedRotation = true;
                bd.Position = new Vector2(-10.0f, y);
                Body body1 = BodyFactory.CreateFromDef(World, bd);
                body1.AddFixture(shape);

                bd.Position = new Vector2(10.0f, y);
                Body body2 = BodyFactory.CreateFromDef(World, bd);
                body2.AddFixture(shape);

                PulleyJointDef pulleyDef = new PulleyJointDef();
                Vector2 anchor1 = new Vector2(-10.0f, y + b);
                Vector2 anchor2 = new Vector2(10.0f, y + b);
                Vector2 groundAnchor1 = new Vector2(-10.0f, y + b + L);
                Vector2 groundAnchor2 = new Vector2(10.0f, y + b + L);
                pulleyDef.Initialize(body1, body2, groundAnchor1, groundAnchor2, anchor1, anchor2, 1.5f);

                _joint1 = (PulleyJoint)JointFactory.CreateFromDef(World, pulleyDef);
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            float ratio = _joint1.Ratio;
            float L = _joint1.CurrentLengthA + ratio * _joint1.CurrentLengthB;
            DrawString($"L1 + {ratio} * L2 = {L}");
        }

        internal static Test Create()
        {
            return new PulleyJointTest();
        }
    }
}