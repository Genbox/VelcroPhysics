using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.DrawingSystem {
    internal class RectangleBrush {
        private Texture2D rectangleTexture;

        public RectangleBrush() { }

        public RectangleBrush(int width, int height, Color color, Color borderColor) {
            this.color = color;
            this.borderColor = borderColor;
            this.width = width;
            this.height = height;
        }

        private Color color = Color.Black;
        public Color Color {
            get { return color; }
            set { color = value; }
        }

        private Color borderColor;

        public Color BorderColor {
            get { return borderColor; }
            set { borderColor = value; }
        }


        private int width = 5;
        public int Width {
            get { return width; }
            set { width = value; }
        }

        private int height;

        public int Height {
            get { return height; }
            set { height = value; }
        }

        private float layer = 0;
        public float Layer {
            get { return layer; }
            set { layer = value; }
        }


        public void Load(GraphicsDevice graphicsDevice) {
            rectangleTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, width, height, color, borderColor);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position) {
            spriteBatch.Draw(rectangleTexture, position, null, Color.White, 0, new Vector2(1 + rectangleTexture.Width / 2f, 1 + rectangleTexture.Height / 2), 1, SpriteEffects.None, layer);
            //new Vector2(radius, radius)
        }
    }
}