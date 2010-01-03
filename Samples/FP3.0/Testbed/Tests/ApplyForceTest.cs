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

using System;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class ApplyForceTest : Test
    {
        private ApplyForceTest()
        {
            _world.Gravity = Vector2.Zero;

            const float k_restitution = 0.4f;

            Body ground;
            {
                ground = _world.CreateBody();
                ground.Position = new Vector2(0.0f, 20.0f);

                Vertices edge = PolygonTools.CreateEdge(new Vector2(-20.0f, -20.0f), new Vector2(-20.0f, 20.0f));

                PolygonShape shape = new PolygonShape(edge, 0.0f);

                // Left vertical
                Fixture fixture = ground.CreateFixture(shape);
                fixture.Restitution = k_restitution;

                // Right vertical
                edge = PolygonTools.CreateEdge(new Vector2(20.0f, -20.0f), new Vector2(20.0f, 20.0f));
                shape.Set(edge);
                ground.CreateFixture(shape);

                // Top horizontal
                edge = PolygonTools.CreateEdge(new Vector2(-20.0f, 20.0f), new Vector2(20.0f, 20.0f));
                shape.Set(edge);
                ground.CreateFixture(shape);

                // Bottom horizontal
                edge = PolygonTools.CreateEdge(new Vector2(-20.0f, -20.0f), new Vector2(20.0f, -20.0f));
                shape.Set(edge);
                ground.CreateFixture(shape);
            }

            {
                Transform xf1 = new Transform();
                xf1.R.Set(0.3524f * Settings.Pi);
                xf1.Position = MathUtils.Multiply(ref xf1.R, new Vector2(1.0f, 0.0f));

                Vertices vertices = new Vertices(3);
                vertices[0] = MathUtils.Multiply(ref xf1, new Vector2(-1.0f, 0.0f));
                vertices[1] = MathUtils.Multiply(ref xf1, new Vector2(1.0f, 0.0f));
                vertices[2] = MathUtils.Multiply(ref xf1, new Vector2(0.0f, 0.5f));

                PolygonShape poly1 = new PolygonShape(vertices, 4.0f);

                Transform xf2 = new Transform();
                xf2.R.Set(-0.3524f * Settings.Pi);
                xf2.Position = MathUtils.Multiply(ref xf2.R, new Vector2(-1.0f, 0.0f));

                vertices[0] = MathUtils.Multiply(ref xf2, new Vector2(-1.0f, 0.0f));
                vertices[1] = MathUtils.Multiply(ref xf2, new Vector2(1.0f, 0.0f));
                vertices[2] = MathUtils.Multiply(ref xf2, new Vector2(0.0f, 0.5f));

                PolygonShape poly2 = new PolygonShape(vertices, 2.0f);

                _body = _world.CreateBody();
                _body.BodyType = BodyType.Dynamic;
                _body.Position = new Vector2(0.0f, 2.0f);
                _body.Rotation = Settings.Pi;
                _body.AngularDamping = 5.0f;
                _body.LinearDamping = 0.1f;

                _body.CreateFixture(poly1);
                _body.CreateFixture(poly2);
            }

            {
                Vertices box = PolygonTools.CreateBox(0.5f, 0.5f);
                PolygonShape shape = new PolygonShape(box, 1.0f);

                for (int i = 0; i < 10; ++i)
                {
                    Body body = _world.CreateBody();
                    body.Position = new Vector2(0.0f, 5.0f + 1.54f * i);
                    body.BodyType = BodyType.Dynamic;

                    Fixture fixture = body.CreateFixture(shape);
                    fixture.Friction = 0.3f;

                    const float gravity = 10.0f;
                    float I = body.Inertia;
                    float mass = body.Mass;

                    // For a circle: I = 0.5 * m * r * r ==> r = sqrt(2 * I / m)
                    float radius = (float)Math.Sqrt(2.0 * (I / mass));

                    FrictionJoint jd = new FrictionJoint(ground, body, Vector2.Zero, Vector2.Zero);
                    jd.CollideConnected = true;
                    jd.MaxForce = mass * gravity;
                    jd.MaxTorque = mass * radius * gravity;

                    _world.CreateJoint(jd);
                }
            }
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.W))
            {
                Vector2 f = _body.GetWorldVector(new Vector2(0.0f, -200.0f));
                Vector2 p = _body.GetWorldPoint(new Vector2(0.0f, 2.0f));
                _body.ApplyForce(f, p);
            }
            if (state.IsKeyDown(Keys.A))
            {
                _body.ApplyTorque(50.0f);
            }
            if (state.IsKeyDown(Keys.D))
            {
                _body.ApplyTorque(-50.0f);
            }
        }

        internal static Test Create()
        {
            return new ApplyForceTest();
        }

        private Body _body;
    }
}