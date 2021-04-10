using Microsoft.Xna.Framework;
using VelcroPhysics.Dynamics.Joints;

namespace VelcroPhysics.Templates.Joints
{
    /// <summary>
    /// Weld joint definition. You need to specify local anchor points
    /// where they are attached and the relative body angle. The position
    /// of the anchor points is important for computing the reaction torque.
    /// </summary>
    public class WeldJointTemplate : JointTemplate
    {
        public WeldJointTemplate() : base(JointType.Weld) { }

        /// <summary>
        /// The damping ratio. 0 = no damping, 1 = critical damping.
        /// </summary>
        public float DampingRatio { get; set; }

        /// <summary>
        /// The mass-spring-damper frequency in Hertz. Rotation only.
        /// Disable softness with a value of 0.
        /// </summary>
        public float FrequencyHz { get; set; }

        /// <summary>
        /// The local anchor point relative to bodyA's origin.
        /// </summary>
        public Vector2 LocalAnchorA { get; set; }

        /// <summary>
        /// The local anchor point relative to bodyB's origin.
        /// </summary>
        public Vector2 LocalAnchorB { get; set; }

        /// <summary>
        /// The bodyB angle minus bodyA angle in the reference state (radians).
        /// </summary>
        public float ReferenceAngle { get; set; }
    }
}