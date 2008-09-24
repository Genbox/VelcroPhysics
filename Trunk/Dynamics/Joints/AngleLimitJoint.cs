using System;
using FarseerGames.FarseerPhysics.Mathematics;
#if (XNA)
using Microsoft.Xna.Framework; 
#endif

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    /// <summary>
    /// Angle limit joint joins together 2 bodies at an upper and lower angel limit.
    /// </summary>
    public class AngleLimitJoint : Joint
    {
        private float _accumlatedAngularImpulseOld;
        private float _accumulatedAngularImpulse;
        private float _angularImpulse;
        private Body _body1;
        private Body _body2;
        private float _difference;
        private float _lowerLimit;
        private bool _lowerLimitViolated;
        private float _massFactor;

        private float _slop = .01f;
        private float _upperLimit;
        private bool _upperLimitViolated;
        private float _velocityBias;

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

        public float Slop
        {
            get { return _slop; }
            set { _slop = value; }
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
            if (Enabled && Math.Abs(JointError) > Breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }
            if (IsDisposed)
            {
                return;
            }
            _difference = (_body2.totalRotation - _body1.totalRotation);
            JointError = 0;

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
                    JointError = 0;
                }
                else
                {
                    JointError = _difference - _upperLimit;
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
                    JointError = 0;
                }
                else
                {
                    JointError = _difference - _lowerLimit;
                }
            }
            else
            {
                _upperLimitViolated = false;
                _lowerLimitViolated = false;
                JointError = 0;
                _accumulatedAngularImpulse = 0;
            }
            _velocityBias = BiasFactor*inverseDt*JointError;

            _massFactor = 1/(Softness + _body1.inverseMomentOfInertia + _body2.inverseMomentOfInertia);

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
                -(_velocityBias + (_body2.angularVelocity - _body1.angularVelocity) +
                  Softness*_accumulatedAngularImpulse)*
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