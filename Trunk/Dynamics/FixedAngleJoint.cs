using System;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public class FixedAngleJoint : Joint
    {
        private float biasFactor = .2f;
        protected Body body;
        private float breakpoint = float.MaxValue;

        private float jointError;
        private float massFactor;
        private float maxImpulse = float.MaxValue;
        private float softness;
        private float targetAngle;
        private float velocityBias;

        public FixedAngleJoint()
        {
        }

        public FixedAngleJoint(Body body)
        {
            this.body = body;
        }

        public FixedAngleJoint(Body body, float targetAngle)
        {
            this.body = body;
            this.targetAngle = targetAngle;
        }

        public Body Body
        {
            get { return body; }
            set { body = value; }
        }

        public float BiasFactor
        {
            get { return biasFactor; }
            set { biasFactor = value; }
        }

        public float TargetAngle
        {
            get { return targetAngle; }
            set { targetAngle = value; }
        }

        public float Softness
        {
            get { return softness; }
            set { softness = value; }
        }

        public float MaxImpulse
        {
            get { return maxImpulse; }
            set { maxImpulse = value; }
        }

        public float Breakpoint
        {
            get { return breakpoint; }
            set { breakpoint = value; }
        }

        public float JointError
        {
            get { return jointError; }
        }

        public event EventHandler<EventArgs> Broke;

        public override void Validate()
        {
            if (body.IsDisposed)
            {
                Dispose();
            }
        }

        public override void PreStep(float inverseDt)
        {
            if (Enabled && Math.Abs(jointError) > breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }
            if (isDisposed)
            {
                return;
            }
            jointError = body.totalRotation - targetAngle;

            velocityBias = -biasFactor*inverseDt*jointError;
            massFactor = (1 - softness)/(body.inverseMomentOfInertia);
        }

        public override void Update()
        {
            if (isDisposed)
            {
                return;
            }
            float angularImpulse;
            angularImpulse = (velocityBias - body.angularVelocity)*massFactor;
            body.angularVelocity += body.inverseMomentOfInertia*Math.Sign(angularImpulse)*
                                    Math.Min(Math.Abs(angularImpulse), maxImpulse);
        }
    }
}