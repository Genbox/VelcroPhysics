using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Input;

namespace FarseerGames.FarseerXNAGame.Input {
    public class KeyDownAction : KeyAction {
        public KeyDownAction(Keys key, KeyActionHandler keyActionHandler)
            : base(key, keyActionHandler) {
        }
    }
}
