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
    public class DominosTest : Test
    {
        private DominosTest()
        {
            Body b1;
            {
                Vertices edge = PolygonTools.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                PolygonShape shape = new PolygonShape(edge, 0);

                BodyDef bd = new BodyDef();
                b1 = _world.CreateBody(bd);
                b1.CreateFixture(shape);
            }

            {
                Vertices box = PolygonTools.CreateBox(6.0f, 0.25f);
                PolygonShape shape = new PolygonShape(box, 0);

                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(-1.5f, 10.0f);
                Body ground = _world.CreateBody(bd);
                ground.CreateFixture(shape);
            }

            {
                Vertices box = PolygonTools.CreateBox(0.1f, 1.0f);
                PolygonShape shape = new PolygonShape(box, 20);

                for (int i = 0; i < 10; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(-6.0f + 1.0f * i, 11.25f);
                    Body body = _world.CreateBody(bd);
                    Fixture fixture = body.CreateFixture(shape);
                    fixture.SetFriction(0.1f);
                }
            }

            {
                Vertices box = PolygonTools.CreateBox(7.0f, 0.25f, Vector2.Zero, 0.3f);
                PolygonShape shape = new PolygonShape(box, 0);

                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(1.0f, 6.0f);
                Body ground = _world.CreateBody(bd);
                ground.CreateFixture(shape);
            }

            Body b2;
            {
                Vertices box = PolygonTools.CreateBox(0.25f, 1.5f);
                PolygonShape shape = new PolygonShape(box, 0);

                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(-7.0f, 4.0f);
                b2 = _world.CreateBody(bd);
                b2.CreateFixture(shape);
            }

            Body b3;
            {
                Vertices box = PolygonTools.CreateBox(6.0f, 0.125f);
                PolygonShape shape = new PolygonShape(box, 10);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-0.9f, 1.0f);
                bd.Angle = -0.15f;

                b3 = _world.CreateBody(bd);
                b3.CreateFixture(shape);
            }

            RevoluteJointDef jd = new RevoluteJointDef();

            Vector2 anchor = new Vector2(-2.0f, 1.0f);
            jd.Initialize(b1, b3, anchor);
            jd.CollideConnected = true;
            _world.CreateJoint(jd);

            Body b4;
            {
                Vertices box = PolygonTools.CreateBox(0.25f, 0.25f);
                PolygonShape shape = new PolygonShape(box, 10);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-10.0f, 15.0f);
                b4 = _world.CreateBody(bd);
                b4.CreateFixture(shape);
            }

            anchor = new Vector2(-7.0f, 15.0f);
            jd.Initialize(b2, b4, anchor);
            _world.CreateJoint(jd);

            Body b5;
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(6.5f, 3.0f);
                b5 = _world.CreateBody(bd);

                Vertices vertices = PolygonTools.CreateBox(1.0f, 0.1f, new Vector2(0.0f, -0.9f), 0.0f);
                PolygonShape shape = new PolygonShape(vertices, 10);

                Fixture fix = b5.CreateFixture(shape);
                fix.SetFriction(0.1f);

                vertices = PolygonTools.CreateBox(0.1f, 1.0f, new Vector2(-0.9f, 0.0f), 0.0f);

                shape.Set(vertices);
                fix = b5.CreateFixture(shape);
                fix.SetFriction(0.1f);

                vertices = PolygonTools.CreateBox(0.1f, 1.0f, new Vector2(0.9f, 0.0f), 0.0f);

                shape.Set(vertices);
                fix = b5.CreateFixture(shape);
                fix.SetFriction(0.1f);

            }

            anchor = new Vector2(6.0f, 2.0f);
            jd.Initialize(b1, b5, anchor);
            _world.CreateJoint(jd);

            Body b6;
            {
                Vertices box = PolygonTools.CreateBox(1.0f, 0.1f);
                PolygonShape shape = new PolygonShape(box, 30);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(6.5f, 4.1f);
                b6 = _world.CreateBody(bd);
                b6.CreateFixture(shape);
            }

            anchor = new Vector2(7.5f, 4.0f);
            jd.Initialize(b5, b6, anchor);
            _world.CreateJoint(jd);

            Body b7;
            {
                Vertices box = PolygonTools.CreateBox(0.1f, 1.0f);
                PolygonShape shape = new PolygonShape(box, 10);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(7.4f, 1.0f);

                b7 = _world.CreateBody(bd);
                b7.CreateFixture(shape);
            }

            DistanceJointDef djd = new DistanceJointDef();
            djd.BodyA = b3;
            djd.BodyB = b7;
            djd.LocalAnchorA = new Vector2(6.0f, 0.0f);
            djd.LocalAnchorB = new Vector2(0.0f, -1.0f);
            Vector2 d = djd.BodyB.GetWorldPoint(djd.LocalAnchorB) - djd.BodyA.GetWorldPoint(djd.LocalAnchorA);
            djd.Length = d.Length();
            _world.CreateJoint(djd);

            {
                const float radius = 0.2f;

                CircleShape shape = new CircleShape(radius, 10);

                for (int i = 0; i < 4; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(5.9f + 2.0f * radius * i, 2.4f);
                    Body body = _world.CreateBody(bd);
                    body.CreateFixture(shape);
                }
            }
        }

        internal static Test Create()
        {
            return new DominosTest();
        }
    }
}