using System.IO;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerSilverlightDemos.Demos.Demo2
{
    public class Demo2 : SimulatorView
    {
        public Demo2()
        {
            Initialize();
        }

        public override string Title
        {
            get { return "Two Bodies With Geom"; }
        }

        public override string Details
        {
            get
            {
                StringWriter sb = new StringWriter();
                sb.Write("This demo shows two bodies each with a single geometry");
                sb.WriteLine(" object attached.");
                sb.WriteLine("");
                sb.WriteLine("Keyboard:");
                sb.WriteLine("  -Rotate : K and L");
                sb.WriteLine("  -Move : A,S,D,W");
                sb.WriteLine("");
                sb.WriteLine("Mouse:");
                sb.WriteLine("  -Hold down left button and drag");
                return sb.ToString();
            }
        }

        public override void Initialize()
        {
            ClearCanvas();
            Body rectangleBody;
            Geom rectangleGeom;
            Body circleBody;
            Geom circleGeom;
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 0));
            rectangleBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            rectangleBody.Position = new Vector2(256, 384);
            rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, rectangleBody, 128, 128);
            circleBody = BodyFactory.Instance.CreateCircleBody(physicsSimulator, 64, 1); //fix 
            circleBody.Position = new Vector2(725, 384);
            circleGeom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, circleBody, 64, 20);
            AddRectangleToCanvas(rectangleBody, new Vector2(128, 128));
            AddCircleToCanvas(circleBody, 64);
            controlledBody = rectangleBody;
            base.Initialize();
        }
    }
}