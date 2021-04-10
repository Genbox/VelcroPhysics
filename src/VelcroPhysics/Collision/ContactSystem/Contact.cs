/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
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
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.Narrowphase;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Shared;
using VelcroPhysics.Shared.Optimization;

namespace VelcroPhysics.Collision.ContactSystem
{
    /// <summary>
    /// The class manages contact between two shapes. A contact exists for each overlapping
    /// AABB in the broad-phase (except if filtered). Therefore a contact object may exist
    /// that has no contact points.
    /// </summary>
    public class Contact
    {
        private static EdgeShape _edge = new EdgeShape();

        private static ContactType[,] _registers =
        {
            {
                ContactType.Circle,
                ContactType.EdgeAndCircle,
                ContactType.PolygonAndCircle,
                ContactType.ChainAndCircle
            },
            {
                ContactType.EdgeAndCircle,
                ContactType.NotSupported,

                // 1,1 is invalid (no ContactType.Edge)
                ContactType.EdgeAndPolygon,
                ContactType.NotSupported

                // 1,3 is invalid (no ContactType.EdgeAndLoop)
            },
            {
                ContactType.PolygonAndCircle,
                ContactType.EdgeAndPolygon,
                ContactType.Polygon,
                ContactType.ChainAndPolygon
            },
            {
                ContactType.ChainAndCircle,
                ContactType.NotSupported,

                // 3,1 is invalid (no ContactType.EdgeAndLoop)
                ContactType.ChainAndPolygon,
                ContactType.NotSupported

                // 3,3 is invalid (no ContactType.Loop)
            }
        };

        // Nodes for connecting bodies.
        internal ContactEdge _nodeA = new ContactEdge();
        internal ContactEdge _nodeB = new ContactEdge();

        internal float _toi;
        internal int _toiCount;
        private ContactType _type;

        public Fixture FixtureA;
        public Fixture FixtureB;

        /// <summary>
        /// Get the contact manifold. Do not modify the manifold unless you understand the
        /// internals of Box2D.
        /// </summary>
        public Manifold Manifold;

        private Contact(Fixture fA, int indexA, Fixture fB, int indexB)
        {
            Reset(fA, indexA, fB, indexB);
        }

        public float Friction { get; set; }
        public float Restitution { get; set; }

        /// <summary>
        /// Get or set the desired tangent speed for a conveyor belt behavior. In meters per second.
        /// </summary>
        public float TangentSpeed { get; set; }


        /// <summary>
        /// Get the child primitive index for fixture A.
        /// </summary>
        /// <value>The child index A.</value>
        public int ChildIndexA { get; private set; }

        /// <summary>
        /// Get the child primitive index for fixture B.
        /// </summary>
        /// <value>The child index B.</value>
        public int ChildIndexB { get; private set; }

        /// <summary>
        /// Enable/disable this contact.The contact is only disabled for the current
        /// time step (or sub-step in continuous collisions).
        /// </summary>
        public bool Enabled
        {
            get => (_flags & ContactFlags.EnabledFlag) == ContactFlags.EnabledFlag;
            set
            {
                if (value)
                    _flags |= ContactFlags.EnabledFlag;
                else
                    _flags &= ~ContactFlags.EnabledFlag;
            }
        }

        internal bool IsTouching => (_flags & ContactFlags.TouchingFlag) == ContactFlags.TouchingFlag;
        internal bool IslandFlag => (_flags & ContactFlags.IslandFlag) == ContactFlags.IslandFlag;
        internal bool TOIFlag => (_flags & ContactFlags.TOIFlag) == ContactFlags.TOIFlag;
        internal bool FilterFlag => (_flags & ContactFlags.FilterFlag) == ContactFlags.FilterFlag;

        internal ContactFlags _flags;

        public void ResetRestitution()
        {
            Restitution = Settings.MixRestitution(FixtureA.Restitution, FixtureB.Restitution);
        }

        public void ResetFriction()
        {
            Friction = Settings.MixFriction(FixtureA.Friction, FixtureB.Friction);
        }

        /// <summary>
        /// Gets the world manifold.
        /// </summary>
        public void GetWorldManifold(out Vector2 normal, out FixedArray2<Vector2> points)
        {
            Body bodyA = FixtureA.Body;
            Body bodyB = FixtureB.Body;
            Shape shapeA = FixtureA.Shape;
            Shape shapeB = FixtureB.Shape;

            WorldManifold.Initialize(ref Manifold, ref bodyA._xf, shapeA.Radius, ref bodyB._xf, shapeB.Radius, out normal, out points, out _);
        }

        private void Reset(Fixture fA, int indexA, Fixture fB, int indexB)
        {
            _flags = ContactFlags.EnabledFlag;

            FixtureA = fA;
            FixtureB = fB;

            ChildIndexA = indexA;
            ChildIndexB = indexB;

            Manifold.PointCount = 0;

            _nodeA.Contact = null;
            _nodeA.Prev = null;
            _nodeA.Next = null;
            _nodeA.Other = null;

            _nodeB.Contact = null;
            _nodeB.Prev = null;
            _nodeB.Next = null;
            _nodeB.Other = null;

            _toiCount = 0;

            //Velcro: We only set the friction and restitution if we are not destroying the contact
            if (FixtureA != null && FixtureB != null)
            {
                Friction = Settings.MixFriction(FixtureA.Friction, FixtureB.Friction);
                Restitution = Settings.MixRestitution(FixtureA.Restitution, FixtureB.Restitution);
            }

            TangentSpeed = 0;
        }

        /// <summary>
        /// Update the contact manifold and touching status.
        /// Note: do not assume the fixture AABBs are overlapping or are valid.
        /// </summary>
        /// <param name="contactManager">The contact manager.</param>
        internal void Update(ContactManager contactManager)
        {
            if (FixtureA == null || FixtureB == null)
                return;

            Body bodyA = FixtureA.Body;
            Body bodyB = FixtureB.Body;

            Manifold oldManifold = Manifold;

            // Re-enable this contact.
            _flags |= ContactFlags.EnabledFlag;

            bool touching;
            bool wasTouching = IsTouching;

            bool sensor = FixtureA.IsSensor || FixtureB.IsSensor;

            // Is this contact a sensor?
            if (sensor)
            {
                Shape shapeA = FixtureA.Shape;
                Shape shapeB = FixtureB.Shape;
                touching = Narrowphase.Collision.TestOverlap(shapeA, ChildIndexA, shapeB, ChildIndexB, ref bodyA._xf, ref bodyB._xf);

                // Sensors don't generate manifolds.
                Manifold.PointCount = 0;
            }
            else
            {
                Evaluate(ref Manifold, ref bodyA._xf, ref bodyB._xf);
                touching = Manifold.PointCount > 0;

                // Match old contact ids to new contact ids and copy the
                // stored impulses to warm start the solver.
                for (int i = 0; i < Manifold.PointCount; ++i)
                {
                    ManifoldPoint mp2 = Manifold.Points[i];
                    mp2.NormalImpulse = 0.0f;
                    mp2.TangentImpulse = 0.0f;
                    ContactID id2 = mp2.Id;

                    for (int j = 0; j < oldManifold.PointCount; ++j)
                    {
                        ManifoldPoint mp1 = oldManifold.Points[j];

                        if (mp1.Id.Key == id2.Key)
                        {
                            mp2.NormalImpulse = mp1.NormalImpulse;
                            mp2.TangentImpulse = mp1.TangentImpulse;
                            break;
                        }
                    }

                    Manifold.Points[i] = mp2;
                }

                if (touching != wasTouching)
                {
                    bodyA.Awake = true;
                    bodyB.Awake = true;
                }
            }

            if (touching)
                _flags |= ContactFlags.TouchingFlag;
            else
                _flags &= ~ContactFlags.TouchingFlag;

            if (wasTouching == false && touching)
            {
                FixtureA.OnCollision?.Invoke(FixtureA, FixtureB, this);
                FixtureB.OnCollision?.Invoke(FixtureB, FixtureA, this);
                contactManager.BeginContact?.Invoke(this);

                // Velcro: If the user disabled the contact (needed to exclude it in TOI solver) at any point by
                // any of the callbacks, we need to mark it as not touching and call any separation
                // callbacks for fixtures that didn't explicitly disable the collision.
                if (!Enabled)
                    touching = false;
            }

            if (wasTouching == true && touching == false)
            {
                FixtureA?.OnSeparation?.Invoke(FixtureA, FixtureB, this);
                FixtureB?.OnSeparation?.Invoke(FixtureB, FixtureA, this);
                contactManager.EndContact?.Invoke(this);
            }

            if (sensor)
                return;

            contactManager.PreSolve?.Invoke(this, ref oldManifold);
        }

        /// <summary>
        /// Evaluate this contact with your own manifold and transforms.
        /// </summary>
        /// <param name="manifold">The manifold.</param>
        /// <param name="transformA">The first transform.</param>
        /// <param name="transformB">The second transform.</param>
        private void Evaluate(ref Manifold manifold, ref Transform transformA, ref Transform transformB)
        {
            switch (_type)
            {
                case ContactType.Polygon:
                    CollidePolygon.CollidePolygons(ref manifold, (PolygonShape)FixtureA.Shape, ref transformA, (PolygonShape)FixtureB.Shape, ref transformB);
                    break;
                case ContactType.PolygonAndCircle:
                    CollideCircle.CollidePolygonAndCircle(ref manifold, (PolygonShape)FixtureA.Shape, ref transformA, (CircleShape)FixtureB.Shape, ref transformB);
                    break;
                case ContactType.EdgeAndCircle:
                    CollideEdge.CollideEdgeAndCircle(ref manifold, (EdgeShape)FixtureA.Shape, ref transformA, (CircleShape)FixtureB.Shape, ref transformB);
                    break;
                case ContactType.EdgeAndPolygon:
                    CollideEdge.CollideEdgeAndPolygon(ref manifold, (EdgeShape)FixtureA.Shape, ref transformA, (PolygonShape)FixtureB.Shape, ref transformB);
                    break;
                case ContactType.ChainAndCircle:
                    ChainShape chain = (ChainShape)FixtureA.Shape;
                    chain.GetChildEdge(_edge, ChildIndexA);
                    CollideEdge.CollideEdgeAndCircle(ref manifold, _edge, ref transformA, (CircleShape)FixtureB.Shape, ref transformB);
                    break;
                case ContactType.ChainAndPolygon:
                    ChainShape loop2 = (ChainShape)FixtureA.Shape;
                    loop2.GetChildEdge(_edge, ChildIndexA);
                    CollideEdge.CollideEdgeAndPolygon(ref manifold, _edge, ref transformA, (PolygonShape)FixtureB.Shape, ref transformB);
                    break;
                case ContactType.Circle:
                    CollideCircle.CollideCircles(ref manifold, (CircleShape)FixtureA.Shape, ref transformA, (CircleShape)FixtureB.Shape, ref transformB);
                    break;
                default:
                    throw new ArgumentException("You are using an unsupported contact type.");
            }
        }

        internal static Contact Create(Fixture fixtureA, int indexA, Fixture fixtureB, int indexB)
        {
            ShapeType type1 = fixtureA.Shape.ShapeType;
            ShapeType type2 = fixtureB.Shape.ShapeType;

            Debug.Assert(ShapeType.Unknown < type1 && type1 < ShapeType.TypeCount);
            Debug.Assert(ShapeType.Unknown < type2 && type2 < ShapeType.TypeCount);

            Contact c;
            Queue<Contact> pool = fixtureA.Body._world._contactPool;
            if (pool.Count > 0)
            {
                c = pool.Dequeue();
                if ((type1 >= type2 || (type1 == ShapeType.Edge && type2 == ShapeType.Polygon)) && !(type2 == ShapeType.Edge && type1 == ShapeType.Polygon))
                {
                    c.Reset(fixtureA, indexA, fixtureB, indexB);
                }
                else
                {
                    c.Reset(fixtureB, indexB, fixtureA, indexA);
                }
            }
            else
            {
                // Edge+Polygon is non-symmetrical due to the way Erin handles collision type registration.
                if ((type1 >= type2 || (type1 == ShapeType.Edge && type2 == ShapeType.Polygon)) && !(type2 == ShapeType.Edge && type1 == ShapeType.Polygon))
                {
                    c = new Contact(fixtureA, indexA, fixtureB, indexB);
                }
                else
                {
                    c = new Contact(fixtureB, indexB, fixtureA, indexA);
                }
            }

            c._type = _registers[(int)type1, (int)type2];

            return c;
        }

        internal void Destroy()
        {
            FixtureA.Body._world._contactPool.Enqueue(this);

            if (Manifold.PointCount > 0 && FixtureA.IsSensor == false && FixtureB.IsSensor == false)
            {
                FixtureA.Body.Awake = true;
                FixtureB.Body.Awake = true;
            }

            Reset(null, 0, null, 0);
        }
    }
}