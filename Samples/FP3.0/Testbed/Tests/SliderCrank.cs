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
using Microsoft.Xna.Framework.Input;

namespace Box2D.XNA.TestBed.Tests
{
    public class SliderCrank : Test
    {
        public SliderCrank()
	    {
		    Body ground = null;
		    {
			    BodyDef bd = new BodyDef();
			    ground = _world.CreateBody(bd);

			    PolygonShape shape = new PolygonShape();
			    shape.SetAsEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
			    ground.CreateFixture(shape, 0.0f);
		    }

		    {
			    Body prevBody = ground;

			    // Define crank.
			    {
				    PolygonShape shape = new PolygonShape();
				    shape.SetAsBox(0.5f, 2.0f);

				    BodyDef bd = new BodyDef();
                    bd.type = BodyType.Dynamic;
				    bd.position = new Vector2(0.0f, 7.0f);
				    Body body = _world.CreateBody(bd);
				    body.CreateFixture(shape, 2.0f);

                    RevoluteJointDef rjd = new RevoluteJointDef();
				    rjd.Initialize(prevBody, body, new Vector2(0.0f, 5.0f));
				    rjd.motorSpeed = 1.0f * (float)Box2D.XNA.Settings.b2_pi;
				    rjd.maxMotorTorque = 10000.0f;
				    rjd.enableMotor = true;
				    _joint1 = (RevoluteJoint)_world.CreateJoint(rjd);

				    prevBody = body;
			    }

			    // Define follower.
			    {
				    PolygonShape shape = new PolygonShape();
				    shape.SetAsBox(0.5f, 4.0f);

				    BodyDef bd = new BodyDef();
                    bd.type = BodyType.Dynamic;
				    bd.position = new Vector2(0.0f, 13.0f);
				    Body body = _world.CreateBody(bd);
				    body.CreateFixture(shape, 2.0f);

                    RevoluteJointDef rjd3 = new RevoluteJointDef();
				    rjd3.Initialize(prevBody, body, new Vector2(0.0f, 9.0f));
				    rjd3.enableMotor = false;
				    _world.CreateJoint(rjd3);

				    prevBody = body;
			    }

			    // Define piston
			    {
				    PolygonShape shape = new PolygonShape();
				    shape.SetAsBox(1.5f, 1.5f);

				    BodyDef bd = new BodyDef();
                    bd.type = BodyType.Dynamic;
				    bd.position = new Vector2(0.0f, 17.0f);
				    Body body = _world.CreateBody(bd);
				    body.CreateFixture(shape, 2.0f);

                    RevoluteJointDef rjd2 = new RevoluteJointDef();
				    rjd2.Initialize(prevBody, body, new Vector2(0.0f, 17.0f));
				    _world.CreateJoint(rjd2);

                    PrismaticJointDef pjd = new PrismaticJointDef();
				    pjd.Initialize(ground, body, new Vector2(0.0f, 17.0f), new Vector2(0.0f, 1.0f));

				    pjd.maxMotorForce = 1000.0f;
				    pjd.enableMotor = true;

				    _joint2 = (PrismaticJoint)_world.CreateJoint(pjd);
			    }

			    // Create a payload
			    {
				    PolygonShape shape = new PolygonShape();
				    shape.SetAsBox(1.5f, 1.5f);

				    BodyDef bd = new BodyDef();
                    bd.type = BodyType.Dynamic;
				    bd.position = new Vector2(0.0f, 23.0f);
				    Body body = _world.CreateBody(bd);
				    body.CreateFixture(shape, 2.0f);
			    }
		    }
	    }

	    public override void Keyboard(KeyboardState state, KeyboardState oldState)
	    {
            if (state.IsKeyDown(Keys.F) && oldState.IsKeyUp(Keys.F))
            {
                _joint2.EnableMotor(!_joint2.IsMotorEnabled());
                _joint2.GetBodyB().SetAwake(true);
            }
            if (state.IsKeyDown(Keys.M) && oldState.IsKeyUp(Keys.M))
            {
                _joint1.EnableMotor(!_joint1.IsMotorEnabled());
                _joint1.GetBodyB().SetAwake(true);
            }
	    }

	    public override void Step(Framework.Settings settings)
	    {
		    base.Step(settings);
		    _debugDraw.DrawString(50, _textLine, "Keys: (f) toggle friction, (m) toggle motor");
		    _textLine += 15;
		    float torque = _joint1.GetMotorTorque();
            _debugDraw.DrawString(50, _textLine, "Motor Torque = {0:n}", (float)torque);
		    _textLine += 15;
	    }

	    internal static Test Create()
	    {
		    return new SliderCrank();
	    }

	    RevoluteJoint _joint1;
	    PrismaticJoint _joint2;
    }
}
