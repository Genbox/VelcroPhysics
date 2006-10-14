using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerXNAGame.Sprites {
    public class SpriteList : List<Sprite> {
        internal static bool IsDisposed(Sprite a) {
            return a.IsDisposed;   
        }
    }
}
