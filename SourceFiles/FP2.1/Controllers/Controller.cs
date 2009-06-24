using System;
using FarseerGames.FarseerPhysics.Interfaces;

namespace FarseerGames.FarseerPhysics.Controllers
{
    /// <summary>
    /// Provides common functionality for controllers.
    /// </summary>
    public abstract class Controller : IIsDisposable
    {
        /// <summary>
        /// If false, this controller will not be processed/updated.
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// Gets or sets the tag. The Tag can contain a custom object.
        /// </summary>
        /// <Value>The tag.</Value>
        public Object Tag { get; set; }

        #region IDisposable Members

        private bool _isDisposed;

        public bool IsDisposed
        {
            get { return _isDisposed; }
            set { _isDisposed = value; }
        }

        protected virtual void Dispose(bool disposing)
        {
            IsDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Validates this instance. 
        /// </summary>
        public abstract void Validate();

        /// <summary>
        /// Updates this instance.
        /// </summary>
        /// <param name="dt">The time since last update.</param>
        /// <param name="dtReal">The real time since last update.</param>
        public abstract void Update(float dt, float dtReal);
    }
}