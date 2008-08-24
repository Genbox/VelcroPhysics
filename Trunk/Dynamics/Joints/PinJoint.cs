using System;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    public class PinJoint : Joint
    {
        private float _accumulatedImpulse;
        private Vector2 _anchor1;
        private Vector2 _anchor2;

        private float _effectiveMass;

        private Vector2 _r1;
        private Vector2 _r2;
        private float _velocityBias;
        private Vector2 _worldAnchor1;
        private Vector2 _worldAnchor2;
        private Vector2 _worldAnchorDifferenceNormalized;

        public PinJoint()
        {
            Breakpoint = float.MaxValue;
            BiasFactor = .2f;
        }

        public PinJoint(Body body1, Vector2 anchor1, Body body2, Vector2 anchor2)
        {
            Breakpoint = float.MaxValue;
            BiasFactor = .2f;
            Body1 = body1;
            Body2 = body2;

            Vector2 difference = (body2.position + anchor2) - (body1.position + anchor1);
            TargetDistance = difference.Length(); //by default the target distance is the diff between anchors.

            //initialize the world anchors (only needed to give valid values to the WorldAnchor properties)
            Anchor1 = anchor1;
            Anchor2 = anchor2;
        }

        public Body Body1 { get; set; }
        public Body Body2 { get; set; }
        public float TargetDistance { get; set; }

        public Vector2 Anchor1
        {
            get { return _anchor1; }
            set
            {
                _anchor1 = value;
                Body1.GetBodyMatrix(out _body1MatrixTemp);
                Vector2.TransformNormal(ref _anchor1, ref _body1MatrixTemp, out _r1);
                Vector2.Add(ref Body1.position, ref _r1, out _worldAnchor1);
            }
        }

        public Vector2 Anchor2
        {
            get { return _anchor2; }
            set
            {
                _anchor2 = value;
                Body2.GetBodyMatrix(out _body2MatrixTemp);
                Vector2.TransformNormal(ref _anchor2, ref _body2MatrixTemp, out _r2);
                Vector2.Add(ref Body2.position, ref _r2, out _worldAnchor2);
            }
        }

        public Vector2 WorldAnchor1
        {
            get { return _worldAnchor1; }
        }

        public Vector2 WorldAnchor2
        {
            get { return _worldAnchor2; }
        }

        public override void Validate()
        {
            if (Body1.IsDisposed || Body2.IsDisposed)
            {
                Dispose();
            }
        }

        public override void PreStep(float inverseDt)
        {
            base.PreStep(inverseDt);

            if (IsDisposed) return;

            //calc r1 and r2 from the anchors
            Body1.GetBodyMatrix(out _body1MatrixTemp);
            Body2.GetBodyMatrix(out _body2MatrixTemp);
            Vector2.TransformNormal(ref _anchor1, ref _body1MatrixTemp, out _r1);
            Vector2.TransformNormal(ref _anchor2, ref _body2MatrixTemp, out _r2);

            //calc the diff between anchor positions
            Vector2.Add(ref Body1.position, ref _r1, out _worldAnchor1);
            Vector2.Add(ref Body2.position, ref _r2, out _worldAnchor2);
            Vector2.Subtract(ref _worldAnchor2, ref _worldAnchor1, out _worldAnchorDifference);

            float distance = _worldAnchorDifference.Length();
            Error = distance - TargetDistance;

            //normalize the difference vector
            Vector2.Multiply(ref _worldAnchorDifference, 1/(distance != 0 ? distance : float.PositiveInfinity),
                             out _worldAnchorDifferenceNormalized); //distance = 0 --> error (fix) 

            //calc velocity bias
            _velocityBias = -BiasFactor*inverseDt*(distance - TargetDistance);

            //calc mass normal (effective mass in relation to constraint)
            Calculator.Cross(ref _r1, ref _worldAnchorDifferenceNormalized, out _r1cn);
            Calculator.Cross(ref _r2, ref _worldAnchorDifferenceNormalized, out _r2cn);
            _kNormal = Body1.inverseMass + Body2.inverseMass + Body1.inverseMomentOfInertia*_r1cn*_r1cn +
                       Body2.inverseMomentOfInertia*_r2cn*_r2cn;
            _effectiveMass = 1/(_kNormal + Softness);

            //convert scalar accumulated impulse to vector
            Vector2.Multiply(ref _worldAnchorDifferenceNormalized, _accumulatedImpulse, out _accumulatedImpulseVector);

            //apply accumulated impulses (warm starting)
            Body2.ApplyImmediateImpulse(ref _accumulatedImpulseVector);
            Calculator.Cross(ref _r2, ref _accumulatedImpulseVector, out _angularImpulse);
            Body2.ApplyAngularImpulse(_angularImpulse);

            Vector2.Multiply(ref _accumulatedImpulseVector, -1, out _accumulatedImpulseVector);
            Body1.ApplyImmediateImpulse(ref _accumulatedImpulseVector);
            Calculator.Cross(ref _r1, ref _accumulatedImpulseVector, out _angularImpulse);
            Body1.ApplyAngularImpulse(_angularImpulse);
        }

        public override void Update()
        {
            if (Math.Abs(Error) > Breakpoint)
            {
                Dispose();
            } //check if joint is broken
            if (IsDisposed)
            {
                return;
            }

            //calc velocity anchor points (angular component + linear)
            Calculator.Cross(ref Body1.angularVelocity, ref _r1, out _angularVelocityComponent1);
            Vector2.Add(ref Body1.linearVelocity, ref _angularVelocityComponent1, out _velocity1);

            Calculator.Cross(ref Body2.angularVelocity, ref _r2, out _angularVelocityComponent2);
            Vector2.Add(ref Body2.linearVelocity, ref _angularVelocityComponent2, out _velocity2);

            //calc velocity difference
            Vector2.Subtract(ref _velocity2, ref _velocity1, out _dv);

            //map the velocity difference into constraint space
            Vector2.Dot(ref _dv, ref _worldAnchorDifferenceNormalized, out _dvNormal);

            //calc the impulse magnitude
            _impulseMagnitude = (_velocityBias - _dvNormal - Softness*_accumulatedImpulse)*_effectiveMass;
            //not sure if softness is implemented correctly.

            //convert scalar impulse to vector
            Vector2.Multiply(ref _worldAnchorDifferenceNormalized, _impulseMagnitude, out _impulse);

            //apply impulse
            Body2.ApplyImmediateImpulse(ref _impulse);
            Calculator.Cross(ref _r2, ref _impulse, out _angularImpulse);
            Body2.ApplyAngularImpulse(_angularImpulse);

            Vector2.Multiply(ref _impulse, -1, out _impulse);
            Body1.ApplyImmediateImpulse(ref _impulse);
            Calculator.Cross(ref _r1, ref _impulse, out _angularImpulse);
            Body1.ApplyAngularImpulse(_angularImpulse);

            //add to the accumulated impulse
            _accumulatedImpulse += _impulseMagnitude;
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