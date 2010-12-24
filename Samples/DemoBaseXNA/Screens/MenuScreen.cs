#region File Description

//-----------------------------------------------------------------------------
// MenuScreen.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.DemoBaseXNA.Screens
{
    /// <summary>
    /// Base class for screens that contain a menu of options. The user can
    /// move up and down to select an entry, or cancel to back out of the screen.
    /// </summary>
    public abstract class MenuScreen : GameScreen
    {
        public float LeftBorder = 60;
        private List<MenuEntry> _menuEntries = new List<MenuEntry>();
        public string MenuTitle;
        private int _selectedEntry;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MenuScreen(string menuTitle)
        {
            MenuTitle = menuTitle;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        /// <summary>
        /// Gets the list of menu entries, so derived classes can add
        /// or change the menu contents.
        /// </summary>
        protected IList<MenuEntry> MenuEntries
        {
            get { return _menuEntries; }
        }

#if !XBOX
        /// <summary>
        /// Returns the index of the menu entry at the position of the given mouse state.
        /// </summary>
        /// <param name="state">Mouse state to use for the test</param>
        /// <returns>Index of menu entry if valid, -1 otherwise</returns>
        private int GetMenuEntryAt(ref MouseState state)
        {
            Point pos = new Point(state.X, state.Y);
            int index = 0;
            foreach (MenuEntry entry in _menuEntries)
            {
                float width = entry.GetWidth(this);
                float height = entry.GetHeight(this);
                Rectangle rect = new Rectangle((int) entry.Position.X, (int) (entry.Position.Y - (height*0.5f)),
                                               (int) width, (int) height);
                if (rect.Contains(pos))
                {
                    return index;
                }
                ++index;
            }
            return -1;
        }
#endif

        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(InputHelper input)
        {
            // Move to the previous menu entry?
            if (input.IsMenuUp(ControllingPlayer))
            {
                _selectedEntry--;

                if (_selectedEntry < 0)
                    _selectedEntry = _menuEntries.Count - 1;
            }

            // Move to the next menu entry?
            if (input.IsMenuDown(ControllingPlayer))
            {
                _selectedEntry++;

                if (_selectedEntry >= _menuEntries.Count)
                    _selectedEntry = 0;
            }

#if !XBOX
            // Mouse or touch on a menu item
            if (input.CurrentMouseState.X != input.LastMouseState.X ||
                input.CurrentMouseState.Y != input.LastMouseState.Y)
            {
                int hoverIndex = GetMenuEntryAt(ref input.CurrentMouseState);
                if (hoverIndex > -1)
                {
                    _selectedEntry = hoverIndex;
                }
            }

            if (input.CurrentMouseState.LeftButton == ButtonState.Released &&
                input.LastMouseState.LeftButton == ButtonState.Pressed)
            {
                int index = GetMenuEntryAt(ref input.LastMouseState);
                if (index > -1)
                {
                    _selectedEntry = index;
                    OnSelectEntry(_selectedEntry, input.DefaultPlayerIndex);
                }
            }
#endif

            // Accept or cancel the menu? We pass in our ControllingPlayer, which may
            // either be null (to accept input from any player) or a specific index.
            // If we pass a null controlling player, the InputState helper returns to
            // us which player actually provided the input. We pass that through to
            // OnSelectEntry and OnCancel, so they can tell which player triggered them.
            PlayerIndex playerIndex;

            if (input.IsMenuSelect(ControllingPlayer, out playerIndex))
            {
                OnSelectEntry(_selectedEntry, playerIndex);
            }
            else if (input.IsMenuCancel(ControllingPlayer, out playerIndex))
            {
                OnCancel(playerIndex);
            }
        }

        /// <summary>
        /// Handler for when the user has chosen a menu entry.
        /// </summary>
        protected virtual void OnSelectEntry(int entryIndex, PlayerIndex playerIndex)
        {
            _menuEntries[entryIndex].OnSelectEntry(playerIndex);
        }

        /// <summary>
        /// Handler for when the user has cancelled the menu.
        /// </summary>
        protected virtual void OnCancel(PlayerIndex playerIndex)
        {
        }

        /// <summary>
        /// Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
        /// </summary>
        protected void OnCancel(object sender, PlayerIndexEventArgs e)
        {
            OnCancel(e.PlayerIndex);
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
            float transitionOffset = (float) Math.Pow(TransitionPosition, 2);

            // start at Y = 175; each X value is generated per entry
#if WINDOWS_PHONE
            Vector2 position = new Vector2(0f, 125f);
#else
            Vector2 position = new Vector2(0f, 175f);
#endif
            // update each menu entry's location in turn
            for (int i = 0; i < _menuEntries.Count; i++)
            {
                MenuEntry menuEntry = _menuEntries[i];

                // each entry is to be centered horizontally
                position.X = LeftBorder;

                if (ScreenState == ScreenState.TransitionOn)
                    position.X -= transitionOffset*256;
                else
                    position.X += transitionOffset*512;

                // set the entry's position
                menuEntry.Position = position;

                // move down for the next entry the size of this entry
                position.Y += menuEntry.GetHeight(this);
            }
        }

        /// <summary>
        /// Updates the menu.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                    bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Update each nested MenuEntry object.
            for (int i = 0; i < _menuEntries.Count; i++)
            {
                bool isSelected = IsActive && (i == _selectedEntry);

                _menuEntries[i].Update(isSelected, gameTime);
            }
        }

        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // make sure our entries are in the right place before we draw them
            UpdateMenuEntryLocations();

            GraphicsDevice graphics = ScreenManager.GraphicsDevice;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.SpriteFonts.MenuSpriteFont;

            spriteBatch.Begin();

            // Draw each menu entry in turn.
            for (int i = 0; i < _menuEntries.Count; i++)
            {
                MenuEntry menuEntry = _menuEntries[i];

                bool isSelected = IsActive && (i == _selectedEntry);

                menuEntry.Draw(this, isSelected, gameTime);
            }

            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float) Math.Pow(TransitionPosition, 2);

            // Draw the menu title centered on the screen
            Vector2 titlePosition = new Vector2(graphics.Viewport.Width/2f, 80);
            Vector2 titleOrigin = font.MeasureString(MenuTitle)/2;
            const float titleScale = 0.9f;

            titlePosition.Y -= transitionOffset*100;

            spriteBatch.DrawString(font, MenuTitle, titlePosition, Color.White, 0,
                                   titleOrigin, titleScale, SpriteEffects.None, 0);

            spriteBatch.End();
        }
    }
}