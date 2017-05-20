using Microsoft.Xna.Framework;
using VelcroPhysics.Dynamics.Joints;

namespace VelcroPhysics.Templates.Joints
{
    /// <summary>
    /// Pulley joint definition. This requires two ground anchors,
    /// two dynamic body anchor points, and a pulley ratio.
    /// </summary>
    public class PulleyJointTemplate : JointTemplate
    {
        public PulleyJointTemplate() : base(JointType.Pulley) { }

        /// <summary>
        /// The first ground anchor in world coordinates. This point never moves.
        /// </summary>
        public Vector2 GroundAnchorA { get; set; }

        /// <summary>
        /// The second ground anchor in world coordinates. This point never moves.
        /// </summary>
        public Vector2 GroundAnchorB { get; set; }

        /// <summary>
        /// The a reference length for the segment attached to bodyA.
        /// </summary>
        public float LengthA { get; set; }

        /// <summary>
        /// The a reference length for the segment attached to bodyB.
        /// </summary>
        public float LengthB { get; set; }

        /// <summary>
        /// The local anchor point relative to bodyA's origin.
        /// </summary>
        public Vector2 LocalAnchorA { get; set; }

        /// <summary>
        /// The local anchor point relative to bodyB's origin.
        public Vector2 LocalAnchorB { get; set; }

        /// <summary>
        /// The pulley ratio, used to simulate a block-and-tackle.
        /// </summary>
        public float Ratio { get; set; }

        public override void SetDefaults()
        {
            GroundAnchorA = new Vector2(-1.0f, 1.0f);
            GroundAnchorB = new Vector2(1.0f, 1.0f);
            LocalAnchorA = new Vector2(-1.0f, 0.0f);
            LocalAnchorB = new Vector2(1.0f, 0.0f);
            Ratio = 1.0f;
            CollideConnected = true;
        }
    }
}