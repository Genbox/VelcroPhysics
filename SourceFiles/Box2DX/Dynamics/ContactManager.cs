/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2007 Erin Catto http://www.gphysics.com

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

using Box2DX.Collision;

namespace Box2DX.Dynamics
{
    /// <summary>
    // Delegate of World.
    /// </summary>
    public class ContactManager
    {
        public BroadPhase _broadPhase;
        public Contact _contactList;
        public int _contactCount;
        public ContactFilter _contactFilter;
        public ContactListener _contactListener;

        public ContactFilter _defaultFilter;
        public ContactListener _defaultListener;

        public ContactManager()
        {
            _broadPhase = new BroadPhase();
            _defaultFilter = new ContactFilter();
            _defaultListener = new ContactListener();

            _contactList = null;
            _contactCount = 0;
            _contactFilter = _defaultFilter;
            _contactListener = _defaultListener;
        }

        public void Destroy(Contact c)
        {
            Fixture fixtureA = c.GetFixtureA();
            Fixture fixtureB = c.GetFixtureB();
            Body body1 = fixtureA.GetBody();
            Body body2 = fixtureB.GetBody();

            if (c._manifold.PointCount > 0)
            {
                _contactListener.EndContact(c);
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

            if (c == _contactList)
            {
                _contactList = c._next;
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

            if (c._nodeA == body1._contactList)
            {
                body1._contactList = c._nodeA.Next;
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

            if (c._nodeB == body2._contactList)
            {
                body2._contactList = c._nodeB.Next;
            }

            // Call the factory.
            Contact.Destroy(c);
            --_contactCount;
        }

        // This is the top level collision call for the time step. Here
        // all the narrow phase collision is processed for the world
        // contact list.
        public void Collide()
        {
            // Update awake contacts.
            Contact c = _contactList;
            while (c != null)
            {
                Fixture fixtureA = c.GetFixtureA();
                Fixture fixtureB = c.GetFixtureB();
                Body bodyA = fixtureA.GetBody();
                Body bodyB = fixtureB.GetBody();

                if (bodyA.IsAwake() == false && bodyB.IsAwake() == false)
                {
                    c = c.GetNext();
                    continue;
                }

                // Is this contact flagged for filtering?
                if ((c._flags & ContactFlag.FilterFlag) == ContactFlag.FilterFlag)
                {
                    //TODO: The following code (next 4 if blocks) use a class and thus copy by ref. It might expect to copy by value
                    // Should these bodies collide?
                    if (bodyB.ShouldCollide(bodyA) == false)
                    {
                        Contact cNuke = c;
                        c = cNuke.GetNext();
                        Destroy(cNuke);
                        continue;
                    }

                    // Check user filtering.
                    if (_contactFilter.ShouldCollide(fixtureA, fixtureB) == false)
                    {
                        Contact cNuke = c;
                        c = cNuke.GetNext();
                        Destroy(cNuke);
                        continue;
                    }

                    // Clear the filtering flag.
                    c._flags &= ~ContactFlag.FilterFlag;
                }

                int proxyIdA = fixtureA._proxyId;
                int proxyIdB = fixtureB._proxyId;
                bool overlap = _broadPhase.TestOverlap(proxyIdA, proxyIdB);

                // Here we destroy contacts that cease to overlap in the broad-phase.
                if (overlap == false)
                {
                    Contact cNuke = c;
                    c = cNuke.GetNext();
                    Destroy(cNuke);
                    continue;
                }

                // The contact persists.
                c.Update(_contactListener);
                c = c.GetNext();
            }
        }

        public void FindNewContacts()
        {
            _broadPhase.UpdatePairs(this);
        }

        public void AddPair(object proxyUserDataA, object proxyUserDataB)
        {
            Fixture fixtureA = (Fixture)proxyUserDataA;
            Fixture fixtureB = (Fixture)proxyUserDataB;

            Body bodyA = fixtureA.GetBody();
            Body bodyB = fixtureB.GetBody();

            // Are the fixtures on the same body?
            if (bodyA == bodyB)
            {
                return;
            }

            // Does a contact already exist?
            ContactEdge edge = bodyB.GetContactList();
            while (edge != null)
            {
                if (edge.Other == bodyA)
                {
                    Fixture fA = edge.Contact.GetFixtureA();
                    Fixture fB = edge.Contact.GetFixtureB();
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
            if (_contactFilter.ShouldCollide(fixtureA, fixtureB) == false)
            {
                return;
            }

            // Call the factory.
            Contact c = Contact.Create(fixtureA, fixtureB);

            // Contact creation may swap fixtures.
            fixtureA = c.GetFixtureA();
            fixtureB = c.GetFixtureB();
            bodyA = fixtureA.GetBody();
            bodyB = fixtureB.GetBody();

            // Insert into the world.
            c._prev = null;
            c._next = _contactList;
            if (_contactList != null)
            {
                _contactList._prev = c;
            }
            _contactList = c;

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

            ++_contactCount;
        }
    }
}
