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
using Genbox.VelcroPhysics.Collision.Narrowphase;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Shared.Optimization;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Collision.ContactSystem
{
    /// <summary>
    /// The class manages contact between two shapes. A contact exists for each overlapping AABB in the broad-phase
    /// (except if filtered). Therefore a contact object may exist that has no contact points.
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

        //private bool _initialized;

        internal ContactFlags _flags;

        // World pool and list pointers.
        internal Contact _prev;
        internal Contact _next;

        // Nodes for connecting bodies.
        internal ContactEdge _nodeA = new ContactEdge();
        internal ContactEdge _nodeB = new ContactEdge();

        internal Fixture _fixtureA;
        internal Fixture _fixtureB;

        private int _indexA;
        private int _indexB;

        internal Manifold _manifold;

        internal int _toiCount;
        internal float _toi;

        private ContactType _type;

        internal float _friction;
        internal float _restitution;
        internal float _restitutionThreshold;

        internal float _tangentSpeed;

        private Contact(Fixture fA, int indexA, Fixture fB, int indexB)
        {
            Reset(fA, indexA, fB, indexB);
        }

        /// <summary>Get the contact manifold. Do not modify the manifold unless you understand the internals of Box2D.</summary>
        public Manifold Manifold => _manifold;

        public float Friction
        {
            get => _friction;
            set => _friction = value;
        }

        public float Restitution
        {
            get => _restitution;
            set => _restitution = value;
        }

        public float RestitutionThreshold
        {
            get => _restitutionThreshold;
            set => _restitutionThreshold = value;
        }

        /// <summary>Get or set the desired tangent speed for a conveyor belt behavior. In meters per second.</summary>
        public float TangentSpeed
        {
            get => _tangentSpeed;
            set => _tangentSpeed = value;
        }

        public Fixture FixtureA => _fixtureA;
        public Fixture FixtureB => _fixtureB;

        /// <summary>Get the child primitive index for fixture A.</summary>
        /// <value>The child index A.</value>
        public int ChildIndexA => _indexA;

        /// <summary>Get the child primitive index for fixture B.</summary>
        /// <value>The child index B.</value>
        public int ChildIndexB => _indexB;

        /// <summary>
        /// Enable/disable this contact.The contact is only disabled for the current time step (or sub-step in continuous
        /// collisions).
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

        public Contact Next => _next;
        public Contact Previous => _prev;

        internal bool IsTouching => (_flags & ContactFlags.TouchingFlag) == ContactFlags.TouchingFlag;
        internal bool IslandFlag => (_flags & ContactFlags.IslandFlag) == ContactFlags.IslandFlag;
        internal bool TOIFlag => (_flags & ContactFlags.TOIFlag) == ContactFlags.TOIFlag;
        internal bool FilterFlag => (_flags & ContactFlags.FilterFlag) == ContactFlags.FilterFlag;

        public void ResetRestitution()
        {
            _restitution = Settings.MixRestitution(_fixtureA.Restitution, _fixtureB.Restitution);
        }

        public void ResetRestitutionThreshold()
        {
            _restitutionThreshold = Settings.MixRestitutionThreshold(_fixtureA.Restitution, _fixtureB.Restitution);
        }

        public void ResetFriction()
        {
            _friction = Settings.MixFriction(_fixtureA.Friction, _fixtureB.Friction);
        }

        /// <summary>Gets the world manifold.</summary>
        public void GetWorldManifold(out Vector2 normal, out FixedArray2<Vector2> points)
        {
            Body bodyA = _fixtureA.Body;
            Body bodyB = _fixtureB.Body;
            Shape shapeA = _fixtureA.Shape;
            Shape shapeB = _fixtureB.Shape;

            WorldManifold.Initialize(ref _manifold, ref bodyA._xf, shapeA._radius, ref bodyB._xf, shapeB._radius, out normal, out points, out _);
        }

        private void Reset(Fixture fA, int indexA, Fixture fB, int indexB)
        {
            _flags = ContactFlags.EnabledFlag;

            _fixtureA = fA;
            _fixtureB = fB;

            _indexA = indexA;
            _indexB = indexB;

            _manifold.PointCount = 0;

            _prev = null;
            _next = null;

            _nodeA.Contact = null;
            _nodeA.Prev = null;
            _nodeA.Next = null;
            _nodeA.Other = null;

            _nodeB.Contact = null;
            _nodeB.Prev = null;
            _nodeB.Next = null;
            _nodeB.Other = null;

            _toiCount = 0;

            //Velcro: We only set the friction and restitution if we are not resetting the contact
            if (_fixtureA != null && _fixtureB != null)
            {
                _friction = Settings.MixFriction(_fixtureA._friction, _fixtureB._friction);
                _restitution = Settings.MixRestitution(_fixtureA._restitution, _fixtureB._restitution);
                _restitutionThreshold = Settings.MixRestitutionThreshold(_fixtureA._restitutionThreshold, _fixtureB._restitutionThreshold);
            }

            _tangentSpeed = 0;
        }

        /// <summary>
        /// Update the contact manifold and touching status. Note: do not assume the fixture AABBs are overlapping or are
        /// valid.
        /// </summary>
        /// <param name="contactManager">The contact manager.</param>
        internal void Update(ContactManager contactManager)
        {
            //Velcro: Do not try and update destroyed contacts
            if (_fixtureA == null || _fixtureB == null)
                return;

            Manifold oldManifold = _manifold;

            // Re-enable this contact.
            _flags |= ContactFlags.EnabledFlag;

            bool touching;
            bool wasTouching = IsTouching;

            bool sensor = _fixtureA.IsSensor || _fixtureB.IsSensor;

            Body bodyA = _fixtureA.Body;
            Body bodyB = _fixtureB.Body;

            Transform xfA = bodyA._xf;
            Transform xfB = bodyB._xf;

            // Is this contact a sensor?
            if (sensor)
            {
                Shape shapeA = _fixtureA.Shape;
                Shape shapeB = _fixtureB.Shape;
                touching = Narrowphase.Collision.TestOverlap(shapeA, _indexA, shapeB, _indexB, ref xfA, ref xfB);

                // Sensors don't generate manifolds.
                _manifold.PointCount = 0;
            }
            else
            {
                Evaluate(ref _manifold, ref xfA, ref xfB);
                touching = _manifold.PointCount > 0;

                // Match old contact ids to new contact ids and copy the
                // stored impulses to warm start the solver.
                for (int i = 0; i < _manifold.PointCount; ++i)
                {
                    ManifoldPoint mp2 = _manifold.Points[i];
                    mp2.NormalImpulse = 0.0f;
                    mp2.TangentImpulse = 0.0f;
                    ContactId id2 = mp2.Id;

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

                    _manifold.Points[i] = mp2;
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
                //Velcro: Report the collision to both fixtures:
                _fixtureA.OnCollision?.Invoke(_fixtureA, _fixtureB, this);
                _fixtureB.OnCollision?.Invoke(_fixtureB, _fixtureA, this);

                //Velcro: Report the collision to both bodies:
                bodyA.OnCollision?.Invoke(_fixtureA, _fixtureB, this);
                bodyB.OnCollision?.Invoke(_fixtureB, _fixtureA, this);

                // Call BeginContact. It can disable the contact as well.
                contactManager.BeginContact?.Invoke(this);

                // Velcro: If the user disabled the contact (needed to exclude it in TOI solver) at any point by
                // any of the callbacks, we need to mark it as not touching and call any separation
                // callbacks for fixtures that didn't explicitly disable the collision.
                if (!Enabled)
                    touching = false;
            }

            if (wasTouching && !touching)
            {
                //Report the separation to both fixtures:
                _fixtureA.OnSeparation?.Invoke(_fixtureA, _fixtureB, this);
                _fixtureB.OnSeparation?.Invoke(_fixtureB, _fixtureA, this);

                //Report the separation to both bodies:
                bodyA.OnSeparation?.Invoke(_fixtureA, _fixtureB, this);
                bodyB.OnSeparation?.Invoke(_fixtureB, _fixtureA, this);

                contactManager.EndContact?.Invoke(this);
            }

            if (!sensor && touching)
                contactManager.PreSolve?.Invoke(this, ref oldManifold);
        }

        /// <summary>Evaluate this contact with your own manifold and transforms.</summary>
        /// <param name="manifold">The manifold.</param>
        /// <param name="transformA">The first transform.</param>
        /// <param name="transformB">The second transform.</param>
        private void Evaluate(ref Manifold manifold, ref Transform transformA, ref Transform transformB)
        {
            switch (_type)
            {
                case ContactType.Polygon:
                    CollidePolygon.CollidePolygons(ref manifold, (PolygonShape)_fixtureA.Shape, ref transformA, (PolygonShape)_fixtureB.Shape, ref transformB);
                    break;
                case ContactType.PolygonAndCircle:
                    CollideCircle.CollidePolygonAndCircle(ref manifold, (PolygonShape)_fixtureA.Shape, ref transformA, (CircleShape)_fixtureB.Shape, ref transformB);
                    break;
                case ContactType.EdgeAndCircle:
                    CollideEdge.CollideEdgeAndCircle(ref manifold, (EdgeShape)_fixtureA.Shape, ref transformA, (CircleShape)_fixtureB.Shape, ref transformB);
                    break;
                case ContactType.EdgeAndPolygon:
                    CollideEdge.CollideEdgeAndPolygon(ref manifold, (EdgeShape)_fixtureA.Shape, ref transformA, (PolygonShape)_fixtureB.Shape, ref transformB);
                    break;
                case ContactType.ChainAndCircle:
                    ChainShape chain = (ChainShape)_fixtureA.Shape;
                    chain.GetChildEdge(_edge, ChildIndexA);
                    CollideEdge.CollideEdgeAndCircle(ref manifold, _edge, ref transformA, (CircleShape)_fixtureB.Shape, ref transformB);
                    break;
                case ContactType.ChainAndPolygon:
                    ChainShape loop2 = (ChainShape)_fixtureA.Shape;
                    loop2.GetChildEdge(_edge, ChildIndexA);
                    CollideEdge.CollideEdgeAndPolygon(ref manifold, _edge, ref transformA, (PolygonShape)_fixtureB.Shape, ref transformB);
                    break;
                case ContactType.Circle:
                    CollideCircle.CollideCircles(ref manifold, (CircleShape)_fixtureA.Shape, ref transformA, (CircleShape)_fixtureB.Shape, ref transformB);
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
                if ((type1 >= type2 || type1 == ShapeType.Edge && type2 == ShapeType.Polygon) && !(type2 == ShapeType.Edge && type1 == ShapeType.Polygon))
                    c.Reset(fixtureA, indexA, fixtureB, indexB);
                else
                    c.Reset(fixtureB, indexB, fixtureA, indexA);
            }
            else
            {
                // Edge+Polygon is non-symmetrical due to the way Erin handles collision type registration.
                if ((type1 >= type2 || type1 == ShapeType.Edge && type2 == ShapeType.Polygon) && !(type2 == ShapeType.Edge && type1 == ShapeType.Polygon))
                    c = new Contact(fixtureA, indexA, fixtureB, indexB);
                else
                    c = new Contact(fixtureB, indexB, fixtureA, indexA);
            }

            c._type = _registers[(int)type1, (int)type2];

            return c;
        }

        internal void Destroy()
        {
            //Debug.Assert(_initialized);

            //Fixture fixtureA = _fixtureA;
            //Fixture fixtureB = _fixtureB;

            if (_manifold.PointCount > 0 && !_fixtureA.IsSensor && !_fixtureB.IsSensor)
            {
                _fixtureA.Body.Awake = true;
                _fixtureB.Body.Awake = true;
            }

            //b2Shape::Type typeA = fixtureA->GetType();
            //b2Shape::Type typeB = fixtureB->GetType();

            //Debug.Assert(0 <= typeA && typeA < b2Shape::e_typeCount);
            //Debug.Assert(0 <= typeB && typeB < b2Shape::e_typeCount);

            _fixtureA._body._world._contactPool.Enqueue(this);

            Reset(null, 0, null, 0);
        }
    }
}