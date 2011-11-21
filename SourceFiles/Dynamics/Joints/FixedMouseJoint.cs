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
    /// <summary>
    /// A mouse joint is used to make a point on a body track a
    /// specified world point. This a soft constraint with a maximum
    /// force. This allows the constraint to stretch and without
    /// applying huge forces.
    /// NOTE: this joint is not documented in the manual because it was
    /// developed to be used in the testbed. If you want to learn how to
    /// use the mouse joint, look at the testbed.
    /// </summary>
    public class FixedMouseJoint : Joint
    {
        public Vector2 LocalAnchorB;
        private Vector2 _targetA;
        private float _beta;
        private float _gamma;

        // Solver shared
        private Vector2 m_impulse;
        private float m_maxForce;
        private float m_gamma;

        // Solver temp
        private int m_indexA;
        private int m_indexB;
        private Vector2 m_rB;
        private Vector2 m_localCenterB;
        private float m_invMassB;
        private float m_invIB;
        private Mat22 m_mass;
        private Vector2 m_C;

        /// <summary>
        /// This requires a world target point,
        /// tuning parameters, and the time step.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="worldAnchor">The target.</param>
        public FixedMouseJoint(Body body, Vector2 worldAnchor)
            : base(body)
        {
            JointType = JointType.FixedMouse;
            Frequency = 5.0f;
            DampingRatio = 0.7f;

            Debug.Assert(worldAnchor.IsValid());

            Transform xf1;
            BodyA.GetTransform(out xf1);

            _targetA = worldAnchor;
            LocalAnchorB = BodyA.GetLocalPoint(worldAnchor);
        }

        public override Vector2 WorldAnchorA
        {
            get { return BodyA.GetWorldPoint(LocalAnchorB); }
        }

        public override Vector2 WorldAnchorB
        {
            get { return _targetA; }
            set
            {
                BodyA.Awake = true;
                _targetA = value;
            }
        }

        /// <summary>
        /// The maximum constraint force that can be exerted
        /// to move the candidate body. Usually you will express
        /// as some multiple of the weight (multiplier * mass * gravity).
        /// </summary>
        public float MaxForce { get; set; }

        /// <summary>
        /// The response speed.
        /// </summary>
        public float Frequency { get; set; }

        /// <summary>
        /// The damping ratio. 0 = no damping, 1 = critical damping.
        /// </summary>
        public float DampingRatio { get; set; }

        public override Vector2 GetReactionForce(float inv_dt)
        {
            return inv_dt * m_impulse;
        }

        public override float GetReactionTorque(float inv_dt)
        {
            return inv_dt * 0.0f;
        }

        internal override void InitVelocityConstraints(ref SolverData data)
        {
            m_indexB = BodyB.IslandIndex;
            m_localCenterB = BodyB.Sweep.LocalCenter;
            m_invMassB = BodyB.InvMass;
            m_invIB = BodyB.InvI;

            Vector2 cB = data.positions[m_indexB].c;
            float aB = data.positions[m_indexB].a;
            Vector2 vB = data.velocities[m_indexB].v;
            float wB = data.velocities[m_indexB].w;

            Rot qB = new Rot(aB);

            float mass = BodyB.Mass;

            // Frequency
            float omega = 2.0f * Settings.Pi * Frequency;

            // Damping coefficient
            float d = 2.0f * mass * DampingRatio * omega;

            // Spring stiffness
            float k = mass * (omega * omega);

            // magic formulas
            // gamma has units of inverse mass.
            // beta has units of inverse time.
            float h = data.step.dt;
            Debug.Assert(d + h * k > Settings.Epsilon);
            m_gamma = h * (d + h * k);
            if (_gamma != 0.0f)
            {
                _gamma = 1.0f / _gamma;
            }

            _beta = h * k * m_gamma;

            // Compute the effective mass matrix.
            m_rB = MathUtils.Mul(qB, LocalAnchorB - m_localCenterB);
            // K    = [(1/m1 + 1/m2) * eye(2) - skew(r1) * invI1 * skew(r1) - skew(r2) * invI2 * skew(r2)]
            //      = [1/m1+1/m2     0    ] + invI1 * [r1.Y*r1.Y -r1.X*r1.Y] + invI2 * [r1.Y*r1.Y -r1.X*r1.Y]
            //        [    0     1/m1+1/m2]           [-r1.X*r1.Y r1.X*r1.X]           [-r1.X*r1.Y r1.X*r1.X]
            Mat22 K = new Mat22();
            K.ex.X = m_invMassB + m_invIB * m_rB.Y * m_rB.Y + m_gamma;
            K.ex.Y = -m_invIB * m_rB.X * m_rB.Y;
            K.ey.X = K.ex.Y;
            K.ey.Y = m_invMassB + m_invIB * m_rB.X * m_rB.X + m_gamma;

            m_mass = K.Inverse;

            m_C = cB + m_rB - _targetA;
            m_C *= _beta;

            // Cheat with some damping
            wB *= 0.98f;

            if (Settings.EnableWarmstarting)
            {
                m_impulse *= data.step.dtRatio;
                vB += m_invMassB * m_impulse;
                wB += m_invIB * MathUtils.Cross(m_rB, m_impulse);
            }
            else
            {
                m_impulse = Vector2.Zero;
            }

            data.velocities[m_indexB].v = vB;
            data.velocities[m_indexB].w = wB;
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            Vector2 vB = data.velocities[m_indexB].v;
            float wB = data.velocities[m_indexB].w;

            // Cdot = v + cross(w, r)
            Vector2 Cdot = vB + MathUtils.Cross(wB, m_rB);
            Vector2 impulse = MathUtils.Mul(ref m_mass, -(Cdot + _beta * m_C + _gamma * m_impulse));

            Vector2 oldImpulse = m_impulse;
            m_impulse += impulse;
            float maxImpulse = data.step.dt * MaxForce;
            if (m_impulse.LengthSquared() > maxImpulse * maxImpulse)
            {
                m_impulse *= maxImpulse / m_impulse.Length();
            }
            impulse = m_impulse - oldImpulse;

            vB += m_invMassB * impulse;
            wB += m_invIB * MathUtils.Cross(m_rB, impulse);

            data.velocities[m_indexB].v = vB;
            data.velocities[m_indexB].w = wB;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            return true;
        }
    }
}