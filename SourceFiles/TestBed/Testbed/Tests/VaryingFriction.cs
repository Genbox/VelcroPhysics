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
using Box2DX.Dynamics;
using Box2DX.Common;

namespace TestBed
{

    class VaryingFriction : Test
    {
        public VaryingFriction()
        {
            {
                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vec2(-40.0f, 0.0f), new Vec2(40.0f, 0.0f));
                ground.CreateFixture(shape, 0);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(13.0f, 0.25f);

                BodyDef bd = new BodyDef();
                bd.Position.Set(-4.0f, 22.0f);
                bd.Angle = -0.25f;

                Body ground = _world.CreateBody(bd);
                ground.CreateFixture(shape, 0);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(0.25f, 1.0f);

                BodyDef bd = new BodyDef();
                bd.Position.Set(10.5f, 19.0f);

                Body ground = _world.CreateBody(bd);
                ground.CreateFixture(shape, 0);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(13.0f, 0.25f);

                BodyDef bd = new BodyDef();
                bd.Position.Set(4.0f, 14.0f);
                bd.Angle = 0.25f;

                Body ground = _world.CreateBody(bd);
                ground.CreateFixture(shape, 0);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(0.25f, 1.0f);

                BodyDef bd = new BodyDef();
                bd.Position.Set(-10.5f, 11.0f);

                Body ground = _world.CreateBody(bd);
                ground.CreateFixture(shape, 0);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(13.0f, 0.25f);

                BodyDef bd = new BodyDef();
                bd.Position.Set(-4.0f, 6.0f);
                bd.Angle = -0.25f;

                Body ground = _world.CreateBody(bd);
                ground.CreateFixture(shape, 0);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(0.5f, 0.5f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Density = 25.0f;

                float[] friction = { 0.75f, 0.5f, 0.35f, 0.1f, 0.0f };

                for (int i = 0; i < 5; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Position.Set(-15.0f + 4.0f * i, 28.0f);
                    Body body = _world.CreateBody(bd);

                    fd.Friction = friction[i];
                    body.CreateFixture(fd);
                }
            }
        }

        public static Test Create()
        {
            return new VaryingFriction();
        }
    }
}
