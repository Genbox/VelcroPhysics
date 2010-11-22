using System.Text;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.AdvancedSamplesXNA
{
    internal class Demo2Screen : PhysicsGameScreen, IDemoScreen
    {
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo2: Chains factory";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to use the chain");
            sb.AppendLine("factory with the path generator.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate : left and right arrows");
            sb.AppendLine("  -Move : A,S,D,W");
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            World = new World(new Vector2(0, -50));

            LinkFactory.CreateChain(World, new Vector2(-5, 0), new Vector2(15, 0), 0.125f, 0.6f, true, true, 15, 1);
            LinkFactory.CreateChain(World, new Vector2(-10, 10), new Vector2(-7, -10), 0.125f, 0.6f, true, false, 15, 1);

            base.LoadContent();
        }
    }
}