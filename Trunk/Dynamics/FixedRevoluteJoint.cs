using System;
using System.Diagnostics;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public class FixedRevoluteJoint : Joint
    {
        private Vector2 _accumulatedImpulse;
        private Matrix _b;

        private Vector2 _localAnchor;
        private Matrix _matrix;
        private Vector2 _r1;
        private Vector2 _velocityBias;
        internal Vector2 anchor;

        public FixedRevoluteJoint()
        {
            BiasFactor = .8f;
            Breakpoint = float.MaxValue;
            MaxImpulse = float.MaxValue;
        }

        public FixedRevoluteJoint(Body body, Vector2 anchor)
        {
            BiasFactor = .8f;
            Breakpoint = float.MaxValue;
            MaxImpulse = float.MaxValue;
            Body = body;
            this.anchor = anchor;
            _accumulatedImpulse = Vector2.Zero;
            body.GetLocalPosition(ref anchor, out _localAnchor);
        }

        public Body Body { get; set; }

        public float BiasFactor { get; set; }

        public float Softness { get; set; }

        public float Breakpoint { get; set; }

        public float MaxImpulse { get; set; }

        public float JointError { get; private set; }

        public Vector2 Anchor
        {
            get { return anchor; }
            set
            {
                anchor = value;
                SetAnchor(anchor);
            }
        }

        public event EventHandler<EventArgs> Broke;

        public void SetAnchor(Vector2 anchor)
        {
            this.anchor = anchor;
            if (Body == null)
            {
                throw new NullReferenceException("Body must be set prior to setting the anchor of the Revolute Joint");
            }
            Body.GetLocalPosition(ref anchor, out _localAnchor);
        }

        public override void Validate()
        {
            if (Body.IsDisposed)
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

            _bodyInverseMass = Body.inverseMass;
            _bodyInverseMomentOfInertia = Body.inverseMomentOfInertia;

            Body.GetBodyMatrix(out _bodyMatrixTemp);
            Vector2.TransformNormal(ref _localAnchor, ref _bodyMatrixTemp, out _r1);

            _k1.M11 = _bodyInverseMass;
            _k1.M12 = 0;
            _k1.M21 = 0;
            _k1.M22 = _bodyInverseMass;

            _k2.M11 = _bodyInverseMomentOfInertia*_r1.Y*_r1.Y;
            _k2.M12 = -_bodyInverseMomentOfInertia*_r1.X*_r1.Y;
            _k2.M21 = -_bodyInverseMomentOfInertia*_r1.X*_r1.Y;
            _k2.M22 = _bodyInverseMomentOfInertia*_r1.X*_r1.X;

            //Matrix _k = _k1 + _k2 + K3;
            Matrix.Add(ref _k1, ref _k2, out _k);

            _k.M11 += Softness;
            _k.M12 += Softness;

            //_matrix = MatrixInvert2D(_k);
            MatrixInvert2D(ref _k, out _matrix);

            Vector2.Add(ref Body.position, ref _r1, out _vectorTemp1);
            Vector2.Subtract(ref anchor, ref _vectorTemp1, out _vectorTemp2);
            Vector2.Multiply(ref _vectorTemp2, -BiasFactor*inverseDt, out _velocityBias);
            JointError = _vectorTemp2.Length();

            //warm starting
            _vectorTemp1.X = -_accumulatedImpulse.X;
            _vectorTemp1.Y = -_accumulatedImpulse.Y;
            if (MaxImpulse < float.MaxValue)
            {
                Calculator.Truncate(ref _vectorTemp1, MaxImpulse, out _vectorTemp1);
            }
            Body.ApplyImmediateImpulse(ref _vectorTemp1);
            Calculator.Cross(ref _r1, ref _vectorTemp1, out _floatTemp1);
            Body.ApplyAngularImpulse(_floatTemp1);
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
            if (Math.Abs(JointError) > Breakpoint)
            {
                Dispose();
            } //check if joint is broken
            if (isDisposed)
            {
                return;
            }

            Calculator.Cross(ref Body.angularVelocity, ref _r1, out _vectorTemp1);
            Vector2.Add(ref Body.linearVelocity, ref _vectorTemp1, out dv);
            dv = -dv;

            Vector2.Subtract(ref _velocityBias, ref dv, out _vectorTemp1);
            Vector2.Multiply(ref _accumulatedImpulse, Softness, out _vectorTemp2);
            Vector2.Subtract(ref _vectorTemp1, ref _vectorTemp2, out dvBias);

            Vector2.Transform(ref dvBias, ref _matrix, out impulse);

            _vectorTemp1.X = -impulse.X;
            _vectorTemp1.Y = -impulse.Y;
            if (MaxImpulse < float.MaxValue) Calculator.Truncate(ref _vectorTemp1, MaxImpulse, out _vectorTemp1);
            Body.ApplyImmediateImpulse(ref _vectorTemp1);
            Calculator.Cross(ref _r1, ref _vectorTemp1, out _floatTemp1);
            Body.ApplyAngularImpulse(_floatTemp1);

            Vector2.Add(ref _accumulatedImpulse, ref impulse, out _accumulatedImpulse);
        }

        #region Update variables

        private Vector2 dv;
        private Vector2 dvBias;
        private Vector2 impulse;

        #endregion

        #region PreStep variables

        private float _bodyInverseMass;
        private float _bodyInverseMomentOfInertia;
        private Matrix _bodyMatrixTemp;
        private float _floatTemp1;
        private Matrix _k;
        private Matrix _k1;
        private Matrix _k2;
        private Vector2 _vectorTemp1 = Vector2.Zero;
        private Vector2 _vectorTemp2 = Vector2.Zero;
        //Note: Cleanup. Never used.
        //private Vector2 vectorTemp3 = Vector2.Zero;
        //private Vector2 vectorTemp4 = Vector2.Zero;
        //private Vector2 vectorTemp5 = Vector2.Zero;

        #endregion
    }
}