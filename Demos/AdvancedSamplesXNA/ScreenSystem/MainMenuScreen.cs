using FarseerGames.AdvancedSamples.Demos.Demo1;
using FarseerGames.AdvancedSamples.Demos.Demo2;
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
            MenuEntries.Add("Demo2: Object pre-loading/caching (Starts with pool disabled)");
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
                    ScreenManager.AddScreen(new Demo2Screen(false));
                    break;
                case 2:
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
                                                 "1) Toggle between debug and normal view using either F1 on the keyboard or 'Y' on the controller",
                                                 new Vector2(100, ScreenManager.ScreenHeight - 116), Color.White);
            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DiagnosticSpriteFont,
                                                 "2) Keyboard users, use arrows and enter to navigate menus",
                                                 new Vector2(100, ScreenManager.ScreenHeight - 100), Color.White);
            base.Draw(gameTime);
            ScreenManager.SpriteBatch.End();
        }
    }
}