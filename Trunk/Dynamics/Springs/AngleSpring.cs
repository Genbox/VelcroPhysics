using System;

namespace FarseerGames.FarseerPhysics.Dynamics.Springs
{
    /// <summary>
    /// Puts 2 bodies at an angle. The angle is variable.
    /// </summary>
    public class AngleSpring : Spring
    {
        private Body _body1;
        private Body _body2;
        private float _maxTorque = float.MaxValue;
        private float _targetAngle;
        private float _torqueMultiplier = 1f;

        public AngleSpring()
        {
        }

        public AngleSpring(Body body1, Body body2, float springConstant, float dampingConstant)
        {
            _body1 = body1;
            _body2 = body2;
            SpringConstant = springConstant;
            DampingConstant = dampingConstant;
            _targetAngle = _body2.TotalRotation - _body1.TotalRotation;
        }

        /// <summary>
        /// Gets or sets the first body.
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

        //TODO: magic numbers
        /// <summary>
        /// Gets or sets the target angle.
        /// </summary>
        /// <Value>The target angle.</Value>
        public float TargetAngle
        {
            get { return _targetAngle; }
            set
            {
                _targetAngle = value;
                if (_targetAngle > 5.5)
                {
                    _targetAngle = 5.5f;
                }
                if (_targetAngle < -5.5f)
                {
                    _targetAngle = -5.5f;
                }
            }
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
            //if either of the springs connected bodies are disposed then dispose the joint.
            if (_body1.IsDisposed || _body2.IsDisposed)
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
            float angleDifference = _body2.totalRotation - (_body1.totalRotation + _targetAngle);
            float springTorque = SpringConstant*angleDifference;
            SpringError = angleDifference; //keep track of '_springError' for breaking joint

            //apply torque at anchor
            if (!_body1.IsStatic)
            {
                float torque1 = springTorque - DampingConstant*_body1.AngularVelocity;
                torque1 = Math.Min(Math.Abs(torque1*_torqueMultiplier), _maxTorque)*Math.Sign(torque1);
                _body1.ApplyTorque(torque1);
            }

            if (!_body2.IsStatic)
            {
                float torque2 = -springTorque - DampingConstant*_body2.AngularVelocity;
                torque2 = Math.Min(Math.Abs(torque2*_torqueMultiplier), _maxTorque)*Math.Sign(torque2);
                _body2.ApplyTorque(torque2);
            }
        }
    }
}