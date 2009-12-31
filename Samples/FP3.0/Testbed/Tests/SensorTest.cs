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
                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);

                {
                    PolygonShape shape = new PolygonShape();
                    shape.SetAsEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                    ground.CreateFixture(shape, 0.0f);
                }

#if false
			    {
				    FixtureDef sd;
				    sd.SetAsBox(10.0f, 2.0f, new Vector2(0.0f, 20.0f), 0.0f);
				    sd.isSensor = true;
				    _sensor = ground.CreateFixture(&sd);
			    }
#else
                {
                    CircleShape shape = new CircleShape(5.0f);
                    shape.Position = new Vector2(0.0f, 10.0f);

                    FixtureDef fd = new FixtureDef();
                    fd.Shape = shape;
                    fd.IsSensor = true;
                    _sensor = ground.CreateFixture(fd);
                }
#endif
            }

            {
                CircleShape shape = new CircleShape(1.0f);

                for (int i = 0; i < Count; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(-10.0f + 3.0f * i, 20.0f);
                    bd.UserData = i;

                    _touching[i] = false;
                    _bodies[i] = _world.CreateBody(bd);

                    _bodies[i].CreateFixture(shape, 1.0f);
                }
            }
        }

        // Implement contact listener.
        public override void BeginContact(Contact contact)
        {
            Fixture fixtureA = contact.GetFixtureA();
            Fixture fixtureB = contact.GetFixtureB();

            if (fixtureA == _sensor && fixtureB.GetBody().GetUserData() != null)
            {
                _touching[(int)(fixtureB.GetBody().GetUserData())] = true;
            }

            if (fixtureB == _sensor && fixtureA.GetBody().GetUserData() != null)
            {
                _touching[(int)(fixtureA.GetBody().GetUserData())] = true;
            }
        }

        // Implement contact listener.
        public override void EndContact(Contact contact)
        {
            Fixture fixtureA = contact.GetFixtureA();
            Fixture fixtureB = contact.GetFixtureB();

            if (fixtureA == _sensor && fixtureB.GetBody().GetUserData() != null)
            {
                _touching[(int)(fixtureB.GetBody().GetUserData())] = false;
            }

            if (fixtureB == _sensor && fixtureA.GetBody().GetUserData() != null)
            {
                _touching[(int)(fixtureA.GetBody().GetUserData())] = false;
            }
        }

        public override void Step(Framework.Settings settings)
        {
            base.Step(settings);

            // Traverse the contact results. Apply a force on shapes
            // that overlap the sensor.
            for (int i = 0; i < Count; ++i)
            {
                if (_touching[i] == false)
                {
                    continue;
                }

                Body body = _bodies[i];
                Body ground = _sensor.GetBody();

                CircleShape circle = (CircleShape)_sensor.GetShape();
                Vector2 center = ground.GetWorldPoint(circle.Position);

                Vector2 position = body.GetPosition();

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