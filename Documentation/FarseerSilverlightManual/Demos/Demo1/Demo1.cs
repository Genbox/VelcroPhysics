using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerSilverlightManual.Screens;

namespace FarseerSilverlightManual.Demos.Demo1
{
    public class Demo1 : SimulatorView
    {
        public Demo1()
        {
            Initialize();
        }

        public override void Initialize()
        {
            ClearCanvas();
            Body rectangleBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            rectangleBody.Position = new Vector2(512, 384);
            AddRectangleToCanvas(rectangleBody, new Vector2(128, 128));
            base.Initialize();
        }
    }
}