/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
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
using System.Collections.Generic;
using System.Diagnostics;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics
{
    [Flags]
    public enum CollisionCategory
    {
        None = 0,
        All = int.MaxValue,
        Cat1 = 1,
        Cat2 = 2,
        Cat3 = 4,
        Cat4 = 8,
        Cat5 = 16,
        Cat6 = 32,
        Cat7 = 64,
        Cat8 = 128,
        Cat9 = 256,
        Cat10 = 512,
        Cat11 = 1024,
        Cat12 = 2048,
        Cat13 = 4096,
        Cat14 = 8192,
        Cat15 = 16384,
        Cat16 = 32768,
        Cat17 = 65536,
        Cat18 = 131072,
        Cat19 = 262144,
        Cat20 = 524288,
        Cat21 = 1048576,
        Cat22 = 2097152,
        Cat23 = 4194304,
        Cat24 = 8388608,
        Cat25 = 16777216,
        Cat26 = 33554432,
        Cat27 = 67108864,
        Cat28 = 134217728,
        Cat29 = 268435456,
        Cat30 = 536870912,
        Cat31 = 1073741824
    }

    /// <summary>
    /// This proxy is used internally to connect fixtures to the broad-phase.
    /// </summary>
    public struct FixtureProxy
    {
        public AABB AABB;
        public int ChildIndex;
        public Fixture Fixture;
        public int ProxyId;
    }

    public delegate bool CollisionEventHandler(Fixture fixtureA, Fixture fixtureB, Contact manifold);

    public delegate void SeparationEventHandler(Fixture fixtureA, Fixture fixtureB);

    /// <summary>
    /// A fixture is used to attach a Shape to a body for collision detection. A fixture
    /// inherits its transform from its parent. Fixtures hold additional non-geometric data
    /// such as friction, collision filters, etc.
    /// Fixtures are created via Body.CreateFixture.
    /// Warning: You cannot reuse fixtures.
    /// </summary>
    public class Fixture
    {
        private static int _fixtureIdCounter;

        /// <summary>
        /// Fires when two shapes collide and a contact is created between them.
        /// Note that the first fixture argument is always the fixture that the delegate is subscribed to.
        /// </summary>
        public CollisionEventHandler OnCollision;

        /// <summary>
        /// Fires when two shapes separate and a contact is removed between them.
        /// Note that the first fixture argument is always the fixture that the delegate is subscribed to.
        /// </summary>
        public SeparationEventHandler OnSeparation;

        public Action<ContactConstraint> PostSolve;
        public FixtureProxy[] Proxies;
        public int ProxyCount;
        private CollisionCategory _collidesWith;
        private CollisionCategory _collisionCategories;
        private short _collisionGroup;
        private Dictionary<int, bool> _collisionIgnores = new Dictionary<int, bool>();

        internal Fixture(Body body, Shape shape)
        {
            //Fixture defaults
            Friction = 0.2f;
            _collisionCategories = CollisionCategory.All;
            _collidesWith = CollisionCategory.All;
            IsSensor = false;

            Body = body;

            if (Settings.ConserveMemory)
                Shape = shape;
            else
                Shape = shape.Clone();

            // Reserve proxy space
            int childCount = Shape.ChildCount;
            Proxies = new FixtureProxy[childCount];
            for (int i = 0; i < childCount; ++i)
            {
                Proxies[i] = new FixtureProxy();
                Proxies[i].Fixture = null;
                Proxies[i].ProxyId = BroadPhase.NullProxy;
            }
            ProxyCount = 0;

            FixtureId = _fixtureIdCounter++;
        }

        /// <summary>
        /// Get the type of the child Shape. You can use this to down cast to the concrete Shape.
        /// </summary>
        /// <value>The type of the shape.</value>
        public ShapeType ShapeType
        {
            get { return Shape.ShapeType; }
        }

        /// <summary>
        /// Get the child Shape. You can modify the child Shape, however you should not change the
        /// number of vertices because this will crash some collision caching mechanisms.
        /// </summary>
        /// <value>The shape.</value>
        public Shape Shape { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this fixture is a sensor.
        /// </summary>
        /// <value><c>true</c> if this instance is a sensor; otherwise, <c>false</c>.</value>
        public bool IsSensor { get; set; }

        /// <summary>
        /// Get the parent body of this fixture. This is null if the fixture is not attached.
        /// </summary>
        /// <value>The body.</value>
        public Body Body { get; internal set; }

        /// <summary>
        /// Set the user data. Use this to store your application specific data.
        /// </summary>
        /// <value>The user data.</value>
        public object UserData { get; set; }

        /// <summary>
        /// Get or set the coefficient of friction.
        /// </summary>
        /// <value>The friction.</value>
        public float Friction { get; set; }

        /// <summary>
        /// Get or set the coefficient of restitution.
        /// </summary>
        /// <value>The restitution.</value>
        public float Restitution { get; set; }

        /// <summary>
        /// Collision groups allow a certain group of objects to never collide (negative)
        /// or always collide (positive). Zero means no collision group. Non-zero group
        /// filtering always wins against the mask bits.
        /// Warning: The filter will not take effect until next step.
        /// </summary>
        public short CollisionGroup
        {
            set
            {
                if (Body == null)
                    return;

                if (_collisionGroup == value)
                    return;

                _collisionGroup = value;
                FilterChanged();
            }
            get { return _collisionGroup; }
        }

        /// <summary>
        /// The collision mask bits. This states the categories that this
        /// shape would accept for collision.
        /// </summary>
        public CollisionCategory CollidesWith
        {
            get { return _collidesWith; }

            set
            {
                if (Body == null)
                    return;

                if (_collidesWith == value)
                    return;

                _collidesWith = value;
                FilterChanged();
            }
        }

        public int FixtureId { get; private set; }

        /// <summary>
        /// The collision category bits. Normally you would just set one bit.
        /// </summary>
        public CollisionCategory CollisionCategories
        {
            get { return _collisionCategories; }

            set
            {
                if (Body == null)
                    return;

                if (_collisionCategories == value)
                    return;

                _collisionCategories = value;
                FilterChanged();
            }
        }

        /// <summary>
        /// Test a point for containment in this fixture.
        /// </summary>
        /// <param name="point">A point in world coordinates.</param>
        /// <returns></returns>
        public bool TestPoint(ref Vector2 point)
        {
            Transform xf;
            Body.GetTransform(out xf);
            return Shape.TestPoint(ref xf, ref point);
        }

        /// <summary>
        /// Cast a ray against this Shape.
        /// </summary>
        /// <param name="output">The ray-cast results.</param>
        /// <param name="input">The ray-cast input parameters.</param>
        /// <param name="childIndex">Index of the child.</param>
        /// <returns></returns>
        public bool RayCast(out RayCastOutput output, ref RayCastInput input, int childIndex)
        {
            Transform xf;
            Body.GetTransform(out xf);
            return Shape.RayCast(out output, ref input, ref xf, childIndex);
        }

        /// <summary>
        /// Get the mass data for this fixture. The mass data is based on the density and
        /// the Shape. The rotational inertia is about the Shape's origin.
        /// </summary>
        /// <param name="massData">The mass data.</param>
        public MassData GetMassData()
        {
            return Shape.MassData;
        }

        /// <summary>
        /// Get the fixture's AABB. This AABB may be enlarge and/or stale.
        /// If you need a more accurate AABB, compute it using the Shape and
        /// the body transform.
        /// </summary>
        /// <param name="aabb">The aabb.</param>
        /// <param name="childIndex">Index of the child.</param>
        public void GetAABB(out AABB aabb, int childIndex)
        {
            Debug.Assert(0 <= childIndex && childIndex < ProxyCount);
            aabb = Proxies[childIndex].AABB;
        }

        internal void Destroy()
        {
            // The proxies must be destroyed before calling this.
            Debug.Assert(ProxyCount == 0);

            // Free the proxy array.
            Proxies = null;

            Shape = null;

            if (Body.World.FixtureRemoved != null)
            {
                Body.World.FixtureRemoved(this);
            }
        }

        // These support body activation/deactivation.
        internal void CreateProxies(BroadPhase broadPhase, ref Transform xf)
        {
            Debug.Assert(ProxyCount == 0);

            // Create proxies in the broad-phase.
            ProxyCount = Shape.ChildCount;

            for (int i = 0; i < ProxyCount; ++i)
            {
                FixtureProxy proxy = Proxies[i];
                Shape.ComputeAABB(out proxy.AABB, ref xf, i);
                proxy.Fixture = this;
                proxy.ChildIndex = i;
                proxy.ProxyId = broadPhase.CreateProxy(ref proxy.AABB, proxy);

                Proxies[i] = proxy;
            }
        }

        internal void DestroyProxies(BroadPhase broadPhase)
        {
            // Destroy proxies in the broad-phase.
            for (int i = 0; i < ProxyCount; ++i)
            {
                broadPhase.DestroyProxy(Proxies[i].ProxyId);
                Proxies[i].ProxyId = BroadPhase.NullProxy;
            }

            ProxyCount = 0;
        }

        internal void Synchronize(BroadPhase broadPhase, ref Transform transform1, ref Transform transform2)
        {
            if (ProxyCount == 0)
            {
                return;
            }

            for (int i = 0; i < ProxyCount; ++i)
            {
                FixtureProxy proxy = Proxies[i];

                // Compute an AABB that covers the swept Shape (may miss some rotation effect).
                AABB aabb1, aabb2;
                Shape.ComputeAABB(out aabb1, ref transform1, proxy.ChildIndex);
                Shape.ComputeAABB(out aabb2, ref transform2, proxy.ChildIndex);

                proxy.AABB.Combine(ref aabb1, ref aabb2);

                Vector2 displacement = transform2.Position - transform1.Position;

                broadPhase.MoveProxy(proxy.ProxyId, ref proxy.AABB, displacement);
            }
        }

        /// <summary>
        /// Restores collisions between this fixture and the provided fixture.
        /// </summary>
        /// <param name="fixture">The fixture.</param>
        public void RestoreCollisionWith(Fixture fixture)
        {
            if (_collisionIgnores.ContainsKey(fixture.FixtureId))
            {
                _collisionIgnores[fixture.FixtureId] = false;
                FilterChanged();
            }
        }

        /// <summary>
        /// Ignores collisions between this fixture and the provided fixture.
        /// </summary>
        /// <param name="fixture">The fixture.</param>
        public void IgnoreCollisionWith(Fixture fixture)
        {
            if (_collisionIgnores.ContainsKey(fixture.FixtureId))
                _collisionIgnores[fixture.FixtureId] = true;
            else
                _collisionIgnores.Add(fixture.FixtureId, true);

            FilterChanged();
        }

        /// <summary>
        /// Determines whether collisions are ignored between this fixture and the provided fixture.
        /// </summary>
        /// <param name="fixture">The fixture.</param>
        /// <returns>
        /// 	<c>true</c> if the fixture is ignored; otherwise, <c>false</c>.
        /// </returns>
        public bool IsFixtureIgnored(Fixture fixture)
        {
            if (_collisionIgnores.ContainsKey(fixture.FixtureId))
                return _collisionIgnores[fixture.FixtureId];

            return false;
        }

        private void FilterChanged()
        {
            // Flag associated contacts for filtering.
            ContactEdge edge = Body.ContactList;
            while (edge != null)
            {
                Contact contact = edge.Contact;
                Fixture fixtureA = contact.FixtureA;
                Fixture fixtureB = contact.FixtureB;
                if (fixtureA == this || fixtureB == this)
                {
                    contact.FlagForFiltering();
                }

                edge = edge.Next;
            }
        }
    }
}