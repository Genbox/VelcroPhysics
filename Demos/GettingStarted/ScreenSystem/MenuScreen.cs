#region Using Statements

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace FarseerGames.FarseerPhysicsDemos.ScreenSystem
{
    /// <summary>
    /// Base class for screens that contain a menu of options. The user can
    /// move up and down to select an entry, or cancel to back out of the screen.
    /// </summary>
    internal abstract class MenuScreen : GameScreen
    {
        #region Fields

        private readonly List<string> menuEntries = new List<string>();
        private float leftBorder = 100;
        private SpriteFont menuSpriteFont;
        private Vector2 position = Vector2.Zero;
        private int selectedEntry;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the list of menu entry strings, so derived classes can add
        /// or change the menu contents.
        /// </summary>
        protected IList<string> MenuEntries
        {
            get { return menuEntries; }
        }

        public float LeftBorder
        {
            get { return leftBorder; }
            set { leftBorder = value; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        protected MenuScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            // Move to the previous menu entry?
            if (input.MenuUp)
            {
                selectedEntry--;

                if (selectedEntry < 0)
                    selectedEntry = menuEntries.Count - 1;
            }

            // Move to the next menu entry?
            if (input.MenuDown)
            {
                selectedEntry++;

                if (selectedEntry >= menuEntries.Count)
                    selectedEntry = 0;
            }

            // Accept or cancel the menu?
            if (input.MenuSelect)
            {
                OnSelectEntry(selectedEntry);
            }
            else if (input.MenuCancel)
            {
                OnCancel();
            }
        }


        /// <summary>
        /// Notifies derived classes that a menu entry has been chosen.
        /// </summary>
        protected abstract void OnSelectEntry(int entryIndex);


        /// <summary>
        /// Notifies derived classes that the menu has been cancelled.
        /// </summary>
        protected abstract void OnCancel();

        public override void LoadContent()
        {
            menuSpriteFont = ScreenManager.ContentManager.Load<SpriteFont>("Content/Fonts/menuFont");
            CalculatePosition();
        }

        private void CalculatePosition()
        {
            float totalHeight = menuSpriteFont.MeasureString("T").Y;
            //totalHeight += ScreenManager.MenuSpriteFont.LineSpacing;
            totalHeight *= menuEntries.Count;

            position.Y = (ScreenManager.GraphicsDevice.Viewport.Height - totalHeight)/2;
            position.X = leftBorder;
        }

        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Vector2 itemPosition = position;

            // Draw each menu entry in turn.
            //ScreenManager.SpriteBatch.Begin();

            for (int i = 0; i < menuEntries.Count; i++)
            {
                Color color;
                float scale;

                if (IsActive && (i == selectedEntry))
                {
                    //// The selected entry is yellow, and has an animating size.
                    double time = gameTime.TotalGameTime.TotalSeconds;

                    float pulsate = (float) Math.Sin(time*3) + 1;
                    scale = 1 + pulsate*0.05f;

                    color = Color.White;
                    //scale = 1;
                }
                else
                {
                    // Other entries are white.
                    color = Color.Black;
                    scale = 1;
                }

                // Modify the alpha to fade text out during transitions.
                color = new Color(color.R, color.G, color.B, TransitionAlpha);

                // Draw text, centered on the middle of each line.
                Vector2 origin = new Vector2(0, ScreenManager.MenuSpriteFont.LineSpacing/2f);

                ScreenManager.SpriteBatch.DrawString(menuSpriteFont, menuEntries[i],
                                                     itemPosition, color, 0, origin, scale,
                                                     SpriteEffects.None, 0);
                // itemPosition.Y += ScreenManager.MenuSpriteFont.MeasureString("T").Y;
                itemPosition.Y += menuSpriteFont.LineSpacing;
            }

            // ScreenManager.SpriteBatch.End();
        }
    }
}