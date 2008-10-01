using System;

namespace FarseerGames.FarseerPhysics.Dynamics.Springs
{
    /// <summary>
    /// Puts a body at an angle at the body's current position. The angle is variable.
    /// </summary>
    public class FixedAngleSpring : Spring
    {
        private Body _body;
        private float _maxTorque = float.MaxValue;
        private float _targetAngle;
        private float _torqueMultiplier = 1f;

        public FixedAngleSpring()
        {
        }

        public FixedAngleSpring(Body body, float springConstant, float dampningConstant)
        {
            _body = body;
            SpringConstant = springConstant;
            DampningConstant = dampningConstant;
            _targetAngle = body.TotalRotation;
        }

        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        /// <value>The body.</value>
        public Body Body
        {
            get { return _body; }
            set { _body = value; }
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
        /// Gets or sets the max torque.
        /// </summary>
        /// <value>The max torque.</value>
        public float MaxTorque
        {
            get { return _maxTorque; }
            set { _maxTorque = value; }
        }

        /// <summary>
        /// The resultant torque will be multiplied by this value prior to being applied to the bodies.
        /// For normal spring behavior this value should be 1
        /// </summary>
        public float TorqueMultiplier
        {
            get { return _torqueMultiplier; }
            set { _torqueMultiplier = value; }
        }

        public override void Validate()
        {
            //if _body is disposed then dispose the joint.
            if (_body.IsDisposed)
            {
                Dispose();
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            
            if (IsDisposed)
                return;
            
            //calculate and apply spring force
            float angleDifference = _targetAngle - _body.totalRotation;
            float springTorque = SpringConstant*angleDifference;
            SpringError = angleDifference;

            //apply torque at anchor
            if (!_body.IsStatic)
            {
                float torque1 = springTorque - DampningConstant*_body.angularVelocity;
                torque1 = Math.Min(Math.Abs(torque1*_torqueMultiplier), _maxTorque)*Math.Sign(torque1);
                _body.ApplyTorque(torque1);
            }
        }
    }
}