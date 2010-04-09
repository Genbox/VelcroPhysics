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

                //The big fixed wheel
                CircleShape shape = new CircleShape(5.0f, 5);

                Body body = World.Add();
                body.Position = new Vector2(0.0f, 15.0f);
                body.BodyType = BodyType.Dynamic;

                body.CreateFixture(shape);

                _fixedJoint = new FixedRevoluteJoint(body, body.Position);
                _fixedJoint.MotorSpeed = 0.25f * Settings.Pi;
                _fixedJoint.MaxMotorTorque = 5000.0f;
                _fixedJoint.MotorEnabled = true;
                _fixedJoint.LowerLimit = -0.25f * Settings.Pi;
                _fixedJoint.UpperLimit = 0.5f * Settings.Pi;
                _fixedJoint.LimitEnabled = false;

                World.Add(_fixedJoint);

                // The small wheel attached to the big one
                CircleShape shape2 = new CircleShape(1.0f, 5);

                Body body2 = World.Add();
                body2.Position = new Vector2(0.0f, 12.0f);
                body2.BodyType = BodyType.Dynamic;

                body2.CreateFixture(shape2);

                _joint = new RevoluteJoint(body, body2, new Vector2(0.0f,0.0f));
                _joint.MotorSpeed = 1.0f * Settings.Pi;
                _joint.MaxMotorTorque = 5000.0f;
                _joint.MotorEnabled = true;
                _joint.LowerLimit = -0.25f * Settings.Pi;
                _joint.UpperLimit = 0.5f * Settings.Pi;
                _joint.LimitEnabled = false;
                _joint.CollideConnected = false;

                World.Add(_joint);
            }
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.L) && oldState.IsKeyUp(Keys.L))
            {
                _joint.LimitEnabled = !_joint.LimitEnabled;
                _fixedJoint.LimitEnabled = !_fixedJoint.LimitEnabled;
            }

            if (state.IsKeyDown(Keys.M) && oldState.IsKeyUp(Keys.M))
            {
                _joint.MotorEnabled = !_joint.MotorEnabled;
                _fixedJoint.MotorEnabled = !_fixedJoint.MotorEnabled;
            }
        }

        public override void Update(Framework.Settings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);
            DebugView.DrawString(50, TextLine, "Keys: (l) limits on/off, (m) motor on/off");
            /*TextLine += 15;
            DebugView.DrawString(50, TextLine, "_joint:{0:n}",_joint.JointAngle);
            */
        }

        internal static Test Create()
        {
            return new RevoluteTest();
        }

        private RevoluteJoint _joint;
        private FixedRevoluteJoint _fixedJoint;
    }
}