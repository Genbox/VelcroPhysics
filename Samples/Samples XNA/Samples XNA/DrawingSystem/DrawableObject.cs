using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;

namespace FarseerPhysics.DemoBaseXNA.DrawingSystem
{
    public class DrawableObject
    {
        public Texture2D texture { get; private set; }
        public Color color { get; private set; }
        public Vector2 origin { get; private set; }
        public Body body;

        public DrawableObject()
        {
        }
    }
}
