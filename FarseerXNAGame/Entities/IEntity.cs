using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerXNAGame.Entities {
    public interface IEntity {
        float Orientation{ get;set;}
        Vector2 Position { get;set;}
    }
}
