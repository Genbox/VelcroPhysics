using BenchmarkDotNet.Running;

namespace Genbox.VelcroPhysics.Benchmarks
{
    internal class Program
    {
        public static void Main()
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run();
        }
    }
}