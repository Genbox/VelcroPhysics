using System;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    public class AngleJoint : Joint
    {
        private float _massFactor;
        private float _velocityBias;

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
        public float TargetAngle { get; set; }
        public float MaxImpulse { get; set; }

        public override void Validate()
        {
            if (Body1.IsDisposed || Body2.IsDisposed)
            {
                Dispose();
            }
        }

        public override void PreStep(float inverseDt)
        {
            base.PreStep(inverseDt);

            if (IsDisposed) return;

            Error = (Body2.TotalRotation - Body1.TotalRotation) - TargetAngle;

            _velocityBias = -BiasFactor*inverseDt*Error;

            _massFactor = (1 - Softness)/(Body1.inverseMomentOfInertia + Body2.inverseMomentOfInertia);
        }

        public override void Update()
        {
            if (IsDisposed) return;
            if (!Enabled) return;

            float angularImpulse = (_velocityBias - Body2.angularVelocity + Body1.angularVelocity)*_massFactor;

            Body1.angularVelocity -= Body1.inverseMomentOfInertia*Math.Sign(angularImpulse)*
                                     Math.Min(Math.Abs(angularImpulse), MaxImpulse);
            Body2.angularVelocity += Body2.inverseMomentOfInertia*Math.Sign(angularImpulse)*
                                     Math.Min(Math.Abs(angularImpulse), MaxImpulse);
        }
    }
}