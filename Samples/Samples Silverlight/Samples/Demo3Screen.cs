using System.Text;
using FarseerPhysics.Common;
using FarseerPhysics.DemoShare;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.ScreenSystem;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Samples
{
    internal class Demo3Screen : GameScreen, IDemoScreen
    {
        private Agent _agent;
        private Body[] _obstacles = new Body[5];

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo3: Multiple fixtures and static bodies";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with multiple shapes");
            sb.AppendLine("attached.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("This demo also shows the use of static bodies.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate: left and right arrows");
            sb.AppendLine("  -Move: A,S,D,W");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }

        #endregion

        public override void Initialize()
        {
            World = new World(new Vector2(0, -20));

            base.Initialize();
        }

        public override void LoadContent()
        {
            _agent = new Agent(World, new Vector2(5, 10));

            LoadObstacles();

            base.LoadContent();
        }

        private void LoadObstacles()
        {
            for (int i = 0; i < 5; i++)
            {
                _obstacles[i] = BodyFactory.CreateRectangle(World, 8, 1.5f, 1);
                _obstacles[i].IsStatic = true;

                if (i == 0)
                {
                    _obstacles[i].Restitution = .2f;
                    _obstacles[i].Friction = .2f;
                }
            }

            _obstacles[0].Position = new Vector2(-5, -15);
            _obstacles[1].Position = new Vector2(15, -10);
            _obstacles[2].Position = new Vector2(10, 5);
            _obstacles[3].Position = new Vector2(-10, 15);
            _obstacles[4].Position = new Vector2(-17, 0);
        }
    }
}