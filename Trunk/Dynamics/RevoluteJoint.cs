using System;
using System.Diagnostics;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public class RevoluteJoint : Joint
    {
        private Vector2 _accumulatedImpulse;
        private Vector2 _anchor;
        private Matrix _b;

        private Vector2 _currentAnchor;
        private Vector2 _dv;
        private Vector2 _dvBias;
        private Vector2 _impulse;
        private Matrix _matrix;

        private Vector2 _r1;
        private Vector2 _r2;
        private Vector2 _velocityBias;
        internal Vector2 localAnchor1;
        internal Vector2 localAnchor2;

        public RevoluteJoint()
        {
            Breakpoint = float.MaxValue;
            BiasFactor = .2f;
        }

        public RevoluteJoint(Body body1, Body body2, Vector2 anchor)
        {
            Breakpoint = float.MaxValue;
            BiasFactor = .2f;
            Body1 = body1;
            Body2 = body2;

            _anchor = anchor;

            body1.GetLocalPosition(ref anchor, out localAnchor1);
            body2.GetLocalPosition(ref anchor, out localAnchor2);

            _accumulatedImpulse = Vector2.Zero;
        }

        public Body Body1 { get; set; }

        public Body Body2 { get; set; }

        public float BiasFactor { get; set; }

        public float Softness { get; set; }

        public float Breakpoint { get; set; }

        public float JointError { get; private set; }

        public Vector2 Anchor
        {
            get { return _anchor; }
            set
            {
                _anchor = value;
                Body1.GetLocalPosition(ref _anchor, out localAnchor1);
                Body2.GetLocalPosition(ref _anchor, out localAnchor2);
            }
        }

        //this gives the anchor position after the simulation starts
        public Vector2 CurrentAnchor
        {
            get
            {
                Vector2.Add(ref Body1.position, ref _r1, out _currentAnchor); //anchor moves once simulator starts
                return _currentAnchor;
            }
        }

        public event EventHandler<EventArgs> Broke;

        public void SetInitialAnchor(Vector2 initialAnchor)
        {
            _anchor = initialAnchor;
            if (Body1 == null)
            {
                throw new NullReferenceException("Body must be set prior to setting the anchor of the Revolute Joint");
            }
            Body1.GetLocalPosition(ref initialAnchor, out localAnchor1);
            Body2.GetLocalPosition(ref initialAnchor, out localAnchor2);
        }

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
            if (isDisposed) return;

            body1InverseMass = Body1.inverseMass;
            body1InverseMomentOfInertia = Body1.inverseMomentOfInertia;

            body2InverseMass = Body2.inverseMass;
            body2InverseMomentOfInertia = Body2.inverseMomentOfInertia;

            Body1.GetBodyMatrix(out body1MatrixTemp);
            Body2.GetBodyMatrix(out body2MatrixTemp);
            Vector2.TransformNormal(ref localAnchor1, ref body1MatrixTemp, out _r1);
            Vector2.TransformNormal(ref localAnchor2, ref body2MatrixTemp, out _r2);

            K1.M11 = body1InverseMass + body2InverseMass;
            K1.M12 = 0;
            K1.M21 = 0;
            K1.M22 = body1InverseMass + body2InverseMass;

            K2.M11 = body1InverseMomentOfInertia*_r1.Y*_r1.Y;
            K2.M12 = -body1InverseMomentOfInertia*_r1.X*_r1.Y;
            K2.M21 = -body1InverseMomentOfInertia*_r1.X*_r1.Y;
            K2.M22 = body1InverseMomentOfInertia*_r1.X*_r1.X;

            K3.M11 = body2InverseMomentOfInertia*_r2.Y*_r2.Y;
            K3.M12 = -body2InverseMomentOfInertia*_r2.X*_r2.Y;
            K3.M21 = -body2InverseMomentOfInertia*_r2.X*_r2.Y;
            K3.M22 = body2InverseMomentOfInertia*_r2.X*_r2.X;

            //Matrix K = K1 + K2 + K3;
            Matrix.Add(ref K1, ref K2, out K);
            Matrix.Add(ref K, ref K3, out K);

            K.M11 += Softness;
            K.M12 += Softness;

            //_matrix = MatrixInvert2D(K);
            MatrixInvert2D(ref K, out _matrix);

            Vector2.Add(ref Body1.position, ref _r1, out vectorTemp1);
            Vector2.Add(ref Body2.position, ref _r2, out vectorTemp2);
            Vector2.Subtract(ref vectorTemp2, ref vectorTemp1, out vectorTemp3);
            Vector2.Multiply(ref vectorTemp3, -BiasFactor*inverseDt, out _velocityBias);

            JointError = vectorTemp3.Length();

            Body2.ApplyImmediateImpulse(ref _accumulatedImpulse);
            Calculator.Cross(ref _r2, ref _accumulatedImpulse, out floatTemp1);
            Body2.ApplyAngularImpulse(floatTemp1);

            Vector2.Multiply(ref _accumulatedImpulse, -1, out vectorTemp1);
            Body1.ApplyImmediateImpulse(ref vectorTemp1);
            Calculator.Cross(ref _r1, ref _accumulatedImpulse, out floatTemp1);
            Body1.ApplyAngularImpulse(-floatTemp1);
        }

        private void MatrixInvert2D(ref Matrix matrix, out Matrix invertedMatrix)
        {
            float a = matrix.M11, b = matrix.M12, c = matrix.M21, d = matrix.M22;
            float det = a*d - b*c;
            Debug.Assert(det != 0.0f);
            det = 1.0f/det;
            _b.M11 = det*d;
            _b.M12 = -det*b;
            _b.M21 = -det*c;
            _b.M22 = det*a;
            invertedMatrix = _b;
        }

        public override void Update()
        {
            //if (Math.Abs(jointError) > breakpoint) { Dispose(); } //check if joint is broken
            if (isDisposed) return;
            if (!Enabled) return;

            //Vector2 dv = body2.LinearVelocity + Calculator.Cross(body2.AngularVelocity, r2) - body1.LinearVelocity - Calculator.Cross(body1.AngularVelocity, r1);

            #region INLINE: Calculator.Cross(ref body2.angularVelocity, ref r2, out vectorTemp1);

            vectorTemp1.X = -Body2.angularVelocity*_r2.Y;
            vectorTemp1.Y = Body2.angularVelocity*_r2.X;

            #endregion

            #region INLINE: Calculator.Cross(ref body1.angularVelocity, ref r1, out vectorTemp2);

            vectorTemp2.X = -Body1.angularVelocity*_r1.Y;
            vectorTemp2.Y = Body1.angularVelocity*_r1.X;

            #endregion

            #region INLINE: Vector2.Add(ref body2.linearVelocity, ref vectorTemp1, out vectorTemp3);

            vectorTemp3.X = Body2.linearVelocity.X + vectorTemp1.X;
            vectorTemp3.Y = Body2.linearVelocity.Y + vectorTemp1.Y;

            #endregion

            #region INLINE: Vector2.Add(ref body1.linearVelocity, ref vectorTemp2, out vectorTemp4);

            vectorTemp4.X = Body1.linearVelocity.X + vectorTemp2.X;
            vectorTemp4.Y = Body1.linearVelocity.Y + vectorTemp2.Y;

            #endregion

            #region INLINE: Vector2.Subtract(ref vectorTemp3, ref vectorTemp4, out dv);

            _dv.X = vectorTemp3.X - vectorTemp4.X;
            _dv.Y = vectorTemp3.Y - vectorTemp4.Y;

            #endregion

            #region INLINE: Vector2.Subtract(ref velocityBias, ref dv, out vectorTemp1);

            vectorTemp1.X = _velocityBias.X - _dv.X;
            vectorTemp1.Y = _velocityBias.Y - _dv.Y;

            #endregion

            #region INLINE: Vector2.Multiply(ref accumulatedImpulse, softness, out vectorTemp2);

            vectorTemp2.X = _accumulatedImpulse.X*Softness;
            vectorTemp2.Y = _accumulatedImpulse.Y*Softness;

            #endregion

            #region INLINE: Vector2.Subtract(ref vectorTemp1, ref vectorTemp2, out dvBias);

            _dvBias.X = vectorTemp1.X - vectorTemp2.X;
            _dvBias.Y = vectorTemp1.Y - vectorTemp2.Y;

            #endregion

            #region INLINE: Vector2.Transform(ref dvBias, ref _matrix, out impulse);

            float num2 = ((_dvBias.X*_matrix.M11) + (_dvBias.Y*_matrix.M21)) + _matrix.M41;
            float num = ((_dvBias.X*_matrix.M12) + (_dvBias.Y*_matrix.M22)) + _matrix.M42;
            _impulse.X = num2;
            _impulse.Y = num;

            #endregion

            #region INLINE: body2.ApplyImpulse(ref impulse);

            _dv.X = _impulse.X*Body2.inverseMass;
            _dv.Y = _impulse.Y*Body2.inverseMass;

            Body2.linearVelocity.X = _dv.X + Body2.linearVelocity.X;
            Body2.linearVelocity.Y = _dv.Y + Body2.linearVelocity.Y;

            #endregion

            #region INLINE: Calculator.Cross(ref r2, ref impulse, out floatTemp1);

            floatTemp1 = _r2.X*_impulse.Y - _r2.Y*_impulse.X;

            #endregion

            #region INLINE: body2.ApplyAngularImpulse(ref floatTemp1);

            Body2.angularVelocity += floatTemp1*Body2.inverseMomentOfInertia;

            #endregion

            #region INLINE: Vector2.Multiply(ref impulse, -1, out vectorTemp1);

            vectorTemp1.X = _impulse.X*-1;
            vectorTemp1.Y = _impulse.Y*-1;

            #endregion

            #region INLINE: body1.ApplyImpulse(ref vectorTemp1);

            _dv.X = vectorTemp1.X*Body1.inverseMass;
            _dv.Y = vectorTemp1.Y*Body1.inverseMass;

            Body1.linearVelocity.X = _dv.X + Body1.linearVelocity.X;
            Body1.linearVelocity.Y = _dv.Y + Body1.linearVelocity.Y;

            #endregion

            #region INLINE: Calculator.Cross(ref r1, ref impulse, out floatTemp1);

            floatTemp1 = _r1.X*_impulse.Y - _r1.Y*_impulse.X;

            #endregion

            floatTemp1 = -floatTemp1;

            #region INLINE: body1.ApplyAngularImpulse(ref floatTemp1);

            Body1.angularVelocity += floatTemp1*Body1.inverseMomentOfInertia;

            #endregion

            #region INLINE: Vector2.Add(ref accumulatedImpulse, ref impulse, out accumulatedImpulse);

            _accumulatedImpulse.X = _accumulatedImpulse.X + _impulse.X;
            _accumulatedImpulse.Y = _accumulatedImpulse.Y + _impulse.Y;

            #endregion
        }

        #region PreStep variables

        private float body1InverseMass;
        private float body1InverseMomentOfInertia;
        private Matrix body1MatrixTemp;
        private float body2InverseMass;
        private float body2InverseMomentOfInertia;
        private Matrix body2MatrixTemp;
        private float floatTemp1;
        private Matrix K;
        private Matrix K1;
        private Matrix K2;
        private Matrix K3;
        private Vector2 vectorTemp1 = Vector2.Zero;
        private Vector2 vectorTemp2 = Vector2.Zero;
        private Vector2 vectorTemp3 = Vector2.Zero;
        private Vector2 vectorTemp4 = Vector2.Zero;

        //Note: Cleanup, variable never used
        //private Vector2 vectorTemp5 = Vector2.Zero;

        #endregion
    }
}