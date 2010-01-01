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
                BodyDef bd = new BodyDef();
                ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape(0);
                shape.SetAsEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.CreateFixture(shape);
            }

            {
                PolygonShape shape = new PolygonShape(5);
                shape.SetAsBox(0.5f, 0.5f);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;

                bd.Position = new Vector2(-5.0f, 5.0f);
                _bodies[0] = _world.CreateBody(bd);
                _bodies[0].CreateFixture(shape);

                bd.Position = new Vector2(5.0f, 5.0f);
                _bodies[1] = _world.CreateBody(bd);
                _bodies[1].CreateFixture(shape);

                bd.Position = new Vector2(5.0f, 15.0f);
                _bodies[2] = _world.CreateBody(bd);
                _bodies[2].CreateFixture(shape);

                bd.Position = new Vector2(-5.0f, 15.0f);
                _bodies[3] = _world.CreateBody(bd);
                _bodies[3].CreateFixture(shape);

                DistanceJointDef jd = new DistanceJointDef();

                jd.FrequencyHz = 4.0f;
                jd.DampingRatio = 0.5f;

                jd.BodyA = ground;
                jd.BodyB = _bodies[0];
                jd.LocalAnchorA = new Vector2(-10.0f, 0.0f);
                jd.LocalAnchorB = new Vector2(-0.5f, -0.5f);
                Vector2 p1 = jd.BodyA.GetWorldPoint(jd.LocalAnchorA);
                Vector2 p2 = jd.BodyB.GetWorldPoint(jd.LocalAnchorB);
                Vector2 d = p2 - p1;
                jd.Length = d.Length();
                _joints[0] = _world.CreateJoint(jd);

                jd.BodyA = ground;
                jd.BodyB = _bodies[1];
                jd.LocalAnchorA = new Vector2(10.0f, 0.0f);
                jd.LocalAnchorB = new Vector2(0.5f, -0.5f);
                p1 = jd.BodyA.GetWorldPoint(jd.LocalAnchorA);
                p2 = jd.BodyB.GetWorldPoint(jd.LocalAnchorB);
                d = p2 - p1;
                jd.Length = d.Length();
                _joints[1] = _world.CreateJoint(jd);

                jd.BodyA = ground;
                jd.BodyB = _bodies[2];
                jd.LocalAnchorA = new Vector2(10.0f, 20.0f);
                jd.LocalAnchorB = new Vector2(0.5f, 0.5f);
                p1 = jd.BodyA.GetWorldPoint(jd.LocalAnchorA);
                p2 = jd.BodyB.GetWorldPoint(jd.LocalAnchorB);
                d = p2 - p1;
                jd.Length = d.Length();
                _joints[2] = _world.CreateJoint(jd);

                jd.BodyA = ground;
                jd.BodyB = _bodies[3];
                jd.LocalAnchorA = new Vector2(-10.0f, 20.0f);
                jd.LocalAnchorB = new Vector2(-0.5f, 0.5f);
                p1 = jd.BodyA.GetWorldPoint(jd.LocalAnchorA);
                p2 = jd.BodyB.GetWorldPoint(jd.LocalAnchorB);
                d = p2 - p1;
                jd.Length = d.Length();
                _joints[3] = _world.CreateJoint(jd);

                jd.BodyA = _bodies[0];
                jd.BodyB = _bodies[1];
                jd.LocalAnchorA = new Vector2(0.5f, 0.0f);
                jd.LocalAnchorB = new Vector2(-0.5f, 0.0f);

                p1 = jd.BodyA.GetWorldPoint(jd.LocalAnchorA);
                p2 = jd.BodyB.GetWorldPoint(jd.LocalAnchorB);
                d = p2 - p1;
                jd.Length = d.Length();
                _joints[4] = _world.CreateJoint(jd);

                jd.BodyA = _bodies[1];
                jd.BodyB = _bodies[2];
                jd.LocalAnchorA = new Vector2(0.0f, 0.5f);
                jd.LocalAnchorB = new Vector2(0.0f, -0.5f);
                p1 = jd.BodyA.GetWorldPoint(jd.LocalAnchorA);
                p2 = jd.BodyB.GetWorldPoint(jd.LocalAnchorB);
                d = p2 - p1;
                jd.Length = d.Length();
                _joints[5] = _world.CreateJoint(jd);

                jd.BodyA = _bodies[2];
                jd.BodyB = _bodies[3];
                jd.LocalAnchorA = new Vector2(-0.5f, 0.0f);
                jd.LocalAnchorB = new Vector2(0.5f, 0.0f);
                p1 = jd.BodyA.GetWorldPoint(jd.LocalAnchorA);
                p2 = jd.BodyB.GetWorldPoint(jd.LocalAnchorB);
                d = p2 - p1;
                jd.Length = d.Length();
                _joints[6] = _world.CreateJoint(jd);

                jd.BodyA = _bodies[3];
                jd.BodyB = _bodies[0];
                jd.LocalAnchorA = new Vector2(0.0f, -0.5f);
                jd.LocalAnchorB = new Vector2(0.0f, 0.5f);
                p1 = jd.BodyA.GetWorldPoint(jd.LocalAnchorA);
                p2 = jd.BodyB.GetWorldPoint(jd.LocalAnchorB);
                d = p2 - p1;
                jd.Length = d.Length();
                _joints[7] = _world.CreateJoint(jd);
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