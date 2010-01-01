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
    public class SliderCrankTest : Test
    {
        private SliderCrankTest()
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
                Body prevBody = ground;

                // Define crank.
                {
                    PolygonShape shape = new PolygonShape(2);
                    shape.SetAsBox(0.5f, 2.0f);

                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(0.0f, 7.0f);
                    Body body = _world.CreateBody(bd);
                    body.CreateFixture(shape);

                    RevoluteJointDef rjd = new RevoluteJointDef();
                    rjd.Initialize(prevBody, body, new Vector2(0.0f, 5.0f));
                    rjd.MotorSpeed = 1.0f * Settings.Pi;
                    rjd.MaxMotorTorque = 10000.0f;
                    rjd.EnableMotor = true;
                    _joint1 = (RevoluteJoint)_world.CreateJoint(rjd);

                    prevBody = body;
                }

                // Define follower.
                {
                    PolygonShape shape = new PolygonShape(2);
                    shape.SetAsBox(0.5f, 4.0f);

                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(0.0f, 13.0f);
                    Body body = _world.CreateBody(bd);
                    body.CreateFixture(shape);

                    RevoluteJointDef rjd3 = new RevoluteJointDef();
                    rjd3.Initialize(prevBody, body, new Vector2(0.0f, 9.0f));
                    rjd3.EnableMotor = false;
                    _world.CreateJoint(rjd3);

                    prevBody = body;
                }

                // Define piston
                {
                    PolygonShape shape = new PolygonShape(2);
                    shape.SetAsBox(1.5f, 1.5f);

                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(0.0f, 17.0f);
                    Body body = _world.CreateBody(bd);
                    body.CreateFixture(shape);

                    RevoluteJointDef rjd2 = new RevoluteJointDef();
                    rjd2.Initialize(prevBody, body, new Vector2(0.0f, 17.0f));
                    _world.CreateJoint(rjd2);

                    PrismaticJointDef pjd = new PrismaticJointDef();
                    pjd.Initialize(ground, body, new Vector2(0.0f, 17.0f), new Vector2(0.0f, 1.0f));

                    pjd.MaxMotorForce = 1000.0f;
                    pjd.EnableMotor = true;

                    _joint2 = (PrismaticJoint)_world.CreateJoint(pjd);
                }

                // Create a payload
                {
                    PolygonShape shape = new PolygonShape(2);
                    shape.SetAsBox(1.5f, 1.5f);

                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(0.0f, 23.0f);
                    Body body = _world.CreateBody(bd);
                    body.CreateFixture(shape);
                }
            }
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.F) && oldState.IsKeyUp(Keys.F))
            {
                _joint2.EnableMotor(!_joint2.IsMotorEnabled());
                _joint2.GetBodyB().Awake = true;
            }
            if (state.IsKeyDown(Keys.M) && oldState.IsKeyUp(Keys.M))
            {
                _joint1.EnableMotor(!_joint1.IsMotorEnabled());
                _joint1.GetBodyB().Awake = true;
            }
        }

        public override void Step(Framework.Settings settings)
        {
            base.Step(settings);
            _debugView.DrawString(50, _textLine, "Keys: (f) toggle friction, (m) toggle motor");
            _textLine += 15;
            float torque = _joint1.GetMotorTorque();
            _debugView.DrawString(50, _textLine, "Motor Torque = {0:n}", torque);
            _textLine += 15;
        }

        internal static Test Create()
        {
            return new SliderCrankTest();
        }

        private RevoluteJoint _joint1;
        private PrismaticJoint _joint2;
    }
}