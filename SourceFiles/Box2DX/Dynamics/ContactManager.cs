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
			Shape shape1 = proxyUserData1 as Shape;
			Shape shape2 = proxyUserData2 as Shape;

			Body body1 = shape1.GetBody();
			Body body2 = shape2.GetBody();

			if (body1.IsStatic() && body2.IsStatic())
			{
				return _nullContact;
			}

			if (shape1.GetBody() == shape2.GetBody())
			{
				return _nullContact;
			}

			if (body2.IsConnected(body1))
			{
				return _nullContact;
			}

			if (_world._contactFilter != null && _world._contactFilter.ShouldCollide(shape1, shape2) == false)
			{
				return _nullContact;
			}

			// Call the factory.
			Contact c = Contact.Create(shape1, shape2);

			if (c == null)
			{
				return _nullContact;
			}

			// Contact creation may swap shapes.
			shape1 = c.GetShape1();
			shape2 = c.GetShape2();
			body1 = shape1.GetBody();
			body2 = shape2.GetBody();

			// Insert into the world.
			c._prev = null;
			c._next = _world._contactList;
			if (_world._contactList != null)
			{
				_world._contactList._prev = c;
			}
			_world._contactList = c;

			// Connect to island graph.

			// Connect to body 1
			c._node1.Contact = c;
			c._node1.Other = body2;

			c._node1.Prev = null;
			c._node1.Next = body1._contactList;
			if (body1._contactList != null)
			{
				body1._contactList.Prev = c._node1;
			}
			body1._contactList = c._node1;

			// Connect to body 2
			c._node2.Contact = c;
			c._node2.Other = body1;

			c._node2.Prev = null;
			c._node2.Next = body2._contactList;
			if (body2._contactList != null)
			{
				body2._contactList.Prev = c._node2;
			}
			body2._contactList = c._node2;

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
			Shape shape1 = c.GetShape1();
			Shape shape2 = c.GetShape2();
			Body body1 = shape1.GetBody();
			Body body2 = shape2.GetBody();

			ContactPoint cp = new ContactPoint();
			cp.Shape1 = shape1;
			cp.Shape2 = shape2;
			cp.Friction = Settings.MixFriction(shape1.Friction, shape2.Friction);
			cp.Restitution = Settings.MixRestitution(shape1.Restitution, shape2.Restitution);

			// Inform the user that this contact is ending.
			int manifoldCount = c.GetManifoldCount();
			if (manifoldCount > 0 && _world._contactListener!=null)
			{
				Manifold[] manifolds = c.GetManifolds();

				for (int i = 0; i < manifoldCount; ++i)
				{
					Manifold manifold = manifolds[i];
					cp.Normal = manifold.Normal;

					for (int j = 0; j < manifold.PointCount; ++j)
					{
						ManifoldPoint mp = manifold.Points[j];
						cp.Position = body1.GetWorldPoint(mp.LocalPoint1);
						Vec2 v1 = body1.GetLinearVelocityFromLocalPoint(mp.LocalPoint1);
						Vec2 v2 = body2.GetLinearVelocityFromLocalPoint(mp.LocalPoint2);
						cp.Velocity = v2 - v1;
						cp.Separation = mp.Separation;
						cp.ID = mp.ID;
						_world._contactListener.Remove(cp);
					}
				}
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

			if (c == _world._contactList)
			{
				_world._contactList = c._next;
			}

			// Remove from body 1
			if (c._node1.Prev != null)
			{
				c._node1.Prev.Next = c._node1.Next;
			}

			if (c._node1.Next != null)
			{
				c._node1.Next.Prev = c._node1.Prev;
			}

			if (c._node1 == body1._contactList)
			{
				body1._contactList = c._node1.Next;
			}

			// Remove from body 2
			if (c._node2.Prev != null)
			{
				c._node2.Prev.Next = c._node2.Next;
			}

			if (c._node2.Next != null)
			{
				c._node2.Next.Prev = c._node2.Prev;
			}

			if (c._node2 == body2._contactList)
			{
				body2._contactList = c._node2.Next;
			}

			// Call the factory.
			Contact.Destroy(c);
			--_world._contactCount;
		}

		// This is the top level collision call for the time step. Here
		// all the narrow phase collision is processed for the world
		// contact list.
		public void Collide()
		{
			// Update awake contacts.
			for (Contact c = _world._contactList; c != null; c = c.GetNext())
			{
				Body body1 = c.GetShape1().GetBody();
				Body body2 = c.GetShape2().GetBody();
				if (body1.IsSleeping() && body2.IsSleeping())
				{
					continue;
				}

				c.Update(_world._contactListener);
			}
		}
	}
}
