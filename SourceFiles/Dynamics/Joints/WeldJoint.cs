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

    /// <summary>
    /// A weld joint essentially glues two bodies together. A weld joint may
    /// distort somewhat because the island constraint solver is approximate.
    /// </summary>
    public class WeldJoint : Joint
    {
        // Solver shared
        public Vector2 LocalAnchorA;
        public Vector2 LocalAnchorB;
        private Vector3 _impulse;
        private float m_gamma;

        private float m_frequencyHz;
        private float m_dampingRatio;
        private float m_bias;

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
        private Mat33 _mass;

        internal WeldJoint()
        {
            JointType = JointType.Weld;
        }

        /// <summary>
        /// You need to specify a local anchor point
        /// where they are attached and the relative body angle. The position
        /// of the anchor point is important for computing the reaction torque.
        /// You can change the anchor points relative to bodyA or bodyB by changing LocalAnchorA
        /// and/or LocalAnchorB.
        /// </summary>
        /// <param name="bodyA">The first body</param>
        /// <param name="bodyB">The second body</param>
        /// <param name="localAnchorA">The first body anchor.</param>
        /// <param name="localAnchorB">The second body anchor.</param>
        public WeldJoint(Body bodyA, Body bodyB, Vector2 localAnchorA, Vector2 localAnchorB)
            : base(bodyA, bodyB)
        {
            JointType = JointType.Weld;

            LocalAnchorA = localAnchorA;
            LocalAnchorB = localAnchorB;
            ReferenceAngle = BodyB.Rotation - BodyA.Rotation;
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
        /// The body2 angle minus body1 angle in the reference state (radians).
        /// </summary>
        public float ReferenceAngle { get; private set; }

        public override Vector2 GetReactionForce(float inv_dt)
        {
            return inv_dt * new Vector2(_impulse.X, _impulse.Y);
        }

        public override float GetReactionTorque(float inv_dt)
        {
            return inv_dt * _impulse.Z;
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

            Vector2 cA = data.positions[m_indexA].c;
            float aA = data.positions[m_indexA].a;
            Vector2 vA = data.velocities[m_indexA].v;
            float wA = data.velocities[m_indexA].w;

            Vector2 cB = data.positions[m_indexB].c;
            float aB = data.positions[m_indexB].a;
            Vector2 vB = data.velocities[m_indexB].v;
            float wB = data.velocities[m_indexB].w;

            Rot qA = new Rot(aA), qB = new Rot(aB);

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

            Mat33 K = new Mat33();
            K.ex.X = mA + mB + m_rA.Y * m_rA.Y * iA + m_rB.Y * m_rB.Y * iB;
            K.ey.X = -m_rA.Y * m_rA.X * iA - m_rB.Y * m_rB.X * iB;
            K.ez.X = -m_rA.Y * iA - m_rB.Y * iB;
            K.ex.Y = K.ey.X;
            K.ey.Y = mA + mB + m_rA.X * m_rA.X * iA + m_rB.X * m_rB.X * iB;
            K.ez.Y = m_rA.X * iA + m_rB.X * iB;
            K.ex.Z = K.ez.X;
            K.ey.Z = K.ez.Y;
            K.ez.Z = iA + iB;

            if (m_frequencyHz > 0.0f)
            {
                K.GetInverse22(ref _mass);

                float invM = iA + iB;
                float m = invM > 0.0f ? 1.0f / invM : 0.0f;

                float C = aB - aA - ReferenceAngle;

                // Frequency
                float omega = 2.0f * Settings.Pi * m_frequencyHz;

                // Damping coefficient
                float d = 2.0f * m * m_dampingRatio * omega;

                // Spring stiffness
                float k = m * omega * omega;

                // magic formulas
                float h = data.step.dt;
                m_gamma = h * (d + h * k);
                m_gamma = m_gamma != 0.0f ? 1.0f / m_gamma : 0.0f;
                m_bias = C * h * k * m_gamma;

                invM += m_gamma;
                _mass.ez.Z = invM != 0.0f ? 1.0f / invM : 0.0f;
            }
            else
            {
                K.GetSymInverse33(ref _mass);
                m_gamma = 0.0f;
                m_bias = 0.0f;
            }

            if (Settings.EnableWarmstarting)
            {
                // Scale impulses to support a variable time step.
                _impulse *= data.step.dtRatio;

                Vector2 P = new Vector2(_impulse.X, _impulse.Y);

                vA -= mA * P;
                wA -= iA * (MathUtils.Cross(m_rA, P) + _impulse.Z);

                vB += mB * P;
                wB += iB * (MathUtils.Cross(m_rB, P) + _impulse.Z);
            }
            else
            {
                _impulse = Vector3.Zero;
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

            if (m_frequencyHz > 0.0f)
            {
                float Cdot2 = wB - wA;

                float impulse2 = -_mass.ez.Z * (Cdot2 + m_bias + m_gamma * _impulse.Z);
                _impulse.Z += impulse2;

                wA -= iA * impulse2;
                wB += iB * impulse2;

                Vector2 Cdot1 = vB + MathUtils.Cross(wB, m_rB) - vA - MathUtils.Cross(wA, m_rA);

                Vector2 impulse1 = -MathUtils.Mul22(_mass, Cdot1);
                _impulse.X += impulse1.X;
                _impulse.Y += impulse1.Y;

                Vector2 P = impulse1;

                vA -= mA * P;
                wA -= iA * MathUtils.Cross(m_rA, P);

                vB += mB * P;
                wB += iB * MathUtils.Cross(m_rB, P);
            }
            else
            {
                Vector2 Cdot1 = vB + MathUtils.Cross(wB, m_rB) - vA - MathUtils.Cross(wA, m_rA);
                float Cdot2 = wB - wA;
                Vector3 Cdot = new Vector3(Cdot1.X, Cdot1.Y, Cdot2);

                Vector3 impulse = -MathUtils.Mul(_mass, Cdot);
                _impulse += impulse;

                Vector2 P = new Vector2(impulse.X, impulse.Y);

                vA -= mA * P;
                wA -= iA * (MathUtils.Cross(m_rA, P) + impulse.Z);

                vB += mB * P;
                wB += iB * (MathUtils.Cross(m_rB, P) + impulse.Z);
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

            float mA = m_invMassA, mB = m_invMassB;
            float iA = m_invIA, iB = m_invIB;

            Vector2 rA = MathUtils.Mul(qA, LocalAnchorA - m_localCenterA);
            Vector2 rB = MathUtils.Mul(qB, LocalAnchorB - m_localCenterB);

            float positionError, angularError;

            Mat33 K = new Mat33();
            K.ex.X = mA + mB + rA.Y * rA.Y * iA + rB.Y * rB.Y * iB;
            K.ey.X = -rA.Y * rA.X * iA - rB.Y * rB.X * iB;
            K.ez.X = -rA.Y * iA - rB.Y * iB;
            K.ex.Y = K.ey.X;
            K.ey.Y = mA + mB + rA.X * rA.X * iA + rB.X * rB.X * iB;
            K.ez.Y = rA.X * iA + rB.X * iB;
            K.ex.Z = K.ez.X;
            K.ey.Z = K.ez.Y;
            K.ez.Z = iA + iB;

            if (m_frequencyHz > 0.0f)
            {
                Vector2 C1 = cB + rB - cA - rA;

                positionError = C1.Length();
                angularError = 0.0f;

                Vector2 P = -K.Solve22(C1);

                cA -= mA * P;
                aA -= iA * MathUtils.Cross(rA, P);

                cB += mB * P;
                aB += iB * MathUtils.Cross(rB, P);
            }
            else
            {
                Vector2 C1 = cB + rB - cA - rA;
                float C2 = aB - aA - ReferenceAngle;

                positionError = C1.Length();
                angularError = Math.Abs(C2);

                Vector3 C = new Vector3(C1.X, C1.Y, C2);

                Vector3 impulse = -K.Solve33(C);
                Vector2 P = new Vector2(impulse.X, impulse.Y);

                cA -= mA * P;
                aA -= iA * (MathUtils.Cross(rA, P) + impulse.Z);

                cB += mB * P;
                aB += iB * (MathUtils.Cross(rB, P) + impulse.Z);
            }

            data.positions[m_indexA].c = cA;
            data.positions[m_indexA].a = aA;
            data.positions[m_indexB].c = cB;
            data.positions[m_indexB].a = aB;

            return positionError <= Settings.LinearSlop && angularError <= Settings.AngularSlop;
        }
    }
}