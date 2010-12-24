using System.Text;
using FarseerPhysics.DebugViews;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.SimpleSamplesXNA
{
    internal class Demo8Screen : PhysicsGameScreen, IDemoScreen
    {
        private Fixture[] _circle;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo8: Restitution";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows several fixtures,");
            sb.AppendLine("with varying restitution.");
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            World = new World(new Vector2(0, -20));
            base.LoadContent();

            DebugMaterial material = new DebugMaterial(MaterialType.Waves)
                                         {
                                             Color = Color.Chocolate,
                                             Scale = 4f
                                         };

            _circle = new Fixture[6];
            Vector2 _position = new Vector2(-14f, 0f);
            float _restitution = 0f;

            for (int i = 0; i < 6; ++i)
            {
                _circle[i] = FixtureFactory.CreateCircle(World, 2f, 1f, _position, material);
                _circle[i].Body.BodyType = BodyType.Dynamic;
                _circle[i].Restitution = _restitution;
                _position.X += 6f;
                _restitution += 0.2f;
            }
        }
    }
}