using Genbox.VelcroPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Definitions
{
    public class BodyDef : IDef
    {
        public BodyDef()
        {
            SetDefaults();
        }

        /// <summary>Is this body initially awake or sleeping?</summary>
        public bool Awake { get; set; }

        /// <summary>Does this body start out active?</summary>
        public bool Enabled { get; set; }

        /// <summary>Is this a fast moving body that should be prevented from tunneling through other moving bodies? Note that all
        /// bodies are prevented from tunneling through kinematic and static bodies. This setting is only considered on dynamic
        /// bodies.
        /// <remarks>Warning: You should use this flag sparingly since it increases processing time.</remarks>
        /// </summary>
        public bool IsBullet { get; set; }

        /// <summary>Set this flag to false if this body should never fall asleep.
        /// <remarks>Note: Setting this to false increases CPU usage.</remarks>
        /// </summary>
        public bool AllowSleep { get; set; }

        /// <summary>The world angle of the body in radians.</summary>
        public float Angle { get; set; }

        /// <summary>Angular damping is use to reduce the angular velocity. The damping parameter can be larger than 1.0f but the
        /// damping effect becomes sensitive to the time step when the damping parameter is large.</summary>
        public float AngularDamping { get; set; }

        /// <summary>The angular velocity of the body.</summary>
        public float AngularVelocity { get; set; }

        /// <summary>Scale the gravity applied to this body.</summary>
        public float GravityScale { get; set; }

        /// <summary>Linear damping is use to reduce the linear velocity. The damping parameter can be larger than 1.0f but the
        /// damping effect becomes sensitive to the time step when the damping parameter is large.</summary>
        public float LinearDamping { get; set; }

        /// <summary>The linear velocity of the body's origin in world co-ordinates.</summary>
        public Vector2 LinearVelocity { get; set; }

        /// <summary>The world position of the body.</summary>
        public Vector2 Position { get; set; }

        /// <summary>Set the type of body
        /// <remarks>Note: if a dynamic body would have zero mass, the mass is set to one.</remarks>
        /// </summary>
        public BodyType Type { get; set; }

        /// <summary>Use this to store application specific body data.</summary>
        public object UserData { get; set; }

        /// <summary>Should this body be prevented from rotating? Useful for characters.</summary>
        public bool FixedRotation { get; set; }

        public void SetDefaults()
        {
            Position = Vector2.Zero;
            Angle = 0.0f;
            LinearVelocity = Vector2.Zero;
            AngularVelocity = 0.0f;
            LinearDamping = 0.0f;
            AngularDamping = 0.0f;
            AllowSleep = true;
            Awake = true;
            FixedRotation = false;
            IsBullet = false;
            Type = BodyType.Static;
            Enabled = true;
            GravityScale = 1.0f;
        }
    }
}