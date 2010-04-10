/*
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
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

using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
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

    /// <summary>
    /// Friction joint. This is used for top-down friction.
    /// It provides 2D translational friction and angular friction.
    /// </summary>
    public class FrictionJoint : Joint
    {
        private float _angularImpulse;
        private float _angularMass;
        private Vector2 _linearImpulse;
        private Mat22 _linearMass;

        public FrictionJoint(Body bodyA, Body bodyB, Vector2 anchor1, Vector2 anchor2)
            : base(bodyA, bodyB)
        {
            JointType = JointType.Friction;
            LocalAnchorA = anchor1;
            LocalAnchorB = anchor2;
        }

        public Vector2 LocalAnchorA { get; private set; }
        public Vector2 LocalAnchorB { get; private set; }

        public override Vector2 WorldAnchorA
        {
            get { return BodyA.GetWorldPoint(LocalAnchorA); }
        }

        public override Vector2 WorldAnchorB
        {
            get { return BodyB.GetWorldPoint(LocalAnchorB); }
        }

        /// <summary>
        /// The maximum friction force in N.
        /// </summary>
        public float MaxForce { get; set; }

        /// <summary>
        /// The maximum friction torque in N-m.
        /// </summary>
        public float MaxTorque { get; set; }

        public override Vector2 GetReactionForce(float inv_dt)
        {
            Vector2 F = (inv_dt * _linearImpulse);
            return F;
        }

        public override float GetReactionTorque(float inv_dt)
        {
            float F = (inv_dt * _angularImpulse);
            return F;
        }

        internal override void InitVelocityConstraints(ref TimeStep step)
        {
            Body bA = BodyA;
            Body bB = BodyB;

            Transform xfA, xfB;
            bA.GetTransform(out xfA);
            bB.GetTransform(out xfB);

            // Compute the effective mass matrix.
            Vector2 rA = MathUtils.Multiply(ref xfA.R, LocalAnchorA - bA.LocalCenter);
            Vector2 rB = MathUtils.Multiply(ref xfB.R, LocalAnchorB - bB.LocalCenter);

            // J = [-I -r1_skew I r2_skew]
            //     [ 0       -1 0       1]
            // r_skew = [-ry; rx]

            // Matlab
            // K = [ mA+r1y^2*iA+mB+r2y^2*iB,  -r1y*iA*r1x-r2y*iB*r2x,          -r1y*iA-r2y*iB]
            //     [  -r1y*iA*r1x-r2y*iB*r2x, mA+r1x^2*iA+mB+r2x^2*iB,           r1x*iA+r2x*iB]
            //     [          -r1y*iA-r2y*iB,           r1x*iA+r2x*iB,                   iA+iB]

            float mA = bA._invMass, mB = bB._invMass;
            float iA = bA._invI, iB = bB._invI;

            Mat22 K1 = new Mat22();
            K1.Col1.X = mA + mB;
            K1.Col2.X = 0.0f;
            K1.Col1.Y = 0.0f;
            K1.Col2.Y = mA + mB;

            Mat22 K2 = new Mat22();
            K2.Col1.X = iA * rA.Y * rA.Y;
            K2.Col2.X = -iA * rA.X * rA.Y;
            K2.Col1.Y = -iA * rA.X * rA.Y;
            K2.Col2.Y = iA * rA.X * rA.X;

            Mat22 K3 = new Mat22();
            K3.Col1.X = iB * rB.Y * rB.Y;
            K3.Col2.X = -iB * rB.X * rB.Y;
            K3.Col1.Y = -iB * rB.X * rB.Y;
            K3.Col2.Y = iB * rB.X * rB.X;

            Mat22 K12;
            Mat22.Add(ref K1, ref K2, out K12);
            Mat22 K;
            Mat22.Add(ref K12, ref K3, out K);
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

                Vector2 P = new Vector2(_linearImpulse.X, _linearImpulse.Y);

                bA._linearVelocity -= mA * P;
                bA._angularVelocity -= iA * (MathUtils.Cross(rA, P) + _angularImpulse);

                bB._linearVelocity += mB * P;
                bB._angularVelocity += iB * (MathUtils.Cross(rB, P) + _angularImpulse);
            }
            else
            {
                _linearImpulse = Vector2.Zero;
                _angularImpulse = 0.0f;
            }
        }

        internal override void SolveVelocityConstraints(ref TimeStep step)
        {
            Body bA = BodyA;
            Body bB = BodyB;

            Vector2 vA = bA._linearVelocity;
            float wA = bA._angularVelocity;
            Vector2 vB = bB._linearVelocity;
            float wB = bB._angularVelocity;

            float mA = bA._invMass, mB = bB._invMass;
            float iA = bA._invI, iB = bB._invI;

            Transform xfA, xfB;
            bA.GetTransform(out xfA);
            bB.GetTransform(out xfB);

            Vector2 rA = MathUtils.Multiply(ref xfA.R, LocalAnchorA - bA.LocalCenter);
            Vector2 rB = MathUtils.Multiply(ref xfB.R, LocalAnchorB - bB.LocalCenter);

            // Solve angular friction
            {
                float Cdot = wB - wA;
                float impulse = -_angularMass * Cdot;

                float oldImpulse = _angularImpulse;
                float maxImpulse = step.DeltaTime * MaxTorque;
                _angularImpulse = MathUtils.Clamp(_angularImpulse + impulse, -maxImpulse, maxImpulse);
                impulse = _angularImpulse - oldImpulse;

                wA -= iA * impulse;
                wB += iB * impulse;
            }

            // Solve linear friction
            {
                Vector2 Cdot = vB + MathUtils.Cross(wB, rB) - vA - MathUtils.Cross(wA, rA);

                Vector2 impulse = -MathUtils.Multiply(ref _linearMass, Cdot);
                Vector2 oldImpulse = _linearImpulse;
                _linearImpulse += impulse;

                float maxImpulse = step.DeltaTime * MaxForce;

                if (_linearImpulse.LengthSquared() > maxImpulse * maxImpulse)
                {
                    _linearImpulse.Normalize();
                    _linearImpulse *= maxImpulse;
                }

                impulse = _linearImpulse - oldImpulse;

                vA -= mA * impulse;
                wA -= iA * MathUtils.Cross(rA, impulse);

                vB += mB * impulse;
                wB += iB * MathUtils.Cross(rB, impulse);
            }

            bA._linearVelocity = vA;
            bA._angularVelocity = wA;
            bB._linearVelocity = vB;
            bB._angularVelocity = wB;
        }

        internal override bool SolvePositionConstraints()
        {
            return true;
        }
    }
}