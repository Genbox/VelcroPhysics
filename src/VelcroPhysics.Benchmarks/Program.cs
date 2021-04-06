using BenchmarkDotNet.Running;

namespace VelcroPhysics.Benchmarks
{
    class Program
    {
        public static void Main()
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run();
        }
    }
}
