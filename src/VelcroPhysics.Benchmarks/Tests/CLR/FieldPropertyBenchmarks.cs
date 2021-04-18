using BenchmarkDotNet.Attributes;
using Genbox.VelcroPhysics.Benchmarks.Code;
using Genbox.VelcroPhysics.Benchmarks.Code.TestClasses;

namespace Genbox.VelcroPhysics.Benchmarks.Tests.CLR
{
    public class FieldPropertyBenchmarks : UnmeasuredBenchmark
    {
        private readonly Dummy _dummy = new Dummy { ValueField = new Struct32() };

        [Benchmark]
        public Struct32 PropertyGetTest()
        {
            return _dummy.ValueProperty;
        }

        [Benchmark]
        public Struct32 FieldGetTest()
        {
            return _dummy.ValueField;
        }

        [Benchmark]
        public Struct32 MethodGetTest()
        {
            return _dummy.ValueMethod();
        }
    }
}
