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
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    /// <summary>This test shows how a distance joint can be used to stabilize a chain of bodies with a heavy payload. Notice
    /// that the distance joint just prevents excessive stretching and has no other effect. By disabling the distance joint you
    /// can see that the Box2D solver has trouble supporting heavy bodies with light bodies. Try playing around with the
    /// densities, time step, and iterations to see how they affect stability. This test also shows how to use contact
    /// filtering. Filtering is configured so that the payload does not collide with the chain.</summary>
    internal class WreckingBallTest : Test
    {
        private DistanceJointDef _distanceJointDef = new DistanceJointDef();
        private readonly Joint _distanceJoint;
        private bool _stabilize;

        private WreckingBallTest()
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
                PolygonShape shape = new PolygonShape(20.0f);
                shape.SetAsBox(0.5f, 0.125f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Friction = 0.2f;
                fd.Filter.Category = Category.Cat1;
                fd.Filter.CategoryMask = Category.All & ~Category.Cat2;

                RevoluteJointDef jd = new RevoluteJointDef();
                jd.CollideConnected = false;

                const int N = 10;
                const float y = 15.0f;
                _distanceJointDef.LocalAnchorA = new Vector2(0.0f, y);

                Body prevBody = ground;
                for (int i = 0; i < N; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(0.5f + 1.0f * i, y);
                    if (i == N - 1)
                    {
                        bd.Position = new Vector2(1.0f * i, y);
                        bd.AngularDamping = 0.4f;
                    }

                    Body body = BodyFactory.CreateFromDef(World, bd);

                    if (i == N - 1)
                    {
                        CircleShape circleShape = new CircleShape(100.0f);
                        circleShape.Radius = 1.5f;
                        FixtureDef sfd = new FixtureDef();
                        sfd.Shape = circleShape;
                        sfd.Filter.Category = Category.Cat2;
                        body.AddFixture(sfd);
                    }
                    else
                    {
                        body.AddFixture(fd);
                    }

                    Vector2 anchor = new Vector2(i, y);
                    jd.Initialize(prevBody, body, anchor);
                    JointFactory.CreateFromDef(World, jd);

                    prevBody = body;
                }

                _distanceJointDef.LocalAnchorB = Vector2.Zero;

                float extraLength = 0.01f;
                _distanceJointDef.MinLength = 0.0f;
                _distanceJointDef.MaxLength = N - 1.0f + extraLength;
                _distanceJointDef.BodyB = prevBody;
            }

            {
                _distanceJointDef.BodyA = ground;
                _distanceJoint = JointFactory.CreateFromDef(World, _distanceJointDef);
                _stabilize = true;
            }
        }

        //void UpdateUI()
        //{
        //	ImGui::SetNextWindowPos(ImVec2(10.0f, 100.0f));
        //	ImGui::SetNextWindowSize(ImVec2(200.0f, 100.0f));
        //	ImGui::Begin("Wrecking Ball Controls", nullptr, ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoResize);

        //	if (ImGui::Checkbox("Stabilize", _stabilize))
        //	{
        //		if (_stabilize == true && _distanceJoint == nullptr)
        //		{
        //			_distanceJoint = World.CreateJoint(_distanceJointDef);
        //		}
        //		else if (_stabilize == false && _distanceJoint != nullptr)
        //		{
        //			World.DestroyJoint(_distanceJoint);
        //			_distanceJoint = nullptr;
        //		}
        //	}

        //	ImGui::End();
        //}

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            if (_distanceJoint != null)
                DrawString("Distance Joint ON");
            else
                DrawString("Distance Joint OFF");
        }

        internal static Test Create()
        {
            return new WreckingBallTest();
        }
    }
}