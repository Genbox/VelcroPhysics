using FarseerGames.FarseerPhysics.Mathematics;

#if (XNA)
using Microsoft.Xna.Framework;
#endif

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    /// <summary>
    /// A pin joint works like 2 revolute joints with a fixed distance between them.
    /// Essentially it places 2 bodies at a fixed distance from each other.
    /// </summary>
    public class PinJoint : Joint
    {
        public event JointDelegate JointUpdated;

        private float _accumulatedImpulse;
        private Vector2 _anchor1;
        private Vector2 _anchor2;
        private Body _body1;
        private Body _body2;
        private float _effectiveMass;
        private Vector2 _r1;
        private Vector2 _r2;
        private float _targetDistance;
        private float _velocityBias;
        private Vector2 _worldAnchor1;
        private Vector2 _worldAnchor2;
        private Vector2 _worldAnchorDifferenceNormalized;

        public PinJoint()
        {
        }

        public PinJoint(Body body1, Vector2 anchor1, Body body2, Vector2 anchor2)
        {
            _body1 = body1;
            _body2 = body2;

            //From http://www.codeplex.com/Thread/View.aspx?ProjectName=FarseerPhysics&ThreadId=22839
            //Vector2 difference = (body2.position + anchor2) - (body1.position + anchor1);
            //_targetDistance = difference.Length(); //by default the target distance is the diff between anchors.

            //Also take rotation into account
            Vector2 difference = body2.GetWorldPosition(anchor2) - body1.GetWorldPosition(anchor1);
            _targetDistance = difference.Length(); //by default the target distance is the diff between anchors.

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
        /// Gets or sets the target distance.
        /// </summary>
        /// <Value>The target distance.</Value>
        public float TargetDistance
        {
            get { return _targetDistance; }
            set { _targetDistance = value; }
        }

        /// <summary>
        /// Gets or sets the fist anchor.
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

            float distance = _worldAnchorDifference.Length();
            JointError = distance - _targetDistance;

            //normalize the difference vector
            Vector2.Multiply(ref _worldAnchorDifference, 1 / (distance != 0 ? distance : float.PositiveInfinity),
                             out _worldAnchorDifferenceNormalized); //distance = 0 --> error (fix) 

            //calc velocity bias
            _velocityBias = -BiasFactor * inverseDt * (distance - _targetDistance);

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

            //Calc velocity anchor points (angular component + linear)
            Calculator.Cross(ref _body1.AngularVelocity, ref _r1, out _angularVelocityComponent1);
            Vector2.Add(ref _body1.LinearVelocity, ref _angularVelocityComponent1, out _velocity1);

            Calculator.Cross(ref _body2.AngularVelocity, ref _r2, out _angularVelocityComponent2);
            Vector2.Add(ref _body2.LinearVelocity, ref _angularVelocityComponent2, out _velocity2);

            //Calc velocity difference
            Vector2.Subtract(ref _velocity2, ref _velocity1, out _dv);

            //Map the velocity difference into constraint space
            Vector2.Dot(ref _dv, ref _worldAnchorDifferenceNormalized, out _dvNormal);

            //Calc the impulse magnitude
            _impulseMagnitude = (_velocityBias - _dvNormal - Softness * _accumulatedImpulse) * _effectiveMass;
            //Note: Not sure if softness is implemented correctly.

            //Convert scalar impulse to vector
            Vector2.Multiply(ref _worldAnchorDifferenceNormalized, _impulseMagnitude, out _impulse);

            //Add to the accumulated impulse
            _accumulatedImpulse += _impulseMagnitude;

            if (_impulse != Vector2.Zero)
            {
                //Apply impulse
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
        private float _kNormal;

        private float _r1cn;
        private float _r2cn;
        private Vector2 _worldAnchorDifference;

        #endregion
    }
}