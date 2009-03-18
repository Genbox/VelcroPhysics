using System;
using FarseerGames.FarseerPhysics.Interfaces;

namespace FarseerGames.FarseerPhysics.Controllers
{
    /// <summary>
    /// Provides common functionality for controllers.
    /// </summary>
    public abstract class Controller : IIsDisposable
    {
        public bool Enabled = true;
        private bool _isDisposed;

        /// <summary>
        /// Gets or sets the tag. The Tag can contain a custom object.
        /// </summary>
        /// <Value>The tag.</Value>
        public Object Tag { get; set; }

        #region IDisposable Members

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
        /// <param name="dt">The dt.</param>
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
        }

        public bool IsDisposed
        {
            get { return _isDisposed; }
            set { _isDisposed = value; }
        }
    }
}