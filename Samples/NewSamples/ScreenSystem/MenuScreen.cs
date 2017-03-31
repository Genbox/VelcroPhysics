using System;
using System.Collections.Generic;
using FarseerPhysics.Samples.MediaSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.Samples.ScreenSystem
{
    /// <summary>
    /// Base class for screens that contain a menu of options. The user can
    /// move up and down to select an entry, or cancel to back out of the screen.
    /// </summary>
    public class MenuScreen : GameScreen
    {
        private const int NumEntries = 11;
        private const int TitleBarHeight = 100;
        private const int EntrySpacer = 5;

        private Vector2 _menuEntrySize;
        private float _menuStart;
        private float _menuSpacing;
        private float _scrollSpacing;

        private List<MenuEntry> _menuEntries = new List<MenuEntry>();
        private MenuSlider _menuSlider;
        private bool _scrollHover;
        private bool _scrollLock;

        private int _selectedEntry;
        private int _hoverEntry;
        private int _menuOffset;

        private Vector2 _titlePosition;
        private Vector2 _titleOrigin;
        private Texture2D _samplesLogo;

        private Vector2 _previewPosition;
        private Vector2 _previewOrigin;

        private SpriteFont _font;

        public Vector2 PreviewPosition
        {
            get { return _previewPosition; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MenuScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.7);
            TransitionOffTime = TimeSpan.FromSeconds(0.7);
            HasCursor = true;
            _menuEntrySize = Vector2.Zero;
            _hoverEntry = -1;
            _selectedEntry = 0;
            _menuOffset = 0;
        }

        public void AddMenuItem(PhysicsDemoScreen screen, Texture2D preview)
        {
            MenuEntry entry = new MenuEntry(screen.GetTitle(), screen, preview);
            _menuEntrySize.X = Math.Max(_menuEntrySize.X, entry.Size.X + 20f);
            _menuEntrySize.Y = Math.Max(_menuEntrySize.Y, entry.Size.Y);
            _menuEntries.Add(entry);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            Viewport viewport = Framework.GraphicsDevice.Viewport;

            _font = ContentWrapper.GetFont("MenuFont");
            _samplesLogo = ContentWrapper.GetTexture("SamplesLogo");

            _titleOrigin = new Vector2(_samplesLogo.Width, _samplesLogo.Height) / 2f;
            _titlePosition = new Vector2(viewport.Width / 2f, TitleBarHeight / 2f);

            float horizontalSpacing = (viewport.Width / 2f - _menuEntrySize.X - EntrySpacer - _menuEntrySize.Y) / 3f;
            float verticalSpacing = (viewport.Height - TitleBarHeight - NumEntries * (_menuEntrySize.Y + EntrySpacer) + EntrySpacer) / 2f;

            _previewOrigin = new Vector2(viewport.Width / 4f, viewport.Height / 4f);
            _previewPosition = new Vector2(viewport.Width - _previewOrigin.X - horizontalSpacing, (viewport.Height - TitleBarHeight) / 2f + TitleBarHeight);

            _menuStart = _menuEntrySize.Y / 2f + verticalSpacing + TitleBarHeight;
            _menuSpacing = _menuEntrySize.Y + EntrySpacer;
            _menuEntries.Sort();

            MenuEntry.InitializeEntries(-_menuEntrySize.X / 2f, _menuEntrySize.X / 2f + horizontalSpacing);
            for (int i = 0; i < _menuEntries.Count; i++)
            {
                _menuEntries[i].InitializePosition(_menuStart + _menuSpacing * i, i < NumEntries);
            }

            _menuSlider = new MenuSlider(new Vector2(_menuEntrySize.X + horizontalSpacing + EntrySpacer + _menuEntrySize.Y / 2f, _menuStart));
            if (_menuEntries.Count > NumEntries)
            {
                _scrollSpacing = _menuSpacing * (NumEntries - 1) / (_menuEntries.Count - NumEntries);
            }
            else
            {
                _scrollSpacing = 0f;
            }
            _scrollHover = false;
            _scrollLock = false;
        }

        /// <summary>
        /// Returns the index of the menu entry at the position of the given mouse state.
        /// </summary>
        /// <returns>Index of menu entry if valid, -1 otherwise</returns>
        private int GetMenuEntryAt(Vector2 position)
        {
            for (int i = 0; i < _menuEntries.Count; i++)
            {
                if (_menuEntries[i].Visible)
                {
                    Rectangle boundingBox = new Rectangle((int)(_menuEntries[i].Position.X - _menuEntrySize.X / 2f), (int)(_menuEntries[i].Position.Y - _menuEntrySize.Y / 2f), (int)_menuEntrySize.X, (int)_menuEntrySize.Y);
                    if (boundingBox.Contains((int)position.X, (int)position.Y))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private bool GetPreviewCollision(Vector2 position)
        {
            Rectangle boundingBox = new Rectangle((int)(_previewPosition.X - _previewOrigin.X), (int)(_previewPosition.Y - _previewOrigin.Y), 2 * (int)_previewOrigin.X, 2 * (int)_previewOrigin.Y);
            if (boundingBox.Contains((int)position.X, (int)position.Y))
                return true;

            return false;
        }

        private bool GetSliderCollision(Vector2 position)
        {
            Rectangle boundingBox = new Rectangle((int)(_menuSlider.Position.X - _menuEntrySize.Y / 2f), (int)(_menuSlider.Position.Y - _menuEntrySize.Y / 2f), (int)_menuEntrySize.Y, (int)_menuEntrySize.Y);
            if (boundingBox.Contains((int)position.X, (int)position.Y))
                return true;

            return false;
        }

        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            // Mouse on a menu item
            _hoverEntry = GetMenuEntryAt(input.Cursor);

            // Accept or cancel the menu? 
            if (input.IsMenuSelect())
            {
                if (GetPreviewCollision(input.Cursor))
                {
                    if (_menuEntries[_selectedEntry].Screen != null)
                    {
                        Framework.AddScreen(_menuEntries[_selectedEntry].Screen);
                        Framework.AddScreen(new DescriptionBoxScreen((_menuEntries[_selectedEntry].Screen as PhysicsDemoScreen).GetDetails()));
                        ContentWrapper.PlaySoundEffect("Click");
                    }
                }
                if (_hoverEntry != -1)
                {
                    if (_selectedEntry == _hoverEntry)
                    {
                        if (_menuEntries[_selectedEntry].Screen != null)
                        {
                            Framework.AddScreen(_menuEntries[_selectedEntry].Screen);
                            Framework.AddScreen(new DescriptionBoxScreen((_menuEntries[_selectedEntry].Screen as PhysicsDemoScreen).GetDetails()));
                            ContentWrapper.PlaySoundEffect("Click");
                        }
                    }
                    else
                    {
                        _selectedEntry = _hoverEntry;
                    }
                }
            }

            if (GetSliderCollision(input.Cursor))
            {
                _scrollHover = true;
                if (input.IsMenuHold())
                {
                    _scrollLock = true;
                }
            }
            else
            {
                _scrollHover = false;
            }

            if (input.IsMenuRelease())
            {
                _scrollLock = false;
            }

            if (_scrollLock)
            {
                _menuOffset = (int)Math.Round((MathHelper.Clamp(input.Cursor.Y, _menuStart, _menuStart + _menuSpacing * (NumEntries - 1)) - _menuStart) / _scrollSpacing);
                UpdateMenuPositions();
            }

            if (input.IsScreenExit())
            {
                Framework.ExitGame();
            }

            if (input.IsMenuDown())
            {
                _menuOffset++;
                UpdateMenuPositions();
            }

            if (input.IsMenuUp())
            {
                _menuOffset--;
                UpdateMenuPositions();
            }
        }

        private void UpdateMenuPositions()
        {
            _menuOffset = (_menuOffset < 0) ? 0 : (_menuOffset > _menuEntries.Count - NumEntries) ? _menuEntries.Count - NumEntries : _menuOffset;
            if (_selectedEntry < _menuOffset)
            {
                _selectedEntry = _menuOffset;
            }
            else if (_selectedEntry >= NumEntries + _menuOffset)
            {
                _selectedEntry = NumEntries + _menuOffset - 1;
            }
            int targetIndex = -_menuOffset;
            // Update each nested MenuEntry position
            for (int i = 0; i < _menuEntries.Count; i++)
            {
                if (targetIndex < 0 || targetIndex >= NumEntries)
                {
                    _menuEntries[i].Visible = false;
                }
                else
                {
                    _menuEntries[i].Visible = true;
                }
                _menuEntries[i].Target = _menuStart + _menuSpacing * targetIndex;
                targetIndex++;
            }
            _menuSlider.Target = _menuStart + _scrollSpacing * _menuOffset;
        }

        /// <summary>
        /// Updates the menu.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Update each nested MenuEntry object.
            for (int i = 0; i < _menuEntries.Count; i++)
            {
                bool isHovered = IsActive && (i == _hoverEntry);
                bool isSelected = (i == _selectedEntry);

                _menuEntries[i].Update(isSelected, isHovered, gameTime);
            }

            _menuSlider.Update(_scrollHover, _scrollLock, gameTime);
        }

        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Vector2 transitionOffset = new Vector2(0f, (float)Math.Pow(TransitionPosition, 2) * 90f);

            // Draw each menu entry in turn.
            Quads.Begin();
            foreach (MenuEntry entry in _menuEntries)
            {
                Quads.Render(entry.Position - _menuEntrySize / 2f, entry.Position + _menuEntrySize / 2f, null, true,
                             ContentWrapper.Grey * entry.Alpha * TransitionAlpha, entry.TileColor * entry.Alpha * TransitionAlpha);
            }
            Quads.Render(_menuSlider.Position - new Vector2(_menuEntrySize.Y / 2f), _menuSlider.Position + new Vector2(_menuEntrySize.Y / 2f), null, true,
                         ContentWrapper.Grey * TransitionAlpha, _menuSlider.TileColor * TransitionAlpha);
            Quads.End();

            Sprites.Begin();
            foreach (MenuEntry entry in _menuEntries)
            {
                Sprites.DrawString(_font, entry.Text, entry.Position + Vector2.One, ContentWrapper.Black * entry.Alpha * entry.Alpha * TransitionAlpha,
                                   0f, entry.Origin, entry.Scale, SpriteEffects.None, 0f);
                Sprites.DrawString(_font, entry.Text, entry.Position, entry.TextColor * entry.Alpha * TransitionAlpha,
                                   0f, entry.Origin, entry.Scale, SpriteEffects.None, 0f);
                if (entry.Fade > 0f)
                {
                    Sprites.Draw(entry.Preview, _previewPosition, null, Color.White * Math.Max((TransitionAlpha - 0.8f) / 0.2f, 0f) * entry.Fade, 0f, _previewOrigin, 1f, SpriteEffects.None, 0f);
                }
            }
            Sprites.End();

            Quads.Begin();
            Quads.Render(Vector2.Zero, new Vector2(Framework.GraphicsDevice.Viewport.Width, TitleBarHeight), null, ContentWrapper.Grey * 0.7f * TransitionAlpha);
            Quads.End();

            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            Sprites.Begin();
            Sprites.Draw(_samplesLogo, _titlePosition - transitionOffset, null, Color.White, 0f, _titleOrigin, 1f, SpriteEffects.None, 0f);
            Sprites.End();
        }
    }
}