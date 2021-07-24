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
    // p = attached point, m = mouse point
    // C = p - m
    // Cdot = v
    //      = v + cross(w, r)
    // J = [I r_skew]
    // Identity used:
    // w k % (rx i + ry j) = w * (-ry i + rx j)

    /// <summary>A mouse joint is used to make a point on a body track a specified world point. This a soft constraint with a
    /// maximum force. This allows the constraint to stretch and without applying huge forces. NOTE: this joint is not
    /// documented in the manual because it was developed to be used in the testbed. If you want to learn how to use the mouse
    /// joint, look at the testbed.</summary>
    public class FixedMouseJoint : Joint
    {
        private Vector2 _localAnchorA;
        private Vector2 _targetB;
        private float _stiffness;
        private float _damping;
        private float _beta;

        // Solver shared
        private Vector2 _impulse;
        private float _maxForce;
        private float _gamma;

        // Solver temp
        private int _indexA;
        private Vector2 _rA;
        private Vector2 _localCenterA;
        private float _invMassA;
        private float _invIA;
        private Mat22 _mass;
        private Vector2 _C;

        public FixedMouseJoint(FixedMouseJointDef def) : base(def)
        {
            _targetB = def.Target;
            _localAnchorA = MathUtils.MulT(_bodyB._xf, _targetB);
            _maxForce = def.MaxForce;
            _stiffness = def.Stiffness;
            _damping = def.Damping;
        }

        /// <summary>This requires a world target point, tuning parameters, and the time step.</summary>
        /// <param name="body">The body.</param>
        /// <param name="target">The target.</param>
        public FixedMouseJoint(Body body, Vector2 target)
            : base(body, JointType.FixedMouse)
        {
            _targetB = target;
            _localAnchorA = MathUtils.MulT(_bodyA._xf, _targetB);
        }

        /// <summary>The local anchor point on BodyB</summary>
        public Vector2 LocalAnchorA
        {
            get => _localAnchorA;
            set => _localAnchorA = value;
        }

        /// <summary>Use this to update the target point.</summary>
        public override Vector2 WorldAnchorA
        {
            get => _bodyA.GetWorldPoint(_localAnchorA);
            set => _localAnchorA = _bodyA.GetLocalPoint(value);
        }

        public override Vector2 WorldAnchorB
        {
            get => _targetB;
            set
            {
                if (_targetB != value)
                {
                    _bodyA.Awake = true;
                    _targetB = value;
                }
            }
        }

        /// <summary>The maximum constraint force that can be exerted to move the candidate body. Usually you will express as some
        /// multiple of the weight (multiplier * mass * gravity). Set/get the maximum force in Newtons.</summary>
        public float MaxForce
        {
            get => _maxForce;
            set => _maxForce = value;
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

        public override void ShiftOrigin(ref Vector2 newOrigin)
        {
            _targetB -= newOrigin;
        }

        public override Vector2 GetReactionForce(float invDt)
        {
            return invDt * _impulse;
        }

        public override float GetReactionTorque(float invDt)
        {
            return 0.0f;
        }

        internal override void InitVelocityConstraints(ref SolverData data)
        {
            _indexA = _bodyA.IslandIndex;
            _localCenterA = _bodyA._sweep.LocalCenter;
            _invMassA = _bodyA._invMass;
            _invIA = _bodyA._invI;

            Vector2 cA = data.Positions[_indexA].C;
            float aA = data.Positions[_indexA].A;
            Vector2 vA = data.Velocities[_indexA].V;
            float wA = data.Velocities[_indexA].W;

            Rot qA = new Rot(aA);

            float d = _damping;
            float k = _stiffness;

            // magic formulas
            // gamma has units of inverse mass.
            // beta has units of inverse time.
            float h = data.Step.DeltaTime;
            _gamma = h * (d + h * k);
            if (_gamma != 0.0f)
                _gamma = 1.0f / _gamma;

            _beta = h * k * _gamma;

            // Compute the effective mass matrix.
            _rA = MathUtils.Mul(qA, _localAnchorA - _localCenterA);

            // K    = [(1/m1 + 1/m2) * eye(2) - skew(r1) * invI1 * skew(r1) - skew(r2) * invI2 * skew(r2)]
            //      = [1/m1+1/m2     0    ] + invI1 * [r1.y*r1.y -r1.x*r1.y] + invI2 * [r1.y*r1.y -r1.x*r1.y]
            //        [    0     1/m1+1/m2]           [-r1.x*r1.y r1.x*r1.x]           [-r1.x*r1.y r1.x*r1.x]
            Mat22 K = new Mat22();
            K.ex.X = _invMassA + _invIA * _rA.Y * _rA.Y + _gamma;
            K.ex.Y = -_invIA * _rA.X * _rA.Y;
            K.ey.X = K.ex.Y;
            K.ey.Y = _invMassA + _invIA * _rA.X * _rA.X + _gamma;

            _mass = K.Inverse;

            _C = cA + _rA - _targetB;
            _C *= _beta;

            // Cheat with some damping
            wA *= 0.98f;

            if (data.Step.WarmStarting)
            {
                _impulse *= data.Step.DeltaTimeRatio;
                vA += _invMassA * _impulse;
                wA += _invIA * MathUtils.Cross(_rA, _impulse);
            }
            else
                _impulse = Vector2.Zero;

            data.Velocities[_indexA].V = vA;
            data.Velocities[_indexA].W = wA;
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            Vector2 vA = data.Velocities[_indexA].V;
            float wA = data.Velocities[_indexA].W;

            // Cdot = v + cross(w, r)
            Vector2 Cdot = vA + MathUtils.Cross(wA, _rA);
            Vector2 impulse = MathUtils.Mul(ref _mass, -(Cdot + _C + _gamma * _impulse));

            Vector2 oldImpulse = _impulse;
            _impulse += impulse;
            float maxImpulse = data.Step.DeltaTime * _maxForce;
            if (_impulse.LengthSquared() > maxImpulse * maxImpulse)
                _impulse *= maxImpulse / _impulse.Length();

            impulse = _impulse - oldImpulse;

            vA += _invMassA * impulse;
            wA += _invIA * MathUtils.Cross(_rA, impulse);

            data.Velocities[_indexA].V = vA;
            data.Velocities[_indexA].W = wA;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            return true;
        }
    }
}