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
    /// Mouse joint definition. This requires a world target point,
    /// tuning parameters, and the time step.
    public class MouseJointDef : JointDef
    {
        public MouseJointDef()
        {
            Type = JointType.Mouse;
            Target = Vector2.Zero;
            MaxForce = 0.0f;
            FrequencyHz = 5.0f;
            DampingRatio = 0.7f;
        }

        /// <summary>
        /// The initial world target point. This is assumed
        /// to coincide with the body anchor initially.
        /// </summary>
        public Vector2 Target;

        /// <summary>
        /// The maximum constraint force that can be exerted
        /// to move the candidate body. Usually you will express
        /// as some multiple of the weight (multiplier * mass * gravity).
        /// </summary>
        public float MaxForce;

        /// <summary>
        /// The response speed.
        /// </summary>
        public float FrequencyHz;

        /// <summary>
        /// The damping ratio. 0 = no damping, 1 = critical damping.
        /// </summary>
        public float DampingRatio;
    }

    /// <summary>
    /// A mouse joint is used to make a point on a body track a
    /// specified world point. This a soft constraint with a maximum
    /// force. This allows the constraint to stretch and without
    /// applying huge forces.
    /// NOTE: this joint is not documented in the manual because it was
    /// developed to be used in the testbed. If you want to learn how to
    /// use the mouse joint, look at the testbed.
    /// </summary>
    public class MouseJoint : Joint
    {
        public override Vector2 GetAnchorA()
        {
            return _target;
        }

        public override Vector2 GetAnchorB()
        {
            return BodyB.GetWorldPoint(_localAnchor);
        }

        public override Vector2 GetReactionForce(float inv_dt)
        {
            return inv_dt * _impulse;
        }

        public override float GetReactionTorque(float inv_dt)
        {
            return inv_dt * 0.0f;
        }

        /// <summary>
        /// Use this to update the target point.
        /// </summary>
        /// <param name="target">The target.</param>
        public void SetTarget(Vector2 target)
        {
            if (BodyB.IsAwake() == false)
            {
                BodyB.SetAwake(true);
            }
            _target = target;
        }

        public Vector2 GetTarget()
        {
            return _target;
        }

        /// <summary>
        /// Set/get the maximum force in Newtons.
        /// </summary>
        /// <param name="force">The force.</param>
        public void SetMaxForce(float force)
        {
            _maxForce = force;
        }

        public float GetMaxForce()
        {
            return _maxForce;
        }

        /// <summary>
        /// Set/get the frequency in Hertz.
        /// </summary>
        /// <param name="hz">The hz.</param>
        public void SetFrequency(float hz)
        {
            _frequencyHz = hz;
        }

        public float GetFrequency()
        {
            return _frequencyHz;
        }

        /// <summary>
        /// Set/get the damping ratio (dimensionless).
        /// </summary>
        /// <param name="ratio">The ratio.</param>
        public void SetDampingRatio(float ratio)
        {
            _dampingRatio = ratio;
        }

        public float GetDampingRatio()
        {
            return _dampingRatio;
        }


        internal MouseJoint(MouseJointDef def)
            : base(def)
        {
            Debug.Assert(def.Target.IsValid());
            Debug.Assert(MathUtils.IsValid(def.MaxForce) && def.MaxForce >= 0.0f);
            Debug.Assert(MathUtils.IsValid(def.FrequencyHz) && def.FrequencyHz >= 0.0f);
            Debug.Assert(MathUtils.IsValid(def.DampingRatio) && def.DampingRatio >= 0.0f);

            Transform xf1;
            BodyB.GetTransform(out xf1);

            _target = def.Target;
            _localAnchor = MathUtils.MultiplyT(ref xf1, _target);

            _maxForce = def.MaxForce;
            _impulse = Vector2.Zero;

            _frequencyHz = def.FrequencyHz;
            _dampingRatio = def.DampingRatio;

            _beta = 0.0f;
            _gamma = 0.0f;
        }

        internal override void InitVelocityConstraints(ref TimeStep step)
        {
            Body b = BodyB;

            float mass = b.GetMass();

            // Frequency
            float omega = 2.0f * Settings.Pi * _frequencyHz;

            // Damping coefficient
            float d = 2.0f * mass * _dampingRatio * omega;

            // Spring stiffness
            float k = mass * (omega * omega);

            // magic formulas
            // gamma has units of inverse mass.
            // beta has units of inverse time.
            Debug.Assert(d + step.DeltaTime * k > Settings.Epsilon);

            _gamma = step.DeltaTime * (d + step.DeltaTime * k);
            if (_gamma != 0.0f)
            {
                _gamma = 1.0f / _gamma;
            }

            _beta = step.DeltaTime * k * _gamma;

            // Compute the effective mass matrix.
            Transform xf1;
            b.GetTransform(out xf1);
            Vector2 r = MathUtils.Multiply(ref xf1.R, _localAnchor - b.GetLocalCenter());

            // K    = [(1/m1 + 1/m2) * eye(2) - skew(r1) * invI1 * skew(r1) - skew(r2) * invI2 * skew(r2)]
            //      = [1/m1+1/m2     0    ] + invI1 * [r1.Y*r1.Y -r1.X*r1.Y] + invI2 * [r1.Y*r1.Y -r1.X*r1.Y]
            //        [    0     1/m1+1/m2]           [-r1.X*r1.Y r1.X*r1.X]           [-r1.X*r1.Y r1.X*r1.X]
            float invMass = b._invMass;
            float invI = b._invI;

            Mat22 K1 = new Mat22(new Vector2(invMass, 0.0f), new Vector2(0.0f, invMass));
            Mat22 K2 = new Mat22(new Vector2(invI * r.Y * r.Y, -invI * r.X * r.Y), new Vector2(-invI * r.X * r.Y, invI * r.X * r.X));

            Mat22 K;
            Mat22.Add(ref K1, ref K2, out K);

            K.col1.X += _gamma;
            K.col2.Y += _gamma;

            _mass = K.GetInverse();

            _C = b._sweep.c + r - _target;

            // Cheat with some damping
            b._angularVelocity *= 0.98f;

            // Warm starting.
            _impulse *= step.DtRatio;
            b._linearVelocity += invMass * _impulse;
            b._angularVelocity += invI * MathUtils.Cross(r, _impulse);
        }

        internal override void SolveVelocityConstraints(ref TimeStep step)
        {
            Body b = BodyB;

            Transform xf1;
            b.GetTransform(out xf1);

            Vector2 r = MathUtils.Multiply(ref xf1.R, _localAnchor - b.GetLocalCenter());

            // Cdot = v + cross(w, r)
            Vector2 Cdot = b._linearVelocity + MathUtils.Cross(b._angularVelocity, r);
            Vector2 impulse = MathUtils.Multiply(ref _mass, -(Cdot + _beta * _C + _gamma * _impulse));

            Vector2 oldImpulse = _impulse;
            _impulse += impulse;
            float maxImpulse = step.DeltaTime * _maxForce;
            if (_impulse.LengthSquared() > maxImpulse * maxImpulse)
            {
                _impulse *= maxImpulse / _impulse.Length();
            }
            impulse = _impulse - oldImpulse;

            b._linearVelocity += b._invMass * impulse;
            b._angularVelocity += b._invI * MathUtils.Cross(r, impulse);
        }

        internal override bool SolvePositionConstraints(float baumgarte)
        {
            return true;
        }

        private Vector2 _localAnchor;
        private Vector2 _target;
        private Vector2 _impulse;

        private Mat22 _mass;		// effective mass for point-to-point constraint.
        private Vector2 _C;				// position error
        private float _maxForce;
        private float _frequencyHz;
        private float _dampingRatio;
        private float _beta;
        private float _gamma;
    }
}
