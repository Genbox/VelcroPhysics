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
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics
{
    /// <summary>
    /// The body type.
    /// </summary>
    public enum BodyType
    {
        /// <summary>
        /// zero mass, zero velocity, may be manually moved
        /// </summary>
        Static,
        /// <summary>
        /// zero mass, non-zero velocity set by user, moved by solver
        /// </summary>
        Kinematic,
        /// <summary>
        ///  positive mass, non-zero velocity determined by forces, moved by solver
        /// </summary>
        Dynamic,
    }

    [Flags]
    public enum BodyFlags
    {
        None = 0,
        Island = (1 << 0),
        Awake = (1 << 1),
        AutoSleep = (1 << 2),
        Bullet = (1 << 3),
        FixedRotation = (1 << 4),
        Active = (1 << 5),
    }

    public class Body
    {
        private float _I;
        internal float _angularDamping;
        internal float _angularVelocity;
        private BodyType _bodyType;
        internal ContactEdge _contactList;
        internal int _fixtureCount;
        internal Fixture _fixtureList;
        internal BodyFlags _flags;
        internal Vector2 _force;
        internal float _invI;
        internal float _invMass;
        internal JointEdge _jointList;
        internal Vector2 _linearVelocity;
        internal float _mass;
        internal Body _next;
        internal Body _prev;
        internal float _sleepTime;
        internal Sweep _sweep;	// the swept motion for CCD
        internal float _torque;
        internal Transform _xf;		// the body origin transform

        public Body(World world)
        {
            World = world;

            AllowSleep = true;
            Awake = true;
            BodyType = BodyType.Static;
            Active = true;
            InertiaScale = 1;

            _xf.R.Set(0);

            _sweep.TimeInt0 = 1.0f;
        }

        internal Body(Body body, World world)
        {
            Debug.Assert(body.Position.IsValid());
            Debug.Assert(body.LinearVelocity.IsValid());
            Debug.Assert(MathUtils.IsValid(body.Rotation));
            Debug.Assert(MathUtils.IsValid(body.AngularVelocity));
            Debug.Assert(MathUtils.IsValid(body.InertiaScale) && body.InertiaScale >= 0.0f);
            Debug.Assert(MathUtils.IsValid(body.AngularDamping) && body.AngularDamping >= 0.0f);
            Debug.Assert(MathUtils.IsValid(body.LinearDamping) && body.LinearDamping >= 0.0f);

            Bullet = body.Bullet;
            FixedRotation = body.FixedRotation;
            AllowSleep = body.AllowSleep;
            Awake = body.Awake;
            Active = body.Active;

            World = world;

            _xf.Position = body.Position;
            _xf.R.Set(body.Rotation);

            _sweep.TimeInt0 = 1.0f;
            _sweep.Angle0 = _sweep.Angle = body.Rotation;
            _sweep.Center0 = _sweep.Center = MathUtils.Multiply(ref _xf, _sweep.LocalCenter);

            _linearVelocity = body.LinearVelocity;
            _angularVelocity = body.AngularVelocity;

            LinearDamping = body.LinearDamping;
            _angularDamping = body.AngularDamping;

            _bodyType = body.BodyType;

            if (BodyType == BodyType.Dynamic)
            {
                _mass = 1.0f;
                _invMass = 1.0f;
            }

            InertiaScale = body.InertiaScale;
            UserData = body.UserData;
        }

        /// <summary>
        /// Set this flag to false if this body should never fall asleep. Note that
        /// this increases CPU usage.
        /// </summary>
        public bool AllowSleep
        {
            get { return (_flags & BodyFlags.AutoSleep) == BodyFlags.AutoSleep; }
            set
            {
                if (value)
                    _flags |= BodyFlags.AutoSleep;
                else
                    _flags &= ~BodyFlags.AutoSleep;
            }
        }

        /// <summary>
        /// Set the type of this body. This may alter the mass and velocity.
        /// </summary>
        /// <value>The type.</value>
        public BodyType BodyType
        {
            set
            {
                if (_bodyType == value)
                {
                    return;
                }

                _bodyType = value;

                ResetMassData();

                if (_bodyType == BodyType.Static)
                {
                    _linearVelocity = Vector2.Zero;
                    _angularVelocity = 0.0f;
                }

                Awake = true;

                _force = Vector2.Zero;
                _torque = 0.0f;

                // Since the body type changed, we need to flag contacts for filtering.
                for (ContactEdge ce = _contactList; ce != null; ce = ce.Next)
                {
                    ce.Contact.FlagForFiltering();
                }
            }
            get { return _bodyType; }
        }

        /// <summary>
        /// Get the world position of the center of mass.
        /// </summary>
        /// <value></value>
        public Vector2 WorldCenter
        {
            get { return _sweep.Center; }
        }

        /// <summary>
        /// Get the local position of the center of mass.
        /// </summary>
        /// <value></value>
        public Vector2 LocalCenter
        {
            get { return _sweep.LocalCenter; }
        }

        /// <summary>
        /// The linear velocity of the body's center of mass in world co-ordinates.
        /// </summary>
        /// <value>
        ///   the linear velocity of the center of mass.
        /// </value>
        public Vector2 LinearVelocity
        {
            get { return _linearVelocity; }
            set
            {
                if (_bodyType == BodyType.Static)
                {
                    return;
                }

                _linearVelocity = value;
            }
        }

        /// <summary>
        /// Get the angular velocity.
        /// </summary>
        /// <value>
        ///   angular velocity in radians/second.
        /// </value>
        public float AngularVelocity
        {
            get { return _angularVelocity; }
            set
            {
                if (_bodyType == BodyType.Static)
                {
                    return;
                }

                _angularVelocity = value;
            }
        }

        /// <summary>
        /// Get the total mass of the body. It returns the mass of all attached shapes.
        /// </summary>
        /// <value>
        ///   the mass, usually in kilograms (kg).
        /// </value>
        public float Mass
        {
            get { return _mass; }
        }

        /// <summary>
        /// Get the central rotational inertia of the body.
        /// </summary>
        /// <value>
        ///   the rotational inertia, usually in kg-m^2.
        /// </value>
        public float Inertia
        {
            get { return _I; }
        }

        /// <summary>
        /// Experimental: scales the inertia tensor.
        /// </summary>
        public float InertiaScale
        {
            get; set;
        }

        /// <summary>
        /// Linear damping is use to reduce the linear velocity. The damping parameter
        /// can be larger than 1.0f but the damping effect becomes sensitive to the
        /// time step when the damping parameter is large.
        /// </summary>
        public float LinearDamping { get; set; }

        /// <summary>
        /// Angular damping is use to reduce the angular velocity. The damping parameter
        /// can be larger than 1.0f but the damping effect becomes sensitive to the
        /// time step when the damping parameter is large.
        /// </summary>
        public float AngularDamping
        {
            set { _angularDamping = value; }
            get { return _angularDamping; }
        }

        /// <summary>
        /// Is this a fast moving body that should be prevented from tunneling through
        /// other moving bodies? Note that all bodies are prevented from tunneling through
        /// static bodies.
        /// @warning You should use this flag sparingly since it increases processing time.
        /// </summary>
        public bool Bullet
        {
            set
            {
                if (value)
                {
                    _flags |= BodyFlags.Bullet;
                }
                else
                {
                    _flags &= ~BodyFlags.Bullet;
                }
            }
            get
            {
                return (_flags & BodyFlags.Bullet) == BodyFlags.Bullet;
            }
        }

        /// <summary>
        /// Is this body awake or sleeping?
        /// </summary>
        /// <value>
        ///   &lt;c&gt;true&lt;/c&gt; if this instance is awake; otherwise, &lt;c&gt;false&lt;/c&gt;.
        /// </value>
        public bool Awake
        {
            get { return (_flags & BodyFlags.Awake) == BodyFlags.Awake; }
            set
            {
                if (value)
                {
                    _flags |= BodyFlags.Awake;
                }
                else
                {
                    _flags &= ~BodyFlags.Awake;
                    _sleepTime = 0.0f;
                    _linearVelocity = Vector2.Zero;
                    _angularVelocity = 0.0f;
                    _force = Vector2.Zero;
                    _torque = 0.0f;
                }
            }
        }

        /// <summary>
        /// Get the active state of the body.
        /// </summary>
        /// <value>
        ///   &lt;c&gt;true&lt;/c&gt; if this instance is active; otherwise, &lt;c&gt;false&lt;/c&gt;.
        /// </value>
        public bool Active
        {
            get { return (_flags & BodyFlags.Active) == BodyFlags.Active; }
            set
            {
                if (value == Active)
                {
                    return;
                }

                if (value)
                {
                    _flags |= BodyFlags.Active;

                    // Create all proxies.
                    BroadPhase broadPhase = World.ContactManager._broadPhase;
                    for (Fixture f = _fixtureList; f != null; f = f._next)
                    {
                        f.CreateProxy(broadPhase, ref _xf);
                    }

                    // Contacts are created the next time step.
                }
                else
                {
                    _flags &= ~BodyFlags.Active;

                    // Destroy all proxies.
                    BroadPhase broadPhase = World.ContactManager._broadPhase;
                    for (Fixture f = _fixtureList; f != null; f = f._next)
                    {
                        f.DestroyProxy(broadPhase);
                    }

                    // Destroy the attached contacts.
                    ContactEdge ce = _contactList;
                    while (ce != null)
                    {
                        ContactEdge ce0 = ce;
                        ce = ce.Next;
                        World.ContactManager.Destroy(ce0.Contact);
                    }
                    _contactList = null;
                }
            }
        }

        /// <summary>
        /// Should this body be prevented from rotating? Useful for characters.
        /// </summary>
        /// <value>
        ///   &lt;c&gt;true&lt;/c&gt; if [is fixed rotation]; otherwise, &lt;c&gt;false&lt;/c&gt;.
        /// </value>
        public bool FixedRotation
        {
            get { return (_flags & BodyFlags.FixedRotation) == BodyFlags.FixedRotation; }
            set
            {
                if (value)
                {
                    _flags |= BodyFlags.FixedRotation;
                }
                else
                {
                    _flags &= ~BodyFlags.FixedRotation;
                }

                ResetMassData();
            }
        }

        public Fixture FixtureList
        {
            get { return _fixtureList; }
        }

        /// <summary>
        /// Get the list of all joints attached to this body.
        /// </summary>
        /// <value></value>
        public JointEdge JointList
        {
            get { return _jointList; }
        }

        /// <summary>
        /// Get the list of all contacts attached to this body.
        /// @warning this list changes during the time step and you may
        /// miss some collisions if you don't use ContactListener.
        /// </summary>
        /// <value></value>
        public ContactEdge ContactList
        {
            get { return _contactList; }
        }

        /// <summary>
        /// Get the next body in the world's body list.
        /// </summary>
        /// <value></value>
        public Body NextBody
        {
            get { return _next; }
        }

        /// <summary>
        /// Set the user data. Use this to store your application specific data.
        /// </summary>
        /// <value>The data.</value>
        public object UserData { set; get; }

        /// <summary>
        /// Get the parent world of this body.
        /// </summary>
        /// <value></value>
        public World World { get; private set; }

        /// <summary>
        /// The world position of the body. Avoid creating bodies at the origin
        /// since this can lead to many overlapping shapes.
        /// </summary>
        /// <returns>Return the world position of the body's origin.</returns>
        public Vector2 Position
        {
            get
            {
                return _xf.Position;
            }
            set
            {
                SetTransform(value, Rotation);
            }
        }

        /// <summary>
        /// Get the angle in radians.
        /// </summary>
        /// <returns>Return the current world rotation angle in radians.</returns>
        public float Rotation
        {
            get
            {
                return _sweep.Angle;
            }
            set
            {
                SetTransform(Position, value);
            }
        }

        /// <summary>
        /// Creates a fixture and attach it to this body. Use this function if you need
        /// to set some fixture parameters, like friction. Otherwise you can create the
        /// fixture directly from a shape.
        /// If the density is non-zero, this function automatically updates the mass of the body.
        /// Contacts are not created until the next time step.
        /// @warning This function is locked during callbacks.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <returns></returns>
        public Fixture CreateFixture(Shape shape)
        {
            Debug.Assert(World.Locked == false);
            if (World.Locked)
                return null;

            Fixture fixture = new Fixture();
            fixture.Create(this, shape);

            if ((_flags & BodyFlags.Active) == BodyFlags.Active)
            {
                BroadPhase broadPhase = World.ContactManager._broadPhase;
                fixture.CreateProxy(broadPhase, ref _xf);
            }

            fixture._next = _fixtureList;
            _fixtureList = fixture;
            ++_fixtureCount;

            // Adjust mass properties if needed.
            if (fixture.Shape.Density > 0.0f)
            {
                ResetMassData();
            }

            // Let the world know we have a new fixture. This will cause new contacts
            // to be created at the beginning of the next time step.
            World.Flags |= WorldFlags.NewFixture;

            return fixture;
        }

        /// <summary>
        /// Destroy a fixture. This removes the fixture from the broad-phase and
        /// destroys all contacts associated with this fixture. This will	
        /// automatically adjust the mass of the body if the body is dynamic and the
        /// fixture has positive density.
        /// All fixtures attached to a body are implicitly destroyed when the body is destroyed.
        /// @warning This function is locked during callbacks.
        /// </summary>
        /// <param name="fixture"> the fixture to be removed.</param>
        public void DestroyFixture(Fixture fixture)
        {
            Debug.Assert(World.Locked == false);
            if (World.Locked)
            {
                return;
            }

#if DEBUG
            Debug.Assert(fixture._body == this);

            // Remove the fixture from this body's singly linked list.
            Debug.Assert(_fixtureCount > 0);

            Fixture node = _fixtureList;
            bool found = false;
            while (node != null)
            {
                if (node == fixture)
                {
                    _fixtureList = fixture._next;
                    found = true;
                    break;
                }

                node = node._next;
            }

            // You tried to remove a shape that is not attached to this body.
            Debug.Assert(found);
#endif

            // Destroy any contacts associated with the fixture.
            ContactEdge edge = _contactList;
            while (edge != null)
            {
                Contact c = edge.Contact;
                edge = edge.Next;

                Fixture fixtureA = c.FixtureA;
                Fixture fixtureB = c.FixtureB;

                if (fixture == fixtureA || fixture == fixtureB)
                {
                    // This destroys the contact and removes it from
                    // this body's contact list.
                    World.ContactManager.Destroy(c);
                }
            }

            if ((_flags & BodyFlags.Active) == BodyFlags.Active)
            {
                Debug.Assert(fixture.ProxyId != BroadPhase.NullProxy);

                BroadPhase broadPhase = World.ContactManager._broadPhase;
                fixture.DestroyProxy(broadPhase);
            }
            else
            {
                Debug.Assert(fixture.ProxyId == BroadPhase.NullProxy);
            }

            fixture.Destroy();
            fixture._body = null;
            fixture._next = null;

            --_fixtureCount;

            ResetMassData();
        }

        /// <summary>
        /// Set the position of the body's origin and rotation.
        /// This breaks any contacts and wakes the other bodies.
        /// </summary>
        /// <param name="position">the world position of the body's local origin.</param>
        /// <param name="angle">the world rotation in radians.</param>
        public void SetTransform(Vector2 position, float angle)
        {
            Debug.Assert(World.Locked == false);
            if (World.Locked)
            {
                return;
            }

            _xf.R.Set(angle);
            _xf.Position = position;

            _sweep.Center0 = _sweep.Center = MathUtils.Multiply(ref _xf, _sweep.LocalCenter);
            _sweep.Angle0 = _sweep.Angle = angle;

            BroadPhase broadPhase = World.ContactManager._broadPhase;
            for (Fixture f = _fixtureList; f != null; f = f._next)
            {
                f.Synchronize(broadPhase, ref _xf, ref _xf);
            }

            World.ContactManager.FindNewContacts();
        }

        /// <summary>
        /// Get the body transform for the body's origin.
        /// </summary>
        /// <param name="xf">the world transform of the body's origin.</param>
        public void GetTransform(out Transform xf)
        {
            xf = _xf;
        }

        /// <summary>
        /// Get the angle in radians.
        /// </summary>
        /// <returns>current world rotation angle in radians.</returns>
        public float GetAngle()
        {
            return _sweep.Angle;
        }

        /// <summary>
        /// Apply a force at a world point. If the force is not
        /// applied at the center of mass, it will generate a torque and
        /// affect the angular velocity. This wakes up the body.
        /// </summary>
        /// <param name="force">the world force vector, usually in Newtons (N).</param>
        /// <param name="point">the world position of the point of application.</param>
        public void ApplyForce(Vector2 force, Vector2 point)
        {
            if (_bodyType != BodyType.Dynamic)
            {
                return;
            }

            if (Awake == false)
            {
                Awake = true;
            }

            _force += force;
            _torque += MathUtils.Cross(point - _sweep.Center, force);
        }

        /// <summary>
        /// Apply a torque. This affects the angular velocity
        /// without affecting the linear velocity of the center of mass.
        /// This wakes up the body.
        /// </summary>
        /// <param name="torque">about the z-axis (out of the screen), usually in N-m.</param>
        public void ApplyTorque(float torque)
        {
            if (_bodyType != BodyType.Dynamic)
            {
                return;
            }

            if (Awake == false)
            {
                Awake = true;
            }

            _torque += torque;
        }

        /// <summary>
        /// Apply an impulse at a point. This immediately modifies the velocity.
        /// It also modifies the angular velocity if the point of application
        /// is not at the center of mass. This wakes up the body.
        /// </summary>
        /// <param name="impulse">the world impulse vector, usually in N-seconds or kg-m/s.</param>
        /// <param name="point">the world position of the point of application.</param>
        public void ApplyImpulse(Vector2 impulse, Vector2 point)
        {
            if (_bodyType != BodyType.Dynamic)
            {
                return;
            }

            if (Awake == false)
            {
                Awake = true;
            }

            _linearVelocity += _invMass * impulse;
            _angularVelocity += _invI * MathUtils.Cross(point - _sweep.Center, impulse);
        }

        /// <summary>
        /// This resets the mass properties to the sum of the mass properties of the fixtures.
        /// This normally does not need to be called unless you called SetMassData to override
        /// the mass and you later want to reset the mass.
        /// </summary>
        public void ResetMassData()
        {
            // Compute mass data from shapes. Each shape has its own density.
            _mass = 0.0f;
            _invMass = 0.0f;
            _I = 0.0f;
            _invI = 0.0f;
            _sweep.LocalCenter = Vector2.Zero;

            // Static and kinematic bodies have zero mass.
            if (_bodyType == BodyType.Static || _bodyType == BodyType.Kinematic)
            {
                return;
            }

            Debug.Assert(_bodyType == BodyType.Dynamic);

            // Accumulate mass over all fixtures.
            Vector2 center = Vector2.Zero;
            for (Fixture f = _fixtureList; f != null; f = f._next)
            {
                if (f.Shape.Density == 0.0f)
                {
                    continue;
                }

                MassData massData = f.Shape.MassData;
                _mass += massData.Mass;
                center += massData.Mass * massData.Center;
                _I += massData.Inertia;
            }

            // Compute center of mass.
            if (_mass > 0.0f)
            {
                _invMass = 1.0f / _mass;
                center *= _invMass;
            }
            else
            {
                // Force all dynamic bodies to have a positive mass.
                _mass = 1.0f;
                _invMass = 1.0f;
            }

            if (_I > 0.0f && (_flags & BodyFlags.FixedRotation) == 0)
            {
                // Center the inertia about the center of mass.
                _I -= _mass * Vector2.Dot(center, center);
                _I *= InertiaScale;

                Debug.Assert(_I > 0.0f);
                _invI = 1.0f / _I;
            }
            else
            {
                _I = 0.0f;
                _invI = 0.0f;
            }

            // Move center of mass.
            Vector2 oldCenter = _sweep.Center;
            _sweep.LocalCenter = center;
            _sweep.Center0 = _sweep.Center = MathUtils.Multiply(ref _xf, _sweep.LocalCenter);

            // Update center of mass velocity.
            _linearVelocity += MathUtils.Cross(_angularVelocity, _sweep.Center - oldCenter);
        }

        /// <summary>
        /// Get the world coordinates of a point given the local coordinates.
        /// </summary>
        /// <param name="localPoint">a point on the body measured relative the the body's origin.</param>
        /// <returns>the same point expressed in world coordinates.</returns>
        public Vector2 GetWorldPoint(Vector2 localPoint)
        {
            return MathUtils.Multiply(ref _xf, localPoint);
        }

        /// <summary>
        /// Get the world coordinates of a vector given the local coordinates.
        /// </summary>
        /// <param name="localVector">a vector fixed in the body.</param>
        /// <returns> the same vector expressed in world coordinates.</returns>
        public Vector2 GetWorldVector(Vector2 localVector)
        {
            return MathUtils.Multiply(ref _xf.R, localVector);
        }

        /// <summary>
        /// Gets a local point relative to the body's origin given a world point.
        /// </summary>
        /// <param name="worldPoint">a point in world coordinates.</param>
        /// <returns>the corresponding local point relative to the body's origin.</returns>
        public Vector2 GetLocalPoint(Vector2 worldPoint)
        {
            return MathUtils.MultiplyT(ref _xf, worldPoint);
        }

        /// <summary>
        /// Gets a local vector given a world vector.
        /// </summary>
        /// <param name="worldVector">a vector in world coordinates.</param>
        /// <returns>the corresponding local vector.</returns>
        public Vector2 GetLocalVector(Vector2 worldVector)
        {
            return MathUtils.MultiplyT(ref _xf.R, worldVector);
        }

        /// <summary>
        /// Get the world linear velocity of a world point attached to this body.
        /// </summary>
        /// <param name="worldPoint">a point in world coordinates.</param>
        /// <returns>the world velocity of a point.</returns>
        public Vector2 GetLinearVelocityFromWorldPoint(Vector2 worldPoint)
        {
            return _linearVelocity + MathUtils.Cross(_angularVelocity, worldPoint - _sweep.Center);
        }

        /// <summary>
        /// Get the world velocity of a local point.
        /// </summary>
        /// <param name="localPoint">a point in local coordinates.</param>
        /// <returns>the world velocity of a point.</returns>
        public Vector2 GetLinearVelocityFromLocalPoint(Vector2 localPoint)
        {
            return GetLinearVelocityFromWorldPoint(GetWorldPoint(localPoint));
        }

        internal void SynchronizeFixtures()
        {
            Transform xf1 = new Transform();
            xf1.R.Set(_sweep.Angle0);
            xf1.Position = _sweep.Center0 - MathUtils.Multiply(ref xf1.R, _sweep.LocalCenter);

            BroadPhase broadPhase = World.ContactManager._broadPhase;
            for (Fixture f = _fixtureList; f != null; f = f._next)
            {
                f.Synchronize(broadPhase, ref xf1, ref _xf);
            }
        }

        internal void SynchronizeTransform()
        {
            _xf.R.Set(_sweep.Angle);
            _xf.Position = _sweep.Center - MathUtils.Multiply(ref _xf.R, _sweep.LocalCenter);
        }

        /// <summary>
        // This is used to prevent connected bodies from colliding.
        // It may lie, depending on the collideConnected flag.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        internal bool ShouldCollide(Body other)
        {
            // At least one body should be dynamic.
            if (_bodyType != BodyType.Dynamic && other._bodyType != BodyType.Dynamic)
            {
                return false;
            }

            // Does a joint prevent collision?
            for (JointEdge jn = _jointList; jn != null; jn = jn.Next)
            {
                if (jn.Other == other)
                {
                    if (jn.Joint.CollideConnected == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal void Advance(float t)
        {
            // Advance to the new safe time.
            _sweep.Advance(t);
            _sweep.Center = _sweep.Center0;
            _sweep.Angle = _sweep.Angle0;
            SynchronizeTransform();
        }
    }
}
