using System;

namespace FarseerGames.FarseerPhysics.Dynamics.Springs
{
    /// <summary>
    /// Puts a body at an angle at the body's current position. The angle is variable.
    /// </summary>
    public class FixedAngleSpring : Spring
    {
        public event FixedSpringDelegate SpringUpdated;

        private Body _body;
        private float _maxTorque = float.MaxValue;
        private float _targetAngle;
        private float _torqueMultiplier = 1f;

        public FixedAngleSpring()
        {
        }

        public FixedAngleSpring(Body body, float springConstant, float dampingConstant)
        {
            _body = body;
            SpringConstant = springConstant;
            DampingConstant = dampingConstant;
            _targetAngle = body.TotalRotation;
        }

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
        /// Gets or sets the target angle.
        /// </summary>
        /// <Value>The target angle.</Value>
        public float TargetAngle
        {
            get { return _targetAngle; }
            set { _targetAngle = value; }
        }

        /// <summary>
        /// Gets or sets the max torque.
        /// </summary>
        /// <Value>The max torque.</Value>
        public float MaxTorque
        {
            get { return _maxTorque; }
            set { _maxTorque = value; }
        }

        /// <summary>
        /// The resultant torque will be multiplied by this Value prior to being applied to the bodies.
        /// For normal spring behavior this Value should be 1
        /// </summary>
        public float TorqueMultiplier
        {
            get { return _torqueMultiplier; }
            set { _torqueMultiplier = value; }
        }

        public override void Validate()
        {
            //if body is disposed then dispose the spring.
            if (_body.IsDisposed)
            {
                Dispose();
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            if (_body.isStatic)
                return;

            if (!_body.Enabled)
                return;

            //Calculate and apply spring force
            float angleDifference = _targetAngle - _body.totalRotation;
            float springTorque = SpringConstant * angleDifference;
            SpringError = angleDifference;

            //apply torque at anchor
            float torque1 = springTorque - DampingConstant * _body.AngularVelocity;
            torque1 = Math.Min(Math.Abs(torque1 * _torqueMultiplier), _maxTorque) * Math.Sign(torque1);

            if (torque1 != 0f)
            {
                _body.ApplyTorque(torque1);

                if (SpringUpdated != null)
                    SpringUpdated(this, _body);
            }
        }
    }
}