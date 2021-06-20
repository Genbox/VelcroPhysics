using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework
{
    public class MouseManager
    {
        internal MouseState _oldState;
        internal MouseState _newState;

        public int DeltaScrollValue => _newState.ScrollWheelValue - _oldState.ScrollWheelValue;

        public Vector2 NewPosition => new Vector2(_newState.X, _newState.Y);
        public Vector2 OldPosition => new Vector2(_oldState.X, _oldState.Y);

        public void Update()
        {
            _oldState = _newState;
            _newState = Mouse.GetState();
        }
    }
}