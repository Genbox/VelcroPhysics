using FarseerGames.FarseerPhysics.Mathematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.DrawingSystem
{
    public class LineBrush
    {
        private Color color = Color.Black;
        private Vector2 difference;
        private float layer;
        private Texture2D lineTexture;
        private Vector2 normalizedDifference = Vector2.Zero;
        private Vector2 origin;
        private float rotation;
        private Vector2 scale;
        private float theta;
        private int thickness = 1;
        private Vector2 xVector = new Vector2(1, 0);

        public LineBrush()
        {
        }

        public LineBrush(int thickness, Color color)
        {
            this.color = color;
            this.thickness = thickness;
        }

        public int Thickness
        {
            get { return thickness; }
            set { thickness = value; }
        }

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public float Layer
        {
            get { return layer; }
            set { layer = value; }
        }

        public void Load(GraphicsDevice graphicsDevice)
        {
            lineTexture = DrawingHelper.CreateLineTexture(graphicsDevice, thickness, color);
            origin = new Vector2(0, thickness/2f + 1);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 startPoint, Vector2 endPoint)
        {
            Vector2.Subtract(ref endPoint, ref startPoint, out difference);
            CalculateRotation(difference);
            CalculateScale(difference);
            spriteBatch.Draw(lineTexture, startPoint, null, color, rotation, origin, scale, SpriteEffects.None, layer);
        }

        private void CalculateRotation(Vector2 difference)
        {
            Vector2.Normalize(ref difference, out normalizedDifference);
            Vector2.Dot(ref xVector, ref normalizedDifference, out theta);

            theta = Calculator.ACos(theta);
            if (difference.Y < 0)
            {
                theta = -theta;
            }
            rotation = theta;
        }

        private void CalculateScale(Vector2 difference)
        {
            float desiredLength = difference.Length();
            scale.X = desiredLength/lineTexture.Width;
            scale.Y = 1;
        }
    }
}