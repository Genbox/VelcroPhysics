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
    public class CCDTest : Test
    {
        public CCDTest()
        {
		    {
                BodyDef bd = new BodyDef();
			    bd.position = new Vector2(0.0f, -0.2f);

			    Body body = _world.CreateBody(bd);
			    PolygonShape shape = new PolygonShape();
			    shape.SetAsBox(10.0f, 0.2f);

			    FixtureDef fd = new FixtureDef();
			    fd.shape = shape;
			    fd.density = 0.0f;
			    
			    body.CreateFixture(fd);

			    shape.SetAsBox(0.2f, 1.0f, new Vector2(0.5f, 1.2f), 0.0f);
			    body.CreateFixture(fd);
		    }

		    {
			    PolygonShape shape = new PolygonShape();
			    shape.SetAsBox(2.0f, 0.1f);

			    FixtureDef fd = new FixtureDef();
			    fd.shape = shape;
			    fd.density = 1.0f;
			    fd.restitution = 0.0f;

			    _angularVelocity = Rand.RandomFloat(-50.0f, 50.0f);
			    _angularVelocity = -30.669577f;

			    BodyDef bd = new BodyDef();
                bd.type = BodyType.Dynamic;
			    bd.position = new Vector2(0.0f, 20.0f);
			    Body body = _world.CreateBody(bd);
			    body.CreateFixture(fd);
			    body.SetLinearVelocity(new Vector2(0.0f, -100.0f));
			    body.SetAngularVelocity(_angularVelocity);
		    }
    #if false
		    {
			    FixtureDef fd = new FixtureDef();
			    fd.SetAsBox(10.0f, 0.1f);
			    fd.density = 0.0f;

			    BodyDef bd = new BodyDef();
			    bd.type = BodyDef.e_static;
			    bd.position = new Vector2(0.0f, -0.2f);
			    Body ground = _world.CreateBody(bd);
			    ground.CreateFixture(fd);
		    }

		    {
			    FixtureDef fd = new FixtureDef();
			    fd.SetAsBox(2.0f, 0.1f);
			    fd.density = 1.0f;
			    fd.restitution = 0.0f;

			    BodyDef bd1;
			    bd1.type = BodyDef.e_dynamic;
			    bd1.bullet = true;
			    bd1.allowSleep = false;
			    bd1.position = new Vector2(0.0f, 20.0f);
			    Body b1 = _world.Create(bd1);
			    b1.CreateFixture(fd);
			    b1.SetLinearVelocity(new Vector2(0.0f, -100.0f));

			    fd.SetAsBox(1.0f, 0.1f);
			    BodyDef bd2;
			    bd2.type = BodyDef.e_dynamic;
			    bd2.bullet = true;
			    bd2.allowSleep = false;
			    bd2.position = new Vector2(0.0f, 20.2f);
			    Body b2 = _world.Create(bd2);
			    b2.CreateFixture(fd);
			    b2.SetLinearVelocity(new Vector2(0.0f, -100.0f));

			    fd.SetAsBox(0.25f, 0.25f);
			    fd.density = 10.0f;
			    BodyDef bd3;
			    bd3.type = BodyDef.e_dynamic;
			    bd3.bullet = true;
			    bd3.allowSleep = false;
			    bd3.position = new Vector2(0.0f, 100.0f);
			    Body b3 = _world.Create(bd3);
			    b3.CreateFixture(fd);
			    b3.SetLinearVelocity(new Vector2(0.0f, -150.0f));
		    }
    #elif false
		    float k_restitution = 1.4f;

		    {
			    BodyDef bd = new BodyDef();
			    bd.position = new Vector2(0.0f, 20.0f);
			    Body body = _world.CreateBody(bd);

			    FixtureDef fd = new FixtureDef();
			    fd.density = 0.0f;
			    fd.restitution = k_restitution;

			    fd.SetAsBox(0.1f, 10.0f, new Vector2(-10.0f, 0.0f), 0.0f);
			    body.CreateFixture(fd);

			    fd.SetAsBox(0.1f, 10.0f, new Vector2(10.0f, 0.0f), 0.0f);
			    body.CreateFixture(fd);

			    fd.SetAsBox(0.1f, 10.0f, new Vector2(0.0f, -10.0f), 0.5f * (float)Box2D.XNA.Settings.b2_pi);
			    body.CreateFixture(fd);

			    fd.SetAsBox(0.1f, 10.0f, new Vector2(0.0f, 10.0f), -0.5f * (float)Box2D.XNA.Settings.b2_pi);
			    body.CreateFixture(fd);
		    }

    #if false
		    {
			    FixtureDef sd_bottom;
			    sd_bottom.SetAsBox(1.0f, 0.1f, new Vector2(0.0f, -1.0f), 0.0f);
			    sd_bottom.density = 4.0f;

			    FixtureDef sd_top;
			    sd_top.SetAsBox(1.0f, 0.1f, new Vector2(0.0f,  1.0f), 0.0f);
			    sd_top.density = 4.0f;

			    FixtureDef sd_left;
			    sd_left.SetAsBox(0.1f, 1.0f, new Vector2(-1.0f, 0.0f), 0.0f);
			    sd_left.density = 4.0f;

			    FixtureDef sd_right;
			    sd_right.SetAsBox(0.1f, 1.0f, new Vector2(1.0f, 0.0f), 0.0f);
			    sd_right.density = 4.0f;

			    BodyDef bd = new BodyDef();
			    bd.type = BodyDef.e_dynamicBody;
			    bd.position = new Vector2(0.0f, 15.0f);
			    Body body = _world.CreateBody(bd);
			    body.CreateFixture(&sd_bottom);
			    body.CreateFixture(&sd_top);
			    body.CreateFixture(&sd_left);
			    body.CreateFixture(&sd_right);
		    }
#elif false
		    {
			    FixtureDef sd_bottom;
			    sd_bottom.SetAsBox( 1.5f, 0.15f );
			    sd_bottom.density = 4.0f;

			    FixtureDef sd_left;
			    sd_left.SetAsBox(0.15f, 2.7f, new Vector2(-1.45f, 2.35f), 0.2f);
			    sd_left.density = 4.0f;

			    FixtureDef sd_right;
			    sd_right.SetAsBox(0.15f, 2.7f, new Vector2(1.45f, 2.35f), -0.2f);
			    sd_right.density = 4.0f;

			    BodyDef bd = new BodyDef();
			    bd.position = new Vector2( 0.0f, 15.0f );
			    Body body = _world.CreateBody(bd);
			    body.CreateFixture(&sd_bottom);
			    body.CreateFixture(&sd_left);
			    body.CreateFixture(&sd_right);
		    }
#else
		    {
			    BodyDef bd = new BodyDef();
			    bd.position = new Vector2(-5.0f, 20.0f);
			    bd.bullet = true;
			    Body body = _world.CreateBody(bd);
			    body.SetAngularVelocity(Rand.RandomFloat(-50.0f, 50.0f));

			    FixtureDef fd = new FixtureDef();
			    fd.SetAsBox(0.1f, 4.0f);
			    fd.density = 1.0f;
			    fd.restitution = 0.0f;
			    body.CreateFixture(fd);
		    }
#endif

		    for (int i = 0; i < 0; ++i)
		    {
			    BodyDef bd = new BodyDef();
			    bd.position = new Vector2(0.0f, 15.0f + i);
			    bd.bullet = true;
			    Body body = _world.CreateBody(bd);
			    body.SetAngularVelocity(Rand.RandomFloat(-50.0f, 50.0f));

			    FixtureDef fd = new FixtureDef();
			    fd.radius = 0.25f;
			    fd.density = 1.0f;
			    fd.restitution = 0.0f;
			    body.CreateFixture(fd);
		    }
#endif
        }

        public override void Step(Framework.Settings settings)
	    {
		    if (_stepCount	== 10)
		    {
			    _stepCount += 0;
		    }

		    base.Step(settings);

		    

		    if (b2_gjkCalls > 0)
		    {
                _debugDraw.DrawString(50, _textLine, "gjk calls = {0:n}, ave gjk iters = {1:n}, max gjk iters = {2:n}",
				    b2_gjkCalls, b2_gjkIters / (float)b2_gjkCalls, b2_gjkMaxIters);
			    _textLine += 15;
		    }

		    if (b2_toiCalls > 0)
		    {
                _debugDraw.DrawString(50, _textLine, "toi calls = {0:n}, ave toi iters = {1:n}, max toi iters = {2:n}",
								    b2_toiCalls, b2_toiIters / (float)b2_toiCalls, b2_toiMaxRootIters);
			    _textLine += 15;
                _debugDraw.DrawString(50, _textLine, "ave toi root iters = {0:n}, max toi root iters = {1:n}",
				    b2_toiRootIters / (float)b2_toiCalls, b2_toiMaxRootIters);
			    _textLine += 15;
		    }
	    }

	    static internal Test Create()
	    {
		    return new CCDTest();
	    }

	    float _angularVelocity;

        static int b2_gjkCalls, b2_gjkIters, b2_gjkMaxIters = 0;
        static int b2_toiCalls, b2_toiIters, b2_toiMaxIters = 0;
        static int b2_toiRootIters, b2_toiMaxRootIters = 0;
    }
}
