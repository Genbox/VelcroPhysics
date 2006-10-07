using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerXNAGame.Sprites {
    public interface ISpriteManagerService {
        void Add(Sprite sprite);
        void Remove(Sprite sprite);
    }
}
