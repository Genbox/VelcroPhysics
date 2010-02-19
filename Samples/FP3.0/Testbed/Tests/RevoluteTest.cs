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
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class RevoluteTest : Test
    {
        private RevoluteTest()
        {
            Body ground;
            {
                ground = World.Add();

                PolygonShape shape = new PolygonShape(0);
                shape.SetAsEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.CreateFixture(shape);
            }

            {
                CircleShape shape = new CircleShape(0.5f, 5);

                Body body = World.Add();
                body.Position = new Vector2(0.0f, 20.0f);
                body.BodyType = BodyType.Dynamic;

                body.CreateFixture(shape);

                //const float w = 100.0f;
                //body.AngularVelocity = w;
                //body.LinearVelocity = new Vector2(-8.0f * w, 0.0f);

                _joint = new RevoluteJoint(body,ground,new Vector2(0.0f, 10.0f));
                _joint.MotorSpeed = 1.0f * Settings.Pi;
                _joint.MaxMotorTorque = 10000.0f;
                _joint.MotorEnabled = false;
                _joint.LowerLimit = -0.25f * Settings.Pi;
                _joint.UpperLimit = 0.5f * Settings.Pi;
                _joint.LimitEnabled = false;
                _joint.CollideConnected = true;

                World.Add(_joint);
            }
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.L) && oldState.IsKeyUp(Keys.L))
            {
                _joint.LimitEnabled = !_joint.LimitEnabled;
            }

            if (state.IsKeyDown(Keys.S) && oldState.IsKeyUp(Keys.S))
            {
                _joint.MotorEnabled = !_joint.MotorEnabled;
            }
        }

        public override void Update(Framework.Settings settings)
        {
            base.Update(settings);
            DebugView.DrawString(50, TextLine, "Keys: (l) limits, (a) left, (s) off, (d) right");
            TextLine += 15;
            //float torque1 = _joint1.GetMotorTorque();
            //_debugDraw.DrawString(50, TextLine, "Motor Torque = %4.0f, %4.0f : Motor Force = %4.0f", (float) torque1, (float) torque2, (float) force3);
            //TextLine += 15;
        }

        internal static Test Create()
        {
            return new RevoluteTest();
        }

        private RevoluteJoint _joint;
    }
}