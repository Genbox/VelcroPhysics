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

using Genbox.VelcroPhysics.Collision.ContactSystem;
using Genbox.VelcroPhysics.Collision.Narrowphase;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class PlatformerTest : Test
    {
        private readonly float _radius;
        private readonly float _top;
        private float _bottom;
        private State _state;
        private readonly Fixture _platform;
        private readonly Fixture _character;

        private enum State
        {
            e_unknown,
            e_above,
            e_below
        }

        private PlatformerTest()
        {
            // Ground
            {
                BodyDef bd = new BodyDef();
                Body ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape();
                shape.SetTwoSided(new Vector2(-20.0f, 0.0f), new Vector2(20.0f, 0.0f));
                ground.AddFixture(shape);
            }

            // Platform
            {
                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(0.0f, 10.0f);
                Body body = BodyFactory.CreateFromDef(World, bd);

                PolygonShape shape = new PolygonShape(0.0f);
                shape.SetAsBox(3.0f, 0.5f);
                _platform = body.AddFixture(shape);

                _bottom = 10.0f - 0.5f;
                _top = 10.0f + 0.5f;
            }

            // Actor
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(0.0f, 12.0f);
                Body body = BodyFactory.CreateFromDef(World, bd);

                _radius = 0.5f;
                CircleShape shape = new CircleShape(20.0f);
                shape.Radius = _radius;
                _character = body.AddFixture(shape);

                body.LinearVelocity = new Vector2(0.0f, -50.0f);

                _state = State.e_unknown;
            }
        }

        protected override void PreSolve(Contact contact, ref Manifold oldManifold)
        {
            base.PreSolve(contact, ref oldManifold);

            Fixture fixtureA = contact.FixtureA;
            Fixture fixtureB = contact.FixtureB;

            if (fixtureA != _platform && fixtureA != _character)
                return;

            if (fixtureB != _platform && fixtureB != _character)
                return;

#if true
            Vector2 position = _character.Body.Position;

            if (position.Y < _top + _radius - 3.0f * Settings.LinearSlop)
                contact.Enabled = false;
#else
        b2Vec2 v = _character.Body.LinearVelocity;
        if (v.y > 0.0f)
		{
            contact.SetEnabled(false);
        }
#endif
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            Vector2 v = _character.Body.LinearVelocity;
            DrawString("Character Linear Velocity: " + v.Y);
        }

        internal static Test Create()
        {
            return new PlatformerTest();
        }
    }
}