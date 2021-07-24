/*
* Velcro Physics:
* Copyright (c) 2021 Ian Qvist
* 
* Original source Box2D:
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
    // 1-D constrained system
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

    /// <summary>
    /// A distance joint constrains two points on two bodies to remain at a fixed distance from each other. You can
    /// view this as a massless, rigid rod.
    /// </summary>
    public class DistanceJoint : Joint
    {
        private float _bias;
        private float _currentLength;
        private float _gamma;
        private float _impulse;

        // Solver temp
        private int _indexA;
        private int _indexB;
        private float _invIA;
        private float _invIB;
        private float _invMassA;
        private float _invMassB;
        private float _length;

        // Solver shared
        private Vector2 _localCenterA;
        private Vector2 _localCenterB;
        private float _lowerImpulse;
        private float _mass;
        private float _maxLength;
        private float _minLength;
        private Vector2 _rA;
        private Vector2 _rB;
        private float _softMass;
        private Vector2 _u;
        private float _upperImpulse;
        private float _stiffness;
        private float _damping;
        private Vector2 _localAnchorA;
        private Vector2 _localAnchorB;

        public DistanceJoint(DistanceJointDef def) : base(def)
        {
            _localAnchorA = def.LocalAnchorA;
            _localAnchorB = def.LocalAnchorB;
            _length = MathUtils.Max(def.Length, Settings.LinearSlop);
            _minLength = MathUtils.Max(def.MinLength, Settings.LinearSlop);
            _maxLength = MathUtils.Max(def.MaxLength, _minLength);
            _stiffness = def.Stiffness;
            _damping = def.Damping;
        }

        /// <summary>
        /// This requires defining an anchor point on both bodies and the non-zero length of the distance joint. If you
        /// don't supply a length, the local anchor points is used so that the initial configuration can violate the constraint
        /// slightly. This helps when saving and loading a game. Warning Do not use a zero or short length.
        /// </summary>
        /// <param name="bodyA">The first body</param>
        /// <param name="bodyB">The second body</param>
        /// <param name="anchorA">The first body anchor</param>
        /// <param name="anchorB">The second body anchor</param>
        /// <param name="useWorldCoordinates">Set to true if you are using world coordinates as anchors.</param>
        public DistanceJoint(Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB, bool useWorldCoordinates = false)
            : base(bodyA, bodyB, JointType.Distance)
        {
            Vector2 d;

            if (useWorldCoordinates)
            {
                _localAnchorA = bodyA.GetLocalPoint(ref anchorA);
                _localAnchorB = bodyB.GetLocalPoint(ref anchorB);

                d = anchorB - anchorA;
            }
            else
            {
                _localAnchorA = anchorA;
                _localAnchorB = anchorB;

                d = bodyB.GetWorldPoint(ref anchorB) - bodyA.GetWorldPoint(ref anchorA);
            }

            _length = MathUtils.Max(d.Length(), Settings.LinearSlop);
            _minLength = _length;
            _maxLength = _length;
        }

        /// <summary>The local anchor point relative to bodyA's origin.</summary>
        public Vector2 LocalAnchorA
        {
            get => _localAnchorA;
            set => _localAnchorA = value;
        }

        /// <summary>The local anchor point relative to bodyB's origin.</summary>
        public Vector2 LocalAnchorB
        {
            get => _localAnchorB;
            set => _localAnchorB = value;
        }

        /// <summary>The anchor on <see cref="Joint.BodyA" /> in world coordinates</summary>
        public sealed override Vector2 WorldAnchorA
        {
            get => _bodyA.GetWorldPoint(_localAnchorA);
            set => Debug.Assert(false, "You can't set the world anchor on this joint type.");
        }

        /// <summary>The anchor on <see cref="Joint.BodyB" /> in world coordinates</summary>
        public sealed override Vector2 WorldAnchorB
        {
            get => _bodyB.GetWorldPoint(_localAnchorB);
            set => Debug.Assert(false, "You can't set the world anchor on this joint type.");
        }

        /// <summary>The rest length of this joint. Clamped to a stable minimum value.</summary>
        public float Length
        {
            get
            {
                Vector2 pA = _bodyA.GetWorldPoint(_localAnchorA);
                Vector2 pB = _bodyB.GetWorldPoint(_localAnchorB);
                Vector2 d = pB - pA;
                float length = d.Length();
                return length;
            }
            set
            {
                _impulse = 0.0f;
                _length = MathUtils.Max(Settings.LinearSlop, value);
            }
        }

        /// <summary>Set/get the linear stiffness in N/m</summary>
        public float Stiffness
        {
            get => _stiffness;
            set => _stiffness = value;
        }

        /// <summary>Set/get linear damping in N*s/m</summary>
        public float Damping
        {
            get => _damping;
            set => _damping = value;
        }

        /// <summary>Minimum length. Clamped to a stable minimum value.</summary>
        public float MinLength
        {
            get => _minLength;
            set
            {
                _lowerImpulse = 0.0f;
                _minLength = MathUtils.Clamp(value, Settings.LinearSlop, _maxLength);
            }
        }

        /// <summary>Maximum length. Must be greater than or equal to the minimum length.</summary>
        public float MaxLength
        {
            get => _maxLength;
            set
            {
                _upperImpulse = 0.0f;
                _maxLength = MathUtils.Max(value, _minLength);
            }
        }

        /// <summary>Get the reaction force given the inverse time step. Unit is N.</summary>
        public override Vector2 GetReactionForce(float invDt)
        {
            Vector2 F = invDt * (_impulse + _lowerImpulse - _upperImpulse) * _u;
            return F;
        }

        /// <summary>Get the reaction torque given the inverse time step. Unit is N*m. This is always zero for a distance joint.</summary>
        public override float GetReactionTorque(float invDt)
        {
            return 0.0f;
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

            Vector2 cA = data.Positions[_indexA].C;
            float aA = data.Positions[_indexA].A;
            Vector2 vA = data.Velocities[_indexA].V;
            float wA = data.Velocities[_indexA].W;

            Vector2 cB = data.Positions[_indexB].C;
            float aB = data.Positions[_indexB].A;
            Vector2 vB = data.Velocities[_indexB].V;
            float wB = data.Velocities[_indexB].W;

            Rot qA = new Rot(aA), qB = new Rot(aB);

            _rA = MathUtils.Mul(ref qA, _localAnchorA - _localCenterA);
            _rB = MathUtils.Mul(ref qB, _localAnchorB - _localCenterB);
            _u = cB + _rB - cA - _rA;

            // Handle singularity.
            _currentLength = _u.Length();
            if (_currentLength > Settings.LinearSlop)
                _u *= 1.0f / _currentLength;
            else
            {
                _u = Vector2.Zero;
                _mass = 0.0f;
                _impulse = 0.0f;
                _lowerImpulse = 0.0f;
                _upperImpulse = 0.0f;
            }

            float crAu = MathUtils.Cross(_rA, _u);
            float crBu = MathUtils.Cross(_rB, _u);
            float invMass = _invMassA + _invIA * crAu * crAu + _invMassB + _invIB * crBu * crBu;
            _mass = invMass != 0.0f ? 1.0f / invMass : 0.0f;

            if (_stiffness > 0.0f && _minLength < _maxLength)
            {
                // soft
                float C = _currentLength - _length;

                float d = _damping;
                float k = _stiffness;

                // magic formulas
                float h = data.Step.DeltaTime;

                // gamma = 1 / (h * (d + h * k))
                // the extra factor of h in the denominator is since the lambda is an impulse, not a force
                _gamma = h * (d + h * k);
                _gamma = _gamma != 0.0f ? 1.0f / _gamma : 0.0f;
                _bias = C * h * k * _gamma;

                invMass += _gamma;
                _softMass = invMass != 0.0f ? 1.0f / invMass : 0.0f;
            }
            else
            {
                // rigid
                _gamma = 0.0f;
                _bias = 0.0f;
                _softMass = _mass;
            }

            if (data.Step.WarmStarting)
            {
                // Scale the impulse to support a variable time step.
                _impulse *= data.Step.DeltaTimeRatio;
                _lowerImpulse *= data.Step.DeltaTimeRatio;
                _upperImpulse *= data.Step.DeltaTimeRatio;

                Vector2 P = (_impulse + _lowerImpulse - _upperImpulse) * _u;
                vA -= _invMassA * P;
                wA -= _invIA * MathUtils.Cross(ref _rA, ref P);
                vB += _invMassB * P;
                wB += _invIB * MathUtils.Cross(ref _rB, ref P);
            }
            else
                _impulse = 0.0f;

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

            if (_minLength < _maxLength)
            {
                if (_stiffness > 0.0f)
                {
                    // Cdot = dot(u, v + cross(w, r))
                    Vector2 vpA = vA + MathUtils.Cross(wA, _rA);
                    Vector2 vpB = vB + MathUtils.Cross(wB, _rB);
                    float Cdot = MathUtils.Dot(_u, vpB - vpA);

                    float impulse = -_softMass * (Cdot + _bias + _gamma * _impulse);
                    _impulse += impulse;

                    Vector2 P = impulse * _u;
                    vA -= _invMassA * P;
                    wA -= _invIA * MathUtils.Cross(_rA, P);
                    vB += _invMassB * P;
                    wB += _invIB * MathUtils.Cross(_rB, P);
                }

                // lower
                {
                    float C = _currentLength - _minLength;
                    float bias = MathUtils.Max(0.0f, C) * data.Step.InvertedDeltaTime;

                    Vector2 vpA = vA + MathUtils.Cross(wA, _rA);
                    Vector2 vpB = vB + MathUtils.Cross(wB, _rB);
                    float Cdot = MathUtils.Dot(_u, vpB - vpA);

                    float impulse = -_mass * (Cdot + bias);
                    float oldImpulse = _lowerImpulse;
                    _lowerImpulse = MathUtils.Max(0.0f, _lowerImpulse + impulse);
                    impulse = _lowerImpulse - oldImpulse;
                    Vector2 P = impulse * _u;

                    vA -= _invMassA * P;
                    wA -= _invIA * MathUtils.Cross(_rA, P);
                    vB += _invMassB * P;
                    wB += _invIB * MathUtils.Cross(_rB, P);
                }

                // upper
                {
                    float C = _maxLength - _currentLength;
                    float bias = MathUtils.Max(0.0f, C) * data.Step.InvertedDeltaTime;

                    Vector2 vpA = vA + MathUtils.Cross(wA, _rA);
                    Vector2 vpB = vB + MathUtils.Cross(wB, _rB);
                    float Cdot = MathUtils.Dot(_u, vpA - vpB);

                    float impulse = -_mass * (Cdot + bias);
                    float oldImpulse = _upperImpulse;
                    _upperImpulse = MathUtils.Max(0.0f, _upperImpulse + impulse);
                    impulse = _upperImpulse - oldImpulse;
                    Vector2 P = -impulse * _u;

                    vA -= _invMassA * P;
                    wA -= _invIA * MathUtils.Cross(_rA, P);
                    vB += _invMassB * P;
                    wB += _invIB * MathUtils.Cross(_rB, P);
                }
            }
            else
            {
                // Equal limits

                // Cdot = dot(u, v + cross(w, r))
                Vector2 vpA = vA + MathUtils.Cross(wA, _rA);
                Vector2 vpB = vB + MathUtils.Cross(wB, _rB);
                float Cdot = MathUtils.Dot(_u, vpB - vpA);

                float impulse = -_mass * Cdot;
                _impulse += impulse;

                Vector2 P = impulse * _u;
                vA -= _invMassA * P;
                wA -= _invIA * MathUtils.Cross(_rA, P);
                vB += _invMassB * P;
                wB += _invIB * MathUtils.Cross(_rB, P);
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

            Vector2 rA = MathUtils.Mul(qA, _localAnchorA - _localCenterA);
            Vector2 rB = MathUtils.Mul(qB, _localAnchorB - _localCenterB);
            Vector2 u = cB + rB - cA - rA;

            float length = MathUtils.Normalize(ref u);
            float C;
            if (_minLength == _maxLength)
                C = length - _minLength;
            else if (length < _minLength)
                C = length - _minLength;
            else if (_maxLength < length)
                C = length - _maxLength;
            else
                return true;

            float impulse = -_mass * C;
            Vector2 P = impulse * u;

            cA -= _invMassA * P;
            aA -= _invIA * MathUtils.Cross(rA, P);
            cB += _invMassB * P;
            aB += _invIB * MathUtils.Cross(rB, P);

            data.Positions[_indexA].C = cA;
            data.Positions[_indexA].A = aA;
            data.Positions[_indexB].C = cB;
            data.Positions[_indexB].A = aB;

            return MathUtils.Abs(C) < Settings.LinearSlop;
        }
    }
}