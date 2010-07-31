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

namespace FarseerPhysics.Dynamics.Contacts
{
    /// A contact edge is used to connect bodies and contacts together
    /// in a contact graph where each body is a node and each contact
    /// is an edge. A contact edge belongs to a doubly linked list
    /// maintained in each attached body. Each contact has two contact
    /// nodes, one for each attached body.
    public class ContactEdge
    {
        /// the contact
        public Contact Contact;

        /// the next contact edge in the body's contact list
        public ContactEdge Next;

        /// provides quick access to the other body attached.
        public Body Other;

        /// the previous contact edge in the body's contact list
        public ContactEdge Prev;
    }

    [Flags]
    public enum ContactFlags
    {
        None = 0,

        // Used when crawling contact graph when forming islands.
        Island = 0x0001,

        // Set when the shapes are touching.
        Touching = 0x0002,

        // This contact can be disabled (by user)
        Enabled = 0x0004,

        // This contact needs filtering because a fixture filter was changed.
        Filter = 0x0008,

        // This bullet contact had a TOI event
        BulletHit = 0x0010,
    }

    /// The class manages contact between two shapes. A contact exists for each overlapping
    /// AABB in the broad-phase (except if filtered). Therefore a contact object may exist
    /// that has no contact points.
    public class Contact
    {
        private static EdgeShape s_edge = new EdgeShape();

        internal static ContactType[,] s_registers = new[,]
                                                         {
                                                             {
                                                                 ContactType.Circle,
                                                                 ContactType.EdgeAndCircle,
                                                                 ContactType.PolygonAndCircle,
                                                                 ContactType.LoopAndCircle,
                                                             },
                                                             {
                                                                 ContactType.EdgeAndCircle,
                                                                 ContactType.EdgeAndCircle,
                                                                 // 1,1 is invalid (no ContactType.Edge)
                                                                 ContactType.EdgeAndPolygon,
                                                                 ContactType.EdgeAndPolygon,
                                                                 // 1,3 is invalid (no ContactType.EdgeAndLoop)
                                                             },
                                                             {
                                                                 ContactType.PolygonAndCircle,
                                                                 ContactType.EdgeAndPolygon,
                                                                 ContactType.Polygon,
                                                                 ContactType.LoopAndPolygon,
                                                             },
                                                             {
                                                                 ContactType.LoopAndCircle,
                                                                 ContactType.LoopAndCircle,
                                                                 // 3,1 is invalid (no ContactType.EdgeAndLoop)
                                                                 ContactType.LoopAndPolygon,
                                                                 ContactType.LoopAndPolygon,
                                                                 // 3,3 is invalid (no ContactType.Loop)
                                                             },
                                                         };

        internal Fixture _fixtureA;
        internal Fixture _fixtureB;
        internal ContactFlags _flags;
        internal int _indexA;
        internal int _indexB;

        internal Manifold _manifold;
        internal Contact _next;

        // Nodes for connecting bodies.
        internal ContactEdge _nodeA = new ContactEdge();
        internal ContactEdge _nodeB = new ContactEdge();
        internal Contact _prev;
        internal int _toiCount;
        private ContactType _type;

        internal Contact(Fixture fA, int indexA, Fixture fB, int indexB)
        {
            Reset(fA, indexA, fB, indexB);
        }

        /// Enable/disable this contact. This can be used inside the pre-solve
        /// contact listener. The contact is only disabled for the current
        /// time step (or sub-step in continuous collisions).
        public bool Enabled
        {
            set
            {
                if (value)
                {
                    _flags |= ContactFlags.Enabled;
                }
                else
                {
                    _flags &= ~ContactFlags.Enabled;
                }
            }

            get { return (_flags & ContactFlags.Enabled) == ContactFlags.Enabled; }
        }

        /// Get fixture A in this contact.
        public Fixture FixtureA
        {
            get { return _fixtureA; }
        }

        /// Get fixture B in this contact.
        public Fixture FixtureB
        {
            get { return _fixtureB; }
        }

        /// Get the child primitive index for fixture A.
        public int ChildIndexA
        {
            get { return _indexA; }
        }

        /// Get the child primitive index for fixture B.
        public int ChildIndexB
        {
            get { return _indexB; }
        }

        /// Get the contact manifold. Do not modify the manifold unless you understand the
        /// internals of Box2D.
        public void GetManifold(out Manifold manifold)
        {
            manifold = _manifold;
        }

        /// Get the world manifold.
        public void GetWorldManifold(out WorldManifold worldManifold)
        {
            Body bodyA = _fixtureA.Body;
            Body bodyB = _fixtureB.Body;
            Shape shapeA = _fixtureA.Shape;
            Shape shapeB = _fixtureB.Shape;

            Transform xfA, xfB;
            bodyA.GetTransform(out xfA);
            bodyB.GetTransform(out xfB);

            worldManifold = new WorldManifold(ref _manifold, ref xfA, shapeA.Radius, ref xfB, shapeB.Radius);
        }

        /// Is this contact touching?
        public bool IsTouching()
        {
            return (_flags & ContactFlags.Touching) == ContactFlags.Touching;
        }

        /// Get the next contact in the world's contact list.
        public Contact GetNext()
        {
            return _next;
        }

        /// Flag this contact for filtering. Filtering will occur the next time step.
        public void FlagForFiltering()
        {
            _flags |= ContactFlags.Filter;
        }

        internal void Reset(Fixture fA, int indexA, Fixture fB, int indexB)
        {
            _flags = ContactFlags.Enabled;

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
        }

        // Update the contact manifold and touching status.
        // Note: do not assume the fixture AABBs are overlapping or are valid.
        internal void Update(ContactManager contactManager)
        {
            Manifold oldManifold = _manifold;

            // Re-enable this contact.
            _flags |= ContactFlags.Enabled;

            bool touching = false;
            bool wasTouching = (_flags & ContactFlags.Touching) == ContactFlags.Touching;

            bool sensorA = _fixtureA.IsSensor;
            bool sensorB = _fixtureB.IsSensor;
            bool sensor = sensorA || sensorB;

            Body bodyA = _fixtureA.Body;
            Body bodyB = _fixtureB.Body;
            Transform xfA;
            bodyA.GetTransform(out xfA);
            Transform xfB;
            bodyB.GetTransform(out xfB);

            // Is this contact a sensor?
            if (sensor)
            {
                Shape shapeA = _fixtureA.Shape;
                Shape shapeB = _fixtureB.Shape;
                touching = AABB.TestOverlap(shapeA, _indexA, shapeB, _indexB, ref xfA, ref xfB);

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
                    ContactID id2 = mp2.Id;
                    bool found = false;

                    for (int j = 0; j < oldManifold.PointCount; ++j)
                    {
                        ManifoldPoint mp1 = oldManifold.Points[j];

                        if (mp1.Id.Key == id2.Key)
                        {
                            mp2.NormalImpulse = mp1.NormalImpulse;
                            mp2.TangentImpulse = mp1.TangentImpulse;
                            found = true;
                            break;
                        }
                    }
                    if (found == false)
                    {
                        mp2.NormalImpulse = 0.0f;
                        mp2.TangentImpulse = 0.0f;
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
            {
                _flags |= ContactFlags.Touching;
            }
            else
            {
                _flags &= ~ContactFlags.Touching;
            }

            if (wasTouching == false && touching && null != contactManager)
            {
                if (contactManager.BeginContact != null)
                    contactManager.BeginContact(this);
            }

            if (wasTouching && touching == false && null != contactManager)
            {
                if (contactManager.EndContact != null)
                    contactManager.EndContact(this);
            }

            if (sensor == false && null != contactManager)
            {
                if (contactManager.PreSolve != null)
                    contactManager.PreSolve(this, ref oldManifold);
            }
        }

        /// Evaluate this contact with your own manifold and transforms.   
        internal void Evaluate(ref Manifold manifold, ref Transform xfA, ref Transform xfB)
        {
            switch (_type)
            {
                case ContactType.Polygon:
                    Collision.Collision.CollidePolygons(ref manifold,
                                                        (PolygonShape)_fixtureA.Shape, ref xfA,
                                                        (PolygonShape)_fixtureB.Shape, ref xfB);
                    break;
                case ContactType.PolygonAndCircle:
                    Collision.Collision.CollidePolygonAndCircle(ref manifold,
                                                                (PolygonShape)_fixtureA.Shape, ref xfA,
                                                                (CircleShape)_fixtureB.Shape, ref xfB);
                    break;
                case ContactType.EdgeAndCircle:
                    Collision.Collision.CollideEdgeAndCircle(ref manifold,
                                                             (EdgeShape)_fixtureA.Shape, ref xfA,
                                                             (CircleShape)_fixtureB.Shape, ref xfB);
                    break;
                case ContactType.EdgeAndPolygon:
                    Collision.Collision.CollideEdgeAndPolygon(ref manifold,
                                                              (EdgeShape)_fixtureA.Shape, ref xfA,
                                                              (PolygonShape)_fixtureB.Shape, ref xfB);
                    break;
                case ContactType.LoopAndCircle:
                    var loop = (LoopShape)_fixtureA.Shape;
                    loop.GetChildEdge(ref s_edge, _indexA);
                    Collision.Collision.CollideEdgeAndCircle(ref manifold, s_edge, ref xfA,
                                                             (CircleShape)_fixtureB.Shape, ref xfB);
                    break;
                case ContactType.LoopAndPolygon:
                    var loop2 = (LoopShape)_fixtureA.Shape;
                    loop2.GetChildEdge(ref s_edge, _indexA);
                    Collision.Collision.CollideEdgeAndPolygon(ref manifold, s_edge, ref xfA,
                                                              (PolygonShape)_fixtureB.Shape, ref xfB);
                    break;
                case ContactType.Circle:
                    Collision.Collision.CollideCircles(ref manifold,
                                                       (CircleShape)_fixtureA.Shape, ref xfA,
                                                       (CircleShape)_fixtureB.Shape, ref xfB);
                    break;
            }
        }

        internal static Contact Create(Fixture fixtureA, int indexA, Fixture fixtureB, int indexB)
        {
            ShapeType type1 = fixtureA.ShapeType;
            ShapeType type2 = fixtureB.ShapeType;

            Debug.Assert(ShapeType.Unknown < type1 && type1 < ShapeType.TypeCount);
            Debug.Assert(ShapeType.Unknown < type2 && type2 < ShapeType.TypeCount);

            Contact c;
            var pool = fixtureA._body._world._contactPool;
            if (pool.Count > 0)
            {
                c = pool.Dequeue();
                if ((type1 >= type2 || (type1 == ShapeType.Edge && type2 == ShapeType.Polygon))
                    &&
                    !(type2 == ShapeType.Edge && type1 == ShapeType.Polygon))
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
                // Edge+Polygon is non-symetrical due to the way Erin handles collision type registration.
                if ((type1 >= type2 || (type1 == ShapeType.Edge && type2 == ShapeType.Polygon))
                    &&
                    !(type2 == ShapeType.Edge && type1 == ShapeType.Polygon))
                {
                    c = new Contact(fixtureA, indexA, fixtureB, indexB);
                }
                else
                {
                    c = new Contact(fixtureB, indexB, fixtureA, indexA);
                }
            }

            c._type = s_registers[(int)type1, (int)type2];

            return c;
        }

        internal void Destroy()
        {
            _fixtureA._body._world._contactPool.Enqueue(this);
            Reset(null, 0, null, 0);
        }

        #region Nested type: ContactType

        internal enum ContactType
        {
            Polygon,
            PolygonAndCircle,
            Circle,
            EdgeAndPolygon,
            EdgeAndCircle,
            LoopAndPolygon,
            LoopAndCircle,
        }

        #endregion
    }
}