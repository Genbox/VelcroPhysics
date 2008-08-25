using System;
using FarseerGames.FarseerPhysics.Controllers;

namespace FarseerGames.FarseerPhysics.Dynamics.Springs
{
    public class FixedAngleSpring : Controller
    {
        public FixedAngleSpring()
        {
            TorqueMultiplier = 1f;
            MaxTorque = float.MaxValue;
            Breakpoint = float.MaxValue;
        }

        public FixedAngleSpring(Body body, float springConstant, float dampningConstant)
        {
            TorqueMultiplier = 1f;
            MaxTorque = float.MaxValue;
            Breakpoint = float.MaxValue;
            Body = body;
            SpringConstant = springConstant;
            DampningConstant = dampningConstant;
            TargetAngle = body.TotalRotation;
        }

        public Body Body { get; set; }
        public float SpringConstant { get; set; }
        public float DampningConstant { get; set; }
        public float TargetAngle { get; set; }
        public float Breakpoint { get; set; }
        public float MaxTorque { get; set; }

        /// <summary>
        /// The resultant torque will be multiplied by this value prior to being applied to the bodies.
        /// For normal spring behavior this value should be 1
        /// </summary>
        public float TorqueMultiplier { get; set; }

        public float SpringError { get; private set; }
        public event EventHandler<EventArgs> Broke;

        public override void Validate()
        {
            //if body is disposed then dispose the joint.
            if (Body.IsDisposed)
            {
                Dispose();
            }
        }

        public override void Update(float dt)
        {
            if (Enabled && Math.Abs(SpringError) > Breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }
            if (IsDisposed)
            {
                return;
            }
            //calculate and apply spring force
            float angleDifference = TargetAngle - Body.TotalRotation;
            float springTorque = SpringConstant*angleDifference;
            SpringError = angleDifference;

            //apply torque at anchor
            if (!Body.IsStatic)
            {
                float torque1 = springTorque - DampningConstant*Body.angularVelocity;
                torque1 = Math.Min(Math.Abs(torque1*TorqueMultiplier), MaxTorque)*Math.Sign(torque1);
                Body.ApplyTorque(torque1);
            }
        }
    }
}