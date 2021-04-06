using BenchmarkDotNet.Attributes;

namespace VelcroPhysics.Benchmarks.Tests.Primitives
{
    [InProcess]
    public class AABBBenchmark
    {
        [Benchmark]
        public void TestOverlap()
        {
            //bool overlap = AABB.TestOverlap(ref _queryAABB, ref _actors[i].AABB);
        }

        [Benchmark]
        public void RayCast()
        {
            //bool hit = _actors[i].AABB.RayCast(out RayCastOutput output, ref input);
        }
    }
}
