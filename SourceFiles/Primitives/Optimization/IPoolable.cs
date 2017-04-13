using System;

namespace VelcroPhysics.Primitives.Optimization
{
    public interface IPoolable : IDisposable
    {
        void Reset();
    }
}
