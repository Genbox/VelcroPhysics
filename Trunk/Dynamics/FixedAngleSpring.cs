using System;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public class FixedAngleSpring : Controller
    {
        protected Body body;

        private float breakpoint = float.MaxValue;
        private float dampningConstant;
        private float maxTorque = float.MaxValue;
        private float springConstant;

        private float springError;
        private float targetAngle;
        private float torqueMultiplier = 1f;

        public FixedAngleSpring()
        {
        }

        public FixedAngleSpring(Body body, float springConstant, float dampningConstant)
        {
            this.body = body;
            this.springConstant = springConstant;
            this.dampningConstant = dampningConstant;
            targetAngle = body.TotalRotation;
        }

        public Body Body
        {
            get { return body; }
            set { body = value; }
        }

        public float SpringConstant
        {
            get { return springConstant; }
            set { springConstant = value; }
        }

        public float DampningConstant
        {
            get { return dampningConstant; }
            set { dampningConstant = value; }
        }

        public float TargetAngle
        {
            get { return targetAngle; }
            set { targetAngle = value; }
        }

        public float Breakpoint
        {
            get { return breakpoint; }
            set { breakpoint = value; }
        }

        public float MaxTorque
        {
            get { return maxTorque; }
            set { maxTorque = value; }
        }

        /// <summary>
        /// The resultant torque will be multiplied by this value prior to being applied to the bodies.
        /// For normal spring behavior this value should be 1
        /// </summary>
        public float TorqueMultiplier
        {
            get { return torqueMultiplier; }
            set { torqueMultiplier = value; }
        }

        public float SpringError
        {
            get { return springError; }
        }

        public event EventHandler<EventArgs> Broke;

        public override void Validate()
        {
            //if body is disposed then dispose the joint.
            if (body.IsDisposed)
            {
                Dispose();
            }
        }

        public override void Update(float dt)
        {
            if (Enabled && Math.Abs(springError) > breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }
            if (isDisposed)
            {
                return;
            }
            //calculate and apply spring force
            float angleDifference = targetAngle - body.TotalRotation;
            float springTorque = springConstant*angleDifference;
            springError = angleDifference;

            //apply torque at anchor
            if (!body.IsStatic)
            {
                float torque1 = springTorque - dampningConstant*body.angularVelocity;
                torque1 = Math.Min(Math.Abs(torque1*torqueMultiplier), maxTorque)*Math.Sign(torque1);
                body.ApplyTorque(torque1);
            }
        }
    }
}