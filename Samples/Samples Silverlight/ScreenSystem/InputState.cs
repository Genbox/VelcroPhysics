using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace FarseerPhysics.ScreenSystem
{
    /// <summary>
    /// This class is used to keep track of Keyboard and Mouse states throughout the game
    /// </summary>
    public class InputState
    {
        private UserControl _userControl;

        public InputState()
        {
            LastKeyboardState = new KeyboardState();
            CurrentKeyboardState = new KeyboardState();
            LastMouseState = new MouseState();
            CurrentMouseState = new MouseState();
        }

        public MouseState LastMouseState { get; set; }
        public MouseState CurrentMouseState { get; set; }
        public KeyboardState LastKeyboardState { get; set; }
        public KeyboardState CurrentKeyboardState { get; set; }

        public void Attach(UserControl userControl)
        {
            CurrentKeyboardState.ClearKeyPresses();
            _userControl = userControl;
            userControl.MouseMove += target_MouseMove;
            userControl.MouseLeftButtonDown += target_MouseLeftButtonDown;
            userControl.MouseLeftButtonUp += target_MouseLeftButtonUp;
            userControl.KeyDown += target_KeyDown;
            userControl.KeyUp += target_KeyUp;
            userControl.LostFocus += target_LostFocus;
        }

        private void target_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LastMouseState.IsLeftButtonDown = CurrentMouseState.IsLeftButtonDown;
            CurrentMouseState.IsLeftButtonDown = false;
        }

        private void target_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LastMouseState.IsLeftButtonDown = CurrentMouseState.IsLeftButtonDown;
            CurrentMouseState.IsLeftButtonDown = true;
        }

        private void target_MouseMove(object sender, MouseEventArgs e)
        {
            LastMouseState.X = CurrentMouseState.X;
            LastMouseState.Y = CurrentMouseState.Y;
            CurrentMouseState.X = e.StylusDevice.GetStylusPoints(_userControl)[0].X;
            CurrentMouseState.Y = e.StylusDevice.GetStylusPoints(_userControl)[0].Y;
        }

        public void Detach(UserControl target)
        {
            target.KeyDown -= target_KeyDown;
            target.KeyUp -= target_KeyUp;
            target.LostFocus -= target_LostFocus;
            CurrentKeyboardState.ClearKeyPresses();
        }

        void target_KeyDown(object sender, KeyEventArgs e)
        {
            LastKeyboardState.Leave(e.Key);
            CurrentKeyboardState.Press(e.Key);
        }

        void target_KeyUp(object sender, KeyEventArgs e)
        {
            LastKeyboardState.Press(e.Key);
            CurrentKeyboardState.Leave(e.Key);
        }

        void target_LostFocus(object sender, EventArgs e)
        {
            CurrentKeyboardState.ClearKeyPresses();
        }

        public bool IsKeyPressed(Key k)
        {
            return CurrentKeyboardState.IsKeyDown(k);
        }

        public void Update(GameTime gameTime)
        {
            LastKeyboardState = new KeyboardState(CurrentKeyboardState);
            LastMouseState = new MouseState(CurrentMouseState);
        }
    }
}
