using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using FarseerGames.FarseerXNAGame.Input;


namespace FarseerGames.FarseerXNATestharness.Ships {
    public class ShipKeyboardEntityController : KeyboardEntityController<ShipEntity> {
        private Keys thrustKey = Keys.W;
        private Keys turnLeftKey = Keys.A;
        private Keys turnRightKey = Keys.D;

        public ShipKeyboardEntityController(ShipEntity shipEntity, IKeyboardInputService keyboardInputService)
            : base(shipEntity, keyboardInputService) {

            keyboardInputService.AddKeyAction(new KeyPressedAction(thrustKey, Thrust));
            keyboardInputService.AddKeyAction(new KeyPressedAction(turnLeftKey,TurnLeft));
            keyboardInputService.AddKeyAction(new KeyPressedAction(turnRightKey,TurnRight));
        }

        private void Thrust() {entity.Thrust();}

        private void TurnLeft() { entity.TurnLeft(); }

        private void TurnRight() { entity.TurnRight(); }
    }
}
