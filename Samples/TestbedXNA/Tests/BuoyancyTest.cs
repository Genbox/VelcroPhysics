using FarseerPhysics.Collision;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class BuoyancyTest : Test
    {
        private AABB _container;

        private BuoyancyTest()
        {
            World.Gravity = new Vector2(0, -9.82f);

            BodyFactory.CreateEdge(World, new Vector2(-40, 0), new Vector2(40, 0));

            float offset = 5;
            for (int i = 0; i < 3; i++)
            {
                Body rectangle = BodyFactory.CreateRectangle(World, 2, 2, 1, new Vector2(-30 + offset, 20));
                rectangle.Rotation = Rand.RandomFloat(0, 3.14f);
                rectangle.BodyType = BodyType.Dynamic;
                offset += 7;
            }

            for (int i = 0; i < 3; i++)
            {
                Body rectangle = BodyFactory.CreateCircle(World, 1, 1, new Vector2(-30 + offset, 20));
                rectangle.Rotation = Rand.RandomFloat(0, 3.14f);
                rectangle.BodyType = BodyType.Dynamic;
                offset += 7;
            }

            _container = new AABB(new Vector2(0, 10), 60, 10);
            BuoyancyController buoyancy = new BuoyancyController(_container, 2, 2, 1, World.Gravity);
            World.AddController(buoyancy);
        }

        internal static Test Create()
        {
            return new BuoyancyTest();
        }
    }
}