/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2007 Erin Catto http://www.gphysics.com

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

//#define B2_DEBUG_SOLVER
#define FIRST

using System;
using System.Collections.Generic;
using System.Text;

using Box2DX.Collision;
using Box2DX.Common;

namespace Box2DX.Dynamics
{
#warning "CAS"
	public class ContactConstraintPoint
	{
		public Vec2 LocalAnchor1;
		public Vec2 LocalAnchor2;
		public Vec2 R1;
		public Vec2 R2;
		public float NormalImpulse;
		public float TangentImpulse;
		public float NormalMass;
		public float TangentMass;
		public float EqualizedMass;
		public float Separation;
		public float VelocityBias;
	}

#warning "CAS"
	public class ContactConstraint
	{
		public ContactConstraintPoint[] Points = new ContactConstraintPoint[Settings.MaxManifoldPoints];
		public Vec2 Normal;
		public Mat22 NormalMass;
		public Mat22 K;
		public Manifold Manifold;
		public Body Body1;
		public Body Body2;
		public float Friction;
		public float Restitution;
		public int PointCount;

		public ContactConstraint()
		{
			for (int i = 0; i < Settings.MaxManifoldPoints; i++)
				Points[i] = new ContactConstraintPoint();
		}
	}

	public class ContactSolver : IDisposable
	{
		public TimeStep _step;
		public ContactConstraint[] _constraints;
		public int _constraintCount;

		public ContactSolver(TimeStep step, Contact[] contacts, int contactCount)
		{
            _step = step;

            _constraintCount = contactCount;
            _constraints = (ContactConstraint)m_allocator->Allocate(m_constraintCount * sizeof(b2ContactConstraint));

            for (int i = 0; i < _constraintCount; ++i)
            {
                Contact contact = contacts[i];

                Fixture fixtureA = contact._fixtureA;
                Fixture fixtureB = contact._fixtureB;
                Shape shapeA = fixtureA.GetShape();
                Shape shapeB = fixtureB.GetShape();
                float radiusA = shapeA.Radius;
                float radiusB = shapeB.Radius;
                Body bodyA = fixtureA.GetBody();
                Body bodyB = fixtureB.GetBody();
                Manifold manifold = contact.GetManifold();

                float friction = Settings.MixFriction(fixtureA.GetFriction(), fixtureB.GetFriction());
                float restitution = Settings.MixRestitution(fixtureA.GetRestitution(), fixtureB.GetRestitution());

                Vec2 vA = bodyA._linearVelocity;
                Vec2 vB = bodyB._linearVelocity;
                float wA = bodyA._angularVelocity;
                float wB = bodyB._angularVelocity;

                Box2DXDebug.Assert(manifold.m_pointCount > 0);

                WorldManifold worldManifold;
                worldManifold.Initialize(manifold, bodyA._xf, radiusA, bodyB._xf, radiusB);

                ContactConstraint cc = _constraints[i];
                cc.Body1 = bodyA;
                cc.Body2 = bodyB;
                cc.Manifold = manifold;
                cc.Normal = worldManifold.m_normal;
                cc.PointCount = manifold.m_pointCount;
                cc.Friction = friction;
                cc.Restitution = restitution;

                cc.localPlaneNormal = manifold.m_localPlaneNormal;
                cc.LocalPoint = manifold.m_localPoint;
                cc.Radius = radiusA + radiusB;
                cc.Type = manifold.m_type;

                for (int j = 0; j < cc.PointCount; ++j)
                {
                    ManifoldPoint cp = manifold.m_points + j;
                    ContactConstraintPoint ccp = cc.points + j;

                    ccp.NormalImpulse = cp.m_normalImpulse;
                    ccp.TangentImpulse = cp.m_tangentImpulse;

                    ccp.localPoint = cp.m_localPoint;

                    ccp.R1 = worldManifold.m_points[j] - bodyA._sweep.C;
                    ccp.R2 = worldManifold.m_points[j] - bodyB._sweep.C;

                    float rnA = Vec2.Cross(ccp.R1, cc.Normal);
                    float rnB = Vec2.Cross(ccp.R2, cc.Normal);
                    rnA *= rnA;
                    rnB *= rnB;

                    float kNormal = bodyA._invMass + bodyB._invMass + bodyA._invI * rnA + bodyB._invI * rnB;

                    Box2DXDebug.Assert(kNormal > Settings.FLT_EPSILON);
                    ccp.NormalMass = 1.0f / kNormal;

                    float kEqualized = bodyA._mass * bodyA._invMass + bodyB._mass * bodyB._invMass;
                    kEqualized += bodyA._mass * bodyA._invI * rnA + bodyB._mass * bodyB._invI * rnB;

                    Box2DXDebug.Assert(kEqualized > Settings.FLT_EPSILON);
                    ccp.EqualizedMass = 1.0f / kEqualized;

                    Vec2 tangent = Vec2.Cross(cc.Normal, 1.0f);

                    float rtA = Vec2.Cross(ccp.R1, tangent);
                    float rtB = Vec2.Cross(ccp.R2, tangent);
                    rtA *= rtA;
                    rtB *= rtB;

                    float kTangent = bodyA._invMass + bodyB._invMass + bodyA._invI * rtA + bodyB._invI * rtB;

                    Box2DXDebug.Assert(kTangent > Settings.FLT_EPSILON);
                    ccp->tangentMass = 1.0f / kTangent;

                    // Setup a velocity bias for restitution.
                    ccp->velocityBias = 0.0f;
                    float32 vRel = b2Dot(cc->normal, vB + b2Cross(wB, ccp->rB) - vA - b2Cross(wA, ccp->rA));
                    if (vRel < -b2_velocityThreshold)
                    {
                        ccp->velocityBias = -cc->restitution * vRel;
                    }
                }

                // If we have two points, then prepare the block solver.
                if (cc->pointCount == 2)
                {
                    b2ContactConstraintPoint* ccp1 = cc->points + 0;
                    b2ContactConstraintPoint* ccp2 = cc->points + 1;

                    float32 invMassA = bodyA->m_invMass;
                    float32 invIA = bodyA->m_invI;
                    float32 invMassB = bodyB->m_invMass;
                    float32 invIB = bodyB->m_invI;

                    float32 rn1A = b2Cross(ccp1->rA, cc->normal);
                    float32 rn1B = b2Cross(ccp1->rB, cc->normal);
                    float32 rn2A = b2Cross(ccp2->rA, cc->normal);
                    float32 rn2B = b2Cross(ccp2->rB, cc->normal);

                    float32 k11 = invMassA + invMassB + invIA * rn1A * rn1A + invIB * rn1B * rn1B;
                    float32 k22 = invMassA + invMassB + invIA * rn2A * rn2A + invIB * rn2B * rn2B;
                    float32 k12 = invMassA + invMassB + invIA * rn1A * rn2A + invIB * rn1B * rn2B;

                    // Ensure a reasonable condition number.
                    const float32 k_maxConditionNumber = 100.0f;
                    if (k11 * k11 < k_maxConditionNumber * (k11 * k22 - k12 * k12))
                    {
                        // K is safe to invert.
                        cc->K.col1.Set(k11, k12);
                        cc->K.col2.Set(k12, k22);
                        cc->normalMass = cc->K.GetInverse();
                    }
                    else
                    {
                        // The constraints are redundant, just use one.
                        // TODO_ERIN use deepest?
                        cc->pointCount = 1;
                    }
                }
            }
		}

		public void Dispose()
		{
			_constraints = null;
		}

		public void InitVelocityConstraints(TimeStep step)
		{
			// Warm start.
			for (int i = 0; i < _constraintCount; ++i)
			{
				ContactConstraint c = _constraints[i];

				Body b1 = c.Body1;
				Body b2 = c.Body2;
				float invMass1 = b1._invMass;
				float invI1 = b1._invI;
				float invMass2 = b2._invMass;
				float invI2 = b2._invI;
				Vec2 normal = c.Normal;
				Vec2 tangent = Vec2.Cross(normal, 1.0f);

				if (step.WarmStarting)
				{
					for (int j = 0; j < c.PointCount; ++j)
					{
						ContactConstraintPoint ccp = c.Points[j];
						ccp.NormalImpulse *= step.DtRatio;
						ccp.TangentImpulse *= step.DtRatio;
						Vec2 P = ccp.NormalImpulse * normal + ccp.TangentImpulse * tangent;
						b1._angularVelocity -= invI1 * Vec2.Cross(ccp.R1, P);
						b1._linearVelocity -= invMass1 * P;
						b2._angularVelocity += invI2 * Vec2.Cross(ccp.R2, P);
						b2._linearVelocity += invMass2 * P;
					}
				}
				else
				{
					for (int j = 0; j < c.PointCount; ++j)
					{
						ContactConstraintPoint ccp = c.Points[j];
						ccp.NormalImpulse = 0.0f;
						ccp.TangentImpulse = 0.0f;
					}
				}
			}
		}

		public void SolveVelocityConstraints()
		{
			for (int i = 0; i < _constraintCount; ++i)
			{
				ContactConstraint c = _constraints[i];
				Body b1 = c.Body1;
				Body b2 = c.Body2;
				float w1 = b1._angularVelocity;
				float w2 = b2._angularVelocity;
				Vec2 v1 = b1._linearVelocity;
				Vec2 v2 = b2._linearVelocity;
				float invMass1 = b1._invMass;
				float invI1 = b1._invI;
				float invMass2 = b2._invMass;
				float invI2 = b2._invI;
				Vec2 normal = c.Normal;
				Vec2 tangent = Vec2.Cross(normal, 1.0f);
				float friction = c.Friction;

				Box2DXDebug.Assert(c.PointCount == 1 || c.PointCount == 2);

				// Solve normal constraints
				if (c.PointCount == 1)
				{
					ContactConstraintPoint ccp = c.Points[0];

					// Relative velocity at contact
					Vec2 dv = v2 + Vec2.Cross(w2, ccp.R2) - v1 - Vec2.Cross(w1, ccp.R1);

					// Compute normal impulse
					float vn = Vec2.Dot(dv, normal);
					float lambda = -ccp.NormalMass * (vn - ccp.VelocityBias);

					// Clamp the accumulated impulse
					float newImpulse = Common.Math.Max(ccp.NormalImpulse + lambda, 0.0f);
					lambda = newImpulse - ccp.NormalImpulse;

					// Apply contact impulse
					Vec2 P = lambda * normal;

					v1 -= invMass1 * P;
					w1 -= invI1 * Vec2.Cross(ccp.R1, P);

					v2 += invMass2 * P;
					w2 += invI2 * Vec2.Cross(ccp.R2, P);

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

					Vec2 a = new Vec2(cp1.NormalImpulse, cp2.NormalImpulse);
					Box2DXDebug.Assert(a.X >= 0.0f && a.Y >= 0.0f);

					// Relative velocity at contact
					Vec2 dv1 = v2 + Vec2.Cross(w2, cp1.R2) - v1 - Vec2.Cross(w1, cp1.R1);
					Vec2 dv2 = v2 + Vec2.Cross(w2, cp2.R2) - v1 - Vec2.Cross(w1, cp2.R1);

					// Compute normal velocity
					float vn1 = Vec2.Dot(dv1, normal);
					float vn2 = Vec2.Dot(dv2, normal);

					Vec2 b;
					b.X = vn1 - cp1.VelocityBias;
					b.Y = vn2 - cp2.VelocityBias;
					b -= Common.Math.Mul(c.K, a);

					const float k_errorTol = 1e-3f;
					for (; ; )
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
						Vec2 x = -Common.Math.Mul(c.NormalMass, b);

						if (x.X >= 0.0f && x.Y >= 0.0f)
						{
							// Resubstitute for the incremental impulse
							Vec2 d = x - a;

							// Apply incremental impulse
							Vec2 P1 = d.X * normal;
							Vec2 P2 = d.Y * normal;
							v1 -= invMass1 * (P1 + P2);
							w1 -= invI1 * (Vec2.Cross(cp1.R1, P1) + Vec2.Cross(cp2.R1, P2));

							v2 += invMass2 * (P1 + P2);
							w2 += invI2 * (Vec2.Cross(cp1.R2, P1) + Vec2.Cross(cp2.R2, P2));

							// Accumulate
							cp1.NormalImpulse = x.X;
							cp2.NormalImpulse = x.Y;

#if B2_DEBUG_SOLVER
							// Postconditions
							dv1 = v2 + Vector2.Cross(w2, cp1.R2) - v1 - Vector2.Cross(w1, cp1.R1);
							dv2 = v2 + Vector2.Cross(w2, cp2.R2) - v1 - Vector2.Cross(w1, cp2.R1);

							// Compute normal velocity
							vn1 = Vector2.Dot(dv1, normal);
							vn2 = Vector2.Dot(dv2, normal);

							Box2DXDebug.Assert(Common.Math.Abs(vn1 - cp1.VelocityBias) < k_errorTol);
							Box2DXDebug.Assert(Common.Math.Abs(vn2 - cp2.VelocityBias) < k_errorTol);
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
							// Resubstitute for the incremental impulse
							Vec2 d = x - a;

							// Apply incremental impulse
							Vec2 P1 = d.X * normal;
							Vec2 P2 = d.Y * normal;
							v1 -= invMass1 * (P1 + P2);
							w1 -= invI1 * (Vec2.Cross(cp1.R1, P1) + Vec2.Cross(cp2.R1, P2));

							v2 += invMass2 * (P1 + P2);
							w2 += invI2 * (Vec2.Cross(cp1.R2, P1) + Vec2.Cross(cp2.R2, P2));

							// Accumulate
							cp1.NormalImpulse = x.X;
							cp2.NormalImpulse = x.Y;

#if B2_DEBUG_SOLVER
							// Postconditions
							dv1 = v2 + Vector2.Cross(w2, cp1.R2) - v1 - Vector2.Cross(w1, cp1.R1);

							// Compute normal velocity
							vn1 = Vector2.Dot(dv1, normal);

							Box2DXDebug.Assert(Common.Math.Abs(vn1 - cp1.VelocityBias) < k_errorTol);
#endif
							break;
						}


						//
						// Case 3: w2 = 0 and x1 = 0
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
							// Resubstitute for the incremental impulse
							Vec2 d = x - a;

							// Apply incremental impulse
							Vec2 P1 = d.X * normal;
							Vec2 P2 = d.Y * normal;
							v1 -= invMass1 * (P1 + P2);
							w1 -= invI1 * (Vec2.Cross(cp1.R1, P1) + Vec2.Cross(cp2.R1, P2));

							v2 += invMass2 * (P1 + P2);
							w2 += invI2 * (Vec2.Cross(cp1.R2, P1) + Vec2.Cross(cp2.R2, P2));

							// Accumulate
							cp1.NormalImpulse = x.X;
							cp2.NormalImpulse = x.Y;

#if B2_DEBUG_SOLVER
							// Postconditions
							dv2 = v2 + Vector2.Cross(w2, cp2.R2) - v1 - Vector2.Cross(w1, cp2.R1);

							// Compute normal velocity
							vn2 = Vector2.Dot(dv2, normal);

							Box2DXDebug.Assert(Common.Math.Abs(vn2 - cp2.VelocityBias) < k_errorTol);
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
							Vec2 d = x - a;

							// Apply incremental impulse
							Vec2 P1 = d.X * normal;
							Vec2 P2 = d.Y * normal;
							v1 -= invMass1 * (P1 + P2);
							w1 -= invI1 * (Vec2.Cross(cp1.R1, P1) + Vec2.Cross(cp2.R1, P2));

							v2 += invMass2 * (P1 + P2);
							w2 += invI2 * (Vec2.Cross(cp1.R2, P1) + Vec2.Cross(cp2.R2, P2));

							// Accumulate
							cp1.NormalImpulse = x.X;
							cp2.NormalImpulse = x.Y;

							break;
						}

						// No solution, give up. This is hit sometimes, but it doesn't seem to matter.
						break;
					}
				}

				// Solve tangent constraints
				for (int j = 0; j < c.PointCount; ++j)
				{
					ContactConstraintPoint ccp = c.Points[j];

					// Relative velocity at contact
					Vec2 dv = v2 + Vec2.Cross(w2, ccp.R2) - v1 - Vec2.Cross(w1, ccp.R1);

					// Compute tangent force
					float vt = Vec2.Dot(dv, tangent);
					float lambda = ccp.TangentMass * (-vt);

					// Clamp the accumulated force
					float maxFriction = friction * ccp.NormalImpulse;
					float newImpulse = Common.Math.Clamp(ccp.TangentImpulse + lambda, -maxFriction, maxFriction);
					lambda = newImpulse - ccp.TangentImpulse;

					// Apply contact impulse
					Vec2 P = lambda * tangent;

					v1 -= invMass1 * P;
					w1 -= invI1 * Vec2.Cross(ccp.R1, P);

					v2 += invMass2 * P;
					w2 += invI2 * Vec2.Cross(ccp.R2, P);

					ccp.TangentImpulse = newImpulse;
				}

				b1._linearVelocity = v1;
				b1._angularVelocity = w1;
				b2._linearVelocity = v2;
				b2._angularVelocity = w2;
			}
		}

		public void FinalizeVelocityConstraints()
		{
			for (int i = 0; i < _constraintCount; ++i)
			{
				ContactConstraint c = _constraints[i];
				Manifold m = c.Manifold;

				for (int j = 0; j < c.PointCount; ++j)
				{
					m.Points[j].NormalImpulse = c.Points[j].NormalImpulse;
					m.Points[j].TangentImpulse = c.Points[j].TangentImpulse;
				}
			}
		}

#if FIRST
		public bool SolvePositionConstraints(float baumgarte)
		{
			float minSeparation = 0.0f;

			for (int i = 0; i < _constraintCount; ++i)
			{
				ContactConstraint c = _constraints[i];
				Body b1 = c.Body1;
				Body b2 = c.Body2;
				float invMass1 = b1._mass * b1._invMass;
				float invI1 = b1._mass * b1._invI;
				float invMass2 = b2._mass * b2._invMass;
				float invI2 = b2._mass * b2._invI;

				Vec2 normal = c.Normal;

				// Solver normal constraints
				for (int j = 0; j < c.PointCount; ++j)
				{
					ContactConstraintPoint ccp = c.Points[j];

					Vec2 r1 = Common.Math.Mul(b1.GetXForm().R, ccp.LocalAnchor1 - b1.GetLocalCenter());
					Vec2 r2 = Common.Math.Mul(b2.GetXForm().R, ccp.LocalAnchor2 - b2.GetLocalCenter());

					Vec2 p1 = b1._sweep.C + r1;
					Vec2 p2 = b2._sweep.C + r2;
					Vec2 dp = p2 - p1;

					// Approximate the current separation.
					float separation = Vec2.Dot(dp, normal) + ccp.Separation;

					// Track max constraint error.
					minSeparation = Common.Math.Min(minSeparation, separation);

					// Prevent large corrections and allow slop.
					float C = baumgarte * Common.Math.Clamp(separation + Settings.LinearSlop, -Settings.MaxLinearCorrection, 0.0f);

					// Compute normal impulse
					float impulse = -ccp.EqualizedMass * C;

					Vec2 P = impulse * normal;

					b1._sweep.C -= invMass1 * P;
					b1._sweep.A -= invI1 * Vec2.Cross(r1, P);
					b1.SynchronizeTransform();

					b2._sweep.C += invMass2 * P;
					b2._sweep.A += invI2 * Vec2.Cross(r2, P);
					b2.SynchronizeTransform();
				}
			}

			// We can't expect minSpeparation >= -Settings.LinearSlop because we don't
			// push the separation above -Settings.LinearSlop.
			return minSeparation >= -1.5f * Settings.LinearSlop;
		}
	}
#else
#endif
}
