/*
* Copyright (c) 2006-2010 Erin Catto http://www.gphysics.com
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Common;

namespace FarseerPhysics.Dynamics.Joints
{
    /// A rope joint enforces a maximum distance between two points
    /// on two bodies. It has no other effect.
    /// Warning: if you attempt to change the maximum length during
    /// the simulation you will get some non-physical behavior.
    /// A model that would allow you to dynamically modify the length
    /// would have some sponginess, so I chose not to implement it
    /// that way. See b2DistanceJoint if you want to dynamically
    /// control length.
    /// 
    // Limit:
    // C = norm(pB - pA) - L
    // u = (pB - pA) / norm(pB - pA)
    // Cdot = dot(u, vB + cross(wB, rB) - vA - cross(wA, rA))
    // J = [-u -cross(rA, u) u cross(rB, u)]
    // K = J * invM * JT
    //   = invMassA + invIA * cross(rA, u)^2 + invMassB + invIB * cross(rB, u)^2

    public class RopeJoint : Joint
    {
        public override Vector2 WorldAnchorA
        {
            get { return BodyA.GetWorldPoint(LocalAnchorA); }
        }

        public override Vector2 WorldAnchorB
        {
            get { return BodyB.GetWorldPoint(LocalAnchorB); }
        }

        public override Vector2 GetReactionForce(float inv_dt)
        {
            Vector2 F = (inv_dt * m_impulse) * m_u;
            return F;
        }

        public override float GetReactionTorque(float inv_dt)
        {
            return 0;
        }

        /// Get the maximum length of the rope.
        public float MaxLength
        {
            get { return m_maxLength; }
            set { m_maxLength = value; }
        }

        public RopeJoint(Body bodyA, Body bodyB, Vector2 localAnchorA, Vector2 localAnchorB)
            : base(bodyA, bodyB)
        {

            LocalAnchorA = localAnchorA;
            LocalAnchorB = localAnchorB;

            Vector2 d = WorldAnchorB - WorldAnchorA;
            m_maxLength = d.Length();

            m_mass = 0.0f;
            m_impulse = 0.0f;
            State  = LimitState.Inactive;
            m_length = 0.0f;
        }

        internal override void InitVelocityConstraints(ref TimeStep step)
        {
            Body bA = BodyA;
            Body bB = BodyB;

            Transform xf1;
            bA.GetTransform(out xf1);

            Transform xf2;
            bB.GetTransform(out xf2);

            m_rA = MathUtils.Multiply(ref xf1.R, LocalAnchorA - bA.LocalCenter);
            m_rB = MathUtils.Multiply(ref xf2.R, LocalAnchorB - bB.LocalCenter);

            // Rope axis
            m_u = bB.Sweep.c + m_rB - bA.Sweep.c - m_rA;

            m_length = m_u.Length();

            float C = m_length - m_maxLength;
            if (C > 0.0f)
            {
                State = LimitState.AtUpper;
            }
            else
            {
                State = LimitState.Inactive;
            }

            if (m_length > Settings.LinearSlop)
            {
                m_u *= 1.0f / m_length;
            }
            else
            {
                m_u = Vector2.Zero;
                m_mass = 0.0f;
                m_impulse = 0.0f;
                return;
            }

            // Compute effective mass.
            float crA = MathUtils.Cross(m_rA, m_u);
            float crB = MathUtils.Cross(m_rB, m_u);
            float invMass = bA.InvMass + bA.InvI * crA * crA + bB.InvMass + bB.InvI * crB * crB;

            m_mass = invMass != 0.0f ? 1.0f / invMass : 0.0f;

            if (Settings.EnableWarmstarting)
            {
                // Scale the impulse to support a variable time step.
                m_impulse *= step.dtRatio;

                Vector2 P = m_impulse * m_u;
                bA.LinearVelocity -= bA.InvMass * P;
                bA.AngularVelocity -= bA.InvI * MathUtils.Cross(m_rA, P);
                bB.LinearVelocity += bB.InvMass * P;
                bB.AngularVelocity += bB.InvI * MathUtils.Cross(m_rB, P);
            }
            else
            {
                m_impulse = 0.0f;
            }
        }

        internal override void SolveVelocityConstraints(ref TimeStep step)
        {
            //B2_NOT_USED(step);

            Body bA = BodyA;
            Body bB = BodyB;

            // Cdot = dot(u, v + cross(w, r))
            Vector2 vA = bA.LinearVelocity + MathUtils.Cross(bA.AngularVelocity, m_rA);
            Vector2 vB = bB.LinearVelocity + MathUtils.Cross(bB.AngularVelocity, m_rB);
            float C = m_length - m_maxLength;
            float Cdot = Vector2.Dot(m_u, vB - vA);

            // Predictive constraint.
            if (C < 0.0f)
            {
                Cdot += step.inv_dt * C;
            }

            float impulse = -m_mass * Cdot;
            float oldImpulse = m_impulse;
            m_impulse = Math.Min(0.0f, m_impulse + impulse);
            impulse = m_impulse - oldImpulse;

            Vector2 P = impulse * m_u;
            bA.LinearVelocity -= bA.InvMass * P;
            bA.AngularVelocity -= bA.InvI * MathUtils.Cross(m_rA, P);
            bB.LinearVelocity += bB.InvMass * P;
            bB.AngularVelocity += bB.InvI * MathUtils.Cross(m_rB, P);
        }

        internal override bool SolvePositionConstraints()
        {
            //B2_NOT_USED(baumgarte);

            Body bA = BodyA;
            Body bB = BodyB;

            Transform xf1;
            bA.GetTransform(out xf1);

            Transform xf2;
            bB.GetTransform(out xf2);

            Vector2 rA = MathUtils.Multiply(ref xf1.R, LocalAnchorA - bA.LocalCenter);
            Vector2 rB = MathUtils.Multiply(ref xf2.R, LocalAnchorB - bB.LocalCenter);

            Vector2 u = bB.Sweep.c + rB - bA.Sweep.c - rA;

            u.Normalize();

            float length = u.Length();
            float C = length - m_maxLength;

            C = MathUtils.Clamp(C, 0.0f, Settings.MaxLinearCorrection);

            float impulse = -m_mass * C;
            Vector2 P = impulse * u;

            bA.Sweep.c -= bA.InvMass * P;
            bA.Sweep.a -= bA.InvI * MathUtils.Cross(rA, P);
            bB.Sweep.c += bB.InvMass * P;
            bB.Sweep.a += bB.InvI * MathUtils.Cross(rB, P);

            bA.SynchronizeTransform();
            bB.SynchronizeTransform();

            return length - m_maxLength < Settings.LinearSlop;
        }


        Vector2 LocalAnchorA;
        Vector2 LocalAnchorB;

        float m_maxLength;
        float m_length;

        // Jacobian info
        Vector2 m_u, m_rA, m_rB;

        // Effective mass
        float m_mass;

        // Impulses for accumulation/warm starting.
        float m_impulse;

        LimitState State;
    }
}
