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
    public class PrismaticTest : Test
    {
        private PrismaticTest()
        {
            PolygonShape shape = new PolygonShape(5.0f);
            shape.SetAsBox(2.0f, 0.5f);

            Body body = World.Add();
            body.BodyType = BodyType.Dynamic;
            body.Position = new Vector2(-10.0f, 10.0f);
            body.Rotation = 0.5f * Settings.Pi;

            body.CreateFixture(shape);

            _joint = new FixedPrismaticJoint(body, Vector2.Zero, new Vector2(1.0f, 0.0f));
            _joint.MotorSpeed = 10.0f;
            _joint.MaxMotorForce = 1000.0f;
            _joint.MotorEnabled = true;
            _joint.LowerLimit = 0.0f;
            _joint.UpperLimit = 20.0f;
            _joint.LimitEnabled = true;

            World.Add(_joint);
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.L) && oldState.IsKeyUp(Keys.L))
            {
                _joint.LimitEnabled = !_joint.LimitEnabled;
            }
            if (state.IsKeyDown(Keys.M) && oldState.IsKeyUp(Keys.M))
            {
                _joint.MotorEnabled = !_joint.MotorEnabled;
            }
            if (state.IsKeyDown(Keys.P) && oldState.IsKeyUp(Keys.P))
            {
                _joint.MotorSpeed = -_joint.MotorSpeed;
            }
        }

        public override void Update(Framework.Settings settings)
        {
            base.Update(settings);
            DebugView.DrawString(50, TextLine, "Keys: (l) limits, (m) motors, (p) speed");
            TextLine += 15;
            float force = _joint.MotorForce;
            DebugView.DrawString(50, TextLine, "Motor Force = {0:n}", force);
            TextLine += 15;
        }

        internal static Test Create()
        {
            return new PrismaticTest();
        }

        private FixedPrismaticJoint _joint;
    }
}
