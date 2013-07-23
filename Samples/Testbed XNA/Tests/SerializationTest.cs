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

            Body bodyA = BodyFactory.CreateCircle(World, 1, 1.5f, new Vector2(10, 25));
            bodyA.BodyType = BodyType.Dynamic;

            Body bodyB = BodyFactory.CreateRectangle(World, 1, 1, 1, new Vector2(-1, 25));
            bodyB.BodyType = BodyType.Dynamic;

            FrictionJoint frictionJoint = JointFactory.CreateFrictionJoint(World, bodyB, ground, Vector2.Zero);
            frictionJoint.CollideConnected = true;
            frictionJoint.MaxForce = 100;

            JointFactory.CreateDistanceJoint(World, bodyA, bodyB);
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