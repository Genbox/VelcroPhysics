using System;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public class AngleLimitJoint : Joint
    {
        private float accumlatedAngularImpulseOld;
        private float accumulatedAngularImpulse;
        private float angularImpulse;
        private float difference;
        private bool lowerLimitViolated;
        private float massFactor;

        private bool upperLimitViolated;
        private float velocityBias;

        public AngleLimitJoint()
        {
            Breakpoint = float.MaxValue;
            Slop = .01f;
            BiasFactor = .2f;
        }

        public AngleLimitJoint(Body body1, Body body2, float lowerLimit, float upperLimit)
        {
            Breakpoint = float.MaxValue;
            Slop = .01f;
            BiasFactor = .2f;
            Body1 = body1;
            Body2 = body2;
            LowerLimit = lowerLimit;
            UpperLimit = upperLimit;
        }

        public Body Body1 { get; set; }
        public Body Body2 { get; set; }
        public float BiasFactor { get; set; }
        public float Slop { get; set; }
        public float Softness { get; set; }
        public float UpperLimit { get; set; }
        public float LowerLimit { get; set; }
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
            difference = (Body2.TotalRotation - Body1.TotalRotation);
            JointError = 0;

            if (difference > UpperLimit)
            {
                if (lowerLimitViolated)
                {
                    accumulatedAngularImpulse = 0;
                    lowerLimitViolated = false;
                }
                upperLimitViolated = true;
                if (difference < UpperLimit + Slop)
                {
                    JointError = 0;
                }
                else
                {
                    JointError = difference - UpperLimit;
                }
            }
            else if (difference < LowerLimit)
            {
                if (upperLimitViolated)
                {
                    accumulatedAngularImpulse = 0;
                    upperLimitViolated = false;
                }
                lowerLimitViolated = true;
                if (difference > LowerLimit - Slop)
                {
                    JointError = 0;
                }
                else
                {
                    JointError = difference - LowerLimit;
                }
            }
            else
            {
                upperLimitViolated = false;
                lowerLimitViolated = false;
                JointError = 0;
                accumulatedAngularImpulse = 0;
            }
            velocityBias = BiasFactor*inverseDt*JointError;

            massFactor = 1/(Softness + Body1.inverseMomentOfInertia + Body2.inverseMomentOfInertia);

            Body1.angularVelocity -= Body1.inverseMomentOfInertia*accumulatedAngularImpulse;
            Body2.angularVelocity += Body2.inverseMomentOfInertia*accumulatedAngularImpulse;
        }

        public override void Update()
        {
            if (isDisposed)
            {
                return;
            }
            if (!upperLimitViolated && !lowerLimitViolated)
            {
                return;
            }
            angularImpulse = 0;
            angularImpulse =
                -(velocityBias + (Body2.angularVelocity - Body1.angularVelocity) + Softness*accumulatedAngularImpulse)*
                massFactor;

            accumlatedAngularImpulseOld = accumulatedAngularImpulse;

            if (upperLimitViolated)
            {
                accumulatedAngularImpulse = MathHelper.Min(accumlatedAngularImpulseOld + angularImpulse, 0);
            }
            else if (lowerLimitViolated)
            {
                accumulatedAngularImpulse = MathHelper.Max(accumlatedAngularImpulseOld + angularImpulse, 0);
            }

            angularImpulse = accumulatedAngularImpulse - accumlatedAngularImpulseOld;

            Body1.angularVelocity -= Body1.inverseMomentOfInertia*angularImpulse;
            Body2.angularVelocity += Body2.inverseMomentOfInertia*angularImpulse;
        }
    }
}