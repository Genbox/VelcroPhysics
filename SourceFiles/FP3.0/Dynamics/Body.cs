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
using Microsoft.Xna.Framework;
using System.Diagnostics;

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

    /// <summary>
    /// A body definition holds all the data needed to construct a rigid body.
    /// You can safely re-use body definitions. Shapes are added to a body after construction.
    /// </summary>
    public class BodyDef
    {
        /// <summary>
        /// This constructor sets the body definition default values.
        /// </summary>
        public BodyDef()
        {
            UserData = null;
            Position = Vector2.Zero;
            Angle = 0.0f;
            LinearVelocity = Vector2.Zero;
            AngularVelocity = 0.0f;
            LinearDamping = 0.0f;
            AngularDamping = 0.0f;
            AllowSleep = true;
            Awake = true;
            FixedRotation = false;
            Bullet = false;
            Type = BodyType.Static;
            Active = true;
            InertiaScale = 1.0f;
        }

        /// <summary>
        /// The body type: static, kinematic, or dynamic.
        /// Note: if a dynamic body would have zero mass, the mass is set to one.
        /// </summary>
        public BodyType Type;

        /// <summary>
        /// The world position of the body. Avoid creating bodies at the origin
        /// since this can lead to many overlapping shapes.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// The world angle of the body in radians.
        /// </summary>
        public float Angle;

        /// <summary>
        /// The linear velocity of the body's origin in world co-ordinates.
        /// </summary>
        public Vector2 LinearVelocity;

        /// <summary>
        /// The angular velocity of the body.
        /// </summary>
        public float AngularVelocity;

        /// <summary>
        /// Linear damping is use to reduce the linear velocity. The damping parameter
        /// can be larger than 1.0f but the damping effect becomes sensitive to the
        /// time step when the damping parameter is large.
        /// </summary>
        public float LinearDamping;

        /// <summary>
        /// Angular damping is use to reduce the angular velocity. The damping parameter
        /// can be larger than 1.0f but the damping effect becomes sensitive to the
        /// time step when the damping parameter is large.
        /// </summary>
        public float AngularDamping;

        /// <summary>
        /// Set this flag to false if this body should never fall asleep. Note that
        /// this increases CPU usage.
        /// </summary>
        public bool AllowSleep;

        /// <summary>
        /// Is this body awake or sleeping?
        /// </summary>
        public bool Awake;

        /// <summary>
        /// Should this body be prevented from rotating? Useful for characters.
        /// </summary>
        public bool FixedRotation;

        /// <summary>
        /// Is this a fast moving body that should be prevented from tunneling through
        /// other moving bodies? Note that all bodies are prevented from tunneling through
        /// static bodies.
        /// @warning You should use this flag sparingly since it increases processing time.
        /// </summary>
        public bool Bullet;

        /// <summary>
        /// Does this body start out active?
        /// </summary>
        public bool Active;

        /// <summary>
        /// Use this to store application specific body data.
        /// </summary>
        public object UserData;

        /// <summary>
        /// Experimental: scales the inertia tensor.
        /// </summary>
        public float InertiaScale;
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
        /// <summary>
        /// Set the type of this body. This may alter the mass and velocity.
        /// </summary>
        /// <param name="type">The type.</param>
        public void SetBodyType(BodyType type)
        {
            if (_bodyType == type)
            {
                return;
            }

            _bodyType = type;

            ResetMassData();

            if (_bodyType == BodyType.Static)
            {
                _linearVelocity = Vector2.Zero;
                _angularVelocity = 0.0f;
            }

            SetAwake(true);

            _force = Vector2.Zero;
            _torque = 0.0f;

            // Since the body type changed, we need to flag contacts for filtering.
            for (ContactEdge ce = _contactList; ce != null; ce = ce.Next)
            {
                ce.Contact.FlagForFiltering();
            }
        }

        /// <summary>
        /// Get the type of this body.
        /// </summary>
        /// <returns></returns>
        public BodyType GetBodyType()
        {
            return _bodyType;
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
            Debug.Assert(_world.IsLocked == false);
            if (_world.IsLocked)
                return null;

            Fixture fixture = new Fixture();
            fixture.Create(this, shape);

            if ((_flags & BodyFlags.Active) == BodyFlags.Active)
            {
                BroadPhase broadPhase = _world.ContactManager._broadPhase;
                fixture.CreateProxy(broadPhase, ref _xf);
            }

            fixture.Next = _fixtureList;
            _fixtureList = fixture;
            ++_fixtureCount;

            // Adjust mass properties if needed.
            if (fixture.Shape.Density > 0.0f)
            {
                ResetMassData();
            }

            // Let the world know we have a new fixture. This will cause new contacts
            // to be created at the beginning of the next time step.
            _world.Flags |= WorldFlags.NewFixture;

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
            Debug.Assert(_world.IsLocked == false);
            if (_world.IsLocked)
            {
                return;
            }

#if DEBUG
            Debug.Assert(fixture.Body == this);

            // Remove the fixture from this body's singly linked list.
            Debug.Assert(_fixtureCount > 0);

            Fixture node = _fixtureList;
            bool found = false;
            while (node != null)
            {
                if (node == fixture)
                {
                    _fixtureList = fixture.Next;
                    found = true;
                    break;
                }

                node = node.Next;
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

                Fixture fixtureA = c.GetFixtureA();
                Fixture fixtureB = c.GetFixtureB();

                if (fixture == fixtureA || fixture == fixtureB)
                {
                    // This destroys the contact and removes it from
                    // this body's contact list.
                    _world.ContactManager.Destroy(c);
                }
            }

            if ((_flags & BodyFlags.Active) == BodyFlags.Active)
            {
                Debug.Assert(fixture.ProxyId != BroadPhase.NullProxy);

                BroadPhase broadPhase = _world.ContactManager._broadPhase;
                fixture.DestroyProxy(broadPhase);
            }
            else
            {
                Debug.Assert(fixture.ProxyId == BroadPhase.NullProxy);
            }

            fixture.Destroy();
            fixture.Body = null;
            fixture.Next = null;

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
            Debug.Assert(_world.IsLocked == false);
            if (_world.IsLocked)
            {
                return;
            }

            _xf.R.Set(angle);
            _xf.Position = position;

            _sweep.c0 = _sweep.c = MathUtils.Multiply(ref _xf, _sweep.localCenter);
            _sweep.a0 = _sweep.a = angle;

            BroadPhase broadPhase = _world.ContactManager._broadPhase;
            for (Fixture f = _fixtureList; f != null; f = f.Next)
            {
                f.Synchronize(broadPhase, ref _xf, ref _xf);
            }

            _world.ContactManager.FindNewContacts();
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
        /// Get the world body origin position.
        /// </summary>
        /// <returns>world position of the body's origin.</returns>
        public Vector2 GetPosition()
        {
            return _xf.Position;
        }

        /// <summary>
        /// Get the angle in radians.
        /// </summary>
        /// <returns>current world rotation angle in radians.</returns>
        public float GetAngle()
        {
            return _sweep.a;
        }

        /// <summary>
        /// Get the world position of the center of mass.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetWorldCenter()
        {
            return _sweep.c;
        }

        /// <summary>
        /// Get the local position of the center of mass.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetLocalCenter()
        {
            return _sweep.localCenter;
        }

        /// <summary>
        /// Set the linear velocity of the center of mass.
        /// </summary>
        /// <param name="v">the new linear velocity of the center of mass.</param>
        public void SetLinearVelocity(Vector2 v)
        {
            if (_bodyType == BodyType.Static)
            {
                return;
            }

            _linearVelocity = v;
        }

        /// <summary>
        /// Get the linear velocity of the center of mass.
        /// </summary>
        /// <returns>the linear velocity of the center of mass.</returns>
        public Vector2 GetLinearVelocity()
        {
            return _linearVelocity;
        }

        /// <summary>
        /// Set the angular velocity.
        /// </summary>
        /// <param name="omega">the new angular velocity in radians/second.</param>
        public void SetAngularVelocity(float omega)
        {
            if (_bodyType == BodyType.Static)
            {
                return;
            }

            _angularVelocity = omega;
        }

        /// <summary>
        /// Get the angular velocity.
        /// </summary>
        /// <returns>angular velocity in radians/second.</returns>
        public float GetAngularVelocity()
        {
            return _angularVelocity;
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

            if (IsAwake() == false)
            {
                SetAwake(true);
            }

            _force += force;
            _torque += MathUtils.Cross(point - _sweep.c, force);
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

            if (IsAwake() == false)
            {
                SetAwake(true);
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

            if (IsAwake() == false)
            {
                SetAwake(true);
            }

            _linearVelocity += _invMass * impulse;
            _angularVelocity += _invI * MathUtils.Cross(point - _sweep.c, impulse);
        }

        /// <summary>
        /// Get the total mass of the body.
        /// </summary>
        /// <returns>the mass, usually in kilograms (kg).</returns>
        public float GetMass()
        {
            return _mass;
        }

        /// <summary>
        /// Get the central rotational inertia of the body.
        /// </summary>
        /// <returns>the rotational inertia, usually in kg-m^2.</returns>
        public float GetInertia()
        {
            return _I;
        }

        /// <summary>
        /// Get the mass data of the body. The rotational inertia is relative
        /// to the center of mass.
        /// </summary>
        /// <param name="massData"> a struct containing the mass, inertia and center of the body.</param>
        public void GetMassData(out MassData massData)
        {
            massData = new MassData();
            massData.Mass = _mass;
            massData.Inertia = _I;
            massData.Center = _sweep.localCenter;
        }

        /// <summary>
        /// Set the mass properties to override the mass properties of the fixtures.
        /// Note that this changes the center of mass position.
        /// Note that creating or destroying fixtures can also alter the mass.
        /// This function has no effect if the body isn't dynamic.
        /// @warning The supplied rotational inertia is assumed to be relative to the center of mass.
        /// </summary>
        /// <param name="massData">The mass data.</param>
        public void SetMassData(ref MassData massData)
        {
            Debug.Assert(_world.IsLocked == false);
            if (_world.IsLocked)
            {
                return;
            }

            if (_bodyType != BodyType.Dynamic)
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
                _invI = 1.0f / _I;
            }

            // Move center of mass.
            Vector2 oldCenter = _sweep.c;
            _sweep.localCenter = massData.Center;
            _sweep.c0 = _sweep.c = MathUtils.Multiply(ref _xf, _sweep.localCenter);

            // Update center of mass velocity.
            _linearVelocity += MathUtils.Cross(_angularVelocity, _sweep.c - oldCenter);
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
            _sweep.localCenter = Vector2.Zero;

            // Static and kinematic bodies have zero mass.
            if (_bodyType == BodyType.Static || _bodyType == BodyType.Kinematic)
            {
                return;
            }

            Debug.Assert(_bodyType == BodyType.Dynamic);

            // Accumulate mass over all fixtures.
            Vector2 center = Vector2.Zero;
            for (Fixture f = _fixtureList; f != null; f = f.Next)
            {
                if (f.Shape.Density == 0.0f)
                {
                    continue;
                }

                MassData massData = f.Shape.MassData;
                //f.GetMassData(out massData);
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
                _I *= _intertiaScale;

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
            return _linearVelocity + MathUtils.Cross(_angularVelocity, worldPoint - _sweep.c);
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

        /// <summary>
        /// Get the linear damping of the body.
        /// </summary>
        /// <returns></returns>
        public float GetLinearDamping()
        {
            return _linearDamping;
        }

        /// <summary>
        /// Set the linear damping of the body.
        /// </summary>
        /// <param name="linearDamping">The linear damping.</param>
        public void SetLinearDamping(float linearDamping)
        {
            _linearDamping = linearDamping;
        }

        /// <summary>
        /// Get the angular damping of the body.
        /// </summary>
        /// <returns></returns>
        public float GetAngularDamping()
        {
            return _angularDamping;
        }

        /// <summary>
        /// Set the angular damping of the body.
        /// </summary>
        /// <param name="angularDamping">The angular damping.</param>
        public void SetAngularDamping(float angularDamping)
        {
            _angularDamping = angularDamping;
        }

        /// <summary>
        /// Is this body treated like a bullet for continuous collision detection?
        /// </summary>
        /// <value><c>true</c> if this instance is bullet; otherwise, <c>false</c>.</value>
        public bool IsBullet
        {
            get
            {
                return (_flags & BodyFlags.Bullet) == BodyFlags.Bullet;
            }
        }

        /// <summary>
        /// Should this body be treated like a bullet for continuous collision detection?
        /// </summary>
        /// <param name="flag">if set to <c>true</c> [flag].</param>
        public void SetBullet(bool flag)
        {
            if (flag)
            {
                _flags |= BodyFlags.Bullet;
            }
            else
            {
                _flags &= ~BodyFlags.Bullet;
            }
        }

        /// <summary>
        /// You can disable sleeping on this body. If you disable sleeping, the
        /// body will be woken.
        /// </summary>
        /// <param name="flag">if set to <c>true</c> [flag].</param>
        public void SetSleepingAllowed(bool flag)
        {
            if (flag)
            {
                _flags |= BodyFlags.AutoSleep;
            }
            else
            {
                _flags &= ~BodyFlags.AutoSleep;
                SetAwake(true);
            }

        }

        /// <summary>
        /// Is this body allowed to sleep
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is sleeping allowed; otherwise, <c>false</c>.
        /// </value>
        public bool IsSleepingAllowed
        {
            get
            {
                return (_flags & BodyFlags.AutoSleep) == BodyFlags.AutoSleep;
            }
        }

        /// <summary>
        /// You can disable sleeping on this body.
        /// </summary>
        /// <param name="flag">if set to <c>true</c> [flag].</param>
        public void AllowSleeping(bool flag)
        {
            if (flag)
            {
                _flags |= BodyFlags.AutoSleep;
            }
            else
            {
                _flags &= ~BodyFlags.AutoSleep;
                SetAwake(true);
            }
        }

        /// <summary>
        /// Set the sleep state of the body. A sleeping body has very
        /// low CPU cost.
        /// </summary>
        /// <param name="flag">if set to <c>true</c> [flag].</param>
        public void SetAwake(bool flag)
        {
            if (flag)
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

        /// <summary>
        /// Get the sleeping state of this body.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is awake; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAwake()
        {
            return (_flags & BodyFlags.Awake) == BodyFlags.Awake;
        }

        /// <summary>
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
        /// </summary>
        /// <param name="flag">if set to <c>true</c> [flag].</param>
        public void SetActive(bool flag)
        {
            if (flag == IsActive())
            {
                return;
            }

            if (flag)
            {
                _flags |= BodyFlags.Active;

                // Create all proxies.
                BroadPhase broadPhase = _world.ContactManager._broadPhase;
                for (Fixture f = _fixtureList; f != null; f = f.Next)
                {
                    f.CreateProxy(broadPhase, ref _xf);
                }

                // Contacts are created the next time step.
            }
            else
            {
                _flags &= ~BodyFlags.Active;

                // Destroy all proxies.
                BroadPhase broadPhase = _world.ContactManager._broadPhase;
                for (Fixture f = _fixtureList; f != null; f = f.Next)
                {
                    f.DestroyProxy(broadPhase);
                }

                // Destroy the attached contacts.
                ContactEdge ce = _contactList;
                while (ce != null)
                {
                    ContactEdge ce0 = ce;
                    ce = ce.Next;
                    _world.ContactManager.Destroy(ce0.Contact);
                }
                _contactList = null;
            }
        }

        /// <summary>
        /// Get the active state of the body.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </returns>
        public bool IsActive()
        {
            return (_flags & BodyFlags.Active) == BodyFlags.Active;
        }

        /// <summary>
        /// Set this body to have fixed rotation. This causes the mass
        /// to be reset.
        /// </summary>
        /// <param name="flag">if set to <c>true</c> [flag].</param>
        public void SetFixedRotation(bool flag)
        {
            if (flag)
            {
                _flags |= BodyFlags.FixedRotation;
            }
            else
            {
                _flags &= ~BodyFlags.FixedRotation;
            }

            ResetMassData();
        }

        /// <summary>
        /// Does this body have fixed rotation?
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is fixed rotation]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsFixedRotation()
        {
            return (_flags & BodyFlags.FixedRotation) == BodyFlags.FixedRotation;
        }

        public Fixture GetFixtureList()
        {
            return _fixtureList;
        }

        /// <summary>
        /// Get the list of all joints attached to this body.
        /// </summary>
        /// <returns></returns>
        public JointEdge GetJointList()
        {
            return _jointList;
        }

        /// <summary>
        /// Get the list of all contacts attached to this body.
        /// @warning this list changes during the time step and you may
        /// miss some collisions if you don't use ContactListener.
        /// </summary>
        /// <returns></returns>
        public ContactEdge GetContactList()
        {
            return _contactList;
        }

        /// <summary>
        /// Get the next body in the world's body list.
        /// </summary>
        /// <returns></returns>
        public Body GetNext()
        {
            return _next;
        }

        /// <summary>
        /// Get the user data pointer that was provided in the body definition.
        /// </summary>
        /// <returns></returns>
        public object GetUserData()
        {
            return _userData;
        }

        /// <summary>
        /// Set the user data. Use this to store your application specific data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void SetUserData(object data)
        {
            _userData = data;
        }

        /// <summary>
        /// Get the parent world of this body.
        /// </summary>
        /// <returns></returns>
        public World GetWorld()
        {
            return _world;
        }

        internal Body(BodyDef bd, World world)
        {
            Debug.Assert(bd.Position.IsValid());
            Debug.Assert(bd.LinearVelocity.IsValid());
            Debug.Assert(MathUtils.IsValid(bd.Angle));
            Debug.Assert(MathUtils.IsValid(bd.AngularVelocity));
            Debug.Assert(MathUtils.IsValid(bd.InertiaScale) && bd.InertiaScale >= 0.0f);
            Debug.Assert(MathUtils.IsValid(bd.AngularDamping) && bd.AngularDamping >= 0.0f);
            Debug.Assert(MathUtils.IsValid(bd.LinearDamping) && bd.LinearDamping >= 0.0f);


            if (bd.Bullet)
            {
                _flags |= BodyFlags.Bullet;
            }
            if (bd.FixedRotation)
            {
                _flags |= BodyFlags.FixedRotation;
            }
            if (bd.AllowSleep)
            {
                _flags |= BodyFlags.AutoSleep;
            }
            if (bd.Awake)
            {
                _flags |= BodyFlags.Awake;
            }
            if (bd.Active)
            {
                _flags |= BodyFlags.Active;
            }

            _world = world;

            _xf.Position = bd.Position;
            _xf.R.Set(bd.Angle);

            _sweep.t0 = 1.0f;
            _sweep.a0 = _sweep.a = bd.Angle;
            _sweep.c0 = _sweep.c = MathUtils.Multiply(ref _xf, _sweep.localCenter);

            _linearVelocity = bd.LinearVelocity;
            _angularVelocity = bd.AngularVelocity;

            _linearDamping = bd.LinearDamping;
            _angularDamping = bd.AngularDamping;

            _bodyType = bd.Type;

            if (_bodyType == BodyType.Dynamic)
            {
                _mass = 1.0f;
                _invMass = 1.0f;
            }

            _intertiaScale = bd.InertiaScale;
            _userData = bd.UserData;
        }

        internal void SynchronizeFixtures()
        {
            Transform xf1 = new Transform();
            xf1.R.Set(_sweep.a0);
            xf1.Position = _sweep.c0 - MathUtils.Multiply(ref xf1.R, _sweep.localCenter);

            BroadPhase broadPhase = _world.ContactManager._broadPhase;
            for (Fixture f = _fixtureList; f != null; f = f.Next)
            {
                f.Synchronize(broadPhase, ref xf1, ref _xf);
            }
        }

        internal void SynchronizeTransform()
        {
            _xf.R.Set(_sweep.a);
            _xf.Position = _sweep.c - MathUtils.Multiply(ref _xf.R, _sweep.localCenter);
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
            _sweep.c = _sweep.c0;
            _sweep.a = _sweep.a0;
            SynchronizeTransform();
        }

        /// <summary>
        /// Get the world body origin position.
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
                return _sweep.a;
            }
            set
            {
                SetTransform(Position, value);
            }
        }

        internal BodyFlags _flags;
        private BodyType _bodyType;
        internal Transform _xf;		// the body origin transform
        internal Sweep _sweep;	// the swept motion for CCD
        internal Vector2 _linearVelocity;
        internal float _angularVelocity;
        internal Vector2 _force;
        internal float _torque;
        private World _world;
        internal Body _prev;
        internal Body _next;
        internal Fixture _fixtureList;
        internal int _fixtureCount;
        internal JointEdge _jointList;
        internal ContactEdge _contactList;
        internal float _mass, _invMass;
        private float _I;
        internal float _invI;
        internal float _linearDamping;
        internal float _angularDamping;
        internal float _sleepTime;
        private object _userData;
        private float _intertiaScale;
    }
}
