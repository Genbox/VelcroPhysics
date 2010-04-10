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
    public class ContactConstraintPoint
    {
        public Vector2 LocalPoint;
        public float NormalImpulse;
        public float NormalMass;
        public Vector2 RA;
        public Vector2 RB;
        public float TangentImpulse;
        public float TangentMass;
        public float VelocityBias;
    } ;

    public class ContactConstraint
    {
        public Body BodyA;
        public Body BodyB;
        public float Friction;
        public Mat22 K;
        public Vector2 LocalNormal;
        public Vector2 LocalPoint;
        public Manifold Manifold;
        public Vector2 Normal;
        public Mat22 NormalMass;
        public int PointCount;
        public ContactConstraintPoint[] Points = new ContactConstraintPoint[2];
        public float Radius;
        public ManifoldType Type;

        public ContactConstraint()
        {
            Points[0] = new ContactConstraintPoint();
            Points[1] = new ContactConstraintPoint();
        }
    } ;

    public class ContactSolver
    {
        internal int ConstraintCount; // collection can be bigger.
        internal ContactConstraint[] Constraints;
        private Contact[] _contacts;

        private Vector2 _normal;
        private Vector2 _point;
        private float _separation;

        public void Reset(Contact[] contacts, int contactCount, float impulseRatio)
        {
            _contacts = contacts;
            ConstraintCount = contactCount;

            // grow the array
            if (Constraints == null || Constraints.Length < ConstraintCount)
            {
                Constraints = new ContactConstraint[ConstraintCount * 2];

                for (int i = 0; i < ConstraintCount * 2; i++)
                {
                    Constraints[i] = new ContactConstraint();
                }
            }

            for (int i = 0; i < ConstraintCount; ++i)
            {
                Contact contact = contacts[i];

                Fixture fixtureA = contact.FixtureA;
                Fixture fixtureB = contact.FixtureB;
                Shape shapeA = fixtureA.Shape;
                Shape shapeB = fixtureB.Shape;
                float radiusA = shapeA.Radius;
                float radiusB = shapeB.Radius;
                Body bodyA = fixtureA.Body;
                Body bodyB = fixtureB.Body;
                Manifold manifold = contact.Manifold;

                float friction = Settings.MixFriction(fixtureA.Friction, fixtureB.Friction);
                float restitution = Settings.MixRestitution(fixtureA.Restitution, fixtureB.Restitution);

                Vector2 vA = bodyA._linearVelocity;
                Vector2 vB = bodyB._linearVelocity;
                float wA = bodyA._angularVelocity;
                float wB = bodyB._angularVelocity;

                Debug.Assert(manifold.PointCount > 0);

                WorldManifold worldManifold = new WorldManifold(ref manifold, ref bodyA._xf, radiusA, ref bodyB._xf,
                                                                radiusB);


                ContactConstraint cc = Constraints[i];
                cc.BodyA = bodyA;
                cc.BodyB = bodyB;
                cc.Manifold = manifold;
                cc.Normal = worldManifold.Normal;
                cc.PointCount = manifold.PointCount;
                cc.Friction = friction;

                cc.LocalNormal = manifold.LocalNormal;
                cc.LocalPoint = manifold.LocalPoint;
                cc.Radius = radiusA + radiusB;
                cc.Type = manifold.Type;

                for (int j = 0; j < cc.PointCount; ++j)
                {
                    ManifoldPoint cp = manifold.Points[j];
                    ContactConstraintPoint ccp = cc.Points[j];

                    ccp.NormalImpulse = impulseRatio * cp.NormalImpulse;
                    ccp.TangentImpulse = impulseRatio * cp.TangentImpulse;

                    ccp.LocalPoint = cp.LocalPoint;

                    ccp.RA = worldManifold.Points[j] - bodyA._sweep.Center;
                    ccp.RB = worldManifold.Points[j] - bodyB._sweep.Center;

#if MATH_OVERLOADS
			        float rnA = MathUtils.Cross(ccp.rA, cc.normal);
			        float rnB = MathUtils.Cross(ccp.rB, cc.normal);
#else
                    float rnA = ccp.RA.X * cc.Normal.Y - ccp.RA.Y * cc.Normal.X;
                    float rnB = ccp.RB.X * cc.Normal.Y - ccp.RB.Y * cc.Normal.X;
#endif
                    rnA *= rnA;
                    rnB *= rnB;

                    float kNormal = bodyA._invMass + bodyB._invMass + bodyA._invI * rnA + bodyB._invI * rnB;

                    Debug.Assert(kNormal > Settings.Epsilon);
                    ccp.NormalMass = 1.0f / kNormal;

#if MATH_OVERLOADS
			        Vector2 tangent = MathUtils.Cross(cc.normal, 1.0f);

			        float rtA = MathUtils.Cross(ccp.rA, tangent);
			        float rtB = MathUtils.Cross(ccp.rB, tangent);
#else
                    Vector2 tangent = new Vector2(cc.Normal.Y, -cc.Normal.X);

                    float rtA = ccp.RA.X * tangent.Y - ccp.RA.Y * tangent.X;
                    float rtB = ccp.RB.X * tangent.Y - ccp.RB.Y * tangent.X;
#endif
                    rtA *= rtA;
                    rtB *= rtB;
                    float kTangent = bodyA._invMass + bodyB._invMass + bodyA._invI * rtA + bodyB._invI * rtB;

                    Debug.Assert(kTangent > Settings.Epsilon);
                    ccp.TangentMass = 1.0f / kTangent;

                    // Setup a velocity bias for restitution.
                    ccp.VelocityBias = 0.0f;
                    float vRel = Vector2.Dot(cc.Normal,
                                             vB + MathUtils.Cross(wB, ccp.RB) - vA - MathUtils.Cross(wA, ccp.RA));
                    if (vRel < -Settings.VelocityThreshold)
                    {
                        ccp.VelocityBias = -restitution * vRel;
                    }
                }

                // If we have two points, then prepare the block solver.
                if (cc.PointCount == 2)
                {
                    ContactConstraintPoint ccp1 = cc.Points[0];
                    ContactConstraintPoint ccp2 = cc.Points[1];

                    float invMassA = bodyA._invMass;
                    float invIA = bodyA._invI;
                    float invMassB = bodyB._invMass;
                    float invIB = bodyB._invI;

                    float rn1A = MathUtils.Cross(ccp1.RA, cc.Normal);
                    float rn1B = MathUtils.Cross(ccp1.RB, cc.Normal);
                    float rn2A = MathUtils.Cross(ccp2.RA, cc.Normal);
                    float rn2B = MathUtils.Cross(ccp2.RB, cc.Normal);

                    float k11 = invMassA + invMassB + invIA * rn1A * rn1A + invIB * rn1B * rn1B;
                    float k22 = invMassA + invMassB + invIA * rn2A * rn2A + invIB * rn2B * rn2B;
                    float k12 = invMassA + invMassB + invIA * rn1A * rn2A + invIB * rn1B * rn2B;

                    // Ensure a reasonable condition number.
                    const float k_maxConditionNumber = 100.0f;
                    if (k11 * k11 < k_maxConditionNumber * (k11 * k22 - k12 * k12))
                    {
                        // K is safe to invert.
                        cc.K = new Mat22(new Vector2(k11, k12), new Vector2(k12, k22));
                        cc.NormalMass = cc.K.GetInverse();
                    }
                    else
                    {
                        // The constraints are redundant, just use one.
                        // TODO_ERIN use deepest?
                        cc.PointCount = 1;
                    }
                }
            }
        }

        public void WarmStart()
        {
            // Warm start.
            for (int i = 0; i < ConstraintCount; ++i)
            {
                ContactConstraint c = Constraints[i];

                Body bodyA = c.BodyA;
                Body bodyB = c.BodyB;
                float invMassA = bodyA._invMass;
                float invIA = bodyA._invI;
                float invMassB = bodyB._invMass;
                float invIB = bodyB._invI;
                Vector2 normal = c.Normal;

#if MATH_OVERLOADS
	            Vector2 tangent = MathUtils.Cross(normal, 1.0f);
#else
                Vector2 tangent = new Vector2(normal.Y, -normal.X);
#endif

                for (int j = 0; j < c.PointCount; ++j)
                {
                    ContactConstraintPoint ccp = c.Points[j];
#if MATH_OVERLOADS
		            Vector2 P = ccp.normalImpulse * normal + ccp.tangentImpulse * tangent;
		            bodyA._angularVelocity -= invIA * MathUtils.Cross(ccp.rA, P);
		            bodyA._linearVelocity -= invMassA * P;
		            bodyB._angularVelocity += invIB * MathUtils.Cross(ccp.rB, P);
		            bodyB._linearVelocity += invMassB * P;
#else
                    Vector2 P = new Vector2(ccp.NormalImpulse * normal.X + ccp.TangentImpulse * tangent.X,
                                            ccp.NormalImpulse * normal.Y + ccp.TangentImpulse * tangent.Y);
                    bodyA._angularVelocity -= invIA * (ccp.RA.X * P.Y - ccp.RA.Y * P.X);
                    bodyA._linearVelocity.X -= invMassA * P.X;
                    bodyA._linearVelocity.Y -= invMassA * P.Y;
                    bodyB._angularVelocity += invIB * (ccp.RB.X * P.Y - ccp.RB.Y * P.X);
                    bodyB._linearVelocity.X += invMassB * P.X;
                    bodyB._linearVelocity.Y += invMassB * P.Y;
#endif
                }
            }
        }

        public void SolveVelocityConstraints()
        {
            for (int i = 0; i < ConstraintCount; ++i)
            {
                ContactConstraint c = Constraints[i];
                Body bodyA = c.BodyA;
                Body bodyB = c.BodyB;
                float wA = bodyA._angularVelocity;
                float wB = bodyB._angularVelocity;
                Vector2 vA = bodyA._linearVelocity;
                Vector2 vB = bodyB._linearVelocity;
                float invMassA = bodyA._invMass;
                float invIA = bodyA._invI;
                float invMassB = bodyB._invMass;
                float invIB = bodyB._invI;
                Vector2 normal = c.Normal;

#if MATH_OVERLOADS
				Vector2 tangent = ZoomEngine.Physics.Common.Math.Cross(normal, 1.0f);
#else
                Vector2 tangent = new Vector2(normal.Y, -normal.X);
#endif
                float friction = c.Friction;

                Debug.Assert(c.PointCount == 1 || c.PointCount == 2);

                // Solve tangent constraints
                for (int j = 0; j < c.PointCount; ++j)
                {
                    ContactConstraintPoint ccp = c.Points[j];

#if MATH_OVERLOADS
    // Relative velocity at contact
			        Vector2 dv = vB + MathUtils.Cross(wB, ccp.rB) - vA - MathUtils.Cross(wA, ccp.rA);

			        // Compute tangent force
			        float vt = Vector2.Dot(dv, tangent);
#else
                    // Relative velocity at contact
                    Vector2 dv = new Vector2(vB.X + (-wB * ccp.RB.Y) - vA.X - (-wA * ccp.RA.Y),
                                             vB.Y + (wB * ccp.RB.X) - vA.Y - (wA * ccp.RA.X));

                    // Compute tangent force
                    float vt = dv.X * tangent.X + dv.Y * tangent.Y;
#endif
                    float lambda = ccp.TangentMass * (-vt);

                    // MathUtils.Clamp the accumulated force
                    float maxFriction = friction * ccp.NormalImpulse;
                    float newImpulse = MathUtils.Clamp(ccp.TangentImpulse + lambda, -maxFriction, maxFriction);
                    lambda = newImpulse - ccp.TangentImpulse;

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
                    wA -= invIA * (ccp.RA.X * P.Y - ccp.RA.Y * P.X);

                    vB.X += invMassB * P.X;
                    vB.Y += invMassB * P.Y;
                    wB += invIB * (ccp.RB.X * P.Y - ccp.RB.Y * P.X);
#endif
                    ccp.TangentImpulse = newImpulse;
                }

                // Solve normal constraints
                if (c.PointCount == 1)
                {
                    ContactConstraintPoint ccp = c.Points[0];

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
                    Vector2 dv = new Vector2(vB.X + (-wB * ccp.RB.Y) - vA.X - (-wA * ccp.RA.Y),
                                             vB.Y + (wB * ccp.RB.X) - vA.Y - (wA * ccp.RA.X));

                    // Compute normal impulse
                    float vn = dv.X * normal.X + dv.Y * normal.Y;
                    float lambda = -ccp.NormalMass * (vn - ccp.VelocityBias);

                    // Clamp the accumulated impulse
                    float newImpulse = Math.Max(ccp.NormalImpulse + lambda, 0.0f);
                    lambda = newImpulse - ccp.NormalImpulse;

                    // Apply contact impulse
                    var P = new Vector2(lambda * normal.X, lambda * normal.Y);

                    vA.X -= invMassA * P.X;
                    vA.Y -= invMassA * P.Y;
                    wA -= invIA * (ccp.RA.X * P.Y - ccp.RA.Y * P.X);

                    vB.X += invMassB * P.X;
                    vB.Y += invMassB * P.Y;
                    wB += invIB * (ccp.RB.X * P.Y - ccp.RB.Y * P.X);
#endif
                    ccp.NormalImpulse = newImpulse;
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

                    ContactConstraintPoint cp1 = c.Points[0];
                    ContactConstraintPoint cp2 = c.Points[1];

                    Vector2 a = new Vector2(cp1.NormalImpulse, cp2.NormalImpulse);
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
                    Vector2 dv1 = new Vector2(vB.X + (-wB * cp1.RB.Y) - vA.X - (-wA * cp1.RA.Y),
                                              vB.Y + (wB * cp1.RB.X) - vA.Y - (wA * cp1.RA.X));
                    Vector2 dv2 = new Vector2(vB.X + (-wB * cp2.RB.Y) - vA.X - (-wA * cp2.RA.Y),
                                              vB.Y + (wB * cp2.RB.X) - vA.Y - (wA * cp2.RA.X));

                    // Compute normal velocity
                    float vn1 = dv1.X * normal.X + dv1.Y * normal.Y;
                    float vn2 = dv2.X * normal.X + dv2.Y * normal.Y;

                    Vector2 b = new Vector2(vn1 - cp1.VelocityBias, vn2 - cp2.VelocityBias);
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
                        Vector2 x = -MathUtils.Multiply(ref c.NormalMass, b);

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
                            wA -= invIA * ((cp1.RA.X * P1.Y - cp1.RA.Y * P1.X) + (cp2.RA.X * P2.Y - cp2.RA.Y * P2.X));

                            vB.X += invMassB * P12.X;
                            vB.Y += invMassB * P12.Y;
                            wB += invIB * ((cp1.RB.X * P1.Y - cp1.RB.Y * P1.X) + (cp2.RB.X * P2.Y - cp2.RB.Y * P2.X));
#endif
                            // Accumulate
                            cp1.NormalImpulse = x.X;
                            cp2.NormalImpulse = x.Y;

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
                        x.X = -cp1.NormalMass * b.X;
                        x.Y = 0.0f;
                        vn1 = 0.0f;
                        vn2 = c.K.Col1.Y * x.X + b.Y;

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
                            wA -= invIA * ((cp1.RA.X * P1.Y - cp1.RA.Y * P1.X) + (cp2.RA.X * P2.Y - cp2.RA.Y * P2.X));

                            vB.X += invMassB * P12.X;
                            vB.Y += invMassB * P12.Y;
                            wB += invIB * ((cp1.RB.X * P1.Y - cp1.RB.Y * P1.X) + (cp2.RB.X * P2.Y - cp2.RB.Y * P2.X));
#endif
                            // Accumulate
                            cp1.NormalImpulse = x.X;
                            cp2.NormalImpulse = x.Y;

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
                        x.Y = -cp2.NormalMass * b.Y;
                        vn1 = c.K.Col2.X * x.Y + b.X;
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
                            wA -= invIA * ((cp1.RA.X * P1.Y - cp1.RA.Y * P1.X) + (cp2.RA.X * P2.Y - cp2.RA.Y * P2.X));

                            vB.X += invMassB * P12.X;
                            vB.Y += invMassB * P12.Y;
                            wB += invIB * ((cp1.RB.X * P1.Y - cp1.RB.Y * P1.X) + (cp2.RB.X * P2.Y - cp2.RB.Y * P2.X));
#endif
                            // Accumulate
                            cp1.NormalImpulse = x.X;
                            cp2.NormalImpulse = x.Y;

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
                            wA -= invIA * ((cp1.RA.X * P1.Y - cp1.RA.Y * P1.X) + (cp2.RA.X * P2.Y - cp2.RA.Y * P2.X));

                            vB.X += invMassB * P12.X;
                            vB.Y += invMassB * P12.Y;
                            wB += invIB * ((cp1.RB.X * P1.Y - cp1.RB.Y * P1.X) + (cp2.RB.X * P2.Y - cp2.RB.Y * P2.X));
#endif
                            // Accumulate
                            cp1.NormalImpulse = x.X;
                            cp2.NormalImpulse = x.Y;

                            break;
                        }

                        // No solution, give up. This is hit sometimes, but it doesn't seem to matter.
                        break;
                    }
                }

                bodyA._linearVelocity = vA;
                bodyA._angularVelocity = wA;
                bodyB._linearVelocity = vB;
                bodyB._angularVelocity = wB;
            }
        }

        public void StoreImpulses()
        {
            for (int i = 0; i < ConstraintCount; ++i)
            {
                ContactConstraint c = Constraints[i];
                Manifold m = c.Manifold;

                for (int j = 0; j < c.PointCount; ++j)
                {
                    ManifoldPoint pj = m.Points[j];
                    ContactConstraintPoint cp = c.Points[j];

                    pj.NormalImpulse = cp.NormalImpulse;
                    pj.TangentImpulse = cp.TangentImpulse;

                    m.Points[j] = pj;
                }

                // TODO: look for better ways of doing this.
                c.Manifold = m;
                _contacts[i].Manifold = m;
            }
        }

        public bool SolvePositionConstraints(float baumgarte)
        {
            float minSeparation = 0.0f;

            for (int i = 0; i < ConstraintCount; ++i)
            {
                ContactConstraint c = Constraints[i];

                Body bodyA = c.BodyA;
                Body bodyB = c.BodyB;

                float invMassA = bodyA.Mass * bodyA._invMass;
                float invIA = bodyA.Mass * bodyA._invI;
                float invMassB = bodyB.Mass * bodyB._invMass;
                float invIB = bodyB.Mass * bodyB._invI;

                // Solve normal constraints
                for (int j = 0; j < c.PointCount; ++j)
                {
                    Debug.Assert(c.PointCount > 0);

                    switch (c.Type)
                    {
                        case ManifoldType.Circles:
                            {
                                Vector2 pointA = c.BodyA.GetWorldPoint(c.LocalPoint);
                                Vector2 pointB = c.BodyB.GetWorldPoint(c.Points[0].LocalPoint);
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
                                _separation = Vector2.Dot(pointB - pointA, _normal) - c.Radius;
                            }
                            break;

                        case ManifoldType.FaceA:
                            {
                                _normal = c.BodyA.GetWorldVector(c.LocalNormal);
                                Vector2 planePoint = c.BodyA.GetWorldPoint(c.LocalPoint);

                                Vector2 clipPoint = c.BodyB.GetWorldPoint(c.Points[j].LocalPoint);
                                _separation = Vector2.Dot(clipPoint - planePoint, _normal) - c.Radius;
                                _point = clipPoint;
                            }
                            break;

                        case ManifoldType.FaceB:
                            {
                                _normal = c.BodyB.GetWorldVector(c.LocalNormal);
                                Vector2 planePoint = c.BodyB.GetWorldPoint(c.LocalPoint);

                                Vector2 clipPoint = c.BodyA.GetWorldPoint(c.Points[j].LocalPoint);
                                _separation = Vector2.Dot(clipPoint - planePoint, _normal) - c.Radius;
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

                    Vector2 rA = _point - bodyA._sweep.Center;
                    Vector2 rB = _point - bodyB._sweep.Center;

                    // Track max constraint error.
                    minSeparation = Math.Min(minSeparation, _separation);

                    // Prevent large corrections and allow slop.
                    float C = MathUtils.Clamp(baumgarte * (_separation + Settings.LinearSlop),
                                              -Settings.MaxLinearCorrection, 0.0f);

                    // Compute the effective mass.
                    float rnA = MathUtils.Cross(rA, _normal);
                    float rnB = MathUtils.Cross(rB, _normal);
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
                    Vector2 P = new Vector2(impulse * _normal.X, impulse * _normal.Y);

                    bodyA._sweep.Center.X -= invMassA * P.X;
                    bodyA._sweep.Center.Y -= invMassA * P.Y;
                    bodyA._sweep.Angle -= invIA * (rA.X * P.Y - rA.Y * P.X);

                    bodyB._sweep.Center.X += invMassB * P.X;
                    bodyB._sweep.Center.Y += invMassB * P.Y;
                    bodyB._sweep.Angle += invIB * (rB.X * P.Y - rB.Y * P.X);
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
}