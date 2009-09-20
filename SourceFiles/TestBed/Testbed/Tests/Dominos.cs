/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2008 Erin Catto http://www.gphysics.com

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace TestBed
{
    public class Dominos : Test
    {
        public Dominos()
		{
            Body b1;
            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vec2(-40.0f, 0.0f), new Vec2(40.0f, 0.0f));

                BodyDef bd = new BodyDef();
                b1 = _world.CreateBody(bd);
                b1.CreateFixture(shape,0);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(6.0f, 0.25f);

                BodyDef bd = new BodyDef();
                bd.Position.Set(-1.5f, 10.0f);
                Body ground = _world.CreateBody(bd);
                ground.CreateFixture(shape,0);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(0.1f, 1.0f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Density = 20.0f;
                fd.Friction = 0.1f;

                for (int i = 0; i < 10; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Position.Set(-6.0f + 1.0f * i, 11.25f);
                    Body body = _world.CreateBody(bd);
                    body.CreateFixture(fd);
                }
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(7.0f, 0.25f, Vec2.Zero, 0.3f);

                BodyDef bd = new BodyDef();
                bd.Position.Set(1.0f, 6.0f);
                Body ground = _world.CreateBody(bd);
                ground.CreateFixture(shape,0);
            }

            Body b2;
            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(0.25f, 1.5f);

                BodyDef bd = new BodyDef();
                bd.Position.Set(-7.0f, 4.0f);
                 b2 = _world.CreateBody(bd);
                b2.CreateFixture(shape,0);
            }

            Body b3;
            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(6.0f, 0.125f);

                BodyDef bd = new BodyDef();
                bd.Position.Set(-0.9f, 1.0f);
                bd.Angle = -0.15f;

                b3 = _world.CreateBody(bd);
                b3.CreateFixture(shape, 10.0f);
            }

            RevoluteJointDef jd = new RevoluteJointDef();
            Vec2 anchor = new Vec2();

            anchor.Set(-2.0f, 1.0f);
            jd.Initialize(b1, b3, anchor);
            jd.CollideConnected = true;
            _world.CreateJoint(jd);

            Body b4;
            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(0.25f, 0.25f);

                BodyDef bd  = new BodyDef();
                bd.Position.Set(-10.0f, 15.0f);
                b4 = _world.CreateBody(bd);
                b4.CreateFixture(shape, 10.0f);
            }

            anchor.Set(-7.0f, 15.0f);
            jd.Initialize(b2, b4, anchor);
            _world.CreateJoint(jd);

            Body b5;
            {
                BodyDef bd = new BodyDef();
                bd.Position.Set(6.5f, 3.0f);
                b5 = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                FixtureDef fd = new FixtureDef();

                fd.Shape = shape;
                fd.Density  = 10.0f;
                fd.Friction = 0.1f;

                shape.SetAsBox(1.0f, 0.1f,new Vec2(0.0f, -0.9f), 0.0f);
                b5.CreateFixture(fd);

                shape.SetAsBox(0.1f, 1.0f, new Vec2(-0.9f, 0.0f), 0.0f);
                b5.CreateFixture(fd);

                shape.SetAsBox(0.1f, 1.0f,new Vec2(0.9f, 0.0f), 0.0f);
                b5.CreateFixture(fd);

            }

            anchor.Set(6.0f, 2.0f);
            jd.Initialize(b1, b5, anchor);
            _world.CreateJoint(jd);

            Body b6;
            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(1.0f, 0.1f);

                BodyDef bd = new BodyDef();
                bd.Position.Set(6.5f, 4.1f);
                b6 = _world.CreateBody(bd);
                b6.CreateFixture(shape, 30.0f);
            }

            anchor.Set(7.5f, 4.0f);
            jd.Initialize(b5, b6, anchor);
            _world.CreateJoint(jd);

            Body b7;
            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(0.1f, 1.0f);

                BodyDef bd = new BodyDef();
                bd.Position.Set(7.4f, 1.0f);

                b7 = _world.CreateBody(bd);
                b7.CreateFixture(shape, 10.0f);
            }

            DistanceJointDef djd  =new DistanceJointDef();
            djd.Body1 = b3;
            djd.Body2 = b7;
            djd.LocalAnchor1.Set(6.0f, 0.0f);
            djd.LocalAnchor2.Set(0.0f, -1.0f);
            Vec2 d = djd.Body2.GetWorldPoint(djd.LocalAnchor2) - djd.Body1.GetWorldPoint(djd.LocalAnchor1);
            djd.Length = d.Length();
            _world.CreateJoint(djd);

            {
                float radius = 0.2f;

                CircleShape shape = new CircleShape();
                shape._radius = radius;

                for (int i = 0; i < 4; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Position.Set(5.9f + 2.0f * radius * i, 2.4f);
                    Body body = _world.CreateBody(bd);
                    body.CreateFixture(shape, 10.0f);
                }
            }
		}

        public static Test Create()
        {
            return new Dominos();
        }
    }
}