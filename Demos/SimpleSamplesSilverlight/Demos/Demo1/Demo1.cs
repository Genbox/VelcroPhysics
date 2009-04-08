using System.IO;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.SimpleSamplesSilverlight.Demos.Demo1
{
    public class Demo1 : SimulatorView
    {
        public Demo1()
        {
            Initialize();
        }

        public override string Title
        {
            get { return "A Single Body"; }
        }

        public override string Details
        {
            get
            {
                StringWriter sb = new StringWriter();
                sb.Write("This demo shows a single body with no geometry");
                sb.WriteLine(" attached.");
                sb.WriteLine(string.Empty);
                sb.WriteLine("Keyboard:");
                sb.WriteLine("  -Rotate : K and L");
                sb.WriteLine("  -Move : A,S,D,W");
                return sb.ToString();
            }
        }

        public override void Initialize()
        {
            ClearCanvas();
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 0));
            Body rectangleBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            rectangleBody.Position = new Vector2(512, 384);
            AddRectangleToCanvas(rectangleBody, new Vector2(128, 128));
            controlledBody = rectangleBody;
            base.Initialize();
        }
    }
}