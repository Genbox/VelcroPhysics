/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
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

using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Testbed.Tests
{
    public class CantileverTest : Test
    {
        private const int Count = 8;

        private CantileverTest()
        {
            Body ground = BodyFactory.CreateEdge(World, new Vector2(-40, 0), new Vector2(40, 0));

            {
                PolygonShape shape = new PolygonShape(20);
                shape.Vertices = PolygonTools.CreateRectangle(0.5f, 0.125f);

                Body prevBody = ground;
                for (int i = 0; i < Count; ++i)
                {
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(-14.5f + 1.0f * i, 5.0f);
                    body.CreateFixture(shape);

                    Vector2 anchor = new Vector2(-15.0f + 1.0f * i, 5.0f);
                    WeldJoint jd = new WeldJoint(prevBody, body, anchor, anchor, true);
                    World.AddJoint(jd);

                    prevBody = body;
                }
            }

            {
                PolygonShape shape = new PolygonShape(20f);
                shape.Vertices = PolygonTools.CreateRectangle(1f, 0.125f);

                Body prevBody = ground;
                for (int i = 0; i < 3; ++i)
                {
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(-14.0f + 2.0f * i, 15.0f);
                    body.CreateFixture(shape);

                    Vector2 anchor = new Vector2(-15.0f + 2.0f * i, 15.0f);
                    WeldJoint jd = new WeldJoint(prevBody, body, anchor, anchor, true);
                    jd.FrequencyHz = 5.0f;
                    jd.DampingRatio = 0.7f;
                    World.AddJoint(jd);

                    prevBody = body;
                }
            }

            {
                PolygonShape shape = new PolygonShape(20f);
                shape.Vertices = PolygonTools.CreateRectangle(0.5f, 0.125f);

                Body prevBody = ground;
                for (int i = 0; i < Count; ++i)
                {
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(-4.5f + 1.0f * i, 5.0f);

                    body.CreateFixture(shape);

                    if (i > 0)
                    {
                        Vector2 anchor = new Vector2(-5.0f + 1.0f * i, 5.0f);
                        WeldJoint jd = new WeldJoint(prevBody, body, anchor, anchor, true);
                        World.AddJoint(jd);
                    }

                    prevBody = body;
                }
            }

            {
                PolygonShape shape = new PolygonShape(20f);
                shape.Vertices = PolygonTools.CreateRectangle(0.5f, 0.125f);

                Body prevBody = ground;
                for (int i = 0; i < Count; ++i)
                {
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(5.5f + 1.0f * i, 10.0f);

                    body.CreateFixture(shape);

                    if (i > 0)
                    {
                        Vector2 anchor = new Vector2(5.0f + 1.0f * i, 10.0f);
                        WeldJoint jd = new WeldJoint(prevBody, body, anchor, anchor, true);
                        jd.FrequencyHz = 8.0f;
                        jd.DampingRatio = 0.7f;

                        World.AddJoint(jd);
                    }

                    prevBody = body;
                }
            }

            //Triangels
            Vertices vertices = new Vertices(3);
            vertices.Add(new Vector2(-0.5f, 0.0f));
            vertices.Add(new Vector2(0.5f, 0.0f));
            vertices.Add(new Vector2(0.0f, 1.5f));

            for (int i = 0; i < 2; ++i)
            {
                PolygonShape shape = new PolygonShape(vertices, 1);

                Body body = BodyFactory.CreateBody(World);
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(-8.0f + 8.0f * i, 12.0f);

                body.CreateFixture(shape);
            }

            //Circles            
            for (int i = 0; i < 2; ++i)
            {
                CircleShape shape = new CircleShape(0.5f, 1);

                Body body = BodyFactory.CreateBody(World);
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(-6.0f + 6.0f * i, 10.0f);

                body.CreateFixture(shape);
            }
        }

        internal static Test Create()
        {
            return new CantileverTest();
        }
    }
}