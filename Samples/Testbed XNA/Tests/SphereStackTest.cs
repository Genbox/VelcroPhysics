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
    public class SphereStackTest : Test
    {
        private const int Count = 10;
        private Body[] _bodies = new Body[Count];

        private SphereStackTest()
        {
            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            {
                CircleShape shape = new CircleShape(1.0f, 1);

                for (int i = 0; i < Count; ++i)
                {
                    _bodies[i] = BodyFactory.CreateBody(World);
                    _bodies[i].BodyType = BodyType.Dynamic;
                    _bodies[i].Position = new Vector2(0.0f, 4.0f + 3.0f * i);

                    _bodies[i].CreateFixture(shape);

                    _bodies[i].LinearVelocity = new Vector2(0.0f, -50.0f);
                }
            }
        }

        public static Test Create()
        {
            return new SphereStackTest();
        }
    }
}