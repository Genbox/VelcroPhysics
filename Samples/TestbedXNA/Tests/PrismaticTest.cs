/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.box2d.org 
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
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class PrismaticTest : Test
    {
        private FixedPrismaticJoint _fixedJoint;
        private PrismaticJoint _joint;

        private PrismaticTest()
        {
            Body ground;
            {
                ground = BodyFactory.CreateBody(World);

                EdgeShape shape3 = new EdgeShape(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.CreateFixture(shape3);
            }

            PolygonShape shape = new PolygonShape(5);
            shape.SetAsBox(2.0f, 0.5f);

            Body body = BodyFactory.CreateBody(World);
            body.BodyType = BodyType.Dynamic;
            body.Position = new Vector2(0.0f, 10.0f);

            body.CreateFixture(shape);

            _fixedJoint = new FixedPrismaticJoint(body, body.Position, new Vector2(0.5f, 1.0f));
            _fixedJoint.MotorSpeed = 5.0f;
            _fixedJoint.MaxMotorForce = 1000.0f;
            _fixedJoint.MotorEnabled = true;
            _fixedJoint.LowerLimit = -10.0f;
            _fixedJoint.UpperLimit = 20.0f;
            _fixedJoint.LimitEnabled = true;

            World.AddJoint(_fixedJoint);

            PolygonShape shape2 = new PolygonShape(5);
            shape2.SetAsBox(2.0f, 0.5f);

            Body body2 = BodyFactory.CreateBody(World);
            body2.BodyType = BodyType.Dynamic;
            body2.Position = new Vector2(10.0f, 10.0f);

            body2.CreateFixture(shape2);

            _joint = new PrismaticJoint(ground, body2, ground.GetLocalPoint(body2.Position), Vector2.Zero,
                                        new Vector2(0.5f, 1.0f));
            _joint.MotorSpeed = 5.0f;
            _joint.MaxMotorForce = 1000.0f;
            _joint.MotorEnabled = true;
            _joint.LowerLimit = -10.0f;
            _joint.UpperLimit = 20.0f;
            _joint.LimitEnabled = true;

            World.AddJoint(_joint);
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.L))
            {
                _fixedJoint.LimitEnabled = !_fixedJoint.LimitEnabled;
                _joint.LimitEnabled = !_joint.LimitEnabled;
            }
            if (keyboardManager.IsNewKeyPress(Keys.M))
            {
                _fixedJoint.MotorEnabled = !_fixedJoint.MotorEnabled;
                _joint.MotorEnabled = !_joint.MotorEnabled;
            }
            if (keyboardManager.IsNewKeyPress(Keys.P))
            {
                _fixedJoint.MotorSpeed = -_fixedJoint.MotorSpeed;
                _joint.MotorSpeed = -_joint.MotorSpeed;
            }

            base.Keyboard(keyboardManager);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);
            DebugView.DrawString(50, TextLine, "Keys: (l) limits, (m) motors, (p) speed");
            TextLine += 15;
        }

        internal static Test Create()
        {
            return new PrismaticTest();
        }
    }
}