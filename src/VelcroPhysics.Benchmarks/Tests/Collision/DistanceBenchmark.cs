using BenchmarkDotNet.Attributes;
using Genbox.VelcroPhysics.Benchmarks.Code;
using Genbox.VelcroPhysics.Collision.Distance;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Benchmarks.Tests.Collision
{
    public class DistanceBenchmark : MeasuredBenchmark
    {
        private PolygonShape _polygonA;
        private PolygonShape _polygonB;
        private Transform _transformA;
        private Transform _transformB;

        [GlobalSetup]
        public void Setup()
        {
            _transformA.SetIdentity();
            _transformA.p = new Vector2(0.0f, -0.2f);
            _polygonA = new PolygonShape(PolygonUtils.CreateRectangle(10.0f, 0.2f), 0);

            _transformB.Set(new Vector2(12.017401f, 0.13678508f), -0.0109265f);
            _polygonB = new PolygonShape(PolygonUtils.CreateRectangle(2.0f, 0.1f), 0);
        }

        [Benchmark]
        public void Distance()
        {
            DistanceInput input = new DistanceInput();
            input.ProxyA = new DistanceProxy(_polygonA, 0);
            input.ProxyB = new DistanceProxy(_polygonB, 0);
            input.TransformA = _transformA;
            input.TransformB = _transformB;
            input.UseRadii = true;
            DistanceGJK.ComputeDistance(ref input, out _, out _);
        }
    }
}