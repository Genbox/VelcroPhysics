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

using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace FarseerPhysics
{
    /// <summary>
    /// A fixture is used to attach a shape to a body for collision detection. A fixture
    /// inherits its transform from its parent. Fixtures hold additional non-geometric data
    /// such as friction, collision filters, etc.
    /// Fixtures are created via Body.CreateFixture.
    /// @warning you cannot reuse fixtures.
    /// </summary>
    public class Fixture
    {
        public Fixture()
        {
            ProxyId = BroadPhase.NullProxy;

            //Fixture defaults
            _friction = 0.2f;
            _categoryBits = 0x0001;
            _maskBits = 0xFFFF;
            _isSensor = false;
        }

        /// <summary>
        /// Get the type of the child shape. You can use this to down cast to the concrete shape.
        /// @return the shape type.
        /// </summary>
        /// <value>The type of the shape.</value>
        public ShapeType ShapeType
        {
            get { return _shape.ShapeType; }
        }

        /// <summary>
        /// Get the child shape. You can modify the child shape, however you should not change the
        /// number of vertices because this will crash some collision caching mechanisms.
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
        public bool Sensor
        {
            set
            {
                if (_isSensor == value)
                {
                    return;
                }

                _isSensor = value;

                if (_body == null)
                {
                    return;
                }

                ContactEdge edge = _body.ContactList;
                while (edge != null)
                {
                    Contact contact = edge.Contact;
                    Fixture fixtureA = contact.GetFixtureA();
                    Fixture fixtureB = contact.GetFixtureB();
                    if (fixtureA == this || fixtureB == this)
                    {
                        contact.SetSensor(fixtureA.Sensor || fixtureB.Sensor);
                    }

                    edge = edge.Next;
                }
            }
            get
            {
                return _isSensor;
            }
        }

        /// <summary>
        /// Collision groups allow a certain group of objects to never collide (negative)
        /// or always collide (positive). Zero means no collision group. Non-zero group
        /// filtering always wins against the mask bits.
        /// Warning: The filter will not take effect until next step.
        /// </summary>
        public short GroupIndex
        {
            set
            {
                if (_body == null)
                    return;

                if (_groupIndex == value)
                    return;

                _groupIndex = value;
                FilterChanged();
            }
            get
            {
                return _groupIndex;
            }
        }

        /// <summary>
        /// The collision mask bits. This states the categories that this
        /// shape would accept for collision.
        /// </summary>
        public ushort MaskBits
        {
            get
            {
                return _maskBits;
            }

            set
            {
                if (_body == null)
                    return;

                if (_maskBits == value)
                    return;

                _maskBits = value;
                FilterChanged();
            }
        }

        /// <summary>
        /// The collision category bits. Normally you would just set one bit.
        /// </summary>
        public ushort CategoryBits
        {
            get
            {
                return _categoryBits;
            }

            set
            {
                if (_body == null)
                    return;

                if (_categoryBits == value)
                    return;

                _categoryBits = value;
                FilterChanged();
            }
        }

        private void FilterChanged()
        {
            // Flag associated contacts for filtering.
            ContactEdge edge = _body.ContactList;
            while (edge != null)
            {
                Contact contact = edge.Contact;
                Fixture fixtureA = contact.GetFixtureA();
                Fixture fixtureB = contact.GetFixtureB();
                if (fixtureA == this || fixtureB == this)
                {
                    contact.FlagForFiltering();
                }

                edge = edge.Next;
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
        /// Get the next fixture in the parent body's fixture list.
        /// </summary>
        /// <value>the next shape.</value>
        public Fixture NextFixture
        {
            get { return _next; }
        }

        /// <summary>
        /// Set the user data. Use this to store your application specific data.
        /// </summary>
        /// <value>The data.</value>
        public object UserData
        {
            set { _userData = value; }
            get { return _userData; }
        }

        public int ProxyId
        {
            get;
            private set;
        }

        /// <summary>
        /// Test a point for containment in this fixture.
        /// </summary>
        /// <param name="p">a point in world coordinates.</param>
        /// <returns></returns>
        public bool TestPoint(Vector2 p)
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
        public bool RayCast(out RayCastOutput output, ref RayCastInput input)
        {
            Transform xf;
            _body.GetTransform(out xf);
            return _shape.RayCast(out output, ref input, ref xf);
        }

        /// <summary>
        /// Set the coefficient of friction.
        /// </summary>
        /// <value>The friction.</value>
        public float Friction
        {
            set { _friction = value; }
            get { return _friction; }
        }

        /// <summary>
        /// Get the coefficient of restitution.
        /// </summary>
        /// <value></value>
        public float Restitution
        {
            get { return _restitution; }
            set { _restitution = value; }
        }

        /// <summary>
        /// Get the fixture's AABB. This AABB may be enlarge and/or stale.
        /// If you need a more accurate AABB, compute it using the shape and
        /// the body transform.
        /// </summary>
        /// <param name="aabb">The aabb.</param>
        public void GetAABB(out AABB aabb)
        {
            aabb = _aabb;
        }

        /// <summary>
        /// We need separation create/destroy functions from the constructor/destructor because
        /// the destructor cannot access the allocator or broad-phase (no destructor arguments allowed by C++).
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="shape">The shape.</param>
        internal void Create(Body body, Shape shape)
        {
            _body = body;
            _next = null;

            _shape = shape.Clone();
        }

        internal void Destroy()
        {
            // The proxy must be destroyed before calling this.
            Debug.Assert(ProxyId == BroadPhase.NullProxy);

            _shape = null;
        }

        // These support body activation/deactivation.
        internal void CreateProxy(BroadPhase broadPhase, ref Transform xf)
        {
            Debug.Assert(ProxyId == BroadPhase.NullProxy);

            // Create proxy in the broad-phase.
            _shape.ComputeAABB(out _aabb, ref xf);
            ProxyId = broadPhase.CreateProxy(ref _aabb, this);
        }

        internal void DestroyProxy(BroadPhase broadPhase)
        {
            if (ProxyId == BroadPhase.NullProxy)
            {
                return;
            }

            // Destroy proxy in the broad-phase.
            broadPhase.DestroyProxy(ProxyId);
            ProxyId = BroadPhase.NullProxy;
        }

        internal void Synchronize(BroadPhase broadPhase, ref Transform transform1, ref Transform transform2)
        {
            if (ProxyId == BroadPhase.NullProxy)
            {
                return;
            }

            // Compute an AABB that covers the swept shape (may miss some rotation effect).
            AABB aabb1, aabb2;
            _shape.ComputeAABB(out aabb1, ref transform1);
            _shape.ComputeAABB(out aabb2, ref transform2);

            _aabb.Combine(ref aabb1, ref aabb2);

            Vector2 displacement = transform2.Position - transform1.Position;

            broadPhase.MoveProxy(ProxyId, ref _aabb, displacement);
        }

        internal AABB _aabb;
        internal Fixture _next;
        internal Body _body;
        private Shape _shape;
        private float _friction;
        private float _restitution;
        private bool _isSensor;
        private object _userData;
        private short _groupIndex;
        private ushort _maskBits;
        private ushort _categoryBits;
    }
}
