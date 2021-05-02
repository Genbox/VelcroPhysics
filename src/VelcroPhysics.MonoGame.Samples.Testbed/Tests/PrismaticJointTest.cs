/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
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

using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    public class PrismaticJointTest : Test
    {
        private readonly PrismaticJoint _joint;
        private readonly bool _enableLimit = true;
        private readonly bool _enableMotor = false;
        private readonly float _motorSpeed = 10.0f;

        private PrismaticJointTest()
        {
            Body ground = BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            PolygonShape shape = new PolygonShape(PolygonUtils.CreateRectangle(1.0f, 1f), 5);

            Body body = BodyFactory.CreateBody(World);
            body.BodyType = BodyType.Dynamic;
            body.Position = new Vector2(0.0f, 10.0f);
            body.Rotation = 0.5f * MathConstants.Pi;
            body.CreateFixture(shape);

            _joint = new PrismaticJoint(ground, body, body.Position, new Vector2(1.0f, 0.0f), true);

            _joint.MotorSpeed = _motorSpeed;
            _joint.MaxMotorForce = 10000.0f;
            _joint.MotorEnabled = _enableMotor;
            _joint.LowerLimit = -10.0f;
            _joint.UpperLimit = 10.0f;
            _joint.LimitEnabled = _enableLimit;

            World.AddJoint(_joint);
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.L))
                _joint.LimitEnabled = !_joint.LimitEnabled;

            if (keyboardManager.IsNewKeyPress(Keys.M))
                _joint.MotorEnabled = !_joint.MotorEnabled;

            if (keyboardManager.IsNewKeyPress(Keys.S))
                _joint.MotorSpeed -= 0.1f;

            base.Keyboard(keyboardManager);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);
            DrawString("Keys: (l) limits, (m) motors, (s) speed");
        }

        internal static Test Create()
        {
            return new PrismaticJointTest();
        }
    }
}