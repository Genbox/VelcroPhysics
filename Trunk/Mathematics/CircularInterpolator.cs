namespace FarseerGames.FarseerPhysics.Mathematics
{
    /// <summary>
    /// Encapsulates the logic to do bilinear interpolation
    /// .V1    .V4
    /// .V2    .V3
    /// </summary>
    public class CircularInterpolator
    {
        private readonly float maxValue = float.MaxValue;
        private readonly float minValue = float.MinValue;
        private readonly float twoOverPi = 1f/MathHelper.PiOver2;
        private readonly float value1;
        private readonly float value2;
        private readonly float value3;
        private readonly float value4;
        private float circleValue1;
        private float circleValue2;

        public CircularInterpolator(float value1, float value2, float value3, float value4, float minValue,
                                    float maxValue)
        {
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
            this.value4 = value4;
            this.minValue = minValue;
            this.maxValue = maxValue;
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
                lerpValue = theta*twoOverPi;
                circleValue1 = MathHelper.Lerp(value1, value2, lerpValue);
                circleValue2 = MathHelper.Lerp(value3, value4, lerpValue);
                value = MathHelper.Lerp(circleValue1, circleValue2, (1 - d)*.5f);
            }
            if (x < 0 && y > 0)
            {
                theta = Calculator.ATan2(y, x);
                lerpValue = (theta - MathHelper.PiOver2)*twoOverPi;
                circleValue1 = MathHelper.Lerp(value2, value3, lerpValue);
                circleValue2 = MathHelper.Lerp(value4, value1, lerpValue);
                value = MathHelper.Lerp(circleValue1, circleValue2, (1 - d)*.5f);
            }
            if (x < 0 && y < 0)
            {
                theta = Calculator.ATan2(y, x) + MathHelper.TwoPi;
                lerpValue = (theta - MathHelper.Pi)*twoOverPi;
                circleValue1 = MathHelper.Lerp(value3, value4, lerpValue);
                circleValue2 = MathHelper.Lerp(value1, value2, lerpValue);
                value = MathHelper.Lerp(circleValue1, circleValue2, (1 - d)*.5f);
            }
            if (x > 0 && y < 0)
            {
                theta = Calculator.ATan2(y, x) + MathHelper.TwoPi;
                lerpValue = (theta - 3*MathHelper.PiOver2)*twoOverPi;
                circleValue1 = MathHelper.Lerp(value4, value1, lerpValue);
                circleValue2 = MathHelper.Lerp(value2, value3, lerpValue);
                value = MathHelper.Lerp(circleValue1, circleValue2, (1 - d)*.5f);
            }
            if (x == 0 && y > 0)
            {
                value = MathHelper.Lerp(value2, value4, (1 - y)/2f);
            }
            if (x == 0 && y < 0)
            {
                value = MathHelper.Lerp(value4, value2, (1 + y)/2f); //1--y
            }
            if (x > 0 && y == 0)
            {
                value = MathHelper.Lerp(value1, value3, (1 - x)/2f);
            }
            if (x < 0 && y == 0)
            {
                value = MathHelper.Lerp(value3, value1, (1 + x)/2f); //1--y
            }
            value = MathHelper.Clamp(value, minValue, maxValue);
            return value;
        }
    }
}