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

using System;
using Genbox.VelcroPhysics.Collision.ContactSystem;
using Genbox.VelcroPhysics.Collision.Narrowphase;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Genbox.VelcroPhysics.Dynamics.Solver;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework
{
    public class Test : IDisposable
    {
        private FixedMouseJoint _fixedMouseJoint;
        internal DebugView.DebugView DebugView;
        internal int StepCount;
        internal int TextLine;
        internal World World;

        protected Test()
        {
            World = new World(new Vector2(0.0f, -10.0f));

            TextLine = 30;

            World.JointRemoved += JointRemoved;
            World.ContactManager.PreSolve += PreSolve;
            World.ContactManager.PostSolve += PostSolve;

            StepCount = 0;
        }

        public Game1 GameInstance { protected get; set; }

        public virtual void Initialize()
        {
            DebugView = new DebugView.DebugView(World);
            DebugView.LoadContent(GameInstance.GraphicsDevice, GameInstance.Content);
        }

        protected virtual void JointRemoved(Joint joint)
        {
            if (_fixedMouseJoint == joint)
                _fixedMouseJoint = null;
        }

        public virtual void Update(GameSettings settings, GameTime gameTime)
        {
            float timeStep = Math.Min((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f, 1f / 30f);

            if (settings.Pause)
            {
                if (settings.SingleStep)
                    settings.SingleStep = false;
                else
                    timeStep = 0.0f;

                DrawString("****PAUSED****");
            }
            else
                World.Step(timeStep);

            if (timeStep > 0.0f)
                ++StepCount;
        }

        public virtual void Keyboard(KeyboardManager keyboard)
        {
            //TODO:
            //if (keyboard.IsNewKeyPress(Keys.F11))
            //    WorldSerializer.Serialize(World, "out.xml");

            //if (keyboard.IsNewKeyPress(Keys.F12))
            //{
            //    World = WorldSerializer.Deserialize("out.xml");
            //    Initialize();
            //}
        }

        public virtual void Gamepad(GamePadManager gamepad) { }

        public virtual void Mouse(MouseManager mouse)
        {
            Vector2 position = GameInstance.ConvertScreenToWorld(mouse.NewPosition);

            if (mouse.NewState.LeftButton == ButtonState.Released && mouse.OldState.LeftButton == ButtonState.Pressed)
                MouseUp();
            else if (mouse.NewState.LeftButton == ButtonState.Pressed && mouse.OldState.LeftButton == ButtonState.Released)
                MouseDown(position);

            MouseMove(position);
        }

        private void MouseDown(Vector2 p)
        {
            if (_fixedMouseJoint != null)
                return;

            Fixture fixture = World.TestPoint(p);

            if (fixture != null)
            {
                Body body = fixture.Body;
                _fixedMouseJoint = new FixedMouseJoint(body, p);
                _fixedMouseJoint.MaxForce = 1000.0f * body.Mass;

                JointHelper.LinearStiffness(5.0f, 0.7f, body, null, out float stiffness, out float damping);
                _fixedMouseJoint.Stiffness = stiffness;
                _fixedMouseJoint.Damping = damping;

                World.AddJoint(_fixedMouseJoint);
                body.Awake = true;
            }
        }

        private void MouseUp()
        {
            if (_fixedMouseJoint != null)
            {
                World.RemoveJoint(_fixedMouseJoint);
                _fixedMouseJoint = null;
            }
        }

        private void MouseMove(Vector2 position)
        {
            if (_fixedMouseJoint != null)
                _fixedMouseJoint.WorldAnchorB = position;
        }

        protected virtual void PreSolve(Contact contact, ref Manifold oldManifold) { }

        protected virtual void PostSolve(Contact contact, ContactVelocityConstraint contactConstraint) { }

        protected void DrawString(string text)
        {
            DebugView.DrawString(50, TextLine, text);
            TextLine += 15;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                DebugView?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}