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

using System.Diagnostics;
using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.Broadphase;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Collision.Filtering;
using VelcroPhysics.Collision.Handlers;
using VelcroPhysics.Collision.RayCast;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Shared;
using VelcroPhysics.Templates;

namespace VelcroPhysics.Dynamics
{
    /// <summary>
    /// A fixture is used to attach a Shape to a body for collision detection. A fixture
    /// inherits its transform from its parent. Fixtures hold additional non-geometric data
    /// such as friction, collision filters, etc.
    /// Fixtures are created via Body.CreateFixture.
    /// Warning: You cannot reuse fixtures.
    /// </summary>
    public class Fixture
    {
        internal Category _collidesWith;
        internal Category _collisionCategories;
        internal short _collisionGroup;
        private float _friction;
        private bool _isSensor;
        private float _restitution;

        /// <summary>
        /// Fires after two shapes has collided and are solved. This gives you a chance to get the impact force.
        /// </summary>
        public AfterCollisionHandler AfterCollision;

        /// <summary>
        /// Fires when two fixtures are close to each other.
        /// Due to how the broadphase works, this can be quite inaccurate as shapes are approximated using AABBs.
        /// </summary>
        public BeforeCollisionHandler BeforeCollision;

        public Category IgnoreCCDWith;

        /// <summary>
        /// Fires when two shapes collide and a contact is created between them.
        /// Note that the first fixture argument is always the fixture that the delegate is subscribed to.
        /// </summary>
        public OnCollisionHandler OnCollision;

        /// <summary>
        /// Fires when two shapes separate and a contact is removed between them.
        /// Note: This can in some cases be called multiple times, as a fixture can have multiple contacts.
        /// Note The first fixture argument is always the fixture that the delegate is subscribed to.
        /// </summary>
        public OnSeparationHandler OnSeparation;

        public FixtureProxy[] Proxies;
        public int ProxyCount;

        internal Fixture()
        {
            _collisionCategories = Settings.DefaultFixtureCollisionCategories;
            _collidesWith = Settings.DefaultFixtureCollidesWith;
            _collisionGroup = 0;

            IgnoreCCDWith = Settings.DefaultFixtureIgnoreCCDWith;
        }

        internal Fixture(Body body, FixtureTemplate template) : this()
        {
            UserData = template.UserData;
            Friction = template.Friction;
            Restitution = template.Restitution;

            Body = body;
            IsSensor = template.IsSensor;
            Shape = template.Shape.Clone();

            RegisterFixture();
        }

        /// <summary>
        /// Defaults to 0
        /// If Settings.UseFPECollisionCategories is set to false:
        /// Collision groups allow a certain group of objects to never collide (negative)
        /// or always collide (positive). Zero means no collision group. Non-zero group
        /// filtering always wins against the mask bits.
        /// If Settings.UseFPECollisionCategories is set to true:
        /// If 2 fixtures are in the same collision group, they will not collide.
        /// </summary>
        public short CollisionGroup
        {
            set
            {
                if (_collisionGroup == value)
                    return;

                _collisionGroup = value;
                Refilter();
            }
            get { return _collisionGroup; }
        }

        /// <summary>
        /// Defaults to Category.All
        /// The collision mask bits. This states the categories that this
        /// fixture would accept for collision.
        /// Use Settings.UseFPECollisionCategories to change the behavior.
        /// </summary>
        public Category CollidesWith
        {
            get { return _collidesWith; }

            set
            {
                if (_collidesWith == value)
                    return;

                _collidesWith = value;
                Refilter();
            }
        }

        /// <summary>
        /// The collision categories this fixture is a part of.
        /// If Settings.UseFPECollisionCategories is set to false:
        /// Defaults to Category.Cat1
        /// If Settings.UseFPECollisionCategories is set to true:
        /// Defaults to Category.All
        /// </summary>
        public Category CollisionCategories
        {
            get { return _collisionCategories; }

            set
            {
                if (_collisionCategories == value)
                    return;

                _collisionCategories = value;
                Refilter();
            }
        }

        /// <summary>
        /// Get the child Shape. You can modify the child Shape, however you should not change the
        /// number of vertices because this will crash some collision caching mechanisms.
        /// </summary>
        /// <value>The shape.</value>
        public Shape Shape { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether this fixture is a sensor.
        /// </summary>
        /// <value><c>true</c> if this instance is a sensor; otherwise, <c>false</c>.</value>
        public bool IsSensor
        {
            get { return _isSensor; }
            set
            {
                if (Body != null)
                    Body.Awake = true;

                _isSensor = value;
            }
        }

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
        /// Set the coefficient of friction. This will _not_ change the friction of
        /// existing contacts.
        /// </summary>
        /// <value>The friction.</value>
        public float Friction
        {
            get { return _friction; }
            set
            {
                Debug.Assert(!float.IsNaN(value));

                _friction = value;
            }
        }

        /// <summary>
        /// Set the coefficient of restitution. This will not change the restitution of
        /// existing contacts.
        /// </summary>
        /// <value>The restitution.</value>
        public float Restitution
        {
            get { return _restitution; }
            set
            {
                Debug.Assert(!float.IsNaN(value));

                _restitution = value;
            }
        }

        /// <summary>
        /// Gets a unique ID for this fixture.
        /// </summary>
        /// <value>The fixture id.</value>
        public int FixtureId { get; internal set; }

        /// <summary>
        /// Contacts are persistent and will keep being persistent unless they are
        /// flagged for filtering.
        /// This methods flags all contacts associated with the body for filtering.
        /// </summary>
        private void Refilter()
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
                    contact._flags |= ContactFlags.FilterFlag;
                }

                edge = edge.Next;
            }

            World world = Body._world;

            if (world == null)
            {
                return;
            }

            // Touch each proxy so that new pairs may be created
            IBroadPhase broadPhase = world.ContactManager.BroadPhase;
            for (int i = 0; i < ProxyCount; ++i)
            {
                broadPhase.TouchProxy(Proxies[i].ProxyId);
            }
        }

        private void RegisterFixture()
        {
            // Reserve proxy space
            Proxies = new FixtureProxy[Shape.ChildCount];
            ProxyCount = 0;

            if (Body.Enabled)
            {
                IBroadPhase broadPhase = Body._world.ContactManager.BroadPhase;
                CreateProxies(broadPhase, ref Body._xf);
            }

            Body.FixtureList.Add(this);

            // Adjust mass properties if needed.
            if (Shape._density > 0.0f)
            {
                Body.ResetMassData();
            }

            // Let the world know we have a new fixture. This will cause new contacts
            // to be created at the beginning of the next time step.
            Body._world._worldHasNewFixture = true;

            //Velcro: Added event
            Body._world.FixtureAdded?.Invoke(this);
        }

        /// <summary>
        /// Test a point for containment in this fixture.
        /// </summary>
        /// <param name="point">A point in world coordinates.</param>
        /// <returns></returns>
        public bool TestPoint(ref Vector2 point)
        {
            return Shape.TestPoint(ref Body._xf, ref point);
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
            return Shape.RayCast(ref input, ref Body._xf, childIndex, out output);
        }

        /// <summary>
        /// Get the fixture's AABB. This AABB may be enlarge and/or stale.
        /// If you need a more accurate AABB, compute it using the Shape and
        /// the body transform.
        /// </summary>
        /// <param name="aabb">The AABB.</param>
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

            //Velcro: We set the userdata to null here to help prevent bugs related to stale references in GC
            UserData = null;

            BeforeCollision = null;
            OnCollision = null;
            OnSeparation = null;
            AfterCollision = null;

            Body._world.FixtureRemoved?.Invoke(this);

            Body._world.FixtureAdded = null;
            Body._world.FixtureRemoved = null;
            OnSeparation = null;
            OnCollision = null;
        }

        // These support body activation/deactivation.
        internal void CreateProxies(IBroadPhase broadPhase, ref Transform xf)
        {
            Debug.Assert(ProxyCount == 0);

            // Create proxies in the broad-phase.
            ProxyCount = Shape.ChildCount;

            for (int i = 0; i < ProxyCount; ++i)
            {
                FixtureProxy proxy = new FixtureProxy();
                Shape.ComputeAABB(ref xf, i, out proxy.AABB);
                proxy.Fixture = this;
                proxy.ChildIndex = i;

                //Velcro note: This line needs to be after the previous two because FixtureProxy is a struct
                proxy.ProxyId = broadPhase.AddProxy(ref proxy);

                Proxies[i] = proxy;
            }
        }

        internal void DestroyProxies(IBroadPhase broadPhase)
        {
            // Destroy proxies in the broad-phase.
            for (int i = 0; i < ProxyCount; ++i)
            {
                broadPhase.RemoveProxy(Proxies[i].ProxyId);
                Proxies[i].ProxyId = -1;
            }

            ProxyCount = 0;
        }

        internal void Synchronize(IBroadPhase broadPhase, ref Transform transform1, ref Transform transform2)
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
                Shape.ComputeAABB(ref transform1, proxy.ChildIndex, out aabb1);
                Shape.ComputeAABB(ref transform2, proxy.ChildIndex, out aabb2);

                proxy.AABB.Combine(ref aabb1, ref aabb2);

                Vector2 displacement = transform2.p - transform1.p;

                broadPhase.MoveProxy(proxy.ProxyId, ref proxy.AABB, displacement);
            }
        }
    }
}