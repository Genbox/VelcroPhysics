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
    public class TheoJansenTest : Test
    {
        private Body _chassis;
        private RevoluteJoint _motorJoint;
        private bool _motorOn;
        private float _motorSpeed;
        private Vector2 _offset;
        private Body _wheel;

        private TheoJansenTest()
        {
            _offset = new Vector2(0.0f, 8.0f);
            _motorSpeed = 2.0f;
            _motorOn = true;
            Vector2 pivot = new Vector2(0.0f, 0.8f);

            // Ground
            {
                Body ground = BodyFactory.CreateEdge(World, new Vector2(-50.0f, 0.0f), new Vector2(50.0f, 0.0f));
                FixtureFactory.AttachEdge(new Vector2(-50.0f, 0.0f), new Vector2(-50.0f, 10.0f), ground);
                FixtureFactory.AttachEdge(new Vector2(50.0f, 0.0f), new Vector2(50.0f, 10.0f), ground);
            }

            // Balls
            for (int i = 0; i < 40; ++i)
            {
                CircleShape shape = new CircleShape(0.25f, 1);

                Body body = BodyFactory.CreateBody(World);
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(-40.0f + 2.0f * i, 0.5f);

                body.CreateFixture(shape);
            }

            // Chassis
            {
                PolygonShape shape = new PolygonShape(1);
                shape.Vertices = PolygonTools.CreateRectangle(2.5f, 1.0f);

                _chassis = BodyFactory.CreateBody(World);
                _chassis.BodyType = BodyType.Dynamic;
                _chassis.Position = pivot + _offset;

                Fixture fixture = _chassis.CreateFixture(shape);
                fixture.CollisionGroup = -1;
            }

            {
                CircleShape shape = new CircleShape(1.6f, 1);

                _wheel = BodyFactory.CreateBody(World);
                _wheel.BodyType = BodyType.Dynamic;
                _wheel.Position = pivot + _offset;

                Fixture fixture = _wheel.CreateFixture(shape);
                fixture.CollisionGroup = -1;
            }

            {
                _motorJoint = new RevoluteJoint(_wheel, _chassis, _chassis.Position, true);
                _motorJoint.CollideConnected = false;
                _motorJoint.MotorSpeed = _motorSpeed;
                _motorJoint.MaxMotorTorque = 400.0f;
                _motorJoint.MotorEnabled = _motorOn;
                World.AddJoint(_motorJoint);
            }

            Vector2 wheelAnchor = pivot + new Vector2(0.0f, -0.8f);

            CreateLeg(-1.0f, wheelAnchor);
            CreateLeg(1.0f, wheelAnchor);

            _wheel.SetTransform(_wheel.Position, 120.0f * Settings.Pi / 180.0f);
            CreateLeg(-1.0f, wheelAnchor);
            CreateLeg(1.0f, wheelAnchor);

            _wheel.SetTransform(_wheel.Position, -120.0f * Settings.Pi / 180.0f);
            CreateLeg(-1.0f, wheelAnchor);
            CreateLeg(1.0f, wheelAnchor);
        }

        private void CreateLeg(float s, Vector2 wheelAnchor)
        {
            Vector2 p1 = new Vector2(5.4f * s, -6.1f);
            Vector2 p2 = new Vector2(7.2f * s, -1.2f);
            Vector2 p3 = new Vector2(4.3f * s, -1.9f);
            Vector2 p4 = new Vector2(3.1f * s, 0.8f);
            Vector2 p5 = new Vector2(6.0f * s, 1.5f);
            Vector2 p6 = new Vector2(2.5f * s, 3.7f);

            PolygonShape poly1 = new PolygonShape(1);
            PolygonShape poly2 = new PolygonShape(1);

            Vertices vertices = new Vertices(3);

            if (s > 0.0f)
            {
                vertices.Add(p1);
                vertices.Add(p2);
                vertices.Add(p3);
                poly1.Vertices = vertices;

                vertices[0] = Vector2.Zero;
                vertices[1] = p5 - p4;
                vertices[2] = p6 - p4;
                poly2.Vertices = vertices;
            }
            else
            {
                vertices.Add(p1);
                vertices.Add(p3);
                vertices.Add(p2);
                poly1.Vertices = vertices;

                vertices[0] = Vector2.Zero;
                vertices[1] = p6 - p4;
                vertices[2] = p5 - p4;
                poly2.Vertices = vertices;
            }

            Body body1 = BodyFactory.CreateBody(World);
            body1.BodyType = BodyType.Dynamic;
            body1.Position = _offset;
            body1.AngularDamping = 10.0f;

            Body body2 = BodyFactory.CreateBody(World);
            body2.BodyType = BodyType.Dynamic;
            body2.Position = p4 + _offset;
            body2.AngularDamping = 10.0f;

            Fixture f1 = body1.CreateFixture(poly1);
            f1.CollisionGroup = -1;

            Fixture f2 = body2.CreateFixture(poly2);
            f2.CollisionGroup = -1;

            // Using a soft distanceraint can reduce some jitter.
            // It also makes the structure seem a bit more fluid by
            // acting like a suspension system.
            DistanceJoint djd = new DistanceJoint(body1, body2, p2 + _offset, p5 + _offset, true);
            djd.DampingRatio = 0.5f;
            djd.Frequency = 10.0f;

            World.AddJoint(djd);

            DistanceJoint djd2 = new DistanceJoint(body1, body2, p3 + _offset, p4 + _offset, true);
            djd2.DampingRatio = 0.5f;
            djd2.Frequency = 10.0f;

            World.AddJoint(djd2);

            DistanceJoint djd3 = new DistanceJoint(body1, _wheel, p3 + _offset, wheelAnchor + _offset, true);
            djd3.DampingRatio = 0.5f;
            djd3.Frequency = 10.0f;

            World.AddJoint(djd3);

            DistanceJoint djd4 = new DistanceJoint(body2, _wheel, p6 + _offset, wheelAnchor + _offset, true);
            djd4.DampingRatio = 0.5f;
            djd4.Frequency = 10.0f;

            World.AddJoint(djd4);

            Vector2 anchor = p4 - new Vector2(0.0f, 0.8f) /*+ _offset*/;
            RevoluteJoint rjd = new RevoluteJoint(body2, _chassis, p4 + _offset, true);
            World.AddJoint(rjd);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("Keys: left = a, brake = s, right = d, toggle motor = m");


            base.Update(settings, gameTime);
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.A))
                _motorJoint.MotorSpeed = -_motorSpeed;
            if (keyboardManager.IsNewKeyPress(Keys.S))
                _motorJoint.MotorSpeed = 0.0f;
            if (keyboardManager.IsNewKeyPress(Keys.D))
                _motorJoint.MotorSpeed = _motorSpeed;
            if (keyboardManager.IsNewKeyPress(Keys.M))
                _motorJoint.MotorEnabled = !_motorJoint.MotorEnabled;

            base.Keyboard(keyboardManager);
        }

        internal static Test Create()
        {
            return new TheoJansenTest();
        }
    }
}