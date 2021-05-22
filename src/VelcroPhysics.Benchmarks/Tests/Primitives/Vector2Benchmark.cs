using BenchmarkDotNet.Attributes;
using Genbox.VelcroPhysics.Benchmarks.Code;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Genbox.VelcroPhysics.Benchmarks.Tests.Primitives
{
    public class Vector2Benchmark : UnmeasuredBenchmark
    {
        [Benchmark]
        public Vector2 CreateWithCtor()
        {
            return new Vector2(1, 2);
        }

        [Benchmark]
        public Vector2 CreateWithNoInit()
        {
            Vector2 v;
            v.X = 1;
            v.Y = 2;
            return v;
        }

        [Benchmark]
        public Vector2 CreateWithZero()
        {
            Vector2 v = Vector2.Zero;
            v.X = 1;
            v.Y = 2;
            return v;
        }
    }
}