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
using Microsoft.Xna.Framework;
using Box2D.XNA;
using Microsoft.Xna.Framework.Input;

namespace Box2D.XNA.TestBed.Tests
{
    public class TheoJansen : Test
    {
        void CreateLeg(float s, Vector2 wheelAnchor)
	    {
		    Vector2 p1 = new Vector2(5.4f * s, -6.1f);
		    Vector2 p2 = new Vector2(7.2f * s, -1.2f);
		    Vector2 p3 = new Vector2(4.3f * s, -1.9f);
		    Vector2 p4 = new Vector2(3.1f * s, 0.8f);
		    Vector2 p5 = new Vector2(6.0f * s, 1.5f);
		    Vector2 p6 = new Vector2(2.5f * s, 3.7f);

		    FixtureDef fd1 = new FixtureDef();
            FixtureDef fd2 = new FixtureDef();

		    fd1.filter.groupIndex = -1;
		    fd2.filter.groupIndex = -1;
		    fd1.density = 1.0f;
		    fd2.density = 1.0f;

		    PolygonShape poly1 = new PolygonShape();
            PolygonShape poly2 = new PolygonShape();

            Vector2[] vertices = new Vector2[3];

		    if (s > 0.0f)
		    {
			    vertices[0] = p1;
			    vertices[1] = p2;
			    vertices[2] = p3;
			    poly1.Set(vertices, 3);

			    vertices[0] = Vector2.Zero;
			    vertices[1] = p5 - p4;
			    vertices[2] = p6 - p4;
			    poly2.Set(vertices, 3);
		    }
		    else
		    {
			    vertices[0] = p1;
			    vertices[1] = p3;
			    vertices[2] = p2;
			    poly1.Set(vertices, 3);

			    vertices[0] = Vector2.Zero;
			    vertices[1] = p6 - p4;
			    vertices[2] = p5 - p4;
			    poly2.Set(vertices, 3);
		    }

		    fd1.shape = poly1;
		    fd2.shape = poly2;

		    BodyDef bd1 = new BodyDef();
            BodyDef bd2 = new BodyDef();
            bd1.type = BodyType.Dynamic;
            bd2.type = BodyType.Dynamic;

		    bd1.position = _offset;
		    bd2.position = p4 + _offset;

		    bd1.angularDamping = 10.0f;
		    bd2.angularDamping = 10.0f;

		    Body body1 = _world.CreateBody(bd1);
		    Body body2 = _world.CreateBody(bd2);

		    body1.CreateFixture(fd1);
		    body2.CreateFixture(fd2);

            DistanceJointDef djd = new DistanceJointDef();

		    // Using a soft distanceraint can reduce some jitter.
		    // It also makes the structure seem a bit more fluid by
		    // acting like a suspension system.
		    djd.dampingRatio = 0.5f;
		    djd.frequencyHz = 10.0f;

		    djd.Initialize(body1, body2, p2 + _offset, p5 + _offset);
		    _world.CreateJoint(djd);

		    djd.Initialize(body1, body2, p3 + _offset, p4 + _offset);
		    _world.CreateJoint(djd);

		    djd.Initialize(body1, _wheel, p3 + _offset, wheelAnchor + _offset);
		    _world.CreateJoint(djd);

		    djd.Initialize(body2, _wheel, p6 + _offset, wheelAnchor + _offset);
		    _world.CreateJoint(djd);

            RevoluteJointDef rjd = new RevoluteJointDef();

		    rjd.Initialize(body2, _chassis, p4 + _offset);
		    _world.CreateJoint(rjd);
	    }

	    TheoJansen()
	    {
		    _offset = new Vector2(0.0f, 8.0f);
		    _motorSpeed = 2.0f;
		    _motorOn = true;
		    Vector2 pivot = new Vector2(0.0f, 0.8f);

		    // Ground
		    {
			    BodyDef bd = new BodyDef();
			    Body ground = _world.CreateBody(bd);

			    PolygonShape shape = new PolygonShape();
			    shape.SetAsEdge(new Vector2(-50.0f, 0.0f), new Vector2(50.0f, 0.0f));
			    ground.CreateFixture(shape, 0.0f);

			    shape.SetAsEdge(new Vector2(-50.0f, 0.0f), new Vector2(-50.0f, 10.0f));
			    ground.CreateFixture(shape, 0.0f);

			    shape.SetAsEdge(new Vector2(50.0f, 0.0f), new Vector2(50.0f, 10.0f));
			    ground.CreateFixture(shape, 0.0f);
		    }

		    // Balls
		    for (int i = 0; i < 40; ++i)
		    {
			    CircleShape shape = new CircleShape();
			    shape._radius = 0.25f;

			    BodyDef bd = new BodyDef();
                bd.type = BodyType.Dynamic;
			    bd.position = new Vector2(-40.0f + 2.0f * i, 0.5f);

			    Body body = _world.CreateBody(bd);
			    body.CreateFixture(shape, 1.0f);
		    }

		    // Chassis
		    {
			    PolygonShape shape = new PolygonShape();
			    shape.SetAsBox(2.5f, 1.0f);

			    FixtureDef sd = new FixtureDef();
			    sd.density = 1.0f;
			    sd.shape = shape;
			    sd.filter.groupIndex = -1;
			    BodyDef bd = new BodyDef();
                bd.type = BodyType.Dynamic;
			    bd.position = pivot + _offset;
			    _chassis = _world.CreateBody(bd);
			    _chassis.CreateFixture(sd);
		    }

		    {
			    CircleShape shape = new CircleShape();
			    shape._radius = 1.6f;

			    FixtureDef sd = new FixtureDef();
			    sd.density = 1.0f;
			    sd.shape = shape;
			    sd.filter.groupIndex = -1;
			    BodyDef bd = new BodyDef();
                bd.type = BodyType.Dynamic;
			    bd.position = pivot + _offset;
			    _wheel = _world.CreateBody(bd);
			    _wheel.CreateFixture(sd);
		    }

		    {
			    RevoluteJointDef jd = new RevoluteJointDef();
			    jd.Initialize(_wheel, _chassis, pivot + _offset);
			    jd.collideConnected = false;
			    jd.motorSpeed = _motorSpeed;
			    jd.maxMotorTorque = 400.0f;
			    jd.enableMotor = _motorOn;
			    _motorJoint = (RevoluteJoint)_world.CreateJoint(jd);
		    }

		    Vector2 wheelAnchor;
    		
		    wheelAnchor = pivot + new Vector2(0.0f, -0.8f);

		    CreateLeg(-1.0f, wheelAnchor);
		    CreateLeg(1.0f, wheelAnchor);

		    _wheel.SetTransform(_wheel.GetPosition(), 120.0f * (float)Box2D.XNA.Settings.b2_pi / 180.0f);
		    CreateLeg(-1.0f, wheelAnchor);
		    CreateLeg(1.0f, wheelAnchor);

		    _wheel.SetTransform(_wheel.GetPosition(), -120.0f * (float)Box2D.XNA.Settings.b2_pi / 180.0f);
		    CreateLeg(-1.0f, wheelAnchor);
		    CreateLeg(1.0f, wheelAnchor);
	    }

	    public override void Step(Framework.Settings settings)
	    {
		    _debugDraw.DrawString(50, _textLine, "Keys: left = a, brake = s, right = d, toggle motor = m");
		    _textLine += 15;

		    base.Step(settings);
	    }

	    public override void Keyboard(KeyboardState state, KeyboardState oldState)
	    {
            if (state.IsKeyDown(Keys.A) && oldState.IsKeyUp(Keys.A))
            {
                _motorJoint.SetMotorSpeed(-_motorSpeed);
            }
            if (state.IsKeyDown(Keys.S) && oldState.IsKeyUp(Keys.S))
            {
                _motorJoint.SetMotorSpeed(0.0f);
            }
            if (state.IsKeyDown(Keys.D) && oldState.IsKeyUp(Keys.D))
            {
                _motorJoint.SetMotorSpeed(_motorSpeed);
            }
            if (state.IsKeyDown(Keys.M) && oldState.IsKeyUp(Keys.M))
            {
                _motorJoint.EnableMotor(!_motorJoint.IsMotorEnabled());
            }
	    }

	    internal static Test Create()
	    {
		    return new TheoJansen();
	    }

	    Vector2 _offset;
	    Body _chassis;
	    Body _wheel;
	    RevoluteJoint _motorJoint;
	    bool _motorOn;
	    float _motorSpeed;
    }
}
