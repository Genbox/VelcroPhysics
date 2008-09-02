using System;
using System.Diagnostics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Mathematics;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    public class FixedRevoluteJoint : Joint
    {
        private Vector2 _accumulatedImpulse;
        internal Vector2 anchor;
        private Matrix _b;
        private float _biasFactor = .8f;
        protected Body body;
        private float _breakpoint = float.MaxValue;

        private float _jointError;
        private Vector2 _localAnchor;
        private Matrix _matrix;
        private float _maxImpulse = float.MaxValue;
        private Vector2 _r1;
        private float _softness;
        private Vector2 _velocityBias;

        public FixedRevoluteJoint()
        {
        }

        public FixedRevoluteJoint(Body body, Vector2 anchor)
        {
            this.body = body;
            this.anchor = anchor;
            _accumulatedImpulse = Vector2.Zero;
            body.GetLocalPosition(ref anchor, out _localAnchor);
        }

        public Body Body
        {
            get { return body; }
            set { body = value; }
        }

        public float BiasFactor
        {
            get { return _biasFactor; }
            set { _biasFactor = value; }
        }

        public float Softness
        {
            get { return _softness; }
            set { _softness = value; }
        }

        public float Breakpoint
        {
            get { return _breakpoint; }
            set { _breakpoint = value; }
        }

        public float MaxImpulse
        {
            get { return _maxImpulse; }
            set { _maxImpulse = value; }
        }

        public float JointError
        {
            get { return _jointError; }
        }

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
            if (body == null)
            {
                throw new NullReferenceException("Body must be set prior to setting the anchor of the Revolute Joint");
            }
            body.GetLocalPosition(ref anchor, out _localAnchor);
        }

        public override void Validate()
        {
            if (body.IsDisposed)
            {
                Dispose();
            }
        }

        public override void PreStep(float inverseDt)
        {
            if (Enabled && Math.Abs(_jointError) > _breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }
            if (isDisposed)
            {
                return;
            }

            _bodyInverseMass = body.inverseMass;
            _bodyInverseMomentOfInertia = body.inverseMomentOfInertia;

            body.GetBodyMatrix(out _bodyMatrixTemp);
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

            _k.M11 += _softness;
            _k.M12 += _softness;

            //_matrix = MatrixInvert2D(_k);
            MatrixInvert2D(ref _k, out _matrix);

            Vector2.Add(ref body.position, ref _r1, out _vectorTemp1);
            Vector2.Subtract(ref anchor, ref _vectorTemp1, out _vectorTemp2);
            Vector2.Multiply(ref _vectorTemp2, -_biasFactor*inverseDt, out _velocityBias);
            _jointError = _vectorTemp2.Length();

            //warm starting
            _vectorTemp1.X = -_accumulatedImpulse.X;
            _vectorTemp1.Y = -_accumulatedImpulse.Y;
            if (_maxImpulse < float.MaxValue)
            {
                Calculator.Truncate(ref _vectorTemp1, _maxImpulse, out _vectorTemp1);
            }
            body.ApplyImmediateImpulse(ref _vectorTemp1);
            Calculator.Cross(ref _r1, ref _vectorTemp1, out _floatTemp1);
            body.ApplyAngularImpulse(_floatTemp1);
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
            if (Math.Abs(_jointError) > _breakpoint)
            {
                Dispose();
            } //check if joint is broken
            if (isDisposed)
            {
                return;
            }

            Calculator.Cross(ref body.angularVelocity, ref _r1, out _vectorTemp1);
            Vector2.Add(ref body.linearVelocity, ref _vectorTemp1, out _dv);
            _dv = -_dv;

            Vector2.Subtract(ref _velocityBias, ref _dv, out _vectorTemp1);
            Vector2.Multiply(ref _accumulatedImpulse, _softness, out _vectorTemp2);
            Vector2.Subtract(ref _vectorTemp1, ref _vectorTemp2, out _dvBias);

            Vector2.Transform(ref _dvBias, ref _matrix, out _impulse);

            _vectorTemp1.X = -_impulse.X;
            _vectorTemp1.Y = -_impulse.Y;
            if (_maxImpulse < float.MaxValue) Calculator.Truncate(ref _vectorTemp1, _maxImpulse, out _vectorTemp1);
            body.ApplyImmediateImpulse(ref _vectorTemp1);
            Calculator.Cross(ref _r1, ref _vectorTemp1, out _floatTemp1);
            body.ApplyAngularImpulse(_floatTemp1);

            Vector2.Add(ref _accumulatedImpulse, ref _impulse, out _accumulatedImpulse);
        }

        #region Update variables

        private Vector2 _dv;
        private Vector2 _dvBias;
        private Vector2 _impulse;

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

        #endregion
    }
}