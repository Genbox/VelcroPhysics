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
    // This tests distance joints, body destruction, and joint destruction.
    public class WebTest : Test
    {
        private WebTest()
        {
            Body ground;
            {

                ground = _world.CreateBody();

                PolygonShape shape = new PolygonShape(0);
                shape.SetAsEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.CreateFixture(shape);
            }

            {
                PolygonShape shape = new PolygonShape(5);
                shape.SetAsBox(0.5f, 0.5f);


                _bodies[0] = _world.CreateBody();
                _bodies[0].BodyType = BodyType.Dynamic;
                _bodies[0].Position = new Vector2(-5.0f, 5.0f);

                _bodies[0].CreateFixture(shape);

                _bodies[1] = _world.CreateBody();
                _bodies[1].BodyType = BodyType.Dynamic;
                _bodies[1].Position = new Vector2(5.0f, 5.0f);

                _bodies[1].CreateFixture(shape);

                _bodies[2] = _world.CreateBody();
                _bodies[2].BodyType = BodyType.Dynamic;
                _bodies[2].Position = new Vector2(5.0f, 15.0f);

                _bodies[2].CreateFixture(shape);

                _bodies[3] = _world.CreateBody();
                _bodies[3].BodyType = BodyType.Dynamic;
                _bodies[3].Position = new Vector2(-5.0f, 15.0f);

                _bodies[3].CreateFixture(shape);

                DistanceJoint dj = new DistanceJoint(ground, _bodies[0], new Vector2(-10.0f, 0.0f), new Vector2(-0.5f, -0.5f));
                _joints[0] = dj;
                _world.CreateJoint(_joints[0]);

                DistanceJoint dj1 = new DistanceJoint(ground, _bodies[1], new Vector2(10.0f, 0.0f), new Vector2(0.5f, -0.5f));
                _joints[1] = dj1;
                _world.CreateJoint(_joints[1]);

                DistanceJoint dj2 = new DistanceJoint(ground, _bodies[2], new Vector2(10.0f, 20.0f), new Vector2(0.5f, 0.5f));
                _joints[2] = dj2;
                _world.CreateJoint(_joints[2]);

                DistanceJoint dj3 = new DistanceJoint(ground, _bodies[3], new Vector2(-10.0f, 20.0f), new Vector2(-0.5f, 0.5f));
                _joints[3] = dj3;
                _world.CreateJoint(_joints[3]);

                DistanceJoint dj4 = new DistanceJoint(_bodies[0], _bodies[1], new Vector2(0.5f, 0.0f), new Vector2(-0.5f, 0.0f));
                _joints[4] = dj4;
                _world.CreateJoint(_joints[4]);

                DistanceJoint dj5 = new DistanceJoint(_bodies[1], _bodies[2], new Vector2(0.0f, 0.5f), new Vector2(0.0f, -0.5f));
                _joints[5] = dj5;
                _world.CreateJoint(_joints[5]);

                DistanceJoint dj6 = new DistanceJoint(_bodies[2], _bodies[3], new Vector2(-0.5f, 0.0f), new Vector2(0.5f, 0.0f));
                _joints[6] = dj6;
                _world.CreateJoint(_joints[6]);

                DistanceJoint dj7 = new DistanceJoint(_bodies[3], _bodies[0], new Vector2(0.0f, -0.5f), new Vector2(0.0f, 0.5f));
                _joints[7] = dj7;
                _world.CreateJoint(_joints[7]);

                for (int i = 0; i < 8; i++)
                {
                    ((DistanceJoint)_joints[i]).Frequency = 4.0f;
                    ((DistanceJoint)_joints[i]).DampingRatio = 0.5f;
                }
            }
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.B) && oldState.IsKeyUp(Keys.B))
            {
                for (int i = 0; i < 4; ++i)
                {
                    if (_bodies[i] != null)
                    {
                        _world.DestroyBody(_bodies[i]);
                        _bodies[i] = null;
                        break;
                    }
                }
            }

            if (state.IsKeyDown(Keys.J) && oldState.IsKeyUp(Keys.J))
            {
                for (int i = 0; i < 8; ++i)
                {
                    if (_joints[i] != null)
                    {
                        _world.DestroyJoint(_joints[i]);
                        _joints[i] = null;
                        break;
                    }
                }
            }
        }

        public override void Step(Framework.Settings settings)
        {
            base.Step(settings);
            _debugView.DrawString(50, _textLine, "This demonstrates a soft distance joint.");
            _textLine += 15;
            _debugView.DrawString(50, _textLine, "Press: (b) to delete a body, (j) to delete a joint");
            _textLine += 15;
        }

        public override void JointDestroyed(Joint joint)
        {
            for (int i = 0; i < 8; ++i)
            {
                if (_joints[i] == joint)
                {
                    _joints[i] = null;
                    break;
                }
            }
        }

        internal static Test Create()
        {
            return new WebTest();
        }

        private Body[] _bodies = new Body[4];
        private Joint[] _joints = new Joint[8];
    }
}