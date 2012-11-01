namespace FarseerPhysics.ScreenSystem
{
    /// <summary>
    /// This class represent a mouse state to be used by game components
    /// </summary>
    public class MouseState
    {
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

        public double X { get; set; }
        public double Y { get; set; }
        public bool IsLeftButtonDown { get; set; }
    }
}
