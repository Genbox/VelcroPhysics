using BenchmarkDotNet.Attributes;
using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.Narrowphase;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Shared;
using VelcroPhysics.Utilities;

namespace Benchmarks.Core.Collision
{
    [MemoryDiagnoser]
    public class DistanceBenchmark
    {
        private PolygonShape _polygonA;
        private PolygonShape _polygonB;
        private Transform _transformA;
        private Transform _transformB = new Transform();

        [Setup]
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
            input.ProxyA.Set(_polygonA, 0);
            input.ProxyB.Set(_polygonB, 0);
            input.TransformA = _transformA;
            input.TransformB = _transformB;
            input.UseRadii = true;
            VelcroPhysics.Collision.Narrowphase.Distance.ComputeDistance(out _, out _, input);
        }
    }
}
