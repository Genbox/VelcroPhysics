using System;
using VelcroPhysics.Utils;

namespace VelcroPhysics.Primitives.Optimization
{
    public interface IPoolable<T> : IDisposable where T : IPoolable<T>
    {
        void Reset();

        Pool<T> Pool { set; }
    }
}
