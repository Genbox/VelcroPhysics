using System;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    /// <summary>
    /// Fixed angle joint put a body at an angle in it's current position
    /// </summary>
    public class FixedAngleJoint : Joint
    {
        private Body _body;
        private float _massFactor;
        private float _maxImpulse = float.MaxValue;
        private float _targetAngle;
        private float _velocityBias;

        public FixedAngleJoint()
        {
        }

        public FixedAngleJoint(Body body)
        {
            _body = body;
        }

        public FixedAngleJoint(Body body, float targetAngle)
        {
            _body = body;
            _targetAngle = targetAngle;
        }

        public Body Body
        {
            get { return _body; }
            set { _body = value; }
        }

        public float TargetAngle
        {
            get { return _targetAngle; }
            set { _targetAngle = value; }
        }

        public float MaxImpulse
        {
            get { return _maxImpulse; }
            set { _maxImpulse = value; }
        }

        public event EventHandler<EventArgs> Broke;

        public override void Validate()
        {
            if (_body.IsDisposed)
            {
                Dispose();
            }
        }

        public override void PreStep(float inverseDt)
        {
            if (Enabled && Math.Abs(JointError) > Breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }
            if (IsDisposed)
            {
                return;
            }
            JointError = _body.totalRotation - _targetAngle;

            _velocityBias = -BiasFactor*inverseDt*JointError;
            _massFactor = (1 - Softness)/(_body.inverseMomentOfInertia);
        }

        public override void Update()
        {
            if (IsDisposed)
            {
                return;
            }
            float angularImpulse = (_velocityBias - _body.angularVelocity)*_massFactor;
            _body.angularVelocity += _body.inverseMomentOfInertia*Math.Sign(angularImpulse)*
                                     Math.Min(Math.Abs(angularImpulse), _maxImpulse);
        }
    }
}