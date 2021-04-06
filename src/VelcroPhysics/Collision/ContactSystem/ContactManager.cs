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

using System.Collections.Generic;
using VelcroPhysics.Collision.Broadphase;
using VelcroPhysics.Collision.Filtering;
using VelcroPhysics.Collision.Handlers;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Dynamics.Handlers;

namespace VelcroPhysics.Collision.ContactSystem
{
    public class ContactManager
    {
        /// <summary>
        /// Fires when a contact is created
        /// </summary>
        public BeginContactHandler BeginContact;

        /// <summary>
        /// The filter used by the contact manager.
        /// </summary>
        public CollisionFilterHandler ContactFilter;

        /// <summary>
        /// Fires when a contact is deleted
        /// </summary>
        public EndContactHandler EndContact;

        /// <summary>
        /// Fires when the broadphase detects that two Fixtures are close to each other.
        /// </summary>
        public BroadphaseHandler OnBroadphaseCollision;

        /// <summary>
        /// Fires after the solver has run
        /// </summary>
        public PostSolveHandler PostSolve;

        /// <summary>
        /// Fires before the solver runs
        /// </summary>
        public PreSolveHandler PreSolve;

        internal ContactManager(IBroadPhase broadPhase)
        {
            BroadPhase = broadPhase;
            OnBroadphaseCollision = AddPair;
            ContactList = new List<Contact>(128);
        }

        public List<Contact> ContactList { get; }
        public IBroadPhase BroadPhase { get; }

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
                return;

            //Check default filter
            if (ShouldCollide(fixtureA, fixtureB) == false)
                return;

            // Check user filtering.
            if (ContactFilter != null && ContactFilter(fixtureA, fixtureB) == false)
                return;

            //Velcro: BeforeCollision delegate
            if (fixtureA.BeforeCollision != null && fixtureA.BeforeCollision(fixtureA, fixtureB) == false)
                return;

            if (fixtureB.BeforeCollision != null && fixtureB.BeforeCollision(fixtureB, fixtureA) == false)
                return;

            // Call the factory.
            Contact c = Contact.Create(fixtureA, indexA, fixtureB, indexB);

            if (c == null)
                return;

            // Contact creation may swap fixtures.
            fixtureA = c.FixtureA;
            fixtureB = c.FixtureB;
            bodyA = fixtureA.Body;
            bodyB = fixtureB.Body;

            // Insert into the world.
            ContactList.Add(c);

            // Connect to island graph.

            // Connect to body A
            c._nodeA.Contact = c;
            c._nodeA.Other = bodyB;

            c._nodeA.Prev = null;
            c._nodeA.Next = bodyA.ContactList;
            if (bodyA.ContactList != null)
            {
                bodyA.ContactList.Prev = c._nodeA;
            }
            bodyA.ContactList = c._nodeA;

            // Connect to body B
            c._nodeB.Contact = c;
            c._nodeB.Other = bodyA;

            c._nodeB.Prev = null;
            c._nodeB.Next = bodyB.ContactList;
            if (bodyB.ContactList != null)
            {
                bodyB.ContactList.Prev = c._nodeB;
            }
            bodyB.ContactList = c._nodeB;

            // Wake up the bodies
            if (fixtureA.IsSensor == false && fixtureB.IsSensor == false)
            {
                bodyA.Awake = true;
                bodyB.Awake = true;
            }
        }

        internal void FindNewContacts()
        {
            BroadPhase.UpdatePairs(OnBroadphaseCollision);
        }

        internal void Destroy(Contact contact)
        {
            if (contact.FixtureA == null || contact.FixtureB == null)
                return;

            Fixture fixtureA = contact.FixtureA;
            Fixture fixtureB = contact.FixtureB;

            //Velcro: When contacts are removed, we invoke OnSeparation
            if (contact.IsTouching)
            {
                //Report the separation to both participants:
                fixtureA.OnSeparation?.Invoke(fixtureA, fixtureB, contact);

                //Reverse the order of the reported fixtures. The first fixture is always the one that the user subscribed to.
                fixtureB.OnSeparation?.Invoke(fixtureB, fixtureA, contact);

                //The generic handler
                EndContact?.Invoke(contact);
            }

            Body bodyA = fixtureA.Body;
            Body bodyB = fixtureB.Body;

            // Remove from the world.
            ContactList.Remove(contact);

            // Remove from body 1
            if (contact._nodeA.Prev != null)
                contact._nodeA.Prev.Next = contact._nodeA.Next;

            if (contact._nodeA.Next != null)
                contact._nodeA.Next.Prev = contact._nodeA.Prev;

            if (contact._nodeA == bodyA.ContactList)
                bodyA.ContactList = contact._nodeA.Next;

            // Remove from body 2
            if (contact._nodeB.Prev != null)
                contact._nodeB.Prev.Next = contact._nodeB.Next;

            if (contact._nodeB.Next != null)
                contact._nodeB.Next.Prev = contact._nodeB.Prev;

            if (contact._nodeB == bodyB.ContactList)
                bodyB.ContactList = contact._nodeB.Next;

            contact.Destroy();
        }

        internal void Collide()
        {
            // Update awake contacts.
            for (int i = 0; i < ContactList.Count; i++)
            {
                Contact c = ContactList[i];
                Fixture fixtureA = c.FixtureA;
                Fixture fixtureB = c.FixtureB;
                int indexA = c.ChildIndexA;
                int indexB = c.ChildIndexB;
                Body bodyA = fixtureA.Body;
                Body bodyB = fixtureB.Body;

                //Velcro: Do no try to collide disabled bodies
                if (!bodyA.Enabled || !bodyB.Enabled)
                    continue;

                // Is this contact flagged for filtering?
                if (c.FilterFlag)
                {
                    // Should these bodies collide?
                    if (bodyB.ShouldCollide(bodyA) == false)
                    {
                        Contact cNuke = c;
                        Destroy(cNuke);
                        continue;
                    }

                    // Check default filtering
                    if (ShouldCollide(fixtureA, fixtureB) == false)
                    {
                        Contact cNuke = c;
                        Destroy(cNuke);
                        continue;
                    }

                    // Check user filtering.
                    if (ContactFilter != null && ContactFilter(fixtureA, fixtureB) == false)
                    {
                        Contact cNuke = c;
                        Destroy(cNuke);
                        continue;
                    }

                    // Clear the filtering flag.
                    c._flags &= ~ContactFlags.FilterFlag;
                }

                bool activeA = bodyA.Awake && bodyA.BodyType != BodyType.Static;
                bool activeB = bodyB.Awake && bodyB.BodyType != BodyType.Static;

                // At least one body must be awake and it must be dynamic or kinematic.
                if (activeA == false && activeB == false)
                {
                    continue;
                }

                int proxyIdA = fixtureA.Proxies[indexA].ProxyId;
                int proxyIdB = fixtureB.Proxies[indexB].ProxyId;
                bool overlap = BroadPhase.TestOverlap(proxyIdA, proxyIdB);

                // Here we destroy contacts that cease to overlap in the broad-phase.
                if (overlap == false)
                {
                    Contact cNuke = c;
                    Destroy(cNuke);
                    continue;
                }

                // The contact persists.
                c.Update(this);
            }
        }

        private static bool ShouldCollide(Fixture fixtureA, Fixture fixtureB)
        {
            if (Settings.UseFPECollisionCategories)
            {
                if ((fixtureA.CollisionGroup == fixtureB.CollisionGroup) &&
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
            {
                return fixtureA.CollisionGroup > 0;
            }

            bool collide = (fixtureA.CollidesWith & fixtureB.CollisionCategories) != 0 &&
                           (fixtureA.CollisionCategories & fixtureB.CollidesWith) != 0;

            return collide;
        }
    }
}