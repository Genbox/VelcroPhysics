using VelcroPhysics.Primitives.Optimization;
using VelcroPhysics.Utils;

namespace Benchmarks.TestObjects
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
