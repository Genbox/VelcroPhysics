using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerPhysics.Dynamics {
    public abstract class Controller {
        private Object tag;
        public abstract void Validate();
        public abstract void Update(float dt);
        protected bool isEnabled = true;
        
        public bool Enabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
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
                if (disposing) {
                    //dispose managed resources
                };
                //dispose unmanaged resources
            }
            isDisposed = true;
            //base.Dispose(disposing)        
        }
    }
}
