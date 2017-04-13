using System;

namespace Benchmarks.Utilities
{
    public static class RandomExtensions
    {
        /// <summary>
        /// Random number in range [-1,1]
        /// </summary>
        public static float RandomFloat(this Random random)
        {
            return (float)(random.NextDouble() * 2.0 - 1.0);
        }

        /// <summary>
        /// Random floating point number in range [lo, hi]
        /// </summary>
        public static float RandomFloat(this Random random, float lo, float hi)
        {
            float r = (float)random.NextDouble();
            r = (hi - lo) * r + lo;
            return r;
        }
    }
}