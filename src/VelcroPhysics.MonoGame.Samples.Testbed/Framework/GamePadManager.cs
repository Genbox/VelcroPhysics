using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework
{
    public class GamePadManager
    {
        internal GamePadState _oldState;
        internal GamePadState _newState;

        public GamePadManager()
        {
            _oldState = _newState = GamePad.GetState(PlayerIndex.One);
        }

        public bool IsConnected => _newState.IsConnected;

        public bool IsNewButtonPress(Buttons button)
        {
            return _newState.IsButtonDown(button) && _oldState.IsButtonUp(button);
        }

        public void Update()
        {
            _oldState = _newState;
            _newState = GamePad.GetState(PlayerIndex.One);
        }
    }
}