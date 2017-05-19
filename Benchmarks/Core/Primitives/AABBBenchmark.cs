using BenchmarkDotNet.Attributes;

namespace Benchmarks.Core.Primitives
{
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
