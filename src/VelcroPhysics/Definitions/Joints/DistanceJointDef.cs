using Genbox.VelcroPhysics.Dynamics.Joints.Misc;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Templates.Joints
{
    /// <summary>Distance joint definition. This requires defining an anchor point on both bodies and the non-zero length of
    /// the distance joint. The definition uses local anchor points so that the initial configuration can violate the
    /// constraint slightly. This helps when saving and loading a game.
    /// <remarks>Do not use a zero or a short length.</remarks>
    /// </summary>
    public sealed class DistanceJointDef : JointDef
    {
        public DistanceJointDef() : base(JointType.Distance)
        {
            SetDefaults();
        }

        /// <summary>The linear damping in N*s/m.</summary>
        public float Damping { get; set; }

        /// <summary>The linear stiffness in N/m.</summary>
        public float Stiffness { get; set; }

        /// <summary>The rest length of this joint. Clamped to a stable minimum value.</summary>
        public float Length { get; set; }

        /// <summary>Minimum length. Clamped to a stable minimum value.</summary>
        public float MinLength { get; set; }

        /// <summary>Maximum length. Must be greater than or equal to the minimum length.</summary>
        public float MaxLength { get; set; }

        /// <summary>The local anchor point relative to bodyA's origin.</summary>
        public Vector2 LocalAnchorA { get; set; }

        /// <summary>The local anchor point relative to bodyB's origin.</summary>
        public Vector2 LocalAnchorB { get; set; }

        public override void SetDefaults()
        {
            LocalAnchorA = Vector2.Zero;
            LocalAnchorB = Vector2.Zero;
            Length = 1.0f;
            MinLength = 0.0f;
            MaxLength = float.MaxValue;
            Stiffness = 0.0f;
            Damping = 0.0f;
        }
    }
}