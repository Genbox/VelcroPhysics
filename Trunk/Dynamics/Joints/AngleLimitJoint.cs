using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    public class AngleLimitJoint : Joint
    {
        private float _accumlatedAngularImpulseOld;
        private float _accumulatedAngularImpulse;
        private float _angularImpulse;
        private float _difference;
        private bool _lowerLimitViolated;
        private float _massFactor;
        private bool _upperLimitViolated;
        private float _velocityBias;

        public AngleLimitJoint()
        {
            Breakpoint = float.MaxValue;
            Slop = .01f;
            BiasFactor = .2f;
        }

        public AngleLimitJoint(Body body1, Body body2, float lowerLimit, float upperLimit)
        {
            Breakpoint = float.MaxValue;
            Slop = .01f;
            BiasFactor = .2f;
            Body1 = body1;
            Body2 = body2;
            LowerLimit = lowerLimit;
            UpperLimit = upperLimit;
        }

        public Body Body1 { get; set; }
        public Body Body2 { get; set; }
        public float Slop { get; set; }
        public float UpperLimit { get; set; }
        public float LowerLimit { get; set; }

        public override void Validate()
        {
            if (Body1.IsDisposed || Body2.IsDisposed)
            {
                Dispose();
            }
        }

        public override void PreStep(float inverseDt)
        {
            base.PreStep(inverseDt);

            if (IsDisposed) return;

            _difference = (Body2.TotalRotation - Body1.TotalRotation);
            Error = 0;

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
                    Error = 0;
                }
                else
                {
                    Error = _difference - UpperLimit;
                }
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
                    Error = 0;
                }
                else
                {
                    Error = _difference - LowerLimit;
                }
            }
            else
            {
                _upperLimitViolated = false;
                _lowerLimitViolated = false;
                Error = 0;
                _accumulatedAngularImpulse = 0;
            }
            _velocityBias = BiasFactor*inverseDt*Error;

            _massFactor = 1/(Softness + Body1.InverseMomentOfInertia + Body2.InverseMomentOfInertia);

            Body1.angularVelocity -= Body1.InverseMomentOfInertia*_accumulatedAngularImpulse;
            Body2.angularVelocity += Body2.InverseMomentOfInertia*_accumulatedAngularImpulse;
        }

        public override void Update()
        {
            if (IsDisposed) return;
            if (!_upperLimitViolated && !_lowerLimitViolated) return;

            _angularImpulse = 0;
            _angularImpulse =
                -(_velocityBias + (Body2.angularVelocity - Body1.angularVelocity) + Softness*_accumulatedAngularImpulse)*
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

            Body1.angularVelocity -= Body1.InverseMomentOfInertia*_angularImpulse;
            Body2.angularVelocity += Body2.InverseMomentOfInertia*_angularImpulse;
        }
    }
}