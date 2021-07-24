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
    internal class BodyTypesTest : Test
    {
        private readonly Body _attachment;
        private readonly Body _platform;
        private readonly float _speed;

        private BodyTypesTest()
        {
            Body ground;
            {
                BodyDef bd = new BodyDef();
                ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape(new Vector2(-20.0f, 0.0f), new Vector2(20.0f, 0.0f));

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;

                ground.AddFixture(fd);
            }

            // Define attachment
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(0.0f, 3.0f);
                _attachment = BodyFactory.CreateFromDef(World, bd);

                PolygonShape shape = new PolygonShape(2.0f);
                shape.SetAsBox(0.5f, 2.0f);
                _attachment.AddFixture(shape);
            }

            // Define platform
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-4.0f, 5.0f);
                _platform = BodyFactory.CreateFromDef(World, bd);

                PolygonShape shape = new PolygonShape(2.0f);
                shape.SetAsBox(0.5f, 4.0f, new Vector2(4.0f, 0.0f), 0.5f * MathConstants.Pi);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Friction = 0.6f;
                _platform.AddFixture(fd);

                RevoluteJointDef rjd = new RevoluteJointDef();
                rjd.Initialize(_attachment, _platform, new Vector2(0.0f, 5.0f));
                rjd.MaxMotorTorque = 50.0f;
                rjd.EnableMotor = true;
                JointFactory.CreateFromDef(World, rjd);

                PrismaticJointDef pjd = new PrismaticJointDef();
                pjd.Initialize(ground, _platform, new Vector2(0.0f, 5.0f), new Vector2(1.0f, 0.0f));

                pjd.MaxMotorForce = 1000.0f;
                pjd.EnableMotor = true;
                pjd.LowerTranslation = -10.0f;
                pjd.UpperTranslation = 10.0f;
                pjd.EnableLimit = true;

                JointFactory.CreateFromDef(World, pjd);

                _speed = 3.0f;
            }

            // Create a payload
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(0.0f, 8.0f);
                Body body = BodyFactory.CreateFromDef(World, bd);

                PolygonShape shape = new PolygonShape(2.0f);
                shape.SetAsBox(0.75f, 0.75f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Friction = 0.6f;

                body.AddFixture(fd);
            }
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsKeyDown(Keys.D))
                _platform.BodyType = BodyType.Dynamic;
            if (keyboard.IsKeyDown(Keys.S))
                _platform.BodyType = BodyType.Static;
            if (keyboard.IsKeyDown(Keys.K))
            {
                _platform.BodyType = BodyType.Kinematic;
                _platform.LinearVelocity = new Vector2(-_speed, 0.0f);
                _platform.AngularVelocity = 0.0f;
            }

            base.Keyboard(keyboard);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            // Drive the kinematic body.
            if (_platform.BodyType == BodyType.Kinematic)
            {
                _platform.GetTransform(out Transform tf);
                Vector2 p = tf.p;
                Vector2 v = _platform.LinearVelocity;

                if (p.X < -10.0f && v.X < 0.0f ||
                    p.X > 10.0f && v.X > 0.0f)
                {
                    v.X = -v.X;
                    _platform.LinearVelocity = v;
                }
            }

            base.Update(settings, gameTime);

            DrawString("Keys: (d) dynamic, (s) static, (k) kinematic");
        }

        internal static Test Create()
        {
            return new BodyTypesTest();
        }
    }
}