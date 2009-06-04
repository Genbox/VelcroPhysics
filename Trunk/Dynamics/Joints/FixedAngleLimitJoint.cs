using System.Xml.Serialization;

#if (XNA)
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    /// <summary>
    /// Fixed angle limit joint puts a body at an angle, with an upper and lower angle limit at it's current position.
    /// </summary>
    public class FixedAngleLimitJoint : Joint
    {
        public event FixedJointDelegate JointUpdated;

        private float _accumlatedAngularImpulseOld;
        private float _accumulatedAngularImpulse;
        private float _angularImpulse;
        private Body _body;
        private float _difference;
        private float _lowerLimit;
        private bool _lowerLimitViolated;
        private float _massFactor;
        private float _slop = .01f;
        private float _upperLimit;
        private bool _upperLimitViolated;
        private float _velocityBias;

        public FixedAngleLimitJoint()
        {
        }

        public FixedAngleLimitJoint(Body body, float lowerLimit, float upperLimit)
        {
            _body = body;
            _lowerLimit = lowerLimit;
            _upperLimit = upperLimit;
        }

#if(XNA)
        [ContentSerializerIgnore]
#endif
        [XmlIgnore]
        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        /// <Value>The body.</Value>
        public Body Body
        {
            get { return _body; }
            set { _body = value; }
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
            if (_body.IsDisposed)
            {
                Dispose();
            }
        }

        public override void PreStep(float inverseDt)
        {
            if (_body.isStatic)
                return;

            if (!_body.Enabled)
                return;

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
                    JointError = 0;
                }
                else
                {
                    JointError = _difference - _upperLimit;
                }
                _velocityBias = BiasFactor*inverseDt*JointError;
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
            _massFactor = (1 - Softness)/_body.inverseMomentOfInertia;
            _body.AngularVelocity += _body.inverseMomentOfInertia*_accumulatedAngularImpulse;
        }

        public override void Update()
        {
            base.Update();

            if (_body.isStatic)
                return;

            if (!_body.Enabled)
                return;

            if (!_upperLimitViolated && !_lowerLimitViolated)
                return;

            _angularImpulse = 0;
            _angularImpulse = -(_velocityBias + _body.AngularVelocity + Softness*_accumulatedAngularImpulse)*_massFactor;

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
                _body.AngularVelocity += _body.inverseMomentOfInertia * _angularImpulse;

                if (JointUpdated != null)
                    JointUpdated(this, _body);
            }
        }
    }
}