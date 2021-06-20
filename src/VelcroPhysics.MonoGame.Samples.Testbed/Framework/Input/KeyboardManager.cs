using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input
{
    public class KeyboardManager
    {
        internal KeyboardState _oldState;
        internal KeyboardState _newState;

        public KeyboardManager()
        {
            _oldState = _newState = Keyboard.GetState();
        }

        public void Update()
        {
            _oldState = _newState;
            _newState = Keyboard.GetState();
        }

        public bool IsNewKeyPress(Keys key)
        {
            return _newState.IsKeyDown(key) && _oldState.IsKeyUp(key);
        }

        public bool IsKeyDown(Keys key)
        {
            return _newState.IsKeyDown(key);
        }

        internal bool IsKeyUp(Keys key)
        {
            return _newState.IsKeyUp(key);
        }
    }
}