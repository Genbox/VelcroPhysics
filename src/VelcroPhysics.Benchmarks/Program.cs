using BenchmarkDotNet.Running;
using VelcroPhysics.Benchmarks.Tests.Collision;
using VelcroPhysics.Benchmarks.Tests.Primitives;
using VelcroPhysics.Benchmarks.Tests.Shared;

namespace VelcroPhysics.Benchmarks
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
