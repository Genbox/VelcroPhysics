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

using Genbox.VelcroPhysics.Collision.Distance;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Collision.TOI;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class ContinuousTest : Test
    {
        private Body _body;
        private float _angularVelocity;

        private ContinuousTest()
        {
            {
                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(0.0f, 0.0f);
                Body body = BodyFactory.CreateFromDef(World, bd);

                EdgeShape edge = new EdgeShape();

                edge.SetTwoSided(new Vector2(-10.0f, 0.0f), new Vector2(10.0f, 0.0f));
                body.AddFixture(edge);

                PolygonShape shape = new PolygonShape(0.0f);
                shape.SetAsBox(0.2f, 1.0f, new Vector2(0.5f, 1.0f), 0.0f);
                body.AddFixture(shape);
            }

#if true
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(0.0f, 20.0f);

                PolygonShape shape = new PolygonShape(1.0f);
                shape.SetAsBox(2.0f, 0.1f);

                _body = BodyFactory.CreateFromDef(World, bd);
                _body.AddFixture(shape);

                _angularVelocity = Rand.RandomFloat(-50.0f, 50.0f);
                _body.LinearVelocity = new Vector2(0.0f, -100.0f);
                _body.AngularVelocity = _angularVelocity;
            }
#else
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(0.0f, 2.0f);
                Body body = BodyFactory.CreateFromDef(World, bd);

                CircleShape shape = new CircleShape(0.5f, 1.0f);
                shape.Position = Vector2.Zero;
                body.AddFixture(shape);

                bd.IsBullet = true;
                bd.Position = new Vector2(0.0f, 10.0f);
                body = BodyFactory.CreateFromDef(World, bd);
                body.AddFixture(shape);
                body.LinearVelocity = new Vector2(0.0f, -100.0f);
            }
#endif

            DistanceGJK.GJKCalls = 0;
            DistanceGJK.GJKIters = 0;
            DistanceGJK.GJKMaxIters = 0;
            TimeOfImpact.TOICalls = 0;
            TimeOfImpact.TOIIters = 0;
            TimeOfImpact.TOIRootIters = 0;
            TimeOfImpact.TOIMaxRootIters = 0;
        }

        private void Launch()
        {
            DistanceGJK.GJKCalls = 0;
            DistanceGJK.GJKIters = 0;
            DistanceGJK.GJKMaxIters = 0;

            TimeOfImpact.TOICalls = 0;
            TimeOfImpact.TOIIters = 0;

            TimeOfImpact.TOIRootIters = 0;
            TimeOfImpact.TOIMaxRootIters = 0;

            _body.SetTransform(new Vector2(0.0f, 20.0f), 0.0f);
            _angularVelocity = Rand.RandomFloat(-50.0f, 50.0f);
            _body.LinearVelocity = new Vector2(0.0f, -100.0f);
            _body.AngularVelocity = _angularVelocity;
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            if (DistanceGJK.GJKCalls > 0)
                DrawString($"gjk calls = {DistanceGJK.GJKCalls}, ave gjk iters = {DistanceGJK.GJKIters / (float)DistanceGJK.GJKCalls}, max gjk iters = {DistanceGJK.GJKMaxIters}");

            if (TimeOfImpact.TOICalls > 0)
            {
                DrawString($"toi calls = {TimeOfImpact.TOICalls}, ave [max] toi iters = {TimeOfImpact.TOIIters / (float)TimeOfImpact.TOICalls} [{TimeOfImpact.TOIMaxRootIters}]");

                DrawString($"ave [max] toi root iters = {TimeOfImpact.TOIRootIters / (float)TimeOfImpact.TOICalls} [{TimeOfImpact.TOIMaxRootIters}]");

                //DrawString("ave [max] toi time = %.1f [%.1f] (microseconds)",
                //1000.0f * b2_toiTime / float(b2_toiCalls), 1000.0f * b2_toiMaxTime);
            }

            //if (m_stepCount % 60 == 0)
            //{
            //    Launch();
            //}

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new ContinuousTest();
        }
    }
}