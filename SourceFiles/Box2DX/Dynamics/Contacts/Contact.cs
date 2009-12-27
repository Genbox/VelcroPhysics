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
        SensorFlag = 0x0001,
        // Generate TOI events
        ContinuousFlag = 0x0002,
        // Used when crawling contact graph when forming islands.
        IslandFlag = 0x0004,
        // Used in SolveTOI to indicate the cached toi value is still valid.
        ToiFlag = 0x0008,
        // Set when the shapes are touching.
        TouchingFlag = 0x0010,
        // Disabled (by user)
        EnabledFlag = 0x0020,
        // This contact needs filtering because a fixture filter was changed.
        FilterFlag = 0x0040,
    }


    /// <summary>
    /// This structure is used to report contact points.
    /// </summary>
    public abstract class Contact
    {
        public static ContactRegister[][] Registers = new ContactRegister[(int)ShapeType.ShapeTypeCount][];
        public static bool Initialized;

        public ContactFlag _flags;

        // World pool and list pointers.
        public Contact _prev;
        public Contact _next;

        // Nodes for connecting bodies.
        public ContactEdge _nodeA;
        public ContactEdge _nodeB;

        public Fixture _fixtureA;
        public Fixture _fixtureB;

        public Manifold _manifold;

        public float _toi;

        public Contact() { }

        public Contact(Fixture fixtureA, Fixture fixtureB)
        {
            _flags = ContactFlag.EnabledFlag;

            if (fixtureA._isSensor || fixtureB._isSensor)
            {
                _flags |= ContactFlag.SensorFlag;
            }

            Body bodyA = fixtureA.GetBody();
            Body bodyB = fixtureB.GetBody();

            if (bodyA.GetType() != Body.BodyType.Dynamic || bodyA.IsBullet() || bodyB.GetType() != Body.BodyType.Dynamic || bodyB.IsBullet())
            {
                _flags |= ContactFlag.ContinuousFlag;
            }

            _fixtureA = fixtureA;
            _fixtureB = fixtureB;

            _manifold = new Manifold();
            _manifold.PointCount = 0;

            _prev = null;
            _next = null;

            _nodeA = new ContactEdge();
            _nodeA.Contact = null;
            _nodeA.Prev = null;
            _nodeA.Next = null;
            _nodeA.Other = null;

            _nodeB = new ContactEdge();
            _nodeB.Contact = null;
            _nodeB.Prev = null;
            _nodeB.Next = null;
            _nodeB.Other = null;
        }

        /// Get the contact manifold. Do not modify the manifold unless you understand the
        /// internals of Box2D.
        public Manifold GetManifold() { return _manifold; }

        /// Get the world manifold.
        public void GetWorldManifold(out WorldManifold worldManifold)
        {
            worldManifold = new WorldManifold();

            Body bodyA = _fixtureA.GetBody();
            Body bodyB = _fixtureB.GetBody();
            Shape shapeA = _fixtureA.GetShape();
            Shape shapeB = _fixtureB.GetShape();

            worldManifold.Initialize(_manifold, bodyA.GetTransform(), shapeA._radius, bodyB.GetTransform(), shapeB._radius);
        }

        public void SetSensor(bool sensor)
        {
            if (sensor)
            {
                _flags |= ContactFlag.SensorFlag;
            }
            else
            {
                _flags &= ~ContactFlag.SensorFlag;
            }
        }

        public bool IsSensor()
        {
            return (_flags & ContactFlag.SensorFlag) == ContactFlag.SensorFlag;
        }

        /// Enable/disable this contact. This can be used inside the pre-solve
        /// contact listener. The contact is only disabled for the current
        /// time step (or sub-step in continuous collisions).
        public void SetEnabled(bool flag)
        {
            if (flag)
            {
                _flags |= ContactFlag.EnabledFlag;
            }
            else
            {
                _flags &= ~ContactFlag.EnabledFlag;
            }
        }

        /// Has this contact been disabled?
        public bool IsEnabled()
        {
            return (_flags & ContactFlag.EnabledFlag) == ContactFlag.EnabledFlag;
        }

        public bool IsTouching()
        {
            return (_flags & ContactFlag.TouchingFlag) == ContactFlag.TouchingFlag;
        }

        public bool IsContinuous()
        {
            return (_flags & ContactFlag.ContinuousFlag) == ContactFlag.ContinuousFlag;
        }

        /// Get the next contact in the world's contact list.
        public Contact GetNext() { return _next; }

        /// Get the first fixture in this contact.
        public Fixture GetFixtureA() { return _fixtureA; }

        /// Get the second fixture in this contact.
        public Fixture GetFixtureB() { return _fixtureB; }

        public void FlagForFiltering()
        {
            _flags |= ContactFlag.FilterFlag;
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
        }

        public static Contact Create(Fixture fixtureA, Fixture fixtureB)
        {
            if (Initialized == false)
            {
                InitializeRegisters();
                Initialized = true;
            }

            ShapeType type1 = fixtureA.GetType();
            ShapeType type2 = fixtureB.GetType();

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
            Destroy(contact, contact.GetFixtureA().GetType(), contact.GetFixtureB().GetType());
        }

        public static void Destroy(Contact contact, ShapeType typeA, ShapeType typeB)
        {
            Box2DXDebug.Assert(Initialized == true);

            if (contact._manifold.PointCount > 0)
            {
                contact.GetFixtureA().GetBody().SetAwake(true);
                contact.GetFixtureB().GetBody().SetAwake(true);
            }

            Box2DXDebug.Assert(ShapeType.UnknownShape < typeA && typeB < ShapeType.ShapeTypeCount);
            Box2DXDebug.Assert(ShapeType.UnknownShape < typeA && typeB < ShapeType.ShapeTypeCount);

            ContactDestroyFcn destroyFcn = Registers[(int)typeA][(int)typeB].DestroyFcn;
            destroyFcn(contact);
        }

        public abstract void Evaluate();

        public void Update(ContactListener listener)
        {
            //Note: Manifold is a class, not a struct. It will reference the old manifest, not copy it - DONE
            Manifold oldManifold = new Manifold();
            oldManifold.LocalPlaneNormal = _manifold.LocalPlaneNormal;
            oldManifold.LocalPoint = _manifold.LocalPoint;
            oldManifold.PointCount = _manifold.PointCount;
            oldManifold.Points = _manifold.Points;
            oldManifold.Type = _manifold.Type;

            // Re-enable this contact.
            _flags |= ContactFlag.EnabledFlag;

            if (Collision.Collision.TestOverlap(_fixtureA._aabb, _fixtureB._aabb))
            {
                Evaluate();
            }
            else
            {
                _manifold.PointCount = 0;
            }

            Body bodyA = _fixtureA.GetBody();
            Body bodyB = _fixtureB.GetBody();

            int oldCount = oldManifold.PointCount;
            int newCount = _manifold.PointCount;

            if (newCount == 0 && oldCount > 0)
            {
                bodyA.SetAwake(true);
                bodyB.SetAwake(true);
            }

            // Slow contacts don't generate TOI events.
            if (bodyA.GetType() != Body.BodyType.Dynamic || bodyA.IsBullet() || bodyB.GetType() != Body.BodyType.Dynamic || bodyB.IsBullet())
            {
                _flags |= ContactFlag.ContinuousFlag;
            }
            else
            {
                _flags &= ~ContactFlag.ContinuousFlag;
            }

            // Match old contact ids to new contact ids and copy the
            // stored impulses to warm start the solver.
            for (int i = 0; i < _manifold.PointCount; ++i)
            {
                ManifoldPoint mp2 = _manifold.Points[i];
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

            if (newCount > 0)
            {
                _flags |= ContactFlag.TouchingFlag;
            }
            else
            {
                _flags &= ~ContactFlag.TouchingFlag;
            }

            if (oldCount == 0 && newCount > 0)
            {
                listener.BeginContact(this);
            }

            if (oldCount > 0 && newCount == 0)
            {
                listener.EndContact(this);
            }

            if ((_flags & ContactFlag.SensorFlag) == 0)
            {
                listener.PreSolve(this, oldManifold);
            }
        }

        public float ComputeTOI(Sweep sweepA, Sweep sweepB)
        {
            Collision.Collision.TOIInput input = new Collision.Collision.TOIInput();
            input.ProxyA.Set(_fixtureA.GetShape());
            input.ProxyB.Set(_fixtureB.GetShape());
            input.SweepA = sweepA;
            input.SweepB = sweepB;
            input.Tolerance = Settings.LinearSlop;

            return Collision.Collision.TimeOfImpact(input);
        }
    }
}
