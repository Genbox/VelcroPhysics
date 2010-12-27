using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace FarseerPhysics.DemoBaseXNA.ScreenSystem
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
#if WINDOWS_PHONE
        public const int MaxInputs = 1;
#else
        public const int MaxInputs = 4;
#endif

        public readonly KeyboardState[] CurrentKeyboardStates;
        public readonly GamePadState[] CurrentGamePadStates;
        public MouseState CurrentMouseState;

        public readonly KeyboardState[] LastKeyboardStates;
        public readonly GamePadState[] LastGamePadStates;
        public MouseState LastMouseState;

        public readonly bool[] GamePadWasConnected;

        public TouchCollection TouchState;

        public readonly List<GestureSample> Gestures = new List<GestureSample>();

        private PlayerIndex _tmpIndex;

        /// <summary>
        ///   Constructs a new input state.
        /// </summary>
        public InputHelper()
        {
            CurrentKeyboardStates = new KeyboardState[MaxInputs];
            CurrentGamePadStates = new GamePadState[MaxInputs];

            LastKeyboardStates = new KeyboardState[MaxInputs];
            LastGamePadStates = new GamePadState[MaxInputs];

            GamePadWasConnected = new bool[MaxInputs];
        }

        /// <summary>
        ///   Reads the latest state of the keyboard and gamepad.
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                LastKeyboardStates[i] = CurrentKeyboardStates[i];
                LastGamePadStates[i] = CurrentGamePadStates[i];

                CurrentKeyboardStates[i] = Keyboard.GetState((PlayerIndex) i);
                CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex) i);


                // Keep track of whether a gamepad has ever been
                // connected, so we can detect if it is unplugged.
                if (CurrentGamePadStates[i].IsConnected)
                {
                    GamePadWasConnected[i] = true;
                }
            }
#if !XBOX
            LastMouseState = CurrentMouseState;
#endif
#if WINDOWS_PHONE
            TouchState = TouchPanel.GetState();
            if (TouchState.Count > 0)
            {
                TouchLocation location = TouchState[0];
                CurrentMouseState = new MouseState((int)location.Position.X, (int)location.Position.Y, 0, ButtonState.Pressed, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
            }
            else
            {
                CurrentMouseState = new MouseState(0, 0, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
            }
#elif !XBOX
            CurrentMouseState = Mouse.GetState();
#endif

            Gestures.Clear();
            while (TouchPanel.IsGestureAvailable)
            {
                Gestures.Add(TouchPanel.ReadGesture());
            }
        }

        public PlayerIndex DefaultPlayerIndex = PlayerIndex.One;

        public GamePadState CurrentGamePadState
        {
            get { return CurrentGamePadStates[(int) DefaultPlayerIndex]; }
        }

        public KeyboardState CurrentKeyboardState
        {
            get { return CurrentKeyboardStates[(int) DefaultPlayerIndex]; }
        }

        public GamePadState LastGamePadState
        {
            get { return LastGamePadStates[(int)DefaultPlayerIndex]; }
        }

        public KeyboardState LastKeyboardState
        {
            get { return LastKeyboardStates[(int)DefaultPlayerIndex]; }
        }

        /// <summary>
        ///   Helper for checking if a key was newly pressed during this update. The
        ///   controllingPlayer parameter specifies which player to read input for.
        ///   If this is null, it will accept input from any player. When a keypress
        ///   is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer,
                                  out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int) playerIndex;

                return (CurrentKeyboardStates[i].IsKeyDown(key) &&
                        LastKeyboardStates[i].IsKeyUp(key));
            }
            else
            {
#if WINDOWS_PHONE
                return IsNewKeyPress(key, PlayerIndex.One, out playerIndex);
#else
                // Accept input from any player.
                return (IsNewKeyPress(key, PlayerIndex.One, out playerIndex) ||
                        IsNewKeyPress(key, PlayerIndex.Two, out playerIndex) ||
                        IsNewKeyPress(key, PlayerIndex.Three, out playerIndex) ||
                        IsNewKeyPress(key, PlayerIndex.Four, out playerIndex));
#endif
            }
        }

        public bool IsNewKeyPress(Keys key)
        {
            return IsNewKeyPress(key, null, out _tmpIndex);
        }

        /// <summary>
        ///   Helper for checking if a button was newly pressed during this update.
        ///   The controllingPlayer parameter specifies which player to read input for.
        ///   If this is null, it will accept input from any player. When a button press
        ///   is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer,
                                     out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int) playerIndex;

                return (CurrentGamePadStates[i].IsButtonDown(button) &&
                        LastGamePadStates[i].IsButtonUp(button));
            }
            else
            {
#if WINDOWS_PHONE
                return IsNewButtonPress(button, PlayerIndex.One, out playerIndex);
#else
                // Accept input from any player.
                return (IsNewButtonPress(button, PlayerIndex.One, out playerIndex) ||
                        IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) ||
                        IsNewButtonPress(button, PlayerIndex.Three, out playerIndex) ||
                        IsNewButtonPress(button, PlayerIndex.Four, out playerIndex));
#endif
            }
        }

        public bool IsNewButtonPress(Buttons button)
        {
            return IsNewButtonPress(button, null, out _tmpIndex);
        }

        /// <summary>
        ///   Checks for a "menu select" input action.
        ///   The controllingPlayer parameter specifies which player to read input for.
        ///   If this is null, it will accept input from any player. When the action
        ///   is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsMenuSelect(PlayerIndex? controllingPlayer,
                                 out PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Space, controllingPlayer, out playerIndex) ||
                   IsNewKeyPress(Keys.Enter, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.A, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
        }


        /// <summary>
        ///   Checks for a "menu cancel" input action.
        ///   The controllingPlayer parameter specifies which player to read input for.
        ///   If this is null, it will accept input from any player. When the action
        ///   is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsMenuCancel(PlayerIndex? controllingPlayer,
                                 out PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.B, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex);
        }


        /// <summary>
        ///   Checks for a "menu up" input action.
        ///   The controllingPlayer parameter specifies which player to read
        ///   input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuUp(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Up, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.DPadUp, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.LeftThumbstickUp, controllingPlayer, out playerIndex);
        }


        /// <summary>
        ///   Checks for a "menu down" input action.
        ///   The controllingPlayer parameter specifies which player to read
        ///   input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuDown(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Down, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.DPadDown, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.LeftThumbstickDown, controllingPlayer, out playerIndex);
        }


        /// <summary>
        ///   Checks for a "pause the game" input action.
        ///   The controllingPlayer parameter specifies which player to read
        ///   input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsPauseGame(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
        }

#if (!XBOX)
        /// <summary>
        ///   The current mouse position.
        /// </summary>
        public Vector2 MousePosition
        {
            get { return new Vector2(CurrentMouseState.X, CurrentMouseState.Y); }
        }

        /// <summary>
        ///   The current mouse velocity.
        ///   Expressed as: 
        ///   current mouse position - last mouse position.
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
        ///   The current mouse scroll wheel position.
        ///   See the Mouse's ScrollWheel property for details.
        /// </summary>
        public float MouseScrollWheelPosition
        {
            get { return CurrentMouseState.ScrollWheelValue; }
        }

        /// <summary>
        ///   The mouse scroll wheel velocity.
        ///   
        ///   Expressed as:
        ///   current scroll wheel position - the last scroll wheel position.
        /// </summary>
        public float MouseScrollWheelVelocity
        {
            get { return (CurrentMouseState.ScrollWheelValue - LastMouseState.ScrollWheelValue); }
        }
#endif

#if (!XBOX)
        /// <summary>
        ///   Checks if the requested mosue button is a new press.
        /// </summary>
        /// <param name = "button">
        ///   The mouse button to check.
        /// </param>
        /// <returns>
        ///   A bool indicating whether the selected mouse button is being
        ///   pressed in the current state but not in the last state.
        /// </returns>
        public bool IsNewButtonPress(MouseButtons button)
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
        /// Checks if the requested mosue button is an old press.
        /// </summary>
        /// <param name="button">
        /// The mouse button to check.
        /// </param>
        /// <returns>
        /// A bool indicating whether the selected mouse button is not being 
        /// pressed in the current state and is being pressed in the old state.
        /// </returns>
        public bool IsOldButtonPress(MouseButtons button)
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

        public bool IsKeyDown(Keys key, PlayerIndex? controllingPlayer,
                              out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int) playerIndex;

                return (CurrentKeyboardStates[i].IsKeyDown(key));
            }
            else
            {
#if WINDOWS_PHONE
                return IsKeyDown(key, PlayerIndex.One, out playerIndex);
#else
                // Accept input from any player.
                return (IsKeyDown(key, PlayerIndex.One, out playerIndex) ||
                        IsKeyDown(key, PlayerIndex.Two, out playerIndex) ||
                        IsKeyDown(key, PlayerIndex.Three, out playerIndex) ||
                        IsKeyDown(key, PlayerIndex.Four, out playerIndex));
#endif
            }
        }

        public bool IsKeyDown(Keys key)
        {
            return IsKeyDown(key, null, out _tmpIndex);
        }

        public bool IsKeyUp(Keys key, PlayerIndex? controllingPlayer,
                            out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int) playerIndex;

                return (CurrentKeyboardStates[i].IsKeyUp(key));
            }
            else
            {
#if WINDOWS_PHONE
                return IsKeyUp(key, PlayerIndex.One, out playerIndex);
#else
                // Accept input from any player.
                return (IsKeyUp(key, PlayerIndex.One, out playerIndex) ||
                        IsKeyUp(key, PlayerIndex.Two, out playerIndex) ||
                        IsKeyUp(key, PlayerIndex.Three, out playerIndex) ||
                        IsKeyUp(key, PlayerIndex.Four, out playerIndex));
#endif
            }
        }

        public bool IsKeyUp(Keys key)
        {
            return IsKeyUp(key, null, out _tmpIndex);
        }
    }
}