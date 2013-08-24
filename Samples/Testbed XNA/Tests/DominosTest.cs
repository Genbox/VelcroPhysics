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
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Testbed.Tests
{
    public class DominosTest : Test
    {
        private DominosTest()
        {
            Body b1 = BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            {
                Vertices box = PolygonTools.CreateRectangle(6.0f, 0.25f);
                PolygonShape shape = new PolygonShape(box, 0);

                Body ground = BodyFactory.CreateBody(World);
                ground.Position = new Vector2(-1.5f, 10.0f);
                ground.CreateFixture(shape);
            }

            {
                Vertices box = PolygonTools.CreateRectangle(0.1f, 1.0f);
                PolygonShape shape = new PolygonShape(box, 20);

                for (int i = 0; i < 10; ++i)
                {
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(-6.0f + 1.0f * i, 11.25f);

                    Fixture fixture = body.CreateFixture(shape);
                    fixture.Friction = 0.1f;
                }
            }

            {
                Vertices box = PolygonTools.CreateRectangle(7.0f, 0.25f, Vector2.Zero, 0.3f);
                PolygonShape shape = new PolygonShape(box, 0);

                Body ground = BodyFactory.CreateBody(World);
                ground.Position = new Vector2(1.0f, 6.0f);

                ground.CreateFixture(shape);
            }

            Body b2;
            {
                Vertices box = PolygonTools.CreateRectangle(0.25f, 1.5f);
                PolygonShape shape = new PolygonShape(box, 0);

                b2 = BodyFactory.CreateBody(World);
                b2.Position = new Vector2(-7.0f, 4.0f);

                b2.CreateFixture(shape);
            }

            Body b3;
            {
                Vertices box = PolygonTools.CreateRectangle(6.0f, 0.125f);
                PolygonShape shape = new PolygonShape(box, 10);

                b3 = BodyFactory.CreateBody(World);
                b3.BodyType = BodyType.Dynamic;
                b3.Position = new Vector2(-0.9f, 1.0f);
                b3.Rotation = -0.15f;

                b3.CreateFixture(shape);
            }

            Vector2 anchor = new Vector2(-2.0f, 1.0f);
            RevoluteJoint jd = new RevoluteJoint(b1, b3, anchor, true);
            jd.CollideConnected = true;
            World.AddJoint(jd);

            Body b4;
            {
                Vertices box = PolygonTools.CreateRectangle(0.25f, 0.25f);
                PolygonShape shape = new PolygonShape(box, 10);

                b4 = BodyFactory.CreateBody(World);
                b4.BodyType = BodyType.Dynamic;
                b4.Position = new Vector2(-10.0f, 15.0f);

                b4.CreateFixture(shape);
            }

            anchor = new Vector2(-7.0f, 15.0f);
            RevoluteJoint jd2 = new RevoluteJoint(b2, b4, anchor, true);
            World.AddJoint(jd2);

            Body b5;
            {
                b5 = BodyFactory.CreateBody(World);
                b5.BodyType = BodyType.Dynamic;
                b5.Position = new Vector2(6.5f, 3.0f);

                Vertices vertices = PolygonTools.CreateRectangle(1.0f, 0.1f, new Vector2(0.0f, -0.9f), 0.0f);
                PolygonShape shape = new PolygonShape(vertices, 10);

                Fixture fix = b5.CreateFixture(shape);
                fix.Friction = 0.1f;

                vertices = PolygonTools.CreateRectangle(0.1f, 1.0f, new Vector2(-0.9f, 0.0f), 0.0f);

                shape.Vertices = vertices;
                fix = b5.CreateFixture(shape);
                fix.Friction = 0.1f;

                vertices = PolygonTools.CreateRectangle(0.1f, 1.0f, new Vector2(0.9f, 0.0f), 0.0f);

                shape.Vertices = vertices;
                fix = b5.CreateFixture(shape);
                fix.Friction = 0.1f;
            }

            anchor = new Vector2(6.0f, 2.0f);
            RevoluteJoint jd3 = new RevoluteJoint(b1, b5, anchor, true);
            World.AddJoint(jd3);

            Body b6;
            {
                Vertices box = PolygonTools.CreateRectangle(1.0f, 0.1f);
                PolygonShape shape = new PolygonShape(box, 30);

                b6 = BodyFactory.CreateBody(World);
                b6.BodyType = BodyType.Dynamic;
                b6.Position = new Vector2(6.5f, 4.1f);

                b6.CreateFixture(shape);
            }

            anchor = new Vector2(7.50f, 4.0f);
            RevoluteJoint jd4 = new RevoluteJoint(b5, b6, anchor, true);
            jd4.CollideConnected = true;
            World.AddJoint(jd4);

            Body b7;
            {
                Vertices box = PolygonTools.CreateRectangle(0.1f, 1.0f);
                PolygonShape shape = new PolygonShape(box, 10);

                b7 = BodyFactory.CreateBody(World);
                b7.BodyType = BodyType.Dynamic;
                b7.Position = new Vector2(7.4f, 1.0f);

                b7.CreateFixture(shape);
            }

            DistanceJoint djd = new DistanceJoint(b3, b7, new Vector2(6.0f, 0.0f), new Vector2(0.0f, -1.0f));
            World.AddJoint(djd);

            {
                const float radius = 0.2f;

                CircleShape shape = new CircleShape(radius, 10);

                for (int i = 0; i < 4; ++i)
                {
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(5.9f + 2.0f * radius * i, 2.4f);

                    Fixture fix = body.CreateFixture(shape);
                    fix.OnCollision += BallCollision;
                }
            }
        }

        private bool BallCollision(Fixture fixturea, Fixture fixtureb, Contact contact)
        {
            if (fixtureb.Shape.ShapeType == ShapeType.Edge)
            {
                //Remove everything from the world
                World.Clear();

                //Add a rectangle
                BodyFactory.CreateRectangle(World, 5, 5, 1);
            }

            return true;
        }

        internal static Test Create()
        {
            return new DominosTest();
        }
    }
}