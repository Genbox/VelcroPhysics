using System;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    public class FixedAngleJoint : Joint
    {
        private float massFactor;
        private float velocityBias;

        public FixedAngleJoint()
        {
            Breakpoint = float.MaxValue;
            MaxImpulse = float.MaxValue;
            BiasFactor = .2f;
        }

        public FixedAngleJoint(Body body)
        {
            Breakpoint = float.MaxValue;
            MaxImpulse = float.MaxValue;
            BiasFactor = .2f;
            Body = body;
        }

        public FixedAngleJoint(Body body, float targetAngle)
        {
            Breakpoint = float.MaxValue;
            MaxImpulse = float.MaxValue;
            BiasFactor = .2f;
            Body = body;
            TargetAngle = targetAngle;
        }

        public Body Body { get; set; }
        public float TargetAngle { get; set; }
        public float MaxImpulse { get; set; }

        public override void Validate()
        {
            if (Body.IsDisposed)
            {
                Dispose();
            }
        }

        public override void PreStep(float inverseDt)
        {
            base.PreStep(inverseDt);

            if (IsDisposed) return;

            Error = Body.TotalRotation - TargetAngle;

            velocityBias = -BiasFactor*inverseDt*Error;
            massFactor = (1 - Softness)/(Body.inverseMomentOfInertia);
        }

        public override void Update()
        {
            if (IsDisposed)
            {
                return;
            }
            float angularImpulse = (velocityBias - Body.angularVelocity)*massFactor;
            Body.angularVelocity += Body.inverseMomentOfInertia*Math.Sign(angularImpulse)*
                                    Math.Min(Math.Abs(angularImpulse), MaxImpulse);
        }
    }
}