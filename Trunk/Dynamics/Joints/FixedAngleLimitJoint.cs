using System;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    public class FixedAngleLimitJoint : Joint
    {
        private float _accumlatedAngularImpulseOld;
        private float _accumulatedAngularImpulse;
        private float _angularImpulse;
        private float _difference;
        private bool _lowerLimitViolated;
        private float _massFactor;
        private bool _upperLimitViolated;
        private float _velocityBias;

        public FixedAngleLimitJoint()
        {
            Breakpoint = float.MaxValue;
            Slop = .01f;
            BiasFactor = .2f;
        }

        public FixedAngleLimitJoint(Body body, float lowerLimit, float upperLimit)
        {
            Breakpoint = float.MaxValue;
            Slop = .01f;
            BiasFactor = .2f;
            Body = body;
            LowerLimit = lowerLimit;
            UpperLimit = upperLimit;
        }

        public Body Body { get; set; }
        public float Slop { get; set; }
        public float UpperLimit { get; set; }
        public float LowerLimit { get; set; }

        public override void Validate()
        {
            if (Body.IsDisposed)
            {
                Dispose();
            }
        }

        public override void PreStep(float inverseDt)
        {
            base.PreStep(inverseDt);

            if (IsDisposed) return;

            _difference = Body.TotalRotation;

            if (_difference > UpperLimit)
            {
                if (_lowerLimitViolated)
                {
                    _accumulatedAngularImpulse = 0;
                    _lowerLimitViolated = false;
                }
                _upperLimitViolated = true;
                if (_difference < UpperLimit + Slop)
                {
                    JointError = 0;
                }
                else
                {
                    JointError = _difference - UpperLimit;
                }
                _velocityBias = BiasFactor * inverseDt * JointError;
            }
            else if (_difference < LowerLimit)
            {
                if (_upperLimitViolated)
                {
                    _accumulatedAngularImpulse = 0;
                    _upperLimitViolated = false;
                }
                _lowerLimitViolated = true;
                if (_difference > LowerLimit - Slop)
                {
                    JointError = 0;
                }
                else
                {
                    JointError = _difference - LowerLimit;
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
            _massFactor = (1 - Softness) / Body.inverseMomentOfInertia;
            Body.angularVelocity += Body.inverseMomentOfInertia * _accumulatedAngularImpulse;
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
            _angularImpulse = -(_velocityBias + Body.angularVelocity + Softness * _accumulatedAngularImpulse) * _massFactor;

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

            Body.angularVelocity += Body.inverseMomentOfInertia * _angularImpulse;
        }
    }
}