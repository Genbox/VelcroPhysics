using System.Text;
using FarseerPhysics.Common;
using FarseerPhysics.DemoShare;
using FarseerPhysics.Dynamics;
using FarseerPhysics.ScreenSystem;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Samples
{
    public class Demo6Screen : GameScreen, IDemoScreen
    {
        private Agent _agent;
        private Spider[] _spiders;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo6: Dynamic Angle Joints";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo demonstrates the use of revolute joints ");
            sb.AppendLine("combined with angle joints that have a dynamic ");
            sb.AppendLine("target angle");
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
            _agent = new Agent(World, new Vector2(0, -10));
            _spiders = new Spider[8];

            for (int i = 0; i < _spiders.Length; i++)
            {
                _spiders[i] = new Spider(World, new Vector2(0, ((i + 1) * 3) - 7));
            }

            base.LoadContent();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (IsActive)
            {
                for (int i = 0; i < _spiders.Length; i++)
                {
                    _spiders[i].Update(gameTime);
                }
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
    }
}