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

// This test holds worlds dumped using b2World::Dump.

using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class DumpLoaderTest : Test
    {
        private readonly Body _ball;

        private DumpLoaderTest()
        {
            Vertices vertices = new Vertices { new Vector2(-5, 0), new Vector2(5, 0), new Vector2(5, 5), new Vector2(4, 1), new Vector2(-4, 1), new Vector2(-5, 5) };
            ChainShape chainShape = new ChainShape(vertices, true);

            FixtureDef groundFixtureDef = new FixtureDef();
            groundFixtureDef.Shape = chainShape;

            BodyDef groundBodyDef = new BodyDef();
            groundBodyDef.Type = BodyType.Static;

            Body groundBody = BodyFactory.CreateFromDef(World, groundBodyDef);
            Fixture groundBodyFixture = groundBody.AddFixture(groundFixtureDef);

            CircleShape ballShape = new CircleShape(1, 1);

            FixtureDef ballFixtureDef = new FixtureDef();
            ballFixtureDef.Restitution = 0.75f;
            ballFixtureDef.Shape = ballShape;

            BodyDef ballBodyDef = new BodyDef();
            ballBodyDef.Type = BodyType.Dynamic;
            ballBodyDef.Position = new Vector2(0, 10);

            // ballBodyDef.AngularDamping = 0.2f;

            _ball = BodyFactory.CreateFromDef(World, ballBodyDef);
            Fixture ballFixture = _ball.AddFixture(ballFixtureDef);
            _ball.ApplyForce(new Vector2(-1000, -400));
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            Vector2 v = _ball.LinearVelocity;
            float omega = _ball.AngularVelocity;

            MassData massData;
            _ball.GetMassData(out massData);

            float ke = 0.5f * massData.Mass * MathUtils.Dot(v, v) + 0.5f * massData.Inertia * omega * omega;

            DrawString($"kinetic energy = {ke}");

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new DumpLoaderTest();
        }
    }
}