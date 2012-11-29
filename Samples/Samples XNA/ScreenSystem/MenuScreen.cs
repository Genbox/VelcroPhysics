#region Using System
using System;
using System.Collections.Generic;
#endregion
#region Using XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion
#region Using Farseer
using FarseerPhysics.Samples.MediaSystem;
#endregion

namespace FarseerPhysics.Samples.ScreenSystem
{
  /// <summary>
  /// Base class for screens that contain a menu of options. The user can
  /// move up and down to select an entry, or cancel to back out of the screen.
  /// </summary>
  public class MenuScreen : GameScreen
  {
    private const int NumEntries = 12;
    private const int TitleBarHeight = 100;

    private Vector2 _menuEntrySize;
    private Vector2 _menuStart;
    private Vector2 _menuSpacing;

    private List<MenuEntry> _menuEntries = new List<MenuEntry>();

    private int _selectedEntry;
    private int _hoverEntry;
    private int _menuOffset;

    private Vector2 _titlePosition;
    private Vector2 _titleOrigin;
    private Texture2D _samplesLogo;

    private Vector2 _previewPosition;
    private Vector2 _previewOrigin;

    private Texture2D _texScrollButton;
    private Texture2D _texSlider;
    private MenuButton _scrollUp;
    private MenuButton _scrollDown;
    private MenuButton _scrollSlider;

    private SpriteFont _font;

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

    public void AddMenuItem(PhysicsGameScreen screen, Texture2D preview)
    {
      MenuEntry entry = new MenuEntry(screen.GetTitle(), screen, preview);
      _menuEntrySize.X = Math.Max(_menuEntrySize.X, entry.Size.X);
      _menuEntrySize.Y = Math.Max(_menuEntrySize.Y, entry.Size.Y);
      _menuEntries.Add(entry);
    }

    public override void LoadContent()
    {
      base.LoadContent();

      Viewport viewport = Framework.GraphicsDevice.Viewport;

      MediaManager.GetFont("menuFont", out _font);
      MediaManager.GetTexture("samplesLogo", out _samplesLogo);
      MediaManager.GetTexture("arrow", out _texScrollButton);
      MediaManager.GetTexture("slider", out _texSlider);

      _titleOrigin = new Vector2(_samplesLogo.Width, _samplesLogo.Height) / 2f;
      _titlePosition = new Vector2(viewport.Width / 2f, TitleBarHeight / 2f);

      _scrollUp = new MenuButton(_texScrollButton, false, new Vector2(700f, 200f));
      _scrollDown = new MenuButton(_texScrollButton, true, new Vector2(700f, 520f));
      //_scrollSlider = new MenuButton(_texSlider, false, new Vector2(scrollBarPos, _menuBorderTop));

      _menuEntrySize.X += 20;

      float horizontalSpacing = ((viewport.Width / 2f) - _menuEntrySize.X - 100f) / 3f;
      float verticalSpacing = (viewport.Height - TitleBarHeight - NumEntries * (_menuEntrySize.Y + 5f)) / 2f;

      _previewOrigin = new Vector2(viewport.Width / 4f, viewport.Height / 4f);
      _previewPosition = new Vector2(viewport.Width - _previewOrigin.X - horizontalSpacing, (viewport.Height - TitleBarHeight) / 2f + TitleBarHeight);

      _menuStart.X = _menuEntrySize.X / 2f + horizontalSpacing;
      _menuStart.Y = _menuEntrySize.Y / 2f + verticalSpacing + TitleBarHeight;
      _menuSpacing.Y = _menuEntrySize.Y + 5f;
      _menuEntries.Sort();
      for (int i = 0; i < _menuEntries.Count; i++)
      {
        _menuEntries[i].InitializePosition(_menuStart + _menuSpacing * i);
      }
    }

    /// <summary>
    /// Returns the index of the menu entry at the position of the given mouse state.
    /// </summary>
    /// <returns>Index of menu entry if valid, -1 otherwise</returns>
    private int GetMenuEntryAt(Vector2 position)
    {
      for (int i = 0; i < _menuEntries.Count; i++)
      {
        Rectangle boundingBox = new Rectangle((int)(_menuEntries[i].Position.X - _menuEntrySize.X / 2f), (int)(_menuEntries[i].Position.Y - _menuEntrySize.Y / 2f), (int)_menuEntrySize.X, (int)_menuEntrySize.Y);
        if (boundingBox.Contains((int)position.X, (int)position.Y))
        {
          return i;
        }
      }
      return -1;
    }

    /// <summary>
    /// Responds to user input, changing the selected entry and accepting
    /// or cancelling the menu.
    /// </summary>
    public override void HandleInput(InputHelper input, GameTime gameTime)
    {
      // Mouse on a menu item
      _hoverEntry = GetMenuEntryAt(input.Cursor);

      if (input.IsCursorValid)
      {
        _scrollUp.Collide(input.Cursor);
        _scrollDown.Collide(input.Cursor);
      }
      else
      {
        _scrollUp.Hover = false;
        _scrollDown.Hover = false;
      }

      // Accept or cancel the menu? 
      if (input.IsMenuSelect() && _hoverEntry != -1)
      {
        if (_selectedEntry == _hoverEntry)
        {
          if (_menuEntries[_selectedEntry].Screen != null)
          {
            Framework.AddScreen(_menuEntries[_selectedEntry].Screen);
            Framework.AddScreen(new MessageBoxScreen((_menuEntries[_selectedEntry].Screen as PhysicsGameScreen).GetDetails()));
            MediaManager.PlaySoundEffect("click");
          }
        }
        else
        {
          _selectedEntry = _hoverEntry;
        }
      }

      if (input.IsMenuCancel())
      {
        Framework.ExitGame();
      }

      if (input.IsNewKeyPress(Keys.Down))
      {
        _menuOffset++;
        _menuOffset = (_menuOffset < 0) ? 0 : (_menuOffset > _menuEntries.Count - NumEntries) ? _menuEntries.Count - NumEntries : _menuOffset;
      }

      if (input.IsNewKeyPress(Keys.Up))
      {
        _menuOffset--;
        _menuOffset = (_menuOffset < 0) ? 0 : (_menuOffset > _menuEntries.Count - NumEntries) ? _menuEntries.Count - NumEntries : _menuOffset;
      }

      if (input.IsMenuPressed())
      {
        if (_scrollUp.Hover)
        {

        }
        if (_scrollDown.Hover)
        {

        }
      }
    }

    /// <summary>
    /// Updates the menu.
    /// </summary>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
      base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

      int targetIndex = -_menuOffset;
      // Update each nested MenuEntry object.
      for (int i = 0; i < _menuEntries.Count; i++)
      {
        bool isHovered = IsActive && (i == _hoverEntry);
        bool isSelected = (i == _selectedEntry);

        if (targetIndex < 0)
        {
          _menuEntries[i].Position = _menuStart + _menuSpacing * targetIndex - new Vector2(_menuEntrySize.X * 2f, 0f);
        }
        else if (targetIndex < NumEntries)
        {
          _menuEntries[i].Position = _menuStart + _menuSpacing * targetIndex;
        }
        else
        {
          _menuEntries[i].Position = _menuStart + _menuSpacing * targetIndex - new Vector2(_menuEntrySize.X * 2f, 0f);
        }

        _menuEntries[i].Update(isSelected, isHovered, gameTime);
        targetIndex++;
      }

      _scrollUp.Update(gameTime);
      _scrollDown.Update(gameTime);
    }

    /// <summary>
    /// Draws the menu.
    /// </summary>
    public override void Draw(GameTime gameTime)
    {
      // Draw each menu entry in turn.
      Quads.Begin();
      foreach (MenuEntry entry in _menuEntries)
      {
        Quads.Render(entry.Position - _menuEntrySize / 2f, entry.Position + _menuEntrySize / 2f, null, true, AssetCreator.Grey, entry.TileColor);
      }
      Quads.End();

      Sprites.Begin();
      foreach (MenuEntry entry in _menuEntries)
      {
        Sprites.DrawString(_font, entry.Text, entry.Position + Vector2.One, AssetCreator.Black, 0f, entry.Origin, entry.Scale, SpriteEffects.None, 0f);
        Sprites.DrawString(_font, entry.Text, entry.Position, entry.TextColor, 0f, entry.Origin, entry.Scale, SpriteEffects.None, 0f);
      }
      Sprites.Draw(_menuEntries[_selectedEntry].Preview, _previewPosition, null, Color.White, 0f, _previewOrigin, 1f, SpriteEffects.None, 0f);
      Sprites.End();

      Quads.Begin();
      Quads.Render(Vector2.Zero, new Vector2(1280f, TitleBarHeight), null, AssetCreator.Grey * 0.7f);
      Quads.End();

      // Make the menu slide into place during transitions, using a
      // power curve to make things look more interesting (this makes
      // the movement slow down as it nears the end).
      Vector2 transitionOffset = new Vector2(0f, (float)Math.Pow(TransitionPosition, 2) * 90f);
      Sprites.Begin();
      Sprites.Draw(_samplesLogo, _titlePosition - transitionOffset, null, Color.White, 0f, _titleOrigin, 1f, SpriteEffects.None, 0f);
      //_scrollUp.Draw(Sprites);
      //_scrollSlider.Draw(Sprites);
      //_scrollDown.Draw(Sprites);
      Sprites.End();
    }
  }
}