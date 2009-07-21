using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DrawingSystem
{
    public class CircleBrush
    {
        private Texture2D _circleTexture;
        private Vector2 _circleOrigin;

        public CircleBrush()
        {
            Color = Color.White;
            BorderColor = Color.Black;
        }

        public CircleBrush(int radius, Color color, Color borderColor)
        {
            Color = color;
            BorderColor = borderColor;
            Radius = radius;
        }

        public Color Color { get; set; }

        public Color BorderColor { get; set; }

        public int Radius { get; set; }

        public float Layer { get; set; }

        public void Load(GraphicsDevice graphicsDevice)
        {
            if (Radius == 0)
                throw new ArgumentException("You need to set a radius before you can load the brush.", "radius");

            _circleTexture = DrawingHelper.CreateCircleTexture(graphicsDevice, Radius, Color, BorderColor);
            _circleOrigin = new Vector2(_circleTexture.Width / 2f, _circleTexture.Height / 2f);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            spriteBatch.Draw(_circleTexture, position, null, Color.White, 0,
                             _circleOrigin, 1, SpriteEffects.None, Layer);
        }
    }
}