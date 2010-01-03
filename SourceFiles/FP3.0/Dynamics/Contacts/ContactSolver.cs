//#define DEBUG_SOLVER

using Microsoft.Xna.Framework;
using System.Diagnostics;
using System;

namespace FarseerPhysics
{
    public struct ContactConstraintPoint
    {
        public Vector2 LocalPoint;
        public Vector2 RA;
        public Vector2 RB;
        public float NormalImpulse;
        public float TangentImpulse;
        public float NormalMass;
        public float TangentMass;
        public float EqualizedMass;
        public float VelocityBias;
    }

    public class ContactConstraint
    {
        public FixedArray2<ContactConstraintPoint> Points;
        public Vector2 LocalPlaneNormal;
        public Vector2 LocalPoint;
        public Vector2 Normal;
        public Mat22 NormalMass;
        public Mat22 K;
        public Body BodyA;
        public Body BodyB;
        public ManifoldType ManifoldType;
        public float Radius;
        public float Friction;
        public float Restitution;
        public int PointCount;
        public Manifold Manifold;
    }

    public class ContactSolver
    {
        public ContactSolver()
        {
            Constraints = new ContactConstraint[50];

            for (int i = 0; i < Constraints.Length; i++)
            {
                Constraints[i] = new ContactConstraint();
            }
        }

        public void Reset(Contact[] contacts, int contactCount)
        {
            _contacts = contacts;
            _constraintCount = contactCount;

            // grow the array
            if (Constraints.Length <= _constraintCount)
            {
                Array.Resize(ref Constraints, _constraintCount * 2);

                for (int i = 0; i < Constraints.Length; i++)
                {
                    Constraints[i] = new ContactConstraint();
                }
            }

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

                float friction = Settings.MixFriction(fixtureA.Friction, fixtureB.Friction);
                float restitution = Settings.MixRestitution(fixtureA.Restitution, fixtureB.Restitution);

                Vector2 vA = bodyA._linearVelocity;
                Vector2 vB = bodyB._linearVelocity;
                float wA = bodyA._angularVelocity;
                float wB = bodyB._angularVelocity;

                Debug.Assert(manifold._pointCount > 0);

                WorldManifold worldManifold = new WorldManifold(ref manifold, ref bodyA._xf, radiusA, ref bodyB._xf, radiusB);

                ContactConstraint cc = Constraints[i];
                cc.BodyA = bodyA;
                cc.BodyB = bodyB;
                cc.Manifold = manifold;
                cc.Normal = worldManifold.Normal;
                cc.PointCount = manifold._pointCount;
                cc.Friction = friction;
                cc.Restitution = restitution;

                cc.LocalPlaneNormal = manifold._localPlaneNormal;
                cc.LocalPoint = manifold._localPoint;
                cc.Radius = radiusA + radiusB;
                cc.ManifoldType = manifold._type;

                for (int j = 0; j < cc.PointCount; ++j)
                {
                    ManifoldPoint cp = manifold._points[j];
                    ContactConstraintPoint ccp = cc.Points[j];

                    ccp.NormalImpulse = cp.NormalImpulse;
                    ccp.TangentImpulse = cp.TangentImpulse;

                    ccp.LocalPoint = cp.LocalPoint;

                    ccp.RA = worldManifold.Points[j] - bodyA._sweep.Center;
                    ccp.RB = worldManifold.Points[j] - bodyB._sweep.Center;

                    float rnA = ccp.RA.X * cc.Normal.Y - ccp.RA.Y * cc.Normal.X;
                    float rnB = ccp.RB.X * cc.Normal.Y - ccp.RB.Y * cc.Normal.X;

                    rnA *= rnA;
                    rnB *= rnB;

                    float kNormal = bodyA._invMass + bodyB._invMass + bodyA._invI * rnA + bodyB._invI * rnB;

                    Debug.Assert(kNormal > Settings.Epsilon);
                    ccp.NormalMass = 1.0f / kNormal;

                    float kEqualized = bodyA._mass * bodyA._invMass + bodyB._mass * bodyB._invMass;
                    kEqualized += bodyA._mass * bodyA._invI * rnA + bodyB._mass * bodyB._invI * rnB;

                    Debug.Assert(kEqualized > Settings.Epsilon);
                    ccp.EqualizedMass = 1.0f / kEqualized;

                    Vector2 tangent = new Vector2(cc.Normal.Y, -cc.Normal.X);

                    float rtA = ccp.RA.X * tangent.Y - ccp.RA.Y * tangent.X;
                    float rtB = ccp.RB.X * tangent.Y - ccp.RB.Y * tangent.X;

                    rtA *= rtA;
                    rtB *= rtB;
                    float kTangent = bodyA._invMass + bodyB._invMass + bodyA._invI * rtA + bodyB._invI * rtB;

                    Debug.Assert(kTangent > Settings.Epsilon);
                    ccp.TangentMass = 1.0f / kTangent;

                    // Setup a velocity bias for restitution.
                    ccp.VelocityBias = 0.0f;
                    float vRel = Vector2.Dot(cc.Normal, vB + MathUtils.Cross(wB, ccp.RB) - vA - MathUtils.Cross(wA, ccp.RA));
                    if (vRel < -Settings.VelocityThreshold)
                    {
                        ccp.VelocityBias = -cc.Restitution * vRel;
                    }

                    cc.Points[j] = ccp;
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

        public void InitVelocityConstraints(ref TimeStep step)
        {
            // Warm start.
            for (int i = 0; i < _constraintCount; ++i)
            {
                ContactConstraint c = Constraints[i];

                Body bodyA = c.BodyA;
                Body bodyB = c.BodyB;
                float invMassA = bodyA._invMass;
                float invIA = bodyA._invI;
                float invMassB = bodyB._invMass;
                float invIB = bodyB._invI;
                Vector2 normal = c.Normal;

                Vector2 tangent = new Vector2(normal.Y, -normal.X);

                if (step.WarmStarting)
                {
                    for (int j = 0; j < c.PointCount; ++j)
                    {
                        ContactConstraintPoint ccp = c.Points[j];
                        ccp.NormalImpulse *= step.DtRatio;
                        ccp.TangentImpulse *= step.DtRatio;

                        Vector2 P = new Vector2(ccp.NormalImpulse * normal.X + ccp.TangentImpulse * tangent.X,
                                                ccp.NormalImpulse * normal.Y + ccp.TangentImpulse * tangent.Y);
                        bodyA._angularVelocity -= invIA * (ccp.RA.X * P.Y - ccp.RA.Y * P.X);
                        bodyA._linearVelocity.X -= invMassA * P.X;
                        bodyA._linearVelocity.Y -= invMassA * P.Y;
                        bodyB._angularVelocity += invIB * (ccp.RB.X * P.Y - ccp.RB.Y * P.X);
                        bodyB._linearVelocity.X += invMassB * P.X;
                        bodyB._linearVelocity.Y += invMassB * P.Y;

                        c.Points[j] = ccp;
                    }
                }
                else
                {
                    for (int j = 0; j < c.PointCount; ++j)
                    {
                        ContactConstraintPoint ccp = c.Points[j];
                        ccp.NormalImpulse = 0.0f;
                        ccp.TangentImpulse = 0.0f;
                        c.Points[j] = ccp;
                    }
                }

            }
        }

        public void SolveVelocityConstraints()
        {
            for (int i = 0; i < _constraintCount; ++i)
            {
                ContactConstraint cc = Constraints[i];
                Body bodyA = cc.BodyA;
                Body bodyB = cc.BodyB;
                float wA = bodyA._angularVelocity;
                float wB = bodyB._angularVelocity;
                Vector2 vA = bodyA._linearVelocity;
                Vector2 vB = bodyB._linearVelocity;
                float invMassA = bodyA._invMass;
                float invIA = bodyA._invI;
                float invMassB = bodyB._invMass;
                float invIB = bodyB._invI;
                Vector2 normal = cc.Normal;

                Vector2 tangent = new Vector2(normal.Y, -normal.X);
                float friction = cc.Friction;

                Debug.Assert(cc.PointCount == 1 || cc.PointCount == 2);

                // Solve tangent constraints
                for (int j = 0; j < cc.PointCount; ++j)
                {
                    ContactConstraintPoint ccp = cc.Points[j];

                    // Relative velocity at contact

                    //INLINED into vt
                    //Vector2 dv = new Vector2(vB.X + (-wB * ccp.RB.Y) - vA.X - (-wA * ccp.RA.Y),
                    //                         vB.Y + (wB * ccp.RB.X) - vA.Y - (wA * ccp.RA.X));

                    // Compute tangent force
                    float vt = (vB.X + (-wB * ccp.RB.Y) - vA.X - (-wA * ccp.RA.Y)) * tangent.X + (vB.Y + (wB * ccp.RB.X) - vA.Y - (wA * ccp.RA.X)) * tangent.Y;
                    float lambda = ccp.TangentMass * (-vt);

                    // MathUtils.Clamp the accumulated force
                    float maxFriction = friction * ccp.NormalImpulse;
                    float newImpulse = MathUtils.Clamp(ccp.TangentImpulse + lambda, -maxFriction, maxFriction);
                    lambda = newImpulse - ccp.TangentImpulse;

                    // Apply contact impulse

                    //INLINED into vA and vB
                    //Vector2 P = new Vector2(lambda * tangent.X, lambda * tangent.Y);

                    vA.X -= invMassA * (lambda * tangent.X);
                    vA.Y -= invMassA * (lambda * tangent.Y);
                    wA -= invIA * (ccp.RA.X * (lambda * tangent.Y) - ccp.RA.Y * (lambda * tangent.X));

                    vB.X += invMassB * (lambda * tangent.X);
                    vB.Y += invMassB * (lambda * tangent.Y);
                    wB += invIB * (ccp.RB.X * (lambda * tangent.Y) - ccp.RB.Y * (lambda * tangent.X));

                    ccp.TangentImpulse = newImpulse;
                    cc.Points[j] = ccp;
                }

                // Solve normal constraints
                if (cc.PointCount == 1)
                {
                    ContactConstraintPoint ccp = cc.Points[0];

                    // Relative velocity at contact

                    //INLINED into vn
                    //Vector2 dv = new Vector2(vB.X + (-wB * ccp.RB.Y) - vA.X - (-wA * ccp.RA.Y),
                    //                         vB.Y + (wB * ccp.RB.X) - vA.Y - (wA * ccp.RA.X));

                    // Compute normal impulse
                    float vn = (vB.X + (-wB * ccp.RB.Y) - vA.X - (-wA * ccp.RA.Y)) * normal.X + (vB.Y + (wB * ccp.RB.X) - vA.Y - (wA * ccp.RA.X)) * normal.Y;
                    float lambda = -ccp.NormalMass * (vn - ccp.VelocityBias);

                    // Clamp the accumulated impulse
                    float newImpulse = ccp.NormalImpulse + lambda > 0.0f ? ccp.NormalImpulse + lambda : 0.0f;

                    lambda = newImpulse - ccp.NormalImpulse;

                    // Apply contact impulse

                    //INLINED into vA and vB
                    //var P = new Vector2(lambda * normal.X, lambda * normal.Y);

                    vA.X -= invMassA * (lambda * normal.X);
                    vA.Y -= invMassA * (lambda * normal.Y);
                    wA -= invIA * (ccp.RA.X * (lambda * normal.Y) - ccp.RA.Y * (lambda * normal.X));

                    vB.X += invMassB * (lambda * normal.X);
                    vB.Y += invMassB * (lambda * normal.Y);
                    wB += invIB * (ccp.RB.X * (lambda * normal.Y) - ccp.RB.Y * (lambda * normal.X));

                    ccp.NormalImpulse = newImpulse;
                    cc.Points[0] = ccp;
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

                    ContactConstraintPoint cp1 = cc.Points[0];
                    ContactConstraintPoint cp2 = cc.Points[1];

                    Debug.Assert((cp1.NormalImpulse) >= 0.0f && (cp2.NormalImpulse) >= 0.0f);

                    // Relative velocity at contact

                    //INLINED into vn1 and vn2
                    //Vector2 dv1 = new Vector2(vB.X + (-wB * cp1.RB.Y) - vA.X - (-wA * cp1.RA.Y),
                    //                          vB.Y + (wB * cp1.RB.X) - vA.Y - (wA * cp1.RA.X));
                    //Vector2 dv2 = new Vector2(vB.X + (-wB * cp2.RB.Y) - vA.X - (-wA * cp2.RA.Y),
                    //                          vB.Y + (wB * cp2.RB.X) - vA.Y - (wA * cp2.RA.X));

#if DEBUG_SOLVER
                    Vector2 dv1 = new Vector2(vB.X + (-wB * cp1.RB.Y) - vA.X - (-wA * cp1.RA.Y),
                                              vB.Y + (wB * cp1.RB.X) - vA.Y - (wA * cp1.RA.X));
                    Vector2 dv2 = new Vector2(vB.X + (-wB * cp2.RB.Y) - vA.X - (-wA * cp2.RA.Y),
                                              vB.Y + (wB * cp2.RB.X) - vA.Y - (wA * cp2.RA.X));
#endif

                    // Compute normal velocity
                    float vn1 = (vB.X + (-wB * cp1.RB.Y) - vA.X - (-wA * cp1.RA.Y)) * normal.X + (vB.Y + (wB * cp1.RB.X) - vA.Y - (wA * cp1.RA.X)) * normal.Y;
                    float vn2 = (vB.X + (-wB * cp2.RB.Y) - vA.X - (-wA * cp2.RA.Y)) * normal.X + (vB.Y + (wB * cp2.RB.X) - vA.Y - (wA * cp2.RA.X)) * normal.Y;

                    Vector2 b = new Vector2(vn1 - cp1.VelocityBias, vn2 - cp2.VelocityBias);
                    b.X -= cc.K.Col1.X * (cp1.NormalImpulse) + cc.K.Col2.X * (cp2.NormalImpulse);
                    b.Y -= cc.K.Col1.Y * (cp1.NormalImpulse) + cc.K.Col2.Y * (cp2.NormalImpulse);

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
                        Vector2 x = -new Vector2(cc.NormalMass.Col1.X * b.X + cc.NormalMass.Col2.X * b.Y, cc.NormalMass.Col1.Y * b.X + cc.NormalMass.Col2.Y * b.Y);

                        if (x.X >= 0.0f && x.Y >= 0.0f)
                        {
                            // Resubstitute for the incremental impulse
                            Vector2 d = new Vector2(x.X - (cp1.NormalImpulse), x.Y - (cp2.NormalImpulse));

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
                            // Accumulate
                            cp1.NormalImpulse = x.X;
                            cp2.NormalImpulse = x.Y;

#if DEBUG_SOLVER

                            const float k_errorTol = 1e-3f;

                            // Postconditions
                            dv1 = vB + MathUtils.Cross(wB, cp1.RB) - vA - MathUtils.Cross(wA, cp1.RA);
                            dv2 = vB + MathUtils.Cross(wB, cp2.RB) - vA - MathUtils.Cross(wA, cp2.RA);

                            // Compute normal velocity
                            {
                                vn1 = Vector2.Dot(dv1, normal);
                                vn2 = Vector2.Dot(dv2, normal);

                                Debug.Assert(MathUtils.Abs(vn1 - cp1.VelocityBias) < k_errorTol);
                                Debug.Assert(MathUtils.Abs(vn2 - cp2.VelocityBias) < k_errorTol);
                            }

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
                        vn2 = cc.K.Col1.Y * x.X + b.Y;

                        if (x.X >= 0.0f && vn2 >= 0.0f)
                        {
                            // Resubstitute for the incremental impulse
                            Vector2 d = new Vector2(x.X - (cp1.NormalImpulse), x.Y - (cp2.NormalImpulse));

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
                            // Accumulate
                            cp1.NormalImpulse = x.X;
                            cp2.NormalImpulse = x.Y;

#if DEBUG_SOLVER
                            const float k_errorTol = 1e-3f;

                            // Postconditions
                            dv1 = vB + MathUtils.Cross(wB, cp1.RB) - vA - MathUtils.Cross(wA, cp1.RA);

                            // Compute normal velocity
                            vn1 = Vector2.Dot(dv1, normal);

                            Debug.Assert(MathUtils.Abs(vn1 - cp1.VelocityBias) < k_errorTol);
#endif
                            break;
                        }


                        //
                        // Case 3: wB = 0 and x1 = 0
                        //
                        // vn1 = a11 * 0 + a12 * x2' + b1' 
                        //   0 = a21 * 0 + a22 * x2' + b2'
                        //
                        x.X = 0.0f;
                        x.Y = -cp2.NormalMass * b.Y;
                        vn1 = cc.K.Col2.X * x.Y + b.X;

                        if (x.Y >= 0.0f && vn1 >= 0.0f)
                        {

                            // Resubstitute for the incremental impulse
                            Vector2 d = new Vector2(x.X - (cp1.NormalImpulse), x.Y - (cp2.NormalImpulse));

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
                            // Accumulate
                            cp1.NormalImpulse = x.X;
                            cp2.NormalImpulse = x.Y;

#if DEBUG_SOLVER
                            const float k_errorTol = 1e-3f;

                            // Postconditions
                            dv2 = vB + MathUtils.Cross(wB, cp2.RB) - vA - MathUtils.Cross(wA, cp2.RA);

                            // Compute normal velocity
                            vn2 = Vector2.Dot(dv2, normal);

                            Debug.Assert(MathUtils.Abs(vn2 - cp2.VelocityBias) < k_errorTol);
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
                            // Resubstitute for the incremental impulse
                            Vector2 d = new Vector2(x.X - (cp1.NormalImpulse), x.Y - (cp2.NormalImpulse));

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

                            // Accumulate
                            cp1.NormalImpulse = x.X;
                            cp2.NormalImpulse = x.Y;

                            break;
                        }

                        // No solution, give up. This is hit sometimes, but it doesn't seem to matter.
                        break;
                    }

                    cc.Points[0] = cp1;
                    cc.Points[1] = cp2;
                }

                bodyA._linearVelocity = vA;
                bodyA._angularVelocity = wA;
                bodyB._linearVelocity = vB;
                bodyB._angularVelocity = wB;
            }
        }

        public void FinalizeVelocityConstraints()
        {
            for (int i = 0; i < _constraintCount; ++i)
            {
                ContactConstraint c = Constraints[i];
                Manifold m = c.Manifold;

                for (int j = 0; j < c.PointCount; ++j)
                {
                    var pj = m._points[j];
                    var cp = c.Points[j];

                    pj.NormalImpulse = cp.NormalImpulse;
                    pj.TangentImpulse = cp.TangentImpulse;

                    m._points[j] = pj;
                }

                // TODO: look for better ways of doing this.
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

                float invMassA = bodyA._mass * bodyA._invMass;
                float invIA = bodyA._mass * bodyA._invI;
                float invMassB = bodyB._mass * bodyB._invMass;
                float invIB = bodyB._mass * bodyB._invI;

                PositionSolverManifold psm = new PositionSolverManifold(ref c);
                Vector2 normal = psm.Normal;

                // Solve normal constraints
                for (int j = 0; j < c.PointCount; ++j)
                {
                    ContactConstraintPoint ccp = c.Points[j];

                    Vector2 point = psm.Points[j];
                    float separation = psm.Separations[j];

                    Vector2 rA = point - bodyA._sweep.Center;
                    Vector2 rB = point - bodyB._sweep.Center;

                    // Track max constraint error.
                    minSeparation = Math.Min(minSeparation, separation);

                    // Prevent large corrections and allow slop.
                    float C = MathUtils.Clamp(baumgarte * (separation + Settings.LinearSlop), -Settings.MaxLinearCorrection, 0.0f);

                    // Compute normal impulse
                    float impulse = -ccp.EqualizedMass * C;

                    Vector2 P = new Vector2(impulse * normal.X, impulse * normal.Y);

                    bodyA._sweep.Center.X -= invMassA * P.X;
                    bodyA._sweep.Center.Y -= invMassA * P.Y;
                    bodyA._sweep.Angle -= invIA * (rA.X * P.Y - rA.Y * P.X);

                    bodyB._sweep.Center.X += invMassB * P.X;
                    bodyB._sweep.Center.Y += invMassB * P.Y;
                    bodyB._sweep.Angle += invIB * (rB.X * P.Y - rB.Y * P.X);

                    bodyA.SynchronizeTransform();
                    bodyB.SynchronizeTransform();
                }
            }

            // We can't expect minSpeparation >= -Settings.LinearSlop because we don't
            // push the separation above -Settings.LinearSlop.
            return minSeparation >= -1.5f * Settings.LinearSlop;
        }

        public ContactConstraint[] Constraints;
        private int _constraintCount; // collection can be bigger.
        private Contact[] _contacts;
    }

    internal struct PositionSolverManifold
    {
        internal PositionSolverManifold(ref ContactConstraint cc)
        {
            Points = new FixedArray2<Vector2>();
            Separations = new FixedArray2<float>();
            Normal = new Vector2();

            Debug.Assert(cc.PointCount > 0);

            switch (cc.ManifoldType)
            {
                case ManifoldType.Circles:
                    {
                        Vector2 pointA = cc.BodyA.GetWorldPoint(cc.LocalPoint);
                        Vector2 pointB = cc.BodyB.GetWorldPoint(cc.Points[0].LocalPoint);
                        if (Vector2.DistanceSquared(pointA, pointB) > Settings.Epsilon * Settings.Epsilon)
                        {
                            Normal = pointB - pointA;
                            Normal.Normalize();
                        }
                        else
                        {
                            Normal = new Vector2(1.0f, 0.0f);
                        }

                        Points[0] = 0.5f * (pointA + pointB);
                        Separations[0] = Vector2.Dot(pointB - pointA, Normal) - cc.Radius;
                    }
                    break;

                case ManifoldType.FaceA:
                    {
                        Normal = cc.BodyA.GetWorldVector(cc.LocalPlaneNormal);
                        Vector2 planePoint = cc.BodyA.GetWorldPoint(cc.LocalPoint);

                        for (int i = 0; i < cc.PointCount; ++i)
                        {
                            Vector2 clipPoint = cc.BodyB.GetWorldPoint(cc.Points[i].LocalPoint);
                            Separations[i] = Vector2.Dot(clipPoint - planePoint, Normal) - cc.Radius;
                            Points[i] = clipPoint;
                        }
                    }
                    break;

                case ManifoldType.FaceB:
                    {
                        Normal = cc.BodyB.GetWorldVector(cc.LocalPlaneNormal);
                        Vector2 planePoint = cc.BodyB.GetWorldPoint(cc.LocalPoint);

                        for (int i = 0; i < cc.PointCount; ++i)
                        {
                            Vector2 clipPoint = cc.BodyA.GetWorldPoint(cc.Points[i].LocalPoint);
                            Separations[i] = Vector2.Dot(clipPoint - planePoint, Normal) - cc.Radius;
                            Points[i] = clipPoint;
                        }

                        // Ensure normal points from A to B
                        Normal = -Normal;
                    }
                    break;
            }
        }

        internal Vector2 Normal;
        internal FixedArray2<Vector2> Points;
        internal FixedArray2<float> Separations;
    }
}