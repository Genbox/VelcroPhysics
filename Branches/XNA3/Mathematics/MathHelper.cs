#if(!XNA)
using System;

namespace FarseerGames.FarseerPhysics.Mathematics
{
    public class MathHelper
    {
        public const float DegreesToRadiansRatio = 57.29577957855f;
        public const float RadiansToDegreesRatio = 1f / 57.29577957855f;

        public static float Lerp(float value1, float value2, float amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        public static float Min(float value1, float value2)
        {
            return Math.Min(value1, value2);
        }

        public static float Max(float value1, float value2)
        {
            return Math.Max(value1, value2);
        }

        public static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        public static double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        public static float Distance(float value1, float value2)
        {
            return Math.Abs((float)(value1 - value2));
        }

        public static float ToRadians(float degrees)
        {
            return degrees * RadiansToDegreesRatio;
        }

        public static float TwoPi = (float)(Math.PI * 2.0);
        public static float Pi = (float)(Math.PI);
        public static float PiOver2 = (float)(Math.PI / 2.0);
        public static float PiOver4 = (float)(Math.PI / 4.0);
      
    }
}
#endif