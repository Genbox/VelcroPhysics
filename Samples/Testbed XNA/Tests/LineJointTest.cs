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
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class WheelJointTest : Test
    {
        private WheelJointTest()
        {
            Body ground = BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            {
                PolygonShape shape = new PolygonShape(1);
                shape.Vertices = PolygonTools.CreateRectangle(0.5f, 2.0f);

                Body body = new Body(World);
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(0.0f, 7.0f);
                
                body.CreateFixture(shape);

                Vector2 axis = new Vector2(-1000.0f, -2.0f);
                axis.Normalize();

                WheelJoint jd = new WheelJoint(ground, body, new Vector2(0, 8.5f), axis);
                jd.MotorSpeed = 1.0f;
                jd.MaxMotorTorque = 1000.0f;
                jd.MotorEnabled = true;
                jd.SpringFrequencyHz = 1.0f;
                jd.SpringDampingRatio = 0.2f;
                World.AddJoint(jd);

                PolygonShape shape2 = new PolygonShape(1);
                shape2.Vertices = PolygonTools.CreateRectangle(0.5f, 2.0f);
                Body body2 = BodyFactory.CreatePolygon(World, shape2.Vertices, 0.5f);
                body2.BodyType = BodyType.Dynamic;
                body2.Position = new Vector2(10.0f, 7.0f);
            }
        }

        internal static Test Create()
        {
            return new WheelJointTest();
        }
    }
}