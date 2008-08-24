using System;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public abstract class Joint : IIsDisposable
    {
        private bool enabled = true;
        protected bool isDisposed;

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public Object Tag { get; set; }

        #region IIsDisposable Members

        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public abstract void Validate();
        public abstract void PreStep(float inverseDt);
        public abstract void Update();

        protected virtual void Dispose(bool disposing)
        {
            //subclasses can override incase they need to dispose of resources
            //otherwise do nothing.
            if (!isDisposed)
            {
                if (disposing)
                {
                }

                isDisposed = true;
            }
        }
    }
}