using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.ScreenSystem
{
    /// <summary>
    /// Base class for screens that contain a menu of options. The user can
    /// move up and down to select an entry, or cancel to back out of the screen.
    /// </summary>
    public abstract class MenuScreen : GameScreen
    {
        private float _leftBorder = 100;
        private List<string> _menuEntries = new List<string>();
        private Vector2 _position = Vector2.Zero;
        private int _selectedEntry;

        /// <summary>
        /// Constructor.
        /// </summary>
        protected MenuScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        /// <summary>
        /// Gets the list of menu entry strings, so derived classes can add
        /// or change the menu contents.
        /// </summary>
        protected IList<string> MenuEntries
        {
            get { return _menuEntries; }
        }

        public float LeftBorder
        {
            get { return _leftBorder; }
            set { _leftBorder = value; }
        }

        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or canceling the menu.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            // Move to the previous menu entry?
            if (input.MenuUp)
            {
                _selectedEntry--;

                if (_selectedEntry < 0)
                    _selectedEntry = _menuEntries.Count - 1;
            }

            // Move to the next menu entry?
            if (input.MenuDown)
            {
                _selectedEntry++;

                if (_selectedEntry >= _menuEntries.Count)
                    _selectedEntry = 0;
            }

            // Accept or cancel the menu?
            if (input.MenuSelect)
            {
                OnSelectEntry(_selectedEntry);
            }
        }

        /// <summary>
        /// Notifies derived classes that a menu entry has been chosen.
        /// </summary>
        protected abstract void OnSelectEntry(int entryIndex);

        public override void LoadContent()
        {
            CalculatePosition();
        }

        private void CalculatePosition()
        {
            float totalHeight = ScreenManager.SpriteFonts.MenuSpriteFont.MeasureString("T").Y;
            totalHeight *= _menuEntries.Count;

            _position.Y = (ScreenManager.GraphicsDevice.Viewport.Height - totalHeight)/2;
            _position.X = _leftBorder;
        }

        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Vector2 itemPosition = _position;

            for (int i = 0; i < _menuEntries.Count; i++)
            {
                Color color;
                float scale;

                if (IsActive && (i == _selectedEntry))
                {
                    //// The selected entry is yellow, and has an animating size.
                    double time = gameTime.TotalGameTime.TotalSeconds;

                    float pulsate = (float) Math.Sin(time*3) + 1;
                    scale = 1 + pulsate*0.05f;

                    color = Color.White;
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
                Vector2 origin = new Vector2(0, ScreenManager.SpriteFonts.MenuSpriteFont.LineSpacing/2f);

                ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.MenuSpriteFont, _menuEntries[i],
                                                     itemPosition, color, 0, origin, scale,
                                                     SpriteEffects.None, 0);
                itemPosition.Y += ScreenManager.SpriteFonts.MenuSpriteFont.LineSpacing;
            }
        }
    }
}