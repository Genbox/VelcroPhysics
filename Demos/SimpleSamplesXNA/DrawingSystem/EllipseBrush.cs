using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.SimpleSamplesXNA.DrawingSystem
{
    public class EllipseBrush
    {
        private Color _borderColor;
        private Color _color;
        private float _layer;
        private Texture2D _ellipseTexture;
        private Vector2 _ellipseOrigin;

        public int XRadius;
        public int YRadius;

        public EllipseBrush()
        {
            _color = Color.White;
            _borderColor = Color.Black;
        }

        public EllipseBrush(int xRadius, int yRadius, Color color, Color borderColor)
        {
            _color = color;
            _borderColor = borderColor;
            XRadius = xRadius;
            YRadius = yRadius;
        }

        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; }
        }

        public float Layer
        {
            get { return _layer; }
            set { _layer = value; }
        }

        public void Load(GraphicsDevice graphicsDevice)
        {
            if (XRadius == 0)
                throw new ArgumentException("You need to set a x-radius before you can load the brush.", "XRadius");

            if (YRadius == 0)
                throw new ArgumentException("You need to set a y-radius before you can load the brush.", "YRadius");

            _ellipseTexture = DrawingHelper.CreateEllipseTexture(graphicsDevice, XRadius, YRadius, 1, _color, _borderColor);
            _ellipseOrigin = new Vector2(_ellipseTexture.Width / 2f, _ellipseTexture.Height / 2f);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, float rotation)
        {
            spriteBatch.Draw(_ellipseTexture, position, null, Color.White, rotation,
                             _ellipseOrigin, 1, SpriteEffects.None, _layer);
        }
    }
}