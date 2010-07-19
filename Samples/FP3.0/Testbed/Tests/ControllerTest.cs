using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class ControllerTest : Test
    {
        private ControllerTest()
        {
            //Ground
            FixtureFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f), 0);

            //Create the gravity controller
            GravityController gravity = new GravityController(20);
            World.AddController(gravity);

            Vector2 startPosition = new Vector2(-10, 2);
            Vector2 offset = new Vector2(2);

            //Create the planet
            Body planet = BodyFactory.CreateBody(World);
            planet.Position = new Vector2(0, 20);

            CircleShape planetShape = new CircleShape(2);
            planet.CreateFixture(planetShape, 1);

            //Add the planet as the one that has gravity
            gravity.AddBody(planet);

            //Create 10 smaller circles
            for (int i = 0; i < 10; i++)
            {
                Body circle = BodyFactory.CreateBody(World);
                circle.BodyType = BodyType.Dynamic;
                circle.Position = startPosition + offset * i;

                CircleShape circleShape = new CircleShape(1);
                circle.CreateFixture(circleShape, 0.1f);
            }
        }

        public static Test Create()
        {
            return new ControllerTest();
        }
    }
}