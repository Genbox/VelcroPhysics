using Microsoft.Xna.Framework;
using VelcroPhysics.Dynamics.Joints;

namespace VelcroPhysics.Templates.Joints
{
    /// <summary>
    /// Revolute joint definition. This requires defining an
    /// anchor point where the bodies are joined. The definition
    /// uses local anchor points so that the initial configuration
    /// can violate the constraint slightly. You also need to
    /// specify the initial relative angle for joint limits. This
    /// helps when saving and loading a game.
    /// The local anchor points are measured from the body's origin
    /// rather than the center of mass because:
    /// 1. you might not know where the center of mass will be.
    /// 2. if you add/remove shapes from a body and recompute the mass,
    /// the joints will be broken.
    /// </summary>
    public class RevoluteJointTemplate : JointTemplate
    {
        public RevoluteJointTemplate() : base(JointType.Revolute) { }

        /// <summary>
        /// A flag to enable joint limits.
        /// </summary>
        public bool EnableLimit { get; set; }

        /// <summary>
        /// A flag to enable the joint motor.
        /// </summary>
        public bool EnableMotor { get; set; }

        /// <summary>
        /// The local anchor point relative to bodyA's origin.
        /// </summary>
        public Vector2 LocalAnchorA { get; set; }

        /// <summary>
        /// The local anchor point relative to bodyB's origin.
        /// </summary>
        public Vector2 LocalAnchorB { get; set; }

        /// <summary>
        /// The lower angle for the joint limit (radians).
        /// </summary>
        public float LowerAngle { get; set; }

        /// <summary>
        /// The maximum motor torque used to achieve the desired motor speed. Usually in N-m.
        /// </summary>
        public float MaxMotorTorque { get; set; }

        /// <summary>
        /// The desired motor speed. Usually in radians per second.
        /// </summary>
        public float MotorSpeed { get; set; }

        /// <summary>
        /// The bodyB angle minus bodyA angle in the reference state (radians).
        /// </summary>
        public float ReferenceAngle { get; set; }

        /// <summary>
        /// The upper angle for the joint limit (radians).
        /// </summary>
        public float UpperAngle { get; set; }
    }
}