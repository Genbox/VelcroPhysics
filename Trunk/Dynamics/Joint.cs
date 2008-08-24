using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerPhysics.Dynamics {
    public abstract class Joint : IIsDisposable {
        private Object tag;
        private bool enabled = true;   

        public abstract void Validate();
        public abstract void PreStep(float inverseDt);
        public abstract void Update();

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public Joint() 
        { 
        }

        public Object Tag {
            get { return tag; }
            set { tag = value; }
        }	

        protected bool isDisposed = false;
        public bool IsDisposed {
            get { return isDisposed; }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            //subclasses can override incase they need to dispose of resources
            //otherwise do nothing.
            if (!isDisposed) {
                if (disposing) { };
                isDisposed = true;
            }
        }
    }
}
