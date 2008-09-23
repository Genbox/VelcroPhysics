using System;

namespace FarseerGames.FarseerPhysics.Controllers
{
    /// <summary>
    /// Provides the implementation that all controllers need.
    /// </summary>
    public abstract class Controller : IDisposable
    {
        public bool Enabled = true;
        public bool IsDisposed;

        public Object Tag { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public abstract void Validate();
        public abstract void Update(float dt);

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