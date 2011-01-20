using System.IO;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class SerializationTest : Test
    {
        private SerializationTest()
        {
            FixtureFactory.CreateEdge(World, new Vector2(-20, 0), new Vector2(20, 0));

            Fixture circle = FixtureFactory.CreateCircle(World, 1, 1);
            circle.Body.BodyType = BodyType.Dynamic;
            circle.Body.Position = new Vector2(-10, 5);

            Fixture rectangle = FixtureFactory.CreateRectangle(World, 1, 1, 1);
            rectangle.Body.BodyType = BodyType.Dynamic;
            rectangle.Body.Position = new Vector2(-5, 10);

            FixtureFactory.CreateRectangle(2, 2, 2, new Vector2(1, 1), rectangle.Body);

            JointFactory.CreateDistanceJoint(World, circle.Body, rectangle.Body, Vector2.Zero, Vector2.Zero);
        }

        private bool save = true;

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            if (StepCount > 100)
            {
                StepCount = 0;
                if (save)
                {
                    using (FileStream fs = new FileStream("out.xml", FileMode.Create))
                    {
                        WorldXmlSerializer serializer = new WorldXmlSerializer();
                        serializer.Serialize(World, fs);
                    }
                }
                else
                {
                    using (FileStream fs = new FileStream("out.xml", FileMode.Open))
                    {
                        WorldXmlDeserializer deserializer = new WorldXmlDeserializer();
                        deserializer.Deserialize(World, fs);
                    }
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