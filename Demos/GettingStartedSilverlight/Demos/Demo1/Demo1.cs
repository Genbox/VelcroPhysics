using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Drawing;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using System.Collections.Generic;
using FarseerSilverlightDemos.Drawing;
using System.IO;

namespace FarseerSilverlightDemos.Demos.Demo1
{
    public class Demo1 : SimulatorView
    {
        public Demo1()
            : base()
        {
            Initialize();
        }

        public override void Initialize()
        {
            ClearCanvas();
            Body rectangleBody;
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 0));
            rectangleBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 128, 128, 1);
            rectangleBody.Position = new Vector2(512, 384);
            AddRectangleToCanvas(rectangleBody, new Vector2(128,128));
            controlledBody = rectangleBody;
            base.Initialize();
        }

        public override string Title
        {
            get
            {
                return "A Single Body";
            }
        }

        public override string Details
        {
            get
            {
                StringWriter sb = new StringWriter();
                sb.Write("This demo shows a single body with no geometry");
                sb.WriteLine(" attached.");
                sb.WriteLine("");
                sb.WriteLine("Keyboard:");
                sb.WriteLine("  -Rotate : K and L");
                sb.WriteLine("  -Move : A,S,D,W");
                return sb.ToString();
            }
        }
    }
}
