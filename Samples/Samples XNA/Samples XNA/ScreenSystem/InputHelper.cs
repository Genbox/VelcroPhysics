using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace FarseerPhysics.SamplesFramework
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
        private KeyboardState CurrentKeyboardState;
        private GamePadState CurrentGamePadState;
        private MouseState CurrentMouseState;

        private KeyboardState LastKeyboardState;
        private GamePadState LastGamePadState;
        private MouseState LastMouseState;

        public readonly List<GestureSample> Gestures = new List<GestureSample>();

        private Vector2 _cursor;
        private Vector2 _cursorOffset;
        private Texture2D _cursorSprite;
        private bool _cursorIsValid;
        private bool _cursorIsVisible;
        private bool _cursorMoved;

        private Viewport viewport;

        private ScreenManager _manager;

        /// <summary>
        ///   Constructs a new input state.
        /// </summary>
        public InputHelper(ScreenManager manager)
        {
            CurrentKeyboardState = new KeyboardState();
            CurrentGamePadState = new GamePadState();
            CurrentMouseState = new MouseState();

            LastKeyboardState = new KeyboardState();
            LastGamePadState = new GamePadState();
            LastMouseState = new MouseState();

            _manager = manager;

            _cursorIsVisible = false;
            _cursorMoved = false;
#if WINDOWS_PHONE
            _cursorIsValid = false;
#else
            _cursorIsValid = true;
#endif
            _cursor = Vector2.Zero;
        }

        public void LoadContent()
        {
            _cursorSprite = _manager.Content.Load<Texture2D>("Common/cursor");
            _cursorOffset = new Vector2(_cursorSprite.Width, _cursorSprite.Height) / 2f;

            viewport = _manager.GraphicsDevice.Viewport;
        }

        /// <summary>
        ///   Reads the latest state of the keyboard and gamepad and mouse/touchpad.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            LastKeyboardState = CurrentKeyboardState;
            LastGamePadState = CurrentGamePadState;
            LastMouseState = CurrentMouseState;

            CurrentKeyboardState = Keyboard.GetState();
            CurrentGamePadState = GamePad.GetState(PlayerIndex.One);
            CurrentMouseState = Mouse.GetState();

            Gestures.Clear();
            while (TouchPanel.IsGestureAvailable)
            {
                Gestures.Add(TouchPanel.ReadGesture());
            }

            // Update cursor
            Vector2 oldCursor = _cursor;
            if (CurrentGamePadState.IsConnected && CurrentGamePadState.ThumbSticks.Left != Vector2.Zero)
            {
                Vector2 temp = CurrentGamePadState.ThumbSticks.Left;
                _cursor += temp * new Vector2(300f, -300f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                Mouse.SetPosition((int)_cursor.X, (int)_cursor.Y);
            }
            else
            {
                _cursor.X = CurrentMouseState.X;
                _cursor.Y = CurrentMouseState.Y;
            }
            _cursor.X = MathHelper.Clamp(_cursor.X, 0f, (float)viewport.Width);
            _cursor.Y = MathHelper.Clamp(_cursor.Y, 0f, (float)viewport.Height);

            if (_cursorIsValid && oldCursor != _cursor)
            {
                _cursorMoved = true;
            }
            else
            {
                _cursorMoved = false;
            }

#if WINDOWS
            if (viewport.Bounds.Contains(CurrentMouseState.X, CurrentMouseState.Y))
            {
                _cursorIsValid = true;
            }
            else
            {
                _cursorIsValid = false;
            }
#endif
#if WINDOWS_PHONE
            if (CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                _cursorIsValid = true;
            }
            else
            {
                _cursorIsValid = false;
            }
#endif
        }

        public void Draw()
        {
            if (_cursorIsVisible && _cursorIsValid)
            {
                _manager.SpriteBatch.Begin();
                _manager.SpriteBatch.Draw(_cursorSprite, _cursor - _cursorOffset, Color.White);
                _manager.SpriteBatch.End();
            }
        }

        public GamePadState GamePadState
        {
            get { return CurrentGamePadState; }
        }

        public KeyboardState KeyboardState
        {
            get { return CurrentKeyboardState; }
        }

        public MouseState MouseState
        {
            get { return CurrentMouseState; }
        }

        public GamePadState PreviousGamePadState
        {
            get { return LastGamePadState; }
        }

        public KeyboardState PreviousKeyboardState
        {
            get { return LastKeyboardState; }
        }

        public MouseState PreviousMouseState
        {
            get { return LastMouseState; }
        }

        /// <summary>
        ///   Helper for checking if a key was newly pressed during this update.
        /// </summary>
        public bool IsNewKeyPress(Keys key)
        {
            return (CurrentKeyboardState.IsKeyDown(key) &&
                    LastKeyboardState.IsKeyUp(key));
        }

        public bool IsNewKeyRelease(Keys key)
        {
            return (LastKeyboardState.IsKeyDown(key) &&
                    CurrentKeyboardState.IsKeyUp(key));
        }

        /// <summary>
        ///   Helper for checking if a button was newly pressed during this update.
        /// </summary>
        public bool IsNewButtonPress(Buttons button)
        {
            return (CurrentGamePadState.IsButtonDown(button) &&
                    LastGamePadState.IsButtonUp(button));
        }

        public bool IsNewButtonRelease(Buttons button)
        {
            return (LastGamePadState.IsButtonDown(button) &&
                    CurrentGamePadState.IsButtonUp(button));
        }

        /// <summary>
        ///   Helper for checking if a mouse button was newly pressed during this update.
        /// </summary>
        public bool IsNewMouseButtonPress(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.LeftButton:
                    return (CurrentMouseState.LeftButton == ButtonState.Pressed &&
                            LastMouseState.LeftButton == ButtonState.Released);
                case MouseButtons.RightButton:
                    return (CurrentMouseState.RightButton == ButtonState.Pressed &&
                            LastMouseState.RightButton == ButtonState.Released);
                case MouseButtons.MiddleButton:
                    return (CurrentMouseState.MiddleButton == ButtonState.Pressed &&
                            LastMouseState.MiddleButton == ButtonState.Released);
                case MouseButtons.ExtraButton1:
                    return (CurrentMouseState.XButton1 == ButtonState.Pressed &&
                            LastMouseState.XButton1 == ButtonState.Released);
                case MouseButtons.ExtraButton2:
                    return (CurrentMouseState.XButton2 == ButtonState.Pressed &&
                            LastMouseState.XButton2 == ButtonState.Released);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if the requested mouse button is released.
        /// </returns>
        public bool IsNewMouseButtonRelease(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.LeftButton:
                    return (LastMouseState.LeftButton == ButtonState.Pressed &&
                            CurrentMouseState.LeftButton == ButtonState.Released);
                case MouseButtons.RightButton:
                    return (LastMouseState.RightButton == ButtonState.Pressed &&
                            CurrentMouseState.RightButton == ButtonState.Released);
                case MouseButtons.MiddleButton:
                    return (LastMouseState.MiddleButton == ButtonState.Pressed &&
                            CurrentMouseState.MiddleButton == ButtonState.Released);
                case MouseButtons.ExtraButton1:
                    return (LastMouseState.XButton1 == ButtonState.Pressed &&
                            CurrentMouseState.XButton1 == ButtonState.Released);
                case MouseButtons.ExtraButton2:
                    return (LastMouseState.XButton2 == ButtonState.Pressed &&
                            CurrentMouseState.XButton2 == ButtonState.Released);
                default:
                    return false;
            }
        }

        /// <summary>
        ///   Checks for a "menu select" input action.
        /// </summary>
        public bool IsMenuSelect()
        {
            return IsNewKeyPress(Keys.Space) ||
                   IsNewKeyPress(Keys.Enter) ||
                   IsNewButtonPress(Buttons.A) ||
                   IsNewButtonPress(Buttons.Start) ||
                   IsNewMouseButtonPress(MouseButtons.LeftButton);
        }

        public bool IsMenuPressed()
        {
            return CurrentKeyboardState.IsKeyDown(Keys.Space) ||
                   CurrentKeyboardState.IsKeyDown(Keys.Enter) ||
                   CurrentGamePadState.IsButtonDown(Buttons.A) ||
                   CurrentGamePadState.IsButtonDown(Buttons.Start) ||
                   CurrentMouseState.LeftButton == ButtonState.Pressed;
        }

        /// <summary>
        ///   Checks for a "menu cancel" input action.
        /// </summary>
        public bool IsMenuCancel()
        {
            return IsNewKeyPress(Keys.Escape) ||
                   IsNewButtonPress(Buttons.Back);
        }      

        public bool ShowCursor
        {
            get { return _cursorIsVisible && _cursorIsValid; }
            set { _cursorIsVisible = value; }
        }

        public Vector2 Cursor
        {
            get { return _cursor; }
        }

        public bool IsCursorMoved
        {
            get { return _cursorMoved; }
        }
    }
}