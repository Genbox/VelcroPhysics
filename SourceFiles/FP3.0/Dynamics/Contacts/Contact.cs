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
        /// provides quick access to the other body attached.
        public Contact Contact;

        /// the previous contact edge in the body's contact list
        public ContactEdge Next;

        /// the next contact edge in the body's contact list
        public Body Other;

        /// the contact
        public ContactEdge Prev;
    } ;

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
        internal static Func<ContactType>[,] DetermineType = new Func<ContactType>[,]
                                                                 {
                                                                     {
                                                                         () => ContactType.CircleContact,
                                                                         () => ContactType.PolygonAndCircleContact
                                                                     },
                                                                     {
                                                                         () => ContactType.PolygonAndCircleContact,
                                                                         () => ContactType.PolygonContact
                                                                     },
                                                                 };

        internal ContactType ContactType;

        public Fixture FixtureA;
        public Fixture FixtureB;
        internal ContactFlags Flags;

        public Manifold Manifold;
        internal ContactEdge NodeA = new ContactEdge();
        internal ContactEdge NodeB = new ContactEdge();

        internal int ToiCount;

        public Contact()
        {
            FixtureA = null;
            FixtureB = null;
        }

        internal Contact(Fixture fA, Fixture fB)
        {
            Flags = ContactFlags.Enabled;

            FixtureA = fA;
            FixtureB = fB;

            Manifold.PointCount = 0;

            NodeA.Contact = null;
            NodeA.Prev = null;
            NodeA.Next = null;
            NodeA.Other = null;

            NodeB.Contact = null;
            NodeB.Prev = null;
            NodeB.Next = null;
            NodeB.Other = null;

            ToiCount = 0;
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
                    Flags |= ContactFlags.Enabled;
                }
                else
                {
                    Flags &= ~ContactFlags.Enabled;
                }
            }
            get { return (Flags & ContactFlags.Enabled) == ContactFlags.Enabled; }
        }

        /// Get the world manifold.
        public void GetWorldManifold(out WorldManifold worldManifold)
        {
            Body bodyA = FixtureA.Body;
            Body bodyB = FixtureB.Body;
            Shape shapeA = FixtureA.Shape;
            Shape shapeB = FixtureB.Shape;

            Transform xfA, xfB;
            bodyA.GetTransform(out xfA);
            bodyB.GetTransform(out xfB);

            worldManifold = new WorldManifold(ref Manifold, ref xfA, shapeA.Radius, ref xfB, shapeB.Radius);
        }

        /// Is this contact touching?
        public bool IsTouching()
        {
            return (Flags & ContactFlags.Touching) == ContactFlags.Touching;
        }

        /// Flag this contact for filtering. Filtering will occur the next time step.
        public void FlagForFiltering()
        {
            Flags |= ContactFlags.Filter;
        }

        internal void Update(ContactManager contactManager)
        {
            Manifold oldManifold = Manifold;

            // Re-enable this contact.
            Flags |= ContactFlags.Enabled;

            bool touching = false;
            bool wasTouching = (Flags & ContactFlags.Touching) == ContactFlags.Touching;

            bool sensorA = FixtureA.Sensor;
            bool sensorB = FixtureB.Sensor;
            bool sensor = sensorA || sensorB;

            Body bodyA = FixtureA.Body;
            Body bodyB = FixtureB.Body;
            Transform xfA;
            bodyA.GetTransform(out xfA);
            Transform xfB;
            bodyB.GetTransform(out xfB);

            // Is this contact a sensor?
            if (sensor)
            {
                Shape shapeA = FixtureA.Shape;
                Shape shapeB = FixtureB.Shape;
                touching = AABB.TestOverlap(shapeA, shapeB, ref xfA, ref xfB);

                // Sensors don't generate manifolds.
                Manifold.PointCount = 0;
            }
            else
            {
                switch (ContactType)
                {
                    case ContactType.CircleContact:
                        CollisionManager.CollideCircles(out Manifold, (CircleShape) FixtureA.Shape, ref xfA,
                                                        (CircleShape) FixtureB.Shape, ref xfB);
                        break;
                    case ContactType.PolygonAndCircleContact:
                        CollisionManager.CollidePolygonAndCircle(out Manifold, (PolygonShape) FixtureA.Shape, ref xfA,
                                                                 (CircleShape) FixtureB.Shape, ref xfB);
                        break;
                    case ContactType.PolygonContact:
                        CollisionManager.CollidePolygons(out Manifold, (PolygonShape) FixtureA.Shape, ref xfA,
                                                         (PolygonShape) FixtureB.Shape, ref xfB);
                        break;
                }

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
            {
                Flags |= ContactFlags.Touching;
            }
            else
            {
                Flags &= ~ContactFlags.Touching;
            }

            if (wasTouching == false && touching)
            {
                //Report the collision to both participants:
                if (FixtureA.OnCollision != null)
                    Enabled = FixtureA.OnCollision(FixtureA, FixtureB, Manifold);

                //Reverse the order of the reported fixtures. The first fixture is always the one that the
                //user subscribed to.
                if (FixtureB.OnCollision != null)
                    Enabled = FixtureB.OnCollision(FixtureB, FixtureA, Manifold);

                //if the user disabled the contact (needed to exclude it in TOI solver), we also need to mark
                //it as not touching.
                if (Enabled == false)
                    Flags &= ~ContactFlags.Touching;

                if (contactManager.BeginContact != null)
                    contactManager.BeginContact(this);
            }

            if (wasTouching && touching == false)
            {
                //Report the separation to both participants:
                if (FixtureA.OnSeparation != null)
                    FixtureA.OnSeparation(FixtureA, FixtureB);

                //Reverse the order of the reported fixtures. The first fixture is always the one that the
                //user subscribed to.
                if (FixtureB.OnSeparation != null)
                    FixtureB.OnSeparation(FixtureB, FixtureA);

                if (contactManager.EndContact != null)
                    contactManager.EndContact(this);
            }

            if (sensor == false)
            {
                if (contactManager.PreSolve != null)
                    contactManager.PreSolve(this, ref oldManifold);
            }
        }

        internal void Create(Fixture fixtureA, Fixture fixtureB)
        {
            ShapeType type1 = fixtureA.Shape.ShapeType;
            ShapeType type2 = fixtureB.Shape.ShapeType;

            Debug.Assert(ShapeType.Unknown < type1 && type1 < ShapeType.TypeCount);
            Debug.Assert(ShapeType.Unknown < type2 && type2 < ShapeType.TypeCount);

            ContactType = DetermineType[(int) type1, (int) type2]();

            if (type1 > type2)
            {
                // primary
                FixtureA = fixtureA;
                FixtureB = fixtureB;
            }
            else
            {
                FixtureA = fixtureB;
                FixtureB = fixtureA;
            }
        }
    }

    public enum ContactType
    {
        CircleContact,
        PolygonAndCircleContact,
        PolygonContact
    }
}