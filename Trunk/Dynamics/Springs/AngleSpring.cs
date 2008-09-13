using System;
using FarseerGames.FarseerPhysics.Controllers;

namespace FarseerGames.FarseerPhysics.Dynamics.Springs
{
    public class AngleSpring : Controller
    {
        private float _breakpoint = float.MaxValue;
        private float _dampningConstant;
        private float _maxTorque = float.MaxValue;
        private float _springConstant;

        private float _springError;
        private float _targetAngle;
        private float _torqueMultiplier = 1f;
        private Body _body1;
        private Body _body2;

        public AngleSpring()
        {
        }

        public AngleSpring(Body body1, Body body2, float springConstant, float dampningConstant)
        {
            _body1 = body1;
            _body2 = body2;
            _springConstant = springConstant;
            _dampningConstant = dampningConstant;
            _targetAngle = _body2.TotalRotation - _body1.TotalRotation;
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

        //TODO: magic numbers
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
            //if either of the springs connected bodies are disposed then dispose the joint.
            if (_body1.IsDisposed || _body2.IsDisposed)
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
            float angleDifference = _body2.totalRotation - (_body1.totalRotation + _targetAngle);
            float springTorque = _springConstant*angleDifference;
            _springError = angleDifference; //keep track of '_springError' for breaking joint

            //apply torque at anchor
            if (!_body1.IsStatic)
            {
                float torque1 = springTorque - _dampningConstant*_body1.angularVelocity;
                torque1 = Math.Min(Math.Abs(torque1*_torqueMultiplier), _maxTorque)*Math.Sign(torque1);
                _body1.ApplyTorque(torque1);
            }

            if (!_body2.IsStatic)
            {
                float torque2 = -springTorque - _dampningConstant*_body2.angularVelocity;
                torque2 = Math.Min(Math.Abs(torque2*_torqueMultiplier), _maxTorque)*Math.Sign(torque2);
                _body2.ApplyTorque(torque2);
            }
        }
    }
}