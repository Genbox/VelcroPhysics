using VelcroPhysics.Shared;
using VelcroPhysics.Shared.Optimization;

namespace VelcroPhysics.Tests.Code
{
    internal class PoolObject : IPoolable<PoolObject>
    {
        public PoolObject()
        {
            IsNew = true;
        }

        public bool IsNew { get; private set; }

        public void Dispose() { }

        public void Reset()
        {
            IsNew = false;
        }

        public Pool<PoolObject> Pool { get; set; }
    }
}