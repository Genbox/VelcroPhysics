using System;
using System.Xml.Serialization;

#if(XNA)
using Microsoft.Xna.Framework.Content;
#endif

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    /// <summary>
    /// Angle joint joins together 2 bodies at an angle
    /// </summary>
    public class AngleJoint : Joint
    {
        public event JointDelegate JointUpdated;

        private Body _body1;
        private Body _body2;
        private float _massFactor;
        private float _maxImpulse = float.MaxValue;
        private float _targetAngle;
        private float _velocityBias;

        public AngleJoint()
        {
        }

        public AngleJoint(Body body1, Body body2)
        {
            _body1 = body1;
            _body2 = body2;
        }

        public AngleJoint(Body body1, Body body2, float targetAngle)
        {
            _body1 = body1;
            _body2 = body2;
            _targetAngle = targetAngle;
        }

#if(XNA)
        [ContentSerializerIgnore]
#endif
        [XmlIgnore]
        /// <summary>
        /// Gets or sets the fist body.
        /// </summary>
        /// <Value>The body1.</Value>
        public Body Body1
        {
            get { return _body1; }
            set { _body1 = value; }
        }

#if(XNA)
        [ContentSerializerIgnore]
#endif
        [XmlIgnore]
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
        /// Gets or sets the target angle.
        /// </summary>
        /// <Value>The target angle.</Value>
        public float TargetAngle
        {
            get { return _targetAngle; }
            set { _targetAngle = value; }
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

            JointError = (_body2.totalRotation - _body1.totalRotation) - _targetAngle;

            _velocityBias = -BiasFactor * inverseDt * JointError;

            _massFactor = (1 - Softness) / (_body1.inverseMomentOfInertia + _body2.inverseMomentOfInertia);
        }

        public override void Update()
        {
            base.Update();

            if (_body1.isStatic && _body2.isStatic)
                return;

            if (!_body1.Enabled && !_body2.Enabled)
                return;

            float angularImpulse = (_velocityBias - _body2.AngularVelocity + _body1.AngularVelocity) * _massFactor;

            if (angularImpulse != 0f)
            {
                _body1.AngularVelocity -= _body1.inverseMomentOfInertia * Math.Sign(angularImpulse) *
                                          Math.Min(Math.Abs(angularImpulse), _maxImpulse);
                _body2.AngularVelocity += _body2.inverseMomentOfInertia * Math.Sign(angularImpulse) *
                                          Math.Min(Math.Abs(angularImpulse), _maxImpulse);

                if (JointUpdated != null)
                    JointUpdated(this, _body1, _body2);
            }
        }
    }
}