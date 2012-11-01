using System.Text;
using FarseerPhysics.Common;
using FarseerPhysics.DemoShare;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.ScreenSystem;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Samples
{
    internal class Demo7Screen : GameScreen, IDemoScreen
    {
        private Ragdoll _ragdoll;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo7: Ragdoll";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to combine physics objects");
            sb.AppendLine("to create a ragdoll.");
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
            _ragdoll = new Ragdoll(World, Vector2.Zero);
            CreateObstacles();
            base.LoadContent();
        }

        private void CreateObstacles()
        {
            Body[] rect = new Body[4];

            for (int i = 0; i < 4; i++)
            {
                rect[i] = BodyFactory.CreateRectangle(World, 6, 1.5f, 1);
            }
            rect[0].Position = new Vector2(-9, -5);
            rect[1].Position = new Vector2(-8, 7);
            rect[2].Position = new Vector2(9, -7);
            rect[3].Position = new Vector2(7, 5);
        }
    }
}