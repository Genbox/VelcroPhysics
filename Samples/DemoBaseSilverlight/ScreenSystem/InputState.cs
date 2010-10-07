using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FarseerPhysics.DemoBaseSilverlight.ScreenSystem
{
    /// <summary>
    /// This class is used to keep track of Keyboard and Mouse states throughout the game
    /// </summary>
    public class InputState
    {
        public MouseState LastMouseState { get; set; }
        public MouseState CurrentMouseState { get; set; }
        public KeyboardState LastKeyboardState { get; set; }
        public KeyboardState CurrentKeyboardState { get; set; }

        private UserControl _userControl = null;

        public InputState()
        {
            LastKeyboardState = new KeyboardState();
            CurrentKeyboardState = new KeyboardState();
            LastMouseState = new MouseState();
            CurrentMouseState = new MouseState();
        }

        public void Attach(UserControl userControl)
        {
            CurrentKeyboardState.ClearKeyPresses();
            _userControl = userControl;
            userControl.MouseMove += new MouseEventHandler(target_MouseMove);
            userControl.MouseLeftButtonDown += new MouseButtonEventHandler(target_MouseLeftButtonDown);
            userControl.MouseLeftButtonUp += new MouseButtonEventHandler(target_MouseLeftButtonUp);
            userControl.KeyDown += new KeyEventHandler(target_KeyDown);
            userControl.KeyUp += new KeyEventHandler(target_KeyUp);
            userControl.LostFocus += new RoutedEventHandler(target_LostFocus);
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
            target.KeyDown -= new KeyEventHandler(target_KeyDown);
            target.KeyUp -= new KeyEventHandler(target_KeyUp);
            target.LostFocus -= new RoutedEventHandler(target_LostFocus);
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
