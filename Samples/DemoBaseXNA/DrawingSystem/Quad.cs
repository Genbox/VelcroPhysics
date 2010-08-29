using System;
using System.Collections.Generic;
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
        public List<RectF> Frames;
        public int CurrentFrame;
        public int NumOfFrames { get { return Frames.Count; } }
        public bool AutoLoop;

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

            Frames = new List<RectF>();
            CurrentFrame = 0;

            // if the quad is not animated assume it uses the full texture
            Frames.Add(new RectF(0, 0, 1, 1));
        }

        public Quad(Vector2 position, float rotation, float width, float height, float alpha, float layer, int textureIndex, Color tint, bool animated)
        {
            Position = position;
            Rotation = rotation;
            Width = width;
            Height = height;
            TextureIndex = textureIndex;
            Alpha = alpha;
            Layer = layer;
            Tint = tint;

            Frames = new List<RectF>();
            CurrentFrame = 0;

            // if the quad is not animated assume it uses the full texture
            if (!animated)
                Frames.Add(new RectF(0, 0, 1, 1));
        }
    }
}