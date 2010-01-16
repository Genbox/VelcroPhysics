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
    public class ChainTest : Test
    {
        private ChainTest()
        {
            Body ground;
            {
                
                ground = World.CreateBody();

                Vertices edge = PolygonTools.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                PolygonShape shape = new PolygonShape(edge, 0);
                ground.CreateFixture(shape);
            }

            {
                Vertices box = PolygonTools.CreateRectangle(0.6f, 0.125f);
                PolygonShape shape = new PolygonShape(box, 20);


                const float y = 25.0f;
                Body prevBody = ground;
                for (int i = 0; i < 30; ++i)
                {
                    Body body = World.CreateBody();
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(0.5f + i, y);

                    Fixture fixture = body.CreateFixture(shape);
                    fixture.Friction = 0.2f;
                    Vector2 anchor = new Vector2(i, y);
                    RevoluteJoint jd = new RevoluteJoint(prevBody,body,anchor);
                    jd.CollideConnected = false;
                    World.CreateJoint(jd);

                    prevBody = body;
                }
            }
        }

        internal static Test Create()
        {
            return new ChainTest();
        }
    }
}