using System;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    /// <summary>
    /// Provides common functionality for joints.
    /// </summary>
    public abstract class Joint : IDisposable
    {
        private bool _enabled = true;
        public bool IsDisposed;

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public Object Tag { get; set; }

        public void Dispose()
        {
            IsDisposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract void Validate();
        public abstract void PreStep(float inverseDt);
        public abstract void Update();

        protected virtual void Dispose(bool disposing)
        {

        }
    }
}