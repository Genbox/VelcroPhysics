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
        public Body BodyA;
        public Body BodyB;
        public Vector2 LocalNormal;
        public Vector2 LocalPoint;
        public FixedArray2<Vector2> LocalPoints;
        public int PointCount;
        public float Radius;
        public ManifoldType Type;
    }

    internal class TOISolver
    {
        private TOIConstraint[] _constraints = new TOIConstraint[1];
        private int _count;
        private Vector2 _normal;
        private Vector2 _point;
        private float _separation;

        private Body _toiBody;

        public void Initialize(Contact[] contacts, int count, Body toiBody)
        {
            _count = count;
            _toiBody = toiBody;

            if (_constraints.Length < _count)
                _constraints = new TOIConstraint[_count];

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
                Manifold manifold = contact.Manifold;

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

        // Perform one solver iteration. Returns true if converged.
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

                float invMassA = massA * bodyA._invMass;
                float invIA = massA * bodyA._invI;
                float invMassB = massB * bodyB._invMass;
                float invIB = massB * bodyB._invI;

                // Solve normal constraints
                for (int j = 0; j < c.PointCount; ++j)
                {
                    switch (c.Type)
                    {
                        case ManifoldType.Circles:
                            {
                                Vector2 pointA = c.BodyA.GetWorldPoint(c.LocalPoint);
                                Vector2 pointB = c.BodyB.GetWorldPoint(c.LocalPoints[0]);
                                if ((pointA - pointB).LengthSquared() > Settings.Epsilon * Settings.Epsilon)
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

                                Vector2 clipPoint = c.BodyB.GetWorldPoint(c.LocalPoints[j]);
                                _separation = Vector2.Dot(clipPoint - planePoint, _normal) - c.Radius;
                                _point = clipPoint;
                            }
                            break;

                        case ManifoldType.FaceB:
                            {
                                _normal = c.BodyB.GetWorldVector(c.LocalNormal);
                                Vector2 planePoint = c.BodyB.GetWorldPoint(c.LocalPoint);

                                Vector2 clipPoint = c.BodyA.GetWorldPoint(c.LocalPoints[j]);
                                _separation = Vector2.Dot(clipPoint - planePoint, _normal) - c.Radius;
                                _point = clipPoint;

                                // Ensure normal points from A to B
                                _normal = -_normal;
                            }
                            break;
                        default:
                            _normal = Vector2.UnitY;
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

                    Vector2 P = impulse * _normal;

                    bodyA._sweep.Center -= invMassA * P;
                    bodyA._sweep.Angle -= invIA * MathUtils.Cross(rA, P);
                    bodyA.SynchronizeTransform();

                    bodyB._sweep.Center += invMassB * P;
                    bodyB._sweep.Angle += invIB * MathUtils.Cross(rB, P);
                    bodyB.SynchronizeTransform();
                }
            }

            // We can't expect minSpeparation >= -b2_linearSlop because we don't
            // push the separation above -b2_linearSlop.
            return minSeparation >= -1.5f * Settings.LinearSlop;
        }
    }
}