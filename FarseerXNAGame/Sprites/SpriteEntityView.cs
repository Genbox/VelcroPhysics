using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerGames.FarseerXNAGame.Entities;

namespace FarseerGames.FarseerXNAGame.Sprites {
    public class SpriteEntityView<TEntity> : Sprite, IEntityView<TEntity> where TEntity : IEntity {
        protected TEntity entity;
        
        public SpriteEntityView(TEntity entity, string textureName, SpriteManager spriteManager)
            : base(textureName, spriteManager) {
            this.entity = entity;
        }

        public TEntity Entity {
            get { return entity; }
            set { entity = value; }
        }

        public void UpdateView() {
            base.Position = entity.Position;
            base.Orientation = entity.Orientation;
        }

        public override void Draw(SpriteBatch spriteBatch) {
            UpdateView();
            base.Draw(spriteBatch);
        }
    }
}
