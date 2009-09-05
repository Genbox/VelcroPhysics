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

            if (c.Manifold.PointCount > 0)
            {
                _contactListener.EndContact(c);
            }

            // Remove from the world.
            if (c.Prev != null)
            {
                c.Prev.Next = c.Next;
            }

            if (c.Next != null)
            {
                c.Next.Prev = c.Prev;
            }

            if (c == _contactList)
            {
                _contactList = c.Next;
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

            if (c.NodeA == body1._contactList)
            {
                body1._contactList = c.NodeA.Next;
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

            if (c.NodeB == body2._contactList)
            {
                body2._contactList = c.NodeB.Next;
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

                if (bodyA.IsSleeping() && bodyB.IsSleeping())
                {
                    c = c.GetNext();
                    continue;
                }

                // Is this contact flagged for filtering?
                if ((c.Flags & ContactFlag.FilterFlag) == ContactFlag.FilterFlag)
                {
                    //TODO: The following code (next 4 if blocks) use a class and thus copy by ref. It might expect to copy by value
                    // Are both bodies static?
                    if (bodyA.IsStatic() && bodyB.IsStatic())
                    {
                        Contact cNuke = c;
                        c = cNuke.GetNext();
                        Destroy(cNuke);
                        continue;
                    }

                    // Does a joint override collision?
                    if (bodyB.IsConnected(bodyA))
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
                    c.Flags &= ~ContactFlag.FilterFlag;
                }

                int proxyIdA = fixtureA.ProxyId;
                int proxyIdB = fixtureB.ProxyId;
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

            // Are both bodies static?
            if (bodyA.IsStatic() && bodyB.IsStatic())
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

            // Does a joint override collision?
            if (bodyB.IsConnected(bodyA))
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
            c.Prev = null;
            c.Next = _contactList;
            if (_contactList != null)
            {
                _contactList.Prev = c;
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
    }
}
