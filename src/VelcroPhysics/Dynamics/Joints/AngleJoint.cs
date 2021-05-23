/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
*/

using System.Diagnostics;
using Genbox.VelcroPhysics.Dynamics.Joints.Misc;
using Genbox.VelcroPhysics.Dynamics.Solver;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Dynamics.Joints
{
    /// <summary>Maintains a fixed angle between two bodies</summary>
    public class AngleJoint : Joint
    {
        private float _bias;
        private float _jointError;
        private float _massFactor;
        private float _targetAngle;
        private float _biasFactor;
        private float _maxImpulse;
        private float _softness;

        public AngleJoint(Body bodyA, Body bodyB)
            : base(bodyA, bodyB, JointType.Angle)
        {
            _biasFactor = .2f;
            _maxImpulse = float.MaxValue;
        }

        public override Vector2 WorldAnchorA
        {
            get => _bodyA.Position;
            set => Debug.Assert(false, "You can't set the world anchor on this joint type.");
        }

        public override Vector2 WorldAnchorB
        {
            get => _bodyB.Position;
            set => Debug.Assert(false, "You can't set the world anchor on this joint type.");
        }

        /// <summary>The desired angle between BodyA and BodyB</summary>
        public float TargetAngle
        {
            get => _targetAngle;
            set
            {
                if (_targetAngle != value)
                {
                    _targetAngle = value;
                    WakeBodies();
                }
            }
        }

        /// <summary>Gets or sets the bias factor. Defaults to 0.2</summary>
        public float BiasFactor
        {
            get => _biasFactor;
            set => _biasFactor = value;
        }

        /// <summary>Gets or sets the maximum impulse. Defaults to float.MaxValue</summary>
        public float MaxImpulse
        {
            get => _maxImpulse;
            set => _maxImpulse = value;
        }

        /// <summary>Gets or sets the softness of the joint. Defaults to 0</summary>
        public float Softness
        {
            get => _softness;
            set => _softness = value;
        }

        public override Vector2 GetReactionForce(float invDt)
        {
            return Vector2.Zero;
        }

        public override float GetReactionTorque(float invDt)
        {
            return 0;
        }

        internal override void InitVelocityConstraints(ref SolverData data)
        {
            int indexA = _bodyA.IslandIndex;
            int indexB = _bodyB.IslandIndex;

            float aW = data.Positions[indexA].A;
            float bW = data.Positions[indexB].A;

            _jointError = bW - aW - _targetAngle;
            _bias = -_biasFactor * data.Step.InvertedDeltaTime * _jointError;
            _massFactor = (1 - _softness) / (_bodyA._invI + _bodyB._invI);
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            int indexA = _bodyA.IslandIndex;
            int indexB = _bodyB.IslandIndex;

            float p = (_bias - data.Velocities[indexB].W + data.Velocities[indexA].W) * _massFactor;

            data.Velocities[indexA].W -= _bodyA._invI * MathUtils.Sign(p) * MathUtils.Min(MathUtils.Abs(p), _maxImpulse);
            data.Velocities[indexB].W += _bodyB._invI * MathUtils.Sign(p) * MathUtils.Min(MathUtils.Abs(p), _maxImpulse);
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            //no position solving for this joint
            return true;
        }
    }
}