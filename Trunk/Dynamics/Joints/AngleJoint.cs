using System;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    /// <summary>
    /// Angle joint joins together 2 bodies in an angle
    /// </summary>
    public class AngleJoint : Joint
    {
        private float _biasFactor = .2f;

        private float _breakpoint = float.MaxValue;

        private float _jointError;
        private float _massFactor;
        private float _maxImpulse = float.MaxValue;
        private float _softness;
        private float _targetAngle;
        private float _velocityBias;
        private Body _body1;
        private Body _body2;

        public AngleJoint()
        {
        }

        public AngleJoint(Body body1, Body body2)
        {
            _body1 = body1;
            _body2 = body2;
        }

        public AngleJoint(Body body1, Body body2, float targetAngle)
        {
            _body1 = body1;
            _body2 = body2;
            _targetAngle = targetAngle;
        }

        public Body Body1
        {
            get { return _body1; }
            set { _body1 = value; }
        }

        public Body Body2
        {
            get { return _body2; }
            set { _body2 = value; }
        }

        public float BiasFactor
        {
            get { return _biasFactor; }
            set { _biasFactor = value; }
        }

        public float TargetAngle
        {
            get { return _targetAngle; }
            set { _targetAngle = value; }
        }

        public float Softness
        {
            get { return _softness; }
            set { _softness = value; }
        }

        public float MaxImpulse
        {
            get { return _maxImpulse; }
            set { _maxImpulse = value; }
        }

        public float Breakpoint
        {
            get { return _breakpoint; }
            set { _breakpoint = value; }
        }

        public float JointError
        {
            get { return _jointError; }
        }

        public event EventHandler<EventArgs> Broke;

        public override void Validate()
        {
            if (_body1.IsDisposed || _body2.IsDisposed)
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

            if (IsDisposed)
            {
                return;
            }
            _jointError = (_body2.totalRotation - _body1.totalRotation) - _targetAngle;

            _velocityBias = -_biasFactor*inverseDt*_jointError;

            _massFactor = (1 - _softness)/(_body1.inverseMomentOfInertia + _body2.inverseMomentOfInertia);
        }

        public override void Update()
        {
            if (IsDisposed) return;
            if (!Enabled) return;
            float angularImpulse = (_velocityBias - _body2.angularVelocity + _body1.angularVelocity)*_massFactor;

            _body1.angularVelocity -= _body1.inverseMomentOfInertia*Math.Sign(angularImpulse)*
                                     Math.Min(Math.Abs(angularImpulse), _maxImpulse);
            _body2.angularVelocity += _body2.inverseMomentOfInertia*Math.Sign(angularImpulse)*
                                     Math.Min(Math.Abs(angularImpulse), _maxImpulse);
        }
    }
}