using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.SamplesFramework
{
    /// <summary>
    /// Base class for screens that contain a menu of options. The user can
    /// move up and down to select an entry, or cancel to back out of the screen.
    /// </summary>
    public class MenuScreen : GameScreen
    {
#if WINDOWS || XBOX
        private const float NumEntries = 15;
        //private const float NumEntries = 9;  //TODO remove me - emulate WP7 debug mode
#elif WINDOWS_PHONE
        private const float NumEntries = 9;
#endif
        private List<MenuEntry> _menuEntries = new List<MenuEntry>();
        private string _menuTitle;
        private Vector2 titlePosition;
        private Vector2 titleOrigin;
        private int _selectedEntry;
        private float _menuBorderTop;
        private float _menuBorderBottom;
        private float _menuBorderMargin;
        private float _menuOffset;
        private float _maxOffset;

        private Texture2D texScrollButton;
        private Texture2D texSlider;

        private MenuButton _scrollUp;
        private MenuButton _scrollDown;
        private MenuButton _scrollSlider;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MenuScreen(string menuTitle)
        {
            _menuTitle = menuTitle;

            TransitionOnTime = TimeSpan.FromSeconds(0.7);
            TransitionOffTime = TimeSpan.FromSeconds(0.7);
            HasCursor = true;
        }

        public void AddMenuItem(string name, EntryType type, GameScreen screen)
        {
            MenuEntry entry = new MenuEntry(this, name, type, screen);
            _menuEntries.Add(entry);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            SpriteFont font = ScreenManager.Fonts.MenuSpriteFont;

            texScrollButton = ScreenManager.Content.Load<Texture2D>("Common/arrow");
            texSlider = ScreenManager.Content.Load<Texture2D>("Common/slider");

            float _scrollBarPos = viewport.Width / 2f;
            for (int i = 0; i < _menuEntries.Count; ++i)
            {
                _menuEntries[i].Initialize();
                _scrollBarPos = Math.Min(_scrollBarPos,
                                         (viewport.Width - _menuEntries[i].GetWidth()) / 2f);
            }
            _scrollBarPos -= texScrollButton.Width + 2f;

            titleOrigin = font.MeasureString(_menuTitle) / 2f;
            titlePosition = new Vector2(viewport.Width / 2f, font.MeasureString("M").Y / 2f + 10f);

            _menuBorderMargin = font.MeasureString("M").Y * 0.8f;
            _menuBorderTop = (viewport.Height - _menuBorderMargin * (NumEntries - 1)) / 2f;
            _menuBorderBottom = (viewport.Height + _menuBorderMargin * (NumEntries - 1)) / 2f;

            _menuOffset = 0f;
            _maxOffset = Math.Max(0f, (_menuEntries.Count - NumEntries) * _menuBorderMargin);

            _scrollUp = new MenuButton(texScrollButton, false, true,
                                       new Vector2(_scrollBarPos, _menuBorderTop - texScrollButton.Height), this);
            _scrollDown = new MenuButton(texScrollButton, true, true,
                                         new Vector2(_scrollBarPos, _menuBorderBottom + texScrollButton.Height), this);
            _scrollSlider = new MenuButton(texSlider, false, false, new Vector2(_scrollBarPos, _menuBorderTop), this);
        }

        /// <summary>
        /// Returns the index of the menu entry at the position of the given mouse state.
        /// </summary>
        /// <param name="state">Mouse state to use for the test</param>
        /// <returns>Index of menu entry if valid, -1 otherwise</returns>
        private int GetMenuEntryAt(Vector2 position)
        {
            int index = 0;
            foreach (MenuEntry entry in _menuEntries)
            {
                float width = entry.GetWidth();
                float height = entry.GetHeight();
                Rectangle rect = new Rectangle((int)(entry.Position.X - width / 2f),
                                               (int)(entry.Position.Y - height / 2f),
                                               (int)width, (int)height);
                if (rect.Contains((int)position.X, (int)position.Y) && entry.Alpha > 0.1f)
                {
                    return index;
                }
                ++index;
            }
            return -1;
        }

        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            // Mouse or touch on a menu item
            int hoverIndex = GetMenuEntryAt(input.Cursor);
            if (hoverIndex > -1 && _menuEntries[hoverIndex].IsSelectable())
            {
                _selectedEntry = hoverIndex;
            }
            else
            {
                _selectedEntry = -1;
            }

            _scrollUp.Collide(input.Cursor);
            _scrollDown.Collide(input.Cursor);

            // Accept or cancel the menu? 
            if (input.IsMenuSelect() && _selectedEntry != -1)
            {
                if (_menuEntries[_selectedEntry].IsExitItem())
                {
                    ScreenManager.Game.Exit();
                }
                else if (_menuEntries[_selectedEntry].Screen != null)
                {
                    ScreenManager.AddScreen(_menuEntries[_selectedEntry].Screen);
                    if (_menuEntries[_selectedEntry].Screen is IDemoScreen)
                    {
                        ScreenManager.AddScreen(new MessageBoxScreen((_menuEntries[_selectedEntry].Screen as IDemoScreen).GetDetails()));
                    }
                }
            }
            else if (input.IsMenuCancel())
            {
                ScreenManager.Game.Exit();
            }

            if (input.IsMenuPressed())
            {
                if (_scrollUp.Hover)
                {
                    _menuOffset = Math.Max(_menuOffset - 160f * (float)gameTime.ElapsedGameTime.TotalSeconds, 0f);
                }
                if (_scrollDown.Hover)
                {
                    _menuOffset = Math.Min(_menuOffset + 160f * (float)gameTime.ElapsedGameTime.TotalSeconds, _maxOffset);
                }
            }
        }

        /// <summary>
        /// Allows the screen the chance to position the menu entries. By default
        /// all menu entries are lined up in a vertical list, centered on the screen.
        /// </summary>
        protected virtual void UpdateMenuEntryLocations()
        {
            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            Vector2 position = Vector2.Zero;
            position.Y = _menuBorderTop - _menuOffset;

            // update each menu entry's location in turn
            for (int i = 0; i < _menuEntries.Count; ++i)
            {
                position.X = ScreenManager.GraphicsDevice.Viewport.Width / 2f;
                if (ScreenState == ScreenState.TransitionOn)
                {
                    position.X -= transitionOffset * 256;
                }
                else
                {
                    position.X += transitionOffset * 256;
                }

                // set the entry's position
                _menuEntries[i].Position = position;

                if (position.Y < _menuBorderTop)
                {
                    _menuEntries[i].Alpha = 1f - Math.Min(_menuBorderTop - position.Y, _menuBorderMargin) / _menuBorderMargin;
                }
                if (position.Y > _menuBorderBottom)
                {
                    _menuEntries[i].Alpha = 1f - Math.Min(position.Y - _menuBorderBottom, _menuBorderMargin) / _menuBorderMargin;
                }

                // move down for the next entry the size of this entry
                position.Y += _menuEntries[i].GetHeight();
            }
            Vector2 scrollPos = _scrollSlider.Position;
            scrollPos.Y = MathHelper.Lerp(_menuBorderTop, _menuBorderBottom, _menuOffset / _maxOffset);
            _scrollSlider.Position = scrollPos;
        }

        /// <summary>
        /// Updates the menu.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                    bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Update each nested MenuEntry object.
            for (int i = 0; i < _menuEntries.Count; ++i)
            {
                bool isSelected = IsActive && (i == _selectedEntry);
                _menuEntries[i].Update(isSelected, gameTime);
            }

            _scrollUp.Update(gameTime);
            _scrollDown.Update(gameTime);
            _scrollSlider.Update(gameTime);
        }

        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // make sure our entries are in the right place before we draw them
            UpdateMenuEntryLocations();

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Fonts.MenuSpriteFont;

            spriteBatch.Begin();
            // Draw each menu entry in turn.
            for (int i = 0; i < _menuEntries.Count; ++i)
            {
                bool isSelected = IsActive && (i == _selectedEntry);
                _menuEntries[i].Draw(isSelected);
            }

            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            Vector2 transitionOffset = new Vector2(0f, (float)Math.Pow(TransitionPosition, 2) * 100f);

            spriteBatch.DrawString(font, _menuTitle, titlePosition - transitionOffset + Vector2.One * 2f, Color.Black, 0,
                                   titleOrigin, 1f, SpriteEffects.None, 0);
            spriteBatch.DrawString(font, _menuTitle, titlePosition - transitionOffset, new Color(255, 210, 0), 0,
                                   titleOrigin, 1f, SpriteEffects.None, 0);
            _scrollUp.Draw();
            _scrollSlider.Draw();
            _scrollDown.Draw();
            spriteBatch.End();
        }
    }
}
