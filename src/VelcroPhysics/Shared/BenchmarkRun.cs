using System;
using System.Diagnostics;
using Genbox.VelcroPhysics.Shared.Optimization;

namespace Genbox.VelcroPhysics.Shared
{
    public class BenchmarkRun : IPoolable<BenchmarkRun>
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private string _area;
        private bool _resultsRecorded;

        public void Dispose()
        {
            RecordResults();
            Pool.ReturnToPool(this);
            GC.SuppressFinalize(this);
        }

        public void Reset()
        {
            _stopwatch.Reset();
        }

        public Pool<BenchmarkRun> Pool { private get; set; }

        public void SetData(string area)
        {
            _area = area;
            _stopwatch.Start();
        }

        public void RecordResults()
        {
            if (_resultsRecorded)
                return;

            _stopwatch.Stop();
            Benchmark.RecordResults(_area, _stopwatch.ElapsedTicks);
            _resultsRecorded = true;
        }
    }
}