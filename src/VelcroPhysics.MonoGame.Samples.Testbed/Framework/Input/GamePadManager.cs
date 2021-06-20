using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input
{
    public class GamePadManager
    {
        public GamePadState OldState;
        public GamePadState NewState;

        public GamePadManager()
        {
            OldState = NewState = GamePad.GetState(PlayerIndex.One);
        }

        public bool IsConnected => NewState.IsConnected;

        public bool IsNewButtonPress(Buttons button)
        {
            return NewState.IsButtonDown(button) && OldState.IsButtonUp(button);
        }

        public void Update()
        {
            OldState = NewState;
            NewState = GamePad.GetState(PlayerIndex.One);
        }
    }
}