/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2011 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
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

using System.Diagnostics;
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
        public Vector2 LocalAnchorA;
        public Vector2 LocalAnchorB;

        // Solver shared
        private Vector2 _linearImpulse;
        private float _angularImpulse;


        // Solver temp
        private int m_indexA;
        private int m_indexB;
        private Vector2 m_rA;
        private Vector2 m_rB;
        private Vector2 m_localCenterA;
        private Vector2 m_localCenterB;
        private float m_invMassA;
        private float m_invMassB;
        private float m_invIA;
        private float m_invIB;
        private float _angularMass;
        private Mat22 _linearMass;

        internal FrictionJoint()
        {
            JointType = JointType.Friction;
        }

        public FrictionJoint(Body bodyA, Body bodyB, Vector2 localAnchorA, Vector2 localAnchorB)
            : base(bodyA, bodyB)
        {
            JointType = JointType.Friction;
            LocalAnchorA = localAnchorA;
            LocalAnchorB = localAnchorB;
        }

        public override Vector2 WorldAnchorA
        {
            get { return BodyA.GetWorldPoint(LocalAnchorA); }
        }

        public override Vector2 WorldAnchorB
        {
            get { return BodyB.GetWorldPoint(LocalAnchorB); }
            set { Debug.Assert(false, "You can't set the world anchor on this joint type."); }
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
            return inv_dt * _linearImpulse;
        }

        public override float GetReactionTorque(float inv_dt)
        {
            return inv_dt * _angularImpulse;
        }

        internal override void InitVelocityConstraints(ref SolverData data)
        {
            m_indexA = BodyA.IslandIndex;
            m_indexB = BodyB.IslandIndex;
            m_localCenterA = BodyA.Sweep.LocalCenter;
            m_localCenterB = BodyB.Sweep.LocalCenter;
            m_invMassA = BodyA.InvMass;
            m_invMassB = BodyB.InvMass;
            m_invIA = BodyA.InvI;
            m_invIB = BodyB.InvI;

            float aA = data.positions[m_indexA].a;
            Vector2 vA = data.velocities[m_indexA].v;
            float wA = data.velocities[m_indexA].w;

            float aB = data.positions[m_indexB].a;
            Vector2 vB = data.velocities[m_indexB].v;
            float wB = data.velocities[m_indexB].w;

            Rot qA = new Rot(aA), qB = new Rot(aB);

            // Compute the effective mass matrix.
            m_rA = MathUtils.Mul(qA, LocalAnchorA - m_localCenterA);
            m_rB = MathUtils.Mul(qB, LocalAnchorB - m_localCenterB);

            // J = [-I -r1_skew I r2_skew]
            //     [ 0       -1 0       1]
            // r_skew = [-ry; rx]

            // Matlab
            // K = [ mA+r1y^2*iA+mB+r2y^2*iB,  -r1y*iA*r1x-r2y*iB*r2x,          -r1y*iA-r2y*iB]
            //     [  -r1y*iA*r1x-r2y*iB*r2x, mA+r1x^2*iA+mB+r2x^2*iB,           r1x*iA+r2x*iB]
            //     [          -r1y*iA-r2y*iB,           r1x*iA+r2x*iB,                   iA+iB]

            float mA = m_invMassA, mB = m_invMassB;
            float iA = m_invIA, iB = m_invIB;

            Mat22 K = new Mat22();
            K.ex.X = mA + mB + iA * m_rA.Y * m_rA.Y + iB * m_rB.Y * m_rB.Y;
            K.ex.Y = -iA * m_rA.X * m_rA.Y - iB * m_rB.X * m_rB.Y;
            K.ey.X = K.ex.Y;
            K.ey.Y = mA + mB + iA * m_rA.X * m_rA.X + iB * m_rB.X * m_rB.X;

            _linearMass = K.Inverse;

            _angularMass = iA + iB;
            if (_angularMass > 0.0f)
            {
                _angularMass = 1.0f / _angularMass;
            }

            if (Settings.EnableWarmstarting)
            {
                // Scale impulses to support a variable time step.
                _linearImpulse *= data.step.dtRatio;
                _angularImpulse *= data.step.dtRatio;

                Vector2 P = new Vector2(_linearImpulse.X, _linearImpulse.Y);

                vA -= mA * P;
                wA -= iA * (MathUtils.Cross(m_rA, P) + _angularImpulse);
                vB += mB * P;
                wB += iB * (MathUtils.Cross(m_rB, P) + _angularImpulse);
            }
            else
            {
                _linearImpulse = Vector2.Zero;
                _angularImpulse = 0.0f;
            }

            data.velocities[m_indexA].v = vA;
            data.velocities[m_indexA].w = wA;
            data.velocities[m_indexB].v = vB;
            data.velocities[m_indexB].w = wB;

        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            Vector2 vA = data.velocities[m_indexA].v;
            float wA = data.velocities[m_indexA].w;
            Vector2 vB = data.velocities[m_indexB].v;
            float wB = data.velocities[m_indexB].w;

            float mA = m_invMassA, mB = m_invMassB;
            float iA = m_invIA, iB = m_invIB;

            float h = data.step.dt;

            // Solve angular friction
            {
                float Cdot = wB - wA;
                float impulse = -_angularMass * Cdot;

                float oldImpulse = _angularImpulse;
                float maxImpulse = h * MaxTorque;
                _angularImpulse = MathUtils.Clamp(_angularImpulse + impulse, -maxImpulse, maxImpulse);
                impulse = _angularImpulse - oldImpulse;

                wA -= iA * impulse;
                wB += iB * impulse;
            }

            // Solve linear friction
            {
                Vector2 Cdot = vB + MathUtils.Cross(wB, m_rB) - vA - MathUtils.Cross(wA, m_rA);

                Vector2 impulse = -MathUtils.Mul(ref _linearMass, Cdot);
                Vector2 oldImpulse = _linearImpulse;
                _linearImpulse += impulse;

                float maxImpulse = h * MaxForce;

                if (_linearImpulse.LengthSquared() > maxImpulse * maxImpulse)
                {
                    _linearImpulse.Normalize();
                    _linearImpulse *= maxImpulse;
                }

                impulse = _linearImpulse - oldImpulse;

                vA -= mA * impulse;
                wA -= iA * MathUtils.Cross(m_rA, impulse);

                vB += mB * impulse;
                wB += iB * MathUtils.Cross(m_rB, impulse);
            }

            data.velocities[m_indexA].v = vA;
            data.velocities[m_indexA].w = wA;
            data.velocities[m_indexB].v = vB;
            data.velocities[m_indexB].w = wB;

        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            return true;
        }
    }
}