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
        private float _jointError;
        private float _breakpoint = float.MaxValue;
        private float _softness;
        private float _biasFactor = .2f;

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public Object Tag { get; set; }

        public float JointError
        {
            get { return _jointError; }
            protected set { _jointError = value; }
        }

        public float Breakpoint
        {
            get { return _breakpoint; }
            set { _breakpoint = value; }
        }

        public float Softness
        {
            get { return _softness; }
            set { _softness = value; }
        }

        public float BiasFactor
        {
            get { return _biasFactor; }
            set { _biasFactor = value; }
        }

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