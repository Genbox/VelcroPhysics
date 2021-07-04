using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints.Misc;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Definitions.Joints
{
    /// <summary>Wheel joint definition. This requires defining a line of motion using an axis and an anchor point. The
    /// definition uses local anchor points and a local axis so that the initial configuration can violate the constraint
    /// slightly. The joint translation is zero when the local anchor points coincide in world space. Using local anchors and a
    /// local axis helps when saving and loading a game.</summary>
    public sealed class WheelJointDef : JointDef
    {
        public WheelJointDef() : base(JointType.Wheel)
        {
            SetDefaults();
        }

        /// <summary>Enable/disable the joint motor.</summary>
        public bool EnableMotor { get; set; }

        /// <summary>The local anchor point relative to bodyA's origin.</summary>
        public Vector2 LocalAnchorA { get; set; }

        /// <summary>The local anchor point relative to bodyB's origin.</summary>
        public Vector2 LocalAnchorB { get; set; }

        /// <summary>The local translation axis in bodyA.</summary>
        public Vector2 LocalAxisA { get; set; }

        /// <summary>The maximum motor torque, usually in N-m.</summary>
        public float MaxMotorTorque { get; set; }

        /// <summary>The desired motor speed in radians per second.</summary>
        public float MotorSpeed { get; set; }

        /// <summary>Suspension damping. Typically in units of N*s/m.</summary>
        public float Damping { get; set; }

        /// <summary>Suspension stiffness. Typically in units N/m.</summary>
        public float Stiffness { get; set; }

        /// <summary>Enable/disable the joint limit.</summary>
        public bool EnableLimit { get; set; }

        /// <summary>The upper translation limit, usually in meters.</summary>
        public float UpperTranslation { get; set; }

        /// <summary>The lower translation limit, usually in meters.</summary>
        public float LowerTranslation { get; set; }

        public void Initialize(Body bA, Body bB, Vector2 anchor, Vector2 axis)
        {
            BodyA = bA;
            BodyB = bB;
            LocalAnchorA = BodyA.GetLocalPoint(anchor);
            LocalAnchorB = BodyB.GetLocalPoint(anchor);
            LocalAxisA = BodyA.GetLocalVector(axis);
        }

        public override void SetDefaults()
        {
            LocalAnchorA = Vector2.Zero;
            LocalAnchorB = Vector2.Zero;
            LocalAxisA = new Vector2(1.0f, 0.0f);
            EnableLimit = false;
            LowerTranslation = 0.0f;
            UpperTranslation = 0.0f;
            EnableMotor = false;
            MaxMotorTorque = 0.0f;
            MotorSpeed = 0.0f;
            Stiffness = 0.0f;
            Damping = 0.0f;
        }
    }
}