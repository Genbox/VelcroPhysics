namespace VelcroPhysics.Extensions.PhysicsLogics.PhysicsLogicBase
{
    public struct PhysicsLogicFilter
    {
        public PhysicsLogicType ControllerIgnores;

        /// <summary>
        /// Ignores the controller. The controller has no effect on this body.
        /// </summary>
        /// <param name="type">The logic type.</param>
        public void IgnorePhysicsLogic(PhysicsLogicType type)
        {
            ControllerIgnores |= type;
        }

        /// <summary>
        /// Restore the controller. The controller affects this body.
        /// </summary>
        /// <param name="type">The logic type.</param>
        public void RestorePhysicsLogic(PhysicsLogicType type)
        {
            ControllerIgnores &= ~type;
        }

        /// <summary>
        /// Determines whether this body ignores the specified controller.
        /// </summary>
        /// <param name="type">The logic type.</param>
        /// <returns>
        /// <c>true</c> if the body has the specified flag; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPhysicsLogicIgnored(PhysicsLogicType type)
        {
            return (ControllerIgnores & type) == type;
        }
    }
}