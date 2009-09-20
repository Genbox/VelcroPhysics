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
    public class CompoundShapes : Test
    {
        public CompoundShapes()
        {
            {
                BodyDef bd = new BodyDef();
                bd.Position.Set(0.0f, 0.0f);
                Body body = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vec2(50.0f, 0.0f), new Vec2(-50.0f, 0.0f));

                body.CreateFixture(shape, 0);
            }

            {
                CircleShape circle1 = new CircleShape();
                circle1._radius = 0.5f;
                circle1._p.Set(-0.5f, 0.5f);

                CircleShape circle2 = new CircleShape();
                circle2._radius = 0.5f;
                circle2._p.Set(0.5f, 0.5f);

                for (int i = 0; i < 10; ++i)
                {
                    float x = Math.Random(-0.1f, 0.1f);
                    BodyDef bd = new BodyDef();
                    bd.Position.Set(x + 5.0f, 1.05f + 2.5f * i);
                    bd.Angle = Math.Random(-Box2DX.Common.Settings.PI, Box2DX.Common.Settings.PI);
                    Body body = _world.CreateBody(bd);
                    body.CreateFixture(circle1, 2.0f);
                    body.CreateFixture(circle2, 0);
                }
            }

            {
                PolygonShape polygon1 = new PolygonShape();
                polygon1.SetAsBox(0.25f, 0.5f);

                PolygonShape polygon2 = new PolygonShape();
                polygon2.SetAsBox(0.25f, 0.5f, new Vec2(0.0f, -0.5f), 0.5f * Box2DX.Common.Settings.PI);

                for (int i = 0; i < 10; ++i)
                {
                    float x = Math.Random(-0.1f, 0.1f);
                    BodyDef bd = new BodyDef();
                    bd.Position.Set(x - 5.0f, 1.05f + 2.5f * i);
                    bd.Angle = Math.Random(-Box2DX.Common.Settings.PI, Box2DX.Common.Settings.PI);
                    Body body = _world.CreateBody(bd);
                    body.CreateFixture(polygon1, 2.0f);
                    body.CreateFixture(polygon2, 2.0f);
                }
            }

            {
                Transform xf1 = new Transform();
                xf1.R.Set(0.3524f * Box2DX.Common.Settings.PI);
                xf1.Position = Math.Mul(xf1.R, new Vec2(1.0f, 0.0f));

                Vec2[] vertices = new Vec2[3];

                PolygonShape triangle1 = new PolygonShape();
                vertices[0] = Math.Mul(xf1, new Vec2(-1.0f, 0.0f));
                vertices[1] = Math.Mul(xf1, new Vec2(1.0f, 0.0f));
                vertices[2] = Math.Mul(xf1, new Vec2(0.0f, 0.5f));
                triangle1.Set(vertices, 3);

                Transform xf2 = new Transform();
                xf2.R.Set(-0.3524f * Box2DX.Common.Settings.PI);
                xf2.Position = Math.Mul(xf2.R, new Vec2(-1.0f, 0.0f));

                PolygonShape triangle2 = new PolygonShape();
                vertices[0] = Math.Mul(xf2, new Vec2(-1.0f, 0.0f));
                vertices[1] = Math.Mul(xf2, new Vec2(1.0f, 0.0f));
                vertices[2] = Math.Mul(xf2, new Vec2(0.0f, 0.5f));
                triangle2.Set(vertices, 3);

                for (int i = 0; i < 10; ++i)
                {
                    float x = Math.Random(-0.1f, 0.1f);
                    BodyDef bd = new BodyDef();
                    bd.Position.Set(x, 2.05f + 2.5f * i);
                    bd.Angle = 0.0f;
                    Body body = _world.CreateBody(bd);
                    body.CreateFixture(triangle1, 2.0f);
                    body.CreateFixture(triangle2, 2.0f);
                }
            }

            {
                PolygonShape bottom = new PolygonShape();
                bottom.SetAsBox(1.5f, 0.15f);

                PolygonShape left = new PolygonShape();
                left.SetAsBox(0.15f, 2.7f, new Vec2(-1.45f, 2.35f), 0.2f);

                PolygonShape right = new PolygonShape();
                right.SetAsBox(0.15f, 2.7f, new Vec2(1.45f, 2.35f), -0.2f);

                BodyDef bd = new BodyDef();
                bd.Position.Set(0.0f, 2.0f);
                Body body = _world.CreateBody(bd);
                body.CreateFixture(bottom, 4.0f);
                body.CreateFixture(left, 4.0f);
                body.CreateFixture(right, 4.0f);
            }
        }

        public static Test Create()
        {
            return new CompoundShapes();
        }
    }
}