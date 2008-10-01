using System;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    /// <summary>
    /// Angle joint joins together 2 bodies at an angle
    /// </summary>
    public class AngleJoint : Joint
    {
        private Body _body1;
        private Body _body2;
        private float _massFactor;
        private float _maxImpulse = float.MaxValue;
        private float _targetAngle;
        private float _velocityBias;

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

        /// <summary>
        /// Gets or sets the fist body.
        /// </summary>
        /// <value>The body1.</value>
        public Body Body1
        {
            get { return _body1; }
            set { _body1 = value; }
        }

        /// <summary>
        /// Gets or sets the second body.
        /// </summary>
        /// <value>The body2.</value>
        public Body Body2
        {
            get { return _body2; }
            set { _body2 = value; }
        }

        /// <summary>
        /// Gets or sets the target angle.
        /// </summary>
        /// <value>The target angle.</value>
        public float TargetAngle
        {
            get { return _targetAngle; }
            set { _targetAngle = value; }
        }

        /// <summary>
        /// Gets or sets the max impulse.
        /// </summary>
        /// <value>The max impulse.</value>
        public float MaxImpulse
        {
            get { return _maxImpulse; }
            set { _maxImpulse = value; }
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
            if (IsDisposed)
                return;

            JointError = (_body2.totalRotation - _body1.totalRotation) - _targetAngle;

            _velocityBias = -BiasFactor*inverseDt*JointError;

            _massFactor = (1 - Softness)/(_body1.inverseMomentOfInertia + _body2.inverseMomentOfInertia);
        }

        public override void Update()
        {
            base.Update();

            if (IsDisposed)
                return;

            float angularImpulse = (_velocityBias - _body2.angularVelocity + _body1.angularVelocity)*_massFactor;

            _body1.angularVelocity -= _body1.inverseMomentOfInertia*Math.Sign(angularImpulse)*
                                      Math.Min(Math.Abs(angularImpulse), _maxImpulse);
            _body2.angularVelocity += _body2.inverseMomentOfInertia*Math.Sign(angularImpulse)*
                                      Math.Min(Math.Abs(angularImpulse), _maxImpulse);
        }
    }
}