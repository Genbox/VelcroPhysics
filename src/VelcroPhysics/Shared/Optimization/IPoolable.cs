using System;

namespace Genbox.VelcroPhysics.Shared.Optimization
{
    public interface IPoolable<T> : IDisposable where T : IPoolable<T>
    {
        Pool<T> Pool { set; }
        void Reset();
    }
}