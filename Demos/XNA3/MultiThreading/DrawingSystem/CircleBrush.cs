using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.DrawingSystem
{
    public class CircleBrush
    {
        private Color _borderColor;
        private Texture2D _circleTexture;
        private Color _color = Color.Black;
        private float _layer;
        private int _radius = 5;

        public CircleBrush()
        {
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
            _circleTexture = DrawingHelper.CreateCircleTexture(graphicsDevice, _radius, _color, _borderColor);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            spriteBatch.Draw(_circleTexture, position, null, Color.White, 0,
                             new Vector2(1 + _circleTexture.Width/2f, 1 + _circleTexture.Height/2), 1,
                             SpriteEffects.None,
                             _layer);
        }
    }
}