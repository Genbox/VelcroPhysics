using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerXNAGame {
    internal interface IEntity {
        float Orientation();
        Vector2 Position();
        bool Remove { get;set;}
    }
}
