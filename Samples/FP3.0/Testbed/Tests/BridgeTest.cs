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
    public class BridgeTest : Test
    {
        private const int Count = 30;

        private BridgeTest()
        {
            Body ground;
            {
                BodyDef bd = new BodyDef();
                ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.CreateFixture(shape, 0.0f);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(0.5f, 0.125f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Density = 20.0f;
                fd.Friction = 0.2f;

                RevoluteJointDef jd = new RevoluteJointDef();

                Body prevBody = ground;
                for (int i = 0; i < Count; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(-14.5f + 1.0f*i, 5.0f);
                    Body body = _world.CreateBody(bd);
                    body.CreateFixture(fd);

                    Vector2 anchor = new Vector2(-15.0f + 1.0f*i, 5.0f);
                    jd.Initialize(prevBody, body, anchor);
                    _world.CreateJoint(jd);

                    if (i == (Count >> 1))
                    {
                    }

                    prevBody = body;
                }

                Vector2 anchor2 = new Vector2(-15.0f + 1.0f*Count, 5.0f);
                jd.Initialize(prevBody, ground, anchor2);
                _world.CreateJoint(jd);
            }

            Vector2[] vertices = new Vector2[3];
            for (int i = 0; i < 2; ++i)
            {
                vertices[0] = new Vector2(-0.5f, 0.0f);
                vertices[1] = new Vector2(0.5f, 0.0f);
                vertices[2] = new Vector2(0.0f, 1.5f);

                PolygonShape shape = new PolygonShape();
                shape.Set(vertices, 3);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Density = 1.0f;

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-8.0f + 8.0f*i, 12.0f);
                Body body = _world.CreateBody(bd);
                body.CreateFixture(fd);
            }

            for (int i = 0; i < 3; ++i)
            {
                CircleShape shape = new CircleShape(0.5f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Density = 1.0f;

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-6.0f + 6.0f*i, 10.0f);
                Body body = _world.CreateBody(bd);
                body.CreateFixture(fd);
            }
        }

        internal static Test Create()
        {
            return new BridgeTest();
        }
    }
}