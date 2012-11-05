namespace FarseerPhysics.Physics.Collisions
{
    public class FluidCollisionProperties
    {
        /// <summary>
        /// Slip is the amount of friction between the collision and the fluid
        /// </summary>
        public float Slip;

        /// <summary>
        /// Toggles the sticky force for this collision
        /// </summary>
        public bool IsSticky;

        /// <summary>
        /// Distance from which sticky force acts
        /// </summary>
        public float StickDistance;

        /// <summary>
        /// Force used for gettings particles to stick to the surface
        /// </summary>
        public float StickForce;
    }
}