using VelcroPhysics.Dynamics;
using VelcroPhysics.Dynamics.Joints;

namespace VelcroPhysics.Templates.Joints
{
    public class JointTemplate : IDefaults
    {
        public JointTemplate(JointType type)
        {
            Type = type;
        }

        /// <summary>
        /// The first attached body.
        /// </summary>
        public Body BodyA { get; set; }

        /// <summary>
        /// The second attached body.
        /// </summary>
        public Body BodyB { get; set; }

        /// <summary>
        /// Set this flag to true if the attached bodies should collide.
        /// </summary>
        public bool CollideConnected { get; set; }

        /// <summary>
        /// The joint type is set automatically for concrete joint types.
        /// </summary>
        public JointType Type { get; }

        /// <summary>
        /// Use this to attach application specific data.
        /// </summary>
        public object UserData { get; set; }

        public virtual void SetDefaults() { }
    }
}