/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
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
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Testbed.Tests
{
    public class CarTest : Test
    {
        private Body _car;
        private float _hz;
        private float _speed;
        private WheelJoint _spring1;
        private WheelJoint _spring2;
        private Body _wheel1;
        private Body _wheel2;
        private float _zeta;

        private CarTest()
        {
            _hz = 4.0f;
            _zeta = 0.7f;
            _speed = 50.0f;

            Body ground = BodyFactory.CreateEdge(World, new Vector2(-20.0f, 0.0f), new Vector2(20.0f, 0.0f));
            {
                float[] hs = new[] { 0.25f, 1.0f, 4.0f, 0.0f, 0.0f, -1.0f, -2.0f, -2.0f, -1.25f, 0.0f };

                float x = 20.0f, y1 = 0.0f;
                const float dx = 5.0f;

                for (int i = 0; i < 10; ++i)
                {
                    float y2 = hs[i];
                    FixtureFactory.AttachEdge(new Vector2(x, y1), new Vector2(x + dx, y2), ground);
                    y1 = y2;
                    x += dx;
                }

                for (int i = 0; i < 10; ++i)
                {
                    float y2 = hs[i];
                    FixtureFactory.AttachEdge(new Vector2(x, y1), new Vector2(x + dx, y2), ground);
                    y1 = y2;
                    x += dx;
                }

                FixtureFactory.AttachEdge(new Vector2(x, 0.0f), new Vector2(x + 40.0f, 0.0f), ground);
                x += 80.0f;
                FixtureFactory.AttachEdge(new Vector2(x, 0.0f), new Vector2(x + 40.0f, 0.0f), ground);
                x += 40.0f;
                FixtureFactory.AttachEdge(new Vector2(x, 0.0f), new Vector2(x + 10.0f, 5.0f), ground);
                x += 20.0f;
                FixtureFactory.AttachEdge(new Vector2(x, 0.0f), new Vector2(x + 40.0f, 0.0f), ground);
                x += 40.0f;
                FixtureFactory.AttachEdge(new Vector2(x, 0.0f), new Vector2(x, 20.0f), ground);

                ground.Friction = 0.6f;
            }

            // Teeter
            {
                Body body = new Body(World);
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(140.0f, 1.0f);

                PolygonShape box = new PolygonShape(1);
                box.Vertices = PolygonTools.CreateRectangle(10.0f, 0.25f);
                body.CreateFixture(box);

                RevoluteJoint jd = JointFactory.CreateRevoluteJoint(World, ground, body, Vector2.Zero);
                jd.LowerLimit = -8.0f * Settings.Pi / 180.0f;
                jd.UpperLimit = 8.0f * Settings.Pi / 180.0f;
                jd.LimitEnabled = true;

                body.ApplyAngularImpulse(100.0f);
            }

            //Bridge
            {
                const int N = 20;
                PolygonShape shape = new PolygonShape(1);
                shape.Vertices = PolygonTools.CreateRectangle(1.0f, 0.125f);

                Body prevBody = ground;
                for (int i = 0; i < N; ++i)
                {
                    Body body = new Body(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(161.0f + 2.0f * i, -0.125f);
                    Fixture fix = body.CreateFixture(shape);
                    fix.Friction = 0.6f;

                    Vector2 anchor = new Vector2(-1, 0);
                    JointFactory.CreateRevoluteJoint(World, prevBody, body, anchor);

                    prevBody = body;
                }

                Vector2 anchor2 = new Vector2(1.0f, 0);
                JointFactory.CreateRevoluteJoint(World, ground, prevBody, anchor2);
            }

            // Boxes
            {
                PolygonShape box = new PolygonShape(0.5f);
                box.Vertices = PolygonTools.CreateRectangle(0.5f, 0.5f);

                Body body = new Body(World);
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(230.0f, 0.5f);
                body.CreateFixture(box);

                body = new Body(World);
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(230.0f, 1.5f);
                body.CreateFixture(box);

                body = new Body(World);
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(230.0f, 2.5f);
                body.CreateFixture(box);

                body = new Body(World);
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(230.0f, 3.5f);
                body.CreateFixture(box);

                body = new Body(World);
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(230.0f, 4.5f);
                body.CreateFixture(box);
            }

            // Car
            {
                Vertices vertices = new Vertices(8);
                vertices.Add(new Vector2(-1.5f, -0.5f));
                vertices.Add(new Vector2(1.5f, -0.5f));
                vertices.Add(new Vector2(1.5f, 0.0f));
                vertices.Add(new Vector2(0.0f, 0.9f));
                vertices.Add(new Vector2(-1.15f, 0.9f));
                vertices.Add(new Vector2(-1.5f, 0.2f));

                PolygonShape chassis = new PolygonShape(vertices, 1);

                CircleShape circle = new CircleShape(0.4f, 1);

                _car = new Body(World);
                _car.BodyType = BodyType.Dynamic;
                _car.Position = new Vector2(0.0f, 1.0f);
                _car.CreateFixture(chassis);

                _wheel1 = new Body(World);
                _wheel1.BodyType = BodyType.Dynamic;
                _wheel1.Position = new Vector2(-1.0f, 0.35f);
                _wheel1.CreateFixture(circle);
                _wheel1.Friction = 0.9f;

                _wheel2 = new Body(World);
                _wheel2.BodyType = BodyType.Dynamic;
                _wheel2.Position = new Vector2(1.0f, 0.4f);
                _wheel2.CreateFixture(circle);
                _wheel2.Friction = 0.9f;

                Vector2 axis = new Vector2(0.0f, 1.0f);
                _spring1 = new WheelJoint(_car, _wheel1, _wheel1.Position, axis, true);
                _spring1.MotorSpeed = 0.0f;
                _spring1.MaxMotorTorque = 20.0f;
                _spring1.MotorEnabled = true;
                _spring1.Frequency = _hz;
                _spring1.DampingRatio = _zeta;
                World.AddJoint(_spring1);

                _spring2 = new WheelJoint(_car, _wheel2, _wheel2.Position, axis, true);
                _spring2.MotorSpeed = 0.0f;
                _spring2.MaxMotorTorque = 10.0f;
                _spring2.MotorEnabled = false;
                _spring2.Frequency = _hz;
                _spring2.DampingRatio = _zeta;
                World.AddJoint(_spring2);
            }
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.A))
            {
                _spring1.MotorSpeed = _speed;
            }
            else if (keyboardManager.IsNewKeyPress(Keys.S))
            {
                _spring1.MotorSpeed = 0.0f;
            }
            else if (keyboardManager.IsNewKeyPress(Keys.D))
            {
                _spring1.MotorSpeed = -_speed;
            }
            else if (keyboardManager.IsNewKeyPress(Keys.Q))
            {
                _hz = Math.Max(0.0f, _hz - 1.0f);
                _spring1.Frequency = _hz;
                _spring2.Frequency = _hz;
            }
            else if (keyboardManager.IsNewKeyPress(Keys.E))
            {
                _hz += 1.0f;
                _spring1.Frequency = _hz;
                _spring2.Frequency = _hz;
            }

            base.Keyboard(keyboardManager);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("Keys: left = a, brake = s, right = d, hz down = q, hz up = e");

            DrawString(string.Format("frequency = {0} hz, damping ratio = {1}", _hz, _zeta));

            DrawString(string.Format("actual speed = {0} rad/sec", _spring1.JointSpeed));


            GameInstance.ViewCenter = _car.Position;

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new CarTest();
        }
    }
}