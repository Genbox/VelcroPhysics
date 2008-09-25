using System;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    public class AngleLimitJoint : Joint
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
        private Body _body1;
        private Body _body2;

        public AngleLimitJoint()
        {
        }

        public AngleLimitJoint(Body body1, Body body2, float lowerLimit, float upperLimit)
        {
            _body1 = body1;
            _body2 = body2;
            _lowerLimit = lowerLimit;
            _upperLimit = upperLimit;
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
            _difference = (_body2.totalRotation - _body1.totalRotation);
            _jointError = 0;

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

            _massFactor = 1/(_softness + _body1.inverseMomentOfInertia + _body2.inverseMomentOfInertia);

            _body1.angularVelocity -= _body1.inverseMomentOfInertia*_accumulatedAngularImpulse;
            _body2.angularVelocity += _body2.inverseMomentOfInertia*_accumulatedAngularImpulse;
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
            _angularImpulse =
                -(_velocityBias + (_body2.angularVelocity - _body1.angularVelocity) + _softness*_accumulatedAngularImpulse)*
                _massFactor;

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

            _body1.angularVelocity -= _body1.inverseMomentOfInertia*_angularImpulse;
            _body2.angularVelocity += _body2.inverseMomentOfInertia*_angularImpulse;
        }
    }
}