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

using Genbox.VelcroPhysics.Collision.Broadphase;
using Genbox.VelcroPhysics.Collision.Filtering;
using Genbox.VelcroPhysics.Collision.Handlers;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Handlers;

namespace Genbox.VelcroPhysics.Collision.ContactSystem
{
    public class ContactManager
    {
        internal int _contactCount;
        internal Contact _contactList;

        /// <summary>Fires when a contact is created</summary>
        public BeginContactHandler BeginContact;

        /// <summary>The filter used by the contact manager.</summary>
        public CollisionFilterHandler ContactFilter;

        /// <summary>Fires when a contact is deleted</summary>
        public EndContactHandler EndContact;

        /// <summary>Fires when the broadphase detects that two Fixtures are close to each other.</summary>
        public BroadphaseHandler OnBroadphaseCollision;

        /// <summary>Fires after the solver has run</summary>
        public PostSolveHandler PostSolve;

        /// <summary>Fires before the solver runs</summary>
        public PreSolveHandler PreSolve;

        internal ContactManager(IBroadPhase broadPhase)
        {
            BroadPhase = broadPhase;
            OnBroadphaseCollision = AddPair;
        }

        public IBroadPhase BroadPhase { get; }

        public int ContactCount => _contactCount;

        // Broad-phase callback.
        private void AddPair(ref FixtureProxy proxyA, ref FixtureProxy proxyB)
        {
            Fixture fixtureA = proxyA.Fixture;
            Fixture fixtureB = proxyB.Fixture;

            int indexA = proxyA.ChildIndex;
            int indexB = proxyB.ChildIndex;

            Body bodyA = fixtureA.Body;
            Body bodyB = fixtureB.Body;

            // Are the fixtures on the same body?
            if (bodyA == bodyB)
                return;

            // TODO_ERIN use a hash table to remove a potential bottleneck when both
            // bodies have a lot of contacts.
            // Does a contact already exist?
            ContactEdge edge = bodyB._contactList;
            while (edge != null)
            {
                if (edge.Other == bodyA)
                {
                    Fixture fA = edge.Contact._fixtureA;
                    Fixture fB = edge.Contact._fixtureB;
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
            if (!bodyB.ShouldCollide(bodyA))
                return;

            //Check default filter
            if (!ShouldCollide(fixtureA, fixtureB))
                return;

            // Check user filtering.
            if (ContactFilter != null && !ContactFilter(fixtureA, fixtureB))
                return;

            //Velcro: BeforeCollision delegate
            if (fixtureA.BeforeCollision != null && !fixtureA.BeforeCollision(fixtureA, fixtureB))
                return;

            if (fixtureB.BeforeCollision != null && !fixtureB.BeforeCollision(fixtureB, fixtureA))
                return;

            // Call the factory.
            Contact c = Contact.Create(fixtureA, indexA, fixtureB, indexB);
            if (c == null)
                return;

            // Contact creation may swap fixtures.
            fixtureA = c._fixtureA;
            fixtureB = c._fixtureB;
            indexA = c.ChildIndexA;
            indexB = c.ChildIndexB;
            bodyA = fixtureA.Body;
            bodyB = fixtureB.Body;

            // Insert into the world.
            c._prev = null;
            c._next = _contactList;
            if (_contactList != null)
                _contactList._prev = c;
            _contactList = c;

            // Connect to island graph.

            // Connect to body A
            c._nodeA.Contact = c;
            c._nodeA.Other = bodyB;

            c._nodeA.Prev = null;
            c._nodeA.Next = bodyA._contactList;
            if (bodyA._contactList != null)
                bodyA._contactList.Prev = c._nodeA;
            bodyA._contactList = c._nodeA;

            // Connect to body B
            c._nodeB.Contact = c;
            c._nodeB.Other = bodyA;

            c._nodeB.Prev = null;
            c._nodeB.Next = bodyB._contactList;
            if (bodyB._contactList != null)
                bodyB._contactList.Prev = c._nodeB;

            bodyB._contactList = c._nodeB;
            ++_contactCount;
        }

        internal void FindNewContacts()
        {
            BroadPhase.UpdatePairs(OnBroadphaseCollision);
        }

        internal void Remove(Contact c)
        {
            if (c._fixtureA == null || c._fixtureB == null)
                return;

            Fixture fixtureA = c._fixtureA;
            Fixture fixtureB = c._fixtureB;

            //Velcro: When contacts are removed, we invoke OnSeparation
            if (c.IsTouching)
            {
                //Report the separation to both participants:
                fixtureA.OnSeparation?.Invoke(fixtureA, fixtureB, c);

                //Reverse the order of the reported fixtures. The first fixture is always the one that the user subscribed to.
                fixtureB.OnSeparation?.Invoke(fixtureB, fixtureA, c);

                //The generic handler
                EndContact?.Invoke(c);
            }

            Body bodyA = fixtureA._body;
            Body bodyB = fixtureB._body;

            // Remove from the world.
            if (c._prev != null)
                c._prev._next = c._next;

            if (c._next != null)
                c._next._prev = c._prev;

            if (c == _contactList)
                _contactList = c._next;

            // Remove from body 1
            if (c._nodeA.Prev != null)
                c._nodeA.Prev.Next = c._nodeA.Next;

            if (c._nodeA.Next != null)
                c._nodeA.Next.Prev = c._nodeA.Prev;

            if (c._nodeA == bodyA._contactList)
                bodyA._contactList = c._nodeA.Next;

            // Remove from body 2
            if (c._nodeB.Prev != null)
                c._nodeB.Prev.Next = c._nodeB.Next;

            if (c._nodeB.Next != null)
                c._nodeB.Next.Prev = c._nodeB.Prev;

            if (c._nodeB == bodyB._contactList)
                bodyB._contactList = c._nodeB.Next;

            // Call the factory.
            c.Destroy();
            --_contactCount;
        }

        /// <summary>
        /// This is the top level collision call for the time step. Here all the narrow phase collision is processed for the world contact list.
        /// </summary>
        internal void Collide()
        {
            // Update awake contacts.

            Contact c = _contactList;

            while (c != null)
            {
                Fixture fixtureA = c._fixtureA;
                Fixture fixtureB = c._fixtureB;
                int indexA = c.ChildIndexA;
                int indexB = c.ChildIndexB;
                Body bodyA = fixtureA._body;
                Body bodyB = fixtureB._body;

                //Velcro: Do no try to collide disabled bodies
                if (!bodyA.Enabled || !bodyB.Enabled)
                {
                    c = c._next;
                    continue;
                }

                // Is this contact flagged for filtering?
                if (c.FilterFlag)
                {
                    // Should these bodies collide?
                    if (!bodyB.ShouldCollide(bodyA))
                    {
                        Contact cNuke = c;
                        c = cNuke._next;
                        Remove(cNuke);
                        continue;
                    }

                    // Check default filtering
                    if (!ShouldCollide(fixtureA, fixtureB))
                    {
                        Contact cNuke = c;
                        c = cNuke._next;
                        Remove(cNuke);
                        continue;
                    }

                    // Check user filtering.
                    if (ContactFilter != null && !ContactFilter(fixtureA, fixtureB))
                    {
                        Contact cNuke = c;
                        c = cNuke._next;
                        Remove(cNuke);
                        continue;
                    }

                    // Clear the filtering flag.
                    c._flags &= ~ContactFlags.FilterFlag;
                }

                bool activeA = bodyA.Awake && bodyA.BodyType != BodyType.Static;
                bool activeB = bodyB.Awake && bodyB.BodyType != BodyType.Static;

                // At least one body must be awake and it must be dynamic or kinematic.
                if (!activeA && !activeB)
                {
                    c = c._next;
                    continue;
                }

                int proxyIdA = fixtureA.Proxies[indexA].ProxyId;
                int proxyIdB = fixtureB.Proxies[indexB].ProxyId;
                bool overlap = BroadPhase.TestOverlap(proxyIdA, proxyIdB);

                // Here we destroy contacts that cease to overlap in the broad-phase.
                if (!overlap)
                {
                    Contact cNuke = c;
                    c = cNuke._next;
                    Remove(cNuke);
                    continue;
                }

                // The contact persists.
                c.Update(this);
                c = c._next;
            }
        }

        private static bool ShouldCollide(Fixture fixtureA, Fixture fixtureB)
        {
            if (Settings.UseFPECollisionCategories)
            {
                if (fixtureA.CollisionGroup == fixtureB.CollisionGroup &&
                    fixtureA.CollisionGroup != 0 && fixtureB.CollisionGroup != 0)
                    return false;

                if (((fixtureA.CollisionCategories & fixtureB.CollidesWith) ==
                     Category.None) &
                    ((fixtureB.CollisionCategories & fixtureA.CollidesWith) ==
                     Category.None))
                    return false;

                return true;
            }

            if (fixtureA.CollisionGroup == fixtureB.CollisionGroup &&
                fixtureA.CollisionGroup != 0)
                return fixtureA.CollisionGroup > 0;

            bool collide = (fixtureA.CollidesWith & fixtureB.CollisionCategories) != 0 &&
                           (fixtureA.CollisionCategories & fixtureB.CollidesWith) != 0;

            return collide;
        }
    }
}