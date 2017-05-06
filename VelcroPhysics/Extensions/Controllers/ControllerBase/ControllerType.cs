using System;

namespace VelcroPhysics.Extensions.Controllers.ControllerBase
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