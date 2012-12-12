#region Using System
using System;
using System.Collections.Generic;
#endregion
#region Using XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
#endregion
#region Using Farseer
using FarseerPhysics.Samples.MediaSystem;
#endregion

namespace FarseerPhysics.Samples.ScreenSystem
{
  /// <summary>
  ///   an enum of all available mouse buttons.
  /// </summary>
  public enum MouseButtons
  {
    LeftButton,
    MiddleButton,
    RightButton,
    ExtraButton1,
    ExtraButton2
  }

  public class InputHelper
  {
    private GamePadState _currentGamePadState;
    private KeyboardState _currentKeyboardState;
    private MouseState _currentMouseState;

    private GamePadState _lastGamePadState;
    private KeyboardState _lastKeyboardState;
    private MouseState _lastMouseState;

    private Vector2 _cursor;
    private bool _cursorIsVisible;
    private bool _cursorIsValid;
    private Sprite _cursorSprite;

    private Viewport _viewport;

    /// <summary>
    ///   Constructs a new input state.
    /// </summary>
    public InputHelper()
    {
      _currentKeyboardState = new KeyboardState();
      _currentGamePadState = new GamePadState();
      _currentMouseState = new MouseState();

      _lastKeyboardState = new KeyboardState();
      _lastGamePadState = new GamePadState();
      _lastMouseState = new MouseState();

      _cursorIsVisible = false;
      _cursorIsValid = true;

      _cursor = Vector2.Zero;
    }

    public GamePadState GamePadState
    {
      get { return _currentGamePadState; }
    }

    public KeyboardState KeyboardState
    {
      get { return _currentKeyboardState; }
    }

    public MouseState MouseState
    {
      get { return _currentMouseState; }
    }

    public GamePadState PreviousGamePadState
    {
      get { return _lastGamePadState; }
    }

    public KeyboardState PreviousKeyboardState
    {
      get { return _lastKeyboardState; }
    }

    public MouseState PreviousMouseState
    {
      get { return _lastMouseState; }
    }

    public bool ShowCursor
    {
      get { return _cursorIsVisible && _cursorIsValid; }
      set { _cursorIsVisible = value; }
    }

    public bool IsCursorValid
    {
      get { return _cursorIsValid; }
    }

    public Vector2 Cursor
    {
      get { return _cursor; }
    }

    public void LoadContent(Viewport viewport)
    {
      Texture2D cursorTexture;
      cursorTexture = ContentWrapper.GetTexture("cursor");
      _cursorSprite = new Sprite(cursorTexture, Vector2.One);
      _viewport = viewport;
    }

    /// <summary>
    ///   Reads the latest state of the keyboard and gamepad and mouse/touchpad.
    /// </summary>
    public void Update(GameTime gameTime)
    {
      _lastKeyboardState = _currentKeyboardState;
      _lastGamePadState = _currentGamePadState;
      _lastMouseState = _currentMouseState;

      _currentKeyboardState = Keyboard.GetState();
      _currentGamePadState = GamePad.GetState(PlayerIndex.One);
      _currentMouseState = Mouse.GetState();

      // Update cursor
      Vector2 oldCursor = _cursor;
      if (_currentGamePadState.IsConnected && _currentGamePadState.ThumbSticks.Left != Vector2.Zero)
      {
        _cursor += _currentGamePadState.ThumbSticks.Left * new Vector2(300f, -300f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        _cursor = Vector2.Clamp(_cursor, Vector2.Zero, new Vector2(_viewport.Width, _viewport.Height));
        Mouse.SetPosition((int)_cursor.X, (int)_cursor.Y);
      }
      else
      {
        _cursor.X = _currentMouseState.X;
        _cursor.Y = _currentMouseState.Y;
        _cursor = Vector2.Clamp(_cursor, Vector2.Zero, new Vector2(_viewport.Width, _viewport.Height));
      }

      if (_viewport.Bounds.Contains(_currentMouseState.X, _currentMouseState.Y))
      {
        _cursorIsValid = true;
      }
      else
      {
        _cursorIsValid = false;
      }
    }

    public void Draw(SpriteBatch batch)
    {
      if (_cursorIsVisible && _cursorIsValid)
      {
        batch.Begin();
        batch.Draw(_cursorSprite.Image, _cursor, null, Color.White, 0f, _cursorSprite.Origin, 1f, SpriteEffects.None, 0f);
        batch.End();
      }
    }

    /// <summary>
    ///   Helper for checking if a key was newly pressed during this update.
    /// </summary>
    public bool IsNewKeyPress(Keys key)
    {
      return (_currentKeyboardState.IsKeyDown(key) && _lastKeyboardState.IsKeyUp(key));
    }

    public bool IsNewKeyRelease(Keys key)
    {
      return (_lastKeyboardState.IsKeyDown(key) && _currentKeyboardState.IsKeyUp(key));
    }

    /// <summary>
    ///   Helper for checking if a button was newly pressed during this update.
    /// </summary>
    public bool IsNewButtonPress(Buttons button)
    {
      return (_currentGamePadState.IsButtonDown(button) && _lastGamePadState.IsButtonUp(button));
    }

    public bool IsNewButtonRelease(Buttons button)
    {
      return (_lastGamePadState.IsButtonDown(button) && _currentGamePadState.IsButtonUp(button));
    }

    /// <summary>
    ///   Helper for checking if a mouse button was newly pressed during this update.
    /// </summary>
    public bool IsNewMouseButtonPress(MouseButtons button)
    {
      switch (button)
      {
        case MouseButtons.LeftButton:
          return (_currentMouseState.LeftButton == ButtonState.Pressed && _lastMouseState.LeftButton == ButtonState.Released);
        case MouseButtons.RightButton:
          return (_currentMouseState.RightButton == ButtonState.Pressed && _lastMouseState.RightButton == ButtonState.Released);
        case MouseButtons.MiddleButton:
          return (_currentMouseState.MiddleButton == ButtonState.Pressed && _lastMouseState.MiddleButton == ButtonState.Released);
        case MouseButtons.ExtraButton1:
          return (_currentMouseState.XButton1 == ButtonState.Pressed && _lastMouseState.XButton1 == ButtonState.Released);
        case MouseButtons.ExtraButton2:
          return (_currentMouseState.XButton2 == ButtonState.Pressed && _lastMouseState.XButton2 == ButtonState.Released);
        default:
          return false;
      }
    }

    /// <summary>
    /// Checks if the requested mouse button is released.
    /// </summary>
    /// <param name="button">The button.</param>
    public bool IsNewMouseButtonRelease(MouseButtons button)
    {
      switch (button)
      {
        case MouseButtons.LeftButton:
          return (_lastMouseState.LeftButton == ButtonState.Pressed && _currentMouseState.LeftButton == ButtonState.Released);
        case MouseButtons.RightButton:
          return (_lastMouseState.RightButton == ButtonState.Pressed && _currentMouseState.RightButton == ButtonState.Released);
        case MouseButtons.MiddleButton:
          return (_lastMouseState.MiddleButton == ButtonState.Pressed && _currentMouseState.MiddleButton == ButtonState.Released);
        case MouseButtons.ExtraButton1:
          return (_lastMouseState.XButton1 == ButtonState.Pressed && _currentMouseState.XButton1 == ButtonState.Released);
        case MouseButtons.ExtraButton2:
          return (_lastMouseState.XButton2 == ButtonState.Pressed && _currentMouseState.XButton2 == ButtonState.Released);
        default:
          return false;
      }
    }

    /// <summary>
    /// Checks if the mouse wheel has been scrolled up
    /// </summary>
    public bool IsNewScrollWheelUp()
    {
      return _currentMouseState.ScrollWheelValue - _lastMouseState.ScrollWheelValue > 0;
    }

    /// <summary>
    /// Checks if the mouse wheel has been scrolled down
    /// </summary>
    public bool IsNewScrollWheelDown()
    {
      return _lastMouseState.ScrollWheelValue - _currentMouseState.ScrollWheelValue > 0;
    }

    /// <summary>
    ///   Checks for a "menu select" input action.
    /// </summary>
    public bool IsMenuSelect()
    {
      return IsNewKeyPress(Keys.Space) ||
             IsNewKeyPress(Keys.Enter) ||
             IsNewButtonPress(Buttons.A) ||
             IsNewMouseButtonPress(MouseButtons.LeftButton);
    }

    public bool IsMenuHold()
    {
      return IsNewButtonPress(Buttons.A) ||
             IsNewMouseButtonPress(MouseButtons.LeftButton);
    }

    public bool IsMenuRelease()
    {
      return _currentGamePadState.IsButtonUp(Buttons.A) &&
             _currentMouseState.LeftButton == ButtonState.Released;
    }

    /// <summary>
    ///   Checks for a "menu cancel" input action.
    /// </summary>
    public bool IsMenuCancel()
    {
      return IsNewKeyPress(Keys.Escape) ||
             IsNewKeyPress(Keys.Back) ||
             IsNewButtonPress(Buttons.B) ||
             IsNewButtonPress(Buttons.Back);
    }

    public bool IsMenuUp()
    {
      return IsNewKeyPress(Keys.Up) ||
             IsNewKeyPress(Keys.PageUp) ||
             IsNewButtonPress(Buttons.DPadUp) ||
             IsNewButtonPress(Buttons.RightThumbstickUp) ||
             IsNewScrollWheelUp();
    }

    public bool IsMenuDown()
    {
      return IsNewKeyPress(Keys.Down) ||
             IsNewKeyPress(Keys.PageDown) ||
             IsNewButtonPress(Buttons.DPadDown) ||
             IsNewButtonPress(Buttons.RightThumbstickDown) ||
             IsNewScrollWheelDown();
    }

    public bool IsScreenExit()
    {
      return IsNewKeyPress(Keys.Escape) ||
             IsNewKeyPress(Keys.Back) ||
             IsNewButtonPress(Buttons.Back);
    }
  }
}