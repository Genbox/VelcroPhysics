using System;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    public abstract class Joint : IIsDisposable
    {
        protected Joint()
        {
            Enabled = true;
        }

        public bool Enabled { get; set; }
        public float BiasFactor { get; set; }
        public float Softness { get; set; }
        public float Breakpoint { get; set; }
        public float JointError { get; set; }
        public Object Tag { get; set; }

        #region IIsDisposable Members

        public bool IsDisposed { get; protected set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public event EventHandler<EventArgs> Broke;

        public abstract void Validate();

        public virtual void PreStep(float inverseDt)
        {
            if (Enabled && Math.Abs(JointError) > Breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }
        }

        public abstract void Update();

        protected virtual void Dispose(bool disposing)
        {
            //subclasses can override incase they need to dispose of resources
            //otherwise do nothing.
            if (!IsDisposed)
            {
                if (disposing)
                {
                }

                IsDisposed = true;
            }
        }
    }
}