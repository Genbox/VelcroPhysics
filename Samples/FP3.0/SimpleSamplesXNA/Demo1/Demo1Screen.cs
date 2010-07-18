using System.Text;
using FarseerPhysics;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleSamplesXNA.Demo1
{
    internal class Demo1Screen : GameScreen, IDemoScreen
    {
        public override void Initialize()
        {
            World = new World(new Vector2(0, 0));

            base.Initialize();

            DebugViewEnabled = true;
            DebugView.AppendFlags(DebugViewFlags.Shape);
        }

        public override void LoadContent()
        {
            Fixture rectangle = FixtureFactory.CreateRectangle(World, 2, 2, 1);
            rectangle.GetBody().SetType(BodyType.Dynamic);

            base.LoadContent();
        }
        
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        public override void HandleInput(InputState input)
        {
            //if (input.CurrentGamePadState.IsConnected)
            //{
            //    HandleGamePadInput(input);
            //}
            //else
            //{
            //    HandleKeyboardInput(input);
            //}

            base.HandleInput(input);
        }

        public string GetTitle()
        {
            return "Demo1: A Single Body";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with no geometry");
            sb.AppendLine("attached. Note that it does not collide with the borders.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate: left and right triggers");
            sb.AppendLine("  -Move: left thumbstick");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate: left and right arrows");
            sb.AppendLine("  -Move: A,S,D,W");
            return sb.ToString();
        }
    }
}
