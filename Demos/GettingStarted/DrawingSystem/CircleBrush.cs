using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.DrawingSystem
{
    public class CircleBrush
    {
        private Color borderColor;
        private Texture2D circleTexture;
        private Color color = Color.Black;
        private float layer;
        private int radius = 5;

        public CircleBrush()
        {
        }

        public CircleBrush(int radius, Color color, Color borderColor)
        {
            this.color = color;
            this.borderColor = borderColor;
            this.radius = radius;
        }

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public Color BorderColor
        {
            get { return borderColor; }
            set { borderColor = value; }
        }


        public int Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        public float Layer
        {
            get { return layer; }
            set { layer = value; }
        }


        public void Load(GraphicsDevice graphicsDevice)
        {
            circleTexture = DrawingHelper.CreateCircleTexture(graphicsDevice, radius, color, borderColor);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            spriteBatch.Draw(circleTexture, position, null, Color.White, 0,
                             new Vector2(1 + circleTexture.Width/2f, 1 + circleTexture.Height/2), 1, SpriteEffects.None,
                             layer);
            //new Vector2(radius, radius)
        }
    }
}