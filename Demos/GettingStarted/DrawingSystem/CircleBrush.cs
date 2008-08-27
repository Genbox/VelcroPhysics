using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.DrawingSystem {
    public class CircleBrush {
        private Texture2D circleTexture;

        public CircleBrush() { }

        public CircleBrush(int radius, Color color, Color borderColor) {
            this.color = color;
            this.borderColor = borderColor;
            this.radius = radius;
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


        private int radius = 5;
        public int Radius {
            get { return radius; }
            set { radius = value; }
        }

        private float layer = 0;
        public float Layer {
            get { return layer; }
            set { layer = value; }
        }


        public void Load(GraphicsDevice graphicsDevice) {
            circleTexture = DrawingHelper.CreateCircleTexture(graphicsDevice, radius, color, borderColor);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position) {
            spriteBatch.Draw(circleTexture, position, null, Color.White, 0, new Vector2(1 + circleTexture.Width / 2f, 1 + circleTexture.Height / 2), 1, SpriteEffects.None, layer);
            //new Vector2(radius, radius)
        }
    }
}