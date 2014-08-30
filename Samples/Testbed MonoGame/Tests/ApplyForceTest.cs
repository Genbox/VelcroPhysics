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
    public class ApplyForceTest : Test
    {
        private Body _body;

        private ApplyForceTest()
        {
            World.Gravity = Vector2.Zero;

            const float restitution = 0.4f;

            Body ground;
            {
                ground = BodyFactory.CreateBody(World);
                ground.Position = new Vector2(0.0f, 20.0f);

                EdgeShape shape = new EdgeShape(new Vector2(-20.0f, -20.0f), new Vector2(-20.0f, 20.0f));

                // Left vertical
                Fixture fixture = ground.CreateFixture(shape);
                fixture.Restitution = restitution;

                // Right vertical
                shape = new EdgeShape(new Vector2(20.0f, -20.0f), new Vector2(20.0f, 20.0f));
                ground.CreateFixture(shape);

                // Top horizontal
                shape = new EdgeShape(new Vector2(-20.0f, 20.0f), new Vector2(20.0f, 20.0f));
                ground.CreateFixture(shape);

                // Bottom horizontal
                shape = new EdgeShape(new Vector2(-20.0f, -20.0f), new Vector2(20.0f, -20.0f));
                ground.CreateFixture(shape);
            }

            {
                Transform xf1 = new Transform();
                xf1.q.Set(0.3524f * Settings.Pi);
                xf1.p = MathUtils.Mul(ref xf1.q, new Vector2(1.0f, 0.0f));

                Vertices vertices = new Vertices(3);
                vertices.Add(MathUtils.Mul(ref xf1, new Vector2(-1.0f, 0.0f)));
                vertices.Add(MathUtils.Mul(ref xf1, new Vector2(1.0f, 0.0f)));
                vertices.Add(MathUtils.Mul(ref xf1, new Vector2(0.0f, 0.5f)));

                PolygonShape poly1 = new PolygonShape(vertices, 4);

                Transform xf2 = new Transform();
                xf2.q.Set(-0.3524f * Settings.Pi);
                xf2.p = MathUtils.Mul(ref xf2.q, new Vector2(-1.0f, 0.0f));

                vertices[0] = MathUtils.Mul(ref xf2, new Vector2(-1.0f, 0.0f));
                vertices[1] = MathUtils.Mul(ref xf2, new Vector2(1.0f, 0.0f));
                vertices[2] = MathUtils.Mul(ref xf2, new Vector2(0.0f, 0.5f));

                PolygonShape poly2 = new PolygonShape(vertices, 2);

                _body = BodyFactory.CreateBody(World);
                _body.BodyType = BodyType.Dynamic;
                _body.Position = new Vector2(0.0f, 2.0f);
                _body.Rotation = Settings.Pi;
                _body.AngularDamping = 5.0f;
                _body.LinearDamping = 0.8f;
                _body.SleepingAllowed = true;

                _body.CreateFixture(poly1);
                _body.CreateFixture(poly2);
            }

            {
                Vertices box = PolygonTools.CreateRectangle(0.5f, 0.5f);
                PolygonShape shape = new PolygonShape(box, 1);

                for (int i = 0; i < 10; ++i)
                {
                    Body body = BodyFactory.CreateBody(World);
                    body.Position = new Vector2(0.0f, 5.0f + 1.54f * i);
                    body.BodyType = BodyType.Dynamic;

                    Fixture fixture = body.CreateFixture(shape);
                    fixture.Friction = 0.3f;

                    const float gravity = 10.0f;
                    float I = body.Inertia;
                    float mass = body.Mass;

                    // For a circle: I = 0.5 * m * r * r ==> r = sqrt(2 * I / m)
                    float radius = (float)Math.Sqrt(2.0 * (I / mass));

                    FrictionJoint jd = new FrictionJoint(ground, body, Vector2.Zero);
                    jd.CollideConnected = true;
                    jd.MaxForce = mass * gravity;
                    jd.MaxTorque = mass * radius * gravity;

                    World.AddJoint(jd);
                }
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("Note: The left side of the ship has a different density than the right side of the ship");
            

            base.Update(settings, gameTime);
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsKeyDown(Keys.W))
                _body.ApplyForce(_body.GetWorldVector(new Vector2(0.0f, -200.0f)));

            if (keyboardManager.IsKeyDown(Keys.A))
                _body.ApplyTorque(50.0f);

            if (keyboardManager.IsKeyDown(Keys.D))
                _body.ApplyTorque(-50.0f);

            base.Keyboard(keyboardManager);
        }

        internal static Test Create()
        {
            return new ApplyForceTest();
        }
    }
}