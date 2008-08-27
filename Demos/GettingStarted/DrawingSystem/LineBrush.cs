using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysicsDemos.DrawingSystem {
    public class LineBrush {
        private Vector2 origin;
        private Vector2 scale;
        private float rotation;
        Vector2 xVector = new Vector2(1, 0);
        private Texture2D lineTexture;

        public LineBrush() {
        }

        public LineBrush(int thickness, Color color) {
            this.color = color;
            this.thickness = thickness;
        }

        private int thickness = 1;
        public int Thickness {
            get { return thickness; }
            set { thickness = value; }
        }

        private Color color = Color.Black;
        public Color Color {
            get { return color; }
            set { color = value; }
        }

        private float layer = 0;
        public float Layer {
            get { return layer; }
            set { layer = value; }
        }

        public void Load(GraphicsDevice graphicsDevice) {
            lineTexture = DrawingHelper.CreateLineTexture(graphicsDevice, thickness, color);
            origin = new Vector2(0, thickness / 2f + 1);
        }

        Vector2 difference;
        public void Draw(SpriteBatch spriteBatch, Vector2 startPoint, Vector2 endPoint) {
            Vector2.Subtract(ref endPoint, ref startPoint, out difference);
            CalculateRotation(difference);
            CalculateScale(difference);
            spriteBatch.Draw(lineTexture, startPoint, null, color, rotation, origin, scale, SpriteEffects.None, layer);
        }

        Vector2 normalizedDifference = Vector2.Zero;
        float theta = 0;
        private void CalculateRotation(Vector2 difference) {
            Vector2.Normalize(ref difference, out normalizedDifference);
            Vector2.Dot(ref xVector, ref normalizedDifference, out theta);

            theta = Calculator.ACos(theta);
            if (difference.Y < 0) { theta = -theta; }
            rotation = theta;
        }

        private void CalculateScale(Vector2 difference) {
            float desiredLength;
            desiredLength = difference.Length();
            scale.X = desiredLength / lineTexture.Width;
            scale.Y = 1;
        }
    }
}
