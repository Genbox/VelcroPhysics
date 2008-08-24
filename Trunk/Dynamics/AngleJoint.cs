using System;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public class AngleJoint : Joint
    {
        //Note: Cleanup, variable never used
        //private bool enabled = true;

        private float massFactor;
        private float velocityBias;

        public AngleJoint()
        {
            Breakpoint = float.MaxValue;
            MaxImpulse = float.MaxValue;
            BiasFactor = .2f;
        }

        public AngleJoint(Body body1, Body body2)
        {
            Breakpoint = float.MaxValue;
            MaxImpulse = float.MaxValue;
            BiasFactor = .2f;
            Body1 = body1;
            Body2 = body2;
        }

        public AngleJoint(Body body1, Body body2, float targetAngle)
        {
            Breakpoint = float.MaxValue;
            MaxImpulse = float.MaxValue;
            BiasFactor = .2f;
            Body1 = body1;
            Body2 = body2;
            TargetAngle = targetAngle;
        }

        public Body Body1 { get; set; }
        public Body Body2 { get; set; }
        public float BiasFactor { get; set; }
        public float TargetAngle { get; set; }
        public float Softness { get; set; }
        public float MaxImpulse { get; set; }
        public float Breakpoint { get; set; }
        public float JointError { get; private set; }
        public event EventHandler<EventArgs> Broke;

        public override void Validate()
        {
            if (Body1.IsDisposed || Body2.IsDisposed)
            {
                Dispose();
            }
        }

        public override void PreStep(float inverseDt)
        {
            if (Enabled && Math.Abs(JointError) > Breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }

            if (isDisposed)
            {
                return;
            }
            JointError = (Body2.TotalRotation - Body1.TotalRotation) - TargetAngle;

            velocityBias = -BiasFactor*inverseDt*JointError;

            massFactor = (1 - Softness)/(Body1.inverseMomentOfInertia + Body2.inverseMomentOfInertia);
        }

        public override void Update()
        {
            if (isDisposed) return;
            if (!Enabled) return;
            float angularImpulse = (velocityBias - Body2.angularVelocity + Body1.angularVelocity)*massFactor;

            Body1.angularVelocity -= Body1.inverseMomentOfInertia*Math.Sign(angularImpulse)*
                                     Math.Min(Math.Abs(angularImpulse), MaxImpulse);
            Body2.angularVelocity += Body2.inverseMomentOfInertia*Math.Sign(angularImpulse)*
                                     Math.Min(Math.Abs(angularImpulse), MaxImpulse);
        }
    }
}