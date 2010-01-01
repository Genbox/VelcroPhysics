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
    public class BodyTypesTest : Test
    {
        private BodyTypesTest()
        {
            Body ground;
            {
                BodyDef bd = new BodyDef();
                ground = _world.CreateBody(bd);

                Vertices edge = PolygonTools.CreateEdge(new Vector2(-20.0f, 0.0f), new Vector2(20.0f, 0.0f));
                PolygonShape shape = new PolygonShape(edge, 0);

                ground.CreateFixture(shape);
            }

            // Define attachment
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(0.0f, 3.0f);
                _attachment = _world.CreateBody(bd);

                Vertices box = PolygonTools.CreateBox(0.5f, 2.0f);
                PolygonShape shape = new PolygonShape(box, 2.0f);
                _attachment.CreateFixture(shape);
            }

            // Define platform
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(0.0f, 5.0f);
                _platform = _world.CreateBody(bd);

                Vertices box = PolygonTools.CreateBox(4.0f, 0.5f);
                PolygonShape shape = new PolygonShape(box, 2.0f);

                Fixture fixture = _platform.CreateFixture(shape);
                fixture.SetFriction(0.6f);

                RevoluteJointDef rjd = new RevoluteJointDef();
                rjd.Initialize(_attachment, _platform, new Vector2(0.0f, 5.0f));
                rjd.MaxMotorTorque = 50.0f;
                rjd.EnableMotor = true;
                _world.CreateJoint(rjd);

                PrismaticJointDef pjd = new PrismaticJointDef();
                pjd.Initialize(ground, _platform, new Vector2(0.0f, 5.0f), new Vector2(1.0f, 0.0f));

                pjd.MaxMotorForce = 1000.0f;
                pjd.EnableMotor = true;
                pjd.LowerTranslation = -10.0f;
                pjd.UpperTranslation = 10.0f;
                pjd.EnableLimit = true;

                _world.CreateJoint(pjd);

                _speed = 3.0f;
            }

            // Create a payload
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(0.0f, 8.0f);
                Body body = _world.CreateBody(bd);

                Vertices box = PolygonTools.CreateBox(0.75f, 0.75f);
                PolygonShape shape = new PolygonShape(box, 2.0f);

                Fixture fixture = body.CreateFixture(shape);
                fixture.SetFriction(0.6f);
            }
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.D))
            {
                _platform.BodyType = BodyType.Dynamic;
            }
            if (state.IsKeyDown(Keys.S))
            {
                _platform.BodyType = BodyType.Static;
            }
            if (state.IsKeyDown(Keys.K))
            {
                _platform.BodyType = BodyType.Kinematic;
                _platform.LinearVelocity = new Vector2(-_speed, 0.0f);
                _platform.AngularVelocity = 0.0f;
            }
        }

        public override void Step(Framework.Settings settings)
        {
            // Drive the kinematic body.
            if (_platform.BodyType == BodyType.Kinematic)
            {
                Transform tf;
                _platform.GetTransform(out tf);
                Vector2 p = tf.Position;
                Vector2 v = _platform.LinearVelocity;

                if ((p.X < -10.0f && v.X < 0.0f) ||
                    (p.X > 10.0f && v.X > 0.0f))
                {
                    v.X = -v.X;
                    _platform.LinearVelocity = v;
                }
            }

            base.Step(settings);
            _debugView.DrawString(5, _textLine, "Keys: (d) dynamic, (s) static, (k) kinematic");
            _textLine += 15;
        }


        internal static Test Create()
        {
            return new BodyTypesTest();
        }

        private Body _attachment;
        private Body _platform;
        private float _speed;
    }
}