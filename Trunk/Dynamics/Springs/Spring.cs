using System;

namespace FarseerGames.FarseerPhysics.Dynamics.Springs
{
    /// <summary>
    /// Provides common functionality for springs.
    /// </summary>
    public abstract class Spring : IDisposable
    {
        /// <summary>
        /// The Breakpoint simply indicates the maximum Value the JointError can be before it breaks.
        /// </summary>
        public float Breakpoint = float.MaxValue;

        public float DampingConstant;
        public bool Enabled = true;
        public bool IsDisposed;
        public float SpringConstant;

        public Object Tag { get; set; }

        /// <summary>
        /// Gets or sets the spring error. The SpringError is a measure of how "broken" a spring is.
        /// When the SpringError is greater than the Breakpoint, the spring is automatically disabled.
        /// </summary>
        /// <Value>The spring error.</Value>
        public float SpringError { get; protected set; }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Fires when the spring is broken.
        /// </summary>
        public event EventHandler<EventArgs> Broke;

        public abstract void Validate();

        public virtual void Update(float dt)
        {
            //TODO: Ehhh, this makes no sense. Please test breakability!
            if (!Enabled || Math.Abs(SpringError) <= Breakpoint)
                return;

            Enabled = false;

            if (Broke != null)
                Broke(this, EventArgs.Empty);
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