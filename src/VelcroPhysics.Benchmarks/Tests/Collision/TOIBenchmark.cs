using BenchmarkDotNet.Attributes;
using Genbox.VelcroPhysics.Benchmarks.Code;
using Genbox.VelcroPhysics.Collision.Distance;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Collision.TOI;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Benchmarks.Tests.Collision
{
    public class ToiBenchmark : MeasuredBenchmark
    {
        private PolygonShape _shapeA;
        private PolygonShape _shapeB;
        private Sweep _sweepA;
        private Sweep _sweepB;

        [GlobalSetup]
        public void Setup()
        {
            _shapeA = new PolygonShape(PolygonUtils.CreateRectangle(25.0f, 5.0f), 0);
            _shapeB = new PolygonShape(PolygonUtils.CreateRectangle(2.5f, 2.5f), 0);

            _sweepA = new Sweep();
            _sweepA.C0 = new Vector2(24.0f, -60.0f);
            _sweepA.A0 = 2.95f;
            _sweepA.C = _sweepA.C0;
            _sweepA.A = _sweepA.A0;
            _sweepA.LocalCenter = Vector2.Zero;

            _sweepB = new Sweep();
            _sweepB.C0 = new Vector2(53.474274f, -50.252514f);
            _sweepB.A0 = 513.36676f;
            _sweepB.C = new Vector2(54.595478f, -51.083473f);
            _sweepB.A = 513.62781f;
            _sweepB.LocalCenter = Vector2.Zero;
        }

        [Benchmark]
        public void Distance()
        {
            TOIInput input = new TOIInput();
            input.ProxyA = new DistanceProxy(_shapeA, 0);
            input.ProxyB = new DistanceProxy(_shapeB, 0);
            input.SweepA = _sweepA;
            input.SweepB = _sweepB;
            input.TMax = 1.0f;

            TimeOfImpact.CalculateTimeOfImpact(ref input, out _);
        }
    }
}