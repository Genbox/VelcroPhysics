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

using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
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
                ground = World.Add();

                PolygonShape shape3 = new PolygonShape(0);
                shape3.SetAsEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.CreateFixture(shape3);
            }

            PolygonShape shape = new PolygonShape(5.0f);
            shape.SetAsBox(2.0f, 0.5f);

            Body body = World.Add();
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

            World.Add(_fixedJoint);

            PolygonShape shape2 = new PolygonShape(5.0f);
            shape2.SetAsBox(2.0f, 0.5f);

            Body body2 = World.Add();
            body2.BodyType = BodyType.Dynamic;
            body2.Position = new Vector2(10.0f, 10.0f);

            body2.CreateFixture(shape2);

            _joint = new PrismaticJoint(ground, body2, new Vector2(0.0f, 0.0f), new Vector2(0.5f, 1.0f));
            _joint.MotorSpeed = 5.0f;
            _joint.MaxMotorForce = 1000.0f;
            _joint.MotorEnabled = true;
            _joint.LowerLimit = -10.0f;
            _joint.UpperLimit = 20.0f;
            _joint.LimitEnabled = true;

            World.Add(_joint);
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.L) && oldState.IsKeyUp(Keys.L))
            {
                _fixedJoint.LimitEnabled = !_fixedJoint.LimitEnabled;
                _joint.LimitEnabled = !_joint.LimitEnabled;
            }
            if (state.IsKeyDown(Keys.M) && oldState.IsKeyUp(Keys.M))
            {
                _fixedJoint.MotorEnabled = !_fixedJoint.MotorEnabled;
                _joint.MotorEnabled = !_joint.MotorEnabled;
            }
            if (state.IsKeyDown(Keys.P) && oldState.IsKeyUp(Keys.P))
            {
                _fixedJoint.MotorSpeed = -_fixedJoint.MotorSpeed;
                _joint.MotorSpeed = -_joint.MotorSpeed;
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);
            DebugView.DrawString(50, TextLine + 200, "Keys: (l) limits, (m) motors, (p) speed");
            TextLine += 15;
            //float force = _fixedJoint.MotorForce;
            //DebugView.DrawString(50, TextLine + 200, "Motor Force = {0:n}", force);
            /*TextLine += 15;
            DebugView.DrawString(50, TextLine + 200, "_fixedJoint joint translation = {0:n}", _fixedJoint.JointTranslation);
            TextLine += 15;
            DebugView.DrawString(50, TextLine + 200, "_joint joint translation = {0:n}", _joint.JointTranslation);
            TextLine += 15;
            DebugView.DrawString(50, TextLine + 200, "_fixedJoint joint JointSpeed = {0:n}", _fixedJoint.JointSpeed);
            TextLine += 15;
            DebugView.DrawString(50, TextLine + 200, "_joint joint JointSpeed = {0:n}", _joint.JointSpeed);*/
        }

        internal static Test Create()
        {
            return new PrismaticTest();
        }
    }
}