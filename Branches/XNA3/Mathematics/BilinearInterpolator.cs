using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Mathematics
{
    /// <summary>
    /// Encapsulates the logic to do bilinear interpolation
    /// .V1    .V4
    /// .V2    .V3
    /// </summary>
    public class BilinearInterpolator
    {
        private readonly float _maxValue = float.MaxValue;
        private readonly float _minValue = float.MinValue;
        private readonly float _value1;
        private readonly float _value2;
        private readonly float _value3;
        private readonly float _value4;

        public BilinearInterpolator()
        {
        }

        public BilinearInterpolator(Vector2 min, Vector2 max)
        {
            Max = max;
            Min = min;
        }

        public BilinearInterpolator(Vector2 min, Vector2 max, float value1, float value2, float value3, float value4,
                                    float minValue, float maxValue)
        {
            Min = min;
            Max = max;
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
            _minValue = minValue;
            _maxValue = maxValue;
        }

        public Vector2 Min { get; set; }

        public Vector2 Max { get; set; }

        public float GetValue(Vector2 position)
        {
            return GetValue(position.X, position.Y);
        }

        public float GetValue(float x, float y)
        {
            x = MathHelper.Clamp(x, Min.X, Max.X);
            y = MathHelper.Clamp(y, Min.Y, Max.Y);

            float xRatio = (x - Min.X)/(Max.X - Min.X);
            float yRatio = (y - Min.Y)/(Max.Y - Min.Y);

            float top = MathHelper.Lerp(_value1, _value4, xRatio);
            float bottom = MathHelper.Lerp(_value2, _value3, xRatio);

            float value = MathHelper.Lerp(top, bottom, yRatio);
            value = MathHelper.Clamp(value, _minValue, _maxValue);
            return value;
        }
    }
}