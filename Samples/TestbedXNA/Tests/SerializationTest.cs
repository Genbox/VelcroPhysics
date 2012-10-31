#if XNA

using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class SerializationTest : Test
    {
        private bool save = true;
        private double time;

        private SerializationTest()
        {
            BodyFactory.CreateEdge(World, new Vector2(-20, 0), new Vector2(20, 0));

            Body circle = BodyFactory.CreateCircle(World, 1, 1);
            circle.BodyType = BodyType.Dynamic;
            circle.Position = new Vector2(-10, 5);

            Body rectangle = BodyFactory.CreateRectangle(World, 1, 1, 1);
            rectangle.BodyType = BodyType.Dynamic;
            rectangle.Position = new Vector2(-5, 10);

            FixtureFactory.AttachRectangle(2, 2, 2, new Vector2(1, 1), rectangle);

            JointFactory.CreateDistanceJoint(World, circle, rectangle, Vector2.Zero, Vector2.Zero);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            time += gameTime.ElapsedGameTime.Milliseconds;

            if (time > 100)
            {
                time = 0;
                if (save)
                {
                    WorldSerializer.Serialize(World, "out.xml");
                }
                else
                {
                    WorldSerializer.Deserialize(World, "out.xml");
                }

                save = !save;
            }
            base.Update(settings, gameTime);
        }


        internal static Test Create()
        {
            return new SerializationTest();
        }
    }
}
#endif