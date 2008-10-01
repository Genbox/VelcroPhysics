using System;

namespace FarseerGames.FarseerPhysics.Dynamics.Springs
{
    /// <summary>
    /// Provides the implementation that all springs need.
    /// </summary>
    public abstract class Spring : IDisposable
    {
        public bool Enabled = true;
        public bool IsDisposed;
        public float Breakpoint = float.MaxValue;
        public float DampningConstant;
        public float SpringConstant;
        private float _springError;
        public event EventHandler<EventArgs> Broke;

        public Object Tag { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public abstract void Validate();

        public virtual void Update(float dt)
        {
            if (Enabled && Math.Abs(SpringError) > Breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }
        }

        public float SpringError
        {
            get { return _springError; }
            protected set { _springError = value; }
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