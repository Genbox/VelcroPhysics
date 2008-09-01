using System;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public class AngleLimitJoint : Joint
    {
        private float accumlatedAngularImpulseOld;
        private float accumulatedAngularImpulse;
        private float angularImpulse;
        private float biasFactor = .2f;
        protected Body body1;
        protected Body body2;
        private float breakpoint = float.MaxValue;
        private float difference;
        private float jointError;
        private float lowerLimit;
        private bool lowerLimitViolated;
        private float massFactor;

        private float slop = .01f;
        private float softness;
        private float upperLimit;
        private bool upperLimitViolated;
        private float velocityBias;

        public AngleLimitJoint()
        {
        }

        public AngleLimitJoint(Body body1, Body body2, float lowerLimit, float upperLimit)
        {
            this.body1 = body1;
            this.body2 = body2;
            this.lowerLimit = lowerLimit;
            this.upperLimit = upperLimit;
        }

        public Body Body1
        {
            get { return body1; }
            set { body1 = value; }
        }

        public Body Body2
        {
            get { return body2; }
            set { body2 = value; }
        }

        public float BiasFactor
        {
            get { return biasFactor; }
            set { biasFactor = value; }
        }

        public float Slop
        {
            get { return slop; }
            set { slop = value; }
        }

        public float Softness
        {
            get { return softness; }
            set { softness = value; }
        }

        public float UpperLimit
        {
            get { return upperLimit; }
            set { upperLimit = value; }
        }

        public float LowerLimit
        {
            get { return lowerLimit; }
            set { lowerLimit = value; }
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
            if (body1.IsDisposed || body2.IsDisposed)
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
            difference = (body2.totalRotation - body1.totalRotation);
            jointError = 0;

            if (difference > upperLimit)
            {
                if (lowerLimitViolated)
                {
                    accumulatedAngularImpulse = 0;
                    lowerLimitViolated = false;
                }
                upperLimitViolated = true;
                if (difference < upperLimit + slop)
                {
                    jointError = 0;
                }
                else
                {
                    jointError = difference - upperLimit;
                }
            }
            else if (difference < lowerLimit)
            {
                if (upperLimitViolated)
                {
                    accumulatedAngularImpulse = 0;
                    upperLimitViolated = false;
                }
                lowerLimitViolated = true;
                if (difference > lowerLimit - slop)
                {
                    jointError = 0;
                }
                else
                {
                    jointError = difference - lowerLimit;
                }
            }
            else
            {
                upperLimitViolated = false;
                lowerLimitViolated = false;
                jointError = 0;
                accumulatedAngularImpulse = 0;
            }
            velocityBias = biasFactor*inverseDt*jointError;

            massFactor = 1/(softness + body1.inverseMomentOfInertia + body2.inverseMomentOfInertia);

            body1.angularVelocity -= body1.inverseMomentOfInertia*accumulatedAngularImpulse;
            body2.angularVelocity += body2.inverseMomentOfInertia*accumulatedAngularImpulse;
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
                -(velocityBias + (body2.angularVelocity - body1.angularVelocity) + softness*accumulatedAngularImpulse)*
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

            body1.angularVelocity -= body1.inverseMomentOfInertia*angularImpulse;
            body2.angularVelocity += body2.inverseMomentOfInertia*angularImpulse;
        }
    }
}