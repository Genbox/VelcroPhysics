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

// This shows how to use sensor shapes. Sensors don't have collision, but report overlap events.

using Genbox.VelcroPhysics.Collision.ContactSystem;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class SensorsTest : Test
    {
        private const int _count = 7;

        private readonly Fixture _sensor;
        private readonly Body[] _bodies = new Body[_count];
        private readonly float _force;
        private readonly bool[] _touching = new bool[_count];

        private SensorsTest()
        {
            {
                BodyDef bd = new BodyDef();
                Body ground = BodyFactory.CreateFromDef(World, bd);

                {
                    EdgeShape shape = new EdgeShape();
                    shape.SetTwoSided(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                    ground.AddFixture(shape);
                }

#if false
			    {
				    FixtureDef sd;
				    sd.SetAsBox(10.0f, 2.0f, b2Vec2(0.0f, 20.0f), 0.0f);
				    sd.isSensor = true;
				    _sensor = ground.CreateFixture(sd);
			    }
#else
                {
                    CircleShape shape = new CircleShape(0.0f);
                    shape.Radius = 5.0f;
                    shape.Position = new Vector2(0.0f, 10.0f);

                    FixtureDef fd = new FixtureDef();
                    fd.Shape = shape;
                    fd.IsSensor = true;
                    _sensor = ground.AddFixture(fd);
                }
#endif
            }

            {
                CircleShape shape = new CircleShape(1.0f);
                shape.Radius = 1.0f;

                for (int i = 0; i < _count; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(-10.0f + 3.0f * i, 20.0f);
                    bd.UserData = i;

                    _touching[i] = false;
                    _bodies[i] = BodyFactory.CreateFromDef(World, bd);

                    _bodies[i].AddFixture(shape);
                }
            }

            _force = 100.0f;

            World.ContactManager.BeginContact += BeginContact;
            World.ContactManager.EndContact += EndContact;
        }

        // Implement contact listener.
        private void BeginContact(Contact contact)
        {
            Fixture fixtureA = contact.FixtureA;
            Fixture fixtureB = contact.FixtureB;

            if (fixtureA == _sensor)
            {
                int index = (int)fixtureB.Body.UserData;
                if (index < _count)
                    _touching[index] = true;
            }

            if (fixtureB == _sensor)
            {
                int index = (int)fixtureA.Body.UserData;
                if (index < _count)
                    _touching[index] = true;
            }
        }

        // Implement contact listener.
        private void EndContact(Contact contact)
        {
            Fixture fixtureA = contact.FixtureA;
            Fixture fixtureB = contact.FixtureB;

            if (fixtureA == _sensor)
            {
                int index = (int)fixtureB.Body.UserData;
                if (index < _count)
                    _touching[index] = false;
            }

            if (fixtureB == _sensor)
            {
                int index = (int)fixtureA.Body.UserData;
                if (index < _count)
                    _touching[index] = false;
            }
        }

        //void UpdateUI()
        //{
        //	ImGui::SetNextWindowPos(ImVec2(10.0f, 100.0f));
        //	ImGui::SetNextWindowSize(ImVec2(200.0f, 60.0f));
        //	ImGui::Begin("Sensor Controls", nullptr, ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoResize);

        //	ImGui::SliderFloat("Force", _force, 0.0f, 2000.0f, "%.0f");

        //	ImGui::End();
        //}

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            // Traverse the contact results. Apply a force on shapes
            // that overlap the sensor.
            for (int i = 0; i < _count; ++i)
            {
                if (_touching[i] == false)
                    continue;

                Body body = _bodies[i];
                Body ground = _sensor.Body;

                CircleShape circle = (CircleShape)_sensor.Shape;
                Vector2 center = ground.GetWorldPoint(circle.Position);

                Vector2 position = body.Position;

                Vector2 d = center - position;
                if (d.LengthSquared() < MathConstants.Epsilon * MathConstants.Epsilon)
                    continue;

                d.Normalize();
                Vector2 F = _force * d;
                body.ApplyForce(F, position);
            }
        }

        internal static Test Create()
        {
            return new SensorsTest();
        }
    }
}