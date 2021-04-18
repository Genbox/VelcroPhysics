using BenchmarkDotNet.Attributes;

namespace Genbox.VelcroPhysics.Benchmarks.Code
{
    /// <summary>This class sets the defaults for benchmarks that don't have to be measured</summary>
    [InProcess]
    [MemoryDiagnoser]
    public abstract class UnmeasuredBenchmark { }
}