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

using Box2DX.Collision;
using Box2DX.Common;

namespace Box2DX.Dynamics
{
	public delegate Contact ContactCreateFcn(Fixture fixtureA, Fixture fixtureB);
	public delegate void ContactDestroyFcn(Contact contact);

	public struct ContactRegister
	{
		public ContactCreateFcn CreateFcn;
		public ContactDestroyFcn DestroyFcn;
		public bool Primary;
	}

#warning "CAS"
	/// <summary>
	/// A contact edge is used to connect bodies and contacts together
	/// in a contact graph where each body is a node and each contact
	/// is an edge. A contact edge belongs to a doubly linked list
	/// maintained in each attached body. Each contact has two contact
	/// nodes, one for each attached body.
	/// </summary>
	public class ContactEdge
	{
		/// <summary>
		/// Provides quick access to the other body attached.
		/// </summary>
		public Body Other;
		/// <summary>
		/// The contact.
		/// </summary>
		public Contact Contact;
		/// <summary>
		/// The previous contact edge in the body's contact list.
		/// </summary>
		public ContactEdge Prev;
		/// <summary>
		/// The next contact edge in the body's contact list.
		/// </summary>
		public ContactEdge Next;
	}

    enum ContactFlag
	{
		    NonSolidFlag	= 0x0001,
		    SlowFlag		= 0x0002,
		    IslandFlag	    = 0x0004,
		    ToiFlag		    = 0x0008,
		    TouchFlag		= 0x0010,
    }

#warning "CAS"
	/// <summary>
	/// This structure is used to report contact points.
	/// </summary>
	public class Contact
	{
		static ContactRegister[,] _registers = new ContactRegister[(int)ShapeType.ShapeTypeCount, (int)ShapeType.ShapeTypeCount];
	    static bool _initialized;

	    ContactFlag _flags;

	    // World pool and list pointers.
	    Contact _prev;
	    Contact _next;

	    // Nodes for connecting bodies.
	    ContactEdge _nodeA;
	    ContactEdge _nodeB;

	    Fixture _fixtureA;
	    Fixture _fixtureB;

	    Manifold _manifold;

	    float _toi;

        /// Get the contact manifold.
	    public Manifold GetManifold() { return _manifold; }

	    /// Get the world manifold.
	    public void GetWorldManifold(WorldManifold worldManifold)
        {

        }

	    /// Is this contact solid?
	    /// @return true if this contact should generate a response.
	    public bool IsSolid() { return (_flags & ContactFlag.NonSolidFlag) == 0; }

	    /// Are fixtures touching?
	    public bool AreTouching() { return (_flags & ContactFlag.TouchFlag) == 0; }

	    /// Get the next contact in the world's contact list.
	    public Contact GetNext() { return _next; }

	    /// Get the first fixture in this contact.
	    public Fixture GetFixtureA() { return _fixtureA; }

	    /// Get the second fixture in this contact.
	    public Fixture GetFixtureB() { return _fixtureB; }
	    
	    public static void AddType(ContactCreateFcn createFcn, ContactDestroyFcn destroyFcn,
						    ShapeType typeA, ShapeType typeB)
        {
	        Box2DXDebug.Assert(ShapeType.UnknownShape < typeA && typeA < ShapeType.ShapeTypeCount);
	        Box2DXDebug.Assert(ShapeType.UnknownShape < typeB && typeB < ShapeType.ShapeTypeCount);
        	
	        _registers[(int)typeA, (int)typeB].CreateFcn = createFcn;
	        _registers[(int)typeA, (int)typeB].DestroyFcn = destroyFcn;
	        _registers[(int)typeA, (int)typeB].Primary = true;

	        if (typeA != typeB)
	        {
		        _registers[(int)typeB, (int)typeA].CreateFcn = createFcn;
		        _registers[(int)typeB, (int)typeA].DestroyFcn = destroyFcn;
		        _registers[(int)typeB, (int)typeA].Primary = false;
	        }
        }

	    public static void InitializeRegisters()
        {
            AddType(CircleContact.Create, CircleContact.Destroy, ShapeType.CircleShape, ShapeType.CircleShape);
	        AddType(PolyAndCircleContact.Create, PolyAndCircleContact.Destroy, ShapeType.PolygonShape, ShapeType.CircleShape);
	        AddType(PolygonContact.Create, PolygonContact.Destroy, ShapeType.PolygonShape, ShapeType.PolygonShape);
	
	        AddType(EdgeAndCircleContact.Create, EdgeAndCircleContact.Destroy, ShapeType.EdgeShape, ShapeType.CircleShape);
	        AddType(PolyAndEdgeContact.Create, PolyAndEdgeContact.Destroy, ShapeType.PolygonShape, ShapeType.EdgeShape);
        }

	    public static Contact Create(Fixture fixtureA, Fixture fixtureB)
        {
            if (_initialized == false)
	        {
		        InitializeRegisters();
		        _initialized = true;
	        }

	        ShapeType type1 = fixtureA.GetType();
	        ShapeType type2 = fixtureB.GetType();

	        Box2DXDebug.Assert(ShapeType.UnknownShape < type1 && type1 < ShapeType.ShapeTypeCount);
	        Box2DXDebug.Assert(ShapeType.UnknownShape < type2 && type2 < ShapeType.ShapeTypeCount);
        	
	        ContactCreateFcn createFcn = _registers[(int)type1, (int)type2].CreateFcn;
	        if (createFcn != null)
	        {
		        if (_registers[(int)type1, (int)type2].Primary)
		        {
			        return createFcn(fixtureA, fixtureB);
		        }
		        else
		        {
			        return createFcn(fixtureB, fixtureA);
		        }
	        }
	        else
	        {
		        return null;
	        }
        }

	    public static void Destroy(Contact contact)
        {
            Box2DXDebug.Assert(_initialized == true);

	        if (contact._manifold._pointCount > 0)
	        {
		        contact.GetFixtureA().GetBody().WakeUp();
		        contact.GetFixtureB().GetBody().WakeUp();
	        }

	        ShapeType typeA = contact.GetFixtureA().GetType();
	        ShapeType typeB = contact.GetFixtureB().GetType();

	        Box2DXDebug.Assert(ShapeType.UnknownShape < typeA && typeB < ShapeType.ShapeTypeCount);
	        Box2DXDebug.Assert(ShapeType.UnknownShape < typeA && typeB < ShapeType.ShapeTypeCount);

	        ContactDestroyFcn destroyFcn = _registers[(int)typeA, (int)typeB].DestroyFcn;
	        destroyFcn(contact);
        }

	    public Contact() { _fixtureA = null; _fixtureB = null; }

	    public Contact(Fixture fixtureA, Fixture fixtureB)
        {
            _flags = 0;

	        if (fixtureA.IsSensor || fixtureB.IsSensor)
	        {
		        _flags |= ContactFlag.NonSolidFlag;
	        }

	        _fixtureA = fixtureA;
	        _fixtureB = fixtureB;

	        _manifold.m_pointCount = 0;

	        _prev = null;
	        _next = null;

	        _nodeA.Contact = null;
	        _nodeA.Prev = null;
	        _nodeA.Next = null;
	        _nodeA.Other = null;

	        _nodeB.Contact = null;
	        _nodeB.Prev = null;
	        _nodeB.Next = null;
	        _nodeB.Other = null;
        }

	    public void Update(ContactListener listener)
        {
            Manifold oldManifold = _manifold;

	        Evaluate();

	        Body bodyA = _fixtureA.GetBody();
	        Body bodyB = _fixtureB.GetBody();

	        int oldCount = oldManifold.m_pointCount;
	        int newCount = _manifold.m_pointCount;

	        if (newCount == 0 && oldCount > 0)
	        {
		        bodyA.WakeUp();
		        bodyB.WakeUp();
	        }

	        // Slow contacts don't generate TOI events.
	        if (bodyA.IsStatic() || bodyA.IsBullet() || bodyB.IsStatic() || bodyB.IsBullet())
	        {
		        _flags &= ContactFlag.SlowFlag;
	        }
	        else
	        {
		        _flags |= ContactFlag.SlowFlag;
	        }

	        // Match old contact ids to new contact ids and copy the
	        // stored impulses to warm start the solver.
	        for (int i = 0; i < _manifold.m_pointCount; ++i)
	        {
		        ManifoldPoint mp2 = _manifold.m_points + i;
		        mp2.m_normalImpulse = 0.0f;
		        mp2.m_tangentImpulse = 0.0f;
		        ContactID id2 = mp2.m_id;

		        for (int j = 0; j < oldManifold.m_pointCount; ++j)
		        {
			        ManifoldPoint mp1 = oldManifold.m_points + j;

			        if (mp1.m_id.key == id2.Key)
			        {
				        mp2.m_normalImpulse = mp1.m_normalImpulse;
				        mp2.m_tangentImpulse = mp1.m_tangentImpulse;
				        break;
			        }
		        }
	        }

	        if (oldCount == 0 && newCount > 0)
	        {
		        _flags |= ContactFlag.TouchFlag;
		        listener.BeginContact(this);
	        }

	        if (oldCount > 0 && newCount == 0)
	        {
		        _flags &= ContactFlag.TouchFlag;
		        listener.EndContact(this);
	        }

	        if ((_flags & ContactFlag.NonSolidFlag) == 0)
	        {
		        listener.PreSolve(this, oldManifold);

		        // The user may have disabled contact.
		        if (_manifold.m_pointCount == 0)
		        {
			        _flags &= ContactFlag.TouchFlag;
		        }
	        }
        }

	    public virtual void Evaluate();

        public virtual float ComputeTOI(Sweep sweepA, Sweep sweepB);
	}
}
