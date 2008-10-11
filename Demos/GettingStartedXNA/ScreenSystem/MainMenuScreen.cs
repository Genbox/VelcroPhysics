using FarseerGames.GettingStarted.Demos.Demo1;
using FarseerGames.GettingStarted.Demos.Demo2;
using FarseerGames.GettingStarted.Demos.Demo3;
using FarseerGames.GettingStarted.Demos.Demo4;
using FarseerGames.GettingStarted.Demos.Demo5;
using FarseerGames.GettingStarted.Demos.Demo6;
using FarseerGames.GettingStarted.Demos.Demo7;
using FarseerGames.GettingStarted.Demos.Demo8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.GettingStarted.ScreenSystem
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
            MenuEntries.Add("Demo1: A Single Body");
            MenuEntries.Add("Demo2: Two Bodies With Geom");
            MenuEntries.Add("Demo3: Static Bodies And Offset Geometries");
            MenuEntries.Add("Demo4: Stacked Bodies");
            MenuEntries.Add("Demo5: Collision Categories");
            MenuEntries.Add("Demo6: Linear and Angular Spring Controllers");
            MenuEntries.Add("Demo7: Dynamic Angle Joints");
            MenuEntries.Add("Demo8: Broadphase Collision Stress Test");
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
                    ScreenManager.AddScreen(new Demo2Screen());
                    break;
                case 2:
                    ScreenManager.AddScreen(new Demo3Screen());
                    break;
                case 3:
                    ScreenManager.AddScreen(new Demo4Screen());
                    break;
                case 4:
                    ScreenManager.AddScreen(new Demo5Screen());
                    break;
                case 5:
                    ScreenManager.AddScreen(new Demo6Screen());
                    break;
                case 6:
                    ScreenManager.AddScreen(new Demo7Screen());
                    break;
                case 7:
                    ScreenManager.AddScreen(new Demo8Screen());
                    break;
                case 8:
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