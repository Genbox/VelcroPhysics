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

using System.Diagnostics;
using Genbox.VelcroPhysics.Definitions.Joints;
using Genbox.VelcroPhysics.Dynamics.Joints.Misc;
using Genbox.VelcroPhysics.Dynamics.Solver;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Dynamics.Joints
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
    /// A wheel joint. This joint provides two degrees of freedom: translation along an axis fixed in bodyA and
    /// rotation in the plane. In other words, it is a point to line constraint with a rotational motor and a linear
    /// spring/damper. The spring/damper is initialized upon creation. This joint is designed for vehicle suspensions.
    /// </summary>
    public class WheelJoint : Joint
    {
        private Vector2 _localAnchorA;
        private Vector2 _localAnchorB;
        private Vector2 _localXAxisA;
        private Vector2 _localYAxisA;

        private float _impulse;
        private float _motorImpulse;
        private float _springImpulse;

        private float _lowerImpulse;
        private float _upperImpulse;
        private float _translation;
        private float _lowerTranslation;
        private float _upperTranslation;

        private float _maxMotorTorque;
        private float _motorSpeed;

        private bool _enableLimit;
        private bool _enableMotor;

        private float _stiffness;
        private float _damping;

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
        private float _axialMass;
        private float _springMass;

        private float _bias;
        private float _gamma;

        /// <summary>Constructor for WheelJoint</summary>
        /// <param name="bodyA">The first body</param>
        /// <param name="bodyB">The second body</param>
        /// <param name="anchor">The anchor point</param>
        /// <param name="axis">The axis</param>
        /// <param name="useWorldCoordinates">Set to true if you are using world coordinates as anchors.</param>
        public WheelJoint(Body bodyA, Body bodyB, Vector2 anchor, Vector2 axis, bool useWorldCoordinates = false)
            : base(bodyA, bodyB, JointType.Wheel)
        {
            if (useWorldCoordinates)
            {
                _localAnchorA = bodyA.GetLocalPoint(anchor);
                _localAnchorB = bodyB.GetLocalPoint(anchor);
                _localXAxisA = bodyA.GetLocalVector(axis);
            }
            else
            {
                _localAnchorA = bodyA.GetLocalPoint(bodyB.GetWorldPoint(anchor));
                _localAnchorB = anchor;
                _localXAxisA = bodyA.GetLocalVector(axis);
            }

            _localYAxisA = MathUtils.Cross(1.0f, _localXAxisA);
        }

        public WheelJoint(WheelJointDef def) : base(def)
        {
            _localAnchorA = def.LocalAnchorA;
            _localAnchorB = def.LocalAnchorB;
            _localXAxisA = def.LocalAxisA;
            _localYAxisA = MathUtils.Cross(1.0f, _localXAxisA);

            _lowerTranslation = def.LowerTranslation;
            _upperTranslation = def.UpperTranslation;
            _enableLimit = def.EnableLimit;

            _maxMotorTorque = def.MaxMotorTorque;
            _motorSpeed = def.MotorSpeed;
            _enableMotor = def.EnableMotor;

            _stiffness = def.Stiffness;
            _damping = def.Damping;
        }

        public Vector2 LocalXAxisA => _localXAxisA;
        public Vector2 LocalYAxisA => _localYAxisA;

        /// <summary>The local anchor point on BodyA</summary>
        public Vector2 LocalAnchorA
        {
            get => _localAnchorA;
            set => _localAnchorA = value;
        }

        /// <summary>The local anchor point on BodyB</summary>
        public Vector2 LocalAnchorB
        {
            get => _localAnchorB;
            set => _localAnchorB = value;
        }

        public override Vector2 WorldAnchorA
        {
            get => _bodyA.GetWorldPoint(_localAnchorA);
            set => _localAnchorA = _bodyA.GetLocalPoint(value);
        }

        public override Vector2 WorldAnchorB
        {
            get => _bodyB.GetWorldPoint(_localAnchorB);
            set => _localAnchorB = _bodyB.GetLocalPoint(value);
        }

        /// <summary>The axis in local coordinates relative to BodyA</summary>
        public Vector2 LocalXAxis { get; private set; }

        /// <summary>The desired motor speed in radians per second.</summary>
        public float MotorSpeed
        {
            get => _motorSpeed;
            set
            {
                if (value != _motorSpeed)
                {
                    WakeBodies();
                    _motorSpeed = value;
                }
            }
        }

        /// <summary>The maximum motor torque, usually in N-m.</summary>
        public float MaxMotorTorque
        {
            get => _maxMotorTorque;
            set
            {
                if (value != _maxMotorTorque)
                {
                    WakeBodies();
                    _maxMotorTorque = value;
                }
            }
        }

        /// <summary>Gets the translation along the axis</summary>
        public float JointTranslation
        {
            get
            {
                Body bA = _bodyA;
                Body bB = _bodyB;

                Vector2 pA = bA.GetWorldPoint(_localAnchorA);
                Vector2 pB = bB.GetWorldPoint(_localAnchorB);
                Vector2 d = pB - pA;
                Vector2 axis = bA.GetWorldVector(_localXAxisA);

                float translation = Vector2.Dot(d, axis);
                return translation;
            }
        }

        public float JointLinearSpeed
        {
            get
            {
                Body bA = _bodyA;
                Body bB = _bodyB;

                Vector2 rA = MathUtils.Mul(bA._xf.q, _localAnchorA - bA._sweep.LocalCenter);
                Vector2 rB = MathUtils.Mul(bB._xf.q, _localAnchorB - bB._sweep.LocalCenter);
                Vector2 p1 = bA._sweep.C + rA;
                Vector2 p2 = bB._sweep.C + rB;
                Vector2 d = p2 - p1;
                Vector2 axis = MathUtils.Mul(bA._xf.q, _localXAxisA);

                Vector2 vA = bA._linearVelocity;
                Vector2 vB = bB._linearVelocity;
                float wA = bA._angularVelocity;
                float wB = bB._angularVelocity;

                float speed = MathUtils.Dot(d, MathUtils.Cross(wA, axis)) + MathUtils.Dot(axis, vB + MathUtils.Cross(wB, rB) - vA - MathUtils.Cross(wA, rA));
                return speed;
            }
        }

        public float JointAngle
        {
            get
            {
                Body bA = _bodyA;
                Body bB = _bodyB;
                return bB._sweep.A - bA._sweep.A;
            }
        }

        /// <summary>Gets the angular velocity of the joint</summary>
        public float JointAngularSpeed
        {
            get
            {
                float wA = _bodyA.AngularVelocity;
                float wB = _bodyB.AngularVelocity;
                return wB - wA;
            }
        }

        /// <summary>Enable/disable the joint motor.</summary>
        public bool MotorEnabled
        {
            get => _enableMotor;
            set
            {
                if (value != _enableMotor)
                {
                    WakeBodies();
                    _enableMotor = value;
                }
            }
        }

        public float UpperLimit
        {
            get => _upperTranslation;
            set
            {
                if (_upperTranslation != value)
                {
                    WakeBodies();
                    _upperTranslation = value;
                    _upperImpulse = 0.0f;
                }
            }
        }

        public float LowerLimit
        {
            get => _lowerTranslation;
            set
            {
                if (_lowerTranslation != value)
                {
                    WakeBodies();
                    _lowerTranslation = value;
                    _lowerImpulse = 0.0f;
                }
            }
        }

        public bool EnableLimit
        {
            get => _enableLimit;
            set
            {
                if (_enableLimit != value)
                {
                    WakeBodies();
                    _enableLimit = value;
                    _lowerImpulse = 0.0f;
                    _upperImpulse = 0.0f;
                }
            }
        }

        public float Damping
        {
            get => _damping;
            set => _damping = value;
        }

        public float Stiffness
        {
            get => _stiffness;
            set => _stiffness = value;
        }

        public void SetLimits(float lower, float upper)
        {
            Debug.Assert(lower <= upper);
            if (lower != _lowerTranslation || upper != _upperTranslation)
            {
                WakeBodies();
                _lowerTranslation = lower;
                _upperTranslation = upper;
                _lowerImpulse = 0.0f;
                _upperImpulse = 0.0f;
            }
        }

        /// <summary>Gets the torque of the motor</summary>
        /// <param name="invDt">inverse delta time</param>
        public float GetMotorTorque(float invDt)
        {
            return invDt * _motorImpulse;
        }

        public override Vector2 GetReactionForce(float invDt)
        {
            return invDt * (_impulse * _ay + (_springImpulse + _lowerImpulse - _upperImpulse) * _ax);
        }

        public override float GetReactionTorque(float invDt)
        {
            return invDt * _motorImpulse;
        }

        internal override void InitVelocityConstraints(ref SolverData data)
        {
            _indexA = _bodyA.IslandIndex;
            _indexB = _bodyB.IslandIndex;
            _localCenterA = _bodyA._sweep.LocalCenter;
            _localCenterB = _bodyB._sweep.LocalCenter;
            _invMassA = _bodyA._invMass;
            _invMassB = _bodyB._invMass;
            _invIA = _bodyA._invI;
            _invIB = _bodyB._invI;

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
            Vector2 rA = MathUtils.Mul(qA, _localAnchorA - _localCenterA);
            Vector2 rB = MathUtils.Mul(qB, _localAnchorB - _localCenterB);
            Vector2 d = cB + rB - cA - rA;

            // Point to line constraint
            {
                _ay = MathUtils.Mul(qA, _localYAxisA);
                _sAy = MathUtils.Cross(d + rA, _ay);
                _sBy = MathUtils.Cross(rB, _ay);

                _mass = mA + mB + iA * _sAy * _sAy + iB * _sBy * _sBy;

                if (_mass > 0.0f)
                {
                    _mass = 1.0f / _mass;
                }
            }

            // Spring constraint
            _ax = MathUtils.Mul(qA, _localXAxisA);
            _sAx = MathUtils.Cross(d + rA, _ax);
            _sBx = MathUtils.Cross(rB, _ax);

            float invMass = mA + mB + iA * _sAx * _sAx + iB * _sBx * _sBx;
            if (invMass > 0.0f)
            {
                _axialMass = 1.0f / invMass;
            }
            else
            {
                _axialMass = 0.0f;
            }

            _springMass = 0.0f;
            _bias = 0.0f;
            _gamma = 0.0f;

            if (_stiffness > 0.0f && invMass > 0.0f)
            {
                _springMass = 1.0f / invMass;

                float C = MathUtils.Dot(d, _ax);

                // magic formulas
                float h = data.Step.DeltaTime;
                _gamma = h * (_damping + h * _stiffness);
                if (_gamma > 0.0f)
                {
                    _gamma = 1.0f / _gamma;
                }

                _bias = C * h * _stiffness * _gamma;

                _springMass = invMass + _gamma;
                if (_springMass > 0.0f)
                {
                    _springMass = 1.0f / _springMass;
                }
            }
            else
            {
                _springImpulse = 0.0f;
            }

            if (_enableLimit)
            {
                _translation = MathUtils.Dot(_ax, d);
            }
            else
            {
                _lowerImpulse = 0.0f;
                _upperImpulse = 0.0f;
            }

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

            if (data.Step.WarmStarting)
            {
                // Account for variable time step.
                _impulse *= data.Step.DeltaTimeRatio;
                _springImpulse *= data.Step.DeltaTimeRatio;
                _motorImpulse *= data.Step.DeltaTimeRatio;

                float axialImpulse = _springImpulse + _lowerImpulse - _upperImpulse;
                Vector2 P = _impulse * _ay + axialImpulse * _ax;
                float LA = _impulse * _sAy + axialImpulse * _sAx + _motorImpulse;
                float LB = _impulse * _sBy + axialImpulse * _sBx + _motorImpulse;

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
                _lowerImpulse = 0.0f;
                _upperImpulse = 0.0f;
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
                float Cdot = MathUtils.Dot(_ax, vB - vA) + _sBx * wB - _sAx * wA;
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
                float maxImpulse = data.Step.DeltaTime * _maxMotorTorque;
                _motorImpulse = MathUtils.Clamp(_motorImpulse + impulse, -maxImpulse, maxImpulse);
                impulse = _motorImpulse - oldImpulse;

                wA -= iA * impulse;
                wB += iB * impulse;
            }

            if (_enableLimit)
            {
                // Lower limit
                {
                    float C = _translation - _lowerTranslation;
                    float Cdot = MathUtils.Dot(_ax, vB - vA) + _sBx * wB - _sAx * wA;
                    float impulse = -_axialMass * (Cdot + MathUtils.Max(C, 0.0f) * data.Step.InvertedDeltaTime);
                    float oldImpulse = _lowerImpulse;
                    _lowerImpulse = MathUtils.Max(_lowerImpulse + impulse, 0.0f);
                    impulse = _lowerImpulse - oldImpulse;

                    Vector2 P = impulse * _ax;
                    float LA = impulse * _sAx;
                    float LB = impulse * _sBx;

                    vA -= mA * P;
                    wA -= iA * LA;
                    vB += mB * P;
                    wB += iB * LB;
                }

                // Upper limit
                // Note: signs are flipped to keep C positive when the constraint is satisfied.
                // This also keeps the impulse positive when the limit is active.
                {
                    float C = _upperTranslation - _translation;
                    float Cdot = MathUtils.Dot(_ax, vA - vB) + _sAx * wA - _sBx * wB;
                    float impulse = -_axialMass * (Cdot + MathUtils.Max(C, 0.0f) * data.Step.InvertedDeltaTime);
                    float oldImpulse = _upperImpulse;
                    _upperImpulse = MathUtils.Max(_upperImpulse + impulse, 0.0f);
                    impulse = _upperImpulse - oldImpulse;

                    Vector2 P = impulse * _ax;
                    float LA = impulse * _sAx;
                    float LB = impulse * _sBx;

                    vA += mA * P;
                    wA += iA * LA;
                    vB -= mB * P;
                    wB -= iB * LB;
                }
            }

            // Solve point to line constraint
            {
                float Cdot = MathUtils.Dot(_ay, vB - vA) + _sBy * wB - _sAy * wA;
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

            float linearError = 0.0f;

            if (_enableLimit)
            {
                Rot qA = new Rot(aA), qB = new Rot(aB);

                Vector2 rA = MathUtils.Mul(qA, _localAnchorA - _localCenterA);
                Vector2 rB = MathUtils.Mul(qB, _localAnchorB - _localCenterB);
                Vector2 d = (cB - cA) + rB - rA;

                Vector2 ax = MathUtils.Mul(qA, _localXAxisA);
                float sAx = MathUtils.Cross(d + rA, _ax);
                float sBx = MathUtils.Cross(rB, _ax);

                float C = 0.0f;
                float translation = MathUtils.Dot(ax, d);
                if (MathUtils.Abs(_upperTranslation - _lowerTranslation) < 2.0f * Settings.LinearSlop)
                {
                    C = translation;
                }
                else if (translation <= _lowerTranslation)
                {
                    C = MathUtils.Min(translation - _lowerTranslation, 0.0f);
                }
                else if (translation >= _upperTranslation)
                {
                    C = MathUtils.Max(translation - _upperTranslation, 0.0f);
                }

                if (C != 0.0f)
                {

                    float invMass = _invMassA + _invMassB + _invIA * sAx * sAx + _invIB * sBx * sBx;
                    float impulse = 0.0f;
                    if (invMass != 0.0f)
                    {
                        impulse = -C / invMass;
                    }

                    Vector2 P = impulse * ax;
                    float LA = impulse * sAx;
                    float LB = impulse * sBx;

                    cA -= _invMassA * P;
                    aA -= _invIA * LA;
                    cB += _invMassB * P;
                    aB += _invIB * LB;

                    linearError = MathUtils.Abs(C);
                }
            }

            // Solve perpendicular constraint
            {
                Rot qA = new Rot(aA), qB = new Rot(aB);

                Vector2 rA = MathUtils.Mul(qA, _localAnchorA - _localCenterA);
                Vector2 rB = MathUtils.Mul(qB, _localAnchorB - _localCenterB);
                Vector2 d = (cB - cA) + rB - rA;

                Vector2 ay = MathUtils.Mul(qA, _localYAxisA);

                float sAy = MathUtils.Cross(d + rA, ay);
                float sBy = MathUtils.Cross(rB, ay);

                float C = MathUtils.Dot(d, ay);

                float invMass = _invMassA + _invMassB + _invIA * _sAy * _sAy + _invIB * _sBy * _sBy;

                float impulse = 0.0f;
                if (invMass != 0.0f)
                {
                    impulse = -C / invMass;
                }

                Vector2 P = impulse * ay;
                float LA = impulse * sAy;
                float LB = impulse * sBy;

                cA -= _invMassA * P;
                aA -= _invIA * LA;
                cB += _invMassB * P;
                aB += _invIB * LB;

                linearError = MathUtils.Max(linearError, MathUtils.Abs(C));
            }

            data.Positions[_indexA].C = cA;
            data.Positions[_indexA].A = aA;
            data.Positions[_indexB].C = cB;
            data.Positions[_indexB].A = aB;

            return linearError <= Settings.LinearSlop;
        }
    }
}