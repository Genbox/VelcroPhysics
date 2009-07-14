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

    [Flags]
    public enum ContactFlag
    {
        // This contact should not participate in Solve
        // The contact equivalent of sensors
        NonSolidFlag = 0x0001,
        // Do not use TOI solve.
        SlowFlag = 0x0002,
        // Used when crawling contact graph when forming islands.
        IslandFlag = 0x0004,
        // Used in SolveTOI to indicate the cached toi value is still valid.
        ToiFlag = 0x0008,
        // TODO: Doc
        TouchFlag = 0x0010,
        // Contacts are invalid if they have been created or modified inside a step
        // and remain invalid until the next step.
        InvalidFlag = 0x0020,
        // Marked for deferred destruction.
        DestroyFlag = 0x0040,
        // This marks if contact is currently being evaluated.
        // Meaning it should be deferred instead of destroyed.
        // This is essntially a poor mans recursive lock.
        LockedFlag = 0x0080,
    }

#warning "CAS"
    /// <summary>
    /// This structure is used to report contact points.
    /// </summary>
    public abstract class Contact
    {
        public static ContactRegister[][] Registers = new ContactRegister[(int)ShapeType.ShapeTypeCount][];
        public static bool Initialized = false;

        public ContactFlag Flags;

        // World pool and list pointers.
        public Contact Prev;
        public Contact Next;

        // Nodes for connecting bodies.
        public ContactEdge NodeA;
        public ContactEdge NodeB;

        public Fixture _fixtureA;
        public Fixture _fixtureB;

        public Manifold Manifold;

        public float Toi;

        public object UserData;

        /// Get the contact manifold.
        public Manifold GetManifold() { return Manifold; }

        /// Get the world manifold.
        public void GetWorldManifold(out WorldManifold worldManifold)
        {
            worldManifold = new WorldManifold();

            Body bodyA = _fixtureA.GetBody();
            Body bodyB = _fixtureB.GetBody();
            Shape shapeA = _fixtureA.GetShape();
            Shape shapeB = _fixtureB.GetShape();

            worldManifold.Initialize(Manifold, bodyA.GetXForm(), shapeA.Radius, bodyB.GetXForm(), shapeB.Radius);
        }

        /// Is this contact solid?
        /// @return true if this contact should generate a response.
        public bool IsSolid() { return (Flags & ContactFlag.NonSolidFlag) == 0; }

        public bool IsInvalid()
        {
	        return (Flags & ContactFlag.InvalidFlag) != 0;
        }

        public bool IsDestroyed()
        {
            return (Flags & ContactFlag.DestroyFlag) != 0;
        }

        /// Are fixtures touching?
        public bool AreTouching() { return (Flags & ContactFlag.TouchFlag) == ContactFlag.TouchFlag; }

        /// Get the next contact in the world's contact list.
        public Contact GetNext() { return Next; }

        /// Get the first fixture in this contact.
        public Fixture GetFixtureA() { return _fixtureA; }

        /// Get the second fixture in this contact.
        public Fixture GetFixtureB() { return _fixtureB; }

        //public void b2Contact::DisableCollisionResponses()
        //{
        //	m_flags |= e_nonSolidFlag;
        //}

        public object GetUserData()
        {
	        return UserData;
        }

        public void SetUserData(object data)
        {
	        UserData = data;
        }

        public static void AddType(ContactCreateFcn createFcn, ContactDestroyFcn destroyFcn,
                            ShapeType typeA, ShapeType typeB)
        {
            Box2DXDebug.Assert(ShapeType.UnknownShape < typeA && typeA < ShapeType.ShapeTypeCount);
            Box2DXDebug.Assert(ShapeType.UnknownShape < typeB && typeB < ShapeType.ShapeTypeCount);

            Registers[(int)typeA][(int)typeB].CreateFcn = createFcn;
            Registers[(int)typeA][(int)typeB].DestroyFcn = destroyFcn;
            Registers[(int)typeA][(int)typeB].Primary = true;

            if (typeA != typeB)
            {
                Registers[(int)typeB][(int)typeA].CreateFcn = createFcn;
                Registers[(int)typeB][(int)typeA].DestroyFcn = destroyFcn;
                Registers[(int)typeB][(int)typeA].Primary = false;
            }
        }

        public static void InitializeRegisters()
        {
            for (int i = 0; i < (int)ShapeType.ShapeTypeCount; i++)
            {
                Registers[i] = new ContactRegister[(int)ShapeType.ShapeTypeCount];
            }

            AddType(CircleContact.Create, CircleContact.Destroy, ShapeType.CircleShape, ShapeType.CircleShape);
            AddType(PolyAndCircleContact.Create, PolyAndCircleContact.Destroy, ShapeType.PolygonShape, ShapeType.CircleShape);
            AddType(PolygonContact.Create, PolygonContact.Destroy, ShapeType.PolygonShape, ShapeType.PolygonShape);

            AddType(EdgeAndCircleContact.Create, EdgeAndCircleContact.Destroy, ShapeType.EdgeShape, ShapeType.CircleShape);
            AddType(PolyAndEdgeContact.Create, PolyAndEdgeContact.Destroy, ShapeType.PolygonShape, ShapeType.EdgeShape);
        }

        public static Contact Create(Fixture fixtureA, Fixture fixtureB)
        {
            if (Initialized == false)
            {
                InitializeRegisters();
                Initialized = true;
            }

            ShapeType type1 = fixtureA.GetShapeType();
            ShapeType type2 = fixtureB.GetShapeType();

            Box2DXDebug.Assert(ShapeType.UnknownShape < type1 && type1 < ShapeType.ShapeTypeCount);
            Box2DXDebug.Assert(ShapeType.UnknownShape < type2 && type2 < ShapeType.ShapeTypeCount);

            ContactCreateFcn createFcn = Registers[(int)type1][(int)type2].CreateFcn;
            if (createFcn != null)
            {
                if (Registers[(int)type1][(int)type2].Primary)
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
            Destroy(contact, contact.GetFixtureA().GetShapeType(), contact.GetFixtureB().GetShapeType());
        }

        public static void Destroy(Contact contact, ShapeType typeA, ShapeType typeB)
        {
            Box2DXDebug.Assert(Initialized == true);

            if (contact.Manifold.PointCount > 0)
            {
                contact.GetFixtureA().GetBody().WakeUp();
                contact.GetFixtureB().GetBody().WakeUp();
            }

            Box2DXDebug.Assert(ShapeType.UnknownShape < typeA && typeB < ShapeType.ShapeTypeCount);
            Box2DXDebug.Assert(ShapeType.UnknownShape < typeA && typeB < ShapeType.ShapeTypeCount);

            ContactDestroyFcn destroyFcn = Registers[(int)typeA][(int)typeB].DestroyFcn;
            destroyFcn(contact);
        }

        public Contact() { }

        public Contact(Fixture fixtureA, Fixture fixtureB)
        {
            Flags = 0;

            if (fixtureA.IsSensor || fixtureB.IsSensor)
            {
                Flags |= ContactFlag.NonSolidFlag;
            }

            _fixtureA = fixtureA;
            _fixtureB = fixtureB;

            Manifold = new Manifold();
            Manifold.PointCount = 0;

            Prev = null;
            Next = null;

            NodeA = new ContactEdge();
            NodeA.Contact = null;
            NodeA.Prev = null;
            NodeA.Next = null;
            NodeA.Other = null;

            NodeB = new ContactEdge();
            NodeB.Contact = null;
            NodeB.Prev = null;
            NodeB.Next = null;
            NodeB.Other = null;
        }

        public abstract void Evaluate();

        public abstract float ComputeTOI(Sweep sweepA, Sweep sweepB);
    }
}
