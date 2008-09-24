using FarseerGames.FarseerPhysics.Mathematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.DrawingSystem
{
    public class LineBrush
    {
        private Color _color = Color.Black;
        private Vector2 _difference;
        private float _layer;
        private Texture2D _lineTexture;
        private Vector2 _normalizedDifference = Vector2.Zero;
        private Vector2 _origin;
        private float _rotation;
        private Vector2 _scale;
        private float _theta;
        private int _thickness = 1;
        private Vector2 _xVector = new Vector2(1, 0);

        public LineBrush()
        {
        }

        public LineBrush(int thickness, Color color)
        {
            _color = color;
            _thickness = thickness;
        }

        public int Thickness
        {
            get { return _thickness; }
            set { _thickness = value; }
        }

        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public float Layer
        {
            get { return _layer; }
            set { _layer = value; }
        }

        public void Load(GraphicsDevice graphicsDevice)
        {
            _lineTexture = DrawingHelper.CreateLineTexture(graphicsDevice, _thickness, _color);
            _origin = new Vector2(0, _thickness/2f + 1);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 startPoint, Vector2 endPoint)
        {
            Vector2.Subtract(ref endPoint, ref startPoint, out _difference);
            CalculateRotation(_difference);
            CalculateScale(_difference);
            spriteBatch.Draw(_lineTexture, startPoint, null, _color, _rotation, _origin, _scale, SpriteEffects.None,
                             _layer);
        }

        private void CalculateRotation(Vector2 difference)
        {
            Vector2.Normalize(ref difference, out _normalizedDifference);
            Vector2.Dot(ref _xVector, ref _normalizedDifference, out _theta);

            _theta = Calculator.ACos(_theta);
            if (difference.Y < 0)
            {
                _theta = -_theta;
            }
            _rotation = _theta;
        }

        private void CalculateScale(Vector2 difference)
        {
            float desiredLength = difference.Length();
            _scale.X = desiredLength/_lineTexture.Width;
            _scale.Y = 1;
        }
    }
}