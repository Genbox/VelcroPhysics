using System;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Shared.Optimization;

namespace Genbox.VelcroPhysics.Benchmarks.Code
{
    public class PoolObject : IPoolable<PoolObject>
    {
        public string TestString { get; set; }
        public int TestInteger { get; set; }

        public void Dispose()
        {
            Pool.ReturnToPool(this);
            GC.SuppressFinalize(this);
        }

        public Pool<PoolObject> Pool { get; set; }

        public void Reset() { }
    }
}