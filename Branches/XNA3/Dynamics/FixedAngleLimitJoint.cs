using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using FarseerGames.FarseerPhysics.Mathematics;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Dynamics {
    public class FixedAngleLimitJoint : Joint {
        protected Body body;

        private float biasFactor = .2f;
        private float slop = .01f;
        private float softness = 0f;
        private float upperLimit = 0;
        private float lowerLimit = 0;
        private float breakpoint = float.MaxValue;

        private float velocityBias;
        private float accumulatedAngularImpulse;
        private float massFactor;
        private float jointError;
        private bool upperLimitViolated = false;
        private bool lowerLimitViolated = false;

        public event EventHandler<EventArgs> Broke;

        public FixedAngleLimitJoint() { }

        public FixedAngleLimitJoint(Body body, float lowerLimit, float upperLimit) {
            this.body = body;
            this.lowerLimit = lowerLimit;
            this.upperLimit = upperLimit;
        }

        public Body Body {
            get { return body; }
            set { body = value; }
        }

        public float BiasFactor {
            get { return biasFactor; }
            set { biasFactor = value; }
        }

        public float Slop {
            get { return slop; }
            set { slop = value; }
        }

        public float Softness {
            get { return softness; }
            set { softness = value; }
        }

        public float UpperLimit {
            get { return upperLimit; }
            set { upperLimit = value; }
        }

        public float LowerLimit {
            get { return lowerLimit; }
            set { lowerLimit = value; }
        }

        public float Breakpoint {
            get { return breakpoint; }
            set { breakpoint = value; }
        }

        public float JointError {
            get { return jointError; }
        }

        public override void Validate() {
            if (this.body.IsDisposed) {
                Dispose();
            }
        }

        float difference;
        public override void PreStep(float inverseDt) {
            if (Enabled && Math.Abs(jointError) > breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }
            if (isDisposed) { return; }
            difference = body.totalRotation;

            if (difference > upperLimit) {
                if (lowerLimitViolated) { accumulatedAngularImpulse = 0; lowerLimitViolated = false; }
                upperLimitViolated = true;
                if (difference < upperLimit + slop) {
                    jointError = 0;
                }
                else {
                    jointError = difference - upperLimit;
                }
                velocityBias = biasFactor * inverseDt * jointError;
            }
            else if (difference < lowerLimit) {
                if (upperLimitViolated) { accumulatedAngularImpulse = 0; upperLimitViolated = false; }
                lowerLimitViolated = true;
                if (difference > lowerLimit - slop) {
                    jointError = 0;
                }
                else {
                    jointError = difference - lowerLimit;
                }
            }
            else {
                upperLimitViolated = false;
                lowerLimitViolated = false;
                jointError = 0;
                accumulatedAngularImpulse = 0;
            }
            velocityBias = biasFactor * inverseDt * jointError;
            massFactor = (1 - softness) / body.inverseMomentOfInertia;
            body.angularVelocity += body.inverseMomentOfInertia * accumulatedAngularImpulse;
        }

        float accumlatedAngularImpulseOld;
        private float angularImpulse;
        public override void Update() {
            if (isDisposed) { return; }
            if (!upperLimitViolated && !lowerLimitViolated) { return; }

            angularImpulse = 0;
            angularImpulse = -(velocityBias + body.angularVelocity + softness*accumulatedAngularImpulse) * massFactor;

            accumlatedAngularImpulseOld = accumulatedAngularImpulse;

            if (upperLimitViolated) {
                accumulatedAngularImpulse = MathHelper.Min(accumlatedAngularImpulseOld + angularImpulse, 0);
            }
            else if (lowerLimitViolated) {
                accumulatedAngularImpulse = MathHelper.Max(accumlatedAngularImpulseOld + angularImpulse, 0);
            }

            angularImpulse = accumulatedAngularImpulse - accumlatedAngularImpulseOld;

            body.angularVelocity += body.inverseMomentOfInertia * angularImpulse;
        }
    }
}
