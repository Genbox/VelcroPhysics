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

using System.Diagnostics;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class PyramidTest : Test
    {
        private const int Count = 20;

        private PyramidTest()
        {
            {
                Body ground = _world.CreateBody();

                Vertices edge = PolygonTools.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                PolygonShape shape = new PolygonShape(edge, 0);
                ground.CreateFixture(shape);
            }

            sw.Start();

            {
                Vertices box = PolygonTools.CreateBox(0.5f, 0.5f);
                PolygonShape shape = new PolygonShape(box, 5);

                Vector2 x = new Vector2(-7.0f, 0.75f);
                Vector2 deltaX = new Vector2(0.5625f, 1.25f);
                Vector2 deltaY = new Vector2(1.125f, 0.0f);

                for (int i = 0; i < Count; ++i)
                {
                    Vector2 y = x;

                    for (int j = i; j < Count; ++j)
                    {
                        Body body = _world.CreateBody();
                        body.BodyType = BodyType.Dynamic;
                        body.Position = y;
                        body.CreateFixture(shape);

                        y += deltaY;
                    }

                    x += deltaX;
                }

                //Vertices gear = PolygonTools.CreateEllipse(0.5f, 1f, 4);
                //PolygonShape gearShape = new PolygonShape(gear, 5);
                //Body gearBody = _world.CreateBody();
                //gearBody.BodyType = BodyType.Dynamic;
                //gearBody.Position = x;
                //gearBody.CreateFixture(gearShape);

            }

            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds.ToString());
        }

        Stopwatch sw = new Stopwatch();

        //void Step(Framework.Settings settings)
        //{
        //	// We need higher accuracy for the pyramid.
        //	int velocityIterations = settings.velocityIterations;
        //	int positionIterations = settings.positionIterations;
        //	settings.velocityIterations = b2Max(8, velocityIterations);
        //	settings.positionIterations = b2Max(1, positionIterations);
        //	base.Step(settings);
        //	settings.velocityIterations = velocityIterations;
        //	settings.positionIterations = positionIterations;
        //}

        public static Test Create()
        {
            return new PyramidTest();
        }
    }
}