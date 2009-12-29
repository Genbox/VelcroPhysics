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
    public class ApplyForce : Test
    {
        public ApplyForce()
        {
            _world.Gravity = new Vector2(0.0f, 0.0f);

		    float k_restitution = 0.4f;

            Body ground;
		    {
			    BodyDef bd = new BodyDef();
			    bd.position = new Vector2(0.0f, 20.0f);
			    ground = _world.CreateBody(bd);

			    PolygonShape shape = new PolygonShape();

			    FixtureDef sd = new FixtureDef();
			    sd.shape = shape;
			    sd.density = 0.0f;
			    sd.restitution = k_restitution;

			    // Left vertical
			    shape.SetAsEdge(new Vector2(-20.0f, -20.0f), new Vector2(-20.0f, 20.0f));
			    ground.CreateFixture(sd);

			    // Right vertical
			    shape.SetAsEdge(new Vector2(20.0f, -20.0f), new Vector2(20.0f, 20.0f));
			    ground.CreateFixture(sd);

			    // Top horizontal
			    shape.SetAsEdge(new Vector2(-20.0f, 20.0f), new Vector2(20.0f, 20.0f));
			    ground.CreateFixture(sd);

			    // Bottom horizontal
			    shape.SetAsEdge(new Vector2(-20.0f, -20.0f), new Vector2(20.0f, -20.0f));
			    ground.CreateFixture(sd);
		    }

		    {
                Transform xf1 = new Transform();
			    xf1.R.Set(0.3524f * (float)Box2D.XNA.Settings.b2_pi);
			    xf1.Position = MathUtils.Multiply(ref xf1.R, new Vector2(1.0f, 0.0f));

			    Vector2[] vertices = new Vector2[3];
			    vertices[0] = MathUtils.Multiply(ref xf1, new Vector2(-1.0f, 0.0f));
			    vertices[1] = MathUtils.Multiply(ref xf1, new Vector2(1.0f, 0.0f));
			    vertices[2] = MathUtils.Multiply(ref xf1, new Vector2(0.0f, 0.5f));
    			
			    PolygonShape poly1 = new PolygonShape();
			    poly1.Set(vertices, 3);

			    FixtureDef sd1 = new FixtureDef();
			    sd1.shape = poly1;
			    sd1.density = 4.0f;

                Transform xf2 = new Transform();
                xf2.R.Set(-0.3524f * (float)Box2D.XNA.Settings.b2_pi);
			    xf2.Position = MathUtils.Multiply(ref xf2.R, new Vector2(-1.0f, 0.0f));

			    vertices[0] = MathUtils.Multiply(ref xf2, new Vector2(-1.0f, 0.0f));
			    vertices[1] = MathUtils.Multiply(ref xf2, new Vector2(1.0f, 0.0f));
			    vertices[2] = MathUtils.Multiply(ref xf2, new Vector2(0.0f, 0.5f));
    			
			    PolygonShape poly2 = new PolygonShape();
			    poly2.Set(vertices, 3);

                FixtureDef sd2 = new FixtureDef();
			    sd2.shape = poly2;
			    sd2.density = 2.0f;

			    BodyDef bd = new BodyDef();
                bd.type = BodyType.Dynamic;
			    bd.angularDamping = 5.0f;
			    bd.linearDamping = 0.1f;

			    bd.position = new Vector2(0.0f, 2.0f);
                bd.angle = (float)Box2D.XNA.Settings.b2_pi;
			    _body = _world.CreateBody(bd);
			    _body.CreateFixture(sd1);
			    _body.CreateFixture(sd2);
		    }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(0.5f, 0.5f);

                FixtureDef fd = new FixtureDef();
                fd.shape = shape;
                fd.density = 1.0f;
                fd.friction = 0.3f;

                for (int i = 0; i < 10; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.type = BodyType.Dynamic;

                    bd.position = new Vector2(0.0f, 5.0f + 1.54f * i);
                    Body body = _world.CreateBody(bd);

                    body.CreateFixture(fd);

                    float gravity = 10.0f;
                    float I = body.GetInertia();
                    float mass = body.GetMass();

                    // For a circle: I = 0.5 * m * r * r ==> r = sqrt(2 * I / m)
                    float radius = (float)Math.Sqrt(2.0 * (double)(I / mass));

                    FrictionJointDef jd = new FrictionJointDef();
                    jd.localAnchorA = Vector2.Zero;
                    jd.localAnchorB = Vector2.Zero;
                    jd.bodyA = ground;
                    jd.bodyB = body;
                    jd.collideConnected = true;
                    jd.maxForce = mass * gravity;
                    jd.maxTorque = mass * radius * gravity;

                    _world.CreateJoint(jd);
                }
            }
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
	    {
            if(state.IsKeyDown(Keys.W))
		    {
			    Vector2 f = _body.GetWorldVector(new Vector2(0.0f, -200.0f));
			    Vector2 p = _body.GetWorldPoint(new Vector2(0.0f, 2.0f));
			    _body.ApplyForce(f, p);
		    }
			if(state.IsKeyDown(Keys.A))
		    {
			    _body.ApplyTorque(50.0f);
		    }
			if(state.IsKeyDown(Keys.D))
		    {
			    _body.ApplyTorque(-50.0f);
		    }
	    }

	    static internal Test Create()
	    {
		    return new ApplyForce();
	    }

	    Body _body;
    }
}
