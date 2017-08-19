namespace VelcroPhysics.Extensions.Controllers.ControllerBase
{
    public struct ControllerFilter
    {
        public ControllerType ControllerFlags;

        /// <summary>
        /// Ignores the controller. The controller has no effect on this body.
        /// </summary>
        /// <param name="controller">The controller type.</param>
        public void IgnoreController(ControllerType controller)
        {
            ControllerFlags |= controller;
        }

        /// <summary>
        /// Restore the controller. The controller affects this body.
        /// </summary>
        /// <param name="controller">The controller type.</param>
        public void RestoreController(ControllerType controller)
        {
            ControllerFlags &= ~controller;
        }

        /// <summary>
        /// Determines whether this body ignores the specified controller.
        /// </summary>
        /// <param name="controller">The controller type.</param>
        /// <returns>
        /// <c>true</c> if the body has the specified flag; otherwise, <c>false</c>.
        /// </returns>
        public bool IsControllerIgnored(ControllerType controller)
        {
            return (ControllerFlags & controller) == controller;
        }
    }
}