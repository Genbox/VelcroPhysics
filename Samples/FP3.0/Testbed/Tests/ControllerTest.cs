using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class ControllerTest : Test
    {
        private ControllerTest()
        {
            Body ground = World.Add();

            Vertices edge = PolygonTools.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
            PolygonShape shape = new PolygonShape(edge, 0);
            ground.CreateFixture(shape);

            //Create the gravity controller
            GravityController gravity = new GravityController(20);
            World.Add(gravity);

            Vector2 startPosition = new Vector2(-10, 2);
            Vector2 offset = new Vector2(2);

            //Create the planet
            Body planet = World.Add();
            planet.Position = new Vector2(0, 20);

            CircleShape planetShape = new CircleShape(2, 1);
            planet.CreateFixture(planetShape);

            //Add the planet as the one that has gravity
            gravity.AddBody(planet);

            //Create 10 smaller circles
            for (int i = 0; i < 10; i++)
            {
                Body circle = World.Add();
                circle.BodyType = BodyType.Dynamic;
                circle.Position = startPosition + offset * i;

                CircleShape circleShape = new CircleShape(1, 0.1f);
                circle.CreateFixture(circleShape);
            }
        }

        public static Test Create()
        {
            return new ControllerTest();
        }
    }
}