using VelcroPhysics.Shared;
using VelcroPhysics.Shared.Optimization;

namespace UnitTests.TestObjects
{
    internal class PoolObject : IPoolable<PoolObject>
    {
        public bool IsNew { get; private set; }

        public PoolObject()
        {
            IsNew = true;
        }

        public void Dispose()
        {
        }

        public void Reset()
        {
            IsNew = false;
        }

        public Pool<PoolObject> Pool { get; set; }
    }
}
