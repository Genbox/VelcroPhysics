using System.Text;
using FarseerPhysics.DebugViews;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.SimpleSamplesXNA
{
    internal class Demo9Screen : PhysicsGameScreen, IDemoScreen
    {
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo9: Friction";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows several fixtures,");
            sb.AppendLine("with varying friction.");
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            World = new World(new Vector2(0, -20));
            base.LoadContent();

            DebugMaterial material = new DebugMaterial(MaterialType.Waves)
                                         {
                                             Color = Color.OliveDrab,
                                             Scale = 4f
                                         };

            Fixture _temp;
            _temp = FixtureFactory.CreateEdge(World, new Vector2(-20f, 17f), new Vector2(10f, 8f));
            _temp = FixtureFactory.CreateEdge(World, new Vector2(13.5f, 11f), new Vector2(13.5f, 7f));

            _temp = FixtureFactory.CreateEdge(World, new Vector2(-10f, -4f), new Vector2(20f, 4f));
            _temp = FixtureFactory.CreateEdge(World, new Vector2(-13.5f, -1f), new Vector2(-13.5f, -5f));

            _temp = FixtureFactory.CreateEdge(World, new Vector2(-20f, -8f), new Vector2(10f, -17f));

            float[] friction = new[] {0.75f, 0.5f, 0.35f, 0.1f, 0.0f};
            for (int i = 0; i < 5; ++i)
            {
                _temp = FixtureFactory.CreateRectangle(World, 2.5f, 2.5f, 1f, material);
                _temp.Body.BodyType = BodyType.Dynamic;
                _temp.Body.Position = new Vector2(-18f + 5.2f*i, 20.0f);
                _temp.Friction = friction[i];
            }
        }
    }
}