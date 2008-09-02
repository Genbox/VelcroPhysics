using System;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    public class AngleLimitJoint : Joint
    {
        private float _accumlatedAngularImpulseOld;
        private float _accumulatedAngularImpulse;
        private float _angularImpulse;
        private float _biasFactor = .2f;
        protected Body body1;
        protected Body body2;
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

        public AngleLimitJoint()
        {
        }

        public AngleLimitJoint(Body body1, Body body2, float lowerLimit, float upperLimit)
        {
            this.body1 = body1;
            this.body2 = body2;
            _lowerLimit = lowerLimit;
            _upperLimit = upperLimit;
        }

        public Body Body1
        {
            get { return body1; }
            set { body1 = value; }
        }

        public Body Body2
        {
            get { return body2; }
            set { body2 = value; }
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
            if (body1.IsDisposed || body2.IsDisposed)
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
            _difference = (body2.totalRotation - body1.totalRotation);
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

            _massFactor = 1/(_softness + body1.inverseMomentOfInertia + body2.inverseMomentOfInertia);

            body1.angularVelocity -= body1.inverseMomentOfInertia*_accumulatedAngularImpulse;
            body2.angularVelocity += body2.inverseMomentOfInertia*_accumulatedAngularImpulse;
        }

        public override void Update()
        {
            if (isDisposed)
            {
                return;
            }
            if (!_upperLimitViolated && !_lowerLimitViolated)
            {
                return;
            }
            _angularImpulse = 0;
            _angularImpulse =
                -(_velocityBias + (body2.angularVelocity - body1.angularVelocity) + _softness*_accumulatedAngularImpulse)*
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

            body1.angularVelocity -= body1.inverseMomentOfInertia*_angularImpulse;
            body2.angularVelocity += body2.inverseMomentOfInertia*_angularImpulse;
        }
    }
}