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

using System;
using Box2D.XNA.TestBed.Framework;
using Box2D.XNA;
using Microsoft.Xna.Framework;

namespace Box2D.XNA.TestBed.Tests
{
    public class SensorTest : Test
    {
		static int e_count = 7;

	    public SensorTest()
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
				    CircleShape shape = new CircleShape();
				    shape._radius = 5.0f;
				    shape._p = new Vector2(0.0f, 10.0f);

				    FixtureDef fd = new FixtureDef();
				    fd.shape = shape;
				    fd.isSensor = true;
				    _sensor = ground.CreateFixture(fd);
			    }
    #endif
		    }

		    {
                CircleShape shape = new CircleShape();
			    shape._radius = 1.0f;

			    for (int i = 0; i < e_count; ++i)
			    {
				    BodyDef bd = new BodyDef();
                    bd.type = BodyType.Dynamic;
				    bd.position = new Vector2(-10.0f + 3.0f * i, 20.0f);
                    bd.userData = i;

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
		    for (int i = 0; i < e_count; ++i)
		    {
			    if (_touching[i] == false)
			    {
				    continue;
			    }

			    Body body = _bodies[i];
			    Body ground = _sensor.GetBody();

			    CircleShape circle = (CircleShape)_sensor.GetShape();
			    Vector2 center = ground.GetWorldPoint(circle._p);

			    Vector2 position = body.GetPosition();

			    Vector2 d = center - position;
                if (d.LengthSquared() < Box2D.XNA.Settings.b2_epsilon * Box2D.XNA.Settings.b2_epsilon)
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

	    Fixture _sensor;
	    Body[] _bodies = new Body[e_count];
	    bool[] _touching = new bool[e_count];
    }
}
