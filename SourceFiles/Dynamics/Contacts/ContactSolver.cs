/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
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
    public sealed class ContactConstraintPoint
    {
        public Vector2 LocalPoint;
        public float NormalImpulse;
        public float NormalMass;
        public float TangentImpulse;
        public float TangentMass;
        public float VelocityBias;
        public Vector2 rA;
        public Vector2 rB;
    }

    public class ContactConstraint
    {
        public Body BodyA;
        public Body BodyB;
        public float Friction;
        public float Restitution;
        public Mat22 K;
        public Vector2 LocalNormal;
        public Vector2 LocalPoint;
        public Manifold Manifold;
        public Vector2 Normal;
        public Mat22 NormalMass;
        public int PointCount;
        public FixedArray2<ContactConstraintPoint> Points;
        public float RadiusA;
        public float RadiusB;
        public ManifoldType Type;

        public ContactConstraint()
        {
            Points[0] = new ContactConstraintPoint();
            Points[1] = new ContactConstraintPoint();
        }
    }

    public class ContactSolver
    {
        public ContactConstraint[] Constraints;
        private int _constraintCount; // collection can be bigger.
        private Contact[] _contacts;

        public void Reset(Contact[] contacts, int contactCount, float impulseRatio)
        {
            _contacts = contacts;

            _constraintCount = contactCount;

            // grow the array
            if (Constraints == null || Constraints.Length < _constraintCount)
            {
                Constraints = new ContactConstraint[_constraintCount * 2];

                for (int i = 0; i < _constraintCount * 2; i++)
                {
                    Constraints[i] = new ContactConstraint();
                }
            }

            // Initialize position independent portions of the constraints.
            for (int i = 0; i < _constraintCount; ++i)
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
                Manifold manifold;
                contact.GetManifold(out manifold);

                Debug.Assert(manifold.PointCount > 0);

                ContactConstraint cc = Constraints[i];
                cc.Friction = Settings.MixFriction(fixtureA.Friction, fixtureB.Friction);
                cc.Restitution = Settings.MixRestitution(fixtureA.Restitution, fixtureB.Restitution);
                cc.BodyA = bodyA;
                cc.BodyB = bodyB;
                cc.Manifold = manifold;
                cc.Normal = Vector2.Zero;
                cc.PointCount = manifold.PointCount;

                cc.LocalNormal = manifold.LocalNormal;
                cc.LocalPoint = manifold.LocalPoint;
                cc.RadiusA = radiusA;
                cc.RadiusB = radiusB;
                cc.Type = manifold.Type;

                for (int j = 0; j < cc.PointCount; ++j)
                {
                    ManifoldPoint cp = manifold.Points[j];
                    ContactConstraintPoint ccp = cc.Points[j];

                    ccp.NormalImpulse = impulseRatio * cp.NormalImpulse;
                    ccp.TangentImpulse = impulseRatio * cp.TangentImpulse;

                    ccp.LocalPoint = cp.LocalPoint;
                    ccp.rA = Vector2.Zero;
			        ccp.rB = Vector2.Zero;
			        ccp.NormalMass = 0.0f;
			        ccp.TangentMass = 0.0f;
			        ccp.VelocityBias = 0.0f;
                }

		        cc.K.SetZero();
		        cc.NormalMass.SetZero();
            }
        }

        public void InitializeVelocityConstraints()
        {
            for (int i = 0; i < _constraintCount; ++i)
            {
                ContactConstraint cc = Constraints[i];

                float radiusA = cc.RadiusA;
                float radiusB = cc.RadiusB;
                Body bodyA = cc.BodyA;
                Body bodyB = cc.BodyB;
                Manifold manifold = cc.Manifold;

                Vector2 vA = bodyA.LinearVelocity;
                Vector2 vB = bodyB.LinearVelocity;
                float wA = bodyA.AngularVelocity;
                float wB = bodyB.AngularVelocity;

                Debug.Assert(manifold.PointCount > 0);

                WorldManifold worldManifold = new WorldManifold(ref manifold, ref bodyA.Xf, radiusA, ref bodyB.Xf, radiusB);

                cc.Normal = worldManifold.Normal;

                for (int j = 0; j < cc.PointCount; ++j)
                {
                    ManifoldPoint cp = manifold.Points[j];
                    ContactConstraintPoint ccp = cc.Points[j];

                    ccp.rA = worldManifold.Points[j] - bodyA.Sweep.c;
                    ccp.rB = worldManifold.Points[j] - bodyB.Sweep.c;
#if MATH_OVERLOADS
			        float rnA = MathUtils.Cross(ccp.rA, cc.Normal);
			        float rnB = MathUtils.Cross(ccp.rB, cc.Normal);
#else
                    float rnA = ccp.rA.X * cc.Normal.Y - ccp.rA.Y * cc.Normal.X;
                    float rnB = ccp.rB.X * cc.Normal.Y - ccp.rB.Y * cc.Normal.X;
#endif
                    rnA *= rnA;
                    rnB *= rnB;

                    float kNormal = bodyA.InvMass + bodyB.InvMass + bodyA.InvI * rnA + bodyB.InvI * rnB;

                    Debug.Assert(kNormal > Settings.Epsilon);
                    ccp.NormalMass = 1.0f / kNormal;

#if MATH_OVERLOADS
			        Vector2 tangent = MathUtils.Cross(cc.Normal, 1.0f);

			        float rtA = MathUtils.Cross(ccp.rA, tangent);
			        float rtB = MathUtils.Cross(ccp.rB, tangent);
#else
                    Vector2 tangent = new Vector2(cc.Normal.Y, -cc.Normal.X);

                    float rtA = ccp.rA.X * tangent.Y - ccp.rA.Y * tangent.X;
                    float rtB = ccp.rB.X * tangent.Y - ccp.rB.Y * tangent.X;
#endif
                    rtA *= rtA;
                    rtB *= rtB;
                    float kTangent = bodyA.InvMass + bodyB.InvMass + bodyA.InvI * rtA + bodyB.InvI * rtB;

                    Debug.Assert(kTangent > Settings.Epsilon);
                    ccp.TangentMass = 1.0f / kTangent;

                    // Setup a velocity bias for restitution.
                    ccp.VelocityBias = 0.0f;
                    float vRel = Vector2.Dot(cc.Normal,
                                             vB + MathUtils.Cross(wB, ccp.rB) - vA - MathUtils.Cross(wA, ccp.rA));
                    if (vRel < -Settings.VelocityThreshold)
                    {
                        ccp.VelocityBias = -cc.Restitution * vRel;
                    }
                }

                // If we have two points, then prepare the block solver.
                if (cc.PointCount == 2)
                {
                    ContactConstraintPoint ccp1 = cc.Points[0];
                    ContactConstraintPoint ccp2 = cc.Points[1];

                    float invMassA = bodyA.InvMass;
                    float invIA = bodyA.InvI;
                    float invMassB = bodyB.InvMass;
                    float invIB = bodyB.InvI;

                    float rn1A = MathUtils.Cross(ccp1.rA, cc.Normal);
                    float rn1B = MathUtils.Cross(ccp1.rB, cc.Normal);
                    float rn2A = MathUtils.Cross(ccp2.rA, cc.Normal);
                    float rn2B = MathUtils.Cross(ccp2.rB, cc.Normal);

                    float k11 = invMassA + invMassB + invIA * rn1A * rn1A + invIB * rn1B * rn1B;
                    float k22 = invMassA + invMassB + invIA * rn2A * rn2A + invIB * rn2B * rn2B;
                    float k12 = invMassA + invMassB + invIA * rn1A * rn2A + invIB * rn1B * rn2B;

                    // Ensure a reasonable condition number.
                    const float k_maxConditionNumber = 100.0f;
                    if (k11 * k11 < k_maxConditionNumber * (k11 * k22 - k12 * k12))
                    {
                        // K is safe to invert.
                        cc.K = new Mat22(new Vector2(k11, k12), new Vector2(k12, k22));
                        cc.NormalMass = cc.K.Inverse;
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
            for (int i = 0; i < _constraintCount; ++i)
            {
                ContactConstraint c = Constraints[i];

                Body bodyA = c.BodyA;
                Body bodyB = c.BodyB;
                float invMassA = bodyA.InvMass;
                float invIA = bodyA.InvI;
                float invMassB = bodyB.InvMass;
                float invIB = bodyB.InvI;
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
		            Vector2 P = ccp.NormalImpulse * normal + ccp.TangentImpulse * tangent;
                    bodyA.AngularVelocityInternal -= invIA * MathUtils.Cross(ccp.rA, P);
                    bodyA.LinearVelocityInternal -= invMassA * P;
                    bodyB.AngularVelocityInternal += invIB * MathUtils.Cross(ccp.rB, P);
                    bodyB.LinearVelocityInternal += invMassB * P;
#else
                    Vector2 P = new Vector2(ccp.NormalImpulse * normal.X + ccp.TangentImpulse * tangent.X,
                                            ccp.NormalImpulse * normal.Y + ccp.TangentImpulse * tangent.Y);
                    bodyA.AngularVelocityInternal -= invIA * (ccp.rA.X * P.Y - ccp.rA.Y * P.X);
                    bodyA.LinearVelocityInternal.X -= invMassA * P.X;
                    bodyA.LinearVelocityInternal.Y -= invMassA * P.Y;
                    bodyB.AngularVelocityInternal += invIB * (ccp.rB.X * P.Y - ccp.rB.Y * P.X);
                    bodyB.LinearVelocityInternal.X += invMassB * P.X;
                    bodyB.LinearVelocityInternal.Y += invMassB * P.Y;
#endif
                }
            }
        }

        public void SolveVelocityConstraints()
        {
            for (int i = 0; i < _constraintCount; ++i)
            {
                ContactConstraint c = Constraints[i];
                Body bodyA = c.BodyA;
                Body bodyB = c.BodyB;
                float wA = bodyA.AngularVelocityInternal;
                float wB = bodyB.AngularVelocityInternal;
                Vector2 vA = bodyA.LinearVelocityInternal;
                Vector2 vB = bodyB.LinearVelocityInternal;
                float invMassA = bodyA.InvMass;
                float invIA = bodyA.InvI;
                float invMassB = bodyB.InvMass;
                float invIB = bodyB.InvI;
                Vector2 normal = c.Normal;

#if MATH_OVERLOADS
				Vector2 tangent = MathUtils.Cross(normal, 1.0f);
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
                    Vector2 dv = new Vector2(vB.X + (-wB * ccp.rB.Y) - vA.X - (-wA * ccp.rA.Y),
                                             vB.Y + (wB * ccp.rB.X) - vA.Y - (wA * ccp.rA.X));

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
                    wA -= invIA * (ccp.rA.X * P.Y - ccp.rA.Y * P.X);

                    vB.X += invMassB * P.X;
                    vB.Y += invMassB * P.Y;
                    wB += invIB * (ccp.rB.X * P.Y - ccp.rB.Y * P.X);
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
			        float lambda = -ccp.NormalMass * (vn - ccp.VelocityBias);

			        // MathUtils.Clamp the accumulated impulse
			        float newImpulse = Math.Max(ccp.NormalImpulse + lambda, 0.0f);
			        lambda = newImpulse - ccp.NormalImpulse;

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
                    float lambda = -ccp.NormalMass * (vn - ccp.VelocityBias);

                    // Clamp the accumulated impulse
                    float newImpulse = Math.Max(ccp.NormalImpulse + lambda, 0.0f);
                    lambda = newImpulse - ccp.NormalImpulse;

                    // Apply contact impulse
                    var P = new Vector2(lambda * normal.X, lambda * normal.Y);

                    vA.X -= invMassA * P.X;
                    vA.Y -= invMassA * P.Y;
                    wA -= invIA * (ccp.rA.X * P.Y - ccp.rA.Y * P.X);

                    vB.X += invMassB * P.X;
                    vB.Y += invMassB * P.Y;
                    wB += invIB * (ccp.rB.X * P.Y - ccp.rB.Y * P.X);
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

                    Vector2 b = new Vector2(vn1 - cp1.VelocityBias, vn2 - cp2.VelocityBias);
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

                    Vector2 b = new Vector2(vn1 - cp1.VelocityBias, vn2 - cp2.VelocityBias);
                    b -= MathUtils.Multiply(ref c.K, ref a); // Inlining didn't help for the multiply.
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
                        Vector2 x = -MathUtils.Multiply(ref c.NormalMass, ref b);

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
                            wA -= invIA * ((cp1.rA.X * P1.Y - cp1.rA.Y * P1.X) + (cp2.rA.X * P2.Y - cp2.rA.Y * P2.X));

                            vB.X += invMassB * P12.X;
                            vB.Y += invMassB * P12.Y;
                            wB += invIB * ((cp1.rB.X * P1.Y - cp1.rB.Y * P1.X) + (cp2.rB.X * P2.Y - cp2.rB.Y * P2.X));
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

                bodyA.LinearVelocityInternal = vA;
                bodyA.AngularVelocityInternal = wA;
                bodyB.LinearVelocityInternal = vB;
                bodyB.AngularVelocityInternal = wB;
            }
        }

        public void StoreImpulses()
        {
            for (int i = 0; i < _constraintCount; ++i)
            {
                ContactConstraint c = Constraints[i];
                Manifold m = c.Manifold;

                for (int j = 0; j < c.PointCount; ++j)
                {
                    var pj = m.Points[j];
                    var cp = c.Points[j];

                    pj.NormalImpulse = cp.NormalImpulse;
                    pj.TangentImpulse = cp.TangentImpulse;

                    m.Points[j] = pj;
                }

                c.Manifold = m;
                _contacts[i].Manifold = m;
            }
        }

        public bool SolvePositionConstraints(float baumgarte)
        {
            float minSeparation = 0.0f;

            for (int i = 0; i < _constraintCount; ++i)
            {
                ContactConstraint c = Constraints[i];

                Body bodyA = c.BodyA;
                Body bodyB = c.BodyB;

                float invMassA = bodyA.Mass * bodyA.InvMass;
                float invIA = bodyA.Mass * bodyA.InvI;
                float invMassB = bodyB.Mass * bodyB.InvMass;
                float invIB = bodyB.Mass * bodyB.InvI;

                // Solve normal constraints
                for (int j = 0; j < c.PointCount; ++j)
                {
                    PositionSolverManifold psm = new PositionSolverManifold(ref c, j);
                    Vector2 normal = psm.Normal;

                    Vector2 point = psm.Point;
                    float separation = psm.Separation;

                    Vector2 rA = point - bodyA.Sweep.c;
                    Vector2 rB = point - bodyB.Sweep.c;

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

			        bodyA.Sweep.c -= invMassA * P;
                    bodyA.Sweep.a -= invIA * MathUtils.Cross(rA, P);

                    bodyB.Sweep.c += invMassB * P;
                    bodyB.Sweep.a += invIB * MathUtils.Cross(rB, P);
#else
                    Vector2 P = new Vector2(impulse * normal.X, impulse * normal.Y);

                    bodyA.Sweep.c.X -= invMassA * P.X;
                    bodyA.Sweep.c.Y -= invMassA * P.Y;
                    bodyA.Sweep.a -= invIA * (rA.X * P.Y - rA.Y * P.X);

                    bodyB.Sweep.c.X += invMassB * P.X;
                    bodyB.Sweep.c.Y += invMassB * P.Y;
                    bodyB.Sweep.a += invIB * (rB.X * P.Y - rB.Y * P.X);
#endif
                    bodyA.SynchronizeTransform();
                    bodyB.SynchronizeTransform();
                }
            }

            // We can't expect minSpeparation >= -Settings.b2_linearSlop because we don't
            // push the separation above -Settings.b2_linearSlop.
            return minSeparation >= -1.5f * Settings.LinearSlop;
        }

        // Sequential position solver for position constraints.
        public bool SolvePositionConstraintsTOI(float baumgarte, Body toiBodyA, Body toiBodyB)
        {
            float minSeparation = 0.0f;

            for (int i = 0; i < _constraintCount; ++i)
            {
                ContactConstraint c = Constraints[i];
                Body bodyA = c.BodyA;
                Body bodyB = c.BodyB;

                float massA = 0.0f;
                if (bodyA == toiBodyA || bodyA == toiBodyB)
                {
                    massA = bodyA.Mass;
                }

                float massB = 0.0f;
                if (bodyB == toiBodyA || bodyB == toiBodyB)
                {
                    massB = bodyB.Mass;
                }

                float invMassA = bodyA.Mass * bodyA.InvMass;
                float invIA = bodyA.Mass * bodyA.InvI;
                float invMassB = bodyB.Mass * bodyB.InvMass;
                float invIB = bodyB.Mass * bodyB.InvI;

                // Solve normal constraints
                for (int j = 0; j < c.PointCount; ++j)
                {
                    PositionSolverManifold psm = new PositionSolverManifold(ref c, j);
                    Vector2 normal = psm.Normal;

                    Vector2 point = psm.Point;
                    float separation = psm.Separation;

                    Vector2 rA = point - bodyA.Sweep.c;
                    Vector2 rB = point - bodyB.Sweep.c;

                    // Track max constraint error.
                    minSeparation = Math.Min(minSeparation, separation);

                    // Prevent large corrections and allow slop.
                    float C = MathUtils.Clamp(baumgarte * (separation + Settings.LinearSlop), -Settings.MaxLinearCorrection, 0.0f);

                    // Compute the effective mass.
                    float rnA = MathUtils.Cross(rA, normal);
                    float rnB = MathUtils.Cross(rB, normal);
                    float K = invMassA + invMassB + invIA * rnA * rnA + invIB * rnB * rnB;

                    // Compute normal impulse
                    float impulse = K > 0.0f ? -C / K : 0.0f;

                    Vector2 P = impulse * normal;

                    bodyA.Sweep.c -= invMassA * P;
                    bodyA.Sweep.a -= invIA * MathUtils.Cross(rA, P);
                    bodyA.SynchronizeTransform();

                    bodyB.Sweep.c += invMassB * P;
                    bodyB.Sweep.a += invIB * MathUtils.Cross(rB, P);
                    bodyB.SynchronizeTransform();
                }
            }

            // We can't expect minSpeparation >= -b2_linearSlop because we don't
            // push the separation above -b2_linearSlop.
            return minSeparation >= -1.5f * Settings.LinearSlop;
        }
    }

    internal struct PositionSolverManifold
    {
        internal Vector2 Normal;
        internal Vector2 Point;
        internal float Separation;

        internal PositionSolverManifold(ref ContactConstraint cc, int index)
        {
            Debug.Assert(cc.PointCount > 0);

            switch (cc.Type)
            {
                case ManifoldType.Circles:
                    {
                        Vector2 pointA = cc.BodyA.GetWorldPoint(ref cc.LocalPoint);
                        Vector2 pointB = cc.BodyB.GetWorldPoint(ref cc.Points[0].LocalPoint);
                        if (Vector2.DistanceSquared(pointA, pointB) > Settings.Epsilon * Settings.Epsilon)
                        {
                            Normal = pointB - pointA;
                            Normal.Normalize();
                        }
                        else
                        {
                            Normal = new Vector2(1.0f, 0.0f);
                        }

                        Point = 0.5f * (pointA + pointB);
                        Separation = Vector2.Dot(pointB - pointA, Normal) - cc.RadiusA - cc.RadiusB;
                    }
                    break;

                case ManifoldType.FaceA:
                    {
                        Normal = cc.BodyA.GetWorldVector(ref cc.LocalNormal);
                        Vector2 planePoint = cc.BodyA.GetWorldPoint(ref cc.LocalPoint);

                        Vector2 clipPoint = cc.BodyB.GetWorldPoint(ref cc.Points[index].LocalPoint);
                        Separation = Vector2.Dot(clipPoint - planePoint, Normal) - cc.RadiusA - cc.RadiusB;
                        Point = clipPoint;
                    }
                    break;

                case ManifoldType.FaceB:
                    {
                        Normal = cc.BodyB.GetWorldVector(ref cc.LocalNormal);
                        Vector2 planePoint = cc.BodyB.GetWorldPoint(ref cc.LocalPoint);

                        Vector2 clipPoint = cc.BodyA.GetWorldPoint(ref cc.Points[index].LocalPoint);
                        Separation = Vector2.Dot(clipPoint - planePoint, Normal) - cc.RadiusA - cc.RadiusB;
                        Point = clipPoint;

                        // Ensure normal points from A to B
                        Normal = -Normal;
                    }
                    break;
                default:
                    Normal = Vector2.Zero;
                    Point = Vector2.Zero;
                    Separation = 0.0f;
                    break;
            }
        }
    }
}
