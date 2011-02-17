using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;

namespace FarseerPhysics.SamplesFramework
{
    public struct DrawableObject
    {
        public Texture2D sprite;
        public Vector2 origin;
        public Color color;

        public DrawableObject(Texture2D sprite, Vector2 origin, Color color)
        {
            this.sprite = sprite;
            this.origin = origin;
            this.color = color;
        }
    }
}
