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
    public class MotorJoint : Joint
    {
        // Solver shared
        private Vector2 _linearOffset;
        private float _angularOffset;
        private Vector2 _linearImpulse;
        private float _angularImpulse;
        private float _maxForce;
        private float _maxTorque;
        private float _correctionFactor;

        // Solver temp
        private int _indexA;
        private int _indexB;
        private Vector2 _rA;
        private Vector2 _rB;
        private Vector2 _localCenterA;
        private Vector2 _localCenterB;
        private Vector2 _linearError;
        private float _angularError;
        private float _invMassA;
        private float _invMassB;
        private float _invIA;
        private float _invIB;
        private Mat22 _linearMass;
        private float _angularMass;

        internal MotorJoint()
        {
            JointType = JointType.Motor;
        }

        public MotorJoint(Body bA, Body bB)
            : base(bA, bB)
        {
            JointType = JointType.Motor;

            Vector2 xB = BodyA.Position;
            _linearOffset = BodyA.GetLocalPoint(xB);

            //Defaults
            _angularOffset = 0.0f;
            _maxForce = 1.0f;
            _maxTorque = 1.0f;
            _correctionFactor = 0.3f;

            float angleA = BodyA.Rotation;
            float angleB = BodyB.Rotation;
            _angularOffset = angleB - angleA;
        }

        public override Vector2 WorldAnchorA
        {
            get { return BodyA.Position; }
        }

        public override Vector2 WorldAnchorB
        {
            get { return BodyB.Position; }
            set { Debug.Assert(false, "You can't set the world anchor on this joint type."); }
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

            Vector2 cA = data.positions[_indexA].c;
            float aA = data.positions[_indexA].a;
            Vector2 vA = data.velocities[_indexA].v;
            float wA = data.velocities[_indexA].w;

            Vector2 cB = data.positions[_indexB].c;
            float aB = data.positions[_indexB].a;
            Vector2 vB = data.velocities[_indexB].v;
            float wB = data.velocities[_indexB].w;

            Rot qA = new Rot(aA);
            Rot qB = new Rot(aB);

            // Compute the effective mass matrix.
            _rA = MathUtils.Mul(qA, -_localCenterA);
            _rB = MathUtils.Mul(qB, -_localCenterB);

            // J = [-I -r1_skew I r2_skew]
            //     [ 0       -1 0       1]
            // r_skew = [-ry; rx]

            // Matlab
            // K = [ mA+r1y^2*iA+mB+r2y^2*iB,  -r1y*iA*r1x-r2y*iB*r2x,          -r1y*iA-r2y*iB]
            //     [  -r1y*iA*r1x-r2y*iB*r2x, mA+r1x^2*iA+mB+r2x^2*iB,           r1x*iA+r2x*iB]
            //     [          -r1y*iA-r2y*iB,           r1x*iA+r2x*iB,                   iA+iB]

            float mA = _invMassA, mB = _invMassB;
            float iA = _invIA, iB = _invIB;

            Mat22 K = new Mat22();
            K.ex.X = mA + mB + iA * _rA.Y * _rA.Y + iB * _rB.Y * _rB.Y;
            K.ex.Y = -iA * _rA.X * _rA.Y - iB * _rB.X * _rB.Y;
            K.ey.X = K.ex.Y;
            K.ey.Y = mA + mB + iA * _rA.X * _rA.X + iB * _rB.X * _rB.X;

            _linearMass = K.Inverse;

            _angularMass = iA + iB;
            if (_angularMass > 0.0f)
            {
                _angularMass = 1.0f / _angularMass;
            }

            _linearError = cB + _rB - cA - _rA - MathUtils.Mul(qA, _linearOffset);
            _angularError = aB - aA - _angularOffset;

            if (Settings.EnableWarmstarting)
            {
                // Scale impulses to support a variable time step.
                _linearImpulse *= data.step.dtRatio;
                _angularImpulse *= data.step.dtRatio;

                Vector2 P = new Vector2(_linearImpulse.X, _linearImpulse.Y);

                vA -= mA * P;
                wA -= iA * (MathUtils.Cross(_rA, P) + _angularImpulse);
                vB += mB * P;
                wB += iB * (MathUtils.Cross(_rB, P) + _angularImpulse);
            }
            else
            {
                _linearImpulse = Vector2.Zero;
                _angularImpulse = 0.0f;
            }

            data.velocities[_indexA].v = vA;
            data.velocities[_indexA].w = wA;
            data.velocities[_indexB].v = vB;
            data.velocities[_indexB].w = wB;
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            Vector2 vA = data.velocities[_indexA].v;
            float wA = data.velocities[_indexA].w;
            Vector2 vB = data.velocities[_indexB].v;
            float wB = data.velocities[_indexB].w;

            float mA = _invMassA, mB = _invMassB;
            float iA = _invIA, iB = _invIB;

            float h = data.step.dt;
            float inv_h = data.step.inv_dt;

            // Solve angular friction
            {
                float Cdot = wB - wA + inv_h * _correctionFactor * _angularError;
                float impulse = -_angularMass * Cdot;

                float oldImpulse = _angularImpulse;
                float maxImpulse = h * _maxTorque;
                _angularImpulse = MathUtils.Clamp(_angularImpulse + impulse, -maxImpulse, maxImpulse);
                impulse = _angularImpulse - oldImpulse;

                wA -= iA * impulse;
                wB += iB * impulse;
            }

            // Solve linear friction
            {
                Vector2 Cdot = vB + MathUtils.Cross(wB, _rB) - vA - MathUtils.Cross(wA, _rA) + inv_h * _correctionFactor * _linearError;

                Vector2 impulse = -MathUtils.Mul(ref _linearMass, ref Cdot);
                Vector2 oldImpulse = _linearImpulse;
                _linearImpulse += impulse;

                float maxImpulse = h * _maxForce;

                if (_linearImpulse.LengthSquared() > maxImpulse * maxImpulse)
                {
                    _linearImpulse.Normalize();
                    _linearImpulse *= maxImpulse;
                }

                impulse = _linearImpulse - oldImpulse;

                vA -= mA * impulse;
                wA -= iA * MathUtils.Cross(_rA, impulse);

                vB += mB * impulse;
                wB += iB * MathUtils.Cross(_rB, impulse);
            }

            data.velocities[_indexA].v = vA;
            data.velocities[_indexA].w = wA;
            data.velocities[_indexB].v = vB;
            data.velocities[_indexB].w = wB;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            return true;
        }

        public override Vector2 GetReactionForce(float invDt)
        {
            return invDt * _linearImpulse;
        }

        public override float GetReactionTorque(float invDt)
        {
            return invDt * _angularImpulse;
        }

        public float MaxForce
        {
            set
            {
                Debug.Assert(MathUtils.IsValid(value) && value >= 0.0f);
                _maxForce = value;
            }
            get { return _maxForce; }
        }

        public float MaxTorque
        {
            set
            {
                Debug.Assert(MathUtils.IsValid(value) && value >= 0.0f);
                _maxTorque = value;
            }
            get { return _maxTorque; }
        }

        public Vector2 LinearOffset
        {
            set
            {
                if (value.X != _linearOffset.X || value.Y != _linearOffset.Y)
                {
                    BodyA.Awake = true;
                    BodyB.Awake = true;
                    _linearOffset = value;
                }
            }
            get { return _linearOffset; }
        }

        public float AngularOffset
        {
            set
            {
                if (value != _angularOffset)
                {
                    BodyA.Awake = true;
                    BodyB.Awake = true;
                    _angularOffset = value;
                }
            }
            get { return _angularOffset; }
        }
    }
}