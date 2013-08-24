using System;

namespace FarseerPhysics.Testbed.Framework
{
    public static class Rand
    {
        public static Random Random = new Random();

        /// <summary>
        /// Random number in range [-1,1]
        /// </summary>
        /// <returns></returns>
        public static float RandomFloat()
        {
            return (float)(Random.NextDouble() * 2.0 - 1.0);
        }

        /// <summary>
        /// Random floating point number in range [lo, hi]
        /// </summary>
        /// <param name="lo">The lo.</param>
        /// <param name="hi">The hi.</param>
        /// <returns></returns>
        public static float RandomFloat(float lo, float hi)
        {
            float r = (float)Random.NextDouble();
            r = (hi - lo) * r + lo;
            return r;
        }
    }
}