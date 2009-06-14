using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace FarseerGames.SimpleSamplesSilverlight
{
    public class KeyHandler
    {
        private bool[] _isPressed = new bool[255];

        public void ClearKeyPresses()
        {
            for (int i = 0; i < 255; i++)
            {
                _isPressed[i] = false;
            }
        }

        public void Attach(UserControl target)
        {
            ClearKeyPresses();
            target.KeyDown += target_KeyDown;
            target.KeyUp += target_KeyUp;
            target.LostFocus += target_LostFocus;
        }

        public void Detach(UserControl target)
        {
            target.KeyDown -= target_KeyDown;
            target.KeyUp -= target_KeyUp;
            target.LostFocus -= target_LostFocus;
            ClearKeyPresses();
        }

        private void target_KeyDown(object sender, KeyEventArgs e)
        {
            _isPressed[(int) e.Key] = true;
        }

        private void target_KeyUp(object sender, KeyEventArgs e)
        {
            _isPressed[(int) e.Key] = false;
        }

        private void target_LostFocus(object sender, EventArgs e)
        {
            ClearKeyPresses();
        }

        public bool IsKeyPressed(Key k)
        {
            int v = (int) k;
            if (v < 0 || v > 82) return false;
            return _isPressed[v];
        }
    }
}