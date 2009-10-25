using System;
using FarseerGames.FarseerPhysics.Mathematics;

#if (XNA)
using Microsoft.Xna.Framework;
#endif

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    /// <summary>
    /// Slider joint is just like pin joint, but the distance between the bodies are not fixed.
    /// The bodies can move towards or away from each other within limits.
    /// </summary>
    public class SliderJoint : Joint
    {
        public event JointDelegate JointUpdated;

        private float _accumulatedImpulse;
        private Vector2 _anchor;
        private Vector2 _anchor1;
        private Vector2 _anchor2;
        private Body _body1;
        private Body _body2;
        private float _effectiveMass;
        private bool _lowerLimitViolated;
        private float _max;
        private float _min;
        private Vector2 _r1;
        private Vector2 _r2;
        private float _slop = .01f;
        private bool _upperLimitViolated;
        private float _velocityBias;
        private Vector2 _worldAnchor1;
        private Vector2 _worldAnchor2;
        private Vector2 _worldAnchorDifferenceNormalized;

        public SliderJoint()
        {
        }

        public SliderJoint(Body body1, Vector2 anchor1, Body body2, Vector2 anchor2, float min, float max)
        {
            _body1 = body1;
            _body2 = body2;

            _anchor1 = anchor1;
            _anchor2 = anchor2;

            _min = min;
            _max = max;

            //initialize the world anchors (only needed to give valid values to the WorldAnchor properties)
            Anchor1 = anchor1;
            Anchor2 = anchor2;
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

        /// <summary>
        /// Gets or sets the slop.
        /// </summary>
        /// <Value>The slop.</Value>
        public float Slop
        {
            get { return _slop; }
            set { _slop = value; }
        }

        /// <summary>
        /// Gets or sets the min.
        /// </summary>
        /// <Value>The min.</Value>
        public float Min
        {
            get { return _min; }
            set { _min = value; }
        }

        /// <summary>
        /// Gets or sets the max.
        /// </summary>
        /// <Value>The max.</Value>
        public float Max
        {
            get { return _max; }
            set { _max = value; }
        }

        /// <summary>
        /// Gets or sets the first anchor.
        /// </summary>
        /// <Value>The anchor1.</Value>
        public Vector2 Anchor1
        {
            get { return _anchor1; }
            set
            {
                _anchor1 = value;
                _body1.GetBodyMatrix(out _body1MatrixTemp);
                Vector2.TransformNormal(ref _anchor1, ref _body1MatrixTemp, out _r1);
                Vector2.Add(ref _body1.position, ref _r1, out _worldAnchor1);
            }
        }

        /// <summary>
        /// Gets or sets the second anchor.
        /// </summary>
        /// <Value>The anchor2.</Value>
        public Vector2 Anchor2
        {
            get { return _anchor2; }
            set
            {
                _anchor2 = value;
                _body2.GetBodyMatrix(out _body2MatrixTemp);
                Vector2.TransformNormal(ref _anchor2, ref _body2MatrixTemp, out _r2);
                Vector2.Add(ref _body2.position, ref _r2, out _worldAnchor2);
            }
        }

        /// <summary>
        /// Gets the first world anchor.
        /// </summary>
        /// <Value>The world anchor1.</Value>
        public Vector2 WorldAnchor1
        {
            get { return _worldAnchor1; }
        }

        /// <summary>
        /// Gets the second world anchor.
        /// </summary>
        /// <Value>The world anchor2.</Value>
        public Vector2 WorldAnchor2
        {
            get { return _worldAnchor2; }
        }

        /// <summary>
        /// Gets the current anchor position.
        /// </summary>
        /// <Value>The current anchor position.</Value>
        public Vector2 CurrentAnchorPosition
        {
            get
            {
                Vector2.Add(ref _body1.position, ref _r1, out _anchor); //_anchor moves once simulator starts
                return _anchor;
            }
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

            //calc r1 and r2 from the anchors
            _body1.GetBodyMatrix(out _body1MatrixTemp);
            _body2.GetBodyMatrix(out _body2MatrixTemp);
            Vector2.TransformNormal(ref _anchor1, ref _body1MatrixTemp, out _r1);
            Vector2.TransformNormal(ref _anchor2, ref _body2MatrixTemp, out _r2);

            //calc the diff between anchor positions
            Vector2.Add(ref _body1.position, ref _r1, out _worldAnchor1);
            Vector2.Add(ref _body2.position, ref _r2, out _worldAnchor2);
            Vector2.Subtract(ref _worldAnchor2, ref _worldAnchor1, out _worldAnchorDifference);

            _distance = _worldAnchorDifference.Length();
            JointError = 0;

            if (_distance > _max)
            {
                if (_lowerLimitViolated)
                {
                    _accumulatedImpulse = 0;
                    _lowerLimitViolated = false;
                }
                _upperLimitViolated = true;
                if (_distance < _max + _slop)
                {
                    JointError = 0; //allow some _slop 
                }
                else
                {
                    JointError = _distance - _max;
                }
            }
            else if (_distance < _min)
            {
                if (_upperLimitViolated)
                {
                    _accumulatedImpulse = 0;
                    _upperLimitViolated = false;
                }
                _lowerLimitViolated = true;
                if (_distance > _min - _slop)
                {
                    JointError = 0;
                }
                else
                {
                    JointError = _distance - _min;
                }
            }
            else
            {
                _upperLimitViolated = false;
                _lowerLimitViolated = false;
                JointError = 0;
                _accumulatedImpulse = 0;
            }

            //normalize the difference vector
            Vector2.Multiply(ref _worldAnchorDifference, 1 / (_distance != 0 ? _distance : float.PositiveInfinity),
                             out _worldAnchorDifferenceNormalized); //distance = 0 --> error (fix) 

            //calc velocity bias
            _velocityBias = BiasFactor * inverseDt * (JointError);

            //calc mass normal (effective mass in relation to constraint)
            Calculator.Cross(ref _r1, ref _worldAnchorDifferenceNormalized, out _r1cn);
            Calculator.Cross(ref _r2, ref _worldAnchorDifferenceNormalized, out _r2cn);
            _kNormal = _body1.inverseMass + _body2.inverseMass + _body1.inverseMomentOfInertia * _r1cn * _r1cn +
                       _body2.inverseMomentOfInertia * _r2cn * _r2cn;
            _effectiveMass = 1 / (_kNormal + Softness);

            //convert scalar accumulated impulse to vector
            Vector2.Multiply(ref _worldAnchorDifferenceNormalized, _accumulatedImpulse, out _accumulatedImpulseVector);

            //apply accumulated impulses (warm starting)
            _body2.ApplyImmediateImpulse(ref _accumulatedImpulseVector);
            Calculator.Cross(ref _r2, ref _accumulatedImpulseVector, out _angularImpulse);
            _body2.ApplyAngularImpulse(_angularImpulse);

            Vector2.Multiply(ref _accumulatedImpulseVector, -1, out _accumulatedImpulseVector);
            _body1.ApplyImmediateImpulse(ref _accumulatedImpulseVector);
            Calculator.Cross(ref _r1, ref _accumulatedImpulseVector, out _angularImpulse);
            _body1.ApplyAngularImpulse(_angularImpulse);
        }

        public override void Update()
        {
            base.Update();

            if (_body1.isStatic && _body2.isStatic)
                return;

            if (!_body1.Enabled && !_body2.Enabled)
                return;

            if (!_upperLimitViolated && !_lowerLimitViolated)
                return;

            //calc velocity anchor points (angular component + linear)
            Calculator.Cross(ref _body1.AngularVelocity, ref _r1, out _angularVelocityComponent1);
            Vector2.Add(ref _body1.LinearVelocity, ref _angularVelocityComponent1, out _velocity1);

            Calculator.Cross(ref _body2.AngularVelocity, ref _r2, out _angularVelocityComponent2);
            Vector2.Add(ref _body2.LinearVelocity, ref _angularVelocityComponent2, out _velocity2);

            //calc velocity difference
            Vector2.Subtract(ref _velocity2, ref _velocity1, out _dv);

            //map the velocity difference into constraint space
            Vector2.Dot(ref _dv, ref _worldAnchorDifferenceNormalized, out _dvNormal);

            //calc the impulse magnitude
            _impulseMagnitude = (-_velocityBias - _dvNormal - Softness * _accumulatedImpulse) * _effectiveMass;
            //Note: Not sure if softness is implemented correctly.

            float oldAccumulatedImpulse = _accumulatedImpulse;

            if (_upperLimitViolated)
            {
                _accumulatedImpulse = Math.Min(oldAccumulatedImpulse + _impulseMagnitude, 0);
            }
            else if (_lowerLimitViolated)
            {
                _accumulatedImpulse = Math.Max(oldAccumulatedImpulse + _impulseMagnitude, 0);
            }

            _impulseMagnitude = _accumulatedImpulse - oldAccumulatedImpulse;

            //convert scalar impulse to vector
            Vector2.Multiply(ref _worldAnchorDifferenceNormalized, _impulseMagnitude, out _impulse);

            if (_impulse != Vector2.Zero)
            {
                //apply impulse
                _body2.ApplyImmediateImpulse(ref _impulse);
                Calculator.Cross(ref _r2, ref _impulse, out _angularImpulse);
                _body2.ApplyAngularImpulse(_angularImpulse);

                Vector2.Multiply(ref _impulse, -1, out _impulse);
                _body1.ApplyImmediateImpulse(ref _impulse);
                Calculator.Cross(ref _r1, ref _impulse, out _angularImpulse);
                _body1.ApplyAngularImpulse(_angularImpulse);

                if (JointUpdated != null)
                    JointUpdated(this, _body1, _body2);
            }
        }

        #region Update variables

        private Vector2 _angularVelocityComponent1;
        private Vector2 _angularVelocityComponent2;
        private Vector2 _dv;
        private float _dvNormal;
        private Vector2 _impulse;
        private float _impulseMagnitude;
        private Vector2 _velocity1;
        private Vector2 _velocity2;

        #endregion

        #region PreStep variables

        private Vector2 _accumulatedImpulseVector;
        private float _angularImpulse;
        private Matrix _body1MatrixTemp;

        private Matrix _body2MatrixTemp;
        private float _distance;
        private float _kNormal;

        private float _r1cn;
        private float _r2cn;
        private Vector2 _worldAnchorDifference;

        #endregion
    }
}