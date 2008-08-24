using System;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public abstract class DynamicsBase : IIsDisposable
    {
        protected DynamicsBase()
        {
            Enabled = true;
        }

        public bool Enabled { get; set; }
        public float Breakpoint { get; set; }
        public float Error { get; set; }
        public float BiasFactor { get; set; }
        public Object Tag { get; set; }

        #region IIsDisposable Members

        public bool IsDisposed { get; protected set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public abstract void Validate();

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
                IsDisposed = true;
        }
    }
}