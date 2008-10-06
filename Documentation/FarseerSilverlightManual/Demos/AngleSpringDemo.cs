using System;
using System.Windows.Media;
using FarseerGames.FarseerPhysics;
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
            rectangleBody.Position = new Vector2(ScreenManager.ScreenWidth / 2f, 200);

            Geom rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody, 128, 128);
            rectangleGeom.CollisionGroup = 1;
            AddRectangleToCanvas(rectangleBody, new Vector2(128, 128));

            Body rectangleBody1 = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            rectangleBody1.Position = new Vector2(ScreenManager.ScreenWidth / 2f, 300);

            Geom rectangleGeom1 = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody1, 128, 128);
            rectangleGeom1.CollisionGroup = 2;
            AddRectangleToCanvas(rectangleBody1, new Vector2(128, 128));

            base.Initialize();
        }
    }
}