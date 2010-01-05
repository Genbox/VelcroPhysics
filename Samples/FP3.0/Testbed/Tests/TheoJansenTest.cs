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
    public class TheoJansenTest : Test
    {
        private void CreateLeg(float s, Vector2 wheelAnchor)
        {
            Vector2 p1 = new Vector2(5.4f * s, -6.1f);
            Vector2 p2 = new Vector2(7.2f * s, -1.2f);
            Vector2 p3 = new Vector2(4.3f * s, -1.9f);
            Vector2 p4 = new Vector2(3.1f * s, 0.8f);
            Vector2 p5 = new Vector2(6.0f * s, 1.5f);
            Vector2 p6 = new Vector2(2.5f * s, 3.7f);

            PolygonShape poly1 = new PolygonShape(1);
            PolygonShape poly2 = new PolygonShape(2);

            Vertices vertices = new Vertices(3);

            if (s > 0.0f)
            {
                vertices.Add(p1);
                vertices.Add(p2);
                vertices.Add(p3);
                poly1.Set(vertices);

                vertices[0] = Vector2.Zero;
                vertices[1] = p5 - p4;
                vertices[2] = p6 - p4;
                poly2.Set(vertices);
            }
            else
            {
                vertices.Add(p1);
                vertices.Add(p3);
                vertices.Add(p2);
                poly1.Set(vertices);

                vertices[0] = Vector2.Zero;
                vertices[1] = p6 - p4;
                vertices[2] = p5 - p4;
                poly2.Set(vertices);
            }


            Body body1 = World.CreateBody();
            body1.BodyType = BodyType.Dynamic;
            body1.Position = _offset;
            body1.AngularDamping = 10.0f;

            Body body2 = World.CreateBody();
            body2.BodyType = BodyType.Dynamic;
            body2.Position = p4 + _offset;
            body2.AngularDamping = 10.0f;

            Fixture f1 = body1.CreateFixture(poly1);
            f1.GroupIndex = -1;

            Fixture f2 = body2.CreateFixture(poly2);
            f2.GroupIndex = -1;

            // Using a soft distanceraint can reduce some jitter.
            // It also makes the structure seem a bit more fluid by
            // acting like a suspension system.
            DistanceJoint djd = new DistanceJoint(body1, body2, p2 + _offset, p5 + _offset);
            djd.DampingRatio = 0.5f;
            djd.Frequency = 10.0f;

            World.CreateJoint(djd);

            DistanceJoint djd2 = new DistanceJoint(body1, body2, p3 + _offset, p4 + _offset);
            djd2.DampingRatio = 0.5f;
            djd2.Frequency = 10.0f;

            World.CreateJoint(djd2);

            DistanceJoint djd3 = new DistanceJoint(body1, _wheel, p3 + _offset, wheelAnchor + _offset);
            djd3.DampingRatio = 0.5f;
            djd3.Frequency = 10.0f;

            World.CreateJoint(djd3);

            DistanceJoint djd4 = new DistanceJoint(body2, _wheel, p6 + _offset, wheelAnchor + _offset);
            djd4.DampingRatio = 0.5f;
            djd4.Frequency = 10.0f;

            World.CreateJoint(djd4);

            RevoluteJoint rjd = new RevoluteJoint(body2, _chassis, p4 + _offset);
            World.CreateJoint(rjd);
        }

        private TheoJansenTest()
        {
            _offset = new Vector2(0.0f, 8.0f);
            _motorSpeed = 2.0f;
            _motorOn = true;
            Vector2 pivot = new Vector2(0.0f, 0.8f);

            // Ground
            {
                Body ground = World.CreateBody();

                PolygonShape shape = new PolygonShape(0);
                shape.SetAsEdge(new Vector2(-50.0f, 0.0f), new Vector2(50.0f, 0.0f));
                ground.CreateFixture(shape);

                shape.SetAsEdge(new Vector2(-50.0f, 0.0f), new Vector2(-50.0f, 10.0f));
                ground.CreateFixture(shape);

                shape.SetAsEdge(new Vector2(50.0f, 0.0f), new Vector2(50.0f, 10.0f));
                ground.CreateFixture(shape);
            }

            // Balls
            for (int i = 0; i < 40; ++i)
            {
                CircleShape shape = new CircleShape(0.25f, 1);

                Body body = World.CreateBody();
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(-40.0f + 2.0f * i, 0.5f);

                body.CreateFixture(shape);
            }

            // Chassis
            {
                PolygonShape shape = new PolygonShape(1);
                shape.SetAsBox(2.5f, 1.0f);

                _chassis = World.CreateBody();
                _chassis.BodyType = BodyType.Dynamic;
                _chassis.Position = pivot + _offset;

                Fixture fixture = _chassis.CreateFixture(shape);
                fixture.GroupIndex = -1;
            }

            {
                CircleShape shape = new CircleShape(1.6f, 1);

                _wheel = World.CreateBody();
                _wheel.BodyType = BodyType.Dynamic;
                _wheel.Position = pivot + _offset;

                Fixture fixture = _wheel.CreateFixture(shape);
                fixture.GroupIndex = -1;
            }

            {
                _motorJoint = new RevoluteJoint(_wheel, _chassis, pivot + _offset);
                _motorJoint.CollideConnected = false;
                _motorJoint.MotorSpeed = _motorSpeed;
                _motorJoint.MaxMotorTorque = 400.0f;
                _motorJoint.MotorEnabled = _motorOn;
                World.CreateJoint(_motorJoint);
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

        public override void Update(Framework.Settings settings)
        {
            DebugView.DrawString(50, TextLine, "Keys: left = a, brake = s, right = d, toggle motor = m");
            TextLine += 15;

            base.Update(settings);
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.A) && oldState.IsKeyUp(Keys.A))
            {
                _motorJoint.MotorSpeed = -_motorSpeed;
            }
            if (state.IsKeyDown(Keys.S) && oldState.IsKeyUp(Keys.S))
            {
                _motorJoint.MotorSpeed = 0.0f;
            }
            if (state.IsKeyDown(Keys.D) && oldState.IsKeyUp(Keys.D))
            {
                _motorJoint.MotorSpeed = _motorSpeed;
            }
            if (state.IsKeyDown(Keys.M) && oldState.IsKeyUp(Keys.M))
            {
                _motorJoint.MotorEnabled = !_motorJoint.MotorEnabled;
            }
        }

        internal static Test Create()
        {
            return new TheoJansenTest();
        }

        private Vector2 _offset;
        private Body _chassis;
        private Body _wheel;
        private RevoluteJoint _motorJoint;
        private bool _motorOn;
        private float _motorSpeed;
    }
}