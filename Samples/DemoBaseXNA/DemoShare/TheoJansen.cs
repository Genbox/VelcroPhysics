using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoBaseXNA.DemoShare
{
    public class TheoJansenWalker
    {
        private Body _chassis;
        private RevoluteJoint _motorJoint;
        private bool _motorOn;
        private float _motorSpeed;
        private Vector2 _position;
        private Body _wheel;

        DebugMaterial _defaultMaterial = new DebugMaterial(MaterialType.Blank)
        {
            Color = Color.WhiteSmoke
        };

        public TheoJansenWalker(World world, Vector2 position)
        {
            _position = position;
            _motorSpeed = 2.0f;
            _motorOn = true;
            Vector2 pivot = new Vector2(0.0f, 0.8f);

            // Chassis
            {
                PolygonShape shape = new PolygonShape(1);
                shape.SetAsBox(2.5f, 1.0f);

                _chassis = BodyFactory.CreateBody(world);
                _chassis.BodyType = BodyType.Dynamic;
                _chassis.Position = pivot + _position;

                Fixture fixture = _chassis.CreateFixture(shape, _defaultMaterial);
                fixture.CollisionFilter.CollisionGroup = -1;
            }

            {
                CircleShape shape = new CircleShape(1.6f, 1);

                _wheel = BodyFactory.CreateBody(world);
                _wheel.BodyType = BodyType.Dynamic;
                _wheel.Position = pivot + _position;

                Fixture fixture = _wheel.CreateFixture(shape, _defaultMaterial);
                fixture.CollisionFilter.CollisionGroup = -1;
            }

            {
                _motorJoint = new RevoluteJoint(_wheel, _chassis, _wheel.GetLocalPoint(_chassis.Position), Vector2.Zero);
                _motorJoint.CollideConnected = false;
                _motorJoint.MotorSpeed = _motorSpeed;
                _motorJoint.MaxMotorTorque = 400.0f;
                _motorJoint.MotorEnabled = _motorOn;
                world.AddJoint(_motorJoint);
            }

            Vector2 wheelAnchor = pivot + new Vector2(0.0f, -0.8f);

            CreateLeg(world, -1.0f, wheelAnchor);
            CreateLeg(world, 1.0f, wheelAnchor);

            _wheel.SetTransform(_wheel.Position, 120.0f * Settings.Pi / 180.0f);
            CreateLeg(world, -1.0f, wheelAnchor);
            CreateLeg(world, 1.0f, wheelAnchor);

            _wheel.SetTransform(_wheel.Position, -120.0f * Settings.Pi / 180.0f);
            CreateLeg(world, -1.0f, wheelAnchor);
            CreateLeg(world, 1.0f, wheelAnchor);
        }

        private void CreateLeg(World world, float s, Vector2 wheelAnchor)
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

            Body body1 = BodyFactory.CreateBody(world);
            body1.BodyType = BodyType.Dynamic;
            body1.Position = _position;
            body1.AngularDamping = 10.0f;

            Body body2 = BodyFactory.CreateBody(world);
            body2.BodyType = BodyType.Dynamic;
            body2.Position = p4 + _position;
            body2.AngularDamping = 10.0f;

            Fixture f1 = body1.CreateFixture(poly1, _defaultMaterial);
            f1.CollisionFilter.CollisionGroup = -1;

            Fixture f2 = body2.CreateFixture(poly2, _defaultMaterial);
            f2.CollisionFilter.CollisionGroup = -1;

            // Using a soft distanceraint can reduce some jitter.
            // It also makes the structure seem a bit more fluid by
            // acting like a suspension system.
            DistanceJoint djd = new DistanceJoint(body1, body2, body1.GetLocalPoint(p2 + _position),
                                                  body2.GetLocalPoint(p5 + _position));
            djd.DampingRatio = 0.5f;
            djd.Frequency = 10.0f;

            world.AddJoint(djd);

            DistanceJoint djd2 = new DistanceJoint(body1, body2, body1.GetLocalPoint(p3 + _position),
                                                   body2.GetLocalPoint(p4 + _position));
            djd2.DampingRatio = 0.5f;
            djd2.Frequency = 10.0f;

            world.AddJoint(djd2);

            DistanceJoint djd3 = new DistanceJoint(body1, _wheel, body1.GetLocalPoint(p3 + _position),
                                                   _wheel.GetLocalPoint(wheelAnchor + _position));
            djd3.DampingRatio = 0.5f;
            djd3.Frequency = 10.0f;

            world.AddJoint(djd3);

            DistanceJoint djd4 = new DistanceJoint(body2, _wheel, body2.GetLocalPoint(p6 + _position),
                                                   _wheel.GetLocalPoint(wheelAnchor + _position));
            djd4.DampingRatio = 0.5f;
            djd4.Frequency = 10.0f;

            world.AddJoint(djd4);

            Vector2 anchor = p4 - new Vector2(0.0f, 0.8f);
            RevoluteJoint rjd = new RevoluteJoint(body2, _chassis, body2.GetLocalPoint(_chassis.GetWorldPoint(anchor)),
                                                  anchor);
            world.AddJoint(rjd);
        }
    }
}