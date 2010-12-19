using System;
using FarseerPhysics.Dynamics;

namespace FarseerPhysics.Controllers
{
    [Flags]
    public enum ControllerType
    {
        GravityController = (1 << 0),
        VelocityLimitController = (1 << 1),
        AbstractForceController = (1 << 2)
    }

    public class FilterControllerData : FilterData
    {
        private ControllerType _type;

        public FilterControllerData(ControllerType type)
        {
            _type = type;
        }

        public override bool IsActiveOn(Body body)
        {
            if (body.ControllerFilter.IsControllerIgnored(_type))
                return false;

            return base.IsActiveOn(body);
        }
    }

    public class ControllerFilter
    {
        public ControllerType ControllerIgnores;

        /// <summary>
        /// Ignores the controller. The controller has no effect on this body.
        /// </summary>
        /// <param name="controller">The flags.</param>
        public void IgnoreController(ControllerType controller)
        {
            ControllerIgnores |= controller;
        }

        /// <summary>
        /// Restore the controller. The controller affects this body.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public void RestoreController(ControllerType controller)
        {
            ControllerIgnores &= ~controller;
        }

        /// <summary>
        /// Determines whether this body ignores the the specified controller.
        /// </summary>
        /// <param name="controller">The flags.</param>
        /// <returns>
        /// 	<c>true</c> if the body has the specified flag; otherwise, <c>false</c>.
        /// </returns>
        public bool IsControllerIgnored(ControllerType controller)
        {
            return (ControllerIgnores & controller) == controller;
        }
    }

    public abstract class Controller
    {
        public FilterControllerData FilterData;

        public bool Enabled;

        public World World;

        public Controller(ControllerType controllerType)
        {
            FilterData = new FilterControllerData(controllerType);
        }

        public abstract void Update(float dt);
    }
}