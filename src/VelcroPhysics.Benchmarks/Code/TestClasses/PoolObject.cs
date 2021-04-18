using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Shared.Optimization;

namespace Genbox.VelcroPhysics.Benchmarks.Code.TestClasses
{
    internal sealed class PoolObject : IPoolable<PoolObject>
    {
        public string TestString { get; set; }
        public int TestInteger { get; set; }

        public void Dispose()
        {
            Pool.ReturnToPool(this);
        }

        public Pool<PoolObject> Pool { get; set; }

        public void Reset() { }
    }
}