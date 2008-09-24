using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FarseerSilverlightDemos
{
    public class KeyHandler
    {
        bool[] isPressed = new bool[255];
        UserControl targetCanvas = null;
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
            target.KeyDown += new KeyEventHandler(target_KeyDown);
            target.KeyUp += new KeyEventHandler(target_KeyUp);
            target.LostFocus += new RoutedEventHandler(target_LostFocus);
        }

        public void Detach(UserControl target)
        {
            target.KeyDown -= new KeyEventHandler(target_KeyDown);
            target.KeyUp -= new KeyEventHandler(target_KeyUp);
            target.LostFocus -= new RoutedEventHandler(target_LostFocus);
            ClearKeyPresses();
        }

        void target_KeyDown(object sender, KeyEventArgs e)
        {
            isPressed[(int)e.Key] = true;
        }

        void target_KeyUp(object sender, KeyEventArgs e)
        {
            isPressed[(int)e.Key] = false;
        }
            
        void target_LostFocus(object sender, EventArgs e)
        {
            ClearKeyPresses();            
        }

        public bool IsKeyPressed(Key k)
        {
            int v = (int)k;
            if (v < 0 || v > 82) return false;
            return isPressed[v];
        }
    }
}
