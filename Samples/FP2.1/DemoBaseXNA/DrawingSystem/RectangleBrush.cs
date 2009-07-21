using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DrawingSystem
{
    public class RectangleBrush
    {
        private Texture2D _rectangleTexture;
        private Vector2 _rectangleOrigin;

        public RectangleBrush()
        {
            Color = Color.White;
            BorderColor = Color.Black;
        }

        public RectangleBrush(int width, int height, Color color, Color borderColor)
        {
            Color = color;
            BorderColor = borderColor;
            Width = width;
            Height = height;
        }

        public Color Color { get; set; }

        public Color BorderColor { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public float Layer { get; set; }

        public void Load(GraphicsDevice graphicsDevice)
        {
            if (Height == 0)
                throw new ArgumentException("You need to set a height before you can load the brush.", "Height");

            if (Width == 0)
                throw new ArgumentException("You need to set a width before you can load the brush.", "Width");

            _rectangleTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, Width, Height, Color,
                                                                     BorderColor);
            _rectangleOrigin = new Vector2(_rectangleTexture.Width / 2f, _rectangleTexture.Height / 2f);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, float rotation)
        {
            spriteBatch.Draw(_rectangleTexture, position, null, Color.White, rotation,
                             _rectangleOrigin, 1, SpriteEffects.None, Layer);
        }
    }
}