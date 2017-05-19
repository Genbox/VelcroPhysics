using System.Diagnostics;
using VelcroPhysics.Shared.Optimization;

namespace VelcroPhysics.Shared
{
    public class BenchmarkRun : IPoolable<BenchmarkRun>
    {
        private string _area;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private bool _resultsRecorded;

        public void SetData(string area)
        {
            _area = area;
            _stopwatch.Start();
        }

        public void Dispose()
        {
            RecordResults();
            Pool.ReturnToPool(this);
        }

        public void RecordResults()
        {
            if (_resultsRecorded)
                return;

            _stopwatch.Stop();
            Benchmark.RecordResults(_area, _stopwatch.ElapsedTicks);
            _resultsRecorded = true;
        }

        public void Reset()
        {
            _stopwatch.Reset();
        }

        public Pool<BenchmarkRun> Pool { private get; set; }
    }
}