using System.Text;
using FarseerPhysics.DebugViewXNA;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleSamplesXNA.Demo1
{
    internal class Demo1Screen : GameScreen
    {
        public override void Initialize()
        {
            PhysicsSimulator = new World(new Vector2(0, -50));
            PhysicsSimulatorView = new DebugViewXNA(PhysicsSimulator);
            DebugViewEnabled = true;

            base.Initialize();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        public override void HandleInput(InputState input)
        {
            if (firstRun)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
                firstRun = false;
            }

            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
            }

            if (input.CurrentGamePadState.IsConnected)
            {
                //HandleGamePadInput(input);
            }
            else
            {
                //HandleKeyboardInput(input);
            }

            base.HandleInput(input);
        }

        public static string GetTitle()
        {
            return "Demo1: A Single Body";
        }

        private static string GetDetails()
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
