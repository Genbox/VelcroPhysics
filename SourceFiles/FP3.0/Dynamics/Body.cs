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
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics
{
    /// The body type.
    /// static: zero mass, zero velocity, may be manually moved
    /// kinematic: zero mass, non-zero velocity set by user, moved by solver
    /// dynamic: positive mass, non-zero velocity determined by forces, moved by solver
    public enum BodyType
    {
        Static,
        Kinematic,
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
        Toi = (1 << 6),
        IgnoreGravity = (1 << 7),
    }

    public class Body
    {
        internal float _I;
        internal float _angularDamping;
        internal float _angularVelocity;
        internal ContactEdge _contactList;
        internal int _fixtureCount;
        public Fixture _fixtureList;
        internal BodyFlags _flags;
        internal Vector2 _force;
        internal float _invI;
        internal float _invMass;
        internal JointEdge _jointList;
        internal float _linearDamping;
        internal Vector2 _linearVelocity;
        internal float _mass;
        internal Body _next;
        internal Body _prev;
        internal float _sleepTime;
        internal Sweep _sweep; // the swept motion for CCD
        internal float _torque;
        internal BodyType _type;
        internal object _userData;
        internal World _world;
        internal Transform _xf; // the body origin transform

        internal Body(World world)
        {
            _world = world;

            FixedRotation = false;
            IsBullet = false;
            SleepingAllowed = true;
            Awake = true;
            BodyType = BodyType.Static;
            Active = true;

            _xf.R.Set(0);
        }

        /// Get the type of this body.
        public BodyType BodyType
        {
            get { return _type; }
            set
            {
                if (_type == value)
                {
                    return;
                }

                _type = value;

                ResetMassData();

                if (_type == BodyType.Static)
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
        }

        /// Set the linear velocity of the center of mass.
        /// @param v the new linear velocity of the center of mass.
        public Vector2 LinearVelocity
        {
            set
            {
                if (_type == BodyType.Static)
                {
                    return;
                }

                if (Vector2.Dot(value, value) > 0.0f)
                {
                    Awake = true;
                }

                _linearVelocity = value;
            }
            get { return _linearVelocity; }
        }

        /// Set the angular velocity.
        /// @param omega the new angular velocity in radians/second.
        public float AngularVelocity
        {
            set
            {
                if (_type == BodyType.Static)
                {
                    return;
                }

                if (value * value > 0.0f)
                {
                    Awake = true;
                }

                _angularVelocity = value;
            }
            get { return _angularVelocity; }
        }

        /// Get the total mass of the body.
        /// @return the mass, usually in kilograms (kg).
        public float Mass
        {
            get { return _mass; }
        }

        /// Get the rotational inertia of the body about the local origin.
        /// @return the rotational inertia, usually in kg-m^2.
        public float Inertia
        {
            get { return _I + _mass * Vector2.Dot(_sweep.localCenter, _sweep.localCenter); }
        }

        /// Get the linear damping of the body.
        public float LinearDamping
        {
            get { return _linearDamping; }
            set { _linearDamping = value; }
        }

        /// Get the angular damping of the body.
        public float AngularDamping
        {
            get { return _angularDamping; }
            set { _angularDamping = value; }
        }

        /// Should this body be treated like a bullet for continuous collision detection?
        public bool IsBullet
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
            get { return (_flags & BodyFlags.Bullet) == BodyFlags.Bullet; }
        }

        /// You can disable sleeping on this body. If you disable sleeping, the
        /// body will be woken.
        public bool SleepingAllowed
        {
            set
            {
                if (value)
                {
                    _flags |= BodyFlags.AutoSleep;
                }
                else
                {
                    _flags &= ~BodyFlags.AutoSleep;
                    Awake = true;
                }
            }
            get { return (_flags & BodyFlags.AutoSleep) == BodyFlags.AutoSleep; }
        }


        /// Set the sleep state of the body. A sleeping body has very
        /// low CPU cost.
        /// @param flag set to true to put body to sleep, false to wake it.
        public bool Awake
        {
            set
            {
                if (value)
                {
                    if ((_flags & BodyFlags.Awake) == 0)
                    {
                        _flags |= BodyFlags.Awake;
                        _sleepTime = 0.0f;
                    }
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
            get { return (_flags & BodyFlags.Awake) == BodyFlags.Awake; }
        }

        /// Set the active state of the body. An inactive body is not
        /// simulated and cannot be collided with or woken up.
        /// If you pass a flag of true, all fixtures will be added to the
        /// broad-phase.
        /// If you pass a flag of false, all fixtures will be removed from
        /// the broad-phase and all contacts will be destroyed.
        /// Fixtures and joints are otherwise unaffected. You may continue
        /// to create/destroy fixtures and joints on inactive bodies.
        /// Fixtures on an inactive body are implicitly inactive and will
        /// not participate in collisions, ray-casts, or queries.
        /// Joints connected to an inactive body are implicitly inactive.
        /// An inactive body is still owned by a b2World object and remains
        /// in the body list.
        public bool Active
        {
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
                    BroadPhase broadPhase = _world._contactManager.BroadPhase;
                    for (Fixture f = _fixtureList; f != null; f = f._next)
                    {
                        f.CreateProxies(broadPhase, ref _xf);
                    }

                    // Contacts are created the next time step.
                }
                else
                {
                    _flags &= ~BodyFlags.Active;

                    // Destroy all proxies.
                    BroadPhase broadPhase = _world._contactManager.BroadPhase;
                    for (Fixture f = _fixtureList; f != null; f = f._next)
                    {
                        f.DestroyProxies(broadPhase);
                    }

                    // Destroy the attached contacts.
                    ContactEdge ce = _contactList;
                    while (ce != null)
                    {
                        ContactEdge ce0 = ce;
                        ce = ce.Next;
                        _world._contactManager.Destroy(ce0.Contact);
                    }
                    _contactList = null;
                }
            }
            get { return (_flags & BodyFlags.Active) == BodyFlags.Active; }
        }

        /// Set this body to have fixed rotation. This causes the mass
        /// to be reset.
        public bool FixedRotation
        {
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
            get { return (_flags & BodyFlags.FixedRotation) == BodyFlags.FixedRotation; }
        }

        public Fixture FixtureList
        {
            get { return _fixtureList; }
        }

        /// Get the list of all joints attached to this body.
        public JointEdge JointList
        {
            get { return _jointList; }
        }

        /// Get the list of all contacts attached to this body.
        /// @warning this list changes during the time step and you may
        /// miss some collisions if you don't use ContactListener.
        public ContactEdge ContactList
        {
            get { return _contactList; }
        }

        /// Set the user data. Use this to store your application specific data.
        public object UserData
        {
            set { _userData = value; }
            get { return _userData; }
        }

        /// <summary>
        /// Get the world body origin position.
        /// </summary>
        /// <returns>Return the world position of the body's origin.</returns>
        public Vector2 Position
        {
            get { return _xf.Position; }
            set { SetTransform(value, Rotation); }
        }

        /// <summary>
        /// Get the angle in radians.
        /// </summary>
        /// <returns>Return the current world rotation angle in radians.</returns>
        public float Rotation
        {
            get { return _sweep.a; }
            set { SetTransform(Position, value); }
        }

        public bool IsStatic
        {
            get { return _type == BodyType.Static; }
            set
            {
                if (value)
                    _type = BodyType.Static;
            }
        }

        public bool IgnoreGravity
        {
            get { return (_flags & BodyFlags.IgnoreGravity) == BodyFlags.IgnoreGravity; }
            set
            {
                if (value)
                    _flags |= BodyFlags.IgnoreGravity;
                else
                    _flags &= ~BodyFlags.IgnoreGravity;
            }
        }

        /// Creates a fixture and attach it to this body. Use this function if you need
        /// to set some fixture parameters, like friction. Otherwise you can create the
        /// fixture directly from a shape.
        /// If the density is non-zero, this function automatically updates the mass of the body.
        /// Contacts are not created until the next time step.
        /// @param def the fixture definition.
        /// @warning This function is locked during callbacks.
        public Fixture CreateFixture(Shape shape, float density)
        {
            Debug.Assert(_world.IsLocked == false);
            if (_world.IsLocked)
            {
                return null;
            }

            Fixture fixture = new Fixture();
            fixture.Create(this, shape, density);

            if ((_flags & BodyFlags.Active) == BodyFlags.Active)
            {
                BroadPhase broadPhase = _world._contactManager.BroadPhase;
                fixture.CreateProxies(broadPhase, ref _xf);
            }

            fixture._next = _fixtureList;
            _fixtureList = fixture;
            ++_fixtureCount;

            fixture._body = this;


            // Adjust mass properties if needed.
            if (fixture._density > 0.0f)
            {
                ResetMassData();
            }

            // Let the world know we have a new fixture. This will cause new contacts
            // to be created at the beginning of the next time step.
            _world._flags |= WorldFlags.NewFixture;

            return fixture;
        }

        public Fixture CreateFixture(Shape shape)
        {
            return CreateFixture(shape, 0);
        }

        /// Destroy a fixture. This removes the fixture from the broad-phase and
        /// destroys all contacts associated with this fixture. This will	
        /// automatically adjust the mass of the body if the body is dynamic and the
        /// fixture has positive density.
        /// All fixtures attached to a body are implicitly destroyed when the body is destroyed.
        /// @param fixture the fixture to be removed.
        /// @warning This function is locked during callbacks.
        public void DestroyFixture(Fixture fixture)
        {
            Debug.Assert(_world.IsLocked == false);
            if (_world.IsLocked)
            {
                return;
            }

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
                    _world._contactManager.Destroy(c);
                }
            }

            if ((_flags & BodyFlags.Active) == BodyFlags.Active)
            {
                Debug.Assert(fixture._proxyId != BroadPhase.NullProxy);

                BroadPhase broadPhase = _world._contactManager.BroadPhase;
                fixture.DestroyProxies(broadPhase);
            }

            fixture.Destroy();
            fixture._body = null;
            fixture._next = null;

            --_fixtureCount;

            ResetMassData();
        }

        /// Set the position of the body's origin and rotation.
        /// This breaks any contacts and wakes the other bodies.
        /// Manipulating a body's transform may cause non-physical behavior.
        /// @param position the world position of the body's local origin.
        /// @param angle the world rotation in radians.
        public void SetTransform(Vector2 position, float angle)
        {
            Debug.Assert(_world.IsLocked == false);
            if (_world.IsLocked)
            {
                return;
            }

            _xf.R.Set(angle);
            _xf.Position = position;

            _sweep.c0 = _sweep.c = MathUtils.Multiply(ref _xf, _sweep.localCenter);
            _sweep.a0 = _sweep.a = angle;

            BroadPhase broadPhase = _world._contactManager.BroadPhase;
            for (Fixture f = _fixtureList; f != null; f = f._next)
            {
                f.Synchronize(broadPhase, ref _xf, ref _xf);
            }

            _world._contactManager.FindNewContacts();
        }

        // For teleporting a body without considering new contacts immediately.
        public void SetTransformIgnoreContacts(Vector2 position, float angle)
        {
            Debug.Assert(_world.IsLocked == false);
            if (_world.IsLocked)
            {
                return;
            }

            _xf.R.Set(angle);
            _xf.Position = position;

            _sweep.c0 = _sweep.c = MathUtils.Multiply(ref _xf, _sweep.localCenter);
            _sweep.a0 = _sweep.a = angle;

            BroadPhase broadPhase = _world._contactManager.BroadPhase;
            for (Fixture f = _fixtureList; f != null; f = f._next)
            {
                f.Synchronize(broadPhase, ref _xf, ref _xf);
            }
        }

        /// Get the body transform for the body's origin.
        /// @return the world transform of the body's origin.
        public void GetTransform(out Transform xf)
        {
            xf = _xf;
        }

        /// Get the world body origin position.
        /// @return the world position of the body's origin.
        public Vector2 GetPosition()
        {
            return _xf.Position;
        }

        /// Get the angle in radians.
        /// @return the current world rotation angle in radians.
        public float GetAngle()
        {
            return _sweep.a;
        }

        /// Get the world position of the center of mass.
        public Vector2 GetWorldCenter()
        {
            return _sweep.c;
        }

        /// Get the local position of the center of mass.
        public Vector2 GetLocalCenter()
        {
            return _sweep.localCenter;
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
            ApplyForce(ref force, ref point);
        }

        public void ApplyForce(ref Vector2 force)
        {
            ApplyForce(ref force, ref _xf.Position);
        }

        public void ApplyForce(Vector2 force)
        {
            ApplyForce(ref force, ref _xf.Position);
        }

        /// Apply a force at a world point. If the force is not
        /// applied at the center of mass, it will generate a torque and
        /// affect the angular velocity. This wakes up the body.
        /// @param force the world force vector, usually in Newtons (N).
        /// @param point the world position of the point of application.
        public void ApplyForce(ref Vector2 force, ref Vector2 point)
        {
            if (_type == BodyType.Dynamic)
            {
                if (Awake == false)
                {
                    Awake = true;
                }

                _force += force;
                _torque += MathUtils.Cross(point - _sweep.c, force);
            }
        }

        /// Apply a torque. This affects the angular velocity
        /// without affecting the linear velocity of the center of mass.
        /// This wakes up the body.
        /// @param torque about the z-axis (out of the screen), usually in N-m.
        public void ApplyTorque(float torque)
        {
            if (_type == BodyType.Dynamic)
            {
                if (Awake == false)
                {
                    Awake = true;
                }

                _torque += torque;
            }
        }

        /// Apply an impulse at a point. This immediately modifies the velocity.
        /// It also modifies the angular velocity if the point of application
        /// is not at the center of mass. This wakes up the body.
        /// @param impulse the world impulse vector, usually in N-seconds or kg-m/s.
        /// @param point the world position of the point of application.
        public void ApplyLinearImpulse(Vector2 impulse, Vector2 point)
        {
            if (_type != BodyType.Dynamic)
            {
                return;
            }
            if (Awake == false)
            {
                Awake = true;
            }
            _linearVelocity += _invMass * impulse;
            _angularVelocity += _invI * MathUtils.Cross(point - _sweep.c, impulse);
        }

        /// Apply an angular impulse.  
        /// @param impulse the angular impulse in units of kg*m*m/s  
        public void ApplyAngularImpulse(float impulse)
        {
            if (_type != BodyType.Dynamic)
            {
                return;
            }

            if (Awake == false)
            {
                Awake = true;
            }

            _angularVelocity += _invI * impulse;
        }

        /// Get the mass data of the body.
        /// @return a struct containing the mass, inertia and center of the body.
        public void GetMassData(out MassData massData)
        {
            massData = new MassData();
            massData.Mass = _mass;
            massData.Inertia = _I + _mass * Vector2.Dot(_sweep.localCenter, _sweep.localCenter);
            massData.Center = _sweep.localCenter;
        }

        /// Set the mass properties to override the mass properties of the fixtures.
        /// Note that this changes the center of mass position.
        /// Note that creating or destroying fixtures can also alter the mass.
        /// This function has no effect if the body isn't dynamic.
        /// @param massData the mass properties.
        public void SetMassData(ref MassData massData)
        {
            Debug.Assert(_world.IsLocked == false);
            if (_world.IsLocked)
            {
                return;
            }

            if (_type != BodyType.Dynamic)
            {
                return;
            }

            _invMass = 0.0f;
            _I = 0.0f;
            _invI = 0.0f;

            _mass = massData.Mass;

            if (_mass <= 0.0f)
            {
                _mass = 1.0f;
            }

            _invMass = 1.0f / _mass;


            if (massData.Inertia > 0.0f && (_flags & BodyFlags.FixedRotation) == 0)
            {
                _I = massData.Inertia - _mass * Vector2.Dot(massData.Center, massData.Center);
                Debug.Assert(_I > 0.0f);
                _invI = 1.0f / _I;
            }

            // Move center of mass.
            Vector2 oldCenter = _sweep.c;
            _sweep.localCenter = massData.Center;
            _sweep.c0 = _sweep.c = MathUtils.Multiply(ref _xf, _sweep.localCenter);

            // Update center of mass velocity.
            _linearVelocity += MathUtils.Cross(_angularVelocity, _sweep.c - oldCenter);
        }

        /// This resets the mass properties to the sum of the mass properties of the fixtures.
        /// This normally does not need to be called unless you called SetMassData to override
        /// the mass and you later want to reset the mass.
        public void ResetMassData()
        {
            // Compute mass data from shapes. Each shape has its own density.
            _mass = 0.0f;
            _invMass = 0.0f;
            _I = 0.0f;
            _invI = 0.0f;
            _sweep.localCenter = Vector2.Zero;

            // Static and kinematic bodies have zero mass.
            if (_type == BodyType.Static || _type == BodyType.Kinematic)
            {
                _sweep.c0 = _sweep.c = _xf.Position;
                return;
            }

            Debug.Assert(_type == BodyType.Dynamic);

            // Accumulate mass over all fixtures.
            Vector2 center = Vector2.Zero;
            for (Fixture f = _fixtureList; f != null; f = f._next)
            {
                if (f._density == 0.0f)
                {
                    continue;
                }

                MassData massData;
                f.GetMassData(out massData);
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

                Debug.Assert(_I > 0.0f);
                _invI = 1.0f / _I;
            }
            else
            {
                _I = 0.0f;
                _invI = 0.0f;
            }

            // Move center of mass.
            Vector2 oldCenter = _sweep.c;
            _sweep.localCenter = center;
            _sweep.c0 = _sweep.c = MathUtils.Multiply(ref _xf, _sweep.localCenter);

            // Update center of mass velocity.
            _linearVelocity += MathUtils.Cross(_angularVelocity, _sweep.c - oldCenter);
        }

        /// Get the world coordinates of a point given the local coordinates.
        /// @param localPoint a point on the body measured relative the the body's origin.
        /// @return the same point expressed in world coordinates.
        public Vector2 GetWorldPoint(Vector2 localPoint)
        {
            return MathUtils.Multiply(ref _xf, localPoint);
        }

        /// Get the world coordinates of a vector given the local coordinates.
        /// @param localVector a vector fixed in the body.
        /// @return the same vector expressed in world coordinates.
        public Vector2 GetWorldVector(Vector2 localVector)
        {
            return MathUtils.Multiply(ref _xf.R, localVector);
        }

        /// Gets a local point relative to the body's origin given a world point.
        /// @param a point in world coordinates.
        /// @return the corresponding local point relative to the body's origin.
        public Vector2 GetLocalPoint(Vector2 worldPoint)
        {
            return MathUtils.MultiplyT(ref _xf, worldPoint);
        }

        /// Gets a local vector given a world vector.
        /// @param a vector in world coordinates.
        /// @return the corresponding local vector.
        public Vector2 GetLocalVector(Vector2 worldVector)
        {
            return MathUtils.MultiplyT(ref _xf.R, worldVector);
        }

        /// Get the world linear velocity of a world point attached to this body.
        /// @param a point in world coordinates.
        /// @return the world velocity of a point.
        public Vector2 GetLinearVelocityFromWorldPoint(Vector2 worldPoint)
        {
            return _linearVelocity + MathUtils.Cross(_angularVelocity, worldPoint - _sweep.c);
        }

        /// Get the world velocity of a local point.
        /// @param a point in local coordinates.
        /// @return the world velocity of a point.
        public Vector2 GetLinearVelocityFromLocalPoint(Vector2 localPoint)
        {
            return GetLinearVelocityFromWorldPoint(GetWorldPoint(localPoint));
        }

        /// Get the next body in the world's body list.
        public Body Next
        {
            get { return _next; }
        }

        internal void SynchronizeFixtures()
        {
            Transform xf1 = new Transform();
            xf1.R.Set(_sweep.a0);
            xf1.Position = _sweep.c0 - MathUtils.Multiply(ref xf1.R, _sweep.localCenter);

            BroadPhase broadPhase = _world._contactManager.BroadPhase;
            for (Fixture f = _fixtureList; f != null; f = f._next)
            {
                f.Synchronize(broadPhase, ref xf1, ref _xf);
            }
        }

        internal void SynchronizeTransform()
        {
            _xf.R.Set(_sweep.a);
            _xf.Position = _sweep.c - MathUtils.Multiply(ref _xf.R, _sweep.localCenter);
        }

        // This is used to prevent connected bodies from colliding.
        // It may lie, depending on the collideConnected flag.
        internal bool ShouldCollide(Body other)
        {
            // At least one body should be dynamic.
            if (_type != BodyType.Dynamic && other._type != BodyType.Dynamic)
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
            _sweep.c = _sweep.c0;
            _sweep.a = _sweep.a0;
            SynchronizeTransform();
        }
    }
}