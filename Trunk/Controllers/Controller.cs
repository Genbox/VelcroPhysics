using System;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public abstract class Controller : IIsDisposable
    {
        protected bool isEnabled = true;

        public bool Enabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }

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