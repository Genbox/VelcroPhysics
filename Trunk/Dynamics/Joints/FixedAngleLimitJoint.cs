using System;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    /// <summary>
    /// Fixed angle limit joint puts a body at an angle, with an upper and lower angle limit at it's current position.
    /// </summary>
    public class FixedAngleLimitJoint : Joint
    {
        private float _accumlatedAngularImpulseOld;
        private float _accumulatedAngularImpulse;
        private float _angularImpulse;
        private float _biasFactor = .2f;
        private float _breakpoint = float.MaxValue;
        private float _difference;
        private float _jointError;
        private float _lowerLimit;
        private bool _lowerLimitViolated;
        private float _massFactor;
        private float _slop = .01f;
        private float _softness;
        private float _upperLimit;
        private bool _upperLimitViolated;
        private float _velocityBias;
        private Body _body;

        public FixedAngleLimitJoint()
        {
        }

        public FixedAngleLimitJoint(Body body, float lowerLimit, float upperLimit)
        {
            _body = body;
            _lowerLimit = lowerLimit;
            _upperLimit = upperLimit;
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

        public float Slop
        {
            get { return _slop; }
            set { _slop = value; }
        }

        public float Softness
        {
            get { return _softness; }
            set { _softness = value; }
        }

        public float UpperLimit
        {
            get { return _upperLimit; }
            set { _upperLimit = value; }
        }

        public float LowerLimit
        {
            get { return _lowerLimit; }
            set { _lowerLimit = value; }
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
            _difference = _body.totalRotation;

            if (_difference > _upperLimit)
            {
                if (_lowerLimitViolated)
                {
                    _accumulatedAngularImpulse = 0;
                    _lowerLimitViolated = false;
                }
                _upperLimitViolated = true;
                if (_difference < _upperLimit + _slop)
                {
                    _jointError = 0;
                }
                else
                {
                    _jointError = _difference - _upperLimit;
                }
                _velocityBias = _biasFactor*inverseDt*_jointError;
            }
            else if (_difference < _lowerLimit)
            {
                if (_upperLimitViolated)
                {
                    _accumulatedAngularImpulse = 0;
                    _upperLimitViolated = false;
                }
                _lowerLimitViolated = true;
                if (_difference > _lowerLimit - _slop)
                {
                    _jointError = 0;
                }
                else
                {
                    _jointError = _difference - _lowerLimit;
                }
            }
            else
            {
                _upperLimitViolated = false;
                _lowerLimitViolated = false;
                _jointError = 0;
                _accumulatedAngularImpulse = 0;
            }
            _velocityBias = _biasFactor*inverseDt*_jointError;
            _massFactor = (1 - _softness)/_body.inverseMomentOfInertia;
            _body.angularVelocity += _body.inverseMomentOfInertia*_accumulatedAngularImpulse;
        }

        public override void Update()
        {
            if (IsDisposed)
            {
                return;
            }
            if (!_upperLimitViolated && !_lowerLimitViolated)
            {
                return;
            }

            _angularImpulse = 0;
            _angularImpulse = -(_velocityBias + _body.angularVelocity + _softness*_accumulatedAngularImpulse)*_massFactor;

            _accumlatedAngularImpulseOld = _accumulatedAngularImpulse;

            if (_upperLimitViolated)
            {
                _accumulatedAngularImpulse = MathHelper.Min(_accumlatedAngularImpulseOld + _angularImpulse, 0);
            }
            else if (_lowerLimitViolated)
            {
                _accumulatedAngularImpulse = MathHelper.Max(_accumlatedAngularImpulseOld + _angularImpulse, 0);
            }

            _angularImpulse = _accumulatedAngularImpulse - _accumlatedAngularImpulseOld;

            _body.angularVelocity += _body.inverseMomentOfInertia*_angularImpulse;
        }
    }
}