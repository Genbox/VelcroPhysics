using System;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    /// <summary>
    /// Provides common functionality for joints.
    /// </summary>
    public abstract class Joint : IDisposable
    {
        public bool Enabled = true;
        public bool IsDisposed;

        /// <summary>
        /// The Breakpoint simply indicates the maximum value the JointError can be before it breaks.
        /// </summary>
        public float Breakpoint = float.MaxValue;
        public float Softness;
        public float BiasFactor = .2f;

        /// <summary>
        /// Fires when the joint is broken.
        /// </summary>
        public event EventHandler<EventArgs> Broke;

        public Object Tag { get; set; }

        /// <summary>
        /// Gets the joint error. The JointError is a measure of how "broken" a joint is.
        /// When the JointError is greater than the Breakpoint, the joint is automatically disabled.
        /// </summary>
        /// <value>The joint error.</value>
        public float JointError { get; protected set; }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public abstract void Validate();
        public abstract void PreStep(float inverseDt);

        public virtual void Update()
        {
            if (!Enabled || Math.Abs(JointError) <= Breakpoint)
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