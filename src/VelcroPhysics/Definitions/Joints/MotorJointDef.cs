using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints.Misc;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Definitions.Joints
{
    public sealed class MotorJointDef : JointDef
    {
        public MotorJointDef() : base(JointType.Motor)
        {
            SetDefaults();
        }

        /// <summary>The bodyB angle minus bodyA angle in radians.</summary>
        public float AngularOffset { get; set; }

        /// <summary>Position correction factor in the range [0,1].</summary>
        public float CorrectionFactor { get; set; }

        /// <summary>Position of bodyB minus the position of bodyA, in bodyA's frame, in meters.</summary>
        public Vector2 LinearOffset { get; set; }

        /// <summary>The maximum motor force in N.</summary>
        public float MaxForce { get; set; }

        /// <summary>The maximum motor torque in N-m.</summary>
        public float MaxTorque { get; set; }

        public void Initialize(Body bA, Body bB)
        {
            BodyA = bA;
            BodyB = bB;
            Vector2 xB = BodyB.Position;
            LinearOffset = BodyA.GetLocalPoint(xB);

            float angleA = BodyA.Rotation;
            float angleB = BodyB.Rotation;
            AngularOffset = angleB - angleA;
        }

        public override void SetDefaults()
        {
            LinearOffset = Vector2.Zero;
            AngularOffset = 0.0f;
            MaxForce = 1.0f;
            MaxTorque = 1.0f;
            CorrectionFactor = 0.3f;
        }
    }
}