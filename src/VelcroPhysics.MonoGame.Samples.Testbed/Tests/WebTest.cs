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

// Test distance joints, body destruction, and joint destruction.

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
    internal class WebTest : Test
    {
        private readonly Body[] _bodies = new Body[4];
        private readonly Joint[] _joints = new Joint[8];

        private WebTest()
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
                PolygonShape shape = new PolygonShape(5.0f);
                shape.SetAsBox(0.5f, 0.5f);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;

                bd.Position = new Vector2(-5.0f, 5.0f);
                _bodies[0] = BodyFactory.CreateFromDef(World, bd);
                _bodies[0].AddFixture(shape);

                bd.Position = new Vector2(5.0f, 5.0f);
                _bodies[1] = BodyFactory.CreateFromDef(World, bd);
                _bodies[1].AddFixture(shape);

                bd.Position = new Vector2(5.0f, 15.0f);
                _bodies[2] = BodyFactory.CreateFromDef(World, bd);
                _bodies[2].AddFixture(shape);

                bd.Position = new Vector2(-5.0f, 15.0f);
                _bodies[3] = BodyFactory.CreateFromDef(World, bd);
                _bodies[3].AddFixture(shape);

                DistanceJointDef jd = new DistanceJointDef();
                Vector2 p1, p2, d;

                float frequencyHz = 2.0f;
                float dampingRatio = 0.0f;

                jd.BodyA = ground;
                jd.BodyB = _bodies[0];
                jd.LocalAnchorA = new Vector2(-10.0f, 0.0f);
                jd.LocalAnchorB = new Vector2(-0.5f, -0.5f);
                p1 = jd.BodyA.GetWorldPoint(jd.LocalAnchorA);
                p2 = jd.BodyB.GetWorldPoint(jd.LocalAnchorB);
                d = p2 - p1;
                jd.Length = d.Length();
                JointHelper.LinearStiffness(frequencyHz, dampingRatio, jd.BodyA, jd.BodyB, out float stiffness, out float damping);
                jd.Stiffness = stiffness;
                jd.Damping = damping;
                _joints[0] = JointFactory.CreateFromDef(World, jd);

                jd.BodyA = ground;
                jd.BodyB = _bodies[1];
                jd.LocalAnchorA = new Vector2(10.0f, 0.0f);
                jd.LocalAnchorB = new Vector2(0.5f, -0.5f);
                p1 = jd.BodyA.GetWorldPoint(jd.LocalAnchorA);
                p2 = jd.BodyB.GetWorldPoint(jd.LocalAnchorB);
                d = p2 - p1;
                jd.Length = d.Length();
                JointHelper.LinearStiffness(frequencyHz, dampingRatio, jd.BodyA, jd.BodyB, out stiffness, out damping);
                jd.Stiffness = stiffness;
                jd.Damping = damping;
                _joints[1] = JointFactory.CreateFromDef(World, jd);

                jd.BodyA = ground;
                jd.BodyB = _bodies[2];
                jd.LocalAnchorA = new Vector2(10.0f, 20.0f);
                jd.LocalAnchorB = new Vector2(0.5f, 0.5f);
                p1 = jd.BodyA.GetWorldPoint(jd.LocalAnchorA);
                p2 = jd.BodyB.GetWorldPoint(jd.LocalAnchorB);
                d = p2 - p1;
                jd.Length = d.Length();
                JointHelper.LinearStiffness(frequencyHz, dampingRatio, jd.BodyA, jd.BodyB, out stiffness, out damping);
                jd.Stiffness = stiffness;
                jd.Damping = damping;
                _joints[2] = JointFactory.CreateFromDef(World, jd);

                jd.BodyA = ground;
                jd.BodyB = _bodies[3];
                jd.LocalAnchorA = new Vector2(-10.0f, 20.0f);
                jd.LocalAnchorB = new Vector2(-0.5f, 0.5f);
                p1 = jd.BodyA.GetWorldPoint(jd.LocalAnchorA);
                p2 = jd.BodyB.GetWorldPoint(jd.LocalAnchorB);
                d = p2 - p1;
                jd.Length = d.Length();
                JointHelper.LinearStiffness(frequencyHz, dampingRatio, jd.BodyA, jd.BodyB, out stiffness, out damping);
                jd.Stiffness = stiffness;
                jd.Damping = damping;
                _joints[3] = JointFactory.CreateFromDef(World, jd);

                jd.BodyA = _bodies[0];
                jd.BodyB = _bodies[1];
                jd.LocalAnchorA = new Vector2(0.5f, 0.0f);
                jd.LocalAnchorB = new Vector2(-0.5f, 0.0f);
                
                p1 = jd.BodyA.GetWorldPoint(jd.LocalAnchorA);
                p2 = jd.BodyB.GetWorldPoint(jd.LocalAnchorB);
                d = p2 - p1;
                jd.Length = d.Length();
                JointHelper.LinearStiffness(frequencyHz, dampingRatio, jd.BodyA, jd.BodyB, out stiffness, out damping);
                jd.Stiffness = stiffness;
                jd.Damping = damping;
                _joints[4] = JointFactory.CreateFromDef(World, jd);

                jd.BodyA = _bodies[1];
                jd.BodyB = _bodies[2];
                jd.LocalAnchorA = new Vector2(0.0f, 0.5f);
                jd.LocalAnchorB = new Vector2(0.0f, -0.5f);
                p1 = jd.BodyA.GetWorldPoint(jd.LocalAnchorA);
                p2 = jd.BodyB.GetWorldPoint(jd.LocalAnchorB);
                d = p2 - p1;
                jd.Length = d.Length();
                JointHelper.LinearStiffness(frequencyHz, dampingRatio, jd.BodyA, jd.BodyB, out stiffness, out damping);
                jd.Stiffness = stiffness;
                jd.Damping = damping;
                _joints[5] = JointFactory.CreateFromDef(World, jd);

                jd.BodyA = _bodies[2];
                jd.BodyB = _bodies[3];
                jd.LocalAnchorA = new Vector2(-0.5f, 0.0f);
                jd.LocalAnchorB = new Vector2(0.5f, 0.0f);
                p1 = jd.BodyA.GetWorldPoint(jd.LocalAnchorA);
                p2 = jd.BodyB.GetWorldPoint(jd.LocalAnchorB);
                d = p2 - p1;
                jd.Length = d.Length();
                JointHelper.LinearStiffness(frequencyHz, dampingRatio, jd.BodyA, jd.BodyB, out stiffness, out damping);
                jd.Stiffness = stiffness;
                jd.Damping = damping;
                _joints[6] = JointFactory.CreateFromDef(World, jd);

                jd.BodyA = _bodies[3];
                jd.BodyB = _bodies[0];
                jd.LocalAnchorA = new Vector2(0.0f, -0.5f);
                jd.LocalAnchorB = new Vector2(0.0f, 0.5f);
                p1 = jd.BodyA.GetWorldPoint(jd.LocalAnchorA);
                p2 = jd.BodyB.GetWorldPoint(jd.LocalAnchorB);
                d = p2 - p1;
                jd.Length = d.Length();
                JointHelper.LinearStiffness(frequencyHz, dampingRatio, jd.BodyA, jd.BodyB, out stiffness, out damping);
                jd.Stiffness = stiffness;
                jd.Damping = damping;
                _joints[7] = JointFactory.CreateFromDef(World, jd);
            }
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsNewKeyPress(Keys.B))
            {
                for (int i = 0; i < 4; ++i)
                    if (_bodies[i] != null)
                    {
                        World.RemoveBody(_bodies[i]);
                        _bodies[i] = null;
                        break;
                    }
            }
            else if (keyboard.IsNewKeyPress(Keys.J))
            {
                for (int i = 0; i < 8; ++i)
                    if (_joints[i] != null)
                    {
                        World.RemoveJoint(_joints[i]);
                        _joints[i] = null;
                        break;
                    }
            }

            base.Keyboard(keyboard);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);
            DrawString("Press: (b) to delete a body, (j) to delete a joint");
        }

        private void JointDestroyed(Joint joint)
        {
            for (int i = 0; i < 8; ++i)
                if (_joints[i] == joint)
                {
                    _joints[i] = null;
                    break;
                }
        }

        internal static Test Create()
        {
            return new WebTest();
        }
    }
}