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
        public Body BodyA;
        public Body BodyB;
        public Vector2 LocalNormal;
        public Vector2 LocalPoint;
        public FixedArray2<Vector2> LocalPoints;
        public int PointCount;
        public float Radius;
        public ManifoldType Type;
    }

    internal struct TOISolverManifold
    {
        internal Vector2 Normal;
        internal Vector2 Point;
        internal float Separation;

        public TOISolverManifold(ref TOIConstraint cc, int index)
        {
            Debug.Assert(cc.PointCount > 0);

            switch (cc.Type)
            {
                case ManifoldType.Circles:
                    {
                        Vector2 pointA = cc.BodyA.GetWorldPoint(ref cc.LocalPoint);
                        Vector2 pointB = cc.BodyB.GetWorldPoint(cc.LocalPoints[0]);
                        if ((pointA - pointB).LengthSquared() > Settings.Epsilon * Settings.Epsilon)
                        {
                            Normal = pointB - pointA;
                            Normal.Normalize();
                        }
                        else
                        {
                            Normal = new Vector2(1.0f, 0.0f);
                        }

                        Point = 0.5f * (pointA + pointB);
                        Separation = Vector2.Dot(pointB - pointA, Normal) - cc.Radius;
                    }
                    break;

                case ManifoldType.FaceA:
                    {
                        Normal = cc.BodyA.GetWorldVector(ref cc.LocalNormal);
                        Vector2 planePoint = cc.BodyA.GetWorldPoint(ref cc.LocalPoint);

                        Vector2 clipPoint = cc.BodyB.GetWorldPoint(cc.LocalPoints[index]);
                        Separation = Vector2.Dot(clipPoint - planePoint, Normal) - cc.Radius;
                        Point = clipPoint;
                    }
                    break;

                case ManifoldType.FaceB:
                    {
                        Normal = cc.BodyB.GetWorldVector(ref cc.LocalNormal);
                        Vector2 planePoint = cc.BodyB.GetWorldPoint(ref cc.LocalPoint);

                        Vector2 clipPoint = cc.BodyA.GetWorldPoint(cc.LocalPoints[index]);
                        Separation = Vector2.Dot(clipPoint - planePoint, Normal) - cc.Radius;
                        Point = clipPoint;

                        // Ensure normal points from A to B
                        Normal = -Normal;
                    }
                    break;
                default:
                    Normal = Vector2.UnitY;
                    Point = Vector2.Zero;
                    Separation = 0.0f;
                    break;
            }
        }
    }

    internal class TOISolver
    {
        private TOIConstraint[] _constraints = new TOIConstraint[8];
        private int _count;
        private Body _toiBody;

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
                constraint.BodyA = bodyA;
                constraint.BodyB = bodyB;
                constraint.LocalNormal = manifold.LocalNormal;
                constraint.LocalPoint = manifold.LocalPoint;
                constraint.Type = manifold.Type;
                constraint.PointCount = manifold.PointCount;
                constraint.Radius = radiusA + radiusB;

                for (int j = 0; j < constraint.PointCount; ++j)
                {
                    constraint.LocalPoints[j] = manifold.Points[j].LocalPoint;
                }

                _constraints[i] = constraint;
            }
        }

        /// <summary>
        /// Perform one solver iteration. Returns true if converged.
        /// </summary>
        /// <param name="baumgarte">The baumgarte value.</param>
        /// <returns></returns>
        public bool Solve(float baumgarte)
        {
            float minSeparation = 0.0f;

            for (int i = 0; i < _count; ++i)
            {
                TOIConstraint c = _constraints[i];
                Body bodyA = c.BodyA;
                Body bodyB = c.BodyB;

                float massA = bodyA.Mass;
                float massB = bodyB.Mass;

                // Only the TOI body should move.
                if (bodyA == _toiBody)
                {
                    massB = 0.0f;
                }
                else
                {
                    massA = 0.0f;
                }

                float invMassA = massA * bodyA.InvMass;
                float invIA = massA * bodyA.InvI;
                float invMassB = massB * bodyB.InvMass;
                float invIB = massB * bodyB.InvI;

                // Solve normal constraints
                for (int j = 0; j < c.PointCount; ++j)
                {
                    TOISolverManifold psm = new TOISolverManifold(ref c, j);

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
}