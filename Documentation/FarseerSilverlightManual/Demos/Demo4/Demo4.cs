using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerSilverlightManual.Demos.DemoShare;
using FarseerSilverlightManual.Screens;

namespace FarseerSilverlightManual.Demos.Demo4
{
    public class Demo4 : SimulatorView
    {
        private const int _pyramidBaseBodyCount = 8;
        private Agent _agent;
        private Pyramid _pyramid;

        private Body _rectangleBody;
        private Geom _rectangleGeom;

        public Demo4()
        {
            Initialize();
        }

        public override void Initialize()
        {
            ClearCanvas();
            //TODO: Reset this when disposed
            physicsSimulator.BiasFactor = .4f;
            _rectangleBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 32, 32, 1f);
            _rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _rectangleBody, 32, 32);
            _rectangleGeom.FrictionCoefficient = .4f;
            _rectangleGeom.RestitutionCoefficient = 0f;

            //create the pyramid near the bottom of the screen.
            _pyramid = new Pyramid(_rectangleBody, _rectangleGeom, 32f/3f, 32f/3f, 32, 32, _pyramidBaseBodyCount,
                                   new Vector2(ScreenManager.ScreenCenter.X - _pyramidBaseBodyCount*.5f*(32 + 32/3),
                                               ScreenManager.ScreenHeight - 125));
            _pyramid.Load(this, physicsSimulator);

            _agent = new Agent(ScreenManager.ScreenCenter - new Vector2(320, 300));
            _agent.Load(this, physicsSimulator);
            base.Initialize();
        }
    }
}