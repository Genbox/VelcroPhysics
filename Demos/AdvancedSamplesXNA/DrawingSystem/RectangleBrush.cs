using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.DrawingSystem
{
    public class RectangleBrush
    {
        private Color _borderColor;
        private Color _color;
        private int _height;
        private float _layer;
        private Texture2D _rectangleTexture;
        private int _width = 5;

        public RectangleBrush()
        {
            _color = Color.Black;
        }

        public RectangleBrush(int width, int height, Color color, Color borderColor)
        {
            _color = Color.Black;
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
            _rectangleTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, _width, _height, _color,
                                                                     _borderColor);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            spriteBatch.Draw(_rectangleTexture, position, null, Color.White, 0,
                             new Vector2(1 + _rectangleTexture.Width/2f, 1 + _rectangleTexture.Height/2), 1,
                             SpriteEffects.None, _layer);
        }
    }
}