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
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class ApplyForceTest : Test
    {
        private readonly Body _body;

        private ApplyForceTest()
        {
            World.Gravity = Vector2.Zero;

            const float restitution = 0.4f;

            Body ground;
            {
                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(0.0f, 20.0f);
                ground = BodyFactory.CreateFromDef(World, bd);

                FixtureDef sd = new FixtureDef();
                sd.Restitution = restitution;

                // Left vertical
                sd.Shape = new EdgeShape(new Vector2(-20.0f, -20.0f), new Vector2(-20.0f, 20.0f));
                ground.AddFixture(sd);

                // Right vertical
                sd.Shape = new EdgeShape(new Vector2(20.0f, -20.0f), new Vector2(20.0f, 20.0f));
                ground.AddFixture(sd);

                // Top horizontal
                sd.Shape = new EdgeShape(new Vector2(-20.0f, 20.0f), new Vector2(20.0f, 20.0f));
                ground.AddFixture(sd);

                // Bottom horizontal
                sd.Shape = new EdgeShape(new Vector2(-20.0f, -20.0f), new Vector2(20.0f, -20.0f));
                ground.AddFixture(sd);
            }

            {
                Transform xf1 = new Transform();
                xf1.q.Set(0.3524f * MathConstants.Pi);
                xf1.p = xf1.q.GetXAxis();

                Vertices vertices = new Vertices(3);
                vertices.Add(MathUtils.Mul(ref xf1, new Vector2(-1.0f, 0.0f)));
                vertices.Add(MathUtils.Mul(ref xf1, new Vector2(1.0f, 0.0f)));
                vertices.Add(MathUtils.Mul(ref xf1, new Vector2(0.0f, 0.5f)));

                PolygonShape poly1 = new PolygonShape(vertices, 2.0f);

                FixtureDef sd1 = new FixtureDef();
                sd1.Shape = poly1;

                Transform xf2 = new Transform();
                xf2.q.Set(-0.3524f * MathConstants.Pi);
                xf2.p = -xf2.q.GetXAxis();

                vertices[0] = MathUtils.Mul(ref xf2, new Vector2(-1.0f, 0.0f));
                vertices[1] = MathUtils.Mul(ref xf2, new Vector2(1.0f, 0.0f));
                vertices[2] = MathUtils.Mul(ref xf2, new Vector2(0.0f, 0.5f));

                PolygonShape poly2 = new PolygonShape(vertices, 2.0f);

                FixtureDef sd2 = new FixtureDef();
                sd2.Shape = poly2;

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;

                bd.Position = new Vector2(0.0f, 3.0f);
                bd.Angle = MathConstants.Pi;
                bd.AllowSleep = false;

                _body = BodyFactory.CreateFromDef(World, bd);
                _body.AddFixture(sd1);
                _body.AddFixture(sd2);

                float gravity = 10.0f;
                float I = _body.Inertia;
                float mass = _body.Mass;

                // Compute an effective radius that can be used to
                // set the max torque for a friction joint
                // For a circle: I = 0.5 * m * r * r ==> r = sqrt(2 * I / m)
                float radius = MathUtils.Sqrt(2.0f * I / mass);

                FrictionJointDef jd = new FrictionJointDef();
                jd.BodyA = ground;
                jd.BodyB = _body;
                jd.LocalAnchorA = Vector2.Zero;
                jd.LocalAnchorB = _body.LocalCenter;
                jd.CollideConnected = true;
                jd.MaxForce = 0.5f * mass * gravity;
                jd.MaxTorque = 0.2f * mass * radius * gravity;

                JointFactory.CreateFromDef(World, jd);
            }

            {
                PolygonShape shape = new PolygonShape(1.0f);
                shape.SetAsBox(0.5f, 0.5f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Friction = 0.3f;

                for (int i = 0; i < 10; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;

                    bd.Position = new Vector2(0.0f, 7.0f + 1.54f * i);
                    Body body = BodyFactory.CreateFromDef(World, bd);

                    body.AddFixture(fd);

                    float gravity = 10.0f;
                    float I = body.Inertia;
                    float mass = body.Mass;

                    // For a circle: I = 0.5 * m * r * r ==> r = sqrt(2 * I / m)
                    float radius = MathUtils.Sqrt(2.0f * I / mass);

                    FrictionJointDef jd = new FrictionJointDef();
                    jd.LocalAnchorA = Vector2.Zero;
                    jd.LocalAnchorB = Vector2.Zero;
                    jd.BodyA = ground;
                    jd.BodyB = body;
                    jd.CollideConnected = true;
                    jd.MaxForce = mass * gravity;
                    jd.MaxTorque = 0.1f * mass * radius * gravity;

                    JointFactory.CreateFromDef(World, jd);
                }
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("Forward (W), Turn (A) and (D)");

            base.Update(settings, gameTime);
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsKeyDown(Keys.W))
            {
                Vector2 f = _body.GetWorldVector(new Vector2(0.0f, -50.0f));
                Vector2 p = _body.GetWorldPoint(new Vector2(0.0f, 3.0f));
                _body.ApplyForce(f, p);
            }

            if (keyboard.IsKeyDown(Keys.A))
                _body.ApplyTorque(10.0f);

            if (keyboard.IsKeyDown(Keys.D))
                _body.ApplyTorque(-10.0f);

            base.Keyboard(keyboard);
        }

        internal static Test Create()
        {
            return new ApplyForceTest();
        }
    }
}