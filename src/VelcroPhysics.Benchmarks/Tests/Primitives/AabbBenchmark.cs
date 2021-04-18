using BenchmarkDotNet.Attributes;
using Genbox.VelcroPhysics.Benchmarks.Code;
using Genbox.VelcroPhysics.Collision.RayCast;
using Genbox.VelcroPhysics.Shared;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Benchmarks.Tests.Primitives
{
    public class AabbBenchmark : UnmeasuredBenchmark
    {
        private AABB _a;
        private AABB _b;
        private AABB _c;

        public AabbBenchmark()
        {
            _a = new AABB(new Vector2(100, 100), 10, 10);
            _b = new AABB(new Vector2(95, 95), 10, 10); //A and B overlap
            _c = new AABB(Vector2.Zero, 10, 10);
        }

        [Benchmark]
        public bool TestOverlap()
        {
            return AABB.TestOverlap(ref _a, ref _b);
        }

        [Benchmark]
        public bool TestNoOverlap()
        {
            return AABB.TestOverlap(ref _a, ref _c);
        }

        [Benchmark]
        public bool RayCast()
        {
            RayCastInput input = new RayCastInput();
            input.Point1 = Vector2.Zero;
            input.Point2 = _a.Center;

            return _a.RayCast(ref input, out _);
        }
    }
}