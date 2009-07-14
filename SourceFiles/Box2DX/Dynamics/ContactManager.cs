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

using System;
using System.Collections.Generic;
using System.Text;

using Box2DX.Common;
using Box2DX.Collision;

namespace Box2DX.Dynamics
{
	/// <summary>
	// Delegate of World.
	/// </summary>
	public class ContactManager : PairCallback
	{
		public World _world;

		// This lets us provide broadphase proxy pair user data for
		// contacts that shouldn't exist.
		public NullContact _nullContact;

        public Contact _nextContact;

		public bool _destroyImmediate;

		public ContactManager()
		{
			_world = null;
			_destroyImmediate = false;
		}

		// This is a callback from the broadphase when two AABB proxies begin
		// to overlap. We create a Contact to manage the narrow phase.
		public override object PairAdded(object proxyUserData1, object proxyUserData2)
		{
			Fixture fixtureA = proxyUserData1 as Fixture;
			Fixture fixtureB = proxyUserData2 as Fixture;

			Body body1 = fixtureA.GetBody();
			Body body2 = fixtureB.GetBody();

			if (body1.IsStatic() && body2.IsStatic())
			{
				return _nullContact;
			}

			if (fixtureA.GetBody() == fixtureB.GetBody())
			{
				return _nullContact;
			}

			if (body2.IsConnected(body1))
			{
				return _nullContact;
			}

			if (_world._contactFilter != null && _world._contactFilter.ShouldCollide(fixtureA, fixtureB) == false)
			{
				return _nullContact;
			}

			// Call the factory.
			Contact c = Contact.Create(fixtureA, fixtureB);

			if (c == null)
			{
				return _nullContact;
			}

			// Contact creation may swap shapes.
			fixtureA = c.GetFixtureA();
			fixtureB = c.GetFixtureB();
			body1 = fixtureA.GetBody();
			body2 = fixtureB.GetBody();

			// Insert into the world.
			c.Prev = null;
			c.Next = _world._contactList;
			if (_world._contactList != null)
			{
				_world._contactList.Prev = c;
			}
			_world._contactList = c;

			// Connect to island graph.

			// Connect to body 1
			c.NodeA.Contact = c;
			c.NodeA.Other = body2;

            c.NodeA.Prev = null;
            c.NodeA.Next = body1._contactList;
			if (body1._contactList != null)
			{
                body1._contactList.Prev = c.NodeA;
			}
            body1._contactList = c.NodeA;

			// Connect to body 2
            c.NodeB.Contact = c;
            c.NodeB.Other = body1;

            c.NodeB.Prev = null;
            c.NodeB.Next = body2._contactList;
			if (body2._contactList != null)
			{
                body2._contactList.Prev = c.NodeB;
			}
            body2._contactList = c.NodeB;

			++_world._contactCount;
			return c;
		}

		// This is a callback from the broadphase when two AABB proxies cease
		// to overlap. We retire the Contact.
		public override void PairRemoved(object proxyUserData1, object proxyUserData2, object pairUserData)
		{
			//B2_NOT_USED(proxyUserData1);
			//B2_NOT_USED(proxyUserData2);

			if (pairUserData == null)
			{
				return;
			}

			Contact c = pairUserData as Contact;
			if (c == _nullContact)
			{
				return;
			}

			// An attached body is being destroyed, we must destroy this contact
			// immediately to avoid orphaned shape pointers.
			Destroy(c);
		}

		public void Destroy(Contact c)
		{
			Fixture fixtureA = c.GetFixtureA();
			Fixture fixtureB = c.GetFixtureB();
			Body body1 = fixtureA.GetBody();
			Body body2 = fixtureB.GetBody();

            if (c.Manifold.PointCount > 0)
            {
                _world._contactListener.EndContact(c);
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

			if (c == _world._contactList)
			{
				_world._contactList = c.Next;
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

            if (_nextContact == c)
            {
                _nextContact = c.GetNext();
            }
			// Call the factory.
            if ((c.Flags & ContactFlag.LockedFlag) != 0)
	        {
		        // We cannot destroy the current contact - it's being worked on.
		        // Instead mark it for deferred destruction.
		        // Collide() will handle calling Destroy slightly later
		        c.Flags |= ContactFlag.DestroyFlag;

		        // Also do some cleaning up so that people don't accidentally do stupid things.
                // TODO: Is this necessary or wise?
		        //c->m_fixtureA = NULL;
		        //c->m_fixtureB = NULL;
		        c.Next = null;
		        c.Prev = null;
	        } else {
			    Contact.Destroy(c);
            }
			--_world._contactCount;
		}

		// This is the top level collision call for the time step. Here
		// all the narrow phase collision is processed for the world
		// contact list.
		public void Collide()
		{
			// Update awake contacts.
            // Note the use of a accessible iterator, m_nextContact, this can be updated elsewhere
            // should that contact get deleted inside the call to m_nextContact
            _nextContact = _world._contactList;
			while(_nextContact != null)
			{
                Contact c = _nextContact;
                _nextContact = _nextContact.Next;
				Body body1 = c.GetFixtureA().GetBody();
				Body body2 = c.GetFixtureB().GetBody();
				if (body1.IsSleeping() && body2.IsSleeping())
				{
					continue;
				}

				Update(c);
			}
            _nextContact = null;
		}

        public bool Update(Contact contact)
        {
            ContactListener listener = _world._contactListener;

            Body bodyA = contact._fixtureA.GetBody();
            Body bodyB = contact._fixtureB.GetBody();

            ShapeType shapeAType = contact._fixtureA.GetShapeType();
            ShapeType shapeBType = contact._fixtureB.GetShapeType();

            Manifold oldManifold = contact.Manifold;

            uint oldLock = (uint)(contact.Flags & ContactFlag.LockedFlag);

            contact.Flags |= ContactFlag.LockedFlag;

            contact.Evaluate();

            contact.Flags &= ~ContactFlag.LockedFlag;

            if ((contact.Flags & ContactFlag.DestroyFlag) != 0)
            {
                Contact.Destroy(contact, shapeAType, shapeBType);
                return true;
            }

            if (!(oldLock > 0))
                contact.Flags &= ~ContactFlag.LockedFlag;

            int oldCount = oldManifold.PointCount;
            int newCount = contact.Manifold.PointCount;

            if (newCount == 0 && oldCount > 0)
            {
                bodyA.WakeUp();
                bodyB.WakeUp();
            }

            // Slow contacts don't generate TOI events.
            if (bodyA.IsStatic() || bodyA.IsBullet() || bodyB.IsStatic() || bodyB.IsBullet())
            {
                contact.Flags &= ~ContactFlag.SlowFlag;
            }
            else
            {
                contact.Flags |= ContactFlag.SlowFlag;
            }

            // Match old contact ids to new contact ids and copy the
            // stored impulses to warm start the solver.
            for (int i = 0; i < contact.Manifold.PointCount; ++i)
            {
                ManifoldPoint mp2 = contact.Manifold.Points[i];
                mp2.NormalImpulse = 0.0f;
                mp2.TangentImpulse = 0.0f;
                ContactID id2 = mp2.ID;

                for (int j = 0; j < oldManifold.PointCount; ++j)
                {
                    ManifoldPoint mp1 = oldManifold.Points[j];

                    if (mp1.ID.Key == id2.Key)
                    {
                        mp2.NormalImpulse = mp1.NormalImpulse;
                        mp2.TangentImpulse = mp1.TangentImpulse;
                        break;
                    }
                }
            }

            if (oldCount == 0 && newCount > 0)
            {
                contact.Flags |= ContactFlag.TouchFlag;
                listener.BeginContact(contact);
            }

            if (oldCount > 0 && newCount == 0)
            {
                contact.Flags &= ~ContactFlag.TouchFlag;
                listener.EndContact(contact);
            }

            if ((contact.Flags & ContactFlag.NonSolidFlag) == 0)
            {
                listener.PreSolve(contact, oldManifold);

                // The user may have disabled contact.
                if (contact.Manifold.PointCount == 0)
                {
                    contact.Flags &= ~ContactFlag.TouchFlag;
                }
            }
            return false;
        }
	}
}
