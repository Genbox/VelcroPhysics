using System;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    /// <summary>
    /// Provides common functionality for joints.
    /// </summary>
    public abstract class Joint : IDisposable
    {
        //TODO: Use public fields instead to decrease method calls
        public float BiasFactor = .2f;
        public float Breakpoint = float.MaxValue;
        public bool Enabled = true;
        private float _jointError;
        public float Softness;
        public bool IsDisposed;


        public Object Tag { get; set; }

        public float JointError
        {
            get { return _jointError; }
            protected set { _jointError = value; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            IsDisposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public abstract void Validate();
        public abstract void PreStep(float inverseDt);
        public abstract void Update();

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}