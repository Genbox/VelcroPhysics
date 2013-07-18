using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
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
            BodyFactory.CreateEdge(World, new Vector2(-20, 0), new Vector2(20, 0));

            Body circle = BodyFactory.CreateCircle(World, 1, 1);
            circle.BodyType = BodyType.Dynamic;
            circle.Position = new Vector2(-10, 5);

            Body rectangle = BodyFactory.CreateRectangle(World, 1, 1, 1);
            rectangle.BodyType = BodyType.Dynamic;
            rectangle.Position = new Vector2(-5, 10);

            FixtureFactory.AttachRectangle(2, 2, 2, new Vector2(1, 1), rectangle);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            _time += gameTime.ElapsedGameTime.Milliseconds;

            if (_time >= 200)
            {
                _time = 0;
                if (_save)
                {
                    WorldSerializer.Serialize(World, "out.xml");
                }
                else
                {
                    World = WorldSerializer.Deserialize("out.xml");
                    base.Initialize(); //To initialize the debug view
                }

                _save = !_save;
            }
            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new SerializationTest();
        }
    }
}