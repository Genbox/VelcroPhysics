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

namespace FarseerPhysics
{
    public class ContactManager
    {
        /// <summary>
        /// Fires when a contact is deleted
        /// </summary>
        public EndContactDelegate EndContact;
        public BeginContactDelegate BeginContact;
        public PreSolveDelegate PreSolve;
        public PostSolveDelegate PostSolve;

        public CollisionFilterDelegate CollisionFilter;

        internal ContactManager()
        {
            _addPair = AddPair;
        }

        // Broad-phase callback.
        private void AddPair(Fixture proxyUserDataA, Fixture proxyUserDataB)
        {
            Fixture fixtureA = proxyUserDataA;
            Fixture fixtureB = proxyUserDataB;

            Body bodyA = fixtureA.Body;
            Body bodyB = fixtureB.Body;

            // Are the fixtures on the same body?
            if (bodyA == bodyB)
            {
                return;
            }

            // Does a contact already exist?
            ContactEdge edge = bodyB.ContactList;
            while (edge != null)
            {
                if (edge.Other == bodyA)
                {
                    Fixture fA = edge.Contact.FixtureA;
                    Fixture fB = edge.Contact.FixtureB;
                    if (fA == fixtureA && fB == fixtureB)
                    {
                        // A contact already exists.
                        return;
                    }

                    if (fA == fixtureB && fB == fixtureA)
                    {
                        // A contact already exists.
                        return;
                    }
                }

                edge = edge.Next;
            }

            // Does a joint override collision? Is at least one body dynamic?
            if (bodyB.ShouldCollide(bodyA) == false)
            {
                return;
            }

            // Check user filtering.
            if (CollisionFilter != null)
            {
                if (CollisionFilter(fixtureA, fixtureB) == false)
                    return;
            }

            // Call the factory.
            Contact c = Contact.Create(fixtureA, fixtureB);

            // Contact creation may swap fixtures.
            fixtureA = c.FixtureA;
            fixtureB = c.FixtureB;
            bodyA = fixtureA.Body;
            bodyB = fixtureB.Body;

            // Insert into the world.
            c.PrevContact = null;
            c.NextContact = _contactList;
            if (_contactList != null)
            {
                _contactList.PrevContact = c;
            }
            _contactList = c;

            // Connect to island graph.

            // Connect to body A
            c.NodeA.Contact = c;
            c.NodeA.Other = bodyB;

            c.NodeA.Prev = null;
            c.NodeA.Next = bodyA._contactList;
            if (bodyA._contactList != null)
            {
                bodyA._contactList.Prev = c.NodeA;
            }
            bodyA._contactList = c.NodeA;

            // Connect to body B
            c.NodeB.Contact = c;
            c.NodeB.Other = bodyA;

            c.NodeB.Prev = null;
            c.NodeB.Next = bodyB._contactList;
            if (bodyB._contactList != null)
            {
                bodyB._contactList.Prev = c.NodeB;
            }
            bodyB._contactList = c.NodeB;

            ++_contactCount;
        }

        internal void FindNewContacts()
        {
            _broadPhase.UpdatePairs(_addPair);
        }

        internal void Destroy(Contact c)
        {
            Fixture fixtureA = c.FixtureA;
            Fixture fixtureB = c.FixtureB;
            Body bodyA = fixtureA.Body;
            Body bodyB = fixtureB.Body;

            if (c.Manifold._pointCount > 0)
            {
                if (EndContact != null)
                    EndContact(c);
            }

            // Remove from the world.
            if (c.PrevContact != null)
            {
                c.PrevContact.NextContact = c.NextContact;
            }

            if (c.NextContact != null)
            {
                c.NextContact.PrevContact = c.PrevContact;
            }

            if (c == _contactList)
            {
                _contactList = c.NextContact;
            }

            // Remove from body 1
            if (c.NodeA.Prev != null)
            {
                c.NodeA.Prev.Next = c.NodeA.Next;
            }

            if (c.NodeA.Next != null)
            {
                c.NodeA.Next.Prev = c.NodeA.Prev;
            }

            if (c.NodeA == bodyA._contactList)
            {
                bodyA._contactList = c.NodeA.Next;
            }

            // Remove from body 2
            if (c.NodeB.Prev != null)
            {
                c.NodeB.Prev.Next = c.NodeB.Next;
            }

            if (c.NodeB.Next != null)
            {
                c.NodeB.Next.Prev = c.NodeB.Prev;
            }

            if (c.NodeB == bodyB._contactList)
            {
                bodyB._contactList = c.NodeB.Next;
            }

            --_contactCount;
        }

        internal void Collide()
        {
            // Update awake contacts.
            Contact c = _contactList;
            while (c != null)
            {
                Fixture fixtureA = c.FixtureA;
                Fixture fixtureB = c.FixtureB;
                Body bodyA = fixtureA.Body;
                Body bodyB = fixtureB.Body;

                if (bodyA.Awake == false && bodyB.Awake == false)
                {
                    c = c.NextContact;
                    continue;
                }

                // Is this contact flagged for filtering?
                if ((c.Flags & ContactFlags.Filter) == ContactFlags.Filter)
                {
                    // Should these bodies collide?
                    if (bodyB.ShouldCollide(bodyA) == false)
                    {
                        Contact cNuke = c;
                        c = cNuke.NextContact;
                        Destroy(cNuke);
                        continue;
                    }

                    // Check user filtering.
                    if (CollisionFilter != null)
                    {
                        if (CollisionFilter(fixtureA, fixtureB) == false)
                        {
                            Contact cNuke = c;
                            c = cNuke.NextContact;
                            Destroy(cNuke);
                            continue;
                        }
                    }

                    // Clear the filtering flag.
                    c.Flags &= ~ContactFlags.Filter;
                }

                int proxyIdA = fixtureA.ProxyId;
                int proxyIdB = fixtureB.ProxyId;

                bool overlap = _broadPhase.TestOverlap(proxyIdA, proxyIdB);

                // Here we destroy contacts that cease to overlap in the broad-phase.
                if (overlap == false)
                {
                    Contact cNuke = c;
                    c = cNuke.NextContact;
                    Destroy(cNuke);
                    continue;
                }

                // The contact persists.
                c.Update(this);
                c = c.NextContact;
            }
        }

        internal BroadPhase _broadPhase = new BroadPhase();
        internal Contact _contactList;
        internal int _contactCount;

        Action<Fixture, Fixture> _addPair;

        public Contact ContactList
        {
            get { return _contactList; }
        }

        public BroadPhase BroadPhase
        {
            get { return _broadPhase; }
        }
    }
}
