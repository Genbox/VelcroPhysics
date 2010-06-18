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

using System;
using System.Diagnostics;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
    /// A distance joint rains two points on two bodies
    /// to remain at a fixed distance from each other. You can view
    /// this as a massless, rigid rod.
    public class MaxDistanceJoint : Joint
    {
        // 1-D rained system
        // m (v2 - v1) = lambda
        // v2 + (beta/h) * x1 + gamma * lambda = 0, gamma has units of inverse mass.
        // x2 = x1 + h * v2

        // 1-D mass-damper-spring system
        // m (v2 - v1) + h * d * v2 + h * k * 

        // C = norm(p2 - p1) - L
        // u = (p2 - p1) / norm(p2 - p1)
        // Cdot = dot(u, v2 + cross(w2, r2) - v1 - cross(w1, r1))
        // J = [-u -cross(r1, u) u cross(r2, u)]
        // K = J * invM * JT
        //   = invMass1 + invI1 * cross(r1, u)^2 + invMass2 + invI2 * cross(r2, u)^2

        private float _bias;
        private float _gamma;
        private float _impulse;
        private float _mass;
        private Vector2 _u;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxDistanceJoint"/> class.
        /// </summary>
        /// <param name="bodyA">The body A.</param>
        /// <param name="bodyB">The body B.</param>
        /// <param name="anchorA">The anchor A.</param>
        /// <param name="anchorB">The anchor B.</param>
        /// <param name="maxlength">The maxlength.</param>
        /// @warning Do not use a zero or short length.
        public MaxDistanceJoint(Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB, float maxlength)
            : base(bodyA, bodyB)
        {
            JointType = JointType.MaxDistance;

            LocalAnchorA = bodyA.GetLocalPoint(anchorA);
            LocalAnchorB = bodyB.GetLocalPoint(anchorB);
            Length = maxlength;
        }

        public Vector2 LocalAnchorA { get; set; }

        public Vector2 LocalAnchorB { get; set; }

        /// <summary>
        /// The natural length between the anchor points.
        /// </summary>
        /// <value>The length.</value>
        public float Length { get; set; }

        /// <summary>
        /// The mass-spring-damper frequency in Hertz.
        /// </summary>
        /// <value>The frequency.</value>
        public float Frequency { get; set; }

        /// <summary>
        /// The damping ratio. 0 = no damping, 1 = critical damping.
        /// </summary>
        /// <value>The damping ratio.</value>
        public float DampingRatio { get; set; }

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
            Vector2 F = (inv_dt * _impulse) * _u;
            return F;
        }

        public override float GetReactionTorque(float inv_dt)
        {
            return 0.0f;
        }

        internal override void InitVelocityConstraints(ref TimeStep step)
        {
            Body b1 = BodyA;
            Body b2 = BodyB;

            Transform xf1, xf2;
            b1.GetTransform(out xf1);
            b2.GetTransform(out xf2);

            // Compute the effective mass matrix.
            Vector2 r1 = MathUtils.Multiply(ref xf1.R, LocalAnchorA - b1.LocalCenter);
            Vector2 r2 = MathUtils.Multiply(ref xf2.R, LocalAnchorB - b2.LocalCenter);
            _u = b2._sweep.Center + r2 - b1._sweep.Center - r1;

            // Handle singularity.
            float length = _u.Length();

            if (length < Length)
            {
                return;
            }

            if (length > Settings.LinearSlop)
            {
                _u *= 1.0f / length;
            }
            else
            {
                _u = new Vector2(0.0f, 0.0f);
            }

            float cr1u = MathUtils.Cross(r1, _u);
            float cr2u = MathUtils.Cross(r2, _u);
            float invMass = b1._invMass + b1._invI * cr1u * cr1u + b2._invMass + b2._invI * cr2u * cr2u;
            Debug.Assert(invMass > Settings.Epsilon);
            _mass = invMass != 0.0f ? 1.0f / invMass : 0.0f;

            if (Frequency > 0.0f)
            {
                float C = length - Length;

                // Frequency
                float omega = 2.0f * Settings.Pi * Frequency;

                // Damping coefficient
                float d = 2.0f * _mass * DampingRatio * omega;

                // Spring stiffness
                float k = _mass * omega * omega;

                // magic formulas
                _gamma = step.DeltaTime * (d + step.DeltaTime * k);
                _gamma = _gamma != 0.0f ? 1.0f / _gamma : 0.0f;
                _bias = C * step.DeltaTime * k * _gamma;

                _mass = invMass + _gamma;
                _mass = _mass != 0.0f ? 1.0f / _mass : 0.0f;
            }

            if (Settings.EnableWarmstarting)
            {
                // Scale the impulse to support a variable time step.
                _impulse *= step.DtRatio;

                Vector2 P = _impulse * _u;
                b1._linearVelocity -= b1._invMass * P;
                b1._angularVelocity -= b1._invI * MathUtils.Cross(r1, P);
                b2._linearVelocity += b2._invMass * P;
                b2._angularVelocity += b2._invI * MathUtils.Cross(r2, P);
            }
            else
            {
                _impulse = 0.0f;
            }
        }

        internal override void SolveVelocityConstraints(ref TimeStep step)
        {
            Body b1 = BodyA;
            Body b2 = BodyB;

            Transform xf1, xf2;
            b1.GetTransform(out xf1);
            b2.GetTransform(out xf2);

            Vector2 r1 = MathUtils.Multiply(ref xf1.R, LocalAnchorA - b1.LocalCenter);
            Vector2 r2 = MathUtils.Multiply(ref xf2.R, LocalAnchorB - b2.LocalCenter);

            Vector2 d = b2._sweep.Center + r2 - b1._sweep.Center - r1;

            float length = d.Length();

            if (length < Length)
            {
                return;
            }

            // Cdot = dot(u, v + cross(w, r))
            Vector2 v1 = b1._linearVelocity + MathUtils.Cross(b1._angularVelocity, r1);
            Vector2 v2 = b2._linearVelocity + MathUtils.Cross(b2._angularVelocity, r2);
            float Cdot = Vector2.Dot(_u, v2 - v1);

            float impulse = -_mass * (Cdot + _bias + _gamma * _impulse);
            _impulse += impulse;

            Vector2 P = impulse * _u;
            b1._linearVelocity -= b1._invMass * P;
            b1._angularVelocity -= b1._invI * MathUtils.Cross(r1, P);
            b2._linearVelocity += b2._invMass * P;
            b2._angularVelocity += b2._invI * MathUtils.Cross(r2, P);
        }

        internal override bool SolvePositionConstraints()
        {
            if (Frequency > 0.0f)
            {
                // There is no position correction for soft distance constraints.
                return true;
            }

            Body b1 = BodyA;
            Body b2 = BodyB;

            Transform xf1, xf2;
            b1.GetTransform(out xf1);
            b2.GetTransform(out xf2);

            Vector2 r1 = MathUtils.Multiply(ref xf1.R, LocalAnchorA - b1.LocalCenter);
            Vector2 r2 = MathUtils.Multiply(ref xf2.R, LocalAnchorB - b2.LocalCenter);

            Vector2 d = b2._sweep.Center + r2 - b1._sweep.Center - r1;

            float length = d.Length();

            if (length < Length)
            {
                return true;
            }

            if (length == 0.0f)
                return true;

            d /= length;
            float C = length - Length;
            C = MathUtils.Clamp(C, -Settings.MaxLinearCorrection, Settings.MaxLinearCorrection);

            float impulse = -_mass * C;
            _u = d;
            Vector2 P = impulse * _u;

            b1._sweep.Center -= b1._invMass * P;
            b1._sweep.Angle -= b1._invI * MathUtils.Cross(r1, P);
            b2._sweep.Center += b2._invMass * P;
            b2._sweep.Angle += b2._invI * MathUtils.Cross(r2, P);

            b1.SynchronizeTransform();
            b2.SynchronizeTransform();

            return Math.Abs(C) < Settings.LinearSlop;
        }
    }
}