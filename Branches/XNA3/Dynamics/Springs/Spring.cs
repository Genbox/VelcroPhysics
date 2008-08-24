using System;

namespace FarseerGames.FarseerPhysics.Dynamics.Springs
{
    public abstract class Spring : DynamicsBase
    {
        public float SpringConstant { get; set; }
        public float DampningConstant { get; set; }
        public event EventHandler<EventArgs> Broke;

        public virtual void Update(float dt)
        {
            if (Enabled && Math.Abs(Error) > Breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }
        }
    }
}