using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.ScreenSystem
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    public class MainMenuScreen : MenuScreen
    {
        private int _id;
        private Dictionary<int, MenuItem> _mainMenuItems = new Dictionary<int, MenuItem>();

        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen()
        {
            LeftBorder = 100;
        }

        public void AddMainMenuItem(string name, GameScreen screen)
        {
            AddMainMenuItem(name, screen, false);
        }

        public void AddMainMenuItem(string name, GameScreen screen, bool isExitItem)
        {
            _mainMenuItems.Add(_id++, new MenuItem(screen, isExitItem));
            MenuEntries.Add(name);
        }

        /// <summary>
        /// Responds to user menu selections.
        /// </summary>
        protected override void OnSelectEntry(int entryIndex)
        {
            if (_mainMenuItems[entryIndex].IsExitItem)
                ScreenManager.Game.Exit();
            else
            {
                ScreenManager.AddScreen(_mainMenuItems[entryIndex].Screen);
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