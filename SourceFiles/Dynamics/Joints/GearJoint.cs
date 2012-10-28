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

using System.Diagnostics;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
    // Gear Joint:
    // C0 = (coordinate1 + ratio * coordinate2)_initial
    // C = (coordinate1 + ratio * coordinate2) - C0 = 0
    // J = [J1 ratio * J2]
    // K = J * invM * JT
    //   = J1 * invM1 * J1T + ratio * ratio * J2 * invM2 * J2T
    //
    // Revolute:
    // coordinate = rotation
    // Cdot = angularVelocity
    // J = [0 0 1]
    // K = J * invM * JT = invI
    //
    // Prismatic:
    // coordinate = dot(p - pg, ug)
    // Cdot = dot(v + cross(w, r), ug)
    // J = [ug cross(r, ug)]
    // K = J * invM * JT = invMass + invI * cross(r, ug)^2

    /// <summary>
    /// A gear joint is used to connect two joints together. Either joint
    /// can be a revolute or prismatic joint. You specify a gear ratio
    /// to bind the motions together:
    /// coordinate1 + ratio * coordinate2 = ant
    /// The ratio can be negative or positive. If one joint is a revolute joint
    /// and the other joint is a prismatic joint, then the ratio will have units
    /// of length or units of 1/length.
    /// @warning You have to manually destroy the gear joint if joint1 or joint2
    /// is destroyed.
    /// </summary>
    public class GearJoint : Joint
    {
        private JointType _typeA;
        private JointType _typeB;

        private Body _bodyA;
        private Body _bodyB;
        private Body _bodyC;
        private Body _bodyD;

        // Solver shared
        private Vector2 _localAnchorA;
        private Vector2 _localAnchorB;
        private Vector2 _localAnchorC;
        private Vector2 _localAnchorD;

        private Vector2 _localAxisC;
        private Vector2 _localAxisD;

        private float _referenceAngleA;
        private float _referenceAngleB;

        private float _constant;
        private float _ratio;

        private float _impulse;

        // Solver temp
        private int _indexA, _indexB, _indexC, _indexD;
        private Vector2 _lcA, _lcB, _lcC, _lcD;
        private float _mA, _mB, _mC, _mD;
        private float _iA, _iB, _iC, _iD;
        private Vector2 _JvAC, _JvBD;
        private float _JwA, _JwB, _JwC, _JwD;
        private float _mass;

        /// <summary>
        /// Requires two existing revolute or prismatic joints (any combination will work).
        /// The provided joints must attach a dynamic body to a static body.
        /// </summary>
        /// <param name="joint1">The first joint.</param>
        /// <param name="joint2">The second joint.</param>
        /// <param name="ratio">The ratio.</param>
        public GearJoint(Joint joint1, Joint joint2, float ratio = 1f)
        {
            JointType = JointType.Gear;
            Joint1 = joint1;
            Joint2 = joint2;
            Ratio = ratio;

            _typeA = joint1.JointType;
            _typeB = joint2.JointType;

            Debug.Assert(_typeA == JointType.Revolute || _typeA == JointType.Prismatic || _typeA == JointType.FixedRevolute || _typeA == JointType.FixedPrismatic);
            Debug.Assert(_typeB == JointType.Revolute || _typeB == JointType.Prismatic || _typeB == JointType.FixedRevolute || _typeB == JointType.FixedPrismatic);

            float coordinateA, coordinateB;

            // TODO_ERIN there might be some problem with the joint edges in b2Joint.

            _bodyC = Joint1.BodyA;
            _bodyA = Joint1.BodyB;

            // Get geometry of joint1
            Transform xfA = _bodyA.Xf;
            float aA = _bodyA.Sweep.A;
            Transform xfC = _bodyC.Xf;
            float aC = _bodyC.Sweep.A;

            if (_typeA == JointType.Revolute)
            {
                RevoluteJoint revolute = (RevoluteJoint)joint1;
                _localAnchorC = revolute.LocalAnchorA;
                _localAnchorA = revolute.LocalAnchorB;
                _referenceAngleA = revolute.ReferenceAngle;
                _localAxisC = Vector2.Zero;

                coordinateA = aA - aC - _referenceAngleA;
            }
            else
            {
                PrismaticJoint prismatic = (PrismaticJoint)joint1;
                _localAnchorC = prismatic.LocalAnchorA;
                _localAnchorA = prismatic.LocalAnchorB;
                _referenceAngleA = prismatic.ReferenceAngle;
                _localAxisC = prismatic.LocalXAxisA;

                Vector2 pC = _localAnchorC;
                Vector2 pA = MathUtils.MulT(xfC.q, MathUtils.Mul(xfA.q, _localAnchorA) + (xfA.p - xfC.p));
                coordinateA = Vector2.Dot(pA - pC, _localAxisC);
            }

            _bodyD = Joint2.BodyA;
            _bodyB = Joint2.BodyB;

            // Get geometry of joint2
            Transform xfB = _bodyB.Xf;
            float aB = _bodyB.Sweep.A;
            Transform xfD = _bodyD.Xf;
            float aD = _bodyD.Sweep.A;

            if (_typeB == JointType.Revolute)
            {
                RevoluteJoint revolute = (RevoluteJoint)joint2;
                _localAnchorD = revolute.LocalAnchorA;
                _localAnchorB = revolute.LocalAnchorB;
                _referenceAngleB = revolute.ReferenceAngle;
                _localAxisD = Vector2.Zero;

                coordinateB = aB - aD - _referenceAngleB;
            }
            else
            {
                PrismaticJoint prismatic = (PrismaticJoint)joint2;
                _localAnchorD = prismatic.LocalAnchorA;
                _localAnchorB = prismatic.LocalAnchorB;
                _referenceAngleB = prismatic.ReferenceAngle;
                _localAxisD = prismatic.LocalXAxisA;

                Vector2 pD = _localAnchorD;
                Vector2 pB = MathUtils.MulT(xfD.q, MathUtils.Mul(xfB.q, _localAnchorB) + (xfB.p - xfD.p));
                coordinateB = Vector2.Dot(pB - pD, _localAxisD);
            }

            _ratio = ratio;

            _constant = coordinateA + _ratio * coordinateB;

            _impulse = 0.0f;
        }

        public override Vector2 WorldAnchorA
        {
            get { return _bodyA.GetWorldPoint(_localAnchorA); }
        }

        public override Vector2 WorldAnchorB
        {
            get { return _bodyB.GetWorldPoint(_localAnchorB); }
            set { Debug.Assert(false, "You can't set the world anchor on this joint type."); }
        }

        /// <summary>
        /// The gear ratio.
        /// </summary>
        public float Ratio
        {
            get { return _ratio; }
            set
            {
                Debug.Assert(MathUtils.IsValid(value));
                _ratio = value;
            }
        }

        /// <summary>
        /// The first revolute/prismatic joint attached to the gear joint.
        /// </summary>
        public Joint Joint1 { get; set; }

        /// <summary>
        /// The second revolute/prismatic joint attached to the gear joint.
        /// </summary>
        public Joint Joint2 { get; set; }


        public override Vector2 GetReactionForce(float invDt)
        {
            Vector2 P = _impulse * _JvAC;
            return invDt * P;
        }

        public override float GetReactionTorque(float invDt)
        {
            float L = _impulse * _JwA;
            return invDt * L;
        }

        internal override void InitVelocityConstraints(ref SolverData data)
        {
            _indexA = _bodyA.IslandIndex;
            _indexB = _bodyB.IslandIndex;
            _indexC = _bodyC.IslandIndex;
            _indexD = _bodyD.IslandIndex;
            _lcA = _bodyA.Sweep.LocalCenter;
            _lcB = _bodyB.Sweep.LocalCenter;
            _lcC = _bodyC.Sweep.LocalCenter;
            _lcD = _bodyD.Sweep.LocalCenter;
            _mA = _bodyA.InvMass;
            _mB = _bodyB.InvMass;
            _mC = _bodyC.InvMass;
            _mD = _bodyD.InvMass;
            _iA = _bodyA.InvI;
            _iB = _bodyB.InvI;
            _iC = _bodyC.InvI;
            _iD = _bodyD.InvI;

            float aA = data.positions[_indexA].a;
            Vector2 vA = data.velocities[_indexA].v;
            float wA = data.velocities[_indexA].w;

            float aB = data.positions[_indexB].a;
            Vector2 vB = data.velocities[_indexB].v;
            float wB = data.velocities[_indexB].w;

            float aC = data.positions[_indexC].a;
            Vector2 vC = data.velocities[_indexC].v;
            float wC = data.velocities[_indexC].w;

            float aD = data.positions[_indexD].a;
            Vector2 vD = data.velocities[_indexD].v;
            float wD = data.velocities[_indexD].w;

            Rot qA = new Rot(aA), qB = new Rot(aB), qC = new Rot(aC), qD = new Rot(aD);

            _mass = 0.0f;

            if (_typeA == JointType.Revolute)
            {
                _JvAC = Vector2.Zero;
                _JwA = 1.0f;
                _JwC = 1.0f;
                _mass += _iA + _iC;
            }
            else
            {
                Vector2 u = MathUtils.Mul(qC, _localAxisC);
                Vector2 rC = MathUtils.Mul(qC, _localAnchorC - _lcC);
                Vector2 rA = MathUtils.Mul(qA, _localAnchorA - _lcA);
                _JvAC = u;
                _JwC = MathUtils.Cross(rC, u);
                _JwA = MathUtils.Cross(rA, u);
                _mass += _mC + _mA + _iC * _JwC * _JwC + _iA * _JwA * _JwA;
            }

            if (_typeB == JointType.Revolute)
            {
                _JvBD = Vector2.Zero;
                _JwB = _ratio;
                _JwD = _ratio;
                _mass += _ratio * _ratio * (_iB + _iD);
            }
            else
            {
                Vector2 u = MathUtils.Mul(qD, _localAxisD);
                Vector2 rD = MathUtils.Mul(qD, _localAnchorD - _lcD);
                Vector2 rB = MathUtils.Mul(qB, _localAnchorB - _lcB);
                _JvBD = _ratio * u;
                _JwD = _ratio * MathUtils.Cross(rD, u);
                _JwB = _ratio * MathUtils.Cross(rB, u);
                _mass += _ratio * _ratio * (_mD + _mB) + _iD * _JwD * _JwD + _iB * _JwB * _JwB;
            }

            // Compute effective mass.
            _mass = _mass > 0.0f ? 1.0f / _mass : 0.0f;

            if (Settings.EnableWarmstarting)
            {
                vA += (_mA * _impulse) * _JvAC;
                wA += _iA * _impulse * _JwA;
                vB += (_mB * _impulse) * _JvBD;
                wB += _iB * _impulse * _JwB;
                vC -= (_mC * _impulse) * _JvAC;
                wC -= _iC * _impulse * _JwC;
                vD -= (_mD * _impulse) * _JvBD;
                wD -= _iD * _impulse * _JwD;
            }
            else
            {
                _impulse = 0.0f;
            }

            data.velocities[_indexA].v = vA;
            data.velocities[_indexA].w = wA;
            data.velocities[_indexB].v = vB;
            data.velocities[_indexB].w = wB;
            data.velocities[_indexC].v = vC;
            data.velocities[_indexC].w = wC;
            data.velocities[_indexD].v = vD;
            data.velocities[_indexD].w = wD;
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            Vector2 vA = data.velocities[_indexA].v;
            float wA = data.velocities[_indexA].w;
            Vector2 vB = data.velocities[_indexB].v;
            float wB = data.velocities[_indexB].w;
            Vector2 vC = data.velocities[_indexC].v;
            float wC = data.velocities[_indexC].w;
            Vector2 vD = data.velocities[_indexD].v;
            float wD = data.velocities[_indexD].w;

            float Cdot = Vector2.Dot(_JvAC, vA - vC) + Vector2.Dot(_JvBD, vB - vD);
            Cdot += (_JwA * wA - _JwC * wC) + (_JwB * wB - _JwD * wD);

            float impulse = -_mass * Cdot;
            _impulse += impulse;

            vA += (_mA * impulse) * _JvAC;
            wA += _iA * impulse * _JwA;
            vB += (_mB * impulse) * _JvBD;
            wB += _iB * impulse * _JwB;
            vC -= (_mC * impulse) * _JvAC;
            wC -= _iC * impulse * _JwC;
            vD -= (_mD * impulse) * _JvBD;
            wD -= _iD * impulse * _JwD;

            data.velocities[_indexA].v = vA;
            data.velocities[_indexA].w = wA;
            data.velocities[_indexB].v = vB;
            data.velocities[_indexB].w = wB;
            data.velocities[_indexC].v = vC;
            data.velocities[_indexC].w = wC;
            data.velocities[_indexD].v = vD;
            data.velocities[_indexD].w = wD;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            Vector2 cA = data.positions[_indexA].c;
            float aA = data.positions[_indexA].a;
            Vector2 cB = data.positions[_indexB].c;
            float aB = data.positions[_indexB].a;
            Vector2 cC = data.positions[_indexC].c;
            float aC = data.positions[_indexC].a;
            Vector2 cD = data.positions[_indexD].c;
            float aD = data.positions[_indexD].a;

            Rot qA = new Rot(aA), qB = new Rot(aB), qC = new Rot(aC), qD = new Rot(aD);

            const float linearError = 0.0f;

            float coordinateA, coordinateB;

            Vector2 JvAC, JvBD;
            float JwA, JwB, JwC, JwD;
            float mass = 0.0f;

            if (_typeA == JointType.Revolute)
            {
                JvAC = Vector2.Zero;
                JwA = 1.0f;
                JwC = 1.0f;
                mass += _iA + _iC;

                coordinateA = aA - aC - _referenceAngleA;
            }
            else
            {
                Vector2 u = MathUtils.Mul(qC, _localAxisC);
                Vector2 rC = MathUtils.Mul(qC, _localAnchorC - _lcC);
                Vector2 rA = MathUtils.Mul(qA, _localAnchorA - _lcA);
                JvAC = u;
                JwC = MathUtils.Cross(rC, u);
                JwA = MathUtils.Cross(rA, u);
                mass += _mC + _mA + _iC * JwC * JwC + _iA * JwA * JwA;

                Vector2 pC = _localAnchorC - _lcC;
                Vector2 pA = MathUtils.MulT(qC, rA + (cA - cC));
                coordinateA = Vector2.Dot(pA - pC, _localAxisC);
            }

            if (_typeB == JointType.Revolute)
            {
                JvBD = Vector2.Zero;
                JwB = _ratio;
                JwD = _ratio;
                mass += _ratio * _ratio * (_iB + _iD);

                coordinateB = aB - aD - _referenceAngleB;
            }
            else
            {
                Vector2 u = MathUtils.Mul(qD, _localAxisD);
                Vector2 rD = MathUtils.Mul(qD, _localAnchorD - _lcD);
                Vector2 rB = MathUtils.Mul(qB, _localAnchorB - _lcB);
                JvBD = _ratio * u;
                JwD = _ratio * MathUtils.Cross(rD, u);
                JwB = _ratio * MathUtils.Cross(rB, u);
                mass += _ratio * _ratio * (_mD + _mB) + _iD * JwD * JwD + _iB * JwB * JwB;

                Vector2 pD = _localAnchorD - _lcD;
                Vector2 pB = MathUtils.MulT(qD, rB + (cB - cD));
                coordinateB = Vector2.Dot(pB - pD, _localAxisD);
            }

            float C = (coordinateA + _ratio * coordinateB) - _constant;

            float impulse = 0.0f;
            if (mass > 0.0f)
            {
                impulse = -C / mass;
            }

            cA += _mA * impulse * JvAC;
            aA += _iA * impulse * JwA;
            cB += _mB * impulse * JvBD;
            aB += _iB * impulse * JwB;
            cC -= _mC * impulse * JvAC;
            aC -= _iC * impulse * JwC;
            cD -= _mD * impulse * JvBD;
            aD -= _iD * impulse * JwD;

            data.positions[_indexA].c = cA;
            data.positions[_indexA].a = aA;
            data.positions[_indexB].c = cB;
            data.positions[_indexB].a = aB;
            data.positions[_indexC].c = cC;
            data.positions[_indexC].a = aC;
            data.positions[_indexD].c = cD;
            data.positions[_indexD].a = aD;

            // TODO_ERIN not implemented
            return linearError < Settings.LinearSlop;
        }
    }
}