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

namespace FarseerPhysics
{
    /// A contact edge is used to connect bodies and contacts together
    /// in a contact graph where each body is a node and each contact
    /// is an edge. A contact edge belongs to a doubly linked list
    /// maintained in each attached body. Each contact has two contact
    /// nodes, one for each attached body.
    public class ContactEdge
    {
        /// <summary>
        /// provides quick access to the other body attached.
        /// </summary>
        public Body Other;

        /// <summary>
        /// the contact
        /// </summary>
        public Contact Contact;

        /// <summary>
        /// the previous contact edge in the body's contact list
        /// </summary>
        public ContactEdge Prev;

        /// <summary>
        /// the next contact edge in the body's contact list
        /// </summary>
        public ContactEdge Next;
    }

    [Flags]
    public enum ContactFlags
    {
        None = 0,

        /// <summary>
        /// This contact should not participate in Solve
        /// The contact equivalent of sensors
        /// </summary>
        Sensor = 0x0001,
        /// <summary>
        /// Do not use TOI solve.
        /// </summary>
        Continuous = 0x0002,
        /// <summary>
        /// Used when crawling contact graph when forming islands.
        /// </summary>
        Island = 0x0004,
        /// <summary>
        /// Used in SolveTOI to indicate the cached toi value is still valid.
        /// </summary>
        Toi = 0x0008,
        /// <summary>
        /// TODO
        /// </summary>
        Touching = 0x0010,
        /// <summary>
        /// This contact can be disabled (by user)
        /// </summary>
        Enabled = 0x0020,
        /// <summary>
        /// This contact needs filtering because a fixture filter was changed.
        /// </summary>
        Filter = 0x0040,
    };

    /// <summary>
    /// The class manages contact between two shapes. A contact exists for each overlapping
    /// AABB in the broad-phase (except if filtered). Therefore a contact object may exist
    /// that has no contact points.
    /// </summary>
    public abstract class Contact
    {
        /// <summary>
        /// Get the contact manifold. Do not modify the manifold unless you understand the
        /// internals of Box2D.
        /// </summary>
        /// <param name="manifold">The manifold.</param>
        public void GetManifold(out Manifold manifold)
        {
            manifold = Manifold;
        }

        /// <summary>
        /// Get the world manifold.
        /// </summary>
        /// <param name="worldManifold">The world manifold.</param>
        public void GetWorldManifold(out WorldManifold worldManifold)
        {
            Body bodyA = FixtureA.GetBody();
            Shape shapeA = FixtureA.Shape;
            Shape shapeB = FixtureB.Shape;

            Transform xfA, xfB;
            bodyA.GetTransform(out xfA);
            bodyA.GetTransform(out xfB);

            worldManifold = new WorldManifold(ref Manifold, ref xfA, shapeA.Radius, ref xfB, shapeB.Radius);
        }

        /// <summary>
        /// Is this contact touching.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is touching; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTouching()
        {
            return (Flags & ContactFlags.Touching) == ContactFlags.Touching;
        }

        /// <summary>
        /// Does this contact generate TOI events for continuous simulation?
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is continuous; otherwise, <c>false</c>.
        /// </returns>
        public bool IsContinuous()
        {
            return (Flags & ContactFlags.Continuous) == ContactFlags.Continuous;
        }

        /// <summary>
        /// Change this to be a sensor or non-sensor contact.
        /// </summary>
        /// <param name="sensor">if set to <c>true</c> [sensor].</param>
        public void SetSensor(bool sensor)
        {
            if (sensor)
            {
                Flags |= ContactFlags.Sensor;
            }
            else
            {
                Flags &= ~ContactFlags.Sensor;
            }
        }

        /// <summary>
        /// Is this contact a sensor?
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is sensor; otherwise, <c>false</c>.
        /// </returns>
        public bool IsSensor()
        {
            return (Flags & ContactFlags.Sensor) == ContactFlags.Sensor;
        }

        /// <summary>
        /// Enable/disable this contact. This can be used inside the pre-solve
        /// contact listener. The contact is only disabled for the current
        /// time step (or sub-step in continuous collisions).
        /// </summary>
        /// <param name="flag">if set to <c>true</c> [flag].</param>
        public void SetEnabled(bool flag)
        {
            if (flag)
            {
                Flags |= ContactFlags.Enabled;
            }
            else
            {
                Flags &= ~ContactFlags.Enabled;
            }
        }

        /// <summary>
        /// Has this contact been disabled?
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </returns>
        public bool IsEnabled()
        {
            return (Flags & ContactFlags.Enabled) == ContactFlags.Enabled;
        }

        /// <summary>
        /// Get the next contact in the world's contact list.
        /// </summary>
        /// <returns></returns>
        public Contact GetNext()
        {
            return Next;
        }

        /// <summary>
        /// Get the first fixture in this contact.
        /// </summary>
        /// <returns></returns>
        public Fixture GetFixtureA()
        {
            return FixtureA;
        }

        /// <summary>
        /// Get the second fixture in this contact.
        /// </summary>
        /// <returns></returns>
        public Fixture GetFixtureB()
        {
            return FixtureB;
        }

        /// <summary>
        /// Flag this contact for filtering. Filtering will occur the next time step.
        /// </summary>
        public void FlagForFiltering()
        {
            Flags |= ContactFlags.Filter;
        }

        internal Contact(Fixture fA, Fixture fB)
        {
            Flags = ContactFlags.Enabled;

            if (fA.Sensor || fB.Sensor)
            {
                Flags |= ContactFlags.Sensor;
            }

            Body bodyA = fA.GetBody();
            Body bodyB = fB.GetBody();

            if (bodyA.GetBodyType() != BodyType.Dynamic || bodyA.IsBullet ||
                bodyB.GetBodyType() != BodyType.Dynamic || bodyB.IsBullet)
            {
                Flags |= ContactFlags.Continuous;
            }

            FixtureA = fA;
            FixtureB = fB;

            Manifold._pointCount = 0;

            Prev = null;
            Next = null;

            NodeA.Contact = null;
            NodeA.Prev = null;
            NodeA.Next = null;
            NodeA.Other = null;

            NodeB.Contact = null;
            NodeB.Prev = null;
            NodeB.Next = null;
            NodeB.Other = null;
        }

        internal void Update(ContactManager contactManager)
        {
            Manifold oldManifold = Manifold;

            // Re-enable this contact.
            Flags |= ContactFlags.Enabled;

            bool touching = false;
            bool wasTouching = (Flags & ContactFlags.Touching) == ContactFlags.Touching;

            Body bodyA = FixtureA.GetBody();
            Body bodyB = FixtureB.GetBody();

            bool aabbOverlap = AABB.TestOverlap(ref FixtureA.Aabb, ref FixtureB.Aabb);

            // Is this contact a sensor?
            if ((Flags & ContactFlags.Sensor) == ContactFlags.Sensor)
            {
                if (aabbOverlap)
                {
                    Shape shapeA = FixtureA.Shape;
                    Shape shapeB = FixtureB.Shape;

                    Transform xfA;
                    Transform xfB;
                    bodyA.GetTransform(out xfA);
                    bodyB.GetTransform(out xfB);

                    touching = AABB.TestOverlap(shapeA, shapeB, ref xfA, ref xfB);
                }

                // Sensors don't generate manifolds.
                Manifold._pointCount = 0;
            }
            else
            {
                // Slow contacts don't generate TOI events.
                if (bodyA.GetBodyType() != BodyType.Dynamic || bodyA.IsBullet ||
                    bodyB.GetBodyType() != BodyType.Dynamic || bodyB.IsBullet)
                {
                    Flags |= ContactFlags.Continuous;
                }
                else
                {
                    Flags &= ~ContactFlags.Continuous;
                }

                if (aabbOverlap)
                {
                    Evaluate();
                    touching = Manifold._pointCount > 0;

                    // Match old contact ids to new contact ids and copy the
                    // stored impulses to warm start the solver.
                    for (int i = 0; i < Manifold._pointCount; ++i)
                    {
                        ManifoldPoint mp2 = Manifold._points[i];
                        mp2.NormalImpulse = 0.0f;
                        mp2.TangentImpulse = 0.0f;
                        ContactID id2 = mp2.Id;

                        for (int j = 0; j < oldManifold._pointCount; ++j)
                        {
                            ManifoldPoint mp1 = oldManifold._points[j];

                            if (mp1.Id.Key == id2.Key)
                            {
                                mp2.NormalImpulse = mp1.NormalImpulse;
                                mp2.TangentImpulse = mp1.TangentImpulse;
                                break;
                            }
                        }

                        Manifold._points[i] = mp2;
                    }
                }
                else
                {
                    Manifold._pointCount = 0;
                }

                if (touching != wasTouching)
                {
                    bodyA.SetAwake(true);
                    bodyB.SetAwake(true);
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
                if (contactManager.BeginContact != null)
                    contactManager.BeginContact(this);
            }

            if (wasTouching && touching == false)
            {
                if (contactManager.BeginContact != null)
                    contactManager.EndContact(this);
            }

            if ((Flags & ContactFlags.Sensor) == 0)
            {
                if (contactManager.BeginContact != null)
                    contactManager.PreSolve(this, ref oldManifold);
            }
        }

        protected abstract void Evaluate();

        internal float ComputeTOI(ref Sweep sweepA, ref Sweep sweepB)
        {
            TOIInput input = new TOIInput();
            input.proxyA.Set(FixtureA.Shape);
            input.proxyB.Set(FixtureB.Shape);
            input.sweepA = sweepA;
            input.sweepB = sweepB;
            input.tolerance = Settings.LinearSlop;

            return TimeOfImpact.CalculateTimeOfImpact(ref input);
        }

        private static Func<Fixture, Fixture, Contact>[,] s_registers = new Func<Fixture, Fixture, Contact>[,] 
        {
            { 
              (f1, f2) => new CircleContact(f1, f2), 
              (f1, f2) => new PolygonAndCircleContact(f1, f2)
            },
            { 
              (f1, f2) => new PolygonAndCircleContact(f1, f2), 
              (f1, f2) => new PolygonContact(f1, f2)
            },
        };

        internal static Contact Create(Fixture fixtureA, Fixture fixtureB)
        {
            ShapeType type1 = fixtureA.ShapeType;
            ShapeType type2 = fixtureB.ShapeType;

            Debug.Assert(type1 != ShapeType.Unknown);
            Debug.Assert(type2 != ShapeType.Unknown);

            if (type1 > type2)
            {
                // primary
                return s_registers[(int)type1, (int)type2](fixtureA, fixtureB);
            }

            return s_registers[(int)type1, (int)type2](fixtureB, fixtureA);
        }

        internal ContactFlags Flags;

        // World pool and list pointers.
        internal Contact Prev;
        internal Contact Next;

        // Nodes for connecting bodies.
        internal ContactEdge NodeA = new ContactEdge();
        internal ContactEdge NodeB = new ContactEdge();

        internal Fixture FixtureA;
        internal Fixture FixtureB;

        internal Manifold Manifold;

        internal float Toi;
    }
}
