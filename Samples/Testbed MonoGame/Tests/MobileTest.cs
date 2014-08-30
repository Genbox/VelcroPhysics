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
    public class MobileTest : Test
    {
        private const int Depth = 4;

        public MobileTest()
        {
            Body ground = BodyFactory.CreateBody(World, new Vector2(0, 20f));

            const float a = 0.5f;
            Vector2 h = new Vector2(0.0f, a);

            Body root = AddNode(ground, Vector2.Zero, 0, 3.0f, a);

            JointFactory.CreateRevoluteJoint(World, ground, root, Vector2.Zero, h);
        }

        private Body AddNode(Body parent, Vector2 localAnchor, int depth, float offset, float a)
        {
            const float density = 20.0f;
            Vector2 h = new Vector2(0.0f, a);

            Vector2 p = parent.Position + localAnchor - h;

            Body body = BodyFactory.CreateRectangle(World, 0.5f * a, a * 2, density, p);
            body.BodyType = BodyType.Dynamic;

            if (depth == Depth)
            {
                return body;
            }

            Vector2 a1 = new Vector2(offset, -a);
            Vector2 a2 = new Vector2(-offset, -a);
            Body body1 = AddNode(body, a1, depth + 1, 0.5f * offset, a);
            Body body2 = AddNode(body, a2, depth + 1, 0.5f * offset, a);

            JointFactory.CreateRevoluteJoint(World, body, body1, a1, h);
            JointFactory.CreateRevoluteJoint(World, body, body2, a2, h);

            return body;
        }

        public static Test Create()
        {
            return new MobileTest();
        }
    }
}