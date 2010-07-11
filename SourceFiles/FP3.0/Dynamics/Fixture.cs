/*
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

    /// This proxy is used internally to connect fixtures to the broad-phase.
    public class FixtureProxy
    {
        public AABB Aabb;
        public Fixture Fixture;
        public int ChildIndex;
        public int ProxyId;
    }

    /// <summary>
    /// A fixture is used to attach a shape to a body for collision detection. A fixture
    /// inherits its transform from its parent. Fixtures hold additional non-geometric data
    /// such as friction, collision filters, etc.
    /// Fixtures are created via Body.CreateFixture.
    /// @warning you cannot reuse fixtures.
    /// </summary>
    public class Fixture
    {
        #region Delegates

        public delegate bool CollisionEventHandler(Fixture fixtureA, Fixture fixtureB, Manifold manifold);

        public delegate void SeparationEventHandler(Fixture fixtureA, Fixture fixtureB);

        #endregion

        public CollisionEventHandler OnCollision;
        public SeparationEventHandler OnSeparation;

        internal Body _body;
        private CollisionCategory _collidesWith;
        private CollisionCategory _collisionCategories;
        private short _collisionGroup;
        private Dictionary<int, bool> _collisionIgnores = new Dictionary<int, bool>();
        private Shape _shape;
        public Fixture _next;

        public FixtureProxy[] _proxies;
        public int _proxyCount;

        internal Fixture()
        {
        }

        /// <summary>
        /// We need separation create/destroy functions from the constructor/destructor because
        /// the destructor cannot access the allocator or broad-phase (no destructor arguments allowed by C++).
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="shape">The shape.</param>
        internal Fixture(Body body, Shape shape)
        {
            ProxyId = BroadPhase.NullProxy;

            //Fixture defaults
            Friction = 0.2f;
            _collisionCategories = CollisionCategory.All;
            _collidesWith = CollisionCategory.All;
            IsSensor = false;

            _body = body;
            _next = null;

            _shape = shape.Clone();

            // Reserve proxy space
            int childCount = _shape.GetChildCount();
            _proxies = new FixtureProxy[childCount];
            for (int i = 0; i < childCount; ++i)
            {
                _proxies[i] = new FixtureProxy();
                _proxies[i].Fixture = null;
                _proxies[i].ProxyId = BroadPhase.NullProxy;
            }
            _proxyCount = 0;

        }

        /// <summary>
        /// Get the child shape. You can modify the child shape, however you should not change the
        /// number of vertices because this will crash some collision caching mechanisms.
        /// Manipulating the shape may lead to non-physical behavior.
        /// </summary>
        /// <value></value>
        public Shape Shape
        {
            get { return _shape; }
        }

        /// <summary>
        /// Set if this fixture is a sensor.
        /// </summary>
        /// <value>
        ///   if set to &lt;c&gt;true&lt;/c&gt; [sensor].
        /// </value>
        public bool IsSensor { get; set; }

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
                if (_body == null)
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
                if (_body == null)
                    return;

                if (_collidesWith == value)
                    return;

                _collidesWith = value;
                FilterChanged();
            }
        }

        /// <summary>
        /// The collision category bits. Normally you would just set one bit.
        /// </summary>
        public CollisionCategory CollisionCategories
        {
            get { return _collisionCategories; }

            set
            {
                if (_body == null)
                    return;

                if (_collisionCategories == value)
                    return;

                _collisionCategories = value;
                FilterChanged();
            }
        }

        /// <summary>
        /// Get the parent body of this fixture. This is null if the fixture is not attached.
        /// </summary>
        /// <value>the parent body.</value>
        public Body Body
        {
            get { return _body; }
        }

        /// <summary>
        /// Set the user data. Use this to store your application specific data.
        /// </summary>
        /// <value>The data.</value>
        public object UserData { get; set; }

        public int ProxyId { get; private set; }

        /// <summary>
        /// Set the coefficient of friction.
        /// </summary>
        /// <value>The friction.</value>
        public float Friction { get; set; }

        /// <summary>
        /// Get the coefficient of restitution.
        /// </summary>
        /// <value></value>
        public float Restitution { get; set; }

        /// <summary>
        /// Get the fixture's AABB. This AABB may be enlarge and/or stale.
        /// If you need a more accurate AABB, compute it using the shape and
        /// the body transform.
        /// </summary>
        public void GetAABB(out AABB aabb, int childIndex)
        {
            Debug.Assert(0 <= childIndex && childIndex < _proxyCount);
            aabb = _proxies[childIndex].Aabb;
        }

        public void RestoreCollisionWith(Fixture fixture)
        {
            if (_collisionIgnores.ContainsKey(fixture.ProxyId))
            {
                _collisionIgnores[fixture.ProxyId] = false;
                FilterChanged();
            }
        }

        public void IgnoreCollisionWith(Fixture fixture)
        {
            if (_collisionIgnores.ContainsKey(fixture.ProxyId))
                _collisionIgnores[fixture.ProxyId] = true;
            else
                _collisionIgnores.Add(fixture.ProxyId, true);

            FilterChanged();
        }

        public bool IsGeometryIgnored(Fixture fixture)
        {
            if (_collisionIgnores.ContainsKey(fixture.ProxyId))
                return _collisionIgnores[fixture.ProxyId];

            return false;
        }

        private void FilterChanged()
        {
            // Flag associated contacts for filtering.
            ContactEdge edge = _body.ContactList;
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

        /// <summary>
        /// Test a point for containment in this fixture.
        /// </summary>
        /// <param name="p">a point in world coordinates.</param>
        /// <returns></returns>
        public bool TestPoint(ref Vector2 p)
        {
            Transform xf;
            _body.GetTransform(out xf);
            return _shape.TestPoint(ref xf, p);
        }

        /// <summary>
        /// Cast a ray against this shape.
        /// </summary>
        /// <param name="output">the ray-cast results.</param>
        /// <param name="input">the ray-cast input parameters.</param>
        /// <returns></returns>
        public bool RayCast(out RayCastOutput output, ref RayCastInput input, int childIndex)
        {
            Transform xf;
            _body.GetTransform(out xf);
            return _shape.RayCast(out output, ref input, ref xf, childIndex);
        }

        internal void Destroy()
        {
            // The proxies must be destroyed before calling this.
            Debug.Assert(_proxyCount == 0);

            // Free the proxy array.
            _proxies = null;

            _shape = null;
        }

        // These support body activation/deactivation.
        internal void CreateProxies(BroadPhase broadPhase, ref Transform xf)
        {
            Debug.Assert(_proxyCount == 0);

            // Create proxies in the broad-phase.
            _proxyCount = _shape.GetChildCount();

            for (int i = 0; i < _proxyCount; ++i)
            {
                FixtureProxy proxy = _proxies[i];
                _shape.ComputeAABB(out proxy.Aabb, ref xf, i);
                proxy.Fixture = this;
                proxy.ChildIndex = i;
                proxy.ProxyId = broadPhase.CreateProxy(ref proxy.Aabb, proxy);

                _proxies[i] = proxy;
            }
        }

        internal void DestroyProxies(BroadPhase broadPhase)
        {
            // Destroy proxies in the broad-phase.
            for (int i = 0; i < _proxyCount; ++i)
            {
                broadPhase.DestroyProxy(_proxies[i].ProxyId);
                _proxies[i].ProxyId = BroadPhase.NullProxy;
            }

            _proxyCount = 0;
        }

        internal void Synchronize(BroadPhase broadPhase, ref Transform transform1, ref Transform transform2)
        {
            if (_proxyCount == 0)
            {
                return;
            }

            for (int i = 0; i < _proxyCount; ++i)
            {
                FixtureProxy proxy = _proxies[i];

                // Compute an AABB that covers the swept Shape (may miss some rotation effect).
                AABB aabb1, aabb2;
                _shape.ComputeAABB(out aabb1, ref transform1, proxy.ChildIndex);
                _shape.ComputeAABB(out aabb2, ref transform2, proxy.ChildIndex);

                proxy.Aabb.Combine(ref aabb1, ref aabb2);

                Vector2 displacement = transform2.Position - transform1.Position;

                broadPhase.MoveProxy(proxy.ProxyId, ref proxy.Aabb, displacement);
            }
        }

        public Action<ContactConstraint> PostSolve;
    }
}