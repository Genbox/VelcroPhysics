using System.Diagnostics;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints.Misc;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Definitions.Joints
{
    /// <summary>Pulley joint definition. This requires two ground anchors, two dynamic body anchor points, and a pulley ratio.</summary>
    public sealed class PulleyJointDef : JointDef
    {
        public PulleyJointDef() : base(JointType.Pulley)
        {
            SetDefaults();
        }

        /// <summary>The first ground anchor in world coordinates. This point never moves.</summary>
        public Vector2 GroundAnchorA { get; set; }

        /// <summary>The second ground anchor in world coordinates. This point never moves.</summary>
        public Vector2 GroundAnchorB { get; set; }

        /// <summary>The a reference length for the segment attached to bodyA.</summary>
        public float LengthA { get; set; }

        /// <summary>The a reference length for the segment attached to bodyB.</summary>
        public float LengthB { get; set; }

        /// <summary>The local anchor point relative to bodyA's origin.</summary>
        public Vector2 LocalAnchorA { get; set; }

        /// <summary>The local anchor point relative to bodyB's origin.</summary>
        public Vector2 LocalAnchorB { get; set; }

        /// <summary>The pulley ratio, used to simulate a block-and-tackle.</summary>
        public float Ratio { get; set; }

        public void Initialize(Body bA, Body bB, Vector2 groundA, Vector2 groundB, Vector2 anchorA, Vector2 anchorB, float r)
        {
            BodyA = bA;
            BodyB = bB;
            GroundAnchorA = groundA;
            GroundAnchorB = groundB;
            LocalAnchorA = BodyA.GetLocalPoint(anchorA);
            LocalAnchorB = BodyB.GetLocalPoint(anchorB);
            Vector2 dA = anchorA - groundA;
            LengthA = dA.Length();
            Vector2 dB = anchorB - groundB;
            LengthB = dB.Length();
            Ratio = r;
            Debug.Assert(Ratio > MathConstants.Epsilon);
        }

        public override void SetDefaults()
        {
            GroundAnchorA = new Vector2(-1.0f, 1.0f);
            GroundAnchorB = new Vector2(1.0f, 1.0f);
            LocalAnchorA = new Vector2(-1.0f, 0.0f);
            LocalAnchorB = new Vector2(1.0f, 0.0f);
            LengthA = 0.0f;
            LengthB = 0.0f;
            Ratio = 1.0f;
            CollideConnected = true;
        }
    }
}