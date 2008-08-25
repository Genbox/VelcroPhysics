using System;
using FarseerGames.FarseerPhysics.Controllers;

namespace FarseerGames.FarseerPhysics.Dynamics.Springs
{
    public class AngleSpring : Controller
    {
        private float _targetAngle;

        public AngleSpring()
        {
            TorqueMultiplier = 1f;
            MaxTorque = float.MaxValue;
            Breakpoint = float.MaxValue;
        }

        public AngleSpring(Body body1, Body body2, float springConstant, float dampningConstant)
        {
            TorqueMultiplier = 1f;
            MaxTorque = float.MaxValue;
            Breakpoint = float.MaxValue;
            Body1 = body1;
            Body2 = body2;
            SpringConstant = springConstant;
            DampningConstant = dampningConstant;
            _targetAngle = Body2.TotalRotation - Body1.TotalRotation;
        }

        public Body Body1 { get; set; }
        public Body Body2 { get; set; }
        public float SpringConstant { get; set; }
        public float DampningConstant { get; set; }

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

        public float Breakpoint { get; set; }
        public float MaxTorque { get; set; }

        /// <summary>
        /// The resultant torque will be multiplied by this value prior to being applied to the bodies.
        /// For normal spring behavior this value should be 1
        /// </summary>
        public float TorqueMultiplier { get; set; }
        public float SpringError { get; private set; }
        public event EventHandler<EventArgs> Broke;

        public override void Validate()
        {
            //if either of the springs connected bodies are disposed then dispose the joint.
            if (Body1.IsDisposed || Body2.IsDisposed)
            {
                Dispose();
            }
        }

        public override void Update(float dt)
        {
            if (Enabled && Math.Abs(SpringError) > Breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }

            if (IsDisposed)
            {
                return;
            }
            //calculate and apply spring force

            float angleDifference = Body2.TotalRotation - (Body1.TotalRotation + _targetAngle);
            float springTorque = SpringConstant*angleDifference;
            SpringError = angleDifference; //keep track of 'springError' for breaking joint

            //apply torque at anchor
            if (!Body1.IsStatic)
            {
                float torque1 = springTorque - DampningConstant*Body1.angularVelocity;
                torque1 = Math.Min(Math.Abs(torque1*TorqueMultiplier), MaxTorque)*Math.Sign(torque1);
                Body1.ApplyTorque(torque1);
            }

            if (!Body2.IsStatic)
            {
                float torque2 = -springTorque - DampningConstant*Body2.angularVelocity;
                torque2 = Math.Min(Math.Abs(torque2*TorqueMultiplier), MaxTorque)*Math.Sign(torque2);
                Body2.ApplyTorque(torque2);
            }
        }
    }
}