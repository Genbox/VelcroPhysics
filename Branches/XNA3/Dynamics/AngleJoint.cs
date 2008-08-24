using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Dynamics {
    public class AngleJoint : Joint {
        protected Body body1;
        protected Body body2;

        private float biasFactor = .2f;
        private float targetAngle = 0f;
        private float softness = 0f;
        private float maxImpulse = float.MaxValue;
        private float breakpoint = float.MaxValue;
        
        private float massFactor;
        private float jointError;
        private float velocityBias;

        private bool enabled = true;

        public event EventHandler<EventArgs> Broke;

        public AngleJoint() {}

        public AngleJoint(Body body1, Body body2) {
            this.body1 = body1;
            this.body2 = body2;
        }
        
        public AngleJoint(Body body1, Body body2, float targetAngle) {
            this.body1 = body1;
            this.body2 = body2;
            this.targetAngle = targetAngle;
        }

        public Body Body1 {
            get { return body1; }
            set { body1 = value; }
        }

        public Body Body2 {
            get { return body2; }
            set { body2 = value; }
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

        public float  Breakpoint {
            get { return breakpoint; }
            set { breakpoint = value; }
        }

        public float JointError {
            get { return jointError; }
        }

        public override void Validate() {
            if (this.body1.IsDisposed || this.body2.IsDisposed) {
                Dispose();
            }
        }

        public override void PreStep(float inverseDt) {
            if (Enabled && Math.Abs(jointError) > breakpoint) 
            { 
                Enabled= false;
                if (Broke != null) Broke(this, new EventArgs());
            }
            
            if (isDisposed) { return; }
            jointError = (body2.totalRotation - body1.totalRotation) - targetAngle;

            velocityBias = -biasFactor * inverseDt * jointError;

            massFactor = (1 - softness) / (body1.inverseMomentOfInertia + body2.inverseMomentOfInertia);
        }

        public override void Update() {
            if (isDisposed) return;
            if (!Enabled) return;
            float angularImpulse;
            angularImpulse = (velocityBias - body2.angularVelocity + body1.angularVelocity) * massFactor;

            body1.angularVelocity -= body1.inverseMomentOfInertia * Math.Sign(angularImpulse) * Math.Min(Math.Abs(angularImpulse), maxImpulse);
            body2.angularVelocity += body2.inverseMomentOfInertia * Math.Sign(angularImpulse) * Math.Min(Math.Abs(angularImpulse), maxImpulse);
        }
    }
}
