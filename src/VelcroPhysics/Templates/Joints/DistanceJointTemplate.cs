using Microsoft.Xna.Framework;
using VelcroPhysics.Dynamics.Joints;

namespace VelcroPhysics.Templates.Joints
{
    /// <summary>
    /// Distance joint definition. This requires defining an
    /// anchor point on both bodies and the non-zero length of the
    /// distance joint. The definition uses local anchor points
    /// so that the initial configuration can violate the constraint
    /// slightly. This helps when saving and loading a game.
    /// <remarks>Do not use a zero or a short length.</remarks>
    /// </summary>
    public class DistanceJointTemplate : JointTemplate
    {
        public DistanceJointTemplate() : base(JointType.Distance) { }

        /// <summary>
        /// The damping ratio. 0 = no damping, 1 = critical damping.
        /// </summary>
        public float DampingRatio { get; set; }

        /// <summary>
        /// The mass-spring-damper frequency in Hertz. A value of 0 disables softness.
        /// </summary>
        public float FrequencyHz { get; set; }

        /// <summary>
        /// The natural length between the anchor points.
        /// </summary>
        public float Length { get; set; }

        /// <summary>
        /// The local anchor point relative to bodyA's origin.
        /// </summary>
        public Vector2 LocalAnchorA { get; set; }

        /// <summary>
        /// The local anchor point relative to bodyB's origin.
        /// </summary>
        public Vector2 LocalAnchorB { get; set; }

        public override void SetDefaults()
        {
            Length = 1.0f;
        }
    }
}