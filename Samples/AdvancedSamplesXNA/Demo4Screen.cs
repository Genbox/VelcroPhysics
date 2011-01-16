using System.Text;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.DemoShare;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.AdvancedSamplesXNA
{
    internal class Demo4Screen : PhysicsGameScreen, IDemoScreen
    {
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo4: Advanced dynamics";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            World = new World(new Vector2(0, -9.82f));
            base.LoadContent();

            DebugView.AppendFlags(DebugViewFlags.Joint);

            new Spiderweb(World, Vector2.Zero, 0.2f, 5, 12);
        }
    }
}