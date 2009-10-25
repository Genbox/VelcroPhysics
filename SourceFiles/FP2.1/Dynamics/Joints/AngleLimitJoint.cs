#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    /// <summary>
    /// Angle limit joint joins together 2 bodies at an upper and lower angel limit.
    /// </summary>
    public class AngleLimitJoint : Joint
    {
        public event JointDelegate JointUpdated;

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

        /// <summary>
        /// Gets or sets the first body
        /// </summary>
        /// <Value>The body1.</Value>
        public Body Body1
        {
            get { return _body1; }
            set { _body1 = value; }
        }

        /// <summary>
        /// Gets or sets the second body.
        /// </summary>
        /// <Value>The body2.</Value>
        public Body Body2
        {
            get { return _body2; }
            set { _body2 = value; }
        }

        /// <summary>
        /// Gets or sets the slop.
        /// </summary>
        /// <Value>The slop.</Value>
        public float Slop
        {
            get { return _slop; }
            set { _slop = value; }
        }

        /// <summary>
        /// Gets or sets the upper limit.
        /// </summary>
        /// <Value>The upper limit.</Value>
        public float UpperLimit
        {
            get { return _upperLimit; }
            set { _upperLimit = value; }
        }

        /// <summary>
        /// Gets or sets the lower limit.
        /// </summary>
        /// <Value>The lower limit.</Value>
        public float LowerLimit
        {
            get { return _lowerLimit; }
            set { _lowerLimit = value; }
        }

        public override void Validate()
        {
            if (_body1.IsDisposed || _body2.IsDisposed)
            {
                Dispose();
            }
        }

        public override void PreStep(float inverseDt)
        {
            if (_body1.isStatic && _body2.isStatic)
                return;

            if (!_body1.Enabled && !_body2.Enabled)
                return;

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
            _velocityBias = BiasFactor * inverseDt * JointError;

            _massFactor = 1 / (Softness + _body1.inverseMomentOfInertia + _body2.inverseMomentOfInertia);

            _body1.AngularVelocity -= _body1.inverseMomentOfInertia * _accumulatedAngularImpulse;
            _body2.AngularVelocity += _body2.inverseMomentOfInertia * _accumulatedAngularImpulse;
        }

        public override void Update()
        {
            base.Update();

            if (_body1.isStatic && _body2.isStatic)
                return;

            if (!_body1.Enabled && !_body2.Enabled)
                return;

            if (!_upperLimitViolated && !_lowerLimitViolated)
                return;

            _angularImpulse = 0;
            _angularImpulse =
                -(_velocityBias + (_body2.AngularVelocity - _body1.AngularVelocity) +
                  Softness * _accumulatedAngularImpulse) *
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

            if (_angularImpulse != 0f)
            {
                _body1.AngularVelocity -= _body1.inverseMomentOfInertia * _angularImpulse;
                _body2.AngularVelocity += _body2.inverseMomentOfInertia * _angularImpulse;

                if (JointUpdated != null)
                    JointUpdated(this, _body1, _body2);
            }
        }
    }
}