using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework
{
    public class MouseManager
    {
        internal MouseState _oldState;
        internal MouseState _newState;

        public int NewScrollValue => _newState.ScrollWheelValue - _oldState.ScrollWheelValue;

        public void Update()
        {
            _oldState = _newState;
            _newState = Mouse.GetState();
        }
    }
}