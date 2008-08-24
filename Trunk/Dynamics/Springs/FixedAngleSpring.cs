using System;

namespace FarseerGames.FarseerPhysics.Dynamics.Springs
{
    public class FixedAngleSpring : Spring
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
        public float TargetAngle { get; set; }
        public float MaxTorque { get; set; }

        /// <summary>
        /// The resultant torque will be multiplied by this value prior to being applied to the bodies.
        /// For normal spring behavior this value should be 1
        /// </summary>
        public float TorqueMultiplier { get; set; }

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
            base.Update(dt);

            if (IsDisposed) return;

            //calculate and apply spring force
            float angleDifference = TargetAngle - Body.TotalRotation;
            float springTorque = SpringConstant*angleDifference;
            Error = angleDifference;

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