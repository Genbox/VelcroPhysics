using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

#if (XNA)
using Microsoft.Xna.Framework; 
#endif

using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Dynamics {
    public class FixedAngleJoint : Joint {
        protected Body body;

        private float biasFactor = .2f;
        private float targetAngle = 0;
        private float softness = 0f;
        private float maxImpulse = float.MaxValue;
        private float breakpoint = float.MaxValue;
  
        private float massFactor;
        private float jointError;
        private float velocityBias;

        public event EventHandler<EventArgs> Broke;

        public FixedAngleJoint() { }

        public FixedAngleJoint(Body body) {
            this.body = body;
        }

        public FixedAngleJoint(Body body, float targetAngle) {
            this.body = body;
            this.targetAngle = targetAngle;
        }

        public Body Body {
            get { return body; }
            set { body = value; }
        }

        public float BiasFactor {
            get { return biasFactor; }
            set { biasFactor = value; }
        }

        public float TargetAngle {
            get { return targetAngle; }
            set { targetAngle = value; }
        }

        public float Softness {
            get { return softness; }
            set { softness = value; }
        }

        public float MaxImpulse {
            get { return maxImpulse; }
            set { maxImpulse = value; }
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

        public override void PreStep(float inverseDt) {
            if (Enabled && Math.Abs(jointError) > breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }
            if (isDisposed) { return; }
            jointError = body.totalRotation - targetAngle;

            velocityBias = -biasFactor * inverseDt * jointError;
            massFactor = (1 - softness) / (body.inverseMomentOfInertia);
        }

        public override void Update() {
            if (isDisposed) { return; }
            float angularImpulse;
            angularImpulse = (velocityBias - body.angularVelocity) * massFactor;
            body.angularVelocity += body.inverseMomentOfInertia * Math.Sign(angularImpulse) * Math.Min(Math.Abs(angularImpulse),maxImpulse);
        }
    }
}
