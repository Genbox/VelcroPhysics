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

// Point-to-point constraint
// Cdot = v2 - v1
//      = v2 + cross(w2, r2) - v1 - cross(w1, r1)
// J = [-I -r1_skew I r2_skew ]
// Identity used:
// w k % (rx i + ry j) = w * (-ry i + rx j)

// Angle constraint
// Cdot = w2 - w1
// J = [0 0 -1 0 0 1]
// K = invI1 + invI2


using Box2DX.Common;

namespace Box2DX.Dynamics
{
    public class FrictionJointDef : JointDef
    {
        public FrictionJointDef()
        {
            base.Type = JointType.FrictionJoint;
            localAnchorA.SetZero();
            localAnchorB.SetZero();
            maxForce = 0.0f;
            maxTorque = 0.0f;
        }

        /// Initialize the bodies, anchors, axis, and reference angle using the world
        /// anchor and world axis.
        public void Initialize(Body bodyA, Body bodyB, Vec2 anchor)
        {
            bodyA = bodyA;
            bodyB = bodyB;
            localAnchorA = bodyA.GetLocalPoint(anchor);
            localAnchorB = bodyB.GetLocalPoint(anchor);
        }

        /// The local anchor point relative to bodyA's origin.
        public Vec2 localAnchorA;

        /// The local anchor point relative to bodyB's origin.
        public Vec2 localAnchorB;

        /// The maximum friction force in N.
        public float maxForce;

        /// The maximum friction torque in N-m.
        public float maxTorque;
    };

    /// Friction joint. This is used for top-down friction.
    /// It provides 2D translational friction and angular friction.
    public class FrictionJoint : Joint
    {
        public FrictionJoint(FrictionJointDef def) : base(def)
        {
            _localAnchorA = def.localAnchorA;
            _localAnchorB = def.localAnchorB;

            _linearImpulse.SetZero();
            _angularImpulse = 0.0f;

            _maxForce = def.maxForce;
            _maxTorque = def.maxTorque;
        }

        public override Vec2 GetReactionForce(float inv_dt)
        {
            return inv_dt * _linearImpulse;

        }

        public override float GetReactionTorque(float inv_dt)
        {
            return inv_dt * _angularImpulse;
        }

        /// Set the maximum friction force in N.
        public void SetMaxForce(float force)
        {
            _maxForce = force;
        }

        /// Get the maximum friction force in N.
        public float GetMaxForce()
        {
            return _maxForce;
        }

        /// Set the maximum friction torque in N*m.
        public void SetMaxTorque(float torque)
        {
            _maxTorque = torque;
        }

        /// Get the maximum friction torque in N*m.
        public float GetMaxTorque()
        {
            return _maxTorque;
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

            Mat22 K1;
            K1.Col1.X = mA + mB; K1.Col2.X = 0.0f;
            K1.Col1.Y = 0.0f; K1.Col2.Y = mA + mB;

            Mat22 K2;
            K2.Col1.X = iA * rA.Y * rA.Y; K2.Col2.X = -iA * rA.X * rA.Y;
            K2.Col1.Y = -iA * rA.X * rA.Y; K2.Col2.Y = iA * rA.X * rA.X;

            Mat22 K3;
            K3.Col1.X = iB * rB.Y * rB.Y; K3.Col2.X = -iB * rB.X * rB.Y;
            K3.Col1.Y = -iB * rB.X * rB.Y; K3.Col2.Y = iB * rB.X * rB.X;

            Mat22 K = K1 + K2 + K3;
            _linearMass = K.GetInverse();

            _angularMass = iA + iB;
            if (_angularMass > 0.0f)
            {
                _angularMass = 1.0f / _angularMass;
            }

            if (step.WarmStarting)
            {
                // Scale impulses to support a variable time step.
                _linearImpulse *= step.DtRatio;
                _angularImpulse *= step.DtRatio;

                Vec2 P = new Vec2(_linearImpulse.X, _linearImpulse.Y);

                bA._linearVelocity -= mA * P;
                bA._angularVelocity -= iA * (Vec2.Cross(rA, P) + _angularImpulse);

                bB._linearVelocity += mB * P;
                bB._angularVelocity += iB * (Vec2.Cross(rB, P) + _angularImpulse);
            }
            else
            {
                _linearImpulse.SetZero();
                _angularImpulse = 0.0f;
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

            // Solve angular friction
            {
                float Cdot = wB - wA;
                float impulse = -_angularMass * Cdot;

                float oldImpulse = _angularImpulse;
                float maxImpulse = step.Dt * _maxTorque;
                _angularImpulse = Math.Clamp(_angularImpulse + impulse, -maxImpulse, maxImpulse);
                impulse = _angularImpulse - oldImpulse;

                wA -= iA * impulse;
                wB += iB * impulse;
            }

            // Solve linear friction
            {
                Vec2 Cdot = vB + Vec2.Cross(wB, rB) - vA - Vec2.Cross(wA, rA);

                Vec2 impulse = -Math.Mul(_linearMass, Cdot);
                Vec2 oldImpulse = _linearImpulse;
                _linearImpulse += impulse;

                float maxImpulse = step.Dt * _maxForce;

                if (_linearImpulse.LengthSquared() > maxImpulse * maxImpulse)
                {
                    _linearImpulse.Normalize();
                    _linearImpulse *= maxImpulse;
                }

                impulse = _linearImpulse - oldImpulse;

                vA -= mA * impulse;
                wA -= iA * Vec2.Cross(rA, impulse);

                vB += mB * impulse;
                wB += iB * Vec2.Cross(rB, impulse);
            }

            bA._linearVelocity = vA;
            bA._angularVelocity = wA;
            bB._linearVelocity = vB;
            bB._angularVelocity = wB;
        }

        internal override bool SolvePositionConstraints(float baumgarte)
        {
            //B2_NOT_USED(baumgarte);

            return true;

        }

        internal Vec2 _localAnchorA;
        internal Vec2 _localAnchorB;

        internal Mat22 _linearMass;
        internal float _angularMass;

        internal Vec2 _linearImpulse;
        internal float _angularImpulse;

        internal float _maxForce;
        internal float _maxTorque;

        public override Vec2 AnchorA
        {
            get
            {
                return _bodyA.GetWorldPoint(_localAnchorA);
            }
        }

        public override Vec2 AnchorB
        {
            get
            {
                return _bodyB.GetWorldPoint(_localAnchorB);
            }
        }
    }
}