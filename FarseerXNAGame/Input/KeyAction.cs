using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.FarseerXNAGame.Input {
    public class KeyAction {
        public delegate void KeyActionHandler();

        private KeyActionHandler keyActionHandler;
        private Keys key;

        public KeyAction(Keys key, KeyActionHandler keyActionHandler) {
            this.key = key;
            this.keyActionHandler = keyActionHandler;
        }

        public Keys Key {
            get { return key; }
            set { key = value; }
        }

        public void DoKeyAction() {
            keyActionHandler();
        }
    }
}
