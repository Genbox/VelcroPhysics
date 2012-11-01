using System.Windows.Input;

namespace FarseerPhysics.ScreenSystem
{
    /// <summary>
    /// This class is used to keep track of keyboard state accross game components
    /// </summary>
    public class KeyboardState
    {
        private bool[] isPressed = new bool[255];

        public KeyboardState()
        {
        }

        public KeyboardState(KeyboardState state)
        {
            isPressed = (bool[])state.isPressed.Clone();
        }

        public bool IsKeyDown(Key k)
        {
            int v = (int)k;
            if (v < 0 || v > 82) return false;
            return isPressed[v];
        }

        public void ClearKeyPresses()
        {
            for (int i = 0; i < 255; i++)
            {
                isPressed[i] = false;
            }
        }

        public void Press(Key k)
        {
            if (k != Key.Unknown)
                isPressed[(int)k] = true;
        }

        public void Leave(Key k)
        {
            if (k != Key.Unknown)
                isPressed[(int)k] = false;
        }
    }
}
