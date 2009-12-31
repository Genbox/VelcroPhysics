﻿/*
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
    public class VaryingFrictionTest : Test
    {
        private VaryingFrictionTest()
        {
            {
                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.CreateFixture(shape, 0.0f);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(13.0f, 0.25f);

                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(-4.0f, 22.0f);
                bd.Angle = -0.25f;

                Body ground = _world.CreateBody(bd);
                ground.CreateFixture(shape, 0.0f);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(0.25f, 1.0f);

                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(10.5f, 19.0f);

                Body ground = _world.CreateBody(bd);
                ground.CreateFixture(shape, 0.0f);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(13.0f, 0.25f);

                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(4.0f, 14.0f);
                bd.Angle = 0.25f;

                Body ground = _world.CreateBody(bd);
                ground.CreateFixture(shape, 0.0f);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(0.25f, 1.0f);

                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(-10.5f, 11.0f);

                Body ground = _world.CreateBody(bd);
                ground.CreateFixture(shape, 0.0f);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(13.0f, 0.25f);

                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(-4.0f, 6.0f);
                bd.Angle = -0.25f;

                Body ground = _world.CreateBody(bd);
                ground.CreateFixture(shape, 0.0f);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(0.5f, 0.5f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Density = 25.0f;

                float[] friction = new float[] {0.75f, 0.5f, 0.35f, 0.1f, 0.0f};

                for (int i = 0; i < 5; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(-15.0f + 4.0f*i, 28.0f);
                    Body body = _world.CreateBody(bd);

                    fd.Friction = friction[i];
                    body.CreateFixture(fd);
                }
            }
        }

        internal static Test Create()
        {
            return new VaryingFrictionTest();
        }
    }
}