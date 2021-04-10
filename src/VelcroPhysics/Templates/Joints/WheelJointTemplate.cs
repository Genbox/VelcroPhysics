using Microsoft.Xna.Framework;
using VelcroPhysics.Dynamics.Joints;

namespace VelcroPhysics.Templates.Joints
{
    /// <summary>
    /// Wheel joint definition. This requires defining a line of
    /// motion using an axis and an anchor point. The definition uses local
    /// anchor points and a local axis so that the initial configuration
    /// can violate the constraint slightly. The joint translation is zero
    /// when the local anchor points coincide in world space. Using local
    /// anchors and a local axis helps when saving and loading a game.
    /// </summary>
    public class WheelJointTemplate : JointTemplate
    {
        public WheelJointTemplate() : base(JointType.Wheel) { }

        /// <summary>
        /// Suspension damping ratio, one indicates critical damping
        /// </summary>
        public float DampingRatio { get; set; }

        /// <summary>
        /// Enable/disable the joint motor.
        /// </summary>
        public bool EnableMotor { get; set; }

        /// <summary>
        /// Suspension frequency, zero indicates no suspension
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
        /// The local translation axis in bodyA.
        /// </summary>
        public Vector2 LocalAxisA { get; set; }

        /// <summary>
        /// The maximum motor torque, usually in N-m.
        public float MaxMotorTorque { get; set; }

        /// <summary>
        /// The desired motor speed in radians per second.
        /// </summary>
        public float MotorSpeed { get; set; }

        public override void SetDefaults()
        {
            LocalAxisA = new Vector2(1.0f, 0.0f);
            FrequencyHz = 2.0f;
            DampingRatio = 0.7f;
        }
    }
}