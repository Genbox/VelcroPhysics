using System;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    public abstract class Joint : DynamicsBase
    {
        public float Softness { get; set; }
        public event EventHandler<EventArgs> Broke;

        public virtual void PreStep(float inverseDt)
        {
            if (Enabled && Math.Abs(Error) > Breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }
        }

        public abstract void Update();
    }
}