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

using System;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Testbed.Tests
{
    public class MotorJointTest : Test
    {
        private MotorJoint _joint;
        private float _time;

        MotorJointTest()
        {
            Body ground = BodyFactory.CreateEdge(World, new Vector2(-20, 0), new Vector2(20, 0));

            // Define motorized body
            Body body = BodyFactory.CreateRectangle(World, 4, 1, 2, new Vector2(0, 8));
            body.BodyType = BodyType.Dynamic;
            body.Friction = 0.6f;

            _joint = new MotorJoint(ground, body);
            _joint.MaxForce = 1000.0f;
            _joint.MaxTorque = 1000.0f;

            World.AddJoint(_joint);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            if (!settings.Pause && settings.Hz > 0.0f)
                _time += 1.0f / settings.Hz;

            Vector2 linearOffset = new Vector2();
            linearOffset.X = 6.0f * (float)Math.Sin(2.0f * _time);
            linearOffset.Y = 8.0f + 4.0f * (float)Math.Sin(1.0f * _time);

            float angularOffset = 4.0f * _time;

            _joint.LinearOffset = linearOffset;
            _joint.AngularOffset = angularOffset;
        }

        public static Test Create()
        {
            return new MotorJointTest();
        }
    }
}