using System;
using FarseerGames.FarseerPhysics.Controllers;

namespace FarseerGames.FarseerPhysics.Dynamics.Springs
{
    /// <summary>
    /// Puts a body at an angle at the body's current position. The angle is variable.
    /// </summary>
    public class FixedAngleSpring : Controller
    {
        private float _breakpoint = float.MaxValue;
        private float _dampningConstant;
        private float _maxTorque = float.MaxValue;
        private float _springConstant;

        private float _springError;
        private float _targetAngle;
        private float _torqueMultiplier = 1f;
        private Body _body;

        public FixedAngleSpring()
        {
        }

        public FixedAngleSpring(Body body, float springConstant, float dampningConstant)
        {
            _body = body;
            _springConstant = springConstant;
            _dampningConstant = dampningConstant;
            _targetAngle = body.TotalRotation;
        }

        public Body Body
        {
            get { return _body; }
            set { _body = value; }
        }

        public float SpringConstant
        {
            get { return _springConstant; }
            set { _springConstant = value; }
        }

        public float DampningConstant
        {
            get { return _dampningConstant; }
            set { _dampningConstant = value; }
        }

        public float TargetAngle
        {
            get { return _targetAngle; }
            set { _targetAngle = value; }
        }

        public float Breakpoint
        {
            get { return _breakpoint; }
            set { _breakpoint = value; }
        }

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

        public float SpringError
        {
            get { return _springError; }
        }

        public event EventHandler<EventArgs> Broke;

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
            if (Enabled && Math.Abs(_springError) > _breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }
            if (IsDisposed)
            {
                return;
            }
            //calculate and apply spring force
            float angleDifference = _targetAngle - _body.totalRotation;
            float springTorque = _springConstant*angleDifference;
            _springError = angleDifference;

            //apply torque at anchor
            if (!_body.IsStatic)
            {
                float torque1 = springTorque - _dampningConstant*_body.angularVelocity;
                torque1 = Math.Min(Math.Abs(torque1*_torqueMultiplier), _maxTorque)*Math.Sign(torque1);
                _body.ApplyTorque(torque1);
            }
        }
    }
}