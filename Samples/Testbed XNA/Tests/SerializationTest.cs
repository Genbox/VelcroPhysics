using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class SerializationTest : Test
    {
        private bool _save = true;
        private double _time;

        private SerializationTest()
        {
            Body ground = BodyFactory.CreateEdge(World, new Vector2(-20, 0), new Vector2(20, 0));

            //Friction and distance joint
            {
                Body bodyA = BodyFactory.CreateCircle(World, 1, 1.5f, new Vector2(10, 25));
                bodyA.BodyType = BodyType.Dynamic;

                Body bodyB = BodyFactory.CreateRectangle(World, 1, 1, 1, new Vector2(-1, 25));
                bodyB.BodyType = BodyType.Dynamic;

                FrictionJoint frictionJoint = JointFactory.CreateFrictionJoint(World, bodyB, ground, Vector2.Zero);
                frictionJoint.CollideConnected = true;
                frictionJoint.MaxForce = 100;

                JointFactory.CreateDistanceJoint(World, bodyA, bodyB);
            }

            //Wheel joint
            {
                Vertices vertices = new Vertices(6);
                vertices.Add(new Vector2(-1.5f, -0.5f));
                vertices.Add(new Vector2(1.5f, -0.5f));
                vertices.Add(new Vector2(1.5f, 0.0f));
                vertices.Add(new Vector2(0.0f, 0.9f));
                vertices.Add(new Vector2(-1.15f, 0.9f));
                vertices.Add(new Vector2(-1.5f, 0.2f));

                Body carBody = BodyFactory.CreatePolygon(World, vertices, 1, new Vector2(0, 1));
                carBody.BodyType = BodyType.Dynamic;

                Body wheel1 = BodyFactory.CreateCircle(World, 0.4f, 1, new Vector2(-1.0f, 0.35f));
                wheel1.BodyType = BodyType.Dynamic;
                wheel1.Friction = 0.9f;

                Body wheel2 = BodyFactory.CreateCircle(World, 0.4f, 1, new Vector2(1.0f, 0.4f));
                wheel2.BodyType = BodyType.Dynamic;
                wheel2.Friction = 0.9f;

                Vector2 axis = new Vector2(0.0f, 1.0f);

                WheelJoint spring1 = JointFactory.CreateWheelJoint(World, carBody, wheel1, axis);
                spring1.MotorSpeed = 0.0f;
                spring1.MaxMotorTorque = 20.0f;
                spring1.MotorEnabled = true;
                spring1.Frequency = 4;
                spring1.DampingRatio = 0.7f;

                WheelJoint spring2 = JointFactory.CreateWheelJoint(World, carBody, wheel2, axis);
                spring2.MotorSpeed = 0.0f;
                spring2.MaxMotorTorque = 10.0f;
                spring2.MotorEnabled = false;
                spring2.Frequency = 4;
                spring2.DampingRatio = 0.7f;
            }

            //Prismatic joint
            {
                Body body = BodyFactory.CreateRectangle(World, 2, 2, 5, new Vector2(-10.0f, 10.0f));
                body.BodyType = BodyType.Dynamic;
                body.Rotation = 0.5f * Settings.Pi;

                Vector2 axis = new Vector2(2.0f, 1.0f);
                axis.Normalize();

                PrismaticJoint joint = JointFactory.CreatePrismaticJoint(World, ground, body, Vector2.Zero, axis);
                joint.MotorSpeed = 5.0f;
                joint.MaxMotorForce = 1000.0f;
                joint.MotorEnabled = true;
                joint.LowerLimit = -10.0f;
                joint.UpperLimit = 20.0f;
                joint.LimitEnabled = true;
            }

            // Pulley joint
            {
                Body body1 = BodyFactory.CreateRectangle(World, 2, 4, 5, new Vector2(-10.0f, 16.0f));
                body1.BodyType = BodyType.Dynamic;

                Body body2 = BodyFactory.CreateRectangle(World, 2, 4, 5, new Vector2(10.0f, 16.0f));
                body2.BodyType = BodyType.Dynamic;

                Vector2 anchor1 = new Vector2(0, 2.0f);
                Vector2 anchor2 = new Vector2(0, 2.0f);
                Vector2 worldAnchor1 = new Vector2(-10.0f, 16.0f + 2.0f + 4.0f);
                Vector2 worldAnchor2 = new Vector2(10.0f, 16.0f + 2.0f + 12.0f);
                JointFactory.CreatePulleyJoint(World, body1, body2, worldAnchor1, worldAnchor2, anchor1, anchor2, 1.5f);
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            _time += gameTime.ElapsedGameTime.Milliseconds;

            //if (_time >= 200)
            //{
            //    _time = 0;
            //    if (_save)
            //    {
            //        WorldSerializer.Serialize(World, "out.xml");
            //    }
            //    else
            //    {
            //        World = WorldSerializer.Deserialize("out.xml");
            //        base.Initialize(); //To initialize the debug view
            //    }

            //    _save = !_save;
            //}
            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new SerializationTest();
        }
    }
}