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
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.TestBed.Tests
{
    public class LineJointTest : Test
    {
        private LineJointTest()
        {
            Body ground;
            {
                PolygonShape shape = new PolygonShape(PolygonTools.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f)), 0);

                
                ground = World.CreateBody();
                ground.CreateFixture(shape);
            }

            {
                PolygonShape shape = new PolygonShape(PolygonTools.CreateRectangle(0.5f, 2.0f), 1);

                Body body = World.CreateBody();
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(0.0f, 7.0f);

                body.CreateFixture(shape);

                Vector2 axis = new Vector2(2.0f, 1.0f);
                axis.Normalize();
                //LineJoint jd = new LineJoint(ground, body, new Vector2(0.0f, 8.5f),new Vector2(0.0f, 1.5f), axis);
                FixedLineJoint jd = new FixedLineJoint(body, /*ground, */new Vector2(0.0f, 8.5f), axis);
                jd.MotorSpeed = 100.0f;
                jd.MaxMotorForce = 100.0f;
                jd.MotorEnabled = false;
                jd.LowerLimit = -4.0f;
                jd.UpperLimit = 4.0f;
                jd.EnableLimit = true;
                World.CreateJoint(jd);
                _jd = jd;
            }
        }

        internal static Test Create()
        {
            return new LineJointTest();
        }

        FixedLineJoint _jd;
        //LineJoint _jd;
    }
}