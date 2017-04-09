using System;

namespace VelcroPhysics.Controllers
{
    [Flags]
    public enum ControllerType
    {
        GravityController = (1 << 0),
        VelocityLimitController = (1 << 1),
        AbstractForceController = (1 << 2),
        BuoyancyController = (1 << 3)
    }
}