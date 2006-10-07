using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerXNAGame.Input {
    public interface IKeyboardInputService {
        void AddKeyAction(KeyDownAction keyDownAction);

        void AddKeyAction(KeyUpAction keyUpAction);

        void AddKeyAction(KeyPressedAction keyPressedAction);
            
        void RemoveKeyAction(KeyDownAction keyDownAction);

        void RemoveKeyAction(KeyUpAction keyUpAction);

        void RemoveKeyAction(KeyPressedAction keyPressedAction);

        void ClearKeyActions();
    }
}
