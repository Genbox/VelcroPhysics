using System.Text;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.DemoShare;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.AdvancedSamplesXNA
{
    internal class Demo3Screen : PhysicsGameScreen, IDemoScreen
    {
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo3: Theo Jansen's walker";
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

            new TheoJansenWalker(World, new Vector2(0,0));
        }
    }
}