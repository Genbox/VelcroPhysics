using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.ScreenSystem
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
            _position.X = 50;
            _position.Y = 100;
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
            if (input.CurrentKeyboardState.IsKeyDown(Key.Up) && input.LastKeyboardState.IsKeyDown(Key.Up) == false)
            {
                _selectedEntry--;

                if (_selectedEntry < 0)
                    _selectedEntry = _menuEntries.Count - 1;
            }

            // Move to the next menu entry?
            if (input.CurrentKeyboardState.IsKeyDown(Key.Down) && input.LastKeyboardState.IsKeyDown(Key.Down) == false)
            {
                _selectedEntry++;

                if (_selectedEntry >= _menuEntries.Count)
                    _selectedEntry = 0;
            }

            // Accept or cancel the menu?
            if (input.CurrentKeyboardState.IsKeyDown(Key.Enter) && input.LastKeyboardState.IsKeyDown(Key.Enter) == false)
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

                    float pulsate = (float) Math.Sin(time * 3) + 1;
                    scale = 1 + pulsate * 0.05f;

                    color = Colors.White;
                }
                else
                {
                    // Other entries are white.
                    color = Colors.Black;
                    scale = 1;
                }

                // Modify the alpha to fade text out during transitions.
                color = Color.FromArgb(TransitionAlpha, color.R, color.G, color.B);

                // Draw text
                TextBlock txt = new TextBlock();
                txt.Text = _menuEntries[i];
                txt.Foreground = new SolidColorBrush(color);
                txt.FontSize=16;
                Canvas.SetLeft(txt, itemPosition.X);
                Canvas.SetTop(txt, itemPosition.Y);

                if (DebugCanvas != null)
                    DebugCanvas.Children.Add(txt);

                itemPosition.Y += 20;

                base.Draw(gameTime);
            }
        }
    }
}