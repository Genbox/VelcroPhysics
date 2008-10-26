using System.Collections.Generic;
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
        private List<Box> _boxes = new List<Box>();
        private Texture2D _texture;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 150));

            // This distance has to be big enough to make sure all objects are reactivated 
            // before they could collide with an currently active object
            // our biggest object is 25*25 -> 120 would be big enough
            PhysicsSimulator.InactivityController.ActivationDistance = 120;

            // Deactivate the object after 2 seconds of idle time
            PhysicsSimulator.InactivityController.MaxIdleTime = 2000;
            PhysicsSimulator.InactivityController.Enabled = true;

            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            _border = new Border(ScreenManager.ScreenWidth, ScreenManager.ScreenHeight, 25, ScreenManager.ScreenCenter);

            base.Initialize();
        }

        public override void LoadContent()
        {
            _texture = DrawingSystem.DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 25, 25, Color.White, Color.Black);

            _border.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);

            //Create 5x13 boxes
            for (int y = 1; y < 6; y++)
            {
                for (int x = 1; x < 14; x++)
                {
                    Box box = new Box(new Vector2(300 + x * 28, 300 + y * 28));
                    box.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);
                    _boxes.Add(box);
                }
            }

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);

            _border.Draw(ScreenManager.SpriteBatch);

            //Draw explanation boxes
            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DiagnosticSpriteFont, "Disabled", new Vector2(130, 150), Color.White);
            ScreenManager.SpriteBatch.Draw(_texture, new Vector2(100, 145), Color.Green);

            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DiagnosticSpriteFont, "Moving", new Vector2(130, 180), Color.White);
            ScreenManager.SpriteBatch.Draw(_texture, new Vector2(100, 175), Color.Red);

            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DiagnosticSpriteFont, "Stopped", new Vector2(130, 210), Color.White);
            ScreenManager.SpriteBatch.Draw(_texture, new Vector2(100, 205), Color.Yellow);

            //Draw all the boxes
            foreach (Box box in _boxes)
            {
                Color color;

                //Visualize the current box state
                if (box.Body.Enabled)
                {
                    if (box.Body.Moves)
                        color = Color.Red;
                    else
                        color = Color.Yellow;
                }
                else
                    color = Color.Green;

                box.Draw(ScreenManager.SpriteBatch, color);
            }

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

            HandleMouseInput(input);
            base.HandleInput(input);
        }

        private void HandleMouseInput(InputState input)
        {
            if (input.CurrentMouseState.LeftButton == ButtonState.Pressed && input.LastMouseState.LeftButton == ButtonState.Released)
            {
                Box box = new Box(new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y));
                box.Load(ScreenManager.GraphicsDevice, PhysicsSimulator);
                _boxes.Add(box);
            }
        }

        public static string GetTitle()
        {
            return "Demo3: Inactivity Controller";
        }

        public static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Shows the usage of Inactivity Controller.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Note: Only some games can use this controller.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("The controller also needs a lot of tweaking");
            sb.AppendLine("to get the behavior perfect.");
            return sb.ToString();
        }
    }
}