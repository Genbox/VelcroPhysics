using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerSilverlightManual.Demos.DemoShare;
using FarseerSilverlightManual.Screens;

namespace FarseerSilverlightManual.Demos
{
    public class AngleSpringDemo : SimulatorView
    {
        public AngleSpringDemo()
        {
            Initialize();
        }

        public override void Initialize()
        {
            Body rectangleBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            rectangleBody.Position = new Vector2(ScreenManager.ScreenWidth/2f, 200);

            Geom rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody, 128, 128);
            rectangleGeom.CollisionGroup = 1;
            AddRectangleToCanvas(rectangleBody, new Vector2(128, 128));

            AngularSpringLever springLever = new AngularSpringLever();
            springLever.AttachPoint = 0;
            springLever.RectangleWidth = 200;
            springLever.RectangleHeight = 20;
            springLever.SpringConstant = 1000000;
            springLever.DampningConstant = 5000;
            springLever.CollisionGroup = 100;
            springLever.Position = ScreenManager.ScreenCenter;

            springLever.Load(this, physicsSimulator);

            base.Initialize();
        }
    }
}