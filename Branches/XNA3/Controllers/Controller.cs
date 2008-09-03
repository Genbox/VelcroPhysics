using System;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public abstract class Controller
    {
        protected bool isDisposed;
        protected bool isEnabled = true;

        public bool Enabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }

        private object _tag;
        public Object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public bool IsDisposed
        {
            get { return isDisposed; }
        }

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
            if (!isDisposed)
            {
                if (disposing)
                {
                    //dispose managed resources
                }
                //dispose unmanaged resources
            }
            isDisposed = true;
            //base.Dispose(disposing)        
        }
    }
}