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
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Testbed.Tests
{
    /// <summary>
    /// This tests bullet collision and provides an example of a gameplay scenario.
    /// </summary>
    public class PinballTest : Test
    {
        private Body _ball;
        private RevoluteJoint _leftJoint;
        private RevoluteJoint _rightJoint;

        private PinballTest()
        {
            // Ground body
            Body ground;
            {
                ground = BodyFactory.CreateBody(World);

                Vertices vertices = new Vertices(5);
                vertices.Add(new Vector2(0.0f, -2.0f));
                vertices.Add(new Vector2(8.0f, 6.0f));
                vertices.Add(new Vector2(8.0f, 20.0f));
                vertices.Add(new Vector2(-8.0f, 20.0f));
                vertices.Add(new Vector2(-8.0f, 6.0f));

                ChainShape chain = new ChainShape(vertices, true);
                ground.CreateFixture(chain);
            }

            // Flippers
            {
                Vector2 p1 = new Vector2(-2.0f, 0f);
                Vector2 p2 = new Vector2(2.0f, 0f);

                Body leftFlipper = BodyFactory.CreateBody(World, p1);
                leftFlipper.BodyType = BodyType.Dynamic;
                Body rightFlipper = BodyFactory.CreateBody(World, p2);
                rightFlipper.BodyType = BodyType.Dynamic;

                PolygonShape box = new PolygonShape(1);
                box.Vertices = PolygonTools.CreateRectangle(1.75f, 0.1f);

                leftFlipper.CreateFixture(box);
                rightFlipper.CreateFixture(box);

                _leftJoint = new RevoluteJoint(ground, leftFlipper, p1, Vector2.Zero);
                _leftJoint.MaxMotorTorque = 1000.0f;
                _leftJoint.LimitEnabled = true;
                _leftJoint.MotorEnabled = true;
                _leftJoint.MotorSpeed = 0.0f;
                _leftJoint.LowerLimit = -30.0f * Settings.Pi / 180.0f;
                _leftJoint.UpperLimit = 5.0f * Settings.Pi / 180.0f;
                World.AddJoint(_leftJoint);

                _rightJoint = new RevoluteJoint(ground, rightFlipper, p2, Vector2.Zero);
                _rightJoint.MaxMotorTorque = 1000.0f;
                _rightJoint.LimitEnabled = true;
                _rightJoint.MotorEnabled = true;
                _rightJoint.MotorSpeed = 0.0f;
                _rightJoint.LowerLimit = -5.0f * Settings.Pi / 180.0f;
                _rightJoint.UpperLimit = 30.0f * Settings.Pi / 180.0f;
                World.AddJoint(_rightJoint);
            }

            // Circle character
            {
                _ball = BodyFactory.CreateBody(World, new Vector2(1.0f, 15.0f));
                _ball.BodyType = BodyType.Dynamic;
                _ball.IsBullet = true;
                _ball.CreateFixture(new CircleShape(0.2f, 1.0f));
            }
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsKeyDown(Keys.A))
            {
                _leftJoint.MotorSpeed = 20.0f;
                _rightJoint.MotorSpeed = -20.0f;
            }
            if (keyboardManager.IsKeyUp(Keys.A))
            {
                _leftJoint.MotorSpeed = -10.0f;
                _rightJoint.MotorSpeed = 10.0f;
            }

            base.Keyboard(keyboardManager);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);
            DrawString("Press 'a' to control the flippers");
        }

        internal static Test Create()
        {
            return new PinballTest();
        }
    }
}