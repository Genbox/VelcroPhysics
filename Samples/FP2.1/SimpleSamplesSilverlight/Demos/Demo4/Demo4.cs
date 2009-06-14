using System.IO;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.SimpleSamplesSilverlight.Demos.DemoShare;

namespace FarseerGames.SimpleSamplesSilverlight.Demos.Demo4
{
    public class Demo4 : SimulatorView
    {
        private const int _pyramidBaseBodyCount = 8;
        private Agent _agent;
        private Border _border;
        private Floor _floor;
        private Pyramid _pyramid;

        private Body _rectangleBody;
        private Geom _rectangleGeom;

        public Demo4()
        {
            Initialize();
            forceAmount = 1000;
            torqueAmount = 14000;
        }

        public override string Title
        {
            get { return "Stacked Objects"; }
        }

        public override string Details
        {
            get
            {
                StringWriter sb = new StringWriter();
                sb.Write("This demo shows the stacking stability of the engine.");
                sb.Write(" It shows a stack of rectangular bodies stacked in");
                sb.WriteLine(" the shape of a pyramid.");
                sb.WriteLine(string.Empty);
                sb.WriteLine("Keyboard:");
                sb.WriteLine("  -Rotate : K and L");
                sb.WriteLine("  -Move : A,S,D,W");
                sb.WriteLine(string.Empty);
                sb.WriteLine("Mouse:");
                sb.WriteLine("  -Hold down left button and drag");
                return sb.ToString();
            }
        }

        public override void Initialize()
        {
            ClearCanvas();
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 100));
            physicsSimulator.BiasFactor = .4f;
            int borderWidth = (int) (ScreenManager.ScreenHeight*.05f);
            _border = new Border(ScreenManager.ScreenWidth + borderWidth*2, ScreenManager.ScreenHeight + borderWidth*2,
                                 borderWidth, ScreenManager.ScreenCenter);
            _border.Load(this, physicsSimulator);
            _rectangleBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 32, 32, 1f);
            _rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _rectangleBody, 32, 32);
            _rectangleGeom.FrictionCoefficient = .4f;
            _rectangleGeom.RestitutionCoefficient = 0f;

            //create the pyramid near the bottom of the screen.
            _pyramid = new Pyramid(_rectangleBody, _rectangleGeom, 32f/3f, 32f/3f, 32, 32, _pyramidBaseBodyCount,
                                   new Vector2(ScreenManager.ScreenCenter.X - _pyramidBaseBodyCount*.5f*(32 + 32/3),
                                               ScreenManager.ScreenHeight - 125));
            _pyramid.Load(this, physicsSimulator);

            _floor = new Floor(ScreenManager.ScreenWidth, 100,
                               new Vector2(ScreenManager.ScreenCenter.X, ScreenManager.ScreenHeight - 50));
            _floor.Load(this, physicsSimulator);

            _agent = new Agent(ScreenManager.ScreenCenter - new Vector2(320, 300));
            _agent.Load(this, physicsSimulator);
            controlledBody = _agent.Body;
            base.Initialize();
        }
    }
}