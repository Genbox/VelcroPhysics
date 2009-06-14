using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.SimpleSamplesXNA.DrawingSystem
{
    public class RectangleBrush
    {
        private Color _borderColor;
        private Color _color;
        private int _width;
        private int _height;
        private float _layer;
        private Texture2D _rectangleTexture;
        private Vector2 _rectangleOrigin;

        public RectangleBrush()
        {
            _color = Color.White;
            _borderColor = Color.Black;
        }

        public RectangleBrush(int width, int height, Color color, Color borderColor)
        {
            _color = color;
            _borderColor = borderColor;
            _width = width;
            _height = height;
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

        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public float Layer
        {
            get { return _layer; }
            set { _layer = value; }
        }

        public void Load(GraphicsDevice graphicsDevice)
        {
            if (_height == 0)
                throw new ArgumentException("You need to set a height before you can load the brush.", "height");

            if (_width == 0)
                throw new ArgumentException("You need to set a width before you can load the brush.", "width");

            _rectangleTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, _width, _height, _color,
                                                                     _borderColor);
            _rectangleOrigin = new Vector2(_rectangleTexture.Width / 2f, _rectangleTexture.Height / 2f);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, float rotation)
        {
            spriteBatch.Draw(_rectangleTexture, position, null, Color.White, rotation,
                             _rectangleOrigin, 1, SpriteEffects.None, _layer);
        }
    }
}