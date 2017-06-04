using BenchmarkDotNet.Running;
using Benchmarks.Core.Collision;
using Benchmarks.Core.Primitives;
using Benchmarks.Core.Shared;

namespace Benchmarks
{
    class Program
    {
        public static void Main()
        {
            //Collision
            BenchmarkRunner.Run<BroadphaseBenchmark>();
            BenchmarkRunner.Run<DistanceBenchmark>();
            BenchmarkRunner.Run<TOIBenchmark>();

            //Primitives
            BenchmarkRunner.Run<AABBBenchmark>();

            //Utils
            BenchmarkRunner.Run<PoolBenchmarks>();
        }
    }
}
