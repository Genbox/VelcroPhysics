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
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Definitions.Joints;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class CarTest : Test
    {
        private readonly Body _car;
        private readonly float _speed;
        private readonly WheelJoint _spring1;
        private readonly WheelJoint _spring2;
        private readonly Body _wheel1;
        private readonly Body _wheel2;

        private CarTest()
        {
            _speed = 50.0f;

            Body ground;
            {
                BodyDef bd = new BodyDef();
                ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape(new Vector2(-20.0f, 0.0f), new Vector2(20.0f, 0.0f));

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Friction = 0.6f;

                ground.AddFixture(fd);

                float[] hs = { 0.25f, 1.0f, 4.0f, 0.0f, 0.0f, -1.0f, -2.0f, -2.0f, -1.25f, 0.0f };

                float x = 20.0f, y1 = 0.0f, dx = 5.0f;

                for (int i = 0; i < 10; ++i)
                {
                    float y2 = hs[i];
                    shape.SetTwoSided(new Vector2(x, y1), new Vector2(x + dx, y2));
                    ground.AddFixture(fd);
                    y1 = y2;
                    x += dx;
                }

                for (int i = 0; i < 10; ++i)
                {
                    float y2 = hs[i];
                    shape.SetTwoSided(new Vector2(x, y1), new Vector2(x + dx, y2));
                    ground.AddFixture(fd);
                    y1 = y2;
                    x += dx;
                }

                shape.SetTwoSided(new Vector2(x, 0.0f), new Vector2(x + 40.0f, 0.0f));
                ground.AddFixture(fd);

                x += 80.0f;
                shape.SetTwoSided(new Vector2(x, 0.0f), new Vector2(x + 40.0f, 0.0f));
                ground.AddFixture(fd);

                x += 40.0f;
                shape.SetTwoSided(new Vector2(x, 0.0f), new Vector2(x + 10.0f, 5.0f));
                ground.AddFixture(fd);

                x += 20.0f;
                shape.SetTwoSided(new Vector2(x, 0.0f), new Vector2(x + 40.0f, 0.0f));
                ground.AddFixture(fd);

                x += 40.0f;
                shape.SetTwoSided(new Vector2(x, 0.0f), new Vector2(x, 20.0f));
                ground.AddFixture(fd);
            }

            // Teeter
            {
                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(140.0f, 1.0f);
                bd.Type = BodyType.Dynamic;
                Body body = BodyFactory.CreateFromDef(World, bd);

                PolygonShape box = new PolygonShape(1.0f);
                box.SetAsBox(10.0f, 0.25f);
                body.AddFixture(box);

                RevoluteJointDef jd = new RevoluteJointDef();
                jd.Initialize(ground, body, body.Position);
                jd.LowerAngle = -8.0f * MathConstants.Pi / 180.0f;
                jd.UpperAngle = 8.0f * MathConstants.Pi / 180.0f;
                jd.EnableLimit = true;
                JointFactory.CreateFromDef(World, jd);

                body.ApplyAngularImpulse(100.0f);
            }

            //Bridge
            {
                int N = 20;
                PolygonShape shape = new PolygonShape(1.0f);
                shape.SetAsBox(1.0f, 0.125f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Friction = 0.6f;

                RevoluteJointDef jd = new RevoluteJointDef();

                Body prevBody = ground;
                for (int i = 0; i < N; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.Position = new Vector2(161.0f + 2.0f * i, -0.125f);
                    Body body = BodyFactory.CreateFromDef(World, bd);
                    body.AddFixture(fd);

                    Vector2 anchor = new Vector2(160.0f + 2.0f * i, -0.125f);
                    jd.Initialize(prevBody, body, anchor);
                    JointFactory.CreateFromDef(World, jd);

                    prevBody = body;
                }

                Vector2 anchor2 = new Vector2(160.0f + 2.0f * N, -0.125f);
                jd.Initialize(prevBody, ground, anchor2);
                JointFactory.CreateFromDef(World, jd);
            }

            // Boxes
            {
                PolygonShape box = new PolygonShape(0.5f);
                box.SetAsBox(0.5f, 0.5f);

                Body body;
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;

                bd.Position = new Vector2(230.0f, 0.5f);
                body = BodyFactory.CreateFromDef(World, bd);
                body.AddFixture(box);

                bd.Position = new Vector2(230.0f, 1.5f);
                body = BodyFactory.CreateFromDef(World, bd);
                body.AddFixture(box);

                bd.Position = new Vector2(230.0f, 2.5f);
                body = BodyFactory.CreateFromDef(World, bd);
                body.AddFixture(box);

                bd.Position = new Vector2(230.0f, 3.5f);
                body = BodyFactory.CreateFromDef(World, bd);
                body.AddFixture(box);

                bd.Position = new Vector2(230.0f, 4.5f);
                body = BodyFactory.CreateFromDef(World, bd);
                body.AddFixture(box);
            }

            // Car
            {
                PolygonShape chassis = new PolygonShape(1.0f);

                Vertices vertices = new Vertices(6);
                vertices.Add(new Vector2(-1.5f, -0.5f));
                vertices.Add(new Vector2(1.5f, -0.5f));
                vertices.Add(new Vector2(1.5f, 0.0f));
                vertices.Add(new Vector2(0.0f, 0.9f));
                vertices.Add(new Vector2(-1.15f, 0.9f));
                vertices.Add(new Vector2(-1.5f, 0.2f));
                chassis.Vertices = vertices;

                CircleShape circle = new CircleShape(0.4f, 1.0f);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(0.0f, 1.0f);
                _car = BodyFactory.CreateFromDef(World, bd);
                _car.AddFixture(chassis);

                FixtureDef fd = new FixtureDef();
                fd.Shape = circle;
                fd.Friction = 0.9f;

                bd.Position = new Vector2(-1.0f, 0.35f);
                _wheel1 = BodyFactory.CreateFromDef(World, bd);
                _wheel1.AddFixture(fd);

                bd.Position = new Vector2(1.0f, 0.4f);
                _wheel2 = BodyFactory.CreateFromDef(World, bd);
                _wheel2.AddFixture(fd);

                WheelJointDef jd = new WheelJointDef();
                Vector2 axis = new Vector2(0.0f, 1.0f);

                float mass1 = _wheel1.Mass;
                float mass2 = _wheel2.Mass;

                float hertz = 4.0f;
                float dampingRatio = 0.7f;
                float omega = 2.0f * MathConstants.Pi * hertz;

                jd.Initialize(_car, _wheel1, _wheel1.Position, axis);
                jd.MotorSpeed = 0.0f;
                jd.MaxMotorTorque = 20.0f;
                jd.EnableMotor = true;
                jd.Stiffness = mass1 * omega * omega;
                jd.Damping = 2.0f * mass1 * dampingRatio * omega;
                jd.LowerTranslation = -0.25f;
                jd.UpperTranslation = 0.25f;
                jd.EnableLimit = true;
                _spring1 = (WheelJoint)JointFactory.CreateFromDef(World, jd);

                jd.Initialize(_car, _wheel2, _wheel2.Position, axis);
                jd.MotorSpeed = 0.0f;
                jd.MaxMotorTorque = 10.0f;
                jd.EnableMotor = false;
                jd.Stiffness = mass2 * omega * omega;
                jd.Damping = 2.0f * mass2 * dampingRatio * omega;
                jd.LowerTranslation = -0.25f;
                jd.UpperTranslation = 0.25f;
                jd.EnableLimit = true;
                _spring2 = (WheelJoint)JointFactory.CreateFromDef(World, jd);
            }
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsNewKeyPress(Keys.A))
                _spring1.MotorSpeed = _speed;
            else if (keyboard.IsNewKeyPress(Keys.S))
                _spring1.MotorSpeed = 0.0f;
            else if (keyboard.IsNewKeyPress(Keys.D))
                _spring1.MotorSpeed = -_speed;

            base.Keyboard(keyboard);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("Keys: left = a, brake = s, right = d");
            DrawString($"Speed = {_spring1.JointAngularSpeed} rad/sec");

            GameInstance.ViewCenter = _car.Position;

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new CarTest();
        }
    }
}