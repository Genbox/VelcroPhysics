using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace FarseerPhysics.ScreenSystem
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
            TransitionOnTime = TimeSpan.FromSeconds(0.5f);
            TransitionOffTime = TimeSpan.FromSeconds(0.5f);
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
            ScreenManager.AddScreen(_mainMenuItems[entryIndex].Screen);
            ExitScreen();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // Draw text
            TextBlock txt = new TextBlock();
            txt.Text = "1) Toggle between debug and normal view using F1\r\n2) Use arrows and enter to navigate menus";
            txt.Foreground = new SolidColorBrush(Colors.White);
            txt.FontSize = 16;
            Canvas.SetLeft(txt, 20);
            Canvas.SetTop(txt, 40);

            if (DebugCanvas != null)
                DebugCanvas.Children.Add(txt);
        }
    }
}