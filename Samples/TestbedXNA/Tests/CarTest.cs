/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
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

using System;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class CarTest : Test
    {
        private Body _car;
        private float _hz;
        private Body _wheel1;
        private Body _wheel2;

        private float _zeta;

        private CarTest()
        {
            _hz = 2.0f;
            _zeta = 0.7f;

            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            {
                PolygonShape box = new PolygonShape(1);
                box.SetAsBox(2.0f, 0.5f);

                CircleShape circle = new CircleShape(0.5f, 1);

                _car = new Body(World);
                _car.BodyType = BodyType.Dynamic;
                _car.Position = new Vector2(0.0f, 1.0f);
                _car.CreateFixture(box);

                _wheel1 = new Body(World);
                _wheel1.BodyType = BodyType.Dynamic;
                _wheel1.Position = new Vector2(-1.5f, 0.5f);
                _wheel1.CreateFixture(circle);

                _wheel2 = new Body(World);
                _wheel2.BodyType = BodyType.Dynamic;
                _wheel2.Position = new Vector2(1.5f, 0.5f);
                _wheel2.CreateFixture(circle);

                Vector2 axis = new Vector2(0.0f, 1.0f);
                LineJoint jd = new LineJoint(_car, _wheel1, _wheel1.Position, axis);
                jd.MotorSpeed = 1.0f;
                jd.MaxMotorTorque = 10.0f;
                jd.MotorEnabled = true;
                jd.FrequencyHz = _hz;
                jd.DampingRatio = _zeta;
                World.AddJoint(jd);

                LineJoint jd2 = new LineJoint(_car, _wheel2, _wheel2.Position, axis);
                jd2.MotorSpeed = 0.0f;
                jd2.MaxMotorTorque = 10.0f;
                jd2.MotorEnabled = false;
                jd2.FrequencyHz = _hz;
                jd2.DampingRatio = _zeta;
                World.AddJoint(jd2);
            }
        }

        private bool On(Fixture fixturea, Fixture fixtureb, Contact contact)
        {
            return true;
        }

        internal static Test Create()
        {
            return new CarTest();
        }
    }
}