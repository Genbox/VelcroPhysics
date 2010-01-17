using FarseerGames.FarseerPhysics.Controllers;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class BuoyancyTest : Test
    {
        private BuoyancyTest()
        {
            //Make a box
            //Bottom
            Body ground = World.CreateBody();
            Vertices edge = PolygonTools.CreateEdge(new Vector2(-20.0f, 0.0f), new Vector2(20.0f, 0.0f));
            PolygonShape shape = new PolygonShape(edge);
            ground.CreateFixture(shape);

            //Left side
            shape.Set(PolygonTools.CreateEdge(new Vector2(-20.0f, 0.0f), new Vector2(-20.0f, 15.0f)));
            ground.CreateFixture(shape);

            //Right side
            shape.Set(PolygonTools.CreateEdge(new Vector2(20.0f, 0.0f), new Vector2(20.0f, 15.0f)));
            ground.CreateFixture(shape);


            //Buoyancy controller
            AABBFluidContainer fluidContainer = new AABBFluidContainer(new Vector2(-20.0f, 15.0f), new Vector2(20.0f, 0.0f));

            FluidDragController buoyancyController = new FluidDragController();
            buoyancyController.Initialize(fluidContainer, 2f, 5f, 2f, World.Gravity);

            Vector2 offset = new Vector2(5, 3);

            //Bunch of balls
            for (int i = 0; i < 4; i++)
            {
                Body circleBody = World.CreateBody();
                circleBody.BodyType = BodyType.Dynamic;
                circleBody.Position = new Vector2(-7, 1) + offset * i;

                //PolygonShape circleShape = new PolygonShape(1);
                //circleShape.SetAsBox(1,1);
                CircleShape circleShape = new CircleShape(1, 1);


                buoyancyController.AddGeom(circleBody.CreateFixture(circleShape));
            }

            World.AddController(buoyancyController);
        }

        public static Test Create()
        {
            return new BuoyancyTest();
        }
    }
}
