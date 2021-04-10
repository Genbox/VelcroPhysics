using Microsoft.Xna.Framework;
using VelcroPhysics.Dynamics.Joints;

namespace VelcroPhysics.Templates.Joints
{
    /// <summary>
    /// Mouse joint definition. This requires a world target point,
    /// tuning parameters, and the time step.
    /// </summary>
    public class MouseJointTemplate : JointTemplate
    {
        public MouseJointTemplate() : base(JointType.FixedMouse) { }

        /// <summary>
        /// The damping ratio. 0 = no damping, 1 = critical damping.
        /// </summary>
        public float DampingRatio { get; set; }

        /// <summary>
        /// The response speed.
        /// </summary>
        public float FrequencyHz { get; set; }

        /// <summary>
        /// The maximum constraint force that can be exerted
        /// to move the candidate body. Usually you will express
        /// as some multiple of the weight (multiplier * mass * gravity).
        /// </summary>
        public float MaxForce { get; set; }

        /// <summary>
        /// The initial world target point. This is assumed
        /// to coincide with the body anchor initially.
        /// </summary>
        public Vector2 Target { get; set; }

        public override void SetDefaults()
        {
            FrequencyHz = 5.0f;
            DampingRatio = 0.7f;
        }
    }
}