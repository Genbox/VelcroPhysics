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

using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Testbed.Tests
{
    public class VaryingRestitutionTest : Test
    {
        private VaryingRestitutionTest()
        {
            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            float[] restitution = new[] { 0.0f, 0.1f, 0.3f, 0.5f, 0.75f, 0.9f, 1.0f };

            for (int i = 0; i < 7; ++i)
            {
                Body body = BodyFactory.CreateCircle(World, 1.0f, 1, new Vector2(-10.0f + 3.0f * i, 20.0f));
                body.BodyType = BodyType.Dynamic;
                body.Restitution = restitution[i];
            }
        }

        internal static Test Create()
        {
            return new VaryingRestitutionTest();
        }
    }
}