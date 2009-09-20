/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2008 Erin Catto http://www.gphysics.com

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

using Box2DX.Common;
using Box2DX.Collision;
using Box2DX.Dynamics;

namespace TestBed
{
    public class SensorTest : Test
    {
        const int _count = 7;
        Fixture _sensor;
        Body[] _bodies = new Body[_count];

        public SensorTest()
        {
            {
                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);

                {
                    PolygonShape shape = new PolygonShape();
                    shape.SetAsEdge(new Vec2(-40.0f, 0.0f), new Vec2(40.0f, 0.0f));
                    ground.CreateFixture(shape, 0);
                }

#if false
			{
				b2FixtureDef sd;
				sd.SetAsBox(10.0f, 2.0f, b2Vec2(0.0f, 20.0f), 0.0f);
				sd.isSensor = true;
				m_sensor = ground->CreateFixture(&sd);
			}
#else
                {
                    CircleShape shape = new CircleShape();
                    shape._radius = 5.0f;
                    shape._p.Set(0.0f, 10.0f);

                    FixtureDef fd = new FixtureDef();
                    fd.Shape = shape;
                    fd.IsSensor = true;
                    _sensor = ground.CreateFixture(fd);
                }
#endif
            }

            {
                CircleShape shape = new CircleShape();
                shape._radius = 1.0f;

                for (int i = 0; i < _count; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Position.Set(-10.0f + 3.0f * i, 20.0f);
                    bd.UserData = false;

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

            if (fixtureA == _sensor)
            {
                fixtureB.GetBody().SetUserData(true);
            }

            if (fixtureB == _sensor)
            {
                fixtureA.GetBody().SetUserData(true);
            }
        }

        // Implement contact listener.
        public override void EndContact(Contact contact)
        {
            Fixture fixtureA = contact.GetFixtureA();
            Fixture fixtureB = contact.GetFixtureB();

            if (fixtureA == _sensor)
            {
                fixtureB.GetBody().SetUserData(false);
            }

            if (fixtureB == _sensor)
            {
                fixtureA.GetBody().SetUserData(false);
            }
        }


        public override void Step(Settings settings)
        {
            base.Step(settings);
            // Traverse the contact results. Apply a force on shapes
            // that overlap the sensor.
            for (int i = 0; i < 7; ++i)
            {
                if ((bool)_bodies[i].GetUserData() == false)
                {
                    continue;
                }

                Body body = _bodies[i];
                Body ground = _sensor.GetBody();

                CircleShape circle = (CircleShape)_sensor.GetShape();
                Vec2 center = ground.GetWorldPoint(circle._p);

                Vec2 position = body.GetPosition();

                Vec2 d = center - position;
                if (d.LengthSquared() < Box2DX.Common.Settings.FLT_EPSILON * Box2DX.Common.Settings.FLT_EPSILON)
                {
                    continue;
                }

                d.Normalize();
                Vec2 F = 100.0f * d;
                _bodies[i].ApplyForce(F, position);
            }
        }

        public static Test Create()
        {
            return new SensorTest();
        }
    }
}