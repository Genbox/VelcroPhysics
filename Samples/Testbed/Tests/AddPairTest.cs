/*
* Farseer Physics Engine
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

using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Testbed.Tests
{
    public class AddPairTest : Test
    {
        private AddPairTest()
        {
            World.Gravity = Vector2.Zero;

            const float minX = -6.0f;
            const float maxX = 0.0f;
            const float minY = 4.0f;
            const float maxY = 6.0f;

            for (int i = 0; i < 400; ++i)
            {
                Body body = BodyFactory.CreateCircle(World, 0.1f, 0.01f, new Vector2(Rand.RandomFloat(minX, maxX), Rand.RandomFloat(minY, maxY)));
                body.BodyType = BodyType.Dynamic;
            }

            {
                Body body = BodyFactory.CreateRectangle(World, 3, 3, 1, new Vector2(-40, 5));
                body.BodyType = BodyType.Dynamic;
                body.IsBullet = true;
                body.LinearVelocity = new Vector2(150.0f, 0.0f);
            }
        }

        public static Test Create()
        {
            return new AddPairTest();
        }
    }
}