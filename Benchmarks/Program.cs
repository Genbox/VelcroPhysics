using BenchmarkDotNet.Running;
using Benchmarks.Benchmarks.Collision;
using Benchmarks.Benchmarks.Primitives;
using Benchmarks.Benchmarks.Utils;

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
