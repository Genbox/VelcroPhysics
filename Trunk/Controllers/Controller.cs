using System;

namespace FarseerGames.FarseerPhysics.Controllers
{
    /// <summary>
    /// Provides the implementation that all controllers need.
    /// </summary>
    public abstract class Controller : IDisposable
    {
        public bool IsDisposed;
        public bool Enabled = true;

        public Object Tag { get; set; }

        public abstract void Validate();
        public abstract void Update(float dt);

        public void Dispose()
        {
            IsDisposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

        }
    }
}