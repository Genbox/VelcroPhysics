using System;
using FarseerGames.FarseerPhysics.Interfaces;

namespace FarseerGames.FarseerPhysics.Dynamics.Springs
{
    /// <summary>
    /// Provides common functionality for springs.
    /// </summary>
    public abstract class Spring : IIsDisposable
    {
        /// <summary>
        /// The Breakpoint simply indicates the maximum Value the JointError can be before it breaks.
        /// The default value is float.MaxValue
        /// </summary>
        public float Breakpoint = float.MaxValue;

        /// <summary>
        /// The amount of spring damping to be applied.
        /// </summary>
        public float DampingConstant;

        /// <summary>
        /// Determines if the spring is enabled or not.
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// The amount of spring force to be applied.
        /// </summary>
        public float SpringConstant;

        /// <summary>
        /// Tag that can contain a user specified object.
        /// </summary>
        public Object Tag { get; set; }

        /// <summary>
        /// Gets or sets the spring error. The SpringError is a measure of how "broken" a spring is.
        /// When the SpringError is greater than the Breakpoint, the spring is automatically disabled.
        /// </summary>
        /// <Value>The spring error.</Value>
        public float SpringError { get; protected set; }

        /// <summary>
        /// Fires when the spring is broken.
        /// </summary>
        public event EventHandler<EventArgs> Broke;

        public abstract void Validate();

        public virtual void Update(float dt)
        {
            if (!Enabled || Math.Abs(SpringError) <= Breakpoint)
                return;

            Enabled = false;

            if (Broke != null)
                Broke(this, EventArgs.Empty);
        }

        protected virtual void Dispose(bool disposing)
        {
            IsDisposed = true;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IIsDisposable Members

        private bool _isDisposed;

        public bool IsDisposed
        {
            get { return _isDisposed; }
            set { _isDisposed = value; }
        }

        #endregion
    }
}