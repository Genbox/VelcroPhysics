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
    // Gear Joint:
    // C0 = (coordinate1 + ratio * coordinate2)_initial
    // C = (coordinate1 + ratio * coordinate2) - C0 = 0
    // J = [J1 ratio * J2]
    // K = J * invM * JT
    //   = J1 * invM1 * J1T + ratio * ratio * J2 * invM2 * J2T
    //
    // Revolute:
    // coordinate = rotation
    // Cdot = angularVelocity
    // J = [0 0 1]
    // K = J * invM * JT = invI
    //
    // Prismatic:
    // coordinate = dot(p - pg, ug)
    // Cdot = dot(v + cross(w, r), ug)
    // J = [ug cross(r, ug)]
    // K = J * invM * JT = invMass + invI * cross(r, ug)^2

    /// <summary>
    /// A gear joint is used to connect two joints together. Either joint
    /// can be a revolute or prismatic joint. You specify a gear ratio
    /// to bind the motions together:
    /// coordinate1 + ratio * coordinate2 = ant
    /// The ratio can be negative or positive. If one joint is a revolute joint
    /// and the other joint is a prismatic joint, then the ratio will have units
    /// of length or units of 1/length.
    /// @warning You have to manually destroy the gear joint if joint1 or joint2
    /// is destroyed.
    /// </summary>
    public class GearJoint : Joint
    {
        private Joint m_joint1;
        private Joint m_joint2;

        private JointType m_typeA;
        private JointType m_typeB;

        // Body A is connected to body C
        // Body B is connected to body D
        private Body m_bodyC;
        private Body m_bodyD;

        // Solver shared
        private Vector2 m_localAnchorA;
        private Vector2 m_localAnchorB;
        private Vector2 m_localAnchorC;
        private Vector2 m_localAnchorD;

        private Vector2 m_localAxisC;
        private Vector2 m_localAxisD;

        private float m_referenceAngleA;
        private float m_referenceAngleB;

        private float m_constant;
        private float _ratio;

        private float m_impulse;

        // Solver temp
        private int m_indexA, m_indexB, m_indexC, m_indexD;
        private Vector2 m_lcA, m_lcB, m_lcC, m_lcD;
        private float m_mA, m_mB, m_mC, m_mD;
        private float m_iA, m_iB, m_iC, m_iD;
        private Vector2 m_JvAC, m_JvBD;
        private float m_JwA, m_JwB, m_JwC, m_JwD;
        private float m_mass;

        /// <summary>
        /// Requires two existing revolute or prismatic joints (any combination will work).
        /// The provided joints must attach a dynamic body to a static body.
        /// </summary>
        /// <param name="jointA">The first joint.</param>
        /// <param name="jointB">The second joint.</param>
        /// <param name="ratio">The ratio.</param>
        public GearJoint(Joint jointA, Joint jointB, float ratio)
            : base(jointA.BodyA, jointA.BodyB)
        {
            JointType = JointType.Gear;
            JointA = jointA;
            JointB = jointB;
            Ratio = ratio;

            m_typeA = jointA.JointType;
            m_typeB = jointB.JointType;

            // Make sure its the right kind of joint
            Debug.Assert(m_typeA == JointType.Revolute || m_typeA == JointType.Prismatic || m_typeA == JointType.FixedRevolute || m_typeA == JointType.FixedPrismatic);
            Debug.Assert(m_typeB == JointType.Revolute || m_typeB == JointType.Prismatic || m_typeB == JointType.FixedRevolute || m_typeB == JointType.FixedPrismatic);

            float coordinateA = 0.0f, coordinateB = 0.0f;

            m_bodyC = m_joint1.BodyA;
            BodyA = m_joint1.BodyB;

            // Get geometry of joint1
            Transform xfA = BodyA.Xf;
            float aA = BodyA.Sweep.A;
            Transform xfC = m_bodyC.Xf;
            float aC = m_bodyC.Sweep.A;

            if (m_typeA == JointType.Revolute)
            {
                RevoluteJoint revolute = (RevoluteJoint)jointA;
                m_localAnchorC = revolute.LocalAnchorA;
                m_localAnchorA = revolute.LocalAnchorB;
                m_referenceAngleA = revolute.ReferenceAngle;
                m_localAxisC = Vector2.Zero;

                coordinateA = aA - aC - m_referenceAngleA;
            }
            else
            {
                PrismaticJoint prismatic = (PrismaticJoint)jointA;
                m_localAnchorC = prismatic.LocalAnchorA;
                m_localAnchorA = prismatic.LocalAnchorB;
                m_referenceAngleA = prismatic.ReferenceAngle;
                m_localAxisC = prismatic.LocalXAxisA;

                Vector2 pC = m_localAnchorC;
                Vector2 pA = MathUtils.MulT(xfC.q, MathUtils.Mul(xfA.q, m_localAnchorA) + (xfA.p - xfC.p));
                coordinateA = Vector2.Dot(pA - pC, m_localAxisC);
            }

            m_bodyD = m_joint2.BodyA;
            BodyB = m_joint2.BodyB;

            // Get geometry of joint2
            Transform xfB = BodyB.Xf;
            float aB = BodyB.Sweep.A;
            Transform xfD = m_bodyD.Xf;
            float aD = m_bodyD.Sweep.A;

            if (m_typeB == JointType.Revolute)
            {
                RevoluteJoint revolute = (RevoluteJoint)jointB;
                m_localAnchorD = revolute.LocalAnchorA;
                m_localAnchorB = revolute.LocalAnchorB;
                m_referenceAngleB = revolute.ReferenceAngle;
                m_localAxisD = Vector2.Zero;

                coordinateB = aB - aD - m_referenceAngleB;
            }
            else
            {
                PrismaticJoint prismatic = (PrismaticJoint)jointB;
                m_localAnchorD = prismatic.LocalAnchorA;
                m_localAnchorB = prismatic.LocalAnchorB;
                m_referenceAngleB = prismatic.ReferenceAngle;
                m_localAxisD = prismatic.LocalXAxisA;

                Vector2 pD = m_localAnchorD;
                Vector2 pB = MathUtils.MulT(xfD.q, MathUtils.Mul(xfB.q, m_localAnchorB) + (xfB.p - xfD.p));
                coordinateB = Vector2.Dot(pB - pD, m_localAxisD);
            }

            _ratio = ratio;
            m_constant = coordinateA + _ratio * coordinateB;
        }

        public override Vector2 WorldAnchorA
        {
            get { return BodyA.GetWorldPoint(m_localAnchorA); }
        }

        public override Vector2 WorldAnchorB
        {
            get { return BodyB.GetWorldPoint(m_localAnchorB); }
            set { Debug.Assert(false, "You can't set the world anchor on this joint type."); }
        }


        /// <summary>
        /// The gear ratio.
        /// </summary>
        public float Ratio
        {
            get { return _ratio; }
            set
            {
                Debug.Assert(MathUtils.IsValid(value));
                _ratio = value;
            }
        }

        /// <summary>
        /// The first revolute/prismatic joint attached to the gear joint.
        /// </summary>
        public Joint JointA { get; set; }

        /// <summary>
        /// The second revolute/prismatic joint attached to the gear joint.
        /// </summary>
        public Joint JointB { get; set; }


        public override Vector2 GetReactionForce(float inv_dt)
        {
            Vector2 P = m_impulse * m_JvAC;
            return inv_dt * P;
        }

        public override float GetReactionTorque(float inv_dt)
        {
            float L = m_impulse * m_JwA;
            return inv_dt * L;
        }

        internal override void InitVelocityConstraints(ref SolverData data)
        {
            m_indexA = BodyA.IslandIndex;
            m_indexB = BodyB.IslandIndex;
            m_indexC = m_bodyC.IslandIndex;
            m_indexD = m_bodyD.IslandIndex;
            m_lcA = BodyA.Sweep.LocalCenter;
            m_lcB = BodyB.Sweep.LocalCenter;
            m_lcC = m_bodyC.Sweep.LocalCenter;
            m_lcD = m_bodyD.Sweep.LocalCenter;
            m_mA = BodyA.InvMass;
            m_mB = BodyB.InvMass;
            m_mC = m_bodyC.InvMass;
            m_mD = m_bodyD.InvMass;
            m_iA = BodyA.InvI;
            m_iB = BodyB.InvI;
            m_iC = m_bodyC.InvI;
            m_iD = m_bodyD.InvI;

            Vector2 cA = data.positions[m_indexA].c;
            float aA = data.positions[m_indexA].a;
            Vector2 vA = data.velocities[m_indexA].v;
            float wA = data.velocities[m_indexA].w;

            Vector2 cB = data.positions[m_indexB].c;
            float aB = data.positions[m_indexB].a;
            Vector2 vB = data.velocities[m_indexB].v;
            float wB = data.velocities[m_indexB].w;

            Vector2 cC = data.positions[m_indexC].c;
            float aC = data.positions[m_indexC].a;
            Vector2 vC = data.velocities[m_indexC].v;
            float wC = data.velocities[m_indexC].w;

            Vector2 cD = data.positions[m_indexD].c;
            float aD = data.positions[m_indexD].a;
            Vector2 vD = data.velocities[m_indexD].v;
            float wD = data.velocities[m_indexD].w;

            Rot qA = new Rot(aA), qB = new Rot(aB), qC = new Rot(aC), qD = new Rot(aD);

            m_mass = 0.0f;

            if (m_typeA == JointType.Revolute)
            {
                m_JvAC = Vector2.Zero;
                m_JwA = 1.0f;
                m_JwC = 1.0f;
                m_mass += m_iA + m_iC;
            }
            else
            {
                Vector2 u = MathUtils.Mul(qC, m_localAxisC);
                Vector2 rC = MathUtils.Mul(qC, m_localAnchorC - m_lcC);
                Vector2 rA = MathUtils.Mul(qA, m_localAnchorA - m_lcA);
                m_JvAC = u;
                m_JwC = MathUtils.Cross(rC, u);
                m_JwA = MathUtils.Cross(rA, u);
                m_mass += m_mC + m_mA + m_iC * m_JwC * m_JwC + m_iA * m_JwA * m_JwA;
            }

            if (m_typeB == JointType.Revolute)
            {
                m_JvBD = Vector2.Zero;
                m_JwB = _ratio;
                m_JwD = _ratio;
                m_mass += _ratio * _ratio * (m_iB + m_iD);
            }
            else
            {
                Vector2 u = MathUtils.Mul(qD, m_localAxisD);
                Vector2 rD = MathUtils.Mul(qD, m_localAnchorD - m_lcD);
                Vector2 rB = MathUtils.Mul(qB, m_localAnchorB - m_lcB);
                m_JvBD = _ratio * u;
                m_JwD = _ratio * MathUtils.Cross(rD, u);
                m_JwB = _ratio * MathUtils.Cross(rB, u);
                m_mass += _ratio * _ratio * (m_mD + m_mB) + m_iD * m_JwD * m_JwD + m_iB * m_JwB * m_JwB;
            }

            // Compute effective mass.
            m_mass = m_mass > 0.0f ? 1.0f / m_mass : 0.0f;

            if (Settings.EnableWarmstarting)
            {
                vA += (m_mA * m_impulse) * m_JvAC;
                wA += m_iA * m_impulse * m_JwA;
                vB += (m_mB * m_impulse) * m_JvBD;
                wB += m_iB * m_impulse * m_JwB;
                vC -= (m_mC * m_impulse) * m_JvAC;
                wC -= m_iC * m_impulse * m_JwC;
                vD -= (m_mD * m_impulse) * m_JvBD;
                wD -= m_iD * m_impulse * m_JwD;
            }
            else
            {
                m_impulse = 0.0f;
            }

            data.velocities[m_indexA].v = vA;
            data.velocities[m_indexA].w = wA;
            data.velocities[m_indexB].v = vB;
            data.velocities[m_indexB].w = wB;
            data.velocities[m_indexC].v = vC;
            data.velocities[m_indexC].w = wC;
            data.velocities[m_indexD].v = vD;
            data.velocities[m_indexD].w = wD;
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            Vector2 vA = data.velocities[m_indexA].v;
            float wA = data.velocities[m_indexA].w;
            Vector2 vB = data.velocities[m_indexB].v;
            float wB = data.velocities[m_indexB].w;
            Vector2 vC = data.velocities[m_indexC].v;
            float wC = data.velocities[m_indexC].w;
            Vector2 vD = data.velocities[m_indexD].v;
            float wD = data.velocities[m_indexD].w;

            float Cdot = Vector2.Dot(m_JvAC, vA - vC) + Vector2.Dot(m_JvBD, vB - vD);
            Cdot += (m_JwA * wA - m_JwC * wC) + (m_JwB * wB - m_JwD * wD);

            float impulse = -m_mass * Cdot;
            m_impulse += impulse;

            vA += (m_mA * impulse) * m_JvAC;
            wA += m_iA * impulse * m_JwA;
            vB += (m_mB * impulse) * m_JvBD;
            wB += m_iB * impulse * m_JwB;
            vC -= (m_mC * impulse) * m_JvAC;
            wC -= m_iC * impulse * m_JwC;
            vD -= (m_mD * impulse) * m_JvBD;
            wD -= m_iD * impulse * m_JwD;

            data.velocities[m_indexA].v = vA;
            data.velocities[m_indexA].w = wA;
            data.velocities[m_indexB].v = vB;
            data.velocities[m_indexB].w = wB;
            data.velocities[m_indexC].v = vC;
            data.velocities[m_indexC].w = wC;
            data.velocities[m_indexD].v = vD;
            data.velocities[m_indexD].w = wD;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            Vector2 cA = data.positions[m_indexA].c;
            float aA = data.positions[m_indexA].a;
            Vector2 cB = data.positions[m_indexB].c;
            float aB = data.positions[m_indexB].a;
            Vector2 cC = data.positions[m_indexC].c;
            float aC = data.positions[m_indexC].a;
            Vector2 cD = data.positions[m_indexD].c;
            float aD = data.positions[m_indexD].a;

            Rot qA = new Rot(aA), qB = new Rot(aB), qC = new Rot(aC), qD = new Rot(aD);

            float linearError = 0.0f;

            float coordinateA, coordinateB;

            Vector2 JvAC, JvBD;
            float JwA, JwB, JwC, JwD;
            float mass = 0.0f;

            if (m_typeA == JointType.Revolute)
            {
                JvAC = Vector2.Zero;
                JwA = 1.0f;
                JwC = 1.0f;
                mass += m_iA + m_iC;

                coordinateA = aA - aC - m_referenceAngleA;
            }
            else
            {
                Vector2 u = MathUtils.Mul(qC, m_localAxisC);
                Vector2 rC = MathUtils.Mul(qC, m_localAnchorC - m_lcC);
                Vector2 rA = MathUtils.Mul(qA, m_localAnchorA - m_lcA);
                JvAC = u;
                JwC = MathUtils.Cross(rC, u);
                JwA = MathUtils.Cross(rA, u);
                mass += m_mC + m_mA + m_iC * JwC * JwC + m_iA * JwA * JwA;

                Vector2 pC = m_localAnchorC - m_lcC;
                Vector2 pA = MathUtils.MulT(qC, rA + (cA - cC));
                coordinateA = Vector2.Dot(pA - pC, m_localAxisC);
            }

            if (m_typeB == JointType.Revolute)
            {
                JvBD = Vector2.Zero;
                JwB = _ratio;
                JwD = _ratio;
                mass += _ratio * _ratio * (m_iB + m_iD);

                coordinateB = aB - aD - m_referenceAngleB;
            }
            else
            {
                Vector2 u = MathUtils.Mul(qD, m_localAxisD);
                Vector2 rD = MathUtils.Mul(qD, m_localAnchorD - m_lcD);
                Vector2 rB = MathUtils.Mul(qB, m_localAnchorB - m_lcB);
                JvBD = _ratio * u;
                JwD = _ratio * MathUtils.Cross(rD, u);
                JwB = _ratio * MathUtils.Cross(rB, u);
                mass += _ratio * _ratio * (m_mD + m_mB) + m_iD * JwD * JwD + m_iB * JwB * JwB;

                Vector2 pD = m_localAnchorD - m_lcD;
                Vector2 pB = MathUtils.MulT(qD, rB + (cB - cD));
                coordinateB = Vector2.Dot(pB - pD, m_localAxisD);
            }

            float C = (coordinateA + _ratio * coordinateB) - m_constant;

            float impulse = 0.0f;
            if (mass > 0.0f)
            {
                impulse = -C / mass;
            }

            cA += m_mA * impulse * JvAC;
            aA += m_iA * impulse * JwA;
            cB += m_mB * impulse * JvBD;
            aB += m_iB * impulse * JwB;
            cC -= m_mC * impulse * JvAC;
            aC -= m_iC * impulse * JwC;
            cD -= m_mD * impulse * JvBD;
            aD -= m_iD * impulse * JwD;

            data.positions[m_indexA].c = cA;
            data.positions[m_indexA].a = aA;
            data.positions[m_indexB].c = cB;
            data.positions[m_indexB].a = aB;
            data.positions[m_indexC].c = cC;
            data.positions[m_indexC].a = aC;
            data.positions[m_indexD].c = cD;
            data.positions[m_indexD].a = aD;

            // TODO_ERIN not implemented
            return linearError < Settings.LinearSlop;
        }
    }
}