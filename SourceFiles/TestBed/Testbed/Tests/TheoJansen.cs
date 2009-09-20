/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2008 Erin Catto http://www.gphysics.com

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

// Inspired by a contribution by roman_m
// Dimensions scooped from APE (http://www.cove.org/ape/index.htm)

using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace TestBed
{
    public class TheoJansen : Test
    {
        Vec2 _offset;
        Body _chassis;
        Body _wheel;
        RevoluteJoint _motorJoint;
        bool _motorOn;
        float _motorSpeed;

        private void CreateLeg(float s, Vec2 wheelAnchor)
        {
            Vec2 p1 = new Vec2(5.4f * s, -6.1f);
            Vec2 p2 = new Vec2(7.2f * s, -1.2f);
            Vec2 p3 = new Vec2(4.3f * s, -1.9f);
            Vec2 p4 = new Vec2(3.1f * s, 0.8f);
            Vec2 p5 = new Vec2(6.0f * s, 1.5f);
            Vec2 p6 = new Vec2(2.5f * s, 3.7f);

            FixtureDef fd1 = new FixtureDef();
            FixtureDef fd2 = new FixtureDef();
            fd1.Filter.GroupIndex = -1;
            fd2.Filter.GroupIndex = -1;
            fd1.Density = 1.0f;
            fd2.Density = 1.0f;

            PolygonShape poly1 = new PolygonShape();
            PolygonShape poly2 = new PolygonShape();

            if (s > 0.0f)
            {
                Vec2[] vertices = new Vec2[3];

                vertices[0] = p1;
                vertices[1] = p2;
                vertices[2] = p3;
                poly1.Set(vertices, 3);

                vertices[0] = Vec2.Zero;
                vertices[1] = p5 - p4;
                vertices[2] = p6 - p4;
                poly2.Set(vertices, 3);
            }
            else
            {
                Vec2[] vertices = new Vec2[3];

                vertices[0] = p1;
                vertices[1] = p3;
                vertices[2] = p2;
                poly1.Set(vertices, 3);

                vertices[0] = Vec2.Zero;
                vertices[1] = p6 - p4;
                vertices[2] = p5 - p4;
                poly2.Set(vertices, 3);
            }

            fd1.Shape = poly1;
            fd2.Shape = poly2;

            BodyDef bd1 = new BodyDef();
            BodyDef bd2 = new BodyDef();
            bd1.Position = _offset;
            bd2.Position = p4 + _offset;

            bd1.AngularDamping = 10.0f;
            bd2.AngularDamping = 10.0f;

            Body body1 = _world.CreateBody(bd1);
            Body body2 = _world.CreateBody(bd2);

            body1.CreateFixture(fd1);
            body2.CreateFixture(fd2);

            DistanceJointDef djd = new DistanceJointDef();

            // Using a soft distance constraint can reduce some jitter.
            // It also makes the structure seem a bit more fluid by
            // acting like a suspension system.
            djd.DampingRatio = 0.5f;
            djd.FrequencyHz = 10.0f;

            djd.Initialize(body1, body2, p2 + _offset, p5 + _offset);
            _world.CreateJoint(djd);

            djd.Initialize(body1, body2, p3 + _offset, p4 + _offset);
            _world.CreateJoint(djd);

            djd.Initialize(body1, _wheel, p3 + _offset, wheelAnchor + _offset);
            _world.CreateJoint(djd);

            djd.Initialize(body2, _wheel, p6 + _offset, wheelAnchor + _offset);
            _world.CreateJoint(djd);

            RevoluteJointDef rjd = new RevoluteJointDef();

            rjd.Initialize(body2, _chassis, p4 + _offset);
            _world.CreateJoint(rjd);
        }

        public TheoJansen()
        {
            _offset.Set(0.0f, 8.0f);
            _motorSpeed = 2.0f;
            _motorOn = true;
            Vec2 pivot = new Vec2(0.0f, 0.8f);

            // Ground
            {
                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vec2(-50.0f, 0.0f), new Vec2(50.0f, 0.0f));
                ground.CreateFixture(shape, 0);

                shape.SetAsEdge(new Vec2(-50.0f, 0.0f), new Vec2(-50.0f, 10.0f));
                ground.CreateFixture(shape, 0);

                shape.SetAsEdge(new Vec2(50.0f, 0.0f), new Vec2(50.0f, 10.0f));
                ground.CreateFixture(shape, 0);
            }

            // Balls
            for (int i = 0; i < 40; ++i)
            {
                CircleShape shape = new CircleShape();
                shape._radius = 0.25f;

                BodyDef bd = new BodyDef();
                bd.Position.Set(-40.0f + 2.0f * i, 0.5f);

                Body body = _world.CreateBody(bd);
                body.CreateFixture(shape, 1.0f);
            }

            // Chassis
            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(2.5f, 1.0f);

                FixtureDef sd = new FixtureDef();
                sd.Density = 1.0f;
                sd.Shape = shape;
                sd.Filter.GroupIndex = -1;
                BodyDef bd = new BodyDef();
                bd.Position = pivot + _offset;
                _chassis = _world.CreateBody(bd);
                _chassis.CreateFixture(sd);
            }

            {
                CircleShape shape = new CircleShape();
                shape._radius = 1.6f;

                FixtureDef sd = new FixtureDef();
                sd.Density = 1.0f;
                sd.Shape = shape;
                sd.Filter.GroupIndex = -1;
                BodyDef bd = new BodyDef();
                bd.Position = pivot + _offset;
                _wheel = _world.CreateBody(bd);
                _wheel.CreateFixture(sd);
            }

            {
                RevoluteJointDef jd = new RevoluteJointDef();
                jd.Initialize(_wheel, _chassis, pivot + _offset);
                jd.CollideConnected = false;
                jd.MotorSpeed = _motorSpeed;
                jd.MaxMotorTorque = 400.0f;
                jd.EnableMotor = _motorOn;
                _motorJoint = (RevoluteJoint)_world.CreateJoint(jd);
            }

            Vec2 wheelAnchor;

            wheelAnchor = pivot + new Vec2(0.0f, -0.8f);

            CreateLeg(-1.0f, wheelAnchor);
            CreateLeg(1.0f, wheelAnchor);

            _wheel.SetTransform(_wheel.GetPosition(), 120.0f * Box2DX.Common.Settings.PI / 180.0f);
            CreateLeg(-1.0f, wheelAnchor);
            CreateLeg(1.0f, wheelAnchor);

            _wheel.SetTransform(_wheel.GetPosition(), -120.0f * Box2DX.Common.Settings.PI / 180.0f);
            CreateLeg(-1.0f, wheelAnchor);
            CreateLeg(1.0f, wheelAnchor);

        }

        public override void Step(Settings settings)
        {
            OpenGLDebugDraw.DrawString(5, _textLine, "Keys: left = a, brake = s, right = d, toggle motor = m");
            _textLine += 15;

            base.Step(settings);
        }

        public override void Keyboard(System.Windows.Forms.Keys key)
        {
            switch (key)
            {
                case System.Windows.Forms.Keys.A:
                    _chassis.WakeUp();
                    _motorJoint.MotorSpeed = -_motorSpeed;
                    break;

                case System.Windows.Forms.Keys.S:
                    _chassis.WakeUp();
                    _motorJoint.MotorSpeed = (0.0f);
                    break;

                case System.Windows.Forms.Keys.D:
                    _chassis.WakeUp();
                    _motorJoint.MotorSpeed = (_motorSpeed);
                    break;

                case System.Windows.Forms.Keys.M:
                    _chassis.WakeUp();
                    _motorJoint.EnableMotor(!_motorJoint.IsMotorEnabled);
                    break;
            }
        }

        public static Test Create()
        {
            return new TheoJansen();
        }
    }
}