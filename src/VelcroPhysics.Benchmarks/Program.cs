using BenchmarkDotNet.Running;

namespace VelcroPhysics.Benchmarks
{
    internal class Program
    {
        public static void Main()
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run();
        }
    }
}