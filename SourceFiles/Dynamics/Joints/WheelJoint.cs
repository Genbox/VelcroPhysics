/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
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
        private Vector2 _localXAxisA;
        private Vector2 _localYAxisA;

        private float _impulse;
        private float _motorImpulse;
        private float _springImpulse;

        private float _maxMotorTorque;
        private float _motorSpeed;
        private bool _enableMotor;

        // Solver temp
        private int _indexA;
        private int _indexB;
        private Vector2 _localCenterA;
        private Vector2 _localCenterB;
        private float _invMassA;
        private float _invMassB;
        private float _invIA;
        private float _invIB;

        private Vector2 _ax, _ay;
        private float _sAx, _sBx;
        private float _sAy, _sBy;

        private float _mass;
        private float _motorMass;
        private float _springMass;

        private float _bias;
        private float _gamma;

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
            _localXAxisA = bA.GetLocalVector(axis);
            _localYAxisA = MathUtils.Cross(1.0f, _localXAxisA);
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
            get { return _motorSpeed; }
            set
            {
                WakeBodies();
                _motorSpeed = value;
            }
        }

        /// The maximum motor torque, usually in N-m.
        public float MaxMotorTorque
        {
            get { return _maxMotorTorque; }
            set
            {
                WakeBodies();
                _maxMotorTorque = value;
            }
        }

        /// Suspension frequency, zero indicates no suspension
        public float SpringFrequencyHz { get; set; }

        /// Suspension damping ratio, one indicates critical damping
        public float SpringDampingRatio { get; set; }

        public override Vector2 GetReactionForce(float invDt)
        {
            return invDt * (_impulse * _ay + _springImpulse * _ax);
        }

        public override float GetReactionTorque(float invDt)
        {
            return invDt * _motorImpulse;
        }

        internal override void InitVelocityConstraints(ref SolverData data)
        {
            _indexA = BodyA.IslandIndex;
            _indexB = BodyB.IslandIndex;
            _localCenterA = BodyA.Sweep.LocalCenter;
            _localCenterB = BodyB.Sweep.LocalCenter;
            _invMassA = BodyA.InvMass;
            _invMassB = BodyB.InvMass;
            _invIA = BodyA.InvI;
            _invIB = BodyB.InvI;

            float mA = _invMassA, mB = _invMassB;
            float iA = _invIA, iB = _invIB;

            Vector2 cA = data.positions[_indexA].c;
            float aA = data.positions[_indexA].a;
            Vector2 vA = data.velocities[_indexA].v;
            float wA = data.velocities[_indexA].w;

            Vector2 cB = data.positions[_indexB].c;
            float aB = data.positions[_indexB].a;
            Vector2 vB = data.velocities[_indexB].v;
            float wB = data.velocities[_indexB].w;

            Rot qA = new Rot(aA), qB = new Rot(aB);

            // Compute the effective masses.
            Vector2 rA = MathUtils.Mul(qA, LocalAnchorA - _localCenterA);
            Vector2 rB = MathUtils.Mul(qB, LocalAnchorB - _localCenterB);
            Vector2 d1 = cB + rB - cA - rA;

            // Point to line constraint
            {
                _ay = MathUtils.Mul(qA, _localYAxisA);
                _sAy = MathUtils.Cross(d1 + rA, _ay);
                _sBy = MathUtils.Cross(rB, _ay);

                _mass = mA + mB + iA * _sAy * _sAy + iB * _sBy * _sBy;

                if (_mass > 0.0f)
                {
                    _mass = 1.0f / _mass;
                }
            }

            // Spring constraint
            _springMass = 0.0f;
            _bias = 0.0f;
            _gamma = 0.0f;
            if (SpringFrequencyHz > 0.0f)
            {
                _ax = MathUtils.Mul(qA, _localXAxisA);
                _sAx = MathUtils.Cross(d1 + rA, _ax);
                _sBx = MathUtils.Cross(rB, _ax);

                float invMass = mA + mB + iA * _sAx * _sAx + iB * _sBx * _sBx;

                if (invMass > 0.0f)
                {
                    _springMass = 1.0f / invMass;

                    float C = Vector2.Dot(d1, _ax);

                    // Frequency
                    float omega = 2.0f * Settings.Pi * SpringFrequencyHz;

                    // Damping coefficient
                    float d = 2.0f * _springMass * SpringDampingRatio * omega;

                    // Spring stiffness
                    float k = _springMass * omega * omega;

                    // magic formulas
                    float h = data.step.dt;
                    _gamma = h * (d + h * k);
                    if (_gamma > 0.0f)
                    {
                        _gamma = 1.0f / _gamma;
                    }

                    _bias = C * h * k * _gamma;

                    _springMass = invMass + _gamma;
                    if (_springMass > 0.0f)
                    {
                        _springMass = 1.0f / _springMass;
                    }
                }
            }
            else
            {
                _springImpulse = 0.0f;
            }

            // Rotational motor
            if (_enableMotor)
            {
                _motorMass = iA + iB;
                if (_motorMass > 0.0f)
                {
                    _motorMass = 1.0f / _motorMass;
                }
            }
            else
            {
                _motorMass = 0.0f;
                _motorImpulse = 0.0f;
            }

            if (Settings.EnableWarmstarting)
            {
                // Account for variable time step.
                _impulse *= data.step.dtRatio;
                _springImpulse *= data.step.dtRatio;
                _motorImpulse *= data.step.dtRatio;

                Vector2 P = _impulse * _ay + _springImpulse * _ax;
                float LA = _impulse * _sAy + _springImpulse * _sAx + _motorImpulse;
                float LB = _impulse * _sBy + _springImpulse * _sBx + _motorImpulse;

                vA -= _invMassA * P;
                wA -= _invIA * LA;

                vB += _invMassB * P;
                wB += _invIB * LB;
            }
            else
            {
                _impulse = 0.0f;
                _springImpulse = 0.0f;
                _motorImpulse = 0.0f;
            }

            data.velocities[_indexA].v = vA;
            data.velocities[_indexA].w = wA;
            data.velocities[_indexB].v = vB;
            data.velocities[_indexB].w = wB;
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            float mA = _invMassA, mB = _invMassB;
            float iA = _invIA, iB = _invIB;

            Vector2 vA = data.velocities[_indexA].v;
            float wA = data.velocities[_indexA].w;
            Vector2 vB = data.velocities[_indexB].v;
            float wB = data.velocities[_indexB].w;

            // Solve spring constraint
            {
                float Cdot = Vector2.Dot(_ax, vB - vA) + _sBx * wB - _sAx * wA;
                float impulse = -_springMass * (Cdot + _bias + _gamma * _springImpulse);
                _springImpulse += impulse;

                Vector2 P = impulse * _ax;
                float LA = impulse * _sAx;
                float LB = impulse * _sBx;

                vA -= mA * P;
                wA -= iA * LA;

                vB += mB * P;
                wB += iB * LB;
            }

            // Solve rotational motor constraint
            {
                float Cdot = wB - wA - _motorSpeed;
                float impulse = -_motorMass * Cdot;

                float oldImpulse = _motorImpulse;
                float maxImpulse = data.step.dt * _maxMotorTorque;
                _motorImpulse = MathUtils.Clamp(_motorImpulse + impulse, -maxImpulse, maxImpulse);
                impulse = _motorImpulse - oldImpulse;

                wA -= iA * impulse;
                wB += iB * impulse;
            }

            // Solve point to line constraint
            {
                float Cdot = Vector2.Dot(_ay, vB - vA) + _sBy * wB - _sAy * wA;
                float impulse = -_mass * Cdot;
                _impulse += impulse;

                Vector2 P = impulse * _ay;
                float LA = impulse * _sAy;
                float LB = impulse * _sBy;

                vA -= mA * P;
                wA -= iA * LA;

                vB += mB * P;
                wB += iB * LB;
            }

            data.velocities[_indexA].v = vA;
            data.velocities[_indexA].w = wA;
            data.velocities[_indexB].v = vB;
            data.velocities[_indexB].w = wB;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            Vector2 cA = data.positions[_indexA].c;
            float aA = data.positions[_indexA].a;
            Vector2 cB = data.positions[_indexB].c;
            float aB = data.positions[_indexB].a;

            Rot qA = new Rot(aA), qB = new Rot(aB);

            Vector2 rA = MathUtils.Mul(qA, LocalAnchorA - _localCenterA);
            Vector2 rB = MathUtils.Mul(qB, LocalAnchorB - _localCenterB);
            Vector2 d = (cB - cA) + rB - rA;

            Vector2 ay = MathUtils.Mul(qA, _localYAxisA);

            float sAy = MathUtils.Cross(d + rA, ay);
            float sBy = MathUtils.Cross(rB, ay);

            float C = Vector2.Dot(d, ay);

            float k = _invMassA + _invMassB + _invIA * _sAy * _sAy + _invIB * _sBy * _sBy;

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

            cA -= _invMassA * P;
            aA -= _invIA * LA;
            cB += _invMassB * P;
            aB += _invIB * LB;

            data.positions[_indexA].c = cA;
            data.positions[_indexA].a = aA;
            data.positions[_indexB].c = cB;
            data.positions[_indexB].a = aB;

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
                Vector2 axis = bA.GetWorldVector(_localXAxisA);

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
            get { return _enableMotor; }
            set
            {
                WakeBodies();
                _enableMotor = value;
            }
        }

        public float GetMotorTorque(float inv_dt)
        {
            return inv_dt * _motorImpulse;
        }
    }
}