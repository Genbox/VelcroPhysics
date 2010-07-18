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

using System;
using System.Diagnostics;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Contacts
{
    internal struct TOIConstraint
    {
        public FixedArray2<Vector2> localPoints;
        public Vector2 localNormal;
        public Vector2 localPoint;
        public ManifoldType type;
        public float radius;
        public int pointCount;
        public Body bodyA;
        public Body bodyB;
    }

    internal struct TOISolverManifold
    {
        public TOISolverManifold(ref TOIConstraint cc, int index)
        {
            Debug.Assert(cc.pointCount > 0);

            switch (cc.type)
            {
                case ManifoldType.Circles:
                    {
                        Vector2 pointA = cc.bodyA.GetWorldPoint(cc.localPoint);
                        Vector2 pointB = cc.bodyB.GetWorldPoint(cc.localPoints[0]);
                        if ((pointA - pointB).LengthSquared() > Settings.Epsilon * Settings.Epsilon)
                        {
                            normal = pointB - pointA;
                            normal.Normalize();
                        }
                        else
                        {
                            normal = new Vector2(1.0f, 0.0f);
                        }

                        point = 0.5f * (pointA + pointB);
                        separation = Vector2.Dot(pointB - pointA, normal) - cc.radius;
                    }
                    break;

                case ManifoldType.FaceA:
                    {
                        normal = cc.bodyA.GetWorldVector(cc.localNormal);
                        Vector2 planePoint = cc.bodyA.GetWorldPoint(cc.localPoint);

                        Vector2 clipPoint = cc.bodyB.GetWorldPoint(cc.localPoints[index]);
                        separation = Vector2.Dot(clipPoint - planePoint, normal) - cc.radius;
                        point = clipPoint;
                    }
                    break;

                case ManifoldType.FaceB:
                    {
                        normal = cc.bodyB.GetWorldVector(cc.localNormal);
                        Vector2 planePoint = cc.bodyB.GetWorldPoint(cc.localPoint);

                        Vector2 clipPoint = cc.bodyA.GetWorldPoint(cc.localPoints[index]);
                        separation = Vector2.Dot(clipPoint - planePoint, normal) - cc.radius;
                        point = clipPoint;

                        // Ensure normal points from A to B
                        normal = -normal;
                    }
                    break;
                default:
                    normal = Vector2.UnitY;
                    point = Vector2.Zero;
                    separation = 0.0f;
                    break;
            }
        }

        internal Vector2 normal;
        internal Vector2 point;
        internal float separation;
    }


    internal class TOISolver
    {
        public TOISolver() { }

        public void Initialize(Contact[] contacts, int count, Body toiBody)
        {
            _count = count;
            _toiBody = toiBody;
            if (_constraints.Length < _count)
                _constraints = new TOIConstraint[Math.Max(_constraints.Length * 2, _count)];

            for (int i = 0; i < _count; ++i)
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

                TOIConstraint constraint = _constraints[i];
                constraint.bodyA = bodyA;
                constraint.bodyB = bodyB;
                constraint.localNormal = manifold.LocalNormal;
                constraint.localPoint = manifold.LocalPoint;
                constraint.type = manifold.Type;
                constraint.pointCount = manifold.PointCount;
                constraint.radius = radiusA + radiusB;

                for (int j = 0; j < constraint.pointCount; ++j)
                {
                    constraint.localPoints[j] = manifold.Points[j].LocalPoint;
                }

                _constraints[i] = constraint;
            }
        }

        // Perform one solver iteration. Returns true if converged.
        public bool Solve(float baumgarte)
        {
            float minSeparation = 0.0f;

            for (int i = 0; i < _count; ++i)
            {
                TOIConstraint c = _constraints[i];
                Body bodyA = c.bodyA;
                Body bodyB = c.bodyB;

                float massA = bodyA._mass;
                float massB = bodyB._mass;

                // Only the TOI body should move.
                if (bodyA == _toiBody)
                {
                    massB = 0.0f;
                }
                else
                {
                    massA = 0.0f;
                }

                float invMassA = massA * bodyA._invMass;
                float invIA = massA * bodyA._invI;
                float invMassB = massB * bodyB._invMass;
                float invIB = massB * bodyB._invI;

                // Solve normal constraints
                for (int j = 0; j < c.pointCount; ++j)
                {
                    TOISolverManifold psm = new TOISolverManifold(ref c, j);

                    Vector2 normal = psm.normal;
                    Vector2 point = psm.point;
                    float separation = psm.separation;

                    Vector2 rA = point - bodyA._sweep.c;
                    Vector2 rB = point - bodyB._sweep.c;

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

                    bodyA._sweep.c -= invMassA * P;
                    bodyA._sweep.a -= invIA * MathUtils.Cross(rA, P);
                    bodyA.SynchronizeTransform();

                    bodyB._sweep.c += invMassB * P;
                    bodyB._sweep.a += invIB * MathUtils.Cross(rB, P);
                    bodyB.SynchronizeTransform();
                }
            }

            // We can't expect minSpeparation >= -b2_linearSlop because we don't
            // push the separation above -b2_linearSlop.
            return minSeparation >= -1.5f * Settings.LinearSlop;
        }

        TOIConstraint[] _constraints = new TOIConstraint[8];
        int _count;
        Body _toiBody;
    }
}
