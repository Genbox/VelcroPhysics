using System.Text;
using FarseerPhysics.DemoBaseXNA.DemoShare;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace SimpleSamplesXNA.Demo9
{
    internal class Demo9Screen : GameScreen, IDemoScreen
    {
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo9: Ragdoll";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to combine physics objects");
            sb.AppendLine("to create a ragdoll");
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
            new Ragdoll(World, new Vector2(0, 0));
            CreateObstacles();
            base.LoadContent();
        }

        private void CreateObstacles()
        {
            Fixture[] rect = new Fixture[4];

            for (int i = 0; i < 4; i++)
            {
                rect[i] = FixtureFactory.CreateRectangle(World, 6, 1.5f, 1);
            }
            rect[0].Body.Position = new Vector2(-9, -5);
            rect[1].Body.Position = new Vector2(-8, 7);
            rect[2].Body.Position = new Vector2(9, -7);
            rect[3].Body.Position = new Vector2(7, 5);
        }
    }
}