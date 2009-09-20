/*
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

using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;
using Math = Box2DX.Common.Math;

namespace TestBed
{
    public class CCDTest : Test
    {
        float _angularVelocity;

        public CCDTest()
        {
#if true
            {
                //TODO: only one shape is created here instead of two. That is because
                //they are reference types and when shape is changed, it changes the original instead of a new copy.
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(10.0f, 0.2f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Density = 0.0f;

                BodyDef bd = new BodyDef();
                bd.Position.Set(0.0f, -0.2f);
                Body body = _world.CreateBody(bd);
                body.CreateFixture(fd);

                shape.SetAsBox(0.2f, 1.0f, new Vec2(0.5f, 1.2f), 0.0f);
                body.CreateFixture(fd);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(2.0f, 0.1f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Density = 1.0f;
                fd.Restitution = 0.0f;

                _angularVelocity = Math.Random(-50.0f, 50.0f);
                _angularVelocity = -30.669577f;

                BodyDef bd = new BodyDef();
                bd.Position.Set(0.0f, 20.0f);
                Body body = _world.CreateBody(bd);
                body.CreateFixture(fd);
                body.SetLinearVelocity(new Vec2(0.0f, -100.0f));
                body.SetAngularVelocity(_angularVelocity);
            }
#elif false
		{
			FixtureDef fd;
			fd.SetAsBox(10.0f, 0.1f);
			fd.density = 0.0f;

			BodyDef bd;
			bd.type = BodyDef::e_static;
			bd.position.Set(0.0f, -0.2f);
			Body* ground = m_world.CreateBody(&bd);
			ground.CreateFixture(&fd);
		}

		{
			FixtureDef fd;
			fd.SetAsBox(2.0f, 0.1f);
			fd.density = 1.0f;
			fd.restitution = 0.0f;

			BodyDef bd1;
			bd1.type = BodyDef::e_dynamic;
			bd1.isBullet = true;
			bd1.allowSleep = false;
			bd1.position.Set(0.0f, 20.0f);
			Body* b1 = m_world.Create(&bd1);
			b1.CreateFixture(&fd);
			b1.SetMassFromShapes();
			b1.SetLinearVelocity(Vec2(0.0f, -100.0f));

			fd.SetAsBox(1.0f, 0.1f);
			BodyDef bd2;
			bd2.type = BodyDef::e_dynamic;
			bd2.isBullet = true;
			bd2.allowSleep = false;
			bd2.position.Set(0.0f, 20.2f);
			Body*  = m_world.Create(&bd2);
			.CreateFixture(&fd);
			.SetMassFromShapes();
			.SetLinearVelocity(Vec2(0.0f, -100.0f));

			fd.SetAsBox(0.25f, 0.25f);
			fd.density = 10.0f;
			BodyDef bd3;
			bd3.type = BodyDef::e_dynamic;
			bd3.isBullet = true;
			bd3.allowSleep = false;
			bd3.position.Set(0.0f, 100.0f);
			Body* b3 = m_world.Create(&bd3);
			b3.CreateFixture(&fd);
			b3.SetMassFromShapes();
			b3.SetLinearVelocity(Vec2(0.0f, -150.0f));
		}
#else
		const float32 k_restitution = 1.4f;

		{
			BodyDef bd;
			bd.position.Set(0.0f, 20.0f);
			Body* body = m_world.CreateBody(&bd);

			FixtureDef fd;
			fd.density = 0.0f;
			fd.restitution = k_restitution;

			fd.SetAsBox(0.1f, 10.0f, Vec2(-10.0f, 0.0f), 0.0f);
			body.CreateFixture(&fd);

			fd.SetAsBox(0.1f, 10.0f, Vec2(10.0f, 0.0f), 0.0f);
			body.CreateFixture(&fd);

			fd.SetAsBox(0.1f, 10.0f, Vec2(0.0f, -10.0f), 0.5f * _pi);
			body.CreateFixture(&fd);

			fd.SetAsBox(0.1f, 10.0f, Vec2(0.0f, 10.0f), -0.5f * _pi);
			body.CreateFixture(&fd);
		}

#if false
		{
			FixtureDef sd_bottom;
			sd_bottom.SetAsBox(1.0f, 0.1f, Vec2(0.0f, -1.0f), 0.0f);
			sd_bottom.density = 4.0f;

			FixtureDef sd_top;
			sd_top.SetAsBox(1.0f, 0.1f, Vec2(0.0f,  1.0f), 0.0f);
			sd_top.density = 4.0f;

			FixtureDef sd_left;
			sd_left.SetAsBox(0.1f, 1.0f, Vec2(-1.0f, 0.0f), 0.0f);
			sd_left.density = 4.0f;

			FixtureDef sd_right;
			sd_right.SetAsBox(0.1f, 1.0f, Vec2(1.0f, 0.0f), 0.0f);
			sd_right.density = 4.0f;

			BodyDef bd;
			bd.type = BodyDef::e_dynamicBody;
			bd.position.Set(0.0f, 15.0f);
			Body* body = m_world.CreateBody(&bd);
			body.CreateFixture(&sd_bottom);
			body.CreateFixture(&sd_top);
			body.CreateFixture(&sd_left);
			body.CreateFixture(&sd_right);
			body.SetMassFromShapes();
		}
#elif false
		{
			FixtureDef sd_bottom;
			sd_bottom.SetAsBox( 1.5f, 0.15f );
			sd_bottom.density = 4.0f;

			FixtureDef sd_left;
			sd_left.SetAsBox(0.15f, 2.7f, Vec2(-1.45f, 2.35f), 0.2f);
			sd_left.density = 4.0f;

			FixtureDef sd_right;
			sd_right.SetAsBox(0.15f, 2.7f, Vec2(1.45f, 2.35f), -0.2f);
			sd_right.density = 4.0f;

			BodyDef bd;
			bd.position.Set( 0.0f, 15.0f );
			Body* body = m_world.CreateBody(&bd);
			body.CreateFixture(&sd_bottom);
			body.CreateFixture(&sd_left);
			body.CreateFixture(&sd_right);
			body.SetMassFromShapes();
		}
#else
		{
			BodyDef bd;
			bd.position.Set(-5.0f, 20.0f);
			bd.isBullet = true;
			Body* body = m_world.CreateBody(&bd);
			body.SetAngularVelocity(RandomFloat(-50.0f, 50.0f));

			FixtureDef fd;
			fd.SetAsBox(0.1f, 4.0f);
			fd.density = 1.0f;
			fd.restitution = 0.0f;
			body.CreateFixture(&fd);
			body.SetMassFromShapes();
		}
#endif

		for (int32 i = 0; i < 0; ++i)
		{
			BodyDef bd;
			bd.position.Set(0.0f, 15.0f + i);
			bd.isBullet = true;
			Body* body = m_world.CreateBody(&bd);
			body.SetAngularVelocity(RandomFloat(-50.0f, 50.0f));

			FixtureDef fd;
			fd.radius = 0.25f;
			fd.density = 1.0f;
			fd.restitution = 0.0f;
			body.CreateFixture(&fd);
			body.SetMassFromShapes();
		}
#endif
        }

        public override void Step(Settings settings)
        {
            if (_stepCount == 10)
            {
                _stepCount += 0;
            }

            base.Step(settings);


            if (Collision.GjkCalls > 0)
            {
                OpenGLDebugDraw.DrawString(5, _textLine, string.Format("gjk calls = {0}, ave gjk iters = {1}, max gjk iters = {2}",
                    Collision.GjkCalls, Collision.GjkIters / Collision.GjkCalls, Collision.GjkMaxIters));
                _textLine += 15;
            }

            if (Collision.ToiCalls > 0)
            {
                OpenGLDebugDraw.DrawString(5, _textLine, string.Format("toi calls = {0}, ave toi iters = {1}, max toi iters = {2}",
                                    Collision.ToiCalls, Collision.ToiIters / Collision.ToiCalls, Collision.ToiMaxRootIters));
                _textLine += 15;
                OpenGLDebugDraw.DrawString(5, _textLine, string.Format("ave toi root iters = {0}, max toi root iters = {1}",
                    Collision.ToiRootIters / Collision.ToiCalls, Collision.ToiMaxRootIters));
                _textLine += 15;
            }
        }

        public static Test Create()
        {
            return new CCDTest();
        }
    }
}
