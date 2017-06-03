using VelcroPhysics.Dynamics.Joints;

namespace VelcroPhysics.Templates.Joints
{
    public class GearJointTemplate : JointTemplate
    {
        public GearJointTemplate() : base(JointType.Gear) { }

        /// <summary>
        /// The first revolute/prismatic joint attached to the gear joint.
        /// </summary>
        public Joint JointA { get; set; }

        /// <summary>
        /// The second revolute/prismatic joint attached to the gear joint.
        /// </summary>
        public Joint JointB { get; set; }

        /// <summary>
        /// The gear ratio.
        /// </summary>
        public float Ratio { get; set; }

        public override void SetDefaults()
        {
            Ratio = 1.0f;
        }
    }
}