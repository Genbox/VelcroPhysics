﻿/*
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
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class PrismaticTest : Test
    {
        private PrismaticTest()
        {
            Body ground;
            {
                BodyDef bd = new BodyDef();
                ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.CreateFixture(shape, 0.0f);
            }

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(2.0f, 0.5f);

                BodyDef bd = new BodyDef();
                bd.type = BodyType.Dynamic;
                bd.position = new Vector2(-10.0f, 10.0f);
                bd.angle = 0.5f * Settings.Pi;
                Body body = _world.CreateBody(bd);
                body.CreateFixture(shape, 5.0f);

                PrismaticJointDef pjd = new PrismaticJointDef();

                // Bouncy limit
                pjd.Initialize(ground, body, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f));

                // Non-bouncy limit
                //pjd.Initialize(ground, body, new Vector2(-10.0f, 10.0f), new Vector2(1.0f, 0.0f));

                pjd.MotorSpeed = 10.0f;
                pjd.MaxMotorForce = 1000.0f;
                pjd.EnableMotor = true;
                pjd.LowerTranslation = 0.0f;
                pjd.UpperTranslation = 20.0f;
                pjd.EnableLimit = true;

                _joint = (PrismaticJoint)_world.CreateJoint(pjd);
            }
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.L) && oldState.IsKeyUp(Keys.L))
            {
                _joint.EnableLimit(!_joint.IsLimitEnabled());
            }
            if (state.IsKeyDown(Keys.M) && oldState.IsKeyUp(Keys.M))
            {
                _joint.EnableMotor(!_joint.IsMotorEnabled());
            }
            if (state.IsKeyDown(Keys.P) && oldState.IsKeyUp(Keys.P))
            {
                _joint.SetMotorSpeed(-_joint.GetMotorSpeed());
            }
        }

        public override void Step(Framework.Settings settings)
        {
            base.Step(settings);
            _debugView.DrawString(50, _textLine, "Keys: (l) limits, (m) motors, (p) speed");
            _textLine += 15;
            float force = _joint.GetMotorForce();
            _debugView.DrawString(50, _textLine, "Motor Force = {0:n}", force);
            _textLine += 15;
        }

        internal static Test Create()
        {
            return new PrismaticTest();
        }

        private PrismaticJoint _joint;
    }
}