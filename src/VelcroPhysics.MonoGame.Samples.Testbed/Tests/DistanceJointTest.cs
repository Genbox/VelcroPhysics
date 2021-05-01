/*
* Velcro Physics:
* Copyright (c) 2021 Ian Qvist
* 
* MIT License
*
* Copyright (c) 2019 Erin Catto
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using System.Diagnostics;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    public class DistanceJointTest : Test
    {
        private readonly DistanceJoint _joint;

        private DistanceJointTest()
        {
            Body ground = BodyFactory.CreateEdge(World, new Vector2(-40, 0), new Vector2(40, 0));

            Body body = BodyFactory.CreateRectangle(World, 1f, 1f, 5.0f, new Vector2(0, 5), bodyType: BodyType.Dynamic);
            body.SleepingAllowed = false;
            body.AngularDamping = 0.1f;

            float hertz = 1.0f;
            float dampingRatio = 0.7f;

            _joint = JointFactory.CreateDistanceJoint(World, ground, body, new Vector2(0, 15), body.Position, true);
            _joint.CollideConnected = true;

            JointHelper.LinearStiffness(hertz, dampingRatio, _joint.BodyA, _joint.BodyB, out float stiffness, out float damping);
            _joint.Stiffness = stiffness;
            _joint.Damping = damping;
            _joint.MaxLength = _joint.Length;
            _joint.MinLength = _joint.Length;
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
            DebugView.DrawString(30, TextLine, "Press + to increase and - to decrease max length");
            DebugView.DrawString(30, TextLine += TextLine, "Max length: " + _joint.MaxLength);
            DebugView.EndCustomDraw();
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsKeyDown(Keys.Add))
                _joint.MaxLength += 0.1f;

            if (keyboardManager.IsKeyDown(Keys.Subtract))
                _joint.MinLength -= 0.1f;

            base.Keyboard(keyboardManager);
        }

        internal static Test Create()
        {
            return new DistanceJointTest();
        }
    }
}