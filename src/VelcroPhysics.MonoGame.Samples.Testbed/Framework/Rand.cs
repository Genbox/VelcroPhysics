using System;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework
{
    public static class Rand
    {
        private static readonly Random _random = new Random();

        /// <summary>Random number in range [-1,1]</summary>
        public static float RandomFloat()
        {
            return (float)(_random.NextDouble() * 2.0 - 1.0);
        }

        /// <summary>Random floating point number in range [lo, hi]</summary>
        /// <param name="lo">The lo.</param>
        /// <param name="hi">The hi.</param>
        public static float RandomFloat(float lo, float hi)
        {
            float r = (float)_random.NextDouble();
            r = (hi - lo) * r + lo;
            return r;
        }
    }
}