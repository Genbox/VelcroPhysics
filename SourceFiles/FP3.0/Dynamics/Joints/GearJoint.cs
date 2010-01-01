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
using System.Diagnostics;

namespace FarseerPhysics
{
    /// <summary>
    /// Gear joint definition. This definition requires two existing
    /// revolute or prismatic joints (any combination will work).
    /// The provided joints must attach a dynamic body to a static body.
    /// </summary>
    public class GearJointDef : JointDef
    {
        public GearJointDef()
        {
            Type = JointType.Gear;
            Joint1 = null;
            Joint2 = null;
            Ratio = 1.0f;
        }

        /// <summary>
        /// The first revolute/prismatic joint attached to the gear joint.
        /// </summary>
        public Joint Joint1;

        /// <summary>
        /// The second revolute/prismatic joint attached to the gear joint.
        /// </summary>
        public Joint Joint2;

        /// <summary>
        /// The gear ratio.
        /// @see [GearJoint] for explanation.
        /// </summary>
        public float Ratio;
    }

    /// <summary>
    /// A gear joint is used to connect two joints together. Either joint
    /// can be a revolute or prismatic joint. You specify a gear ratio
    /// to bind the motions together:
    /// coordinate1 + ratio * coordinate2 = ant
    /// The ratio can be negative or positive. If one joint is a revolute joint
    /// and the other joint is a prismatic joint, then the ratio will have units
    /// of length or units of 1/length.
    /// @warning The revolute and prismatic joints must be attached to
    /// fixed bodies (which must be body1 on those joints).
    /// </summary>
    public class GearJoint : Joint
    {
        public override Vector2 GetAnchorA()
        {
            return BodyA.GetWorldPoint(_localAnchor1);
        }

        public override Vector2 GetAnchorB()
        {
            return BodyB.GetWorldPoint(_localAnchor2);
        }

        public override Vector2 GetReactionForce(float inv_dt)
        {
            Vector2 P = _impulse * _J.LinearB;
            return inv_dt * P;
        }

        public override float GetReactionTorque(float inv_dt)
        {
            Transform xf1;
            BodyB.GetTransform(out xf1);

            Vector2 r = MathUtils.Multiply(ref xf1.R, _localAnchor2 - BodyB.LocalCenter);
            Vector2 P = _impulse * _J.LinearB;
            float L = _impulse * _J.AngularB - MathUtils.Cross(r, P);
            return inv_dt * L;
        }

        /// Get the gear ratio.
        public float GetRatio()
        {
            return _ratio;
        }

        public void SetRatio(float ratio)
        {
            Debug.Assert(MathUtils.IsValid(ratio));
            _ratio = ratio;
        }

        internal GearJoint(GearJointDef def)
            : base(def)
        {
            JointType type1 = def.Joint1.JointType;
            JointType type2 = def.Joint2.JointType;

            Debug.Assert(type1 == JointType.Revolute || type1 == JointType.Prismatic);
            Debug.Assert(type2 == JointType.Revolute || type2 == JointType.Prismatic);
            Debug.Assert(def.Joint1.GetBodyA().BodyType == BodyType.Static);
            Debug.Assert(def.Joint2.GetBodyA().BodyType == BodyType.Static);

            _revolute1 = null;
            _prismatic1 = null;
            _revolute2 = null;
            _prismatic2 = null;

            float coordinate1, coordinate2;

            _ground1 = def.Joint1.GetBodyA();
            BodyA = def.Joint1.GetBodyB();
            if (type1 == JointType.Revolute)
            {
                _revolute1 = (RevoluteJoint)def.Joint1;
                _localAnchor1 = _revolute1.LocalAnchor2;
                coordinate1 = _revolute1.GetJointAngle();
            }
            else
            {
                _prismatic1 = (PrismaticJoint)def.Joint1;
                _localAnchor1 = _prismatic1.LocalAnchor2;
                coordinate1 = _prismatic1.GetJointTranslation();
            }

            BodyB = def.Joint2.GetBodyB();
            if (type2 == JointType.Revolute)
            {
                _revolute2 = (RevoluteJoint)def.Joint2;
                _localAnchor2 = _revolute2.LocalAnchor2;
                coordinate2 = _revolute2.GetJointAngle();
            }
            else
            {
                _prismatic2 = (PrismaticJoint)def.Joint2;
                _localAnchor2 = _prismatic2.LocalAnchor2;
                coordinate2 = _prismatic2.GetJointTranslation();
            }

            _ratio = def.Ratio;

            _ant = coordinate1 + _ratio * coordinate2;

            _impulse = 0.0f;
        }

        internal override void InitVelocityConstraints(ref TimeStep step)
        {
            Body g1 = _ground1;
            Body b1 = BodyA;
            Body b2 = BodyB;

            float K = 0.0f;
            _J.SetZero();

            if (_revolute1 != null)
            {
                _J.AngularA = -1.0f;
                K += b1._invI;
            }
            else
            {
                Transform xf1, xfg1;
                b1.GetTransform(out xf1);
                g1.GetTransform(out xfg1);

                Vector2 ug = MathUtils.Multiply(ref xfg1.R, _prismatic1.LocalXAxis1);
                Vector2 r = MathUtils.Multiply(ref xf1.R, _localAnchor1 - b1.LocalCenter);
                float crug = MathUtils.Cross(r, ug);
                _J.LinearA = -ug;
                _J.AngularA = -crug;
                K += b1._invMass + b1._invI * crug * crug;
            }

            if (_revolute2 != null)
            {
                _J.AngularB = -_ratio;
                K += _ratio * _ratio * b2._invI;
            }
            else
            {
                Transform xfg1, xf2;
                g1.GetTransform(out xfg1);
                b2.GetTransform(out xf2);

                Vector2 ug = MathUtils.Multiply(ref xfg1.R, _prismatic2.LocalXAxis1);
                Vector2 r = MathUtils.Multiply(ref xf2.R, _localAnchor2 - b2.LocalCenter);
                float crug = MathUtils.Cross(r, ug);
                _J.LinearB = -_ratio * ug;
                _J.AngularB = -_ratio * crug;
                K += _ratio * _ratio * (b2._invMass + b2._invI * crug * crug);
            }

            // Compute effective mass.
            Debug.Assert(K > 0.0f);
            _mass = K > 0.0f ? 1.0f / K : 0.0f;

            if (step.WarmStarting)
            {
                // Warm starting.
                b1._linearVelocity += b1._invMass * _impulse * _J.LinearA;
                b1._angularVelocity += b1._invI * _impulse * _J.AngularA;
                b2._linearVelocity += b2._invMass * _impulse * _J.LinearB;
                b2._angularVelocity += b2._invI * _impulse * _J.AngularB;
            }
            else
            {
                _impulse = 0.0f;
            }
        }

        internal override void SolveVelocityConstraints(ref TimeStep step)
        {
            Body b1 = BodyA;
            Body b2 = BodyB;

            float Cdot = _J.Compute(b1._linearVelocity, b1._angularVelocity,
                                        b2._linearVelocity, b2._angularVelocity);

            float impulse = _mass * (-Cdot);
            _impulse += impulse;

            b1._linearVelocity += b1._invMass * impulse * _J.LinearA;
            b1._angularVelocity += b1._invI * impulse * _J.AngularA;
            b2._linearVelocity += b2._invMass * impulse * _J.LinearB;
            b2._angularVelocity += b2._invI * impulse * _J.AngularB;
        }

        internal override bool SolvePositionConstraints(float baumgarte)
        {
            const float linearError = 0.0f;

            Body b1 = BodyA;
            Body b2 = BodyB;

            float coordinate1, coordinate2;
            if (_revolute1 != null)
            {
                coordinate1 = _revolute1.GetJointAngle();
            }
            else
            {
                coordinate1 = _prismatic1.GetJointTranslation();
            }

            if (_revolute2 != null)
            {
                coordinate2 = _revolute2.GetJointAngle();
            }
            else
            {
                coordinate2 = _prismatic2.GetJointTranslation();
            }

            float C = _ant - (coordinate1 + _ratio * coordinate2);

            float impulse = _mass * (-C);

            b1._sweep.c += b1._invMass * impulse * _J.LinearA;
            b1._sweep.a += b1._invI * impulse * _J.AngularA;
            b2._sweep.c += b2._invMass * impulse * _J.LinearB;
            b2._sweep.a += b2._invI * impulse * _J.AngularB;

            b1.SynchronizeTransform();
            b2.SynchronizeTransform();

            // TODO_ERIN not implemented
            return linearError < Settings.LinearSlop;
        }

        private Body _ground1;

        // One of these is null.
        private RevoluteJoint _revolute1;
        private PrismaticJoint _prismatic1;

        // One of these is null.
        private RevoluteJoint _revolute2;
        private PrismaticJoint _prismatic2;

        private Vector2 _localAnchor1;
        private Vector2 _localAnchor2;

        private Jacobian _J;

        private float _ant;
        private float _ratio;

        // Effective mass
        private float _mass;

        // Impulse for accumulation/warm starting.
        private float _impulse;
    }
}
