using FarseerGames.AdvancedSamples.Demos.Demo1;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.AdvancedSamples.ScreenSystem
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    internal class MainMenuScreen : MenuScreen
    {
        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen()
        {
            MenuEntries.Add("Demo1: Multithreaded Stacked Objects");
            MenuEntries.Add("Exit");
            LeftBorder = 100;
        }

        /// <summary>
        /// Responds to user menu selections.
        /// </summary>
        protected override void OnSelectEntry(int entryIndex)
        {
            switch (entryIndex)
            {
                case 0:
                    ScreenManager.AddScreen(new Demo1Screen());
                    break;
                case 1:
                    // Exit the sample.
                    ScreenManager.Game.Exit();
                    break;
            }

            ExitScreen();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DiagnosticSpriteFont,
                                                 "*toggle between debug and normal view using either F1 on the keyboard or 'Y' on the controller",
                                                 new Vector2(100, ScreenManager.ScreenHeight - 116), Color.Black);
            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DiagnosticSpriteFont,
                                                 "**keyboard users, use arrows and enter to navigate menus",
                                                 new Vector2(100, ScreenManager.ScreenHeight - 100), Color.Black);
            base.Draw(gameTime);
            ScreenManager.SpriteBatch.End();
        }
    }
}