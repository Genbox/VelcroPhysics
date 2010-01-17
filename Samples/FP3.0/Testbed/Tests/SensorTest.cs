/*
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class SensorTest : Test
    {
        private const int Count = 7;

        private SensorTest()
        {
            {
                Body ground = World.CreateBody();

                {
                    Vertices edge = PolygonTools.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                    PolygonShape shape = new PolygonShape(edge, 0);
                    ground.CreateFixture(shape);
                }

                {
                    CircleShape shape = new CircleShape(5.0f, 0);
                    shape.Position = new Vector2(0.0f, 10.0f);

                    _sensor = ground.CreateFixture(shape);
                    _sensor.Sensor = true;
                }
            }

            {
                CircleShape shape = new CircleShape(1.0f, 1);

                for (int i = 0; i < Count; ++i)
                {
                    _touching[i] = false;
                    _bodies[i] = World.CreateBody();
                    _bodies[i].BodyType = BodyType.Dynamic;
                    _bodies[i].Position = new Vector2(-10.0f + 3.0f * i, 20.0f);
                    _bodies[i].UserData = i;

                    _bodies[i].CreateFixture(shape);
                }
            }
        }

        // Implement contact listener.
        public override void BeginContact(Contact contact)
        {
            Fixture fixtureA = contact.FixtureA;
            Fixture fixtureB = contact.FixtureB;

            if (fixtureA == _sensor && fixtureB.Body.UserData != null)
            {
                _touching[(int)(fixtureB.Body.UserData)] = true;
            }

            if (fixtureB == _sensor && fixtureA.Body.UserData != null)
            {
                _touching[(int)(fixtureA.Body.UserData)] = true;
            }
        }

        // Implement contact listener.
        public override void EndContact(Contact contact)
        {
            Fixture fixtureA = contact.FixtureA;
            Fixture fixtureB = contact.FixtureB;

            if (fixtureA == _sensor && fixtureB.Body.UserData != null)
            {
                _touching[(int)(fixtureB.Body.UserData)] = false;
            }

            if (fixtureB == _sensor && fixtureA.Body.UserData != null)
            {
                _touching[(int)(fixtureA.Body.UserData)] = false;
            }
        }

        public override void Update(Framework.Settings settings)
        {
            base.Update(settings);

            // Traverse the contact results. Apply a force on shapes
            // that overlap the sensor.
            for (int i = 0; i < Count; ++i)
            {
                if (_touching[i] == false)
                {
                    continue;
                }

                Body body = _bodies[i];
                Body ground = _sensor.Body;

                CircleShape circle = (CircleShape)_sensor.Shape;
                Vector2 center = ground.GetWorldPoint(circle.Position);

                Vector2 position = body.Position;

                Vector2 d = center - position;
                if (d.LengthSquared() < Settings.Epsilon * Settings.Epsilon)
                {
                    continue;
                }

                d.Normalize();
                Vector2 F = 100.0f * d;
                body.ApplyForce(F, position);
            }
        }

        internal static Test Create()
        {
            return new SensorTest();
        }

        private Fixture _sensor;
        private Body[] _bodies = new Body[Count];
        private bool[] _touching = new bool[Count];
    }
}