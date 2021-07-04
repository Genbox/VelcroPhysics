using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints.Misc;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Definitions.Joints
{
    /// <summary>Weld joint definition. You need to specify local anchor points where they are attached and the relative body
    /// angle. The position of the anchor points is important for computing the reaction torque.</summary>
    public sealed class WeldJointDef : JointDef
    {
        public WeldJointDef() : base(JointType.Weld)
        {
            SetDefaults();
        }

        /// <summary>The rotational damping in N*m*s</summary>
        public float Damping { get; set; }

        /// <summary>The rotational stiffness in N*m. Disable softness with a value of 0</summary>
        public float Stiffness { get; set; }

        /// <summary>The local anchor point relative to bodyA's origin.</summary>
        public Vector2 LocalAnchorA { get; set; }

        /// <summary>The local anchor point relative to bodyB's origin.</summary>
        public Vector2 LocalAnchorB { get; set; }

        /// <summary>The bodyB angle minus bodyA angle in the reference state (radians).</summary>
        public float ReferenceAngle { get; set; }

        public void Initialize(Body bA, Body bB, Vector2 anchor)
        {
            BodyA = bA;
            BodyB = bB;
            LocalAnchorA = BodyA.GetLocalPoint(anchor);
            LocalAnchorB = BodyB.GetLocalPoint(anchor);
            ReferenceAngle = BodyB.Rotation - BodyA.Rotation;
        }

        public override void SetDefaults()
        {
            LocalAnchorA = Vector2.Zero;
            LocalAnchorB = Vector2.Zero;
            ReferenceAngle = 0.0f;
            Stiffness = 0.0f;
            Damping = 0.0f;

            base.SetDefaults();
        }
    }
}