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
    /// This class represent a mouse state to be used by game components
    /// </summary>
    public class MouseState
    {
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsLeftButtonDown { get; set; }

        public MouseState()
        {
            X = 0;
            Y = 0;
            IsLeftButtonDown = false;
        }

        public MouseState(MouseState mouseState)
        {
            X = mouseState.X;
            Y = mouseState.Y;
            IsLeftButtonDown = mouseState.IsLeftButtonDown;
        }
    }
}
