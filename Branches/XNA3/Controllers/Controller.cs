using System;

namespace FarseerGames.FarseerPhysics.Controllers
{
    public abstract class Controller
    {
        protected Controller()
        {
            Enabled = true;
        }

        public bool Enabled { get; set; }
        public Object Tag { get; set; }
        public bool IsDisposed { get; protected set; }

        public abstract void Validate();
        public abstract void Update(float dt);

        public virtual void PreStep(float dt)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
                IsDisposed = true;
        }
    }
}