using System;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    /// <summary>
    /// Fixed angle joint put a body at an angle in it's current position
    /// </summary>
    public class FixedAngleJoint : Joint
    {
        private float _biasFactor = .2f;
        private float _breakpoint = float.MaxValue;

        private float _jointError;
        private float _massFactor;
        private float _maxImpulse = float.MaxValue;
        private float _softness;
        private float _targetAngle;
        private float _velocityBias;
        private Body _body;

        public FixedAngleJoint()
        {
        }

        public FixedAngleJoint(Body body)
        {
            _body = body;
        }

        public FixedAngleJoint(Body body, float targetAngle)
        {
            _body = body;
            _targetAngle = targetAngle;
        }

        public Body Body
        {
            get { return _body; }
            set { _body = value; }
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
            if (_body.IsDisposed)
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
            _jointError = _body.totalRotation - _targetAngle;

            _velocityBias = -_biasFactor*inverseDt*_jointError;
            _massFactor = (1 - _softness)/(_body.inverseMomentOfInertia);
        }

        public override void Update()
        {
            if (IsDisposed)
            {
                return;
            }
            float angularImpulse = (_velocityBias - _body.angularVelocity)*_massFactor;
            _body.angularVelocity += _body.inverseMomentOfInertia*Math.Sign(angularImpulse)*
                                    Math.Min(Math.Abs(angularImpulse), _maxImpulse);
        }
    }
}