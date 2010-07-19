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

//#define MATH_OVERLOADS

using System;
using System.Diagnostics;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Contacts
{
    public struct ContactConstraintPoint
    {
        public Vector2 localPoint;
        public float normalImpulse;
        public float normalMass;
        public Vector2 rA;
        public Vector2 rB;
        public float tangentImpulse;
        public float tangentMass;
        public float velocityBias;
    }

    public struct ContactConstraint
    {
        public Mat22 K;
        public Body bodyA;
        public Body bodyB;
        public float friction;
        public Vector2 localNormal;
        public Vector2 localPoint;
        public Manifold manifold;
        public Vector2 normal;
        public Mat22 normalMass;
        public int pointCount;
        public FixedArray2<ContactConstraintPoint> points;
        public float radius;
        public ManifoldType type;
    }

    public class ContactSolver
    {
        public int _constraintCount; // collection can be bigger.
        public ContactConstraint[] _constraints;
        private Contact[] _contacts;

        public void Reset(Contact[] contacts, int contactCount, float impulseRatio)
        {
            _contacts = contacts;

            _constraintCount = contactCount;

            // grow the array
            if (_constraints == null || _constraints.Length < _constraintCount)
            {
                _constraints = new ContactConstraint[_constraintCount * 2];
            }

            for (int i = 0; i < _constraintCount; ++i)
            {
                Contact contact = contacts[i];

                Fixture fixtureA = contact._fixtureA;
                Fixture fixtureB = contact._fixtureB;
                Shape shapeA = fixtureA.Shape;
                Shape shapeB = fixtureB.Shape;
                float radiusA = shapeA.Radius;
                float radiusB = shapeB.Radius;
                Body bodyA = fixtureA.Body;
                Body bodyB = fixtureB.Body;
                Manifold manifold;
                contact.GetManifold(out manifold);

                float friction = Settings.b2MixFriction(fixtureA.Friction, fixtureB.Friction);
                float restitution = Settings.b2MixRestitution(fixtureA.Restitution, fixtureB.Restitution);

                Vector2 vA = bodyA._linearVelocity;
                Vector2 vB = bodyB._linearVelocity;
                float wA = bodyA._angularVelocity;
                float wB = bodyB._angularVelocity;

                Debug.Assert(manifold.PointCount > 0);

                WorldManifold worldManifold = new WorldManifold(ref manifold, ref bodyA._xf, radiusA, ref bodyB._xf,
                                                                radiusB);

                ContactConstraint cc = _constraints[i];
                cc.bodyA = bodyA;
                cc.bodyB = bodyB;
                cc.manifold = manifold;
                cc.normal = worldManifold.Normal;
                cc.pointCount = manifold.PointCount;
                cc.friction = friction;

                cc.localNormal = manifold.LocalNormal;
                cc.localPoint = manifold.LocalPoint;
                cc.radius = radiusA + radiusB;
                cc.type = manifold.Type;

                for (int j = 0; j < cc.pointCount; ++j)
                {
                    ManifoldPoint cp = manifold.Points[j];
                    ContactConstraintPoint ccp = cc.points[j];

                    ccp.normalImpulse = impulseRatio * cp.NormalImpulse;
                    ccp.tangentImpulse = impulseRatio * cp.TangentImpulse;

                    ccp.localPoint = cp.LocalPoint;

                    ccp.rA = worldManifold.Points[j] - bodyA._sweep.c;
                    ccp.rB = worldManifold.Points[j] - bodyB._sweep.c;

#if MATH_OVERLOADS
			        float rnA = MathUtils.Cross(ccp.rA, cc.normal);
			        float rnB = MathUtils.Cross(ccp.rB, cc.normal);
#else
                    float rnA = ccp.rA.X * cc.normal.Y - ccp.rA.Y * cc.normal.X;
                    float rnB = ccp.rB.X * cc.normal.Y - ccp.rB.Y * cc.normal.X;
#endif
                    rnA *= rnA;
                    rnB *= rnB;

                    float kNormal = bodyA._invMass + bodyB._invMass + bodyA._invI * rnA + bodyB._invI * rnB;

                    Debug.Assert(kNormal > Settings.Epsilon);
                    ccp.normalMass = 1.0f / kNormal;

#if MATH_OVERLOADS
			        Vector2 tangent = MathUtils.Cross(cc.normal, 1.0f);

			        float rtA = MathUtils.Cross(ccp.rA, tangent);
			        float rtB = MathUtils.Cross(ccp.rB, tangent);
#else
                    Vector2 tangent = new Vector2(cc.normal.Y, -cc.normal.X);

                    float rtA = ccp.rA.X * tangent.Y - ccp.rA.Y * tangent.X;
                    float rtB = ccp.rB.X * tangent.Y - ccp.rB.Y * tangent.X;
#endif
                    rtA *= rtA;
                    rtB *= rtB;
                    float kTangent = bodyA._invMass + bodyB._invMass + bodyA._invI * rtA + bodyB._invI * rtB;

                    Debug.Assert(kTangent > Settings.Epsilon);
                    ccp.tangentMass = 1.0f / kTangent;

                    // Setup a velocity bias for restitution.
                    ccp.velocityBias = 0.0f;
                    float vRel = Vector2.Dot(cc.normal,
                                             vB + MathUtils.Cross(wB, ccp.rB) - vA - MathUtils.Cross(wA, ccp.rA));
                    if (vRel < -Settings.VelocityThreshold)
                    {
                        ccp.velocityBias = -restitution * vRel;
                    }

                    cc.points[j] = ccp;
                }

                // If we have two points, then prepare the block solver.
                if (cc.pointCount == 2)
                {
                    ContactConstraintPoint ccp1 = cc.points[0];
                    ContactConstraintPoint ccp2 = cc.points[1];

                    float invMassA = bodyA._invMass;
                    float invIA = bodyA._invI;
                    float invMassB = bodyB._invMass;
                    float invIB = bodyB._invI;

                    float rn1A = MathUtils.Cross(ccp1.rA, cc.normal);
                    float rn1B = MathUtils.Cross(ccp1.rB, cc.normal);
                    float rn2A = MathUtils.Cross(ccp2.rA, cc.normal);
                    float rn2B = MathUtils.Cross(ccp2.rB, cc.normal);

                    float k11 = invMassA + invMassB + invIA * rn1A * rn1A + invIB * rn1B * rn1B;
                    float k22 = invMassA + invMassB + invIA * rn2A * rn2A + invIB * rn2B * rn2B;
                    float k12 = invMassA + invMassB + invIA * rn1A * rn2A + invIB * rn1B * rn2B;

                    // Ensure a reasonable condition number.
                    const float k_maxConditionNumber = 100.0f;
                    if (k11 * k11 < k_maxConditionNumber * (k11 * k22 - k12 * k12))
                    {
                        // K is safe to invert.
                        cc.K = new Mat22(new Vector2(k11, k12), new Vector2(k12, k22));
                        cc.normalMass = cc.K.GetInverse();
                    }
                    else
                    {
                        // The constraints are redundant, just use one.
                        // TODO_ERIN use deepest?
                        cc.pointCount = 1;
                    }
                }

                _constraints[i] = cc;

                if (fixtureA.PostSolve != null)
                    fixtureA.PostSolve(cc);

                if (fixtureB.PostSolve != null)
                    fixtureB.PostSolve(cc);
            }
        }

        public void WarmStart()
        {
            // Warm start.
            for (int i = 0; i < _constraintCount; ++i)
            {
                ContactConstraint c = _constraints[i];

                Body bodyA = c.bodyA;
                Body bodyB = c.bodyB;
                float invMassA = bodyA._invMass;
                float invIA = bodyA._invI;
                float invMassB = bodyB._invMass;
                float invIB = bodyB._invI;
                Vector2 normal = c.normal;

#if MATH_OVERLOADS
	            Vector2 tangent = MathUtils.Cross(normal, 1.0f);
#else
                Vector2 tangent = new Vector2(normal.Y, -normal.X);
#endif

                for (int j = 0; j < c.pointCount; ++j)
                {
                    ContactConstraintPoint ccp = c.points[j];
#if MATH_OVERLOADS
		            Vector2 P = ccp.normalImpulse * normal + ccp.tangentImpulse * tangent;
		            bodyA._angularVelocity -= invIA * MathUtils.Cross(ccp.rA, P);
		            bodyA._linearVelocity -= invMassA * P;
		            bodyB._angularVelocity += invIB * MathUtils.Cross(ccp.rB, P);
		            bodyB._linearVelocity += invMassB * P;
#else
                    Vector2 P = new Vector2(ccp.normalImpulse * normal.X + ccp.tangentImpulse * tangent.X,
                                            ccp.normalImpulse * normal.Y + ccp.tangentImpulse * tangent.Y);
                    bodyA._angularVelocity -= invIA * (ccp.rA.X * P.Y - ccp.rA.Y * P.X);
                    bodyA._linearVelocity.X -= invMassA * P.X;
                    bodyA._linearVelocity.Y -= invMassA * P.Y;
                    bodyB._angularVelocity += invIB * (ccp.rB.X * P.Y - ccp.rB.Y * P.X);
                    bodyB._linearVelocity.X += invMassB * P.X;
                    bodyB._linearVelocity.Y += invMassB * P.Y;
#endif
                    c.points[j] = ccp;
                }

                _constraints[i] = c;
            }
        }

        public void SolveVelocityConstraints()
        {
            for (int i = 0; i < _constraintCount; ++i)
            {
                ContactConstraint c = _constraints[i];
                Body bodyA = c.bodyA;
                Body bodyB = c.bodyB;
                float wA = bodyA._angularVelocity;
                float wB = bodyB._angularVelocity;
                Vector2 vA = bodyA._linearVelocity;
                Vector2 vB = bodyB._linearVelocity;
                float invMassA = bodyA._invMass;
                float invIA = bodyA._invI;
                float invMassB = bodyB._invMass;
                float invIB = bodyB._invI;
                Vector2 normal = c.normal;

#if MATH_OVERLOADS
				Vector2 tangent = ZoomEngine.Physics.Common.Math.Cross(normal, 1.0f);
#else
                Vector2 tangent = new Vector2(normal.Y, -normal.X);
#endif
                float friction = c.friction;

                Debug.Assert(c.pointCount == 1 || c.pointCount == 2);

                // Solve tangent constraints
                for (int j = 0; j < c.pointCount; ++j)
                {
                    ContactConstraintPoint ccp = c.points[j];

#if MATH_OVERLOADS
    // Relative velocity at contact
			        Vector2 dv = vB + MathUtils.Cross(wB, ccp.rB) - vA - MathUtils.Cross(wA, ccp.rA);

			        // Compute tangent force
			        float vt = Vector2.Dot(dv, tangent);
#else
                    // Relative velocity at contact
                    Vector2 dv = new Vector2(vB.X + (-wB * ccp.rB.Y) - vA.X - (-wA * ccp.rA.Y),
                                             vB.Y + (wB * ccp.rB.X) - vA.Y - (wA * ccp.rA.X));

                    // Compute tangent force
                    float vt = dv.X * tangent.X + dv.Y * tangent.Y;
#endif
                    float lambda = ccp.tangentMass * (-vt);

                    // MathUtils.Clamp the accumulated force
                    float maxFriction = friction * ccp.normalImpulse;
                    float newImpulse = MathUtils.Clamp(ccp.tangentImpulse + lambda, -maxFriction, maxFriction);
                    lambda = newImpulse - ccp.tangentImpulse;

#if MATH_OVERLOADS
    // Apply contact impulse
			        Vector2 P = lambda * tangent;

			        vA -= invMassA * P;
			        wA -= invIA * MathUtils.Cross(ccp.rA, P);

			        vB += invMassB * P;
			        wB += invIB * MathUtils.Cross(ccp.rB, P);
#else
                    // Apply contact impulse
                    Vector2 P = new Vector2(lambda * tangent.X, lambda * tangent.Y);

                    vA.X -= invMassA * P.X;
                    vA.Y -= invMassA * P.Y;
                    wA -= invIA * (ccp.rA.X * P.Y - ccp.rA.Y * P.X);

                    vB.X += invMassB * P.X;
                    vB.Y += invMassB * P.Y;
                    wB += invIB * (ccp.rB.X * P.Y - ccp.rB.Y * P.X);
#endif
                    ccp.tangentImpulse = newImpulse;
                    c.points[j] = ccp;
                }

                // Solve normal constraints
                if (c.pointCount == 1)
                {
                    ContactConstraintPoint ccp = c.points[0];

#if MATH_OVERLOADS
    // Relative velocity at contact
			        Vector2 dv = vB + MathUtils.Cross(wB, ccp.rB) - vA - MathUtils.Cross(wA, ccp.rA);

			        // Compute normal impulse
			        float vn = Vector2.Dot(dv, normal);
			        float lambda = -ccp.normalMass * (vn - ccp.velocityBias);

			        // MathUtils.Clamp the accumulated impulse
			        float newImpulse = Math.Max(ccp.normalImpulse + lambda, 0.0f);
			        lambda = newImpulse - ccp.normalImpulse;

			        // Apply contact impulse
			        Vector2 P = lambda * normal;
			        vA -= invMassA * P;
			        wA -= invIA * MathUtils.Cross(ccp.rA, P);

			        vB += invMassB * P;
			        wB += invIB * MathUtils.Cross(ccp.rB, P);
#else
                    // Relative velocity at contact
                    Vector2 dv = new Vector2(vB.X + (-wB * ccp.rB.Y) - vA.X - (-wA * ccp.rA.Y),
                                             vB.Y + (wB * ccp.rB.X) - vA.Y - (wA * ccp.rA.X));

                    // Compute normal impulse
                    float vn = dv.X * normal.X + dv.Y * normal.Y;
                    float lambda = -ccp.normalMass * (vn - ccp.velocityBias);

                    // Clamp the accumulated impulse
                    float newImpulse = Math.Max(ccp.normalImpulse + lambda, 0.0f);
                    lambda = newImpulse - ccp.normalImpulse;

                    // Apply contact impulse
                    var P = new Vector2(lambda * normal.X, lambda * normal.Y);

                    vA.X -= invMassA * P.X;
                    vA.Y -= invMassA * P.Y;
                    wA -= invIA * (ccp.rA.X * P.Y - ccp.rA.Y * P.X);

                    vB.X += invMassB * P.X;
                    vB.Y += invMassB * P.Y;
                    wB += invIB * (ccp.rB.X * P.Y - ccp.rB.Y * P.X);
#endif
                    ccp.normalImpulse = newImpulse;
                    c.points[0] = ccp;
                }
                else
                {
                    // Block solver developed in collaboration with Dirk Gregorius (back in 01/07 on Box2D_Lite).
                    // Build the mini LCP for this contact patch
                    //
                    // vn = A * x + b, vn >= 0, , vn >= 0, x >= 0 and vn_i * x_i = 0 with i = 1..2
                    //
                    // A = J * W * JT and J = ( -n, -r1 x n, n, r2 x n )
                    // b = vn_0 - velocityBias
                    //
                    // The system is solved using the "Total enumeration method" (s. Murty). The complementary constraint vn_i * x_i
                    // implies that we must have in any solution either vn_i = 0 or x_i = 0. So for the 2D contact problem the cases
                    // vn1 = 0 and vn2 = 0, x1 = 0 and x2 = 0, x1 = 0 and vn2 = 0, x2 = 0 and vn1 = 0 need to be tested. The first valid
                    // solution that satisfies the problem is chosen.
                    // 
                    // In order to account of the accumulated impulse 'a' (because of the iterative nature of the solver which only requires
                    // that the accumulated impulse is clamped and not the incremental impulse) we change the impulse variable (x_i).
                    //
                    // Substitute:
                    // 
                    // x = x' - a
                    // 
                    // Plug into above equation:
                    //
                    // vn = A * x + b
                    //    = A * (x' - a) + b
                    //    = A * x' + b - A * a
                    //    = A * x' + b'
                    // b' = b - A * a;

                    ContactConstraintPoint cp1 = c.points[0];
                    ContactConstraintPoint cp2 = c.points[1];

                    Vector2 a = new Vector2(cp1.normalImpulse, cp2.normalImpulse);
                    Debug.Assert(a.X >= 0.0f && a.Y >= 0.0f);

#if MATH_OVERLOADS
    // Relative velocity at contact
			        Vector2 dv1 = vB + MathUtils.Cross(wB, cp1.rB) - vA - MathUtils.Cross(wA, cp1.rA);
			        Vector2 dv2 = vB + MathUtils.Cross(wB, cp2.rB) - vA - MathUtils.Cross(wA, cp2.rA);

			        // Compute normal velocity
			        float vn1 = Vector2.Dot(dv1, normal);
			        float vn2 = Vector2.Dot(dv2, normal);

                    Vector2 b = new Vector2(vn1 - cp1.velocityBias, vn2 - cp2.velocityBias);
			        b -= MathUtils.Multiply(ref c.K, a);
#else
                    // Relative velocity at contact
                    Vector2 dv1 = new Vector2(vB.X + (-wB * cp1.rB.Y) - vA.X - (-wA * cp1.rA.Y),
                                              vB.Y + (wB * cp1.rB.X) - vA.Y - (wA * cp1.rA.X));
                    Vector2 dv2 = new Vector2(vB.X + (-wB * cp2.rB.Y) - vA.X - (-wA * cp2.rA.Y),
                                              vB.Y + (wB * cp2.rB.X) - vA.Y - (wA * cp2.rA.X));

                    // Compute normal velocity
                    float vn1 = dv1.X * normal.X + dv1.Y * normal.Y;
                    float vn2 = dv2.X * normal.X + dv2.Y * normal.Y;

                    Vector2 b = new Vector2(vn1 - cp1.velocityBias, vn2 - cp2.velocityBias);
                    b -= MathUtils.Multiply(ref c.K, a); // Inlining didn't help for the multiply.
#endif
                    while (true)
                    {
                        //
                        // Case 1: vn = 0
                        //
                        // 0 = A * x' + b'
                        //
                        // Solve for x':
                        //
                        // x' = - inv(A) * b'
                        //
                        Vector2 x = -MathUtils.Multiply(ref c.normalMass, b);

                        if (x.X >= 0.0f && x.Y >= 0.0f)
                        {
#if MATH_OVERLOADS
    // Resubstitute for the incremental impulse
					        Vector2 d = x - a;

					        // Apply incremental impulse
					        Vector2 P1 = d.X * normal;
					        Vector2 P2 = d.Y * normal;
					        vA -= invMassA * (P1 + P2);
					        wA -= invIA * (MathUtils.Cross(cp1.rA, P1) + MathUtils.Cross(cp2.rA, P2));

					        vB += invMassB * (P1 + P2);
					        wB += invIB * (MathUtils.Cross(cp1.rB, P1) + MathUtils.Cross(cp2.rB, P2));
#else
                            // Resubstitute for the incremental impulse
                            Vector2 d = new Vector2(x.X - a.X, x.Y - a.Y);

                            // Apply incremental impulse
                            Vector2 P1 = new Vector2(d.X * normal.X, d.X * normal.Y);
                            Vector2 P2 = new Vector2(d.Y * normal.X, d.Y * normal.Y);
                            Vector2 P12 = new Vector2(P1.X + P2.X, P1.Y + P2.Y);

                            vA.X -= invMassA * P12.X;
                            vA.Y -= invMassA * P12.Y;
                            wA -= invIA * ((cp1.rA.X * P1.Y - cp1.rA.Y * P1.X) + (cp2.rA.X * P2.Y - cp2.rA.Y * P2.X));

                            vB.X += invMassB * P12.X;
                            vB.Y += invMassB * P12.Y;
                            wB += invIB * ((cp1.rB.X * P1.Y - cp1.rB.Y * P1.X) + (cp2.rB.X * P2.Y - cp2.rB.Y * P2.X));
#endif
                            // Accumulate
                            cp1.normalImpulse = x.X;
                            cp2.normalImpulse = x.Y;

#if B2_DEBUG_SOLVER 
                            
			                float k_errorTol = 1e-3f;

					        // Postconditions
					        dv1 = vB + MathUtils.Cross(wB, cp1.rB) - vA - MathUtils.Cross(wA, cp1.rA);
					        dv2 = vB + MathUtils.Cross(wB, cp2.rB) - vA - MathUtils.Cross(wA, cp2.rA);

					        // Compute normal velocity
					        vn1 = Vector2.Dot(dv1, normal);
					        vn2 = Vector2.Dot(dv2, normal);

					        Debug.Assert(MathUtils.Abs(vn1 - cp1.velocityBias) < k_errorTol);
					        Debug.Assert(MathUtils.Abs(vn2 - cp2.velocityBias) < k_errorTol);
#endif
                            break;
                        }

                        //
                        // Case 2: vn1 = 0 and x2 = 0
                        //
                        //   0 = a11 * x1' + a12 * 0 + b1' 
                        // vn2 = a21 * x1' + a22 * 0 + b2'
                        //
                        x.X = -cp1.normalMass * b.X;
                        x.Y = 0.0f;
                        vn1 = 0.0f;
                        vn2 = c.K.col1.Y * x.X + b.Y;

                        if (x.X >= 0.0f && vn2 >= 0.0f)
                        {
#if MATH_OVERLOADS
    // Resubstitute for the incremental impulse
					        Vector2 d = x - a;

					        // Apply incremental impulse
					        Vector2 P1 = d.X * normal;
					        Vector2 P2 = d.Y * normal;
					        vA -= invMassA * (P1 + P2);
					        wA -= invIA * (MathUtils.Cross(cp1.rA, P1) + MathUtils.Cross(cp2.rA, P2));

					        vB += invMassB * (P1 + P2);
					        wB += invIB * (MathUtils.Cross(cp1.rB, P1) + MathUtils.Cross(cp2.rB, P2));
#else
                            // Resubstitute for the incremental impulse
                            Vector2 d = new Vector2(x.X - a.X, x.Y - a.Y);

                            // Apply incremental impulse
                            Vector2 P1 = new Vector2(d.X * normal.X, d.X * normal.Y);
                            Vector2 P2 = new Vector2(d.Y * normal.X, d.Y * normal.Y);
                            Vector2 P12 = new Vector2(P1.X + P2.X, P1.Y + P2.Y);

                            vA.X -= invMassA * P12.X;
                            vA.Y -= invMassA * P12.Y;
                            wA -= invIA * ((cp1.rA.X * P1.Y - cp1.rA.Y * P1.X) + (cp2.rA.X * P2.Y - cp2.rA.Y * P2.X));

                            vB.X += invMassB * P12.X;
                            vB.Y += invMassB * P12.Y;
                            wB += invIB * ((cp1.rB.X * P1.Y - cp1.rB.Y * P1.X) + (cp2.rB.X * P2.Y - cp2.rB.Y * P2.X));
#endif
                            // Accumulate
                            cp1.normalImpulse = x.X;
                            cp2.normalImpulse = x.Y;

#if B2_DEBUG_SOLVER 
    // Postconditions
					        dv1 = vB + MathUtils.Cross(wB, cp1.rB) - vA - MathUtils.Cross(wA, cp1.rA);

					        // Compute normal velocity
					        vn1 = Vector2.Dot(dv1, normal);

					        Debug.Assert(MathUtils.Abs(vn1 - cp1.velocityBias) < k_errorTol);
#endif
                            break;
                        }


                        //
                        // Case 3: vn2 = 0 and x1 = 0
                        //
                        // vn1 = a11 * 0 + a12 * x2' + b1' 
                        //   0 = a21 * 0 + a22 * x2' + b2'
                        //
                        x.X = 0.0f;
                        x.Y = -cp2.normalMass * b.Y;
                        vn1 = c.K.col2.X * x.Y + b.X;
                        vn2 = 0.0f;

                        if (x.Y >= 0.0f && vn1 >= 0.0f)
                        {
#if MATH_OVERLOADS
    // Resubstitute for the incremental impulse
					        Vector2 d = x - a;

					        // Apply incremental impulse
					        Vector2 P1 = d.X * normal;
					        Vector2 P2 = d.Y * normal;
					        vA -= invMassA * (P1 + P2);
					        wA -= invIA * (MathUtils.Cross(cp1.rA, P1) + MathUtils.Cross(cp2.rA, P2));

					        vB += invMassB * (P1 + P2);
					        wB += invIB * (MathUtils.Cross(cp1.rB, P1) + MathUtils.Cross(cp2.rB, P2));
#else
                            // Resubstitute for the incremental impulse
                            Vector2 d = new Vector2(x.X - a.X, x.Y - a.Y);

                            // Apply incremental impulse
                            Vector2 P1 = new Vector2(d.X * normal.X, d.X * normal.Y);
                            Vector2 P2 = new Vector2(d.Y * normal.X, d.Y * normal.Y);
                            Vector2 P12 = new Vector2(P1.X + P2.X, P1.Y + P2.Y);

                            vA.X -= invMassA * P12.X;
                            vA.Y -= invMassA * P12.Y;
                            wA -= invIA * ((cp1.rA.X * P1.Y - cp1.rA.Y * P1.X) + (cp2.rA.X * P2.Y - cp2.rA.Y * P2.X));

                            vB.X += invMassB * P12.X;
                            vB.Y += invMassB * P12.Y;
                            wB += invIB * ((cp1.rB.X * P1.Y - cp1.rB.Y * P1.X) + (cp2.rB.X * P2.Y - cp2.rB.Y * P2.X));
#endif
                            // Accumulate
                            cp1.normalImpulse = x.X;
                            cp2.normalImpulse = x.Y;

#if B2_DEBUG_SOLVER 
    // Postconditions
					        dv2 = vB + MathUtils.Cross(wB, cp2.rB) - vA - MathUtils.Cross(wA, cp2.rA);

					        // Compute normal velocity
					        vn2 = Vector2.Dot(dv2, normal);

					        Debug.Assert(MathUtils.Abs(vn2 - cp2.velocityBias) < k_errorTol);
#endif
                            break;
                        }

                        //
                        // Case 4: x1 = 0 and x2 = 0
                        // 
                        // vn1 = b1
                        // vn2 = b2;
                        x.X = 0.0f;
                        x.Y = 0.0f;
                        vn1 = b.X;
                        vn2 = b.Y;

                        if (vn1 >= 0.0f && vn2 >= 0.0f)
                        {
#if MATH_OVERLOADS
    // Resubstitute for the incremental impulse
					        Vector2 d = x - a;

					        // Apply incremental impulse
					        Vector2 P1 = d.X * normal;
					        Vector2 P2 = d.Y * normal;
					        vA -= invMassA * (P1 + P2);
					        wA -= invIA * (MathUtils.Cross(cp1.rA, P1) + MathUtils.Cross(cp2.rA, P2));

					        vB += invMassB * (P1 + P2);
					        wB += invIB * (MathUtils.Cross(cp1.rB, P1) + MathUtils.Cross(cp2.rB, P2));
#else
                            // Resubstitute for the incremental impulse
                            Vector2 d = new Vector2(x.X - a.X, x.Y - a.Y);

                            // Apply incremental impulse
                            Vector2 P1 = new Vector2(d.X * normal.X, d.X * normal.Y);
                            Vector2 P2 = new Vector2(d.Y * normal.X, d.Y * normal.Y);
                            Vector2 P12 = new Vector2(P1.X + P2.X, P1.Y + P2.Y);

                            vA.X -= invMassA * P12.X;
                            vA.Y -= invMassA * P12.Y;
                            wA -= invIA * ((cp1.rA.X * P1.Y - cp1.rA.Y * P1.X) + (cp2.rA.X * P2.Y - cp2.rA.Y * P2.X));

                            vB.X += invMassB * P12.X;
                            vB.Y += invMassB * P12.Y;
                            wB += invIB * ((cp1.rB.X * P1.Y - cp1.rB.Y * P1.X) + (cp2.rB.X * P2.Y - cp2.rB.Y * P2.X));
#endif
                            // Accumulate
                            cp1.normalImpulse = x.X;
                            cp2.normalImpulse = x.Y;

                            break;
                        }

                        // No solution, give up. This is hit sometimes, but it doesn't seem to matter.
                        break;
                    }

                    c.points[0] = cp1;
                    c.points[1] = cp2;
                }

                _constraints[i] = c;

                bodyA._linearVelocity = vA;
                bodyA._angularVelocity = wA;
                bodyB._linearVelocity = vB;
                bodyB._angularVelocity = wB;
            }
        }

        public void StoreImpulses()
        {
            for (int i = 0; i < _constraintCount; ++i)
            {
                ContactConstraint c = _constraints[i];
                Manifold m = c.manifold;

                for (int j = 0; j < c.pointCount; ++j)
                {
                    var pj = m.Points[j];
                    var cp = c.points[j];

                    pj.NormalImpulse = cp.normalImpulse;
                    pj.TangentImpulse = cp.tangentImpulse;

                    m.Points[j] = pj;
                }

                // TODO: look for better ways of doing this.
                c.manifold = m;
                _constraints[i] = c;
                _contacts[i]._manifold = m;
            }
        }

        public bool SolvePositionConstraints(float baumgarte)
        {
            float minSeparation = 0.0f;

            for (int i = 0; i < _constraintCount; ++i)
            {
                ContactConstraint c = _constraints[i];

                Body bodyA = c.bodyA;
                Body bodyB = c.bodyB;

                float invMassA = bodyA._mass * bodyA._invMass;
                float invIA = bodyA._mass * bodyA._invI;
                float invMassB = bodyB._mass * bodyB._invMass;
                float invIB = bodyB._mass * bodyB._invI;

                // Solve normal constraints
                for (int j = 0; j < c.pointCount; ++j)
                {
                    PositionSolverManifold psm = new PositionSolverManifold(ref c, j);
                    Vector2 normal = psm._normal;

                    Vector2 point = psm._point;
                    float separation = psm._separation;

                    Vector2 rA = point - bodyA._sweep.c;
                    Vector2 rB = point - bodyB._sweep.c;

                    // Track max constraint error.
                    minSeparation = Math.Min(minSeparation, separation);

                    // Prevent large corrections and allow slop.
                    float C = MathUtils.Clamp(baumgarte * (separation + Settings.LinearSlop),
                                              -Settings.MaxLinearCorrection, 0.0f);

                    // Compute the effective mass.
                    float rnA = MathUtils.Cross(rA, normal);
                    float rnB = MathUtils.Cross(rB, normal);
                    float K = invMassA + invMassB + invIA * rnA * rnA + invIB * rnB * rnB;

                    // Compute normal impulse
                    float impulse = K > 0.0f ? -C / K : 0.0f;

#if MATH_OVERLOADS
			        Vector2 P = impulse * normal;

			        bodyA._sweep.c -= invMassA * P;
			        bodyA._sweep.a -= invIA * MathUtils.Cross(rA, P);
			        
			        bodyB._sweep.c += invMassB * P;
			        bodyB._sweep.a += invIB * MathUtils.Cross(rB, P);
#else
                    Vector2 P = new Vector2(impulse * normal.X, impulse * normal.Y);

                    bodyA._sweep.c.X -= invMassA * P.X;
                    bodyA._sweep.c.Y -= invMassA * P.Y;
                    bodyA._sweep.a -= invIA * (rA.X * P.Y - rA.Y * P.X);

                    bodyB._sweep.c.X += invMassB * P.X;
                    bodyB._sweep.c.Y += invMassB * P.Y;
                    bodyB._sweep.a += invIB * (rB.X * P.Y - rB.Y * P.X);
#endif
                    bodyA.SynchronizeTransform();
                    bodyB.SynchronizeTransform();
                }
            }

            // We can't expect minSpeparation >= -Settings.b2_linearSlop because we don't
            // push the separation above -Settings.b2_linearSlop.
            return minSeparation >= -1.5f * Settings.LinearSlop;
        }
    }

    internal struct PositionSolverManifold
    {
        internal Vector2 _normal;
        internal Vector2 _point;
        internal float _separation;

        internal PositionSolverManifold(ref ContactConstraint cc, int index)
        {
            Debug.Assert(cc.pointCount > 0);

            switch (cc.type)
            {
                case ManifoldType.Circles:
                    {
                        Vector2 pointA = cc.bodyA.GetWorldPoint(cc.localPoint);
                        Vector2 pointB = cc.bodyB.GetWorldPoint(cc.points[0].localPoint);
                        if (Vector2.DistanceSquared(pointA, pointB) > Settings.Epsilon * Settings.Epsilon)
                        {
                            _normal = pointB - pointA;
                            _normal.Normalize();
                        }
                        else
                        {
                            _normal = new Vector2(1.0f, 0.0f);
                        }

                        _point = 0.5f * (pointA + pointB);
                        _separation = Vector2.Dot(pointB - pointA, _normal) - cc.radius;
                    }
                    break;

                case ManifoldType.FaceA:
                    {
                        _normal = cc.bodyA.GetWorldVector(cc.localNormal);
                        Vector2 planePoint = cc.bodyA.GetWorldPoint(cc.localPoint);

                        Vector2 clipPoint = cc.bodyB.GetWorldPoint(cc.points[index].localPoint);
                        _separation = Vector2.Dot(clipPoint - planePoint, _normal) - cc.radius;
                        _point = clipPoint;
                    }
                    break;

                case ManifoldType.FaceB:
                    {
                        _normal = cc.bodyB.GetWorldVector(cc.localNormal);
                        Vector2 planePoint = cc.bodyB.GetWorldPoint(cc.localPoint);

                        Vector2 clipPoint = cc.bodyA.GetWorldPoint(cc.points[index].localPoint);
                        _separation = Vector2.Dot(clipPoint - planePoint, _normal) - cc.radius;
                        _point = clipPoint;

                        // Ensure normal points from A to B
                        _normal = -_normal;
                    }
                    break;
                default:
                    _normal = Vector2.Zero;
                    _point = Vector2.Zero;
                    _separation = 0.0f;
                    break;
            }
        }
    }
}