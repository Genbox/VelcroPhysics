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

using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Testbed.Tests
{
    public class BodyTypesTest : Test
    {
        private Body _attachment;
        private Body _platform;
        private float _speed;

        private BodyTypesTest()
        {
            //Ground
            Body ground = BodyFactory.CreateEdge(World, new Vector2(-20.0f, 0.0f), new Vector2(20.0f, 0.0f));

            // Define attachment
            {
                _attachment = BodyFactory.CreateRectangle(World, 1, 4, 2);
                _attachment.BodyType = BodyType.Dynamic;
                _attachment.Position = new Vector2(0.0f, 3.0f);
            }

            // Define platform
            {
                _platform = BodyFactory.CreateRectangle(World, 8.0f, 1f, 2);
                _platform.BodyType = BodyType.Dynamic;
                _platform.Position = new Vector2(0.0f, 5.0f);
                _platform.Friction = 0.6f;

                RevoluteJoint rjd = new RevoluteJoint(_attachment, _platform, new Vector2(0, 5), true);
                rjd.MaxMotorTorque = 50.0f;
                rjd.MotorEnabled = true;
                World.AddJoint(rjd);

                PrismaticJoint pjd = new PrismaticJoint(ground, _platform, new Vector2(0.0f, 5.0f), new Vector2(1.0f, 0.0f), true);
                pjd.MaxMotorForce = 1000.0f;
                pjd.MotorEnabled = true;
                pjd.LowerLimit = -10.0f;
                pjd.UpperLimit = 10.0f;
                pjd.LimitEnabled = true;

                World.AddJoint(pjd);

                _speed = 3.0f;
            }

            // Create a payload
            {
                Body body = BodyFactory.CreateRectangle(World, 1.5f, 1.5f, 2);
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(0.0f, 8.0f);
                body.Friction = 0.6f;
            }
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsKeyDown(Keys.D))
                _platform.BodyType = BodyType.Dynamic;
            if (keyboardManager.IsKeyDown(Keys.S))
                _platform.BodyType = BodyType.Static;
            if (keyboardManager.IsKeyDown(Keys.K))
            {
                _platform.BodyType = BodyType.Kinematic;
                _platform.LinearVelocity = new Vector2(-_speed, 0.0f);
                _platform.AngularVelocity = 0.0f;
            }

            base.Keyboard(keyboardManager);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            // Drive the kinematic body.
            if (_platform.BodyType == BodyType.Kinematic)
            {
                Transform tf;
                _platform.GetTransform(out tf);
                Vector2 p = tf.p;
                Vector2 v = _platform.LinearVelocity;

                if ((p.X < -10.0f && v.X < 0.0f) ||
                    (p.X > 10.0f && v.X > 0.0f))
                {
                    v.X = -v.X;
                    _platform.LinearVelocity = v;
                }
            }

            base.Update(settings, gameTime);
            DrawString("Keys: (d) dynamic, (s) static, (k) kinematic");
        }

        internal static Test Create()
        {
            return new BodyTypesTest();
        }
    }
}