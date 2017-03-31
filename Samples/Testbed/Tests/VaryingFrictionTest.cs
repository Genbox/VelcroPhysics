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
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Testbed.Tests
{
    public class VaryingFrictionTest : Test
    {
        private VaryingFrictionTest()
        {
            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            {
                Vertices box = PolygonTools.CreateRectangle(13.0f, 0.25f);
                PolygonShape shape = new PolygonShape(box, 0);

                Body ground = BodyFactory.CreateBody(World);
                ground.Position = new Vector2(-4.0f, 22.0f);
                ground.Rotation = -0.25f;

                ground.CreateFixture(shape);
            }

            {
                Vertices box = PolygonTools.CreateRectangle(0.25f, 1.0f);
                PolygonShape shape = new PolygonShape(box, 0);

                Body ground = BodyFactory.CreateBody(World);
                ground.Position = new Vector2(10.5f, 19.0f);

                ground.CreateFixture(shape);
            }

            {
                Vertices box = PolygonTools.CreateRectangle(13.0f, 0.25f);
                PolygonShape shape = new PolygonShape(box, 0);

                Body ground = BodyFactory.CreateBody(World);
                ground.Position = new Vector2(4.0f, 14.0f);
                ground.Rotation = 0.25f;

                ground.CreateFixture(shape);
            }

            {
                Vertices box = PolygonTools.CreateRectangle(0.25f, 1.0f);
                PolygonShape shape = new PolygonShape(box, 0);

                Body ground = BodyFactory.CreateBody(World);
                ground.Position = new Vector2(-10.5f, 11.0f);

                ground.CreateFixture(shape);
            }

            {
                Vertices box = PolygonTools.CreateRectangle(13f, 0.25f);
                PolygonShape shape = new PolygonShape(box, 0);

                Body ground = BodyFactory.CreateBody(World);
                ground.Position = new Vector2(-4.0f, 6.0f);
                ground.Rotation = -0.25f;

                ground.CreateFixture(shape);
            }

            {
                Vertices box = PolygonTools.CreateRectangle(0.5f, 0.5f);
                PolygonShape shape = new PolygonShape(box, 25);

                float[] friction = new[] { 0.75f, 0.5f, 0.35f, 0.1f, 0.0f };

                for (int i = 0; i < 5; ++i)
                {
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(-15.0f + 4.0f * i, 28.0f);

                    Fixture fixture = body.CreateFixture(shape);
                    fixture.Friction = friction[i];
                }
            }
        }

        internal static Test Create()
        {
            return new VaryingFrictionTest();
        }
    }
}