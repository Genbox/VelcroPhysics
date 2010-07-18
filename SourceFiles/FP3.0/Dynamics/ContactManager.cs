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
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics.Contacts;

namespace FarseerPhysics.Dynamics
{
    public class ContactManager
    {
        public BeginContactDelegate BeginContact;

        public Action<FixtureProxy, FixtureProxy> BroadphaseCollision;
        public CollisionFilterDelegate ContactFilter;

        /// <summary>
        /// Fires when a contact is deleted
        /// </summary>
        public EndContactDelegate EndContact;

        public PostSolveDelegate PostSolve;
        public PreSolveDelegate PreSolve;

        internal ContactManager()
        {
            _addPair = AddPair;

            ContactList = null;
            ContactCount = 0;

            BroadphaseCollision = AddPair;
        }

        // Broad-phase callback.
        internal void AddPair(FixtureProxy proxyA, FixtureProxy proxyB)
        {
            Fixture fixtureA = proxyA.Fixture;
            Fixture fixtureB = proxyB.Fixture;

            int indexA = proxyA.ChildIndex;
            int indexB = proxyB.ChildIndex;

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
                    int iA = edge.Contact.ChildIndexA;
                    int iB = edge.Contact.ChildIndexB;

                    if (fA == fixtureA && fB == fixtureB && iA == indexA && iB == indexB)
                    {
                        // A contact already exists.
                        return;
                    }

                    if (fA == fixtureB && fB == fixtureA && iA == indexB && iB == indexA)
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
            if (ContactFilter != null && ContactFilter(fixtureA, fixtureB) == false)
            {
                return;
            }

            // Call the factory.
            Contact c = Contact.Create(fixtureA, indexA, fixtureB, indexB);

            // Contact creation may swap fixtures.
            fixtureA = c.FixtureA;
            fixtureB = c.FixtureB;
            indexA = c.ChildIndexA;
            indexB = c.ChildIndexB;
            bodyA = fixtureA.Body;
            bodyB = fixtureB.Body;

            // Insert into the world.
            c._prev = null;
            c._next = ContactList;
            if (ContactList != null)
            {
                ContactList._prev = c;
            }
            ContactList = c;

            // Connect to island graph.

            // Connect to body A
            c._nodeA.Contact = c;
            c._nodeA.Other = bodyB;

            c._nodeA.Prev = null;
            c._nodeA.Next = bodyA._contactList;
            if (bodyA._contactList != null)
            {
                bodyA._contactList.Prev = c._nodeA;
            }
            bodyA._contactList = c._nodeA;

            // Connect to body B
            c._nodeB.Contact = c;
            c._nodeB.Other = bodyA;

            c._nodeB.Prev = null;
            c._nodeB.Next = bodyB._contactList;
            if (bodyB._contactList != null)
            {
                bodyB._contactList.Prev = c._nodeB;
            }
            bodyB._contactList = c._nodeB;

            ++ContactCount;
        }

        internal void FindNewContacts()
        {
            BroadPhase.UpdatePairs<FixtureProxy>(_addPair);
        }

        internal void Destroy(Contact c)
        {
            Fixture fixtureA = c.FixtureA;
            Fixture fixtureB = c.FixtureB;
            Body bodyA = fixtureA.Body;
            Body bodyB = fixtureB.Body;

            if (EndContact != null && c.IsTouching())
            {
                EndContact(c);
            }

            // Remove from the world.
            if (c._prev != null)
            {
                c._prev._next = c._next;
            }

            if (c._next != null)
            {
                c._next._prev = c._prev;
            }

            if (c == ContactList)
            {
                ContactList = c._next;
            }

            // Remove from body 1
            if (c._nodeA.Prev != null)
            {
                c._nodeA.Prev.Next = c._nodeA.Next;
            }

            if (c._nodeA.Next != null)
            {
                c._nodeA.Next.Prev = c._nodeA.Prev;
            }

            if (c._nodeA == bodyA._contactList)
            {
                bodyA._contactList = c._nodeA.Next;
            }

            // Remove from body 2
            if (c._nodeB.Prev != null)
            {
                c._nodeB.Prev.Next = c._nodeB.Next;
            }

            if (c._nodeB.Next != null)
            {
                c._nodeB.Next.Prev = c._nodeB.Prev;
            }

            if (c._nodeB == bodyB._contactList)
            {
                bodyB._contactList = c._nodeB.Next;
            }

            c.Destroy();

            --ContactCount;
        }

        internal void Collide()
        {
            // Update awake contacts.
            Contact c = ContactList;
            while (c != null)
            {
                Fixture fixtureA = c.FixtureA;
                Fixture fixtureB = c.FixtureB;
                int indexA = c.ChildIndexA;
                int indexB = c.ChildIndexB;
                Body bodyA = fixtureA.Body;
                Body bodyB = fixtureB.Body;

                if (bodyA.Awake == false && bodyB.Awake == false)
                {
                    c = c.GetNext();
                    continue;
                }

                // Is this contact flagged for filtering?
                if ((c._flags & ContactFlags.Filter) == ContactFlags.Filter)
                {
                    // Should these bodies collide?
                    if (bodyB.ShouldCollide(bodyA) == false)
                    {
                        Contact cNuke = c;
                        c = cNuke.GetNext();
                        Destroy(cNuke);
                        continue;
                    }

                    // Check user filtering.
                    if (ContactFilter != null && ContactFilter(fixtureA, fixtureB) == false)
                    {
                        Contact cNuke = c;
                        c = cNuke.GetNext();
                        Destroy(cNuke);
                        continue;
                    }

                    // Clear the filtering flag.
                    c._flags &= ~ContactFlags.Filter;
                }

                int proxyIdA = fixtureA._proxies[indexA].ProxyId;
                int proxyIdB = fixtureB._proxies[indexB].ProxyId;

                bool overlap = BroadPhase.TestOverlap(proxyIdA, proxyIdB);

                // Here we destroy contacts that cease to overlap in the broad-phase.
                if (overlap == false)
                {
                    Contact cNuke = c;
                    c = cNuke.GetNext();
                    Destroy(cNuke);
                    continue;
                }

                // The contact persists.
                c.Update(this);
                c = c.GetNext();
            }
        }

        public BroadPhase BroadPhase = new BroadPhase();
        public Contact ContactList;
        public int ContactCount;

        Action<FixtureProxy, FixtureProxy> _addPair;
    }
}
