using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DrawingSystem
{
    /// <summary>
    /// Used to hold information about a quadrilateral.
    /// </summary>
    public class Quad
    {
        #region Fields

        public Vector2 Position;
        public float Rotation;
        public float Width;
        public float Height;
        public float Alpha;
        public float Layer;
        public int TextureIndex;
        public Color Tint;

        #endregion

        public Quad(Vector2 position, float rotation, float width, float height, int textureIndex)
        {
            Position = position;
            Rotation = rotation;
            Width = width;
            Height = height;
            TextureIndex = textureIndex;

            // alpha is opaque
            Alpha = 1.0f;
            // layer is half
            Layer = 0.5f;
            // tint is none aka white
            Tint = Color.White;
        }

        public Quad(Vector2 position, float rotation, float width, float height, int textureIndex, Color tint)
        {
            Position = position;
            Rotation = rotation;
            Width = width;
            Height = height;
            TextureIndex = textureIndex;

            // alpha is opaque
            Alpha = 1.0f;
            // layer is half
            Layer = 0.5f;
            // tint is none aka white
            Tint = tint;
        }
    }
}