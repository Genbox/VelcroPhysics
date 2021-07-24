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

using Genbox.VelcroPhysics.Definitions.Joints;
using Genbox.VelcroPhysics.Dynamics.Joints.Misc;
using Genbox.VelcroPhysics.Dynamics.Solver;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Dynamics.Joints
{
    // Point-to-point constraint
    // C = p2 - p1
    // Cdot = v2 - v1
    //      = v2 + cross(w2, r2) - v1 - cross(w1, r1)
    // J = [-I -r1_skew I r2_skew ]
    // Identity used:
    // w k % (rx i + ry j) = w * (-ry i + rx j)

    // Motor constraint
    // Cdot = w2 - w1
    // J = [0 0 -1 0 0 1]
    // K = invI1 + invI2

    /// <summary>
    /// A revolute joint constrains to bodies to share a common point while they are free to rotate about the point.
    /// The relative rotation about the shared point is the joint angle. You can limit the relative rotation with a joint limit
    /// that specifies a lower and upper angle. You can use a motor to drive the relative rotation about the shared point. A
    /// maximum motor torque is provided so that infinite forces are not generated.
    /// </summary>
    public class RevoluteJoint : Joint
    {
        // Solver shared
        internal Vector2 _localAnchorA;
        internal Vector2 _localAnchorB;
        private Vector2 _impulse;
        private float _motorImpulse;
        private float _lowerImpulse;
        private float _upperImpulse;
        private bool _enableMotor;
        private float _maxMotorTorque;
        private float _motorSpeed;
        private bool _enableLimit;
        internal float _referenceAngle;
        private float _lowerAngle;
        private float _upperAngle;

        // Solver temp
        private int _indexA;
        private int _indexB;
        private Vector2 _rA;
        private Vector2 _rB;
        private Vector2 _localCenterA;
        private Vector2 _localCenterB;
        private float _invMassA;
        private float _invMassB;
        private float _invIA;
        private float _invIB;
        private Mat22 _K;
        private float _angle;
        private float _axialMass;

        public RevoluteJoint(RevoluteJointDef def)
            : base(def)
        {
            _localAnchorA = def.LocalAnchorA;
            _localAnchorB = def.LocalAnchorB;
            _referenceAngle = def.ReferenceAngle;

            _lowerAngle = def.LowerAngle;
            _upperAngle = def.UpperAngle;
            _maxMotorTorque = def.MaxMotorTorque;
            _motorSpeed = def.MotorSpeed;
            _enableLimit = def.EnableLimit;
            _enableMotor = def.EnableMotor;

            _angle = 0.0f;
        }

        /// <summary>Constructor of RevoluteJoint.</summary>
        /// <param name="bodyA">The first body.</param>
        /// <param name="bodyB">The second body.</param>
        /// <param name="anchorA">The first body anchor.</param>
        /// <param name="anchorB">The second anchor.</param>
        /// <param name="useWorldCoordinates">Set to true if you are using world coordinates as anchors.</param>
        public RevoluteJoint(Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB, bool useWorldCoordinates = false)
            : base(bodyA, bodyB, JointType.Revolute)
        {
            if (useWorldCoordinates)
            {
                _localAnchorA = bodyA.GetLocalPoint(anchorA);
                _localAnchorB = bodyB.GetLocalPoint(anchorB);
            }
            else
            {
                _localAnchorA = anchorA;
                _localAnchorB = anchorB;
            }

            _referenceAngle = bodyB._sweep.A - bodyA._sweep.A;
        }

        /// <summary>Constructor of RevoluteJoint.</summary>
        /// <param name="bodyA">The first body.</param>
        /// <param name="bodyB">The second body.</param>
        /// <param name="anchor">The shared anchor.</param>
        /// <param name="useWorldCoordinates"></param>
        public RevoluteJoint(Body bodyA, Body bodyB, Vector2 anchor, bool useWorldCoordinates = false)
            : this(bodyA, bodyB, anchor, anchor, useWorldCoordinates) { }

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

        /// <summary>The referance angle computed as BodyB angle minus BodyA angle.</summary>
        public float ReferenceAngle
        {
            get => _referenceAngle;
            set => _referenceAngle = value;
        }

        /// <summary>Get the current joint angle in radians.</summary>
        public float JointAngle => _bodyB._sweep.A - _bodyA._sweep.A - _referenceAngle;

        /// <summary>Get the current joint angle speed in radians per second.</summary>
        public float JointSpeed => _bodyB._angularVelocity - _bodyA._angularVelocity;

        /// <summary>Is the joint limit enabled?</summary>
        /// <value><c>true</c> if [limit enabled]; otherwise, <c>false</c>.</value>
        public bool LimitEnabled
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

        /// <summary>Get the lower joint limit in radians.</summary>
        public float LowerLimit
        {
            get => _lowerAngle;
            set
            {
                if (_lowerAngle != value)
                {
                    WakeBodies();
                    _lowerAngle = value;
                    _lowerImpulse = 0.0f;
                }
            }
        }

        /// <summary>Get the upper joint limit in radians.</summary>
        public float UpperLimit
        {
            get => _upperAngle;
            set
            {
                if (_upperAngle != value)
                {
                    WakeBodies();
                    _upperAngle = value;
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

        /// <summary>Get or set the motor speed in radians per second.</summary>
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

        /// <summary>Get or set the maximum motor torque, usually in N-m.</summary>
        public float MaxMotorTorque
        {
            set
            {
                if (value != _maxMotorTorque)
                {
                    WakeBodies();
                    _maxMotorTorque = value;
                }
            }
            get => _maxMotorTorque;
        }

        /// <summary>Set the joint limits, usually in meters.</summary>
        /// <param name="lower">The lower limit</param>
        /// <param name="upper">The upper limit</param>
        public void SetLimits(float lower, float upper)
        {
            if (lower != _lowerAngle || upper != _upperAngle)
            {
                WakeBodies();
                _lowerImpulse = 0.0f;
                _upperImpulse = 0.0f;
                _upperAngle = upper;
                _lowerAngle = lower;
            }
        }

        /// <summary>Gets the motor torque in N-m.</summary>
        /// <param name="invDt">The inverse delta time</param>
        public float GetMotorTorque(float invDt)
        {
            return invDt * _motorImpulse;
        }

        public override Vector2 GetReactionForce(float invDt)
        {
            Vector2 p = new Vector2(_impulse.X, _impulse.Y);
            return invDt * p;
        }

        public override float GetReactionTorque(float invDt)
        {
            return invDt * (_motorImpulse + _lowerImpulse - _upperImpulse);
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

            float aA = data.Positions[_indexA].A;
            Vector2 vA = data.Velocities[_indexA].V;
            float wA = data.Velocities[_indexA].W;

            float aB = data.Positions[_indexB].A;
            Vector2 vB = data.Velocities[_indexB].V;
            float wB = data.Velocities[_indexB].W;

            Rot qA = new Rot(aA), qB = new Rot(aB);

            _rA = MathUtils.Mul(qA, _localAnchorA - _localCenterA);
            _rB = MathUtils.Mul(qB, _localAnchorB - _localCenterB);

            // J = [-I -r1_skew I r2_skew]
            // r_skew = [-ry; rx]

            // Matlab
            // K = [ mA+r1y^2*iA+mB+r2y^2*iB,  -r1y*iA*r1x-r2y*iB*r2x]
            //     [  -r1y*iA*r1x-r2y*iB*r2x, mA+r1x^2*iA+mB+r2x^2*iB]

            float mA = _invMassA, mB = _invMassB;
            float iA = _invIA, iB = _invIB;

            _K.ex.X = mA + mB + _rA.Y * _rA.Y * iA + _rB.Y * _rB.Y * iB;
            _K.ey.X = -_rA.Y * _rA.X * iA - _rB.Y * _rB.X * iB;
            _K.ex.Y = _K.ey.X;
            _K.ey.Y = mA + mB + _rA.X * _rA.X * iA + _rB.X * _rB.X * iB;

            _axialMass = iA + iB;
            bool fixedRotation;
            if (_axialMass > 0.0f)
            {
                _axialMass = 1.0f / _axialMass;
                fixedRotation = false;
            }
            else
            {
                fixedRotation = true;
            }

            _angle = aB - aA - _referenceAngle;
            if (_enableLimit == false || fixedRotation)
            {
                _lowerImpulse = 0.0f;
                _upperImpulse = 0.0f;
            }

            if (_enableMotor == false || fixedRotation)
            {
                _motorImpulse = 0.0f;
            }

            if (data.Step.WarmStarting)
            {
                // Scale impulses to support a variable time step.
                _impulse *= data.Step.DeltaTimeRatio;
                _motorImpulse *= data.Step.DeltaTimeRatio;
                _lowerImpulse *= data.Step.DeltaTimeRatio;
                _upperImpulse *= data.Step.DeltaTimeRatio;

                float axialImpulse = _motorImpulse + _lowerImpulse - _upperImpulse;
                Vector2 P = new Vector2(_impulse.X, _impulse.Y);

                vA -= mA * P;
                wA -= iA * (MathUtils.Cross(_rA, P) + axialImpulse);

                vB += mB * P;
                wB += iB * (MathUtils.Cross(_rB, P) + axialImpulse);
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

            bool fixedRotation = (iA + iB == 0.0f);

            // Solve motor constraint.
            if (_enableMotor && fixedRotation == false)
            {
                float Cdot = wB - wA - _motorSpeed;
                float impulse = -_axialMass * Cdot;
                float oldImpulse = _motorImpulse;
                float maxImpulse = data.Step.DeltaTime * _maxMotorTorque;
                _motorImpulse = MathUtils.Clamp(_motorImpulse + impulse, -maxImpulse, maxImpulse);
                impulse = _motorImpulse - oldImpulse;

                wA -= iA * impulse;
                wB += iB * impulse;
            }

            if (_enableLimit && fixedRotation == false)
            {
                // Lower limit
                {
                    float C = _angle - _lowerAngle;
                    float Cdot = wB - wA;
                    float impulse = -_axialMass * (Cdot + MathUtils.Max(C, 0.0f) * data.Step.InvertedDeltaTime);
                    float oldImpulse = _lowerImpulse;
                    _lowerImpulse = MathUtils.Max(_lowerImpulse + impulse, 0.0f);
                    impulse = _lowerImpulse - oldImpulse;

                    wA -= iA * impulse;
                    wB += iB * impulse;
                }

                // Upper limit
                // Note: signs are flipped to keep C positive when the constraint is satisfied.
                // This also keeps the impulse positive when the limit is active.
                {
                    float C = _upperAngle - _angle;
                    float Cdot = wA - wB;
                    float impulse = -_axialMass * (Cdot + MathUtils.Max(C, 0.0f) * data.Step.InvertedDeltaTime);
                    float oldImpulse = _upperImpulse;
                    _upperImpulse = MathUtils.Max(_upperImpulse + impulse, 0.0f);
                    impulse = _upperImpulse - oldImpulse;

                    wA += iA * impulse;
                    wB -= iB * impulse;
                }
            }

            // Solve point-to-point constraint
            {
                Vector2 Cdot = vB + MathUtils.Cross(wB, _rB) - vA - MathUtils.Cross(wA, _rA);
                Vector2 impulse = _K.Solve(-Cdot);

                _impulse.X += impulse.X;
                _impulse.Y += impulse.Y;

                vA -= mA * impulse;
                wA -= iA * MathUtils.Cross(_rA, impulse);

                vB += mB * impulse;
                wB += iB * MathUtils.Cross(_rB, impulse);
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

            float angularError = 0.0f;
            float positionError = 0.0f;

            bool fixedRotation = (_invIA + _invIB == 0.0f);

            // Solve angular limit constraint
            if (_enableLimit && fixedRotation == false)
            {
                float angle = aB - aA - _referenceAngle;
                float C = 0.0f;

                if (MathUtils.Abs(_upperAngle - _lowerAngle) < 2.0f * Settings.AngularSlop)
                {
                    // Prevent large angular corrections
                    C = MathUtils.Clamp(angle - _lowerAngle, -Settings.MaxAngularCorrection, Settings.MaxAngularCorrection);
                }
                else if (angle <= _lowerAngle)
                {
                    // Prevent large angular corrections and allow some slop.
                    C = MathUtils.Clamp(angle - _lowerAngle + Settings.AngularSlop, -Settings.MaxAngularCorrection, 0.0f);
                }
                else if (angle >= _upperAngle)
                {
                    // Prevent large angular corrections and allow some slop.
                    C = MathUtils.Clamp(angle - _upperAngle - Settings.AngularSlop, 0.0f, Settings.MaxAngularCorrection);
                }

                float limitImpulse = -_axialMass * C;
                aA -= _invIA * limitImpulse;
                aB += _invIB * limitImpulse;
                angularError = MathUtils.Abs(C);
            }

            // Solve point-to-point constraint.
            {
                qA.Set(aA);
                qB.Set(aB);
                Vector2 rA = MathUtils.Mul(qA, _localAnchorA - _localCenterA);
                Vector2 rB = MathUtils.Mul(qB, _localAnchorB - _localCenterB);

                Vector2 C = cB + rB - cA - rA;
                positionError = C.Length();

                float mA = _invMassA, mB = _invMassB;
                float iA = _invIA, iB = _invIB;

                Mat22 K;
                K.ex.X = mA + mB + iA * rA.Y * rA.Y + iB * rB.Y * rB.Y;
                K.ex.Y = -iA * rA.X * rA.Y - iB * rB.X * rB.Y;
                K.ey.X = K.ex.Y;
                K.ey.Y = mA + mB + iA * rA.X * rA.X + iB * rB.X * rB.X;

                Vector2 impulse = -K.Solve(C);

                cA -= mA * impulse;
                aA -= iA * MathUtils.Cross(rA, impulse);

                cB += mB * impulse;
                aB += iB * MathUtils.Cross(rB, impulse);
            }

            data.Positions[_indexA].C = cA;
            data.Positions[_indexA].A = aA;
            data.Positions[_indexB].C = cB;
            data.Positions[_indexB].A = aB;

            return positionError <= Settings.LinearSlop && angularError <= Settings.AngularSlop;
        }
    }
}