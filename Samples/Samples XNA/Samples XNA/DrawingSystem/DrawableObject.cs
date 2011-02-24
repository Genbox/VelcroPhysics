using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;

namespace FarseerPhysics.SamplesFramework
{
    public struct DrawableObject
    {
        public Texture2D texture;
        public Vector2 origin;

        public DrawableObject(Texture2D sprite, Vector2 origin)
        {
            this.texture = sprite;
            this.origin = origin;
        }

        public DrawableObject(Texture2D sprite)
        {
            this.texture = sprite;
            this.origin = new Vector2(sprite.Width / 2f, sprite.Height / 2f);
        }
    }
}
