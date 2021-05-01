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

using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    /// <summary>
    /// This tests distance joints, body destruction, and joint destruction.
    /// </summary>
    public class WebTest : Test
    {
        private readonly Body[] _bodies = new Body[4];
        private readonly Joint[] _joints = new Joint[8];

        private int _removedBodies;
        private int _removedJoints;

        private WebTest()
        {
            World.JointRemoved += JointRemovedFired;
            World.BodyRemoved += BodyRemovedFired;

            Body ground = BodyFactory.CreateEdge(World, new Vector2(-40, 0), new Vector2(40, 0));

            {
                _bodies[0] = BodyFactory.CreateRectangle(World, 1f, 1f, 5, new Vector2(-5.0f, 5.0f), bodyType: BodyType.Dynamic);
                _bodies[1] = BodyFactory.CreateRectangle(World, 1f, 1f, 5, new Vector2(5.0f, 5.0f), bodyType: BodyType.Dynamic);
                _bodies[2] = BodyFactory.CreateRectangle(World, 1f, 1f, 5, new Vector2(5.0f, 15.0f), bodyType: BodyType.Dynamic);
                _bodies[3] = BodyFactory.CreateRectangle(World, 1f, 1f, 5, new Vector2(-5.0f, 15.0f), bodyType: BodyType.Dynamic);

                Vector2 p1, p2, d;

                float frequencyHz = 2.0f;
                float dampingRatio = 0.0f;

                DistanceJoint dj = new DistanceJoint(ground, _bodies[0], new Vector2(-10.0f, 0.0f), new Vector2(-0.5f, -0.5f));

                p1 = dj.BodyA.GetWorldPoint(dj.LocalAnchorA);
                p2 = dj.BodyB.GetWorldPoint(dj.LocalAnchorB);
                d = p2 - p1;
                dj.Length = d.Length();

                //Velcro: We only calculate this once as the mass is identical for all bodies
                JointHelper.LinearStiffness(frequencyHz, dampingRatio, dj.BodyA, dj.BodyB, out float stiffness, out float damping);
                dj.Stiffness = stiffness;
                dj.Damping = damping;
                _joints[0] = dj;
                World.AddJoint(_joints[0]);

                DistanceJoint dj1 = new DistanceJoint(ground, _bodies[1], new Vector2(10.0f, 0.0f), new Vector2(0.5f, -0.5f));
                dj1.Stiffness = stiffness;
                dj1.Damping = damping;
                _joints[1] = dj1;
                World.AddJoint(_joints[1]);

                DistanceJoint dj2 = new DistanceJoint(ground, _bodies[2], new Vector2(10.0f, 20.0f), new Vector2(0.5f, 0.5f));
                dj2.Stiffness = stiffness;
                dj2.Damping = damping;
                _joints[2] = dj2;
                World.AddJoint(_joints[2]);

                DistanceJoint dj3 = new DistanceJoint(ground, _bodies[3], new Vector2(-10.0f, 20.0f), new Vector2(-0.5f, 0.5f));
                dj3.Stiffness = stiffness;
                dj3.Damping = damping;
                _joints[3] = dj3;
                World.AddJoint(_joints[3]);

                DistanceJoint dj4 = new DistanceJoint(_bodies[0], _bodies[1], new Vector2(0.5f, 0.0f), new Vector2(-0.5f, 0.0f));
                dj4.Stiffness = stiffness;
                dj4.Damping = damping;
                _joints[4] = dj4;
                World.AddJoint(_joints[4]);

                DistanceJoint dj5 = new DistanceJoint(_bodies[1], _bodies[2], new Vector2(0.0f, 0.5f), new Vector2(0.0f, -0.5f));
                dj5.Stiffness = stiffness;
                dj5.Damping = damping;
                _joints[5] = dj5;
                World.AddJoint(_joints[5]);

                DistanceJoint dj6 = new DistanceJoint(_bodies[2], _bodies[3], new Vector2(-0.5f, 0.0f), new Vector2(0.5f, 0.0f));
                dj6.Stiffness = stiffness;
                dj6.Damping = damping;
                _joints[6] = dj6;
                World.AddJoint(_joints[6]);

                DistanceJoint dj7 = new DistanceJoint(_bodies[3], _bodies[0], new Vector2(0.0f, -0.5f), new Vector2(0.0f, 0.5f));
                dj7.Stiffness = stiffness;
                dj7.Damping = damping;
                _joints[7] = dj7;
                World.AddJoint(_joints[7]);
            }
        }

        private void BodyRemovedFired(Body body)
        {
            _removedBodies++;
        }

        private void JointRemovedFired(Joint joint)
        {
            if (joint is DistanceJoint)
                _removedJoints++;
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.B))
            {
                for (int i = 0; i < 4; ++i)
                {
                    if (_bodies[i] != null)
                    {
                        World.RemoveBody(_bodies[i]);
                        _bodies[i] = null;
                        break;
                    }
                }
            }

            if (keyboardManager.IsNewKeyPress(Keys.J))
            {
                for (int i = 0; i < 8; ++i)
                {
                    if (_joints[i] != null)
                    {
                        World.RemoveJoint(_joints[i]);
                        _joints[i] = null;
                        break;
                    }
                }
            }

            base.Keyboard(keyboardManager);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);
            DrawString("This demonstrates a soft distance joint.");

            DrawString("Press: (b) to delete a body, (j) to delete a joint");

            DrawString("Bodies removed: " + _removedBodies);

            DrawString("Joints removed: " + _removedJoints);
        }

        protected override void JointRemoved(Joint joint)
        {
            for (int i = 0; i < 8; ++i)
            {
                if (_joints[i] == joint)
                {
                    _joints[i] = null;
                    break;
                }
            }

            base.JointRemoved(joint);
        }

        internal static Test Create()
        {
            return new WebTest();
        }
    }
}