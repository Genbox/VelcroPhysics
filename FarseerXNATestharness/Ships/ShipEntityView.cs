using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerGames.FarseerXNAGame.Entities;
using FarseerGames.FarseerXNAGame.Sprites;

namespace FarseerGames.FarseerXNATestharness.Ships {
    public class ShipEntityView : SpriteEntityView<ShipEntity> {
        public ShipEntityView(ShipEntity entity, string textureName, SpriteManager spriteManager)
            : base(entity, textureName, spriteManager) {           
            
        }
    }
}
