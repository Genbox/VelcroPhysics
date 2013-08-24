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
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Testbed.Tests
{
    public class CircleBenchmarkTest : Test
    {
        private const int XCount = 30;
        private const int YCount = 15;

        private CircleBenchmarkTest()
        {
            Body ground = BodyFactory.CreateBody(World);

            // Floor
            EdgeShape ashape = new EdgeShape(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
            ground.CreateFixture(ashape);

            // Left wall
            ashape = new EdgeShape(new Vector2(-40.0f, 0.0f), new Vector2(-40.0f, 45.0f));
            ground.CreateFixture(ashape);

            // Right wall
            ashape = new EdgeShape(new Vector2(40.0f, 0.0f), new Vector2(40.0f, 45.0f));
            ground.CreateFixture(ashape);

            // Roof
            ashape = new EdgeShape(new Vector2(-40.0f, 45.0f), new Vector2(40.0f, 45.0f));
            ground.CreateFixture(ashape);

            CircleShape shape = new CircleShape(1.0f, 1);

            for (int i = 0; i < XCount; i++)
            {
                for (int j = 0; j < YCount; ++j)
                {
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(-38f + 2.1f * i, 2.0f + 2.0f * j);

                    body.CreateFixture(shape);
                }
            }
        }

        public static Test Create()
        {
            return new CircleBenchmarkTest();
        }
    }
}