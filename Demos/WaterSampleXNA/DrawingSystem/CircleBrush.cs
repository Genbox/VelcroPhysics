using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.WaterSampleXNA.DrawingSystem
{
    public class CircleBrush
    {
        private Color _borderColor;
        private Color _color;
        private float _layer;
        private int _radius;
        private Texture2D _circleTexture;
        private Vector2 _circleOrigin;

        public CircleBrush()
        {
            _color = Color.White;
            _borderColor = Color.Black;
        }

        public CircleBrush(int radius, Color color, Color borderColor)
        {
            _color = color;
            _borderColor = borderColor;
            _radius = radius;
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

        public int Radius
        {
            get { return _radius; }
            set { _radius = value; }
        }

        public float Layer
        {
            get { return _layer; }
            set { _layer = value; }
        }

        public void Load(GraphicsDevice graphicsDevice)
        {
            if (_radius == 0)
                throw new ArgumentException("You need to set a radius before you can load the brush.", "radius");

            _circleTexture = DrawingHelper.CreateCircleTexture(graphicsDevice, _radius, _color, _borderColor);
            _circleOrigin = new Vector2(_circleTexture.Width / 2f, _circleTexture.Height / 2f);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            spriteBatch.Draw(_circleTexture, position, null, Color.White, 0,
                             _circleOrigin, 1, SpriteEffects.None, _layer);
        }
    }
}