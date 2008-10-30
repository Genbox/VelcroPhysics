#if (XNA)
using Microsoft.Xna.Framework;
#endif

namespace FarseerGames.FarseerPhysics.Mathematics
{
    /// <summary>
    /// Encapsulates the logic to do bilinear interpolation
    /// .V1    .V4
    /// .V2    .V3
    /// </summary>
    public class CircularInterpolator
    {
        private const float _twoOverPi = 1f/MathHelper.PiOver2;
        private float _circleValue1;
        private float _circleValue2;
        private float _maxValue = float.MaxValue;
        private float _minValue = float.MinValue;
        private float _value1;
        private float _value2;
        private float _value3;
        private float _value4;

        public CircularInterpolator(float value1, float value2, float value3, float value4, float minValue,
                                    float maxValue)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
            _minValue = minValue;
            _maxValue = maxValue;
        }

        public float GetValue(Vector2 position)
        {
            return GetValue(position.X, position.Y);
        }

        public float GetValue(float x, float y)
        {
            float theta;
            float lerpValue;
            float value = 0;

            float d = MathHelper.Distance(x, y);

            //quadrant 1
            if (x > 0 && y > 0)
            {
                theta = Calculator.ATan2(y, x);
                lerpValue = theta*_twoOverPi;
                _circleValue1 = MathHelper.Lerp(_value1, _value2, lerpValue);
                _circleValue2 = MathHelper.Lerp(_value3, _value4, lerpValue);
                value = MathHelper.Lerp(_circleValue1, _circleValue2, (1 - d)*.5f);
            }
            if (x < 0 && y > 0)
            {
                theta = Calculator.ATan2(y, x);
                lerpValue = (theta - MathHelper.PiOver2)*_twoOverPi;
                _circleValue1 = MathHelper.Lerp(_value2, _value3, lerpValue);
                _circleValue2 = MathHelper.Lerp(_value4, _value1, lerpValue);
                value = MathHelper.Lerp(_circleValue1, _circleValue2, (1 - d)*.5f);
            }
            if (x < 0 && y < 0)
            {
                theta = Calculator.ATan2(y, x) + MathHelper.TwoPi;
                lerpValue = (theta - MathHelper.Pi)*_twoOverPi;
                _circleValue1 = MathHelper.Lerp(_value3, _value4, lerpValue);
                _circleValue2 = MathHelper.Lerp(_value1, _value2, lerpValue);
                value = MathHelper.Lerp(_circleValue1, _circleValue2, (1 - d)*.5f);
            }
            if (x > 0 && y < 0)
            {
                theta = Calculator.ATan2(y, x) + MathHelper.TwoPi;
                lerpValue = (theta - 3*MathHelper.PiOver2)*_twoOverPi;
                _circleValue1 = MathHelper.Lerp(_value4, _value1, lerpValue);
                _circleValue2 = MathHelper.Lerp(_value2, _value3, lerpValue);
                value = MathHelper.Lerp(_circleValue1, _circleValue2, (1 - d)*.5f);
            }
            if (x == 0 && y > 0)
            {
                value = MathHelper.Lerp(_value2, _value4, (1 - y)/2f);
            }
            if (x == 0 && y < 0)
            {
                value = MathHelper.Lerp(_value4, _value2, (1 + y)/2f); //1--y
            }
            if (x > 0 && y == 0)
            {
                value = MathHelper.Lerp(_value1, _value3, (1 - x)/2f);
            }
            if (x < 0 && y == 0)
            {
                value = MathHelper.Lerp(_value3, _value1, (1 + x)/2f); //1--y
            }
            value = MathHelper.Clamp(value, _minValue, _maxValue);
            return value;
        }
    }
}