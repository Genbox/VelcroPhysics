using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace FarseerSilverlightDemos
{
    public class KeyHandler
    {
        private bool[] isPressed = new bool[255];
        private UserControl targetCanvas;

        public void ClearKeyPresses()
        {
            for (int i = 0; i < 255; i++)
            {
                isPressed[i] = false;
            }
        }

        public void Attach(UserControl target)
        {
            ClearKeyPresses();
            targetCanvas = target;
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
            isPressed[(int) e.Key] = true;
        }

        private void target_KeyUp(object sender, KeyEventArgs e)
        {
            isPressed[(int) e.Key] = false;
        }

        private void target_LostFocus(object sender, EventArgs e)
        {
            ClearKeyPresses();
        }

        public bool IsKeyPressed(Key k)
        {
            int v = (int) k;
            if (v < 0 || v > 82) return false;
            return isPressed[v];
        }
    }
}