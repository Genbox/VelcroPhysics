using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Testbed.Framework
{
    public class KeyboardManager
    {
        internal KeyboardState _newKeyboardState;
        internal KeyboardState _oldKeyboardState;

        public bool IsNewKeyPress(Keys key)
        {
            return _newKeyboardState.IsKeyDown(key) && _oldKeyboardState.IsKeyUp(key);
        }

        public bool IsKeyDown(Keys key)
        {
            return _newKeyboardState.IsKeyDown(key);
        }

        internal bool IsKeyUp(Keys key)
        {
            return _newKeyboardState.IsKeyUp(key);
        }
    }
}