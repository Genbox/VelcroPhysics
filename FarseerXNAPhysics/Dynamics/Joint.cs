using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public abstract class Joint : IIsDisposable {
        protected Body _body1;
        protected Body _body2;

        public Body Body1 {
            get { return _body1; }
            set { _body1 = value; }
        }

        public Body Body2 {
            get { return _body2 ; }
            set { _body2  = value; }
        }	

        public abstract void PreStep(float inverseDt);
        public abstract void Update();

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
