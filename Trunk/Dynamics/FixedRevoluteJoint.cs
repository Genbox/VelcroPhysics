using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

#if (XNA)
using Microsoft.Xna.Framework; 
#endif

using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Dynamics {
    public class FixedRevoluteJoint : Joint {
        protected Body body;

        private float biasFactor = .8f;
        private float softness = 0;
        private float breakpoint = float.MaxValue;
        private float maxImpulse = float.MaxValue;

        private float jointError;
        private Vector2 localAnchor;
        internal Vector2 anchor;
        private Vector2 r1;
        private Matrix matrix;
        private Vector2 velocityBias;
        private Vector2 accumulatedImpulse;

        public event EventHandler<EventArgs> Broke;

        public FixedRevoluteJoint() { }

        public FixedRevoluteJoint(Body body, Vector2 anchor) {
            this.body = body;
            this.anchor = anchor;
            this.accumulatedImpulse = Vector2.Zero;
            body.GetLocalPosition(ref anchor, out localAnchor);
        }

        public Body Body {
            get { return body; }
            set { body = value; }
        }

        public float BiasFactor {
            get { return biasFactor; }
            set { biasFactor = value; }
        }

        public float Softness {
            get { return softness; }
            set { softness = value; }
        }

        public float Breakpoint {
            get { return breakpoint; }
            set { breakpoint = value; }
        }

        public float MaxImpulse {
            get { return maxImpulse; }
            set { maxImpulse = value; }
        }

        public float JointError {
            get { return jointError; }
        }

        public Vector2 Anchor {
            get { return anchor; }
            set {
                anchor = value;
                SetAnchor(anchor);
            }
        }
        
        public void SetAnchor(Vector2 anchor) {
            this.anchor = anchor;
            if (body == null) { throw new NullReferenceException("Body must be set prior to setting the anchor of the Revolute Joint"); }
            body.GetLocalPosition(ref anchor, out localAnchor);
        }
        
        public override void Validate() {
            if (this.body.IsDisposed) {
                Dispose();
            }
        }

        #region PreStep variables
        float floatTemp1 = 0;
        Vector2 vectorTemp1 = Vector2.Zero;
        Vector2 vectorTemp2 = Vector2.Zero;
        Vector2 vectorTemp3 = Vector2.Zero;
        Vector2 vectorTemp4 = Vector2.Zero;
        Vector2 vectorTemp5 = Vector2.Zero;

        Matrix K = new Matrix();
        Matrix K1 = new Matrix();
        Matrix K2 = new Matrix();

        Matrix bodyMatrixTemp;
        float bodyInverseMass;
        float bodyInverseMomentOfInertia;
        #endregion
        public override void PreStep(float inverseDt) {
            if (Enabled && Math.Abs(jointError) > breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }
            if (isDisposed) { return; }

            bodyInverseMass = body.inverseMass;
            bodyInverseMomentOfInertia = body.inverseMomentOfInertia;
         
            body.GetBodyMatrix(out bodyMatrixTemp);
            Vector2.TransformNormal(ref localAnchor, ref bodyMatrixTemp, out r1);

            K1.M11 = bodyInverseMass;
            K1.M12 = 0;
            K1.M21 = 0;
            K1.M22 = bodyInverseMass;

            K2.M11 = bodyInverseMomentOfInertia * r1.Y * r1.Y;
            K2.M12 = -bodyInverseMomentOfInertia * r1.X * r1.Y;
            K2.M21 = -bodyInverseMomentOfInertia * r1.X * r1.Y;
            K2.M22 = bodyInverseMomentOfInertia * r1.X * r1.X;

            //Matrix K = K1 + K2 + K3;
            Matrix.Add(ref K1, ref K2, out K);

            K.M11 += softness;
            K.M12 += softness;
  
            //_matrix = MatrixInvert2D(K);
            MatrixInvert2D(ref K, out matrix);

            Vector2.Add(ref body.position, ref r1, out vectorTemp1);
            Vector2.Subtract(ref anchor, ref vectorTemp1, out vectorTemp2);
            Vector2.Multiply(ref vectorTemp2, -biasFactor * inverseDt, out velocityBias);
            jointError = vectorTemp2.Length();

            //warm starting
            vectorTemp1.X = -accumulatedImpulse.X;
            vectorTemp1.Y = -accumulatedImpulse.Y;
            if (maxImpulse < float.MaxValue) {Calculator.Truncate(ref vectorTemp1, maxImpulse, out vectorTemp1);}
            body.ApplyImmediateImpulse(ref vectorTemp1);
            Calculator.Cross(ref r1, ref vectorTemp1, out floatTemp1);
            body.ApplyAngularImpulse(floatTemp1);
        }

        Matrix B = new Matrix();
        private void MatrixInvert2D(ref Matrix matrix, out Matrix invertedMatrix) {
            float a = matrix.M11, b = matrix.M12, c = matrix.M21, d = matrix.M22;
            float det = a * d - b * c;
            Debug.Assert(det != 0.0f);
            det = 1.0f / det;
            B.M11 = det * d; B.M12 = -det * b;
            B.M21 = -det * c; B.M22 = det * a;
            invertedMatrix = B;
        }

        #region Update variables
        Vector2 dv;
        Vector2 dvBias;
        Vector2 impulse; 
        #endregion
        public override void Update() {
            if (Math.Abs(jointError) > breakpoint) { Dispose(); } //check if joint is broken
            if (isDisposed) { return; }
            
            Calculator.Cross(ref body.angularVelocity, ref r1, out vectorTemp1);
            Vector2.Add(ref body.linearVelocity, ref vectorTemp1, out dv);
            dv = -dv;

            Vector2.Subtract(ref velocityBias, ref dv, out vectorTemp1);
            Vector2.Multiply(ref accumulatedImpulse, softness, out vectorTemp2);
            Vector2.Subtract(ref vectorTemp1, ref vectorTemp2, out dvBias);

            Vector2.Transform(ref dvBias, ref matrix, out impulse);

            vectorTemp1.X = -impulse.X;
            vectorTemp1.Y = -impulse.Y;            
            if (maxImpulse < float.MaxValue) Calculator.Truncate(ref vectorTemp1, maxImpulse, out vectorTemp1);
            body.ApplyImmediateImpulse(ref vectorTemp1);
            Calculator.Cross(ref r1, ref vectorTemp1, out floatTemp1);
            body.ApplyAngularImpulse(floatTemp1);

            Vector2.Add(ref accumulatedImpulse, ref impulse, out accumulatedImpulse);
        }
    }
}
