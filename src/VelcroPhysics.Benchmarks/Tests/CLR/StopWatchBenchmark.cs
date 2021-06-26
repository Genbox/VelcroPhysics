using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using Genbox.VelcroPhysics.Benchmarks.Code;

namespace Genbox.VelcroPhysics.Benchmarks.Tests.CLR
{
    public class StopWatchBenchmark : UnmeasuredBenchmark
    {
        private Stopwatch _stopWatch = new Stopwatch();
        private DummyStopWatch _dummy = new DummyStopWatch();
        public static bool Enabled = true;

        [Benchmark]
        public long StopWatchReset()
        {
            long time1 = 0;
            long time2 = 0;
            long time3 = 0;
            long time4 = 0;
            long time5 = 0;

            _stopWatch.Start();
            time1 = _stopWatch.ElapsedMilliseconds;
            _stopWatch.Restart();
            time2 = _stopWatch.ElapsedMilliseconds;
            _stopWatch.Restart();
            time3 = _stopWatch.ElapsedMilliseconds;
            _stopWatch.Restart();
            time4 = _stopWatch.ElapsedMilliseconds;
            _stopWatch.Restart();
            time5 = _stopWatch.ElapsedMilliseconds;
            _stopWatch.Stop();

            return time1 + time2 + time3 + time4 + time5;
        }

        [Benchmark]
        public long DeltaTime()
        {
            long time1 = 0;
            long time2 = 0;
            long time3 = 0;
            long time4 = 0;
            long time5 = 0;

            _stopWatch.Start();
            time1 = _stopWatch.ElapsedMilliseconds;
            time2 = _stopWatch.ElapsedMilliseconds;
            time3 = _stopWatch.ElapsedMilliseconds;
            time4 = _stopWatch.ElapsedMilliseconds;
            time5 = _stopWatch.ElapsedMilliseconds;
            _stopWatch.Stop();

            return time1 + time2 + time3 + time4 + time5;
        }

        [Benchmark]
        public long WithBranches()
        {
            long time1 = 0;
            long time2 = 0;
            long time3 = 0;
            long time4 = 0;
            long time5 = 0;

            if (Enabled)
                _stopWatch.Start();
            if (Enabled)
                time1 = _stopWatch.ElapsedMilliseconds;
            if (Enabled)
                time2 = _stopWatch.ElapsedMilliseconds;
            if (Enabled)
                time3 = _stopWatch.ElapsedMilliseconds;
            if (Enabled)
                time4 = _stopWatch.ElapsedMilliseconds;
            if (Enabled)
                time5 = _stopWatch.ElapsedMilliseconds;
            if (Enabled)
                _stopWatch.Stop();

            return time1 + time2 + time3 + time4 + time5;
        }

        [Benchmark]
        public long WithoutBranches()
        {
            long time1 = 0;
            long time2 = 0;
            long time3 = 0;
            long time4 = 0;
            long time5 = 0;

            _dummy.Start();
            time1 = _dummy.ElapsedMilliseconds;
            time2 = _dummy.ElapsedMilliseconds;
            time3 = _dummy.ElapsedMilliseconds;
            time4 = _dummy.ElapsedMilliseconds;
            time5 = _dummy.ElapsedMilliseconds;
            _dummy.Stop();

            return time1 + time2 + time3 + time4 + time5;
        }
    }
}
