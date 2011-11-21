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
    // Limit:
    // C = norm(pB - pA) - L
    // u = (pB - pA) / norm(pB - pA)
    // Cdot = dot(u, vB + cross(wB, rB) - vA - cross(wA, rA))
    // J = [-u -cross(rA, u) u cross(rB, u)]
    // K = J * invM * JT
    //   = invMassA + invIA * cross(rA, u)^2 + invMassB + invIB * cross(rB, u)^2

    /// <summary>
    /// A rope joint enforces a maximum distance between two points
    /// on two bodies. It has no other effect.
    /// Warning: if you attempt to change the maximum length during
    /// the simulation you will get some non-physical behavior.
    /// A model that would allow you to dynamically modify the length
    /// would have some sponginess, so I chose not to implement it
    /// that way. See b2DistanceJoint if you want to dynamically
    /// control length.
    /// </summary>
    public class RopeJoint : Joint
    {
        // Solver shared
        public Vector2 LocalAnchorA;
        public Vector2 LocalAnchorB;

        private float _impulse;
        private float _length;

        // Solver temp
        private int m_indexA;
        private int m_indexB;
        private Vector2 m_u;
        private Vector2 m_rA;
        private Vector2 m_rB;
        private Vector2 m_localCenterA;
        private Vector2 m_localCenterB;
        private float m_invMassA;
        private float m_invMassB;
        private float m_invIA;
        private float m_invIB;
        private float _mass;
        private Vector2 _rA, _rB;
        private LimitState _state;
        private Vector2 _u;

        internal RopeJoint()
        {
            JointType = JointType.Rope;
        }

        public RopeJoint(Body bodyA, Body bodyB, Vector2 localAnchorA, Vector2 localAnchorB)
            : base(bodyA, bodyB)
        {
            JointType = JointType.Rope;
            LocalAnchorA = localAnchorA;
            LocalAnchorB = localAnchorB;

            Vector2 d = WorldAnchorB - WorldAnchorA;
            MaxLength = d.Length();

            _mass = 0.0f;
            _impulse = 0.0f;
            _state = LimitState.Inactive;
            _length = 0.0f;
        }

        /// Get the maximum length of the rope.
        public float MaxLength { get; set; }

        public LimitState State
        {
            get { return _state; }
        }

        public override sealed Vector2 WorldAnchorA
        {
            get { return BodyA.GetWorldPoint(LocalAnchorA); }
        }

        public override sealed Vector2 WorldAnchorB
        {
            get { return BodyB.GetWorldPoint(LocalAnchorB); }
            set { Debug.Assert(false, "You can't set the world anchor on this joint type."); }
        }

        public override Vector2 GetReactionForce(float invDt)
        {
            return (invDt * _impulse) * _u;
        }

        public override float GetReactionTorque(float invDt)
        {
            return 0;
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
            m_u = cB + m_rB - cA - m_rA;

            _length = _u.Length();

            float C = _length - MaxLength;
            if (C > 0.0f)
            {
                _state = LimitState.AtUpper;
            }
            else
            {
                _state = LimitState.Inactive;
            }

            if (_length > Settings.LinearSlop)
            {
                _u *= 1.0f / _length;
            }
            else
            {
                _u = Vector2.Zero;
                _mass = 0.0f;
                _impulse = 0.0f;
                return;
            }

            // Compute effective mass.
            float crA = MathUtils.Cross(_rA, _u);
            float crB = MathUtils.Cross(_rB, _u);
            float invMass = m_invMassA + m_invIA * crA * crA + m_invMassB + m_invIB * crB * crB;

            _mass = invMass != 0.0f ? 1.0f / invMass : 0.0f;

            if (Settings.EnableWarmstarting)
            {
                // Scale the impulse to support a variable time step.
                _impulse *= data.step.dtRatio;

                Vector2 P = _impulse * _u;
                vA -= m_invMassA * P;
                wA -= m_invIA * MathUtils.Cross(m_rA, P);
                vB += m_invMassB * P;
                wB += m_invIB * MathUtils.Cross(m_rB, P);
            }
            else
            {
                _impulse = 0.0f;
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


            // Cdot = dot(u, v + cross(w, r))
            Vector2 vpA = vA + MathUtils.Cross(wA, m_rA);
            Vector2 vpB = vB + MathUtils.Cross(wB, m_rB);
            float C = _length - MaxLength;
            float Cdot = Vector2.Dot(m_u, vpB - vpA);

            // Predictive constraint.
            if (C < 0.0f)
            {
                Cdot += data.step.inv_dt * C;
            }

            float impulse = -_mass * Cdot;
            float oldImpulse = _impulse;
            _impulse = Math.Min(0.0f, _impulse + impulse);
            impulse = _impulse - oldImpulse;

            Vector2 P = impulse * _u;
            vA -= m_invMassA * P;
            wA -= m_invIA * MathUtils.Cross(m_rA, P);
            vB += m_invMassB * P;
            wB += m_invIB * MathUtils.Cross(m_rB, P);

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
            Vector2 u = cB + rB - cA - rA;

            float length = u.Length();
            u.Normalize();

            float C = length - MaxLength;

            C = MathUtils.Clamp(C, 0.0f, Settings.MaxLinearCorrection);

            float impulse = -_mass * C;
            Vector2 P = impulse * u;

            cA -= m_invMassA * P;
            aA -= m_invIA * MathUtils.Cross(rA, P);
            cB += m_invMassB * P;
            aB += m_invIB * MathUtils.Cross(rB, P);

            data.positions[m_indexA].c = cA;
            data.positions[m_indexA].a = aA;
            data.positions[m_indexB].c = cB;
            data.positions[m_indexB].a = aB;

            return length - MaxLength < Settings.LinearSlop;
        }
    }
}