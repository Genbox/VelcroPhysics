/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
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
using Microsoft.Xna.Framework;
using VelcroPhysics.Dynamics.Solver;
using VelcroPhysics.Shared;
using VelcroPhysics.Utilities;

namespace VelcroPhysics.Dynamics.Joints
{
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

    /// <summary>
    /// A wheel joint. This joint provides two degrees of freedom: translation
    /// along an axis fixed in bodyA and rotation in the plane. You can use a
    /// joint limit to restrict the range of motion and a joint motor to drive
    /// the rotation or to model rotational friction.
    /// This joint is designed for vehicle suspensions.
    /// </summary>
    public class WheelJoint : Joint
    {
        private Vector2 _ax, _ay;
        private Vector2 _axis;

        private float _bias;
        private bool _enableMotor;
        private float _gamma;

        private float _impulse;

        // Solver temp
        private int _indexA;

        private int _indexB;
        private float _invIA;
        private float _invIB;
        private float _invMassA;
        private float _invMassB;
        private Vector2 _localCenterA;

        private Vector2 _localCenterB;

        // Solver shared
        private Vector2 _localYAxis;

        private float _mass;

        private float _maxMotorTorque;
        private float _motorImpulse;
        private float _motorMass;
        private float _motorSpeed;
        private float _sAx, _sBx;
        private float _sAy, _sBy;
        private float _springImpulse;
        private float _springMass;

        internal WheelJoint()
        {
            JointType = JointType.Wheel;
        }

        /// <summary>
        /// Constructor for WheelJoint
        /// </summary>
        /// <param name="bodyA">The first body</param>
        /// <param name="bodyB">The second body</param>
        /// <param name="anchor">The anchor point</param>
        /// <param name="axis">The axis</param>
        /// <param name="useWorldCoordinates">Set to true if you are using world coordinates as anchors.</param>
        public WheelJoint(Body bodyA, Body bodyB, Vector2 anchor, Vector2 axis, bool useWorldCoordinates = false)
            : base(bodyA, bodyB)
        {
            JointType = JointType.Wheel;

            if (useWorldCoordinates)
            {
                LocalAnchorA = bodyA.GetLocalPoint(anchor);
                LocalAnchorB = bodyB.GetLocalPoint(anchor);
            }
            else
            {
                LocalAnchorA = bodyA.GetLocalPoint(bodyB.GetWorldPoint(anchor));
                LocalAnchorB = anchor;
            }

            Axis = axis; //Velcro only: We maintain the original value as it is supposed to.
        }

        /// <summary>
        /// The local anchor point on BodyA
        /// </summary>
        public Vector2 LocalAnchorA { get; set; }

        /// <summary>
        /// The local anchor point on BodyB
        /// </summary>
        public Vector2 LocalAnchorB { get; set; }

        public override Vector2 WorldAnchorA
        {
            get { return BodyA.GetWorldPoint(LocalAnchorA); }
            set { LocalAnchorA = BodyA.GetLocalPoint(value); }
        }

        public override Vector2 WorldAnchorB
        {
            get { return BodyB.GetWorldPoint(LocalAnchorB); }
            set { LocalAnchorB = BodyB.GetLocalPoint(value); }
        }

        /// <summary>
        /// The axis at which the suspension moves.
        /// </summary>
        public Vector2 Axis
        {
            get { return _axis; }
            set
            {
                _axis = value;
                LocalXAxis = BodyA.GetLocalVector(_axis);
                _localYAxis = MathUtils.Cross(1.0f, LocalXAxis);
            }
        }

        /// <summary>
        /// The axis in local coordinates relative to BodyA
        /// </summary>
        public Vector2 LocalXAxis { get; private set; }

        /// <summary>
        /// The desired motor speed in radians per second.
        /// </summary>
        public float MotorSpeed
        {
            get { return _motorSpeed; }
            set
            {
                if (value == _motorSpeed)
                    return;

                WakeBodies();
                _motorSpeed = value;
            }
        }

        /// <summary>
        /// The maximum motor torque, usually in N-m.
        /// </summary>
        public float MaxMotorTorque
        {
            get { return _maxMotorTorque; }
            set
            {
                if (value == _maxMotorTorque)
                    return;

                WakeBodies();
                _maxMotorTorque = value;
            }
        }

        /// <summary>
        /// Suspension frequency, zero indicates no suspension
        /// </summary>
        public float Frequency { get; set; }

        /// <summary>
        /// Suspension damping ratio, one indicates critical damping
        /// </summary>
        public float DampingRatio { get; set; }

        /// <summary>
        /// Gets the translation along the axis
        /// </summary>
        public float JointTranslation
        {
            get
            {
                Body bA = BodyA;
                Body bB = BodyB;

                Vector2 pA = bA.GetWorldPoint(LocalAnchorA);
                Vector2 pB = bB.GetWorldPoint(LocalAnchorB);
                Vector2 d = pB - pA;
                Vector2 axis = bA.GetWorldVector(LocalXAxis);

                float translation = Vector2.Dot(d, axis);
                return translation;
            }
        }

        public float JointLinearSpeed
        {
            get
            {
                Body bA = BodyA;
                Body bB = BodyB;

                Transform xfA;
                bA.GetTransform(out xfA);

                Transform xfB;
                bB.GetTransform(out xfB);

                Vector2 rA = MathUtils.Mul(xfA.q, LocalAnchorA - bA._sweep.LocalCenter);
                Vector2 rB = MathUtils.Mul(xfB.q, LocalAnchorB - bB._sweep.LocalCenter);
                Vector2 p1 = bA._sweep.C + rA;
                Vector2 p2 = bB._sweep.C + rB;
                Vector2 d = p2 - p1;
                Vector2 axis = MathUtils.Mul(xfA.q, LocalXAxis);

                Vector2 vA = bA.LinearVelocity;
                Vector2 vB = bB.LinearVelocity;
                float wA = bA.AngularVelocity;
                float wB = bB.AngularVelocity;

                float speed = Vector2.Dot(d, MathUtils.Cross(wA, axis)) + Vector2.Dot(axis, vB + MathUtils.Cross(wB, rB) - vA - MathUtils.Cross(wA, rA));
                return speed;
            }
        }

        public float JointAngle
        {
            get
            {
                Body bA = BodyA;
                Body bB = BodyB;
                return bB._sweep.A - bA._sweep.A;
            }
        }

        /// <summary>
        /// Gets the angular velocity of the joint
        /// </summary>
        public float JointAngularSpeed
        {
            get
            {
                float wA = BodyA.AngularVelocity;
                float wB = BodyB.AngularVelocity;
                return wB - wA;
            }
        }

        /// <summary>
        /// Enable/disable the joint motor.
        /// </summary>
        public bool MotorEnabled
        {
            get { return _enableMotor; }
            set
            {
                if (value == _enableMotor)
                    return;

                WakeBodies();
                _enableMotor = value;
            }
        }

        /// <summary>
        /// Gets the torque of the motor
        /// </summary>
        /// <param name="invDt">inverse delta time</param>
        public float GetMotorTorque(float invDt)
        {
            return invDt * _motorImpulse;
        }

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
            _localCenterA = BodyA._sweep.LocalCenter;
            _localCenterB = BodyB._sweep.LocalCenter;
            _invMassA = BodyA._invMass;
            _invMassB = BodyB._invMass;
            _invIA = BodyA._invI;
            _invIB = BodyB._invI;

            float mA = _invMassA, mB = _invMassB;
            float iA = _invIA, iB = _invIB;

            Vector2 cA = data.Positions[_indexA].C;
            float aA = data.Positions[_indexA].A;
            Vector2 vA = data.Velocities[_indexA].V;
            float wA = data.Velocities[_indexA].W;

            Vector2 cB = data.Positions[_indexB].C;
            float aB = data.Positions[_indexB].A;
            Vector2 vB = data.Velocities[_indexB].V;
            float wB = data.Velocities[_indexB].W;

            Rot qA = new Rot(aA), qB = new Rot(aB);

            // Compute the effective masses.
            Vector2 rA = MathUtils.Mul(qA, LocalAnchorA - _localCenterA);
            Vector2 rB = MathUtils.Mul(qB, LocalAnchorB - _localCenterB);
            Vector2 d1 = cB + rB - cA - rA;

            // Point to line constraint
            {
                _ay = MathUtils.Mul(qA, _localYAxis);
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
            if (Frequency > 0.0f)
            {
                _ax = MathUtils.Mul(qA, LocalXAxis);
                _sAx = MathUtils.Cross(d1 + rA, _ax);
                _sBx = MathUtils.Cross(rB, _ax);

                float invMass = mA + mB + iA * _sAx * _sAx + iB * _sBx * _sBx;

                if (invMass > 0.0f)
                {
                    _springMass = 1.0f / invMass;

                    float C = Vector2.Dot(d1, _ax);

                    // Frequency
                    float omega = 2.0f * Settings.Pi * Frequency;

                    // Damping coefficient
                    float d = 2.0f * _springMass * DampingRatio * omega;

                    // Spring stiffness
                    float k = _springMass * omega * omega;

                    // magic formulas
                    float h = data.Step.dt;
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
                _impulse *= data.Step.dtRatio;
                _springImpulse *= data.Step.dtRatio;
                _motorImpulse *= data.Step.dtRatio;

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

            data.Velocities[_indexA].V = vA;
            data.Velocities[_indexA].W = wA;
            data.Velocities[_indexB].V = vB;
            data.Velocities[_indexB].W = wB;
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            float mA = _invMassA, mB = _invMassB;
            float iA = _invIA, iB = _invIB;

            Vector2 vA = data.Velocities[_indexA].V;
            float wA = data.Velocities[_indexA].W;
            Vector2 vB = data.Velocities[_indexB].V;
            float wB = data.Velocities[_indexB].W;

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
                float maxImpulse = data.Step.dt * _maxMotorTorque;
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

            data.Velocities[_indexA].V = vA;
            data.Velocities[_indexA].W = wA;
            data.Velocities[_indexB].V = vB;
            data.Velocities[_indexB].W = wB;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            Vector2 cA = data.Positions[_indexA].C;
            float aA = data.Positions[_indexA].A;
            Vector2 cB = data.Positions[_indexB].C;
            float aB = data.Positions[_indexB].A;

            Rot qA = new Rot(aA), qB = new Rot(aB);

            Vector2 rA = MathUtils.Mul(qA, LocalAnchorA - _localCenterA);
            Vector2 rB = MathUtils.Mul(qB, LocalAnchorB - _localCenterB);
            Vector2 d = (cB - cA) + rB - rA;

            Vector2 ay = MathUtils.Mul(qA, _localYAxis);

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

            data.Positions[_indexA].C = cA;
            data.Positions[_indexA].A = aA;
            data.Positions[_indexB].C = cB;
            data.Positions[_indexB].A = aB;

            return Math.Abs(C) <= Settings.LinearSlop;
        }
    }
}