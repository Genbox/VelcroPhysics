using System;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Dynamics
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

        public float BiasFactor { get; set; }

        public float Softness { get; set; }

        public float Breakpoint { get; set; }

        public float JointError { get; private set; }

        public float TargetDistance { get; set; }

        public Vector2 Anchor1
        {
            get { return _anchor1; }
            set
            {
                _anchor1 = value;
                Body1.GetBodyMatrix(out body1MatrixTemp);
                Vector2.TransformNormal(ref _anchor1, ref body1MatrixTemp, out _r1);
                Vector2.Add(ref Body1.position, ref _r1, out _worldAnchor1);
            }
        }

        public Vector2 Anchor2
        {
            get { return _anchor2; }
            set
            {
                _anchor2 = value;
                Body2.GetBodyMatrix(out body2MatrixTemp);
                Vector2.TransformNormal(ref _anchor2, ref body2MatrixTemp, out _r2);
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

        public event EventHandler<EventArgs> Broke;

        public override void Validate()
        {
            if (Body1.IsDisposed || Body2.IsDisposed)
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
            if (isDisposed)
            {
                return;
            }

            //Note: Cleanup, variable never used
            //set some temp variables
            //body1InverseMass = body1.inverseMass;
            //body1InverseMomentOfInertia = body1.inverseMomentOfInertia;
            //body2InverseMass = body2.inverseMass;
            //body2InverseMomentOfInertia = body2.inverseMomentOfInertia;

            //calc r1 and r2 from the anchors
            Body1.GetBodyMatrix(out body1MatrixTemp);
            Body2.GetBodyMatrix(out body2MatrixTemp);
            Vector2.TransformNormal(ref _anchor1, ref body1MatrixTemp, out _r1);
            Vector2.TransformNormal(ref _anchor2, ref body2MatrixTemp, out _r2);

            //calc the diff between anchor positions
            Vector2.Add(ref Body1.position, ref _r1, out _worldAnchor1);
            Vector2.Add(ref Body2.position, ref _r2, out _worldAnchor2);
            Vector2.Subtract(ref _worldAnchor2, ref _worldAnchor1, out worldAnchorDifference);

            float distance = worldAnchorDifference.Length();
            JointError = distance - TargetDistance;

            //normalize the difference vector
            Vector2.Multiply(ref worldAnchorDifference, 1/(distance != 0 ? distance : float.PositiveInfinity),
                             out _worldAnchorDifferenceNormalized); //distance = 0 --> error (fix) 

            //calc velocity bias
            _velocityBias = -BiasFactor*inverseDt*(distance - TargetDistance);

            //calc mass normal (effective mass in relation to constraint)
            Calculator.Cross(ref _r1, ref _worldAnchorDifferenceNormalized, out r1cn);
            Calculator.Cross(ref _r2, ref _worldAnchorDifferenceNormalized, out r2cn);
            kNormal = Body1.inverseMass + Body2.inverseMass + Body1.inverseMomentOfInertia*r1cn*r1cn +
                      Body2.inverseMomentOfInertia*r2cn*r2cn;
            _effectiveMass = 1/(kNormal + Softness);

            //convert scalar accumulated impulse to vector
            Vector2.Multiply(ref _worldAnchorDifferenceNormalized, _accumulatedImpulse, out accumulatedImpulseVector);

            //apply accumulated impulses (warm starting)
            Body2.ApplyImmediateImpulse(ref accumulatedImpulseVector);
            Calculator.Cross(ref _r2, ref accumulatedImpulseVector, out angularImpulse);
            Body2.ApplyAngularImpulse(angularImpulse);

            Vector2.Multiply(ref accumulatedImpulseVector, -1, out accumulatedImpulseVector);
            Body1.ApplyImmediateImpulse(ref accumulatedImpulseVector);
            Calculator.Cross(ref _r1, ref accumulatedImpulseVector, out angularImpulse);
            Body1.ApplyAngularImpulse(angularImpulse);
        }

        public override void Update()
        {
            if (Math.Abs(JointError) > Breakpoint)
            {
                Dispose();
            } //check if joint is broken
            if (isDisposed)
            {
                return;
            }

            //calc velocity anchor points (angular component + linear)
            Calculator.Cross(ref Body1.angularVelocity, ref _r1, out angularVelocityComponent1);
            Vector2.Add(ref Body1.linearVelocity, ref angularVelocityComponent1, out velocity1);

            Calculator.Cross(ref Body2.angularVelocity, ref _r2, out angularVelocityComponent2);
            Vector2.Add(ref Body2.linearVelocity, ref angularVelocityComponent2, out velocity2);

            //calc velocity difference
            Vector2.Subtract(ref velocity2, ref velocity1, out dv);

            //map the velocity difference into constraint space
            Vector2.Dot(ref dv, ref _worldAnchorDifferenceNormalized, out dvNormal);

            //calc the impulse magnitude
            impulseMagnitude = (_velocityBias - dvNormal - Softness*_accumulatedImpulse)*_effectiveMass;
            //not sure if softness is implemented correctly.

            //convert scalar impulse to vector
            Vector2.Multiply(ref _worldAnchorDifferenceNormalized, impulseMagnitude, out impulse);

            //apply impulse
            Body2.ApplyImmediateImpulse(ref impulse);
            Calculator.Cross(ref _r2, ref impulse, out angularImpulse);
            Body2.ApplyAngularImpulse(angularImpulse);

            Vector2.Multiply(ref impulse, -1, out impulse);
            Body1.ApplyImmediateImpulse(ref impulse);
            Calculator.Cross(ref _r1, ref impulse, out angularImpulse);
            Body1.ApplyAngularImpulse(angularImpulse);

            //add to the accumulated impulse
            _accumulatedImpulse += impulseMagnitude;
        }

        #region Update variables

        private Vector2 angularVelocityComponent1;
        private Vector2 angularVelocityComponent2;
        private Vector2 dv;
        private float dvNormal;
        private Vector2 impulse;
        private float impulseMagnitude;
        private Vector2 velocity1;
        private Vector2 velocity2;

        #endregion

        #region PreStep variables

        private Vector2 accumulatedImpulseVector;
        private float angularImpulse;

        //Note: Cleanup, variable never used
        //private float body1InverseMass;
        //private float body1InverseMomentOfInertia;
        private Matrix body1MatrixTemp;

        //Note: Cleanup, variable never used
        //private float body2InverseMass;
        //private float body2InverseMomentOfInertia;
        private Matrix body2MatrixTemp;
        private float kNormal;

        private float r1cn;
        private float r2cn;
        private Vector2 worldAnchorDifference;

        #endregion
    }
}