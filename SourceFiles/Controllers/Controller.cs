using System;
using FarseerPhysics.Dynamics;

namespace FarseerPhysics.Controllers
{
    [Flags]
    public enum IgnoreController
    {
        GravityController = (1 << 0),
        VelocityLimitController = (1 << 1)

        //TODO: Add force controller
    }

    public abstract class Controller
    {
        public bool Enabled;

        public World World;

        public abstract void Update(float dt);
    }
}