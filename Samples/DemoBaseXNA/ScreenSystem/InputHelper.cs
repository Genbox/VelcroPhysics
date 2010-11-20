using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.DemoBaseXNA.ScreenSystem
{
    /// <summary>
    /// an enum of all available mouse buttons.
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
        private PlayerIndex _index = PlayerIndex.One;
        private bool _refreshData;

        /// <summary>
        /// Fetches the latest input states.
        /// </summary>
        public void Update()
        {
            if (!_refreshData)
                _refreshData = true;

            LastGamepadState = CurrentGamepadState;
            CurrentGamepadState = GamePad.GetState(_index);
#if (!XBOX)
            LastKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();

            LastMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();
#endif
        }

        /// <summary>
        /// The previous state of the gamepad. 
        /// Exposed only for convenience.
        /// </summary>
        public GamePadState LastGamepadState { get; private set; }

        /// <summary>
        /// the current state of the gamepad.
        /// Exposed only for convenience.
        /// </summary>
        public GamePadState CurrentGamepadState { get; private set; }

        /// <summary>
        /// the index that is used to poll the gamepad. 
        /// </summary>
        public PlayerIndex Index
        {
            get { return _index; }
            set
            {
                _index = value;
                if (_refreshData)
                {
                    Update();
                    Update();
                }
            }
        }

#if (!XBOX)
        /// <summary>
        /// The previous keyboard state.
        /// Exposed only for convenience.
        /// </summary>
        public KeyboardState LastKeyboardState { get; private set; }

        /// <summary>
        /// The current state of the keyboard.
        /// Exposed only for convenience.
        /// </summary>
        public KeyboardState CurrentKeyboardState { get; private set; }

        /// <summary>
        /// The previous mouse state.
        /// Exposed only for convenience.
        /// </summary>
        public MouseState LastMouseState { get; private set; }

        /// <summary>
        /// The current state of the mouse.
        /// Exposed only for convenience.
        /// </summary>
        public MouseState CurrentMouseState { get; private set; }
#endif

        /// <summary>
        /// The current position of the left stick. 
        /// Y is automatically reversed for you.
        /// </summary>
        public Vector2 LeftStickPosition
        {
            get
            {
                return new Vector2(
                    CurrentGamepadState.ThumbSticks.Left.X,
                    -CurrentGamepadState.ThumbSticks.Left.Y);
            }
        }

        /// <summary>
        /// The current position of the right stick.
        /// Y is automatically reversed for you.
        /// </summary>
        public Vector2 RightStickPosition
        {
            get
            {
                return new Vector2(
                    CurrentGamepadState.ThumbSticks.Right.X,
                    -CurrentGamepadState.ThumbSticks.Right.Y);
            }
        }

        /// <summary>
        /// The current velocity of the left stick.
        /// Y is automatically reversed for you.
        /// expressed as: 
        /// current stick position - last stick position.
        /// </summary>
        public Vector2 LeftStickVelocity
        {
            get
            {
                Vector2 temp =
                    CurrentGamepadState.ThumbSticks.Left -
                    LastGamepadState.ThumbSticks.Left;
                return new Vector2(temp.X, -temp.Y);
            }
        }

        /// <summary>
        /// The current velocity of the right stick.
        /// Y is automatically reversed for you.
        /// expressed as: 
        /// current stick position - last stick position.
        /// </summary>
        public Vector2 RightStickVelocity
        {
            get
            {
                Vector2 temp =
                    CurrentGamepadState.ThumbSticks.Right -
                    LastGamepadState.ThumbSticks.Right;
                return new Vector2(temp.X, -temp.Y);
            }
        }

        /// <summary>
        /// the current position of the left trigger.
        /// </summary>
        public float LeftTriggerPosition
        {
            get { return CurrentGamepadState.Triggers.Left; }
        }

        /// <summary>
        /// the current position of the right trigger.
        /// </summary>
        public float RightTriggerPosition
        {
            get { return CurrentGamepadState.Triggers.Right; }
        }

        /// <summary>
        /// the velocity of the left trigger.
        /// expressed as: 
        /// current trigger position - last trigger position.
        /// </summary>
        public float LeftTriggerVelocity
        {
            get
            {
                return
                    CurrentGamepadState.Triggers.Left -
                    LastGamepadState.Triggers.Left;
            }
        }

        /// <summary>
        /// the velocity of the right trigger.
        /// expressed as: 
        /// current trigger position - last trigger position.
        /// </summary>
        public float RightTriggerVelocity
        {
            get
            {
                return CurrentGamepadState.Triggers.Right -
                       LastGamepadState.Triggers.Right;
            }
        }

#if (!XBOX)
        /// <summary>
        /// the current mouse position.
        /// </summary>
        public Vector2 MousePosition
        {
            get { return new Vector2(CurrentMouseState.X, CurrentMouseState.Y); }
        }

        /// <summary>
        /// the current mouse velocity.
        /// Expressed as: 
        /// current mouse position - last mouse position.
        /// </summary>
        public Vector2 MouseVelocity
        {
            get
            {
                return (
                           new Vector2(CurrentMouseState.X, CurrentMouseState.Y) -
                           new Vector2(LastMouseState.X, LastMouseState.Y)
                       );
            }
        }

        /// <summary>
        /// the current mouse scroll wheel position.
        /// See the Mouse's ScrollWheel property for details.
        /// </summary>
        public float MouseScrollWheelPosition
        {
            get { return CurrentMouseState.ScrollWheelValue; }
        }

        /// <summary>
        /// the mouse scroll wheel velocity.
        /// Expressed as:
        /// current scroll wheel position - 
        /// the last scroll wheel position.
        /// </summary>
        public float MouseScrollWheelVelocity
        {
            get { return (CurrentMouseState.ScrollWheelValue - LastMouseState.ScrollWheelValue); }
        }
#endif

        /// <summary>
        /// Checks if the requested button is a new press.
        /// </summary>
        /// <param name="button">
        /// The button to check.
        /// </param>
        /// <returns>
        /// a bool indicating whether the selected button is being 
        /// pressed in the current state but not the last state.
        /// </returns>
        public bool IsNewPress(Buttons button)
        {
            return (
                       LastGamepadState.IsButtonUp(button) &&
                       CurrentGamepadState.IsButtonDown(button));
        }

        /// <summary>
        /// Checks if the requested button is a current press.
        /// </summary>
        /// <param name="button">
        /// the button to check.
        /// </param>
        /// <returns>
        /// a bool indicating whether the selected button is being 
        /// pressed in the current state and in the last state.
        /// </returns>
        public bool IsCurPress(Buttons button)
        {
            return (
                       LastGamepadState.IsButtonDown(button) &&
                       CurrentGamepadState.IsButtonDown(button));
        }

        /// <summary>
        /// Checks if the requested button is an old press.
        /// </summary>
        /// <param name="button">
        /// the button to check.
        /// </param>
        /// <returns>
        /// a bool indicating whether the selected button is not being
        /// pressed in the current state and is being pressed in the last state.
        /// </returns>
        public bool IsOldPress(Buttons button)
        {
            return (
                       LastGamepadState.IsButtonDown(button) &&
                       CurrentGamepadState.IsButtonUp(button));
        }

        /// <summary>
        /// Checks for a "menu up" input action (on either keyboard or gamepad).
        /// </summary>
        public bool MenuUp
        {
            get
            {
                return IsNewPress(Keys.Up) ||
                       (CurrentGamepadState.DPad.Up == ButtonState.Pressed &&
                        LastGamepadState.DPad.Up == ButtonState.Released) ||
                       (CurrentGamepadState.ThumbSticks.Left.Y > 0 &&
                        LastGamepadState.ThumbSticks.Left.Y <= 0);
            }
        }

        /// <summary>
        /// Checks for a "menu down" input action (on either keyboard or gamepad).
        /// </summary>
        public bool MenuDown
        {
            get
            {
                return IsNewPress(Keys.Down) ||
                       (CurrentGamepadState.DPad.Down == ButtonState.Pressed &&
                        LastGamepadState.DPad.Down == ButtonState.Released) ||
                       (CurrentGamepadState.ThumbSticks.Left.Y < 0 &&
                        LastGamepadState.ThumbSticks.Left.Y >= 0);
            }
        }

        /// <summary>
        /// Checks for a "menu select" input action (on either keyboard or gamepad).
        /// </summary>
        public bool MenuSelect
        {
            get
            {
                return IsNewPress(Keys.Space) || IsNewPress(Keys.Enter) || IsNewPress(Buttons.A) ||
                       IsNewPress(Buttons.Start);
            }
        }

        /// <summary>
        /// Checks for a "menu cancel" input action (on either keyboard or gamepad).
        /// </summary>
        public bool MenuCancel
        {
            get { return IsNewPress(Keys.Escape) || IsNewPress(Buttons.B) || IsNewPress(Buttons.Back); }
        }

        /// <summary>
        /// Checks for a "pause the game" input action (on either keyboard or gamepad).
        /// </summary>
        public bool PauseGame
        {
            get { return IsNewPress(Keys.Escape) || IsNewPress(Buttons.Back) || IsNewPress(Buttons.Start); }
        }

#if (!XBOX)
        /// <summary>
        /// Checks if the requested key is a new press.
        /// </summary>
        /// <param name="key">
        /// the key to check.
        /// </param>
        /// <returns>
        /// a bool that indicates whether the selected key is being 
        /// pressed in the current state and not in the last state.
        /// </returns>
        public bool IsNewPress(Keys key)
        {
            return (
                       LastKeyboardState.IsKeyUp(key) &&
                       CurrentKeyboardState.IsKeyDown(key));
        }

        /// <summary>
        /// Checks if the requested key is a current press.
        /// </summary>
        /// <param name="key">
        /// the key to check.
        /// </param>
        /// <returns>
        /// a bool that indicates whether the selected key is being 
        /// pressed in the current state and in the last state.
        /// </returns>
        public bool IsCurPress(Keys key)
        {
            return (
                       LastKeyboardState.IsKeyDown(key) &&
                       CurrentKeyboardState.IsKeyDown(key));
        }

        /// <summary>
        /// Checks if the requested button is an old press.
        /// </summary>
        /// <param name="key">
        /// the key to check.
        /// </param>
        /// <returns>
        /// a bool indicating whether the selectde button is not being
        /// pressed in the current state and being pressed in the last state.
        /// </returns>
        public bool IsOldPress(Keys key)
        {
            return (
                       LastKeyboardState.IsKeyDown(key) &&
                       CurrentKeyboardState.IsKeyUp(key));
        }

        /// <summary>
        /// Checks if the requested mosue button is a new press.
        /// </summary>
        /// <param name="button">
        /// teh mouse button to check.
        /// </param>
        /// <returns>
        /// a bool indicating whether the selected mouse button is being
        /// pressed in the current state but not in the last state.
        /// </returns>
        public bool IsNewPress(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.LeftButton:
                    return (
                               LastMouseState.LeftButton == ButtonState.Released &&
                               CurrentMouseState.LeftButton == ButtonState.Pressed);
                case MouseButtons.MiddleButton:
                    return (
                               LastMouseState.MiddleButton == ButtonState.Released &&
                               CurrentMouseState.MiddleButton == ButtonState.Pressed);
                case MouseButtons.RightButton:
                    return (
                               LastMouseState.RightButton == ButtonState.Released &&
                               CurrentMouseState.RightButton == ButtonState.Pressed);
                case MouseButtons.ExtraButton1:
                    return (
                               LastMouseState.XButton1 == ButtonState.Released &&
                               CurrentMouseState.XButton1 == ButtonState.Pressed);
                case MouseButtons.ExtraButton2:
                    return (
                               LastMouseState.XButton2 == ButtonState.Released &&
                               CurrentMouseState.XButton2 == ButtonState.Pressed);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if the requested mosue button is a current press.
        /// </summary>
        /// <param name="button">
        /// the mouse button to be checked.
        /// </param>
        /// <returns>
        /// a bool indicating whether the selected mouse button is being 
        /// pressed in the current state and in the last state.
        /// </returns>
        public bool IsCurPress(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.LeftButton:
                    return (
                               LastMouseState.LeftButton == ButtonState.Pressed &&
                               CurrentMouseState.LeftButton == ButtonState.Pressed);
                case MouseButtons.MiddleButton:
                    return (
                               LastMouseState.MiddleButton == ButtonState.Pressed &&
                               CurrentMouseState.MiddleButton == ButtonState.Pressed);
                case MouseButtons.RightButton:
                    return (
                               LastMouseState.RightButton == ButtonState.Pressed &&
                               CurrentMouseState.RightButton == ButtonState.Pressed);
                case MouseButtons.ExtraButton1:
                    return (
                               LastMouseState.XButton1 == ButtonState.Pressed &&
                               CurrentMouseState.XButton1 == ButtonState.Pressed);
                case MouseButtons.ExtraButton2:
                    return (
                               LastMouseState.XButton2 == ButtonState.Pressed &&
                               CurrentMouseState.XButton2 == ButtonState.Pressed);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if the requested mosue button is an old press.
        /// </summary>
        /// <param name="button">
        /// the mouse button to check.
        /// </param>
        /// <returns>
        /// a bool indicating whether the selected mouse button is not being 
        /// pressed in the current state and is being pressed in the old state.
        /// </returns>
        public bool IsOldPress(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.LeftButton:
                    return (
                               LastMouseState.LeftButton == ButtonState.Pressed &&
                               CurrentMouseState.LeftButton == ButtonState.Released);
                case MouseButtons.MiddleButton:
                    return (
                               LastMouseState.MiddleButton == ButtonState.Pressed &&
                               CurrentMouseState.MiddleButton == ButtonState.Released);
                case MouseButtons.RightButton:
                    return (
                               LastMouseState.RightButton == ButtonState.Pressed &&
                               CurrentMouseState.RightButton == ButtonState.Released);
                case MouseButtons.ExtraButton1:
                    return (
                               LastMouseState.XButton1 == ButtonState.Pressed &&
                               CurrentMouseState.XButton1 == ButtonState.Released);
                case MouseButtons.ExtraButton2:
                    return (
                               LastMouseState.XButton2 == ButtonState.Pressed &&
                               CurrentMouseState.XButton2 == ButtonState.Released);
                default:
                    return false;
            }
        }
#endif
    }
}