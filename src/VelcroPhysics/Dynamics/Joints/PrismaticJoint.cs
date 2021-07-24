/*
* Velcro Physics:
* Copyright (c) 2021 Ian Qvist
* 
* MIT License
*
* Copyright (c) 2019 Erin Catto
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
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
    // d = p2 - p1 = x2 + r2 - x1 - r1
    // C = dot(perp, d)
    // Cdot = dot(d, cross(w1, perp)) + dot(perp, v2 + cross(w2, r2) - v1 - cross(w1, r1))
    //      = -dot(perp, v1) - dot(cross(d + r1, perp), w1) + dot(perp, v2) + dot(cross(r2, perp), v2)
    // J = [-perp, -cross(d + r1, perp), perp, cross(r2,perp)]
    //
    // Angular constraint
    // C = a2 - a1 + a_initial
    // Cdot = w2 - w1
    // J = [0 0 -1 0 0 1]
    //
    // K = J * invM * JT
    //
    // J = [-a -s1 a s2]
    //     [0  -1  0  1]
    // a = perp
    // s1 = cross(d + r1, a) = cross(p2 - x1, a)
    // s2 = cross(r2, a) = cross(p2 - x2, a)

    // Motor/Limit linear constraint
    // C = dot(ax1, d)
    // Cdot = -dot(ax1, v1) - dot(cross(d + r1, ax1), w1) + dot(ax1, v2) + dot(cross(r2, ax1), v2)
    // J = [-ax1 -cross(d+r1,ax1) ax1 cross(r2,ax1)]

    // Predictive limit is applied even when the limit is not active.
    // Prevents a constraint speed that can lead to a constraint error in one time step.
    // Want C2 = C1 + h * Cdot >= 0
    // Or:
    // Cdot + C1/h >= 0
    // I do not apply a negative constraint error because that is handled in position correction.
    // So:
    // Cdot + max(C1, 0)/h >= 0

    // Block Solver
    // We develop a block solver that includes the angular and linear constraints. This makes the limit stiffer.
    //
    // The Jacobian has 2 rows:
    // J = [-uT -s1 uT s2] // linear
    //     [0   -1   0  1] // angular
    //
    // u = perp
    // s1 = cross(d + r1, u), s2 = cross(r2, u)
    // a1 = cross(d + r1, v), a2 = cross(r2, v)

    /// <summary>
    /// A prismatic joint. This joint provides one degree of freedom: translation along an axis fixed in bodyA.
    /// Relative rotation is prevented. You can use a joint limit to restrict the range of motion and a joint motor to drive
    /// the motion or to model joint friction.
    /// </summary>
    public class PrismaticJoint : Joint
    {
        internal Vector2 _localAnchorA;
        internal Vector2 _localAnchorB;
        internal Vector2 _localXAxisA;
        private Vector2 _localYAxisA;
        internal float _referenceAngle;
        private Vector2 _impulse;
        private float _motorImpulse;
        private float _lowerImpulse;
        private float _upperImpulse;
        private float _lowerTranslation;
        private float _upperTranslation;
        private float _maxMotorForce;
        private float _motorSpeed;
        private bool _enableLimit;
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
        private Vector2 _axis, _perp;
        private float _s1, _s2;
        private float _a1, _a2;
        private Mat22 _K;
        private float _translation;
        private float _axialMass;

        /// <summary>
        /// This requires defining a line of motion using an axis and an anchor point. The definition uses local anchor
        /// points and a local axis so that the initial configuration can violate the constraint slightly. The joint translation is
        /// zero when the local anchor points coincide in world space. Using local anchors and a local axis helps when saving and
        /// loading a game.
        /// </summary>
        /// <param name="bodyA">The first body.</param>
        /// <param name="bodyB">The second body.</param>
        /// <param name="anchorA">The first body anchor.</param>
        /// <param name="anchorB">The second body anchor.</param>
        /// <param name="axis">The axis.</param>
        /// <param name="useWorldCoordinates">Set to true if you are using world coordinates as anchors.</param>
        public PrismaticJoint(Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB, Vector2 axis, bool useWorldCoordinates = false)
            : base(bodyA, bodyB, JointType.Prismatic)
        {
            Initialize(anchorA, anchorB, axis, useWorldCoordinates);
        }

        public PrismaticJoint(Body bodyA, Body bodyB, Vector2 anchor, Vector2 axis, bool useWorldCoordinates = false)
            : base(bodyA, bodyB, JointType.Prismatic)
        {
            Initialize(anchor, anchor, axis, useWorldCoordinates);
        }

        public PrismaticJoint(PrismaticJointDef def)
            : base(def)
        {
            _localAnchorA = def.LocalAnchorA;
            _localAnchorB = def.LocalAnchorB;
            _localXAxisA = def.LocalAxisA;

            _localXAxisA.Normalize();
            _localYAxisA = MathUtils.Cross(1.0f, _localXAxisA);
            _referenceAngle = def.ReferenceAngle;


            _lowerTranslation = def.LowerTranslation;
            _upperTranslation = def.UpperTranslation;

            Debug.Assert(_lowerTranslation <= _upperTranslation);

            _maxMotorForce = def.MaxMotorForce;
            _motorSpeed = def.MotorSpeed;
            _enableLimit = def.EnableLimit;
            _enableMotor = def.EnableMotor;
        }

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

        /// <summary>Get the current joint translation, usually in meters.</summary>
        public float JointTranslation
        {
            get
            {
                Vector2 pA = _bodyA.GetWorldPoint(_localAnchorA);
                Vector2 pB = _bodyB.GetWorldPoint(_localAnchorB);
                Vector2 d = pB - pA;
                Vector2 axis = _bodyA.GetWorldVector(_localXAxisA);

                float translation = MathUtils.Dot(d, axis);
                return translation;
            }
        }

        /// <summary>Get the current joint translation speed, usually in meters per second.</summary>
        public float JointSpeed
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

        /// <summary>Is the joint limit enabled?</summary>
        /// <value><c>true</c> if [limit enabled]; otherwise, <c>false</c>.</value>
        public bool LimitEnabled
        {
            get => _enableLimit;
            set
            {
                if (value != _enableLimit)
                {
                    WakeBodies();
                    _enableLimit = value;
                    _lowerImpulse = 0.0f;
                    _upperImpulse = 0.0f;
                }
            }
        }

        /// <summary>Get the lower joint limit, usually in meters.</summary>
        public float LowerLimit
        {
            get => _lowerTranslation;
            set
            {
                if (value != _lowerTranslation)
                {
                    WakeBodies();
                    _lowerTranslation = value;
                    _lowerImpulse = 0.0f;
                }
            }
        }

        /// <summary>Get the upper joint limit, usually in meters.</summary>
        public float UpperLimit
        {
            get => _upperTranslation;
            set
            {
                if (value != _upperTranslation)
                {
                    WakeBodies();
                    _upperTranslation = value;
                    _upperImpulse = 0.0f;
                }
            }
        }

        /// <summary>Is the joint motor enabled?</summary>
        /// <value><c>true</c> if [motor enabled]; otherwise, <c>false</c>.</value>
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

        /// <summary>Set the motor speed, usually in meters per second.</summary>
        /// <value>The speed.</value>
        public float MotorSpeed
        {
            set
            {
                if (value != _motorSpeed)
                {
                    WakeBodies();
                    _motorSpeed = value;
                }
            }
            get => _motorSpeed;
        }

        /// <summary>Set the maximum motor force, usually in N.</summary>
        /// <value>The force.</value>
        public float MaxMotorForce
        {
            get => _maxMotorForce;
            set
            {
                if (value != _maxMotorForce)
                {
                    WakeBodies();
                    _maxMotorForce = value;
                }
            }
        }

        /// <summary>The local joint axis relative to bodyA.</summary>
        public Vector2 LocalXAxisA => _localXAxisA;

        public Vector2 LocalYAxisA => _localYAxisA;

        /// <summary>Get the reference angle.</summary>
        public float ReferenceAngle => _referenceAngle;

        /// <summary>Get the current motor force given the inverse time step, usually in N.</summary>
        public float GetMotorForce(float invDt)
        {
            return invDt * _motorImpulse;
        }

        /// <summary>Set the joint limits, usually in meters.</summary>
        /// <param name="lower">The lower limit</param>
        /// <param name="upper">The upper limit</param>
        public void SetLimits(float lower, float upper)
        {
            if (upper != _upperTranslation || lower != _lowerTranslation)
            {
                WakeBodies();
                _upperTranslation = upper;
                _lowerTranslation = lower;
                _lowerImpulse = 0.0f;
                _upperImpulse = 0.0f;
            }
        }

        public override Vector2 GetReactionForce(float invDt)
        {
            return invDt * (_impulse.X * _perp + (_motorImpulse + _lowerImpulse - _upperImpulse) * _axis);
        }

        public override float GetReactionTorque(float invDt)
        {
            return invDt * _impulse.Y;
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
            Vector2 d = (cB - cA) + rB - rA;

            float mA = _invMassA, mB = _invMassB;
            float iA = _invIA, iB = _invIB;

            // Compute motor Jacobian and effective mass.
            {
                _axis = MathUtils.Mul(qA, _localXAxisA);
                _a1 = MathUtils.Cross(d + rA, _axis);
                _a2 = MathUtils.Cross(rB, _axis);

                _axialMass = mA + mB + iA * _a1 * _a1 + iB * _a2 * _a2;
                if (_axialMass > 0.0f)
                    _axialMass = 1.0f / _axialMass;
            }

            // Prismatic constraint.
            {
                _perp = MathUtils.Mul(qA, _localYAxisA);

                _s1 = MathUtils.Cross(d + rA, _perp);
                _s2 = MathUtils.Cross(rB, _perp);

                float k11 = mA + mB + iA * _s1 * _s1 + iB * _s2 * _s2;
                float k12 = iA * _s1 + iB * _s2;
                float k22 = iA + iB;
                if (k22 == 0.0f)
                {
                    // For bodies with fixed rotation.
                    k22 = 1.0f;
                }

                _K.ex = new Vector2(k11, k12);
                _K.ey = new Vector2(k12, k22);
            }

            if (_enableLimit)
            {
                _translation = Vector2.Dot(_axis, d);
            }
            else
            {
                _lowerImpulse = 0.0f;
                _upperImpulse = 0.0f;
            }

            if (!_enableMotor)
                _motorImpulse = 0.0f;

            if (data.Step.WarmStarting)
            {
                // Account for variable time step.
                _impulse *= data.Step.DeltaTimeRatio;
                _motorImpulse *= data.Step.DeltaTimeRatio;
                _lowerImpulse *= data.Step.DeltaTimeRatio;
                _upperImpulse *= data.Step.DeltaTimeRatio;

                float axialImpulse = _motorImpulse + _lowerImpulse - _upperImpulse;
                Vector2 P = _impulse.X * _perp + axialImpulse * _axis;
                float LA = _impulse.X * _s1 + _impulse.Y + axialImpulse * _a1;
                float LB = _impulse.X * _s2 + _impulse.Y + axialImpulse * _a2;

                vA -= mA * P;
                wA -= iA * LA;

                vB += mB * P;
                wB += iB * LB;
            }
            else
            {
                _impulse = Vector2.Zero;
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
            Vector2 vA = data.Velocities[_indexA].V;
            float wA = data.Velocities[_indexA].W;
            Vector2 vB = data.Velocities[_indexB].V;
            float wB = data.Velocities[_indexB].W;

            float mA = _invMassA, mB = _invMassB;
            float iA = _invIA, iB = _invIB;

            // Solve linear motor constraint.
            if (_enableMotor)
            {
                float Cdot = Vector2.Dot(_axis, vB - vA) + _a2 * wB - _a1 * wA;
                float impulse = _axialMass * (_motorSpeed - Cdot);
                float oldImpulse = _motorImpulse;
                float maxImpulse = data.Step.DeltaTime * _maxMotorForce;
                _motorImpulse = MathUtils.Clamp(_motorImpulse + impulse, -maxImpulse, maxImpulse);
                impulse = _motorImpulse - oldImpulse;

                Vector2 P = impulse * _axis;
                float LA = impulse * _a1;
                float LB = impulse * _a2;

                vA -= mA * P;
                wA -= iA * LA;
                vB += mB * P;
                wB += iB * LB;
            }

            if (_enableLimit)
            {
                // Lower limit
                {
                    float C = _translation - _lowerTranslation;
                    float Cdot = MathUtils.Dot(_axis, vB - vA) + _a2 * wB - _a1 * wA;
                    float impulse = -_axialMass * (Cdot + MathUtils.Max(C, 0.0f) * data.Step.InvertedDeltaTime);
                    float oldImpulse = _lowerImpulse;
                    _lowerImpulse = MathUtils.Max(_lowerImpulse + impulse, 0.0f);
                    impulse = _lowerImpulse - oldImpulse;

                    Vector2 P = impulse * _axis;
                    float LA = impulse * _a1;
                    float LB = impulse * _a2;

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
                    float Cdot = MathUtils.Dot(_axis, vA - vB) + _a1 * wA - _a2 * wB;
                    float impulse = -_axialMass * (Cdot + MathUtils.Max(C, 0.0f) * data.Step.InvertedDeltaTime);
                    float oldImpulse = _upperImpulse;
                    _upperImpulse = MathUtils.Max(_upperImpulse + impulse, 0.0f);
                    impulse = _upperImpulse - oldImpulse;

                    Vector2 P = impulse * _axis;
                    float LA = impulse * _a1;
                    float LB = impulse * _a2;

                    vA += mA * P;
                    wA += iA * LA;
                    vB -= mB * P;
                    wB -= iB * LB;
                }
            }

            // Solve the prismatic constraint in block form.
            {
                Vector2 Cdot;
                Cdot.X = MathUtils.Dot(_perp, vB - vA) + _s2 * wB - _s1 * wA;
                Cdot.Y = wB - wA;

                Vector2 df = _K.Solve(-Cdot);
                _impulse += df;

                Vector2 P = df.X * _perp;
                float LA = df.X * _s1 + df.Y;
                float LB = df.X * _s2 + df.Y;

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

        // A velocity based solver computes reaction forces(impulses) using the velocity constraint solver. Under this context,
        // the position solver is not there to resolve forces. It is only there to cope with integration error.
        //
        // Therefore, the pseudo impulses in the position solver do not have any physical meaning. Thus it is okay if they suck.
        //
        // We could take the active state from the velocity solver. However, the joint might push past the limit when the velocity
        // solver indicates the limit is inactive.
        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            Vector2 cA = data.Positions[_indexA].C;
            float aA = data.Positions[_indexA].A;
            Vector2 cB = data.Positions[_indexB].C;
            float aB = data.Positions[_indexB].A;

            Rot qA = new Rot(aA), qB = new Rot(aB);

            float mA = _invMassA, mB = _invMassB;
            float iA = _invIA, iB = _invIB;

            // Compute fresh Jacobians
            Vector2 rA = MathUtils.Mul(qA, _localAnchorA - _localCenterA);
            Vector2 rB = MathUtils.Mul(qB, _localAnchorB - _localCenterB);
            Vector2 d = cB + rB - cA - rA;

            Vector2 axis = MathUtils.Mul(qA, _localXAxisA);
            float a1 = MathUtils.Cross(d + rA, axis);
            float a2 = MathUtils.Cross(rB, axis);
            Vector2 perp = MathUtils.Mul(qA, _localYAxisA);

            float s1 = MathUtils.Cross(d + rA, perp);
            float s2 = MathUtils.Cross(rB, perp);

            Vector3 impulse;
            Vector2 C1;
            C1.X = Vector2.Dot(perp, d);
            C1.Y = aB - aA - _referenceAngle;

            float linearError = MathUtils.Abs(C1.X);
            float angularError = MathUtils.Abs(C1.Y);

            bool active = false;
            float C2 = 0.0f;
            if (_enableLimit)
            {
                float translation = MathUtils.Dot(axis, d);
                if (MathUtils.Abs(_upperTranslation - _lowerTranslation) < 2.0f * Settings.LinearSlop)
                {
                    C2 = translation;
                    linearError = MathUtils.Max(linearError, MathUtils.Abs(translation));
                    active = true;
                }
                else if (translation <= _lowerTranslation)
                {
                    C2 = MathUtils.Min(translation - _lowerTranslation, 0.0f);
                    linearError = MathUtils.Max(linearError, _lowerTranslation - translation);
                    active = true;
                }
                else if (translation >= _upperTranslation)
                {
                    C2 = MathUtils.Max(translation - _upperTranslation, 0.0f);
                    linearError = MathUtils.Max(linearError, translation - _upperTranslation);
                    active = true;
                }
            }

            if (active)
            {
                float k11 = mA + mB + iA * s1 * s1 + iB * s2 * s2;
                float k12 = iA * s1 + iB * s2;
                float k13 = iA * s1 * a1 + iB * s2 * a2;
                float k22 = iA + iB;
                if (k22 == 0.0f)
                {
                    // For fixed rotation
                    k22 = 1.0f;
                }
                float k23 = iA * a1 + iB * a2;
                float k33 = mA + mB + iA * a1 * a1 + iB * a2 * a2;

                Mat33 K;
                K.ex = new Vector3(k11, k12, k13);
                K.ey = new Vector3(k12, k22, k23);
                K.ez = new Vector3(k13, k23, k33);

                Vector3 C;
                C.X = C1.X;
                C.Y = C1.Y;
                C.Z = C2;

                impulse = K.Solve33(-C);
            }
            else
            {
                float k11 = mA + mB + iA * s1 * s1 + iB * s2 * s2;
                float k12 = iA * s1 + iB * s2;
                float k22 = iA + iB;
                if (k22 == 0.0f)
                    k22 = 1.0f;

                Mat22 K;
                K.ex = new Vector2(k11, k12);
                K.ey = new Vector2(k12, k22);

                Vector2 impulse1 = K.Solve(-C1);
                impulse.X = impulse1.X;
                impulse.Y = impulse1.Y;
                impulse.Z = 0.0f;
            }

            Vector2 P = impulse.X * perp + impulse.Z * axis;
            float LA = impulse.X * s1 + impulse.Y + impulse.Z * a1;
            float LB = impulse.X * s2 + impulse.Y + impulse.Z * a2;

            cA -= mA * P;
            aA -= iA * LA;
            cB += mB * P;
            aB += iB * LB;

            data.Positions[_indexA].C = cA;
            data.Positions[_indexA].A = aA;
            data.Positions[_indexB].C = cB;
            data.Positions[_indexB].A = aB;

            return linearError <= Settings.LinearSlop && angularError <= Settings.AngularSlop;
        }

        //Velcro: We support initializing without a template
        private void Initialize(Vector2 localAnchorA, Vector2 localAnchorB, Vector2 axis, bool useWorldCoordinates)
        {
            //Velcro: We support setting anchors in world coordinates
            if (useWorldCoordinates)
            {
                _localAnchorA = BodyA.GetLocalPoint(localAnchorA);
                _localAnchorB = BodyB.GetLocalPoint(localAnchorB);
                _localXAxisA = _bodyA.GetLocalVector(axis);
            }
            else
            {
                _localAnchorA = localAnchorA;
                _localAnchorB = localAnchorB;
                _localXAxisA = axis;
            }

            _referenceAngle = _bodyB.Rotation - _bodyA.Rotation;
            _localXAxisA.Normalize();
            _localYAxisA = MathUtils.Cross(1.0f, _localXAxisA);
        }
    }
}