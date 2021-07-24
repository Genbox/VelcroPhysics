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
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class MobileBalancedTest : Test
    {
        private const int _depth = 4;

        private MobileBalancedTest()
        {
            Body ground;

            // Create ground body.
            {
                BodyDef bodyDef = new BodyDef();
                bodyDef.Position = new Vector2(0.0f, 20.0f);
                ground = BodyFactory.CreateFromDef(World, bodyDef);
            }

            float a = 0.5f;
            Vector2 h = new Vector2(0.0f, a);

            Body root = AddNode(ground, Vector2.Zero, 0, 3.0f, a);

            RevoluteJointDef jointDef = new RevoluteJointDef();
            jointDef.BodyA = ground;
            jointDef.BodyB = root;
            jointDef.LocalAnchorA = Vector2.Zero;
            jointDef.LocalAnchorB = h;
            JointFactory.CreateFromDef(World, jointDef);
        }

        private Body AddNode(Body parent, Vector2 localAnchor, int depth, float offset, float a)
        {
            float density = 20.0f;
            Vector2 h = new Vector2(0.0f, a);

            Vector2 p = parent.Position + localAnchor - h;

            BodyDef bodyDef = new BodyDef();
            bodyDef.Type = BodyType.Dynamic;
            bodyDef.Position = p;
            Body body = BodyFactory.CreateFromDef(World, bodyDef);

            PolygonShape shape = new PolygonShape(density);
            shape.SetAsBox(0.25f * a, a);
            body.AddFixture(shape);

            if (depth == _depth)
                return body;

            shape.SetAsBox(offset, 0.25f * a, new Vector2(0, -a), 0.0f);
            body.AddFixture(shape);

            Vector2 a1 = new Vector2(offset, -a);
            Vector2 a2 = new Vector2(-offset, -a);
            Body body1 = AddNode(body, a1, depth + 1, 0.5f * offset, a);
            Body body2 = AddNode(body, a2, depth + 1, 0.5f * offset, a);

            RevoluteJointDef jointDef = new RevoluteJointDef();
            jointDef.BodyA = body;
            jointDef.LocalAnchorB = h;

            jointDef.LocalAnchorA = a1;
            jointDef.BodyB = body1;
            JointFactory.CreateFromDef(World, jointDef);

            jointDef.LocalAnchorA = a2;
            jointDef.BodyB = body2;
            JointFactory.CreateFromDef(World, jointDef);

            return body;
        }

        internal static Test Create()
        {
            return new MobileBalancedTest();
        }
    }
}