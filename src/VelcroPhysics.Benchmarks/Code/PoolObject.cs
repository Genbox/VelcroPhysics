using VelcroPhysics.Shared;
using VelcroPhysics.Shared.Optimization;

namespace VelcroPhysics.Benchmarks.Code
{
    public class PoolObject : IPoolable<PoolObject>
    {
        public string TestString { get; set; }
        public int TestInteger { get; set; }

        public void Dispose()
        {
            Pool.ReturnToPool(this);
        }

        public Pool<PoolObject> Pool { get; set; }

        public void Reset()
        {

        }
    }
}
