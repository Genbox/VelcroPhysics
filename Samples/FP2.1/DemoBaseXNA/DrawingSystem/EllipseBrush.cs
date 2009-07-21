using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DrawingSystem
{
    public class EllipseBrush
    {
        private Texture2D _ellipseTexture;
        private Vector2 _ellipseOrigin;

        public int XRadius;
        public int YRadius;

        public EllipseBrush()
        {
            Color = Color.White;
            BorderColor = Color.Black;
        }

        public EllipseBrush(int xRadius, int yRadius, Color color, Color borderColor)
        {
            Color = color;
            BorderColor = borderColor;
            XRadius = xRadius;
            YRadius = yRadius;
        }

        public Color Color { get; set; }

        public Color BorderColor { get; set; }

        public float Layer { get; set; }

        public void Load(GraphicsDevice graphicsDevice)
        {
            if (XRadius == 0)
                throw new ArgumentException("You need to set a x-radius before you can load the brush.", "XRadius");

            if (YRadius == 0)
                throw new ArgumentException("You need to set a y-radius before you can load the brush.", "YRadius");

            _ellipseTexture = DrawingHelper.CreateEllipseTexture(graphicsDevice, XRadius, YRadius, 1, Color, BorderColor);
            _ellipseOrigin = new Vector2(_ellipseTexture.Width / 2f, _ellipseTexture.Height / 2f);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, float rotation)
        {
            spriteBatch.Draw(_ellipseTexture, position, null, Color.White, rotation,
                             _ellipseOrigin, 1, SpriteEffects.None, Layer);
        }
    }
}