using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;

namespace FarseerPhysics.SamplesFramework
{
    public struct Sprite
    {
        public Texture2D texture;
        public Vector2 origin;

        public Sprite(Texture2D texture, Vector2 origin)
        {
            this.texture = texture;
            this.origin = origin;
        }

        public Sprite(Texture2D sprite)
        {
            this.texture = sprite;
            this.origin = new Vector2(sprite.Width / 2f, sprite.Height / 2f);
        }
    }
}
