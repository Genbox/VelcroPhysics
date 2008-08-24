using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

#if (XNA)
using Microsoft.Xna.Framework; 
#endif

using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Dynamics {
    public class RevoluteJoint : Joint {
        protected Body body1;
        protected Body body2;

        private float biasFactor = .2f;
        private float softness;
        private float breakpoint = float.MaxValue;

        private float jointError;
        private Matrix _matrix;
        private Vector2 anchor;
        internal Vector2 localAnchor1;
        internal Vector2 localAnchor2;
        private Vector2 r1;
        private Vector2 r2;
        private Vector2 velocityBias;
        private Vector2 accumulatedImpulse;
        private Vector2 currentAnchor;

        public event EventHandler<EventArgs> Broke;

        public RevoluteJoint() { }

        public RevoluteJoint(Body body1, Body body2, Vector2 anchor) {
            this.body1 = body1;
            this.body2 = body2;

            this.anchor = anchor;

            body1.GetLocalPosition(ref anchor, out localAnchor1);
            body2.GetLocalPosition(ref anchor, out localAnchor2);

            accumulatedImpulse = Vector2.Zero;
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

        public float Softness {
            get { return softness; }
            set { softness = value; }
        }

        public float Breakpoint {
            get { return breakpoint; }
            set { breakpoint = value; }
        }

        public float JointError {
            get { return jointError; }
        }

        public Vector2 Anchor {
            get {
                return anchor;
            }
            set {
                anchor = value;
                body1.GetLocalPosition(ref anchor, out localAnchor1);
                body2.GetLocalPosition(ref anchor, out localAnchor2);
            }
        }

        //this gives the anchor position after the simulation starts
        public Vector2 CurrentAnchor {
            get {
                Vector2.Add(ref body1.position, ref r1, out currentAnchor); //anchor moves once simulator starts
                return currentAnchor;
            }
        }

        public void SetInitialAnchor(Vector2 initialAnchor) {
            this.anchor = initialAnchor;
            if (body1 == null) { throw new NullReferenceException("Body must be set prior to setting the anchor of the Revolute Joint"); }
            body1.GetLocalPosition(ref initialAnchor, out localAnchor1);
            body2.GetLocalPosition(ref initialAnchor, out localAnchor2);
        }
        
        public override void Validate() {
            if (this.body1.IsDisposed || this.body2.IsDisposed) {
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
        Matrix K3 = new Matrix();

        Matrix body1MatrixTemp;
        float body1InverseMass;
        float body1InverseMomentOfInertia;

        Matrix body2MatrixTemp;
        float body2InverseMass;
        float body2InverseMomentOfInertia;
        #endregion
        public override void PreStep(float inverseDt) {
            if (Enabled && Math.Abs(jointError) > breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }
            if (isDisposed)  return; 

            body1InverseMass = body1.inverseMass;
            body1InverseMomentOfInertia = body1.inverseMomentOfInertia;

            body2InverseMass = body2.inverseMass;
            body2InverseMomentOfInertia = body2.inverseMomentOfInertia; 
         
            body1.GetBodyMatrix(out body1MatrixTemp);
            body2.GetBodyMatrix(out body2MatrixTemp);
            Vector2.TransformNormal(ref localAnchor1, ref body1MatrixTemp, out r1);
            Vector2.TransformNormal(ref localAnchor2, ref body2MatrixTemp, out r2);

            K1.M11 = body1InverseMass + body2InverseMass;
            K1.M12 = 0;
            K1.M21 = 0;
            K1.M22 = body1InverseMass + body2InverseMass;

            K2.M11 = body1InverseMomentOfInertia * r1.Y * r1.Y;
            K2.M12 = -body1InverseMomentOfInertia * r1.X * r1.Y;
            K2.M21 = -body1InverseMomentOfInertia * r1.X * r1.Y;
            K2.M22 = body1InverseMomentOfInertia * r1.X * r1.X;

            K3.M11 = body2InverseMomentOfInertia * r2.Y * r2.Y;
            K3.M12 = -body2InverseMomentOfInertia * r2.X * r2.Y;
            K3.M21 = -body2InverseMomentOfInertia * r2.X * r2.Y;
            K3.M22 = body2InverseMomentOfInertia * r2.X * r2.X;

            //Matrix K = K1 + K2 + K3;
            Matrix.Add(ref K1, ref K2, out K);
            Matrix.Add(ref K, ref K3, out K);

            K.M11 += softness;
            K.M12 += softness;
  
            //_matrix = MatrixInvert2D(K);
            MatrixInvert2D(ref K, out _matrix);

            Vector2.Add(ref body1.position, ref r1, out vectorTemp1);
            Vector2.Add(ref body2.position, ref r2, out vectorTemp2);
            Vector2.Subtract(ref vectorTemp2, ref vectorTemp1, out vectorTemp3);
            Vector2.Multiply(ref vectorTemp3, -biasFactor * inverseDt, out velocityBias);

            jointError = vectorTemp3.Length();

            body2.ApplyImmediateImpulse(ref accumulatedImpulse);
            Calculator.Cross(ref r2, ref accumulatedImpulse, out floatTemp1);
            body2.ApplyAngularImpulse(floatTemp1);

            Vector2.Multiply(ref accumulatedImpulse, -1, out vectorTemp1);
            body1.ApplyImmediateImpulse(ref vectorTemp1);
            Calculator.Cross(ref r1, ref accumulatedImpulse, out floatTemp1);
            body1.ApplyAngularImpulse(-floatTemp1);
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

        Vector2 dv;
        Vector2 dvBias;
        Vector2 impulse;
        public override void Update() {
            //if (Math.Abs(jointError) > breakpoint) { Dispose(); } //check if joint is broken
            if (isDisposed)  return; 
            if (!Enabled) return;

            //Vector2 dv = body2.LinearVelocity + Calculator.Cross(body2.AngularVelocity, r2) - body1.LinearVelocity - Calculator.Cross(body1.AngularVelocity, r1);
            #region INLINE: Calculator.Cross(ref body2.angularVelocity, ref r2, out vectorTemp1);
            vectorTemp1.X = -body2.angularVelocity * r2.Y;
            vectorTemp1.Y = body2.angularVelocity * r2.X;
            #endregion

            #region INLINE: Calculator.Cross(ref body1.angularVelocity, ref r1, out vectorTemp2);
            vectorTemp2.X = -body1.angularVelocity * r1.Y;
            vectorTemp2.Y = body1.angularVelocity * r1.X;
            #endregion

            #region INLINE: Vector2.Add(ref body2.linearVelocity, ref vectorTemp1, out vectorTemp3);
            vectorTemp3.X = body2.linearVelocity.X + vectorTemp1.X;
            vectorTemp3.Y = body2.linearVelocity.Y + vectorTemp1.Y;
            #endregion

            #region INLINE: Vector2.Add(ref body1.linearVelocity, ref vectorTemp2, out vectorTemp4);
            vectorTemp4.X = body1.linearVelocity.X + vectorTemp2.X;
            vectorTemp4.Y = body1.linearVelocity.Y + vectorTemp2.Y;
            #endregion

            #region INLINE: Vector2.Subtract(ref vectorTemp3, ref vectorTemp4, out dv);
            dv.X = vectorTemp3.X - vectorTemp4.X;
            dv.Y = vectorTemp3.Y - vectorTemp4.Y;
            #endregion

            #region INLINE: Vector2.Subtract(ref velocityBias, ref dv, out vectorTemp1);
            vectorTemp1.X = velocityBias.X - dv.X;
            vectorTemp1.Y = velocityBias.Y - dv.Y;
            #endregion

            #region INLINE: Vector2.Multiply(ref accumulatedImpulse, softness, out vectorTemp2);
            vectorTemp2.X = accumulatedImpulse.X * softness;
            vectorTemp2.Y = accumulatedImpulse.Y * softness;
            #endregion

            #region INLINE: Vector2.Subtract(ref vectorTemp1, ref vectorTemp2, out dvBias);
            dvBias.X = vectorTemp1.X - vectorTemp2.X;
            dvBias.Y = vectorTemp1.Y - vectorTemp2.Y;
            #endregion

            #region INLINE: Vector2.Transform(ref dvBias, ref _matrix, out impulse);
            float num2 = ((dvBias.X * _matrix.M11) + (dvBias.Y * _matrix.M21)) + _matrix.M41;
            float num = ((dvBias.X * _matrix.M12) + (dvBias.Y * _matrix.M22)) + _matrix.M42;
            impulse.X = num2;
            impulse.Y = num;
            #endregion

            #region INLINE: body2.ApplyImpulse(ref impulse);
            dv.X = impulse.X * body2.inverseMass;
            dv.Y = impulse.Y * body2.inverseMass;

            body2.linearVelocity.X = dv.X + body2.linearVelocity.X;
            body2.linearVelocity.Y = dv.Y + body2.linearVelocity.Y;
            #endregion

            #region INLINE: Calculator.Cross(ref r2, ref impulse, out floatTemp1);
            floatTemp1 = r2.X * impulse.Y - r2.Y * impulse.X;
            #endregion

            #region INLINE: body2.ApplyAngularImpulse(ref floatTemp1);
            body2.angularVelocity += floatTemp1 * body2.inverseMomentOfInertia;
            #endregion

            #region INLINE: Vector2.Multiply(ref impulse, -1, out vectorTemp1);
            vectorTemp1.X = impulse.X * -1;
            vectorTemp1.Y = impulse.Y * -1;
            #endregion

            #region INLINE: body1.ApplyImpulse(ref vectorTemp1);
            dv.X = vectorTemp1.X * body1.inverseMass;
            dv.Y = vectorTemp1.Y * body1.inverseMass;

            body1.linearVelocity.X = dv.X + body1.linearVelocity.X;
            body1.linearVelocity.Y = dv.Y + body1.linearVelocity.Y;
            #endregion

            #region INLINE: Calculator.Cross(ref r1, ref impulse, out floatTemp1);
            floatTemp1 = r1.X * impulse.Y - r1.Y * impulse.X;
            #endregion

            floatTemp1 = -floatTemp1;
            #region INLINE: body1.ApplyAngularImpulse(ref floatTemp1);
            body1.angularVelocity += floatTemp1 * body1.inverseMomentOfInertia;
            #endregion

            #region INLINE: Vector2.Add(ref accumulatedImpulse, ref impulse, out accumulatedImpulse);
            accumulatedImpulse.X = accumulatedImpulse.X + impulse.X;
            accumulatedImpulse.Y = accumulatedImpulse.Y + impulse.Y;
            #endregion
        }

    }
}
