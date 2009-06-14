using System.IO;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using SimpleSamplesWPF.SharedDemoObjects;

namespace SimpleSamplesWPF.Demos.Demo4
{
    class Demo4 : Demo
    {
        private const int _pyramidBaseBodyCount = 12;
        private Agent _agent;
        private Pyramid _pyramid;

        private Body _rectangleBody;
        private Geom _rectangleGeom;

        public Demo4()
        {
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

        protected override void Initialize()
        {
            ClearCanvas();
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 100));
            physicsSimulator.BiasFactor = .4f;

            _rectangleBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 32, 32, 1f);
            _rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _rectangleBody, 32, 32);
            _rectangleGeom.FrictionCoefficient = .4f;
            _rectangleGeom.RestitutionCoefficient = 0f;

            //create the pyramid near the bottom of the screen.
            _pyramid = new Pyramid(_rectangleBody, _rectangleGeom, 32f / 3f, 32f / 3f, 32, 32, _pyramidBaseBodyCount,
                                   new Vector2(ScreenCenter.X - _pyramidBaseBodyCount * .5f * (32 + 32 / 3),
                                               ScreenHeight - 100));
            _pyramid.Load(this, physicsSimulator);

            _agent = new Agent(ScreenCenter - new Vector2(320, 300));
            _agent.Load(this, physicsSimulator);
            controlledBody = _agent.Body;
        }
    }
}