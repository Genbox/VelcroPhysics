/*
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com
*
* This software is provided 'as-is', without any express or implied
* warranty.  In no event will the authors be held liable for any damages
* arising from the use of this software.
* Permission is granted to anyone to use this software for any purpose,
* including commercial applications, and to alter it and redistribute it
* freely, subject to the following restrictions:
* 1. The origin of this software must not be misrepresented; you must not
* claim that you wrote the original software. If you use this software
* in a product, an acknowledgment in the product documentation would be
* appreciated but is not required.
* 2. Altered source versions must be plainly marked as such, and must not be
* misrepresented as being the original software.
* 3. This notice may not be removed or altered from any source distribution.
*/

using Box2DX.Dynamics;
using Box2DX.Common;

// Point-to-point constraint
// C = p2 - p1
// Cdot = v2 - v1
//      = v2 + cross(w2, r2) - v1 - cross(w1, r1)
// J = [-I -r1_skew I r2_skew ]
// Identity used:
// w k % (rx i + ry j) = w * (-ry i + rx j)

// Angle constraint
// C = angle2 - angle1 - referenceAngle
// Cdot = w2 - w1
// J = [0 0 -1 0 0 1]
// K = invI1 + invI2

namespace Box2DX.Dynamics
{
    /// Weld joint definition. You need to specify local anchor points
    /// where they are attached and the relative body angle. The position
    /// of the anchor points is important for computing the reaction torque.
    public class WeldJointDef : JointDef
    {
        public WeldJointDef()
        {
            base.Type = JointType.WeldJoint;
            localAnchorA.Set(0.0f, 0.0f);
            localAnchorB.Set(0.0f, 0.0f);
            referenceAngle = 0.0f;
        }

        /// Initialize the bodies, anchors, and reference angle using a world
        /// anchor point.
        public void Initialize(Body body1, Body body2, Vec2 anchor)
        {
            BodyA = body1;
            BodyB = body2;
            localAnchorA = BodyA.GetLocalPoint(anchor);
            localAnchorB = BodyB.GetLocalPoint(anchor);
            referenceAngle = BodyB.GetAngle() - BodyA.GetAngle();
        }

        /// The local anchor point relative to body1's origin.
        public Vec2 localAnchorA;

        /// The local anchor point relative to body2's origin.
        public Vec2 localAnchorB;

        /// The body2 angle minus body1 angle in the reference state (radians).
        public float referenceAngle;
    }

    /// A weld joint essentially glues two bodies together. A weld joint may
    /// distort somewhat because the island constraint solver is approximate.
    public class WeldJoint : Joint
    {
        public override Vec2 AnchorA
        {
            get { return _bodyA.GetWorldPoint(_localAnchorA); }
        }

        public override Vec2 AnchorB
        {
            get { return _bodyB.GetWorldPoint(_localAnchorB); }
        }

        public override Vec2 GetReactionForce(float inv_dt)
        {
            Vec2 P = new Vec2(_impulse.X, _impulse.Y);
            return inv_dt * P;
        }

        public override float GetReactionTorque(float inv_dt)
        {
            return inv_dt * _impulse.Z;
        }

        public WeldJoint(WeldJointDef def)
            : base(def)
        {
            _localAnchorA = def.localAnchorA;
            _localAnchorB = def.localAnchorB;
            _referenceAngle = def.referenceAngle;

            _impulse.SetZero();
        }

        internal override void InitVelocityConstraints(TimeStep step)
        {
            Body bA = _bodyA;
            Body bB = _bodyB;

            // Compute the effective mass matrix.
            Vec2 rA = Math.Mul(bA.GetTransform().R, _localAnchorA - bA.GetLocalCenter());
            Vec2 rB = Math.Mul(bB.GetTransform().R, _localAnchorB - bB.GetLocalCenter());

            // J = [-I -r1_skew I r2_skew]
            //     [ 0       -1 0       1]
            // r_skew = [-ry; rx]

            // Matlab
            // K = [ mA+r1y^2*iA+mB+r2y^2*iB,  -r1y*iA*r1x-r2y*iB*r2x,          -r1y*iA-r2y*iB]
            //     [  -r1y*iA*r1x-r2y*iB*r2x, mA+r1x^2*iA+mB+r2x^2*iB,           r1x*iA+r2x*iB]
            //     [          -r1y*iA-r2y*iB,           r1x*iA+r2x*iB,                   iA+iB]

            float mA = bA._invMass, mB = bB._invMass;
            float iA = bA._invI, iB = bB._invI;

            _mass.Col1.X = mA + mB + rA.Y * rA.Y * iA + rB.Y * rB.Y * iB;
            _mass.Col2.X = -rA.Y * rA.X * iA - rB.Y * rB.X * iB;
            _mass.Col3.X = -rA.Y * iA - rB.Y * iB;
            _mass.Col1.Y = _mass.Col2.X;
            _mass.Col2.Y = mA + mB + rA.X * rA.X * iA + rB.X * rB.X * iB;
            _mass.Col3.Y = rA.X * iA + rB.X * iB;
            _mass.Col1.Z = _mass.Col3.X;
            _mass.Col2.Z = _mass.Col3.Y;
            _mass.Col3.Z = iA + iB;

            if (step.WarmStarting)
            {
                // Scale impulses to support a variable time step.
                _impulse *= step.DtRatio;

                Vec2 P = new Vec2(_impulse.X, _impulse.Y);

                bA._linearVelocity -= mA * P;
                bA._angularVelocity -= iA * (Vec2.Cross(rA, P) + _impulse.Z);

                bB._linearVelocity += mB * P;
                bB._angularVelocity += iB * (Vec2.Cross(rB, P) + _impulse.Z);
            }
            else
            {
                _impulse.SetZero();
            }
        }

        internal override void SolveVelocityConstraints(TimeStep step)
        {
            //B2_NOT_USED(step);

            Body bA = _bodyA;
            Body bB = _bodyB;

            Vec2 vA = bA._linearVelocity;
            float wA = bA._angularVelocity;
            Vec2 vB = bB._linearVelocity;
            float wB = bB._angularVelocity;

            float mA = bA._invMass, mB = bB._invMass;
            float iA = bA._invI, iB = bB._invI;

            Vec2 rA = Math.Mul(bA.GetTransform().R, _localAnchorA - bA.GetLocalCenter());
            Vec2 rB = Math.Mul(bB.GetTransform().R, _localAnchorB - bB.GetLocalCenter());

            // Solve point-to-point constraint
            Vec2 Cdot1 = vB + Vec2.Cross(wB, rB) - vA - Vec2.Cross(wA, rA);
            float Cdot2 = wB - wA;
            Vec3 Cdot = new Vec3(Cdot1.X, Cdot1.Y, Cdot2);

            Vec3 impulse = _mass.Solve33(-Cdot);
            _impulse += impulse;

            Vec2 P = new Vec2(impulse.X, impulse.Y);

            vA -= mA * P;
            wA -= iA * (Vec2.Cross(rA, P) + impulse.Z);

            vB += mB * P;
            wB += iB * (Vec2.Cross(rB, P) + impulse.Z);

            bA._linearVelocity = vA;
            bA._angularVelocity = wA;
            bB._linearVelocity = vB;
            bB._angularVelocity = wB;
        }

        internal override bool SolvePositionConstraints(float baumgarte)
        {
            //B2_NOT_USED(baumgarte);

            Body bA = _bodyA;
            Body bB = _bodyB;

            float mA = bA._invMass, mB = bB._invMass;
            float iA = bA._invI, iB = bB._invI;

            Vec2 rA = Math.Mul(bA.GetTransform().R, _localAnchorA - bA.GetLocalCenter());
            Vec2 rB = Math.Mul(bB.GetTransform().R, _localAnchorB - bB.GetLocalCenter());

            Vec2 C1 = bB._sweep.C + rB - bA._sweep.C - rA;
            float C2 = bB._sweep.A - bA._sweep.A - _referenceAngle;

            // Handle large detachment.
            float k_allowedStretch = 10.0f * Settings.LinearSlop;
            float positionError = C1.Length();
            float angularError = Math.Abs(C2);
            if (positionError > k_allowedStretch)
            {
                iA *= 1.0f;
                iB *= 1.0f;
            }

            _mass.Col1.X = mA + mB + rA.Y * rA.Y * iA + rB.Y * rB.Y * iB;
            _mass.Col2.X = -rA.Y * rA.X * iA - rB.Y * rB.X * iB;
            _mass.Col3.X = -rA.Y * iA - rB.Y * iB;
            _mass.Col1.Y = _mass.Col2.X;
            _mass.Col2.Y = mA + mB + rA.X * rA.X * iA + rB.X * rB.X * iB;
            _mass.Col3.Y = rA.X * iA + rB.X * iB;
            _mass.Col1.Z = _mass.Col3.X;
            _mass.Col2.Z = _mass.Col3.Y;
            _mass.Col3.Z = iA + iB;

            Vec3 C = new Vec3(C1.X, C1.Y, C2);

            Vec3 impulse = _mass.Solve33(-C);

            Vec2 P = new Vec2(impulse.X, impulse.Y);

            bA._sweep.C -= mA * P;
            bA._sweep.A -= iA * (Vec2.Cross(rA, P) + impulse.Z);

            bB._sweep.C += mB * P;
            bB._sweep.A += iB * (Vec2.Cross(rB, P) + impulse.Z);

            bA.SynchronizeTransform();
            bB.SynchronizeTransform();

            return positionError <= Settings.LinearSlop && angularError <= Settings.AngularSlop;
        }

        internal Vec2 _localAnchorA;
        internal Vec2 _localAnchorB;
        internal float _referenceAngle;

        internal Vec3 _impulse;

        internal Mat33 _mass;
    }
}