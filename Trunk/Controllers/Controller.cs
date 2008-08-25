using System;

namespace FarseerGames.FarseerPhysics.Controllers
{
    public abstract class Controller : IIsDisposable
    {
        protected Controller()
        {
            Enabled = true;
        }

        public bool Enabled { get; set; }

        public Object Tag { get; set; }

        #region IIsDisposable Members

        public bool IsDisposed { get; protected set; }

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
            //base.Dispose(disposing)        
        }
    }
}