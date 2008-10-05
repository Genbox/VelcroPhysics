using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerSilverlightManual.Screens;

namespace FarseerSilverlightManual.Demos.Demo2
{
    public class Demo2 : SimulatorView
    {
        public Demo2()
        {
            Initialize();
        }

        public override void Initialize()
        {
            ClearCanvas();

            Body rectangleBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            rectangleBody.Position = new Vector2(256, 384);

            Geom rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody, 128, 128);
            rectangleGeom.CollisionGroup = 15;

            Body circleBody = BodyFactory.Instance.CreateCircleBody(physicsSimulator, 64, 1);
            circleBody.Position = new Vector2(725, 384);

            Geom circleGeom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, circleBody, 64, 20);
            circleGeom.CollisionGroup = 14124;

            AddRectangleToCanvas(rectangleBody, new Vector2(128, 128));
            AddCircleToCanvas(circleBody, 64);

            base.Initialize();
        }
    }
}