using System;
using System.Collections.Generic;
using System.Text;

using Box2DX.Common;

namespace Box2DX.Dynamics
{
    using Box2DXMath = Box2DX.Common.Math;
	using SystemMath = System.Math;

    /// FixedJoint: Attaches two bodies rigidly together
    class FixedJointDef : JointDef
    {
        FixedJointDef()
        {
            Type = JointType.FixedJoint;
        }

        public void Initialize(Body b1, Body b2)
        {
	        Body1 = b1;
	        Body2 = b2;
        }
    }

    /// A fixed joint constrains all degrees of freedom between two bodies
    /// Author: Jorrit Rouwe
    /// See: www.jrouwe.nl/fixedjoint/ for more info
    class FixedJoint : Joint
    {
        private Vec2 _dp;	//< Distance between body.GetTransform().Position between the two bodies at rest in the reference frame of body1
        private float _a;	//< Angle between the bodies at rest
        private Mat22 _R0;	//< Rotation matrix of _a

        // Distance between center of masses for this time step (when the shapes of the bodies change, their local canters can change so we derive this from _dp every frame)
        private Vec2 _d;		//< Distance between center of masses for both bodies at rest in the reference frame of body1

        // Effective mass and inertia for angle constraint
        private float _mass;
        private float _inertia;

        // Accumulated impulse for warm starting and returning the constraint force/torque
        private float _lambda_a;
        private Vec2 _lambda_p;
        private float _lambda_p_a;

        

        public FixedJoint(FixedJointDef def) : base(def)
        {
	        // Get bodies
	        Body b1 = _body1;
	        Body b2 = _body2;

	        // Get initial delta Position and angle
	        _dp = Box2DXMath.MulT(b1.GetTransform().R, b2.GetTransform().Position - b1.GetTransform().Position);
	        _a = b2.GetAngle() - b1.GetAngle();
	        _R0 = Box2DXMath.MulT(b1.GetTransform().R, b2.GetTransform().R);

	        // Reset accumulators
	        _lambda_a = 0.0f;
	        _lambda_p.Set(0.0f, 0.0f);
	        _lambda_p_a = 0.0f;
        }

        internal override void InitVelocityConstraints(TimeStep step)
        {
	        // Get bodies
	        Body b1 = _body1;
	        Body b2 = _body2;

	        // Get d for this step
	        _d = _dp - b1._sweep.LocalCenter + Box2DXMath.Mul(_R0, b2._sweep.LocalCenter);

	        // Calculate effective mass for angle constraint
	        float invMass = b1._invMass + b2._invMass;
	        Box2DXDebug.Assert(invMass > Settings.FLT_EPSILON);
	        _mass = 1.0f / invMass;

	        // Calculate effective inertia for angle constraint
	        float invInertia = b1._invI + b2._invI;
	        Box2DXDebug.Assert(invInertia > Settings.FLT_EPSILON);
	        _inertia = 1.0f / invInertia;

	        if (step.WarmStarting)
	        {
		        // Take results of previous frame for angular constraint
		        b1._angularVelocity -= b1._invI * _lambda_a;
		        b2._angularVelocity += b2._invI * _lambda_a;

		        // Take results of previous frame for Position constraint
                float s = (float)SystemMath.Sin(b1._sweep.A), c = (float)SystemMath.Cos(b1._sweep.A);
		        Vec2 A = new Vec2(-s * _d.X - c * _d.Y, c * _d.X - s * _d.Y);
		        b1._linearVelocity -= b1._invMass * _lambda_p;
		        b1._angularVelocity -= b1._invI * Vec2.Dot(_lambda_p, A);
		        b2._linearVelocity += b2._invMass * _lambda_p;
	        }
	        else
	        {
		        // Reset accumulators
		        _lambda_a = 0.0f;
		        _lambda_p.Set(0.0f, 0.0f);
		        _lambda_p_a = 0.0f;
	        }
        }

        internal override void SolveVelocityConstraints(TimeStep step)
        {
	        // Get bodies
	        Body b1 = _body1;
	        Body b2 = _body2;

	        // Angle constraint: w2 - w1 = 0
	        float Cdot_a = b2._angularVelocity - b1._angularVelocity;
	        float lambda_a = -_inertia * Cdot_a;
	        _lambda_a += lambda_a;
	        b1._angularVelocity -= b1._invI * lambda_a;
	        b2._angularVelocity += b2._invI * lambda_a;

	        // Position constraint: v2 - v1 - d/dt R(a1) d = v2 - v1 - A w1 = 0
	        float s = (float)SystemMath.Sin(b1._sweep.A), c = (float)SystemMath.Cos(b1._sweep.A);
            Vec2 A = new Vec2(-s * _d.X - c * _d.Y, c * _d.X - s * _d.Y);
	        Vec2 Cdot_p = b2._linearVelocity - b1._linearVelocity - b1._angularVelocity * A;
	        Vec2 mc_p = new Vec2(1.0f / (b1._invMass + b1._invI * A.X * A.X + b2._invMass), 1.0f / (b1._invMass + b1._invI * A.Y * A.Y + b2._invMass));
	        Vec2 lambda_p = new Vec2(-mc_p.X * Cdot_p.X, -mc_p.Y * Cdot_p.Y);
	        _lambda_p += lambda_p;
	        float lambda_p_a = Vec2.Dot(A, lambda_p);
	        _lambda_p_a += lambda_p_a;
	        b1._linearVelocity -= b1._invMass * lambda_p;
	        b1._angularVelocity -= b1._invI * lambda_p_a;
	        b2._linearVelocity += b2._invMass * lambda_p;
        }

        internal override bool SolvePositionConstraints(float baumgarte)
        {
	        // Get bodies
	        Body b1 = _body1;
	        Body b2 = _body2;

	        // Angle constraint: a2 - a1 - a0 = 0
	        float C_a = b2._sweep.A - b1._sweep.A - _a;
	        float lambda_a = -_inertia * C_a;
	        b1._sweep.A -= b1._invI * lambda_a;
	        b2._sweep.A += b2._invI * lambda_a;

	        // Position constraint: x2 - x1 - R(a1) d = 0
            float s = (float)SystemMath.Sin(b1._sweep.A), c = (float)SystemMath.Cos(b1._sweep.A);
            Vec2 Rd = new Vec2(c * _d.X - s * _d.Y, s * _d.X + c * _d.Y);
	        Vec2 C_p = b2._sweep.C - b1._sweep.C - Rd;
	        Vec2 lambda_p = -_mass * C_p;
	        b1._sweep.C -= b1._invMass * lambda_p;
	        b2._sweep.C += b2._invMass * lambda_p;

	        // Push the changes to the transforms
	        b1.SynchronizeTransform();
	        b2.SynchronizeTransform();

	        // Constraint is satisfied if all constraint equations are nearly zero
            return SystemMath.Abs(C_p.X) < Settings.LinearSlop && SystemMath.Abs(C_p.Y) < Settings.LinearSlop && SystemMath.Abs(C_a) < Settings.LinearSlop;
        }

        public Vec2 GetAnchor1() 
        { 
	        // Return arbitrary Position (we have to implement this abstract virtual function)
	        return _body1.GetWorldCenter();
        }

        public Vec2 GetAnchor2() 
        { 
	        // Return arbitrary Position (we have to implement this abstract virtual function)
	        return _body2.GetWorldCenter(); 
        }

        public override Vec2 Anchor1
        {
            get { return _body1.GetWorldCenter(); }
        }

        public override Vec2 Anchor2
        {
            get { return _body2.GetWorldCenter(); }
        }

        public override  Vec2 GetReactionForce(float inv_dt)
        {
	        return inv_dt * _lambda_p;
        }

        public override  float GetReactionTorque(float inv_dt)
        {
	        return inv_dt * (_lambda_a + _lambda_p_a);
        }

    }
}
