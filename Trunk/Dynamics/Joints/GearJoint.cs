namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    /// <summary>

    /// Angle joint joins together 2 bodies at an angle

    /// </summary>

    public class GearJoint : Joint
    {
        public event JointDelegate JointUpdated;

        private Body _body1;
        private Body _body2;
        private float _massFactor;
        private float _maxImpulse = float.MaxValue;
        private float _velocityBias;
        private float _ratio;

        public GearJoint()
        {
        }

        public GearJoint(Body body1, Body body2)
        {
            _body1 = body1;
            _body2 = body2;
        }

        public GearJoint(Body body1, Body body2, float ratio)
        {
            _body1 = body1;
            _body2 = body2;
            _ratio = ratio;
        }

        /// <summary>

        /// Gets or sets the fist body.

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

        /// <summary>

        /// Gets or sets the ratio.

        /// </summary>

        /// <Value>The ratio.</Value>

        public float Ratio
        {
            get { return _ratio; }
            set { _ratio = value; }
        }

        /// <summary>

        /// Gets or sets the max impulse.

        /// </summary>

        /// <Value>The max impulse.</Value>

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
            if (_body1.isStatic && _body2.isStatic)
                return;

            if (!_body1.Enabled && !_body2.Enabled)
                return;
        }

        public override void Update()
        {
            base.Update();

            if (_body1.isStatic && _body2.isStatic)
                return;

            if (!_body1.Enabled && !_body2.Enabled)
                return;

            float vel1 = -(_body1.AngularVelocity * _ratio);
            //float vel2 = (_body2.AngularVelocity * (1f / _ratio));

            _body2.AngularVelocity = vel1 * 0.997f;

            if (JointUpdated != null)
                JointUpdated(this, _body1, _body2);
        }
    }
}
