using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters.Csv;

namespace Genbox.VelcroPhysics.Benchmarks.Code
{
    /// <summary>This class sets the defaults for benchmarks that should be measured on a continuous basis.</summary>
    [CsvMeasurementsExporter(CsvSeparator.Comma)]
    [RPlotExporter]
    [InProcess]
    [MemoryDiagnoser]
    public abstract class MeasuredBenchmark { }
}