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
using System;
namespace Box2D.XNA
{
    /// Weld joint definition. You need to specify local anchor points
    /// where they are attached and the relative body angle. The position
    /// of the anchor points is important for computing the reaction torque.
    public class WeldJointDef : JointDef
    {
        public WeldJointDef()
	    {
            type = JointType.Weld;
	    }

        // Point-to-point constraint
        // C = p2 - p1
        // Cdot = v2 - v1
        //      = v2 + cross(w2, r2) - v1 - cross(w1, r1)
        // J = [-I -r1_skew I r2_skew ]
        // Identity used:
        // w k % (rx i + ry j) = w * (-ry i + rx j)

        // Angle constraint
        // C = angle2 - angle1 - referenceAngle
        // Cdot = w2 - w1
        // J = [0 0 -1 0 0 1]
        // K = invI1 + invI2

	    public void Initialize(Body b1, Body b2, Vector2 anchor)
        {
	        bodyA = b1;
	        bodyB = b2;
	        localAnchorA = bodyA.GetLocalPoint(anchor);
	        localAnchorB = bodyB.GetLocalPoint(anchor);
            referenceAngle = bodyB.GetAngle() - bodyA.GetAngle();
        }

	    /// The local anchor point relative to body1's origin.
	    public Vector2 localAnchorA;

	    /// The local anchor point relative to body2's origin.
	    public Vector2 localAnchorB;
    
	    /// The body2 angle minus body1 angle in the reference state (radians).
	    public float referenceAngle;
    };

    /// A weld joint essentially glues two bodies together. A weld joint may
    /// distort somewhat because the island constraint solver is approximate.
    public class WeldJoint : Joint
    {
	    public override Vector2 GetAnchorA()
        {
            return _bodyA.GetWorldPoint(_localAnchorA);
        }

        public override Vector2 GetAnchorB()
        {
	        return _bodyB.GetWorldPoint(_localAnchorB);
        }

        public override Vector2 GetReactionForce(float inv_dt)
        {
	        Vector2 F = (inv_dt * new Vector2(_impulse.X, _impulse.Y));
	        return F;
        }

        public override float GetReactionTorque(float inv_dt)
        {
            float F = (inv_dt * _impulse.Z);
            return F;
        }

	    internal WeldJoint(WeldJointDef def)
            : base(def)
        {
	        _localAnchorA = def.localAnchorA;
	        _localAnchorB = def.localAnchorB;
            _referenceAngle = def.referenceAngle;
        }

        internal override void InitVelocityConstraints(ref TimeStep step)
        {
	        Body bA = _bodyA;
	        Body bB = _bodyB;

            Transform xfA, xfB;
            bA.GetTransform(out xfA);
            bB.GetTransform(out xfB);

	        // Compute the effective mass matrix.
            Vector2 rA = MathUtils.Multiply(ref xfA.R, _localAnchorA - bA.GetLocalCenter());
            Vector2 rB = MathUtils.Multiply(ref xfB.R, _localAnchorB - bB.GetLocalCenter());
	        
           	// J = [-I -r1_skew I r2_skew]
	        //     [ 0       -1 0       1]
	        // r_skew = [-ry; rx]

	        // Matlab
	        // K = [ mA+r1y^2*iA+mB+r2y^2*iB,  -r1y*iA*r1x-r2y*iB*r2x,          -r1y*iA-r2y*iB]
	        //     [  -r1y*iA*r1x-r2y*iB*r2x, mA+r1x^2*iA+mB+r2x^2*iB,           r1x*iA+r2x*iB]
	        //     [          -r1y*iA-r2y*iB,           r1x*iA+r2x*iB,                   iA+iB]

	        float mA = bA._invMass, mB = bB._invMass;
	        float iA = bA._invI, iB = bB._invI;

	        _mass.col1.X = mA + mB + rA.Y * rA.Y * iA + rB.Y * rB.Y * iB;
	        _mass.col2.X = -rA.Y * rA.X * iA - rB.Y * rB.X * iB;
	        _mass.col3.X = -rA.Y * iA - rB.Y * iB;
	        _mass.col1.Y = _mass.col2.X;
	        _mass.col2.Y = mA + mB + rA.X * rA.X * iA + rB.X * rB.X * iB;
	        _mass.col3.Y = rA.X * iA + rB.X * iB;
	        _mass.col1.Z = _mass.col3.X;
	        _mass.col2.Z = _mass.col3.Y;
	        _mass.col3.Z = iA + iB;

	        if (step.warmStarting)
	        {
		        // Scale impulses to support a variable time step.
		        _impulse *= step.dtRatio;

		        Vector2 P = new Vector2(_impulse.X, _impulse.Y);

		        bA._linearVelocity -= mA * P;
		        bA._angularVelocity -= iA * (MathUtils.Cross(rA, P) + _impulse.Z);

		        bB._linearVelocity += mB * P;
		        bB._angularVelocity += iB * (MathUtils.Cross(rB, P) + _impulse.Z);
	        }
	        else
	        {
		        _impulse = Vector3.Zero;
	        }

        }

        internal override void SolveVelocityConstraints(ref TimeStep step)
        {
	        Body bA = _bodyA;
	        Body bB = _bodyB;

            Vector2 vA = bA._linearVelocity;
            float wA = bA._angularVelocity;
            Vector2 vB = bB._linearVelocity;
            float wB = bB._angularVelocity;

            float mA = bA._invMass, mB = bB._invMass;
            float iA = bA._invI, iB = bB._invI;

            Transform xfA, xfB;
            bA.GetTransform(out xfA);
            bB.GetTransform(out xfB);

            Vector2 rA = MathUtils.Multiply(ref xfA.R, _localAnchorA - bA.GetLocalCenter());
            Vector2 rB = MathUtils.Multiply(ref xfB.R, _localAnchorB - bB.GetLocalCenter());

            //  Solve point-to-point constraint
	        Vector2 Cdot1 = vB + MathUtils.Cross(wB, rB) - vA - MathUtils.Cross(wA, rA);
	        float Cdot2 = wB - wA;
	        Vector3 Cdot = new Vector3(Cdot1.X, Cdot1.Y, Cdot2);

	        Vector3 impulse = _mass.Solve33(-Cdot);
	        _impulse += impulse;

	        Vector2 P = new Vector2(impulse.X, impulse.Y);

	        vA -= mA * P;
	        wA -= iA * (MathUtils.Cross(rA, P) + impulse.Z);

	        vB += mB * P;
	        wB += iB * (MathUtils.Cross(rB, P) + impulse.Z);

	        bA._linearVelocity = vA;
	        bA._angularVelocity = wA;
	        bB._linearVelocity = vB;
	        bB._angularVelocity = wB;

        }

        internal override bool SolvePositionConstraints(float baumgarte)
        {
	        Body bA = _bodyA;
	        Body bB = _bodyB;

	        float mA = bA._invMass, mB = bB._invMass;
	        float iA = bA._invI, iB = bB._invI;

            Transform xfA;
            Transform xfB;
            bA.GetTransform(out xfA);
            bB.GetTransform(out xfB);

	        Vector2 rA = MathUtils.Multiply(ref xfA.R, _localAnchorA - bA.GetLocalCenter());
	        Vector2 rB = MathUtils.Multiply(ref xfB.R, _localAnchorB - bB.GetLocalCenter());

	        Vector2 C1 =  bB._sweep.c + rB - bA._sweep.c - rA;
	        float C2 = bB._sweep.a - bA._sweep.a - _referenceAngle;

	        // Handle large detachment.
	        float k_allowedStretch = 10.0f * Settings.b2_linearSlop;
	        float positionError = C1.Length();
	        float angularError = Math.Abs(C2);
	        if (positionError > k_allowedStretch)
	        {
		        iA *= 1.0f;
		        iB *= 1.0f;
	        }

	        _mass.col1.X = mA + mB + rA.Y * rA.Y * iA + rB.Y * rB.Y * iB;
	        _mass.col2.X = -rA.Y * rA.X * iA - rB.Y * rB.X * iB;
	        _mass.col3.X = -rA.Y * iA - rB.Y * iB;
	        _mass.col1.Y = _mass.col2.X;
	        _mass.col2.Y = mA + mB + rA.X * rA.X * iA + rB.X * rB.X * iB;
	        _mass.col3.Y = rA.X * iA + rB.X * iB;
	        _mass.col1.Z = _mass.col3.X;
	        _mass.col2.Z = _mass.col3.Y;
	        _mass.col3.Z = iA + iB;

	        Vector3 C = new Vector3(C1.X, C1.Y, C2);

	        Vector3 impulse = _mass.Solve33(-C);

	        Vector2 P = new Vector2(impulse.X, impulse.Y);

	        bA._sweep.c -= mA * P;
	        bA._sweep.a -= iA * (MathUtils.Cross(rA, P) + impulse.Z);

	        bB._sweep.c += mB * P;
	        bB._sweep.a += iB * (MathUtils.Cross(rB, P) + impulse.Z);

	        bA.SynchronizeTransform();
	        bB.SynchronizeTransform();

	        return positionError <= Settings.b2_linearSlop && angularError <= Settings.b2_angularSlop;
        }

	    internal Vector2 _localAnchorA;
	    internal Vector2 _localAnchorB;
        internal float _referenceAngle;
	    internal Vector3 _impulse;
	    internal Mat33 _mass;
    };
}
