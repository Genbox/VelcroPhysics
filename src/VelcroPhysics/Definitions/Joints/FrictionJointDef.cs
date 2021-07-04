using Genbox.VelcroPhysics.Dynamics.Joints.Misc;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Definitions.Joints
{
    public sealed class FrictionJointDef : JointDef
    {
        public FrictionJointDef() : base(JointType.Friction)
        {
            SetDefaults();
        }

        /// <summary>The local anchor point relative to bodyA's origin.</summary>
        public Vector2 LocalAnchorA { get; set; }

        /// <summary>The local anchor point relative to bodyB's origin.</summary>
        public Vector2 LocalAnchorB { get; set; }

        /// <summary>The maximum friction force in N.</summary>
        public float MaxForce { get; set; }

        /// <summary>The maximum friction torque in N-m.</summary>
        public float MaxTorque { get; set; }

        public override void SetDefaults()
        {
            LocalAnchorA = Vector2.Zero;
            LocalAnchorB = Vector2.Zero;
            MaxForce = 0.0f;
            MaxTorque = 0.0f;

            base.SetDefaults();
        }
    }
}