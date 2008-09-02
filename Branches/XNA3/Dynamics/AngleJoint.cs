using System;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public class AngleJoint : Joint
    {
        private float _biasFactor = .2f;
        protected Body body1;
        protected Body body2;

        private float _breakpoint = float.MaxValue;

        private float _jointError;
        private float _massFactor;
        private float _maxImpulse = float.MaxValue;
        private float _softness;
        private float _targetAngle;
        private float _velocityBias;

        public AngleJoint()
        {
        }

        public AngleJoint(Body body1, Body body2)
        {
            this.body1 = body1;
            this.body2 = body2;
        }

        public AngleJoint(Body body1, Body body2, float targetAngle)
        {
            this.body1 = body1;
            this.body2 = body2;
            _targetAngle = targetAngle;
        }

        public Body Body1
        {
            get { return body1; }
            set { body1 = value; }
        }

        public Body Body2
        {
            get { return body2; }
            set { body2 = value; }
        }

        public float BiasFactor
        {
            get { return _biasFactor; }
            set { _biasFactor = value; }
        }

        public float TargetAngle
        {
            get { return _targetAngle; }
            set { _targetAngle = value; }
        }

        public float Softness
        {
            get { return _softness; }
            set { _softness = value; }
        }

        public float MaxImpulse
        {
            get { return _maxImpulse; }
            set { _maxImpulse = value; }
        }

        public float Breakpoint
        {
            get { return _breakpoint; }
            set { _breakpoint = value; }
        }

        public float JointError
        {
            get { return _jointError; }
        }

        public event EventHandler<EventArgs> Broke;

        public override void Validate()
        {
            if (body1.IsDisposed || body2.IsDisposed)
            {
                Dispose();
            }
        }

        public override void PreStep(float inverseDt)
        {
            if (Enabled && Math.Abs(_jointError) > _breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }

            if (isDisposed)
            {
                return;
            }
            _jointError = (body2.totalRotation - body1.totalRotation) - _targetAngle;

            _velocityBias = -_biasFactor*inverseDt*_jointError;

            _massFactor = (1 - _softness)/(body1.inverseMomentOfInertia + body2.inverseMomentOfInertia);
        }

        public override void Update()
        {
            if (isDisposed) return;
            if (!Enabled) return;
            float angularImpulse = (_velocityBias - body2.angularVelocity + body1.angularVelocity)*_massFactor;

            body1.angularVelocity -= body1.inverseMomentOfInertia*Math.Sign(angularImpulse)*
                                     Math.Min(Math.Abs(angularImpulse), _maxImpulse);
            body2.angularVelocity += body2.inverseMomentOfInertia*Math.Sign(angularImpulse)*
                                     Math.Min(Math.Abs(angularImpulse), _maxImpulse);
        }
    }
}