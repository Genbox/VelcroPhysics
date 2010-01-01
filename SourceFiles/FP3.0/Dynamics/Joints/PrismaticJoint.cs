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

using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace FarseerPhysics
{
    /// Prismatic joint definition. This requires defining a line of
    /// motion using an axis and an anchor point. The definition uses local
    /// anchor points and a local axis so that the initial configuration
    /// can violate the constraint slightly. The joint translation is zero
    /// when the local anchor points coincide in world space. Using local
    /// anchors and a local axis helps when saving and loading a game.
    public class PrismaticJointDef : JointDef
    {
        public PrismaticJointDef()
        {
            Type = JointType.Prismatic;
            LocalAnchorA = Vector2.Zero;
            LocalAnchorB = Vector2.Zero;
            LocalAxis1 = new Vector2(1.0f, 0.0f);
            ReferenceAngle = 0.0f;
            EnableLimit = false;
            LowerTranslation = 0.0f;
            UpperTranslation = 0.0f;
            EnableMotor = false;
            MaxMotorForce = 0.0f;
            MotorSpeed = 0.0f;
        }

        /// Initialize the bodies, anchors, axis, and reference angle using the world
        /// anchor and world axis.
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
        // Cdot = = -dot(ax1, v1) - dot(cross(d + r1, ax1), w1) + dot(ax1, v2) + dot(cross(r2, ax1), v2)
        // J = [-ax1 -cross(d+r1,ax1) ax1 cross(r2,ax1)]

        // Block Solver
        // We develop a block solver that includes the joint limit. This makes the limit stiff (inelastic) even
        // when the mass has poor distribution (leading to large torques about the joint anchor points).
        //
        // The Jacobian has 3 rows:
        // J = [-uT -s1 uT s2] // linear
        //     [0   -1   0  1] // angular
        //     [-vT -a1 vT a2] // limit
        //
        // u = perp
        // v = axis
        // s1 = cross(d + r1, u), s2 = cross(r2, u)
        // a1 = cross(d + r1, v), a2 = cross(r2, v)

        // M * (v2 - v1) = JT * df
        // J * v2 = bias
        //
        // v2 = v1 + invM * JT * df
        // J * (v1 + invM * JT * df) = bias
        // K * df = bias - J * v1 = -Cdot
        // K = J * invM * JT
        // Cdot = J * v1 - bias
        //
        // Now solve for f2.
        // df = f2 - f1
        // K * (f2 - f1) = -Cdot
        // f2 = invK * (-Cdot) + f1
        //
        // Clamp accumulated limit impulse.
        // lower: f2(3) = max(f2(3), 0)
        // upper: f2(3) = min(f2(3), 0)
        //
        // Solve for correct f2(1:2)
        // K(1:2, 1:2) * f2(1:2) = -Cdot(1:2) - K(1:2,3) * f2(3) + K(1:2,1:3) * f1
        //                       = -Cdot(1:2) - K(1:2,3) * f2(3) + K(1:2,1:2) * f1(1:2) + K(1:2,3) * f1(3)
        // K(1:2, 1:2) * f2(1:2) = -Cdot(1:2) - K(1:2,3) * (f2(3) - f1(3)) + K(1:2,1:2) * f1(1:2)
        // f2(1:2) = invK(1:2,1:2) * (-Cdot(1:2) - K(1:2,3) * (f2(3) - f1(3))) + f1(1:2)
        //
        // Now compute impulse to be applied:
        // df = f2 - f1
        public void Initialize(Body b1, Body b2, Vector2 anchor, Vector2 axis)
        {
            BodyA = b1;
            BodyB = b2;
            LocalAnchorA = BodyA.GetLocalPoint(anchor);
            LocalAnchorB = BodyB.GetLocalPoint(anchor);
            LocalAxis1 = BodyA.GetLocalVector(axis);
            ReferenceAngle = BodyB.GetAngle() - BodyA.GetAngle();
        }

        /// <summary>
        /// The local anchor point relative to body1's origin.
        /// </summary>
        public Vector2 LocalAnchorA;

        /// <summary>
        /// The local anchor point relative to body2's origin.
        /// </summary>
        public Vector2 LocalAnchorB;

        /// <summary>
        /// The local translation axis in body1.
        /// </summary>
        public Vector2 LocalAxis1;

        /// <summary>
        /// The rained angle between the bodies: body2_angle - body1_angle.
        /// </summary>
        public float ReferenceAngle;

        /// <summary>
        /// Enable/disable the joint limit.
        /// </summary>
        public bool EnableLimit;

        /// <summary>
        /// The lower translation limit, usually in meters.
        /// </summary>
        public float LowerTranslation;

        /// <summary>
        /// The upper translation limit, usually in meters.
        /// </summary>
        public float UpperTranslation;

        /// <summary>
        /// Enable/disable the joint motor.
        /// </summary>
        public bool EnableMotor;

        /// <summary>
        /// The maximum motor torque, usually in N-m.
        /// </summary>
        public float MaxMotorForce;

        /// <summary>
        /// The desired motor speed in radians per second.
        /// </summary>
        public float MotorSpeed;
    }

    /// <summary>
    /// A prismatic joint. This joint provides one degree of freedom: translation
    /// along an axis fixed in body1. Relative rotation is prevented. You can
    /// use a joint limit to restrict the range of motion and a joint motor to
    /// drive the motion or to model joint friction.
    /// </summary>
    public class PrismaticJoint : Joint
    {
        public override Vector2 GetAnchorA()
        {
            return BodyA.GetWorldPoint(LocalAnchor1);
        }

        public override Vector2 GetAnchorB()
        {
            return BodyB.GetWorldPoint(LocalAnchor2);
        }

        public override Vector2 GetReactionForce(float inv_dt)
        {
            return inv_dt * (_impulse.X * _perp + (_motorImpulse + _impulse.Z) * _axis);
        }

        public override float GetReactionTorque(float inv_dt)
        {
            return inv_dt * _impulse.Y;
        }

        /// <summary>
        /// Get the current joint translation, usually in meters.
        /// </summary>
        /// <returns></returns>
        public float GetJointTranslation()
        {
            Body b1 = BodyA;
            Body b2 = BodyB;

            Vector2 p1 = b1.GetWorldPoint(LocalAnchor1);
            Vector2 p2 = b2.GetWorldPoint(LocalAnchor2);
            Vector2 d = p2 - p1;
            Vector2 axis = b1.GetWorldVector(LocalXAxis1);

            float translation = Vector2.Dot(d, axis);
            return translation;
        }

        /// <summary>
        /// Get the current joint translation speed, usually in meters per second.
        /// </summary>
        /// <returns></returns>
        public float GetJointSpeed()
        {
            Body b1 = BodyA;
            Body b2 = BodyB;

            Transform xf1, xf2;
            b1.GetTransform(out xf1);
            b2.GetTransform(out xf2);

            Vector2 r1 = MathUtils.Multiply(ref xf1.R, LocalAnchor1 - b1.LocalCenter);
            Vector2 r2 = MathUtils.Multiply(ref xf2.R, LocalAnchor2 - b2.LocalCenter);
            Vector2 p1 = b1._sweep.c + r1;
            Vector2 p2 = b2._sweep.c + r2;
            Vector2 d = p2 - p1;
            Vector2 axis = b1.GetWorldVector(LocalXAxis1);

            Vector2 v1 = b1._linearVelocity;
            Vector2 v2 = b2._linearVelocity;
            float w1 = b1._angularVelocity;
            float w2 = b2._angularVelocity;

            float speed = Vector2.Dot(d, MathUtils.Cross(w1, axis)) + Vector2.Dot(axis, v2 + MathUtils.Cross(w2, r2) - v1 - MathUtils.Cross(w1, r1));
            return speed;
        }

        /// <summary>
        /// Is the joint limit enabled?
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is limit enabled]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsLimitEnabled()
        {
            return _enableLimit;
        }

        /// <summary>
        /// Enable/disable the joint limit.
        /// </summary>
        /// <param name="flag">if set to <c>true</c> [flag].</param>
        public void EnableLimit(bool flag)
        {
            BodyA.Awake = true;
            BodyB.Awake = true;
            _enableLimit = flag;
        }

        /// <summary>
        /// Get the lower joint limit, usually in meters.
        /// </summary>
        /// <returns></returns>
        public float GetLowerLimit()
        {
            return _lowerTranslation;
        }

        /// <summary>
        /// Get the upper joint limit, usually in meters.
        /// </summary>
        /// <returns></returns>
        public float GetUpperLimit()
        {
            return _upperTranslation;
        }

        /// <summary>
        /// Set the joint limits, usually in meters.
        /// </summary>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        public void SetLimits(float lower, float upper)
        {
            Debug.Assert(lower <= upper);
            BodyA.Awake = true;
            BodyB.Awake = true;
            _lowerTranslation = lower;
            _upperTranslation = upper;
        }

        /// <summary>
        /// Is the joint motor enabled?
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is motor enabled]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMotorEnabled()
        {
            return _enableMotor;
        }

        /// <summary>
        /// Enable/disable the joint motor.
        /// </summary>
        /// <param name="flag">if set to <c>true</c> [flag].</param>
        public void EnableMotor(bool flag)
        {
            BodyA.Awake = true;
            BodyB.Awake = true;
            _enableMotor = flag;
        }

        /// <summary>
        /// Set the motor speed, usually in meters per second.
        /// </summary>
        /// <param name="speed">The speed.</param>
        public void SetMotorSpeed(float speed)
        {
            BodyA.Awake = true;
            BodyB.Awake = true;
            _motorSpeed = speed;
        }

        /// <summary>
        /// Get the motor speed, usually in meters per second.
        /// </summary>
        /// <returns></returns>
        public float GetMotorSpeed()
        {
            return _motorSpeed;
        }

        /// <summary>
        /// Set the maximum motor force, usually in N.
        /// </summary>
        /// <param name="force">The force.</param>
        public void SetMaxMotorForce(float force)
        {
            BodyA.Awake = true;
            BodyB.Awake = true;
            _maxMotorForce = force;
        }

        /// <summary>
        /// Get the current motor force, usually in N.
        /// </summary>
        /// <returns></returns>
        public float GetMotorForce()
        {
            return _motorImpulse;
        }

        internal PrismaticJoint(PrismaticJointDef def)
            : base(def)
        {
            LocalAnchor1 = def.LocalAnchorA;
            LocalAnchor2 = def.LocalAnchorB;
            LocalXAxis1 = def.LocalAxis1;
            _localYAxis1 = MathUtils.Cross(1.0f, LocalXAxis1);
            _refAngle = def.ReferenceAngle;

            _impulse = Vector3.Zero;
            _motorMass = 0.0f;
            _motorImpulse = 0.0f;

            _lowerTranslation = def.LowerTranslation;
            _upperTranslation = def.UpperTranslation;
            _maxMotorForce = def.MaxMotorForce;
            _motorSpeed = def.MotorSpeed;
            _enableLimit = def.EnableLimit;
            _enableMotor = def.EnableMotor;
            _limitState = LimitState.Inactive;

            _axis = Vector2.Zero;
            _perp = Vector2.Zero;
        }

        internal override void InitVelocityConstraints(ref TimeStep step)
        {
            Body b1 = BodyA;
            Body b2 = BodyB;

            LocalCenterA = b1.LocalCenter;
            LocalCenterB = b2.LocalCenter;

            Transform xf1, xf2;
            b1.GetTransform(out xf1);
            b2.GetTransform(out xf2);

            // Compute the effective masses.
            Vector2 r1 = MathUtils.Multiply(ref xf1.R, LocalAnchor1 - LocalCenterA);
            Vector2 r2 = MathUtils.Multiply(ref xf2.R, LocalAnchor2 - LocalCenterB);
            Vector2 d = b2._sweep.c + r2 - b1._sweep.c - r1;

            InvMassA = b1._invMass;
            InvIA = b1._invI;
            InvMassB = b2._invMass;
            InvIB = b2._invI;

            // Compute motor Jacobian and effective mass.
            {
                _axis = MathUtils.Multiply(ref xf1.R, LocalXAxis1);
                _a1 = MathUtils.Cross(d + r1, _axis);
                _a2 = MathUtils.Cross(r2, _axis);

                _motorMass = InvMassA + InvMassB + InvIA * _a1 * _a1 + InvIB * _a2 * _a2;

                if (_motorMass > Settings.Epsilon)
                {
                    _motorMass = 1.0f / _motorMass;
                }
            }

            // Prismatic constraint.
            {
                _perp = MathUtils.Multiply(ref xf1.R, _localYAxis1);

                _s1 = MathUtils.Cross(d + r1, _perp);
                _s2 = MathUtils.Cross(r2, _perp);

                float m1 = InvMassA, m2 = InvMassB;
                float i1 = InvIA, i2 = InvIB;

                float k11 = m1 + m2 + i1 * _s1 * _s1 + i2 * _s2 * _s2;
                float k12 = i1 * _s1 + i2 * _s2;
                float k13 = i1 * _s1 * _a1 + i2 * _s2 * _a2;
                float k22 = i1 + i2;
                float k23 = i1 * _a1 + i2 * _a2;
                float k33 = m1 + m2 + i1 * _a1 * _a1 + i2 * _a2 * _a2;

                _K.col1 = new Vector3(k11, k12, k13);
                _K.col2 = new Vector3(k12, k22, k23);
                _K.col3 = new Vector3(k13, k23, k33);
            }

            // Compute motor and limit terms.
            if (_enableLimit)
            {
                float jointTranslation = Vector2.Dot(_axis, d);
                if (Math.Abs(_upperTranslation - _lowerTranslation) < 2.0f * Settings.LinearSlop)
                {
                    _limitState = LimitState.Equal;
                }
                else if (jointTranslation <= _lowerTranslation)
                {
                    if (_limitState != LimitState.AtLower)
                    {
                        _limitState = LimitState.AtLower;
                        _impulse.Z = 0.0f;
                    }
                }
                else if (jointTranslation >= _upperTranslation)
                {
                    if (_limitState != LimitState.AtUpper)
                    {
                        _limitState = LimitState.AtUpper;
                        _impulse.Z = 0.0f;
                    }
                }
                else
                {
                    _limitState = LimitState.Inactive;
                    _impulse.Z = 0.0f;
                }
            }
            else
            {
                _limitState = LimitState.Inactive;
            }

            if (_enableMotor == false)
            {
                _motorImpulse = 0.0f;
            }

            if (step.WarmStarting)
            {
                // Account for variable time step.
                _impulse *= step.DtRatio;
                _motorImpulse *= step.DtRatio;

                Vector2 P = _impulse.X * _perp + (_motorImpulse + _impulse.Z) * _axis;
                float L1 = _impulse.X * _s1 + _impulse.Y + (_motorImpulse + _impulse.Z) * _a1;
                float L2 = _impulse.X * _s2 + _impulse.Y + (_motorImpulse + _impulse.Z) * _a2;

                b1._linearVelocity -= InvMassA * P;
                b1._angularVelocity -= InvIA * L1;

                b2._linearVelocity += InvMassB * P;
                b2._angularVelocity += InvIB * L2;
            }
            else
            {
                _impulse = Vector3.Zero;
                _motorImpulse = 0.0f;
            }
        }

        internal override void SolveVelocityConstraints(ref TimeStep step)
        {
            Body b1 = BodyA;
            Body b2 = BodyB;

            Vector2 v1 = b1._linearVelocity;
            float w1 = b1._angularVelocity;
            Vector2 v2 = b2._linearVelocity;
            float w2 = b2._angularVelocity;

            // Solve linear motor constraint.
            if (_enableMotor && _limitState != LimitState.Equal)
            {
                float Cdot = Vector2.Dot(_axis, v2 - v1) + _a2 * w2 - _a1 * w1;
                float impulse = _motorMass * (_motorSpeed - Cdot);
                float oldImpulse = _motorImpulse;
                float maxImpulse = step.DeltaTime * _maxMotorForce;
                _motorImpulse = MathUtils.Clamp(_motorImpulse + impulse, -maxImpulse, maxImpulse);
                impulse = _motorImpulse - oldImpulse;

                Vector2 P = impulse * _axis;
                float L1 = impulse * _a1;
                float L2 = impulse * _a2;

                v1 -= InvMassA * P;
                w1 -= InvIA * L1;

                v2 += InvMassB * P;
                w2 += InvIB * L2;
            }

            Vector2 Cdot1 = new Vector2(Vector2.Dot(_perp, v2 - v1) + _s2 * w2 - _s1 * w1, w2 - w1);

            if (_enableLimit && _limitState != LimitState.Inactive)
            {
                // Solve prismatic and limit constraint in block form.
                float Cdot2 = Vector2.Dot(_axis, v2 - v1) + _a2 * w2 - _a1 * w1;
                Vector3 Cdot = new Vector3(Cdot1.X, Cdot1.Y, Cdot2);

                Vector3 f1 = _impulse;
                Vector3 df = _K.Solve33(-Cdot);
                _impulse += df;

                if (_limitState == LimitState.AtLower)
                {
                    _impulse.Z = Math.Max(_impulse.Z, 0.0f);
                }
                else if (_limitState == LimitState.AtUpper)
                {
                    _impulse.Z = Math.Min(_impulse.Z, 0.0f);
                }

                // f2(1:2) = invK(1:2,1:2) * (-Cdot(1:2) - K(1:2,3) * (f2(3) - f1(3))) + f1(1:2)
                Vector2 b = -Cdot1 - (_impulse.Z - f1.Z) * new Vector2(_K.col3.X, _K.col3.Y);
                Vector2 f2r = _K.Solve22(b) + new Vector2(f1.X, f1.Y);
                _impulse.X = f2r.X;
                _impulse.Y = f2r.Y;

                df = _impulse - f1;

                Vector2 P = df.X * _perp + df.Z * _axis;
                float L1 = df.X * _s1 + df.Y + df.Z * _a1;
                float L2 = df.X * _s2 + df.Y + df.Z * _a2;

                v1 -= InvMassA * P;
                w1 -= InvIA * L1;

                v2 += InvMassB * P;
                w2 += InvIB * L2;
            }
            else
            {
                // Limit is inactive, just solve the prismatic constraint in block form.
                Vector2 df = _K.Solve22(-Cdot1);
                _impulse.X += df.X;
                _impulse.Y += df.Y;

                Vector2 P = df.X * _perp;
                float L1 = df.X * _s1 + df.Y;
                float L2 = df.X * _s2 + df.Y;

                v1 -= InvMassA * P;
                w1 -= InvIA * L1;

                v2 += InvMassB * P;
                w2 += InvIB * L2;
            }

            b1._linearVelocity = v1;
            b1._angularVelocity = w1;
            b2._linearVelocity = v2;
            b2._angularVelocity = w2;
        }

        internal override bool SolvePositionConstraints(float baumgarte)
        {
            Body b1 = BodyA;
            Body b2 = BodyB;

            Vector2 c1 = b1._sweep.c;
            float a1 = b1._sweep.a;

            Vector2 c2 = b2._sweep.c;
            float a2 = b2._sweep.a;

            // Solve linear limit constraint.
            float linearError = 0.0f;
            bool active = false;
            float C2 = 0.0f;

            Mat22 R1 = new Mat22(a1);
            Mat22 R2 = new Mat22(a2);

            Vector2 r1 = MathUtils.Multiply(ref R1, LocalAnchor1 - LocalCenterA);
            Vector2 r2 = MathUtils.Multiply(ref R2, LocalAnchor2 - LocalCenterB);
            Vector2 d = c2 + r2 - c1 - r1;

            if (_enableLimit)
            {
                _axis = MathUtils.Multiply(ref R1, LocalXAxis1);

                _a1 = MathUtils.Cross(d + r1, _axis);
                _a2 = MathUtils.Cross(r2, _axis);

                float translation = Vector2.Dot(_axis, d);
                if (Math.Abs(_upperTranslation - _lowerTranslation) < 2.0f * Settings.LinearSlop)
                {
                    // Prevent large angular corrections
                    C2 = MathUtils.Clamp(translation, -Settings.MaxLinearCorrection, Settings.MaxLinearCorrection);
                    linearError = Math.Abs(translation);
                    active = true;
                }
                else if (translation <= _lowerTranslation)
                {
                    // Prevent large linear corrections and allow some slop.
                    C2 = MathUtils.Clamp(translation - _lowerTranslation + Settings.LinearSlop, -Settings.MaxLinearCorrection, 0.0f);
                    linearError = _lowerTranslation - translation;
                    active = true;
                }
                else if (translation >= _upperTranslation)
                {
                    // Prevent large linear corrections and allow some slop.
                    C2 = MathUtils.Clamp(translation - _upperTranslation - Settings.LinearSlop, 0.0f, Settings.MaxLinearCorrection);
                    linearError = translation - _upperTranslation;
                    active = true;
                }
            }

            _perp = MathUtils.Multiply(ref R1, _localYAxis1);

            _s1 = MathUtils.Cross(d + r1, _perp);
            _s2 = MathUtils.Cross(r2, _perp);

            Vector3 impulse;
            Vector2 C1 = new Vector2(Vector2.Dot(_perp, d), a2 - a1 - _refAngle);

            linearError = Math.Max(linearError, Math.Abs(C1.X));
            float angularError = Math.Abs(C1.Y);

            if (active)
            {
                float m1 = InvMassA, m2 = InvMassB;
                float i1 = InvIA, i2 = InvIB;

                float k11 = m1 + m2 + i1 * _s1 * _s1 + i2 * _s2 * _s2;
                float k12 = i1 * _s1 + i2 * _s2;
                float k13 = i1 * _s1 * _a1 + i2 * _s2 * _a2;
                float k22 = i1 + i2;
                float k23 = i1 * _a1 + i2 * _a2;
                float k33 = m1 + m2 + i1 * _a1 * _a1 + i2 * _a2 * _a2;

                _K.col1 = new Vector3(k11, k12, k13);
                _K.col2 = new Vector3(k12, k22, k23);
                _K.col3 = new Vector3(k13, k23, k33);

                Vector3 C = new Vector3(-C1.X, -C1.Y, -C2);
                impulse = _K.Solve33(C); // negated above
            }
            else
            {
                float m1 = InvMassA, m2 = InvMassB;
                float i1 = InvIA, i2 = InvIB;

                float k11 = m1 + m2 + i1 * _s1 * _s1 + i2 * _s2 * _s2;
                float k12 = i1 * _s1 + i2 * _s2;
                float k22 = i1 + i2;

                _K.col1 = new Vector3(k11, k12, 0.0f);
                _K.col2 = new Vector3(k12, k22, 0.0f);

                Vector2 impulse1 = _K.Solve22(-C1);
                impulse.X = impulse1.X;
                impulse.Y = impulse1.Y;
                impulse.Z = 0.0f;
            }

            Vector2 P = impulse.X * _perp + impulse.Z * _axis;
            float L1 = impulse.X * _s1 + impulse.Y + impulse.Z * _a1;
            float L2 = impulse.X * _s2 + impulse.Y + impulse.Z * _a2;

            c1 -= InvMassA * P;
            a1 -= InvIA * L1;
            c2 += InvMassB * P;
            a2 += InvIB * L2;

            // TODO_ERIN remove need for this.
            b1._sweep.c = c1;
            b1._sweep.a = a1;
            b2._sweep.c = c2;
            b2._sweep.a = a2;
            b1.SynchronizeTransform();
            b2.SynchronizeTransform();

            return linearError <= Settings.LinearSlop && angularError <= Settings.AngularSlop;
        }

        public Vector2 LocalAnchor1;
        public Vector2 LocalAnchor2;
        public Vector2 LocalXAxis1;
        private Vector2 _localYAxis1;
        private float _refAngle;

        private Vector2 _axis;
        private Vector2 _perp;
        private float _s1, _s2;
        private float _a1, _a2;

        private Mat33 _K;
        private Vector3 _impulse;

        private float _motorMass;			// effective mass for motor/limit translational constraint.
        private float _motorImpulse;

        private float _lowerTranslation;
        private float _upperTranslation;
        private float _maxMotorForce;
        private float _motorSpeed;

        private bool _enableLimit;
        private bool _enableMotor;
        private LimitState _limitState;
    }
}
