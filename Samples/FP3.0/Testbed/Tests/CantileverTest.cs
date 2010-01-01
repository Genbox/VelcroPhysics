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
    public class CantileverTest : Test
    {
        private const int Count = 8;

        private CantileverTest()
        {
            Body ground;
            {
                BodyDef bd = new BodyDef();
                ground = _world.CreateBody(bd);

                Vertices edge = PolygonTools.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                PolygonShape shape = new PolygonShape(edge, 0.0f);
                Fixture fixture = ground.CreateFixture(shape);
            }

            {
                Vertices box = PolygonTools.CreateBox(0.5f, 0.125f);
                PolygonShape shape = new PolygonShape(box, 20.0f);

                WeldJointDef jd = new WeldJointDef();

                Body prevBody = ground;
                for (int i = 0; i < Count; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(-14.5f + 1.0f * i, 5.0f);
                    Body body = _world.CreateBody(bd);
                    Fixture fixture = body.CreateFixture(shape);

                    Vector2 anchor = new Vector2(-15.0f + 1.0f * i, 5.0f);
                    jd.Initialize(prevBody, body, anchor);
                    _world.CreateJoint(jd);

                    prevBody = body;
                }
            }

            {
                Vertices box = PolygonTools.CreateBox(0.5f, 0.125f);
                PolygonShape shape = new PolygonShape(box, 20.0f);

                WeldJointDef jd = new WeldJointDef();

                Body prevBody = ground;
                for (int i = 0; i < Count; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(-14.5f + 1.0f * i, 15.0f);
                    bd.InertiaScale = 10.0f;
                    Body body = _world.CreateBody(bd);
                    body.CreateFixture(shape);

                    Vector2 anchor = new Vector2(-15.0f + 1.0f * i, 15.0f);
                    jd.Initialize(prevBody, body, anchor);
                    _world.CreateJoint(jd);

                    prevBody = body;
                }
            }

            {
                Vertices box = PolygonTools.CreateBox(0.5f, 0.125f);
                PolygonShape shape = new PolygonShape(box, 20.0f);

                WeldJointDef jd = new WeldJointDef();

                Body prevBody = ground;
                for (int i = 0; i < Count; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(-4.5f + 1.0f * i, 5.0f);
                    Body body = _world.CreateBody(bd);
                    body.CreateFixture(shape);

                    if (i > 0)
                    {
                        Vector2 anchor = new Vector2(-5.0f + 1.0f * i, 5.0f);
                        jd.Initialize(prevBody, body, anchor);
                        _world.CreateJoint(jd);
                    }

                    prevBody = body;
                }
            }


            {
                Vertices box = PolygonTools.CreateBox(0.5f, 0.125f);
                PolygonShape shape = new PolygonShape(box, 20.0f);

                WeldJointDef jd = new WeldJointDef();

                Body prevBody = ground;
                for (int i = 0; i < Count; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(5.5f + 1.0f * i, 10.0f);
                    bd.InertiaScale = 10.0f;
                    Body body = _world.CreateBody(bd);
                    body.CreateFixture(shape);

                    if (i > 0)
                    {
                        Vector2 anchor = new Vector2(5.0f + 1.0f * i, 10.0f);
                        jd.Initialize(prevBody, body, anchor);
                        _world.CreateJoint(jd);
                    }

                    prevBody = body;
                }
            }

            //Triangels
            for (int i = 0; i < 2; ++i)
            {
                Vertices vertices = new Vertices(3);
                vertices[0] = new Vector2(-0.5f, 0.0f);
                vertices[1] = new Vector2(0.5f, 0.0f);
                vertices[2] = new Vector2(0.0f, 1.5f);

                PolygonShape shape = new PolygonShape(vertices, 1.0f);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-8.0f + 8.0f * i, 12.0f);
                Body body = _world.CreateBody(bd);
                body.CreateFixture(shape);
            }

            //Circles            
            for (int i = 0; i < 2; ++i)
            {
                CircleShape shape = new CircleShape(0.5f, 1.0f);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-6.0f + 6.0f * i, 10.0f);
                Body body = _world.CreateBody(bd);
                body.CreateFixture(shape);
            }
        }

        internal static Test Create()
        {
            return new CantileverTest();
        }
    }
}