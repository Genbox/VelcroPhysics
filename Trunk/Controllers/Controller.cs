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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            //subclasses can override incase they need to dispose of resources
            //otherwise do nothing.
            if (!IsDisposed)
            {
                if (disposing)
                {
                    //dispose managed resources 
                }

                //dispose unmanaged resources
            }
            IsDisposed = true;
        }
    }
}