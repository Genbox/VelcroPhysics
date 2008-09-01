using System;
using FarseerGames.FarseerPhysics.Mathematics;
using Microsoft.Xna.Framework;
#if (XNA)
using Microsoft.Xna.Framework;
#endif

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public class PinJoint : Joint
    {
        private float accumulatedImpulse;
        private Vector2 anchor1;
        private Vector2 anchor2;
        private float biasFactor = .2f;
        protected Body body1;
        protected Body body2;

        private float breakpoint = float.MaxValue;
        private float effectiveMass;

        private float jointError;
        private Vector2 r1;
        private Vector2 r2;
        private float softness;
        private float targetDistance;
        private float velocityBias;
        private Vector2 worldAnchor1;
        private Vector2 worldAnchor2;
        private Vector2 worldAnchorDifferenceNormalized;

        public PinJoint()
        {
        }

        public PinJoint(Body body1, Vector2 anchor1, Body body2, Vector2 anchor2)
        {
            this.body1 = body1;
            this.body2 = body2;

            Vector2 difference = (body2.position + anchor2) - (body1.position + anchor1);
            targetDistance = difference.Length(); //by default the target distance is the diff between anchors.

            //initialize the world anchors (only needed to give valid values to the WorldAnchor properties)
            Anchor1 = anchor1;
            Anchor2 = anchor2;
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
            get { return biasFactor; }
            set { biasFactor = value; }
        }

        public float Softness
        {
            get { return softness; }
            set { softness = value; }
        }

        public float Breakpoint
        {
            get { return breakpoint; }
            set { breakpoint = value; }
        }

        public float JointError
        {
            get { return jointError; }
        }

        public float TargetDistance
        {
            get { return targetDistance; }
            set { targetDistance = value; }
        }

        public Vector2 Anchor1
        {
            get { return anchor1; }
            set
            {
                anchor1 = value;
                body1.GetBodyMatrix(out body1MatrixTemp);
                Vector2.TransformNormal(ref anchor1, ref body1MatrixTemp, out r1);
                Vector2.Add(ref body1.position, ref r1, out worldAnchor1);
            }
        }

        public Vector2 Anchor2
        {
            get { return anchor2; }
            set
            {
                anchor2 = value;
                body2.GetBodyMatrix(out body2MatrixTemp);
                Vector2.TransformNormal(ref anchor2, ref body2MatrixTemp, out r2);
                Vector2.Add(ref body2.position, ref r2, out worldAnchor2);
            }
        }

        public Vector2 WorldAnchor1
        {
            get { return worldAnchor1; }
        }

        public Vector2 WorldAnchor2
        {
            get { return worldAnchor2; }
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
            if (Enabled && Math.Abs(jointError) > breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }
            if (isDisposed)
            {
                return;
            }

            //set some temp variables
            body1InverseMass = body1.inverseMass;
            body1InverseMomentOfInertia = body1.inverseMomentOfInertia;
            body2InverseMass = body2.inverseMass;
            body2InverseMomentOfInertia = body2.inverseMomentOfInertia;

            //calc r1 and r2 from the anchors
            body1.GetBodyMatrix(out body1MatrixTemp);
            body2.GetBodyMatrix(out body2MatrixTemp);
            Vector2.TransformNormal(ref anchor1, ref body1MatrixTemp, out r1);
            Vector2.TransformNormal(ref anchor2, ref body2MatrixTemp, out r2);

            //calc the diff between anchor positions
            Vector2.Add(ref body1.position, ref r1, out worldAnchor1);
            Vector2.Add(ref body2.position, ref r2, out worldAnchor2);
            Vector2.Subtract(ref worldAnchor2, ref worldAnchor1, out worldAnchorDifference);

            float distance = worldAnchorDifference.Length();
            jointError = distance - targetDistance;

            //normalize the difference vector
            Vector2.Multiply(ref worldAnchorDifference, 1/(distance != 0 ? distance : float.PositiveInfinity),
                             out worldAnchorDifferenceNormalized); //distance = 0 --> error (fix) 

            //calc velocity bias
            velocityBias = -biasFactor*inverseDt*(distance - targetDistance);

            //calc mass normal (effective mass in relation to constraint)
            Calculator.Cross(ref r1, ref worldAnchorDifferenceNormalized, out r1cn);
            Calculator.Cross(ref r2, ref worldAnchorDifferenceNormalized, out r2cn);
            kNormal = body1.inverseMass + body2.inverseMass + body1.inverseMomentOfInertia*r1cn*r1cn +
                      body2.inverseMomentOfInertia*r2cn*r2cn;
            effectiveMass = 1/(kNormal + softness);

            //convert scalar accumulated impulse to vector
            Vector2.Multiply(ref worldAnchorDifferenceNormalized, accumulatedImpulse, out accumulatedImpulseVector);

            //apply accumulated impulses (warm starting)
            body2.ApplyImmediateImpulse(ref accumulatedImpulseVector);
            Calculator.Cross(ref r2, ref accumulatedImpulseVector, out angularImpulse);
            body2.ApplyAngularImpulse(angularImpulse);

            Vector2.Multiply(ref accumulatedImpulseVector, -1, out accumulatedImpulseVector);
            body1.ApplyImmediateImpulse(ref accumulatedImpulseVector);
            Calculator.Cross(ref r1, ref accumulatedImpulseVector, out angularImpulse);
            body1.ApplyAngularImpulse(angularImpulse);
        }

        public override void Update()
        {
            if (Math.Abs(jointError) > breakpoint)
            {
                Dispose();
            } //check if joint is broken
            if (isDisposed)
            {
                return;
            }

            //calc velocity anchor points (angular component + linear)
            Calculator.Cross(ref body1.angularVelocity, ref r1, out angularVelocityComponent1);
            Vector2.Add(ref body1.linearVelocity, ref angularVelocityComponent1, out velocity1);

            Calculator.Cross(ref body2.angularVelocity, ref r2, out angularVelocityComponent2);
            Vector2.Add(ref body2.linearVelocity, ref angularVelocityComponent2, out velocity2);

            //calc velocity difference
            Vector2.Subtract(ref velocity2, ref velocity1, out dv);

            //map the velocity difference into constraint space
            Vector2.Dot(ref dv, ref worldAnchorDifferenceNormalized, out dvNormal);

            //calc the impulse magnitude
            impulseMagnitude = (velocityBias - dvNormal - softness*accumulatedImpulse)*effectiveMass;
                //not sure if softness is implemented correctly.

            //convert scalar impulse to vector
            Vector2.Multiply(ref worldAnchorDifferenceNormalized, impulseMagnitude, out impulse);

            //apply impulse
            body2.ApplyImmediateImpulse(ref impulse);
            Calculator.Cross(ref r2, ref impulse, out angularImpulse);
            body2.ApplyAngularImpulse(angularImpulse);

            Vector2.Multiply(ref impulse, -1, out impulse);
            body1.ApplyImmediateImpulse(ref impulse);
            Calculator.Cross(ref r1, ref impulse, out angularImpulse);
            body1.ApplyAngularImpulse(angularImpulse);

            //add to the accumulated impulse
            accumulatedImpulse += impulseMagnitude;
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
        private float body1InverseMass;
        private float body1InverseMomentOfInertia;
        private Matrix body1MatrixTemp;

        private float body2InverseMass;
        private float body2InverseMomentOfInertia;
        private Matrix body2MatrixTemp;
        private float kNormal;

        private float r1cn;
        private float r2cn;
        private Vector2 worldAnchorDifference;

        #endregion
    }
}