using System.Text;
using FarseerGames.AdvancedSamples.Demos.DemoShare;
using FarseerGames.AdvancedSamples.ScreenSystem;
using FarseerGames.FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.AdvancedSamples.Demos.Demo3
{
    public class Demo3Screen : GameScreen
    {
        private Border _border;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 150));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            _border = new Border(ScreenManager.ScreenWidth, ScreenManager.ScreenHeight, 25, ScreenManager.ScreenCenter);


            base.Initialize();
        }

        public override void LoadContent()
        {
            _border.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            base.LoadContent();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
            }
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            _border.Draw(ScreenManager.SpriteBatch);

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        public override void HandleInput(InputState input)
        {
            if (FirstRun)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails(), this));
                FirstRun = false;
            }

            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails(), this));
            }

            HandleKeyboardInput(input);
            base.HandleInput(input);
        }

        private void HandleKeyboardInput(InputState input)
        {
            if (input.LastKeyboardState.IsKeyUp(Keys.R) && input.CurrentKeyboardState.IsKeyDown(Keys.R))
            {
            }
        }

        public string GetTitle()
        {
            return "Inactivity Controller";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Shows the usage of Inactivity Controller.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("Press T toggle the use of the controller");
            sb.AppendLine(string.Empty);
            sb.AppendLine(string.Empty);
            sb.AppendLine(string.Empty);
            return sb.ToString();
        }
    }
}