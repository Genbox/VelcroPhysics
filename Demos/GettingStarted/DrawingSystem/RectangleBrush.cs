using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.DrawingSystem
{
    internal class RectangleBrush
    {
        private Color borderColor;
        private Color color = Color.Black;
        private int height;
        private float layer;
        private Texture2D rectangleTexture;
        private int width = 5;

        public RectangleBrush()
        {
        }

        public RectangleBrush(int width, int height, Color color, Color borderColor)
        {
            this.color = color;
            this.borderColor = borderColor;
            this.width = width;
            this.height = height;
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


        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        public float Layer
        {
            get { return layer; }
            set { layer = value; }
        }


        public void Load(GraphicsDevice graphicsDevice)
        {
            rectangleTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, width, height, color, borderColor);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            spriteBatch.Draw(rectangleTexture, position, null, Color.White, 0,
                             new Vector2(1 + rectangleTexture.Width/2f, 1 + rectangleTexture.Height/2), 1,
                             SpriteEffects.None, layer);
            //new Vector2(radius, radius)
        }
    }
}