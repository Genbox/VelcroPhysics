using System;
using FarseerGames.FarseerPhysics.Controllers;

namespace FarseerGames.FarseerPhysics.Dynamics.Springs
{
    public class FixedAngleSpring : Controller
    {
        private float _breakpoint = float.MaxValue;
        private float _dampningConstant;
        private float _maxTorque = float.MaxValue;
        private float _springConstant;

        private float _springError;
        private float _targetAngle;
        private float _torqueMultiplier = 1f;
        protected Body body;

        public FixedAngleSpring()
        {
        }

        public FixedAngleSpring(Body body, float springConstant, float dampningConstant)
        {
            this.body = body;
            _springConstant = springConstant;
            _dampningConstant = dampningConstant;
            _targetAngle = body.TotalRotation;
        }

        public Body Body
        {
            get { return body; }
            set { body = value; }
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
            //if body is disposed then dispose the joint.
            if (body.IsDisposed)
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
            if (isDisposed)
            {
                return;
            }
            //calculate and apply spring force
            float angleDifference = _targetAngle - body.totalRotation;
            float springTorque = _springConstant*angleDifference;
            _springError = angleDifference;

            //apply torque at anchor
            if (!body.IsStatic)
            {
                float torque1 = springTorque - _dampningConstant*body.angularVelocity;
                torque1 = Math.Min(Math.Abs(torque1*_torqueMultiplier), _maxTorque)*Math.Sign(torque1);
                body.ApplyTorque(torque1);
            }
        }
    }
}