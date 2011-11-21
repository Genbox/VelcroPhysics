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

using System;
using System.Diagnostics;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
    /// <summary>
    /// A wheel joint. This joint provides two degrees of freedom: translation
    /// along an axis fixed in bodyA and rotation in the plane. You can use a
    /// joint limit to restrict the range of motion and a joint motor to drive
    /// the rotation or to model rotational friction.
    /// This joint is designed for vehicle suspensions.
    /// </summary>
    public class WheelJoint : Joint
    {
        // Solver shared
        private Vector2 m_localXAxisA;
        private Vector2 m_localYAxisA;

        private float m_impulse;
        private float m_motorImpulse;
        private float m_springImpulse;

        private float m_maxMotorTorque;
        private float m_motorSpeed;
        private bool m_enableMotor;

        // Solver temp
        private int m_indexA;
        private int m_indexB;
        private Vector2 m_localCenterA;
        private Vector2 m_localCenterB;
        private float m_invMassA;
        private float m_invMassB;
        private float m_invIA;
        private float m_invIB;

        private Vector2 m_ax, m_ay;
        private float m_sAx, m_sBx;
        private float m_sAy, m_sBy;

        private float m_mass;
        private float m_motorMass;
        private float m_springMass;

        private float m_bias;
        private float m_gamma;

        // Linear constraint (point-to-line)
        // d = pB - pA = xB + rB - xA - rA
        // C = dot(ay, d)
        // Cdot = dot(d, cross(wA, ay)) + dot(ay, vB + cross(wB, rB) - vA - cross(wA, rA))
        //      = -dot(ay, vA) - dot(cross(d + rA, ay), wA) + dot(ay, vB) + dot(cross(rB, ay), vB)
        // J = [-ay, -cross(d + rA, ay), ay, cross(rB, ay)]

        // Spring linear constraint
        // C = dot(ax, d)
        // Cdot = = -dot(ax, vA) - dot(cross(d + rA, ax), wA) + dot(ax, vB) + dot(cross(rB, ax), vB)
        // J = [-ax -cross(d+rA, ax) ax cross(rB, ax)]

        // Motor rotational constraint
        // Cdot = wB - wA
        // J = [0 0 -1 0 0 1]

        internal WheelJoint()
        {
            JointType = JointType.Wheel;
        }

        public WheelJoint(Body bA, Body bB, Vector2 anchor, Vector2 axis)
            : base(bA, bB)
        {
            JointType = JointType.Wheel;
            LocalAnchorA = bA.GetLocalPoint(anchor);
            LocalAnchorB = bB.GetLocalPoint(anchor);
            m_localXAxisA = bA.GetLocalVector(axis);
            m_localYAxisA = MathUtils.Cross(1.0f, m_localXAxisA);
        }

        public Vector2 LocalAnchorA { get; set; }

        public Vector2 LocalAnchorB { get; set; }

        public override Vector2 WorldAnchorA
        {
            get { return BodyA.GetWorldPoint(LocalAnchorA); }
        }

        public override Vector2 WorldAnchorB
        {
            get { return BodyB.GetWorldPoint(LocalAnchorB); }
            set { Debug.Assert(false, "You can't set the world anchor on this joint type."); }
        }

        /// The desired motor speed in radians per second.
        public float MotorSpeed
        {
            get { return m_motorSpeed; }
            set
            {
                WakeBodies();
                m_motorSpeed = value;
            }
        }

        /// The maximum motor torque, usually in N-m.
        public float MaxMotorTorque
        {
            get { return m_maxMotorTorque; }
            set
            {
                WakeBodies();
                m_maxMotorTorque = value;
            }
        }

        /// Suspension frequency, zero indicates no suspension
        public float SpringFrequencyHz { get; set; }

        /// Suspension damping ratio, one indicates critical damping
        public float SpringDampingRatio { get; set; }

        public override Vector2 GetReactionForce(float invDt)
        {
            return invDt * (m_impulse * m_ay + m_springImpulse * m_ax);
        }

        public override float GetReactionTorque(float invDt)
        {
            return invDt * m_motorImpulse;
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

            float mA = m_invMassA, mB = m_invMassB;
            float iA = m_invIA, iB = m_invIB;

            Vector2 cA = data.positions[m_indexA].c;
            float aA = data.positions[m_indexA].a;
            Vector2 vA = data.velocities[m_indexA].v;
            float wA = data.velocities[m_indexA].w;

            Vector2 cB = data.positions[m_indexB].c;
            float aB = data.positions[m_indexB].a;
            Vector2 vB = data.velocities[m_indexB].v;
            float wB = data.velocities[m_indexB].w;

            Rot qA = new Rot(aA), qB = new Rot(aB);

            // Compute the effective masses.
            Vector2 rA = MathUtils.Mul(qA, LocalAnchorA - m_localCenterA);
            Vector2 rB = MathUtils.Mul(qB, LocalAnchorB - m_localCenterB);
            Vector2 d1 = cB + rB - cA - rA;

            // Point to line constraint
            {
                m_ay = MathUtils.Mul(qA, m_localYAxisA);
                m_sAy = MathUtils.Cross(d1 + rA, m_ay);
                m_sBy = MathUtils.Cross(rB, m_ay);

                m_mass = mA + mB + iA * m_sAy * m_sAy + iB * m_sBy * m_sBy;

                if (m_mass > 0.0f)
                {
                    m_mass = 1.0f / m_mass;
                }
            }

            // Spring constraint
            m_springMass = 0.0f;
            m_bias = 0.0f;
            m_gamma = 0.0f;
            if (SpringFrequencyHz > 0.0f)
            {
                m_ax = MathUtils.Mul(qA, m_localXAxisA);
                m_sAx = MathUtils.Cross(d1 + rA, m_ax);
                m_sBx = MathUtils.Cross(rB, m_ax);

                float invMass = mA + mB + iA * m_sAx * m_sAx + iB * m_sBx * m_sBx;

                if (invMass > 0.0f)
                {
                    m_springMass = 1.0f / invMass;

                    float C = Vector2.Dot(d1, m_ax);

                    // Frequency
                    float omega = 2.0f * Settings.Pi * SpringFrequencyHz;

                    // Damping coefficient
                    float d = 2.0f * m_springMass * SpringDampingRatio * omega;

                    // Spring stiffness
                    float k = m_springMass * omega * omega;

                    // magic formulas
                    float h = data.step.dt;
                    m_gamma = h * (d + h * k);
                    if (m_gamma > 0.0f)
                    {
                        m_gamma = 1.0f / m_gamma;
                    }

                    m_bias = C * h * k * m_gamma;

                    m_springMass = invMass + m_gamma;
                    if (m_springMass > 0.0f)
                    {
                        m_springMass = 1.0f / m_springMass;
                    }
                }
            }
            else
            {
                m_springImpulse = 0.0f;
            }

            // Rotational motor
            if (m_enableMotor)
            {
                m_motorMass = iA + iB;
                if (m_motorMass > 0.0f)
                {
                    m_motorMass = 1.0f / m_motorMass;
                }
            }
            else
            {
                m_motorMass = 0.0f;
                m_motorImpulse = 0.0f;
            }

            if (Settings.EnableWarmstarting)
            {
                // Account for variable time step.
                m_impulse *= data.step.dtRatio;
                m_springImpulse *= data.step.dtRatio;
                m_motorImpulse *= data.step.dtRatio;

                Vector2 P = m_impulse * m_ay + m_springImpulse * m_ax;
                float LA = m_impulse * m_sAy + m_springImpulse * m_sAx + m_motorImpulse;
                float LB = m_impulse * m_sBy + m_springImpulse * m_sBx + m_motorImpulse;

                vA -= m_invMassA * P;
                wA -= m_invIA * LA;

                vB += m_invMassB * P;
                wB += m_invIB * LB;
            }
            else
            {
                m_impulse = 0.0f;
                m_springImpulse = 0.0f;
                m_motorImpulse = 0.0f;
            }

            data.velocities[m_indexA].v = vA;
            data.velocities[m_indexA].w = wA;
            data.velocities[m_indexB].v = vB;
            data.velocities[m_indexB].w = wB;
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            float mA = m_invMassA, mB = m_invMassB;
            float iA = m_invIA, iB = m_invIB;

            Vector2 vA = data.velocities[m_indexA].v;
            float wA = data.velocities[m_indexA].w;
            Vector2 vB = data.velocities[m_indexB].v;
            float wB = data.velocities[m_indexB].w;

            // Solve spring constraint
            {
                float Cdot = Vector2.Dot(m_ax, vB - vA) + m_sBx * wB - m_sAx * wA;
                float impulse = -m_springMass * (Cdot + m_bias + m_gamma * m_springImpulse);
                m_springImpulse += impulse;

                Vector2 P = impulse * m_ax;
                float LA = impulse * m_sAx;
                float LB = impulse * m_sBx;

                vA -= mA * P;
                wA -= iA * LA;

                vB += mB * P;
                wB += iB * LB;
            }

            // Solve rotational motor constraint
            {
                float Cdot = wB - wA - m_motorSpeed;
                float impulse = -m_motorMass * Cdot;

                float oldImpulse = m_motorImpulse;
                float maxImpulse = data.step.dt * m_maxMotorTorque;
                m_motorImpulse = MathUtils.Clamp(m_motorImpulse + impulse, -maxImpulse, maxImpulse);
                impulse = m_motorImpulse - oldImpulse;

                wA -= iA * impulse;
                wB += iB * impulse;
            }

            // Solve point to line constraint
            {
                float Cdot = Vector2.Dot(m_ay, vB - vA) + m_sBy * wB - m_sAy * wA;
                float impulse = -m_mass * Cdot;
                m_impulse += impulse;

                Vector2 P = impulse * m_ay;
                float LA = impulse * m_sAy;
                float LB = impulse * m_sBy;

                vA -= mA * P;
                wA -= iA * LA;

                vB += mB * P;
                wB += iB * LB;
            }

            data.velocities[m_indexA].v = vA;
            data.velocities[m_indexA].w = wA;
            data.velocities[m_indexB].v = vB;
            data.velocities[m_indexB].w = wB;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            Vector2 cA = data.positions[m_indexA].c;
            float aA = data.positions[m_indexA].a;
            Vector2 cB = data.positions[m_indexB].c;
            float aB = data.positions[m_indexB].a;

            Rot qA = new Rot(aA), qB = new Rot(aB);

            Vector2 rA = MathUtils.Mul(qA, LocalAnchorA - m_localCenterA);
            Vector2 rB = MathUtils.Mul(qB, LocalAnchorB - m_localCenterB);
            Vector2 d = (cB - cA) + rB - rA;

            Vector2 ay = MathUtils.Mul(qA, m_localYAxisA);

            float sAy = MathUtils.Cross(d + rA, ay);
            float sBy = MathUtils.Cross(rB, ay);

            float C = Vector2.Dot(d, ay);

            float k = m_invMassA + m_invMassB + m_invIA * m_sAy * m_sAy + m_invIB * m_sBy * m_sBy;

            float impulse;
            if (k != 0.0f)
            {
                impulse = -C / k;
            }
            else
            {
                impulse = 0.0f;
            }

            Vector2 P = impulse * ay;
            float LA = impulse * sAy;
            float LB = impulse * sBy;

            cA -= m_invMassA * P;
            aA -= m_invIA * LA;
            cB += m_invMassB * P;
            aB += m_invIB * LB;

            data.positions[m_indexA].c = cA;
            data.positions[m_indexA].a = aA;
            data.positions[m_indexB].c = cB;
            data.positions[m_indexB].a = aB;

            return Math.Abs(C) <= Settings.LinearSlop;
        }

        public float JointTranslation
        {
            get
            {
                Body bA = BodyA;
                Body bB = BodyB;

                Vector2 pA = bA.GetWorldPoint(LocalAnchorA);
                Vector2 pB = bB.GetWorldPoint(LocalAnchorB);
                Vector2 d = pB - pA;
                Vector2 axis = bA.GetWorldVector(m_localXAxisA);

                float translation = Vector2.Dot(d, axis);
                return translation;
            }
        }

        public float JointSpeed
        {
            get
            {
                float wA = BodyA.AngularVelocity;
                float wB = BodyB.AngularVelocity;
                return wB - wA;
            }
        }

        /// Enable/disable the joint motor.
        public bool MotorEnabled
        {
            get { return m_enableMotor; }
            set
            {
                WakeBodies();
                m_enableMotor = value;
            }
        }

        public float GetMotorTorque(float inv_dt)
        {
            return inv_dt * m_motorImpulse;
        }
    }
}