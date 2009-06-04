using System;
using System.Xml.Serialization;
using FarseerGames.FarseerPhysics.Interfaces;

#if(XNA)
using Microsoft.Xna.Framework.Content;
#endif

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    /// <summary>
    /// Provides common functionality for joints.
    /// </summary>
    public abstract class Joint : IIsDisposable
    {
        public float BiasFactor = .2f;

        /// <summary>
        /// The Breakpoint simply indicates the maximum Value the JointError can be before it breaks.
        /// The default value is float.MaxValue
        /// </summary>
        public float Breakpoint = float.MaxValue;

        public bool Enabled = true;

        public float Softness;

#if(XNA)
        [ContentSerializerIgnore]
#endif
        [XmlIgnore]
        /// <summary>
        /// Tag that can contain a user specified object.
        /// </summary>
        public Object Tag { get; set; }

        /// <summary>
        /// Gets the joint error. The JointError is a measure of how "broken" a joint is.
        /// When the JointError is greater than the Breakpoint, the joint is automatically disabled.
        /// </summary>
        /// <Value>The joint error.</Value>
        public float JointError { get; protected set; }

        /// <summary>
        /// Fires when the joint is broken.
        /// </summary>
        public event EventHandler<EventArgs> Broke;

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
            IsDisposed = true;
        }

        #region IDisposable Members

        private bool _isDisposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IIsDisposable Members

#if(XNA)
        [ContentSerializerIgnore]
#endif
        [XmlIgnore]
        public bool IsDisposed
        {
            get { return _isDisposed; }
            set { _isDisposed = value; }
        }

        #endregion
    }
}