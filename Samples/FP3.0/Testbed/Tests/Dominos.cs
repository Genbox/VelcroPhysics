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
    public class Dominos : Test
    {
        public Dominos()
        {
            Body b1 = null;
            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

                BodyDef bd = new BodyDef();
                b1 = _world.CreateBody(bd);
                b1.CreateFixture(shape, 0.0f);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(6.0f, 0.25f);

                BodyDef bd = new BodyDef();
                bd.position = new Vector2(-1.5f, 10.0f);
                Body ground = _world.CreateBody(bd);
                ground.CreateFixture(shape, 0.0f);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(0.1f, 1.0f);

                FixtureDef fd = new FixtureDef();
                fd.shape = shape;
                fd.density = 20.0f;
                fd.friction = 0.1f;

                for (int i = 0; i < 10; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.type = BodyType.Dynamic;
                    bd.position = new Vector2(-6.0f + 1.0f * i, 11.25f);
                    Body body = _world.CreateBody(bd);
                    body.CreateFixture(fd);
                }
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(7.0f, 0.25f, Vector2.Zero, 0.3f);

                BodyDef bd = new BodyDef();
                bd.position = new Vector2(1.0f, 6.0f);
                Body ground = _world.CreateBody(bd);
                ground.CreateFixture(shape, 0.0f);
            }

            Body b2 = null;
            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(0.25f, 1.5f);

                BodyDef bd = new BodyDef();
                bd.position = new Vector2(-7.0f, 4.0f);
                b2 = _world.CreateBody(bd);
                b2.CreateFixture(shape, 0.0f);
            }

            Body b3 = null;
            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(6.0f, 0.125f);

                BodyDef bd = new BodyDef();
                bd.type = BodyType.Dynamic;
                bd.position = new Vector2(-0.9f, 1.0f);
                bd.angle = -0.15f;

                b3 = _world.CreateBody(bd);
                b3.CreateFixture(shape, 10.0f);
            }

            RevoluteJointDef jd = new RevoluteJointDef();
            Vector2 anchor;

            anchor = new Vector2(-2.0f, 1.0f);
            jd.Initialize(b1, b3, anchor);
            jd.collideConnected = true;
            _world.CreateJoint(jd);

            Body b4 = null;
            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(0.25f, 0.25f);

                BodyDef bd = new BodyDef();
                bd.type = BodyType.Dynamic;
                bd.position = new Vector2(-10.0f, 15.0f);
                b4 = _world.CreateBody(bd);
                b4.CreateFixture(shape, 10.0f);
            }

            anchor = new Vector2(-7.0f, 15.0f);
            jd.Initialize(b2, b4, anchor);
            _world.CreateJoint(jd);

            Body b5 = null;
            {
                BodyDef bd = new BodyDef();
                bd.type = BodyType.Dynamic;
                bd.position = new Vector2(6.5f, 3.0f);
                b5 = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                FixtureDef fd = new FixtureDef();

                fd.shape = shape;
                fd.density = 10.0f;
                fd.friction = 0.1f;

                shape.SetAsBox(1.0f, 0.1f, new Vector2(0.0f, -0.9f), 0.0f);
                b5.CreateFixture(fd);

                shape.SetAsBox(0.1f, 1.0f, new Vector2(-0.9f, 0.0f), 0.0f);
                b5.CreateFixture(fd);

                shape.SetAsBox(0.1f, 1.0f, new Vector2(0.9f, 0.0f), 0.0f);
                b5.CreateFixture(fd);
            }

            anchor = new Vector2(6.0f, 2.0f);
            jd.Initialize(b1, b5, anchor);
            _world.CreateJoint(jd);

            Body b6 = null;
            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(1.0f, 0.1f);

                BodyDef bd = new BodyDef();
                bd.type = BodyType.Dynamic;
                bd.position = new Vector2(6.5f, 4.1f);
                b6 = _world.CreateBody(bd);
                b6.CreateFixture(shape, 30.0f);
            }

            anchor = new Vector2(7.5f, 4.0f);
            jd.Initialize(b5, b6, anchor);
            _world.CreateJoint(jd);

            Body b7 = null;
            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(0.1f, 1.0f);

                BodyDef bd = new BodyDef();
                bd.type = BodyType.Dynamic;
                bd.position = new Vector2(7.4f, 1.0f);

                b7 = _world.CreateBody(bd);
                b7.CreateFixture(shape, 10.0f);
            }

            DistanceJointDef djd = new DistanceJointDef();
            djd.bodyA = b3;
            djd.bodyB = b7;
            djd.localAnchorA = new Vector2(6.0f, 0.0f);
            djd.localAnchorB = new Vector2(0.0f, -1.0f);
            Vector2 d = djd.bodyB.GetWorldPoint(djd.localAnchorB) - djd.bodyA.GetWorldPoint(djd.localAnchorA);
            djd.length = d.Length();
            _world.CreateJoint(djd);

            {
                float radius = 0.2f;

                CircleShape shape = new CircleShape();
                shape._radius = radius;

                for (int i = 0; i < 4; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.type = BodyType.Dynamic;
                    bd.position = new Vector2(5.9f + 2.0f * radius * i, 2.4f);
                    Body body = _world.CreateBody(bd);
                    body.CreateFixture(shape, 10.0f);
                }
            }
        }

        internal static Test Create()
	    {
		    return new Dominos();
	    }
    }
}
