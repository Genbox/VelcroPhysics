using System;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace FarseerPhysics
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

    /// A body definition holds all the data needed to ruct a rigid body.
    /// You can safely re-use body definitions. Shapes are added to a body after construction.
    public class BodyDef
    {
        /// This ructor sets the body definition default values.
        public BodyDef()
        {
            userData = null;
            position = new Vector2(0.0f, 0.0f);
            angle = 0.0f;
            linearVelocity = new Vector2(0.0f, 0.0f);
            angularVelocity = 0.0f;
            linearDamping = 0.0f;
            angularDamping = 0.0f;
            allowSleep = true;
            awake = true;
            fixedRotation = false;
            bullet = false;
            type = BodyType.Static;
		    active = true;
            inertiaScale = 1.0f;
        }

        /// The body type: static, kinematic, or dynamic.
        /// Note: if a dynamic body would have zero mass, the mass is set to one.
        public BodyType type;

        /// The world position of the body. Avoid creating bodies at the origin
        /// since this can lead to many overlapping shapes.
        public Vector2 position;

        /// The world angle of the body in radians.
        public float angle;

        /// The linear velocity of the body's origin in world co-ordinates.
        public Vector2 linearVelocity;

        /// The angular velocity of the body.
        public float angularVelocity;

        /// Linear damping is use to reduce the linear velocity. The damping parameter
        /// can be larger than 1.0f but the damping effect becomes sensitive to the
        /// time step when the damping parameter is large.
        public float linearDamping;

        /// Angular damping is use to reduce the angular velocity. The damping parameter
        /// can be larger than 1.0f but the damping effect becomes sensitive to the
        /// time step when the damping parameter is large.
        public float angularDamping;

        /// Set this flag to false if this body should never fall asleep. Note that
        /// this increases CPU usage.
        public bool allowSleep;

        /// Is this body awake or sleeping?
        public bool awake;

        /// Should this body be prevented from rotating? Useful for characters.
        public bool fixedRotation;

        /// Is this a fast moving body that should be prevented from tunneling through
        /// other moving bodies? Note that all bodies are prevented from tunneling through
        /// static bodies.
        /// @warning You should use this flag sparingly since it increases processing time.
        public bool bullet;

        /// Does this body start out active?
	    public bool active;

        /// Use this to store application specific body data.
        public object userData;

        /// Experimental: scales the inertia tensor.
        public float inertiaScale;
    };


    [Flags]
    public enum BodyFlags
    {
        None            = 0,
        Island          = (1 << 0),
        Awake           = (1 << 1),
        AutoSleep       = (1 << 2),
        Bullet          = (1 << 3),
        FixedRotation   = (1 << 4),
        Active          = (1 << 5),
    }

    public class Body
    {

        /// Set the type of this body. This may alter the mass and velocity.
	    public void SetType(BodyType type)
        {
            if (_type == type)
            {
                return;
            }

            _type = type;

            ResetMassData();

            if (_type == BodyType.Static)
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

	    /// Get the type of this body.
        public BodyType GetType()
        {
            return _type;
        }

	    /// Creates a fixture and attach it to this body. Use this function if you need
	    /// to set some fixture parameters, like friction. Otherwise you can create the
	    /// fixture directly from a shape.
	    /// If the density is non-zero, this function automatically updates the mass of the body.
	    /// Contacts are not created until the next time step.
	    /// @param def the fixture definition.
	    /// @warning This function is locked during callbacks.
	    public Fixture CreateFixture(FixtureDef def)
        {
            Debug.Assert(_world.IsLocked == false);
	        if (_world.IsLocked == true)
	        {
		        return null;
	        }

            Fixture fixture = new Fixture();
            fixture.Create(this, def);

            if ((_flags & BodyFlags.Active) == BodyFlags.Active)
            {
                BroadPhase broadPhase = _world._contactManager._broadPhase;
                fixture.CreateProxy(broadPhase, ref _xf);
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

	    /// Creates a fixture from a shape and attach it to this body.
	    /// This is a convenience function. Use FixtureDef if you need to set parameters
	    /// like friction, restitution, user data, or filtering.
	    /// If the density is non-zero, this function automatically updates the mass of the body.
	    /// @param shape the shape to be cloned.
	    /// @param density the shape density (set to zero for static bodies).
	    /// @warning This function is locked during callbacks.
	    public Fixture CreateFixture(Shape shape, float density)
        {
            FixtureDef def = new FixtureDef();
            def.shape = shape;
            def.density = density;

            return CreateFixture(def);
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
	        if (_world.IsLocked == true)
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

		        Fixture fixtureA = c.GetFixtureA();
		        Fixture fixtureB = c.GetFixtureB();

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

                BroadPhase broadPhase = _world._contactManager._broadPhase;
                fixture.DestroyProxy(broadPhase);
            }
            else
            {
                Debug.Assert(fixture._proxyId == BroadPhase.NullProxy);
            }

	        fixture.Destroy();
	        fixture._body = null;
	        fixture._next = null;
	        
            --_fixtureCount;

            
            ResetMassData();
        }

	    /// Set the position of the body's origin and rotation.
	    /// This breaks any contacts and wakes the other bodies.
	    /// @param position the world position of the body's local origin.
	    /// @param angle the world rotation in radians.
	    public void SetTransform(Vector2 position, float angle)
        {
            Debug.Assert(_world.IsLocked == false);
	        if (_world.IsLocked == true)
	        {
		        return;
	        }

	        _xf.R.Set(angle);
	        _xf.Position = position;

	        _sweep.c0 = _sweep.c = MathUtils.Multiply(ref _xf, _sweep.localCenter);
	        _sweep.a0 = _sweep.a = angle;

	        BroadPhase broadPhase = _world._contactManager._broadPhase;
	        for (Fixture f = _fixtureList; f != null; f = f._next)
	        {
		        f.Synchronize(broadPhase, ref _xf, ref _xf);
	        }

	        _world._contactManager.FindNewContacts();
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

	    /// Set the linear velocity of the center of mass.
	    /// @param v the new linear velocity of the center of mass.
	    public void SetLinearVelocity(Vector2 v)
        {
            if (_type != BodyType.Static)
            {
                _linearVelocity = v;
            }
        }

	    /// Get the linear velocity of the center of mass.
	    /// @return the linear velocity of the center of mass.
	    public Vector2 GetLinearVelocity()
        {
            return _linearVelocity;
        }

	    /// Set the angular velocity.
	    /// @param omega the new angular velocity in radians/second.
	    public void SetAngularVelocity(float omega)
        {
            if (_type != BodyType.Static)
            {
                _angularVelocity = omega;
            }
        }

	    /// Get the angular velocity.
	    /// @return the angular velocity in radians/second.
	    public float GetAngularVelocity()
        {
            return _angularVelocity;
        }

	    /// Apply a force at a world point. If the force is not
	    /// applied at the center of mass, it will generate a torque and
	    /// affect the angular velocity. This wakes up the body.
	    /// @param force the world force vector, usually in Newtons (N).
	    /// @param point the world position of the point of application.
	    public void ApplyForce(Vector2 force, Vector2 point)
        {
            if (_type == BodyType.Dynamic)
            {
                if (IsAwake() == false)
                {
                    SetAwake(true);
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
                if (IsAwake() == false)
                {
                    SetAwake(true);
                }

                _torque += torque;
            }
        }

	    /// Apply an impulse at a point. This immediately modifies the velocity.
	    /// It also modifies the angular velocity if the point of application
	    /// is not at the center of mass. This wakes up the body.
	    /// @param impulse the world impulse vector, usually in N-seconds or kg-m/s.
	    /// @param point the world position of the point of application.
	    public void ApplyImpulse(Vector2 impulse, Vector2 point)
        {
            if (_type == BodyType.Dynamic)
            {
                if (IsAwake() == false)
                {
                    SetAwake(true);
                }
                _linearVelocity += _invMass * impulse;
                _angularVelocity += _invI * MathUtils.Cross(point - _sweep.c, impulse);
            }
        }

	    /// Get the total mass of the body.
	    /// @return the mass, usually in kilograms (kg).
	    public float GetMass()
        {
            return _mass;
        }

	    /// Get the central rotational inertia of the body.
	    /// @return the rotational inertia, usually in kg-m^2.
	    public float GetInertia()
        {
            return _I;
        }

        /// Get the mass data of the body. The rotational inertia is relative
        /// to the center of mass.
	    /// @return a struct containing the mass, inertia and center of the body.
	    public void GetMassData(out MassData massData)
        {
            massData = new MassData();
            massData.mass = _mass;
            massData.i = _I;
            massData.center = _sweep.localCenter;
        }

        /// Set the mass properties to override the mass properties of the fixtures.
 	    /// Note that this changes the center of mass position.
        /// Note that creating or destroying fixtures can also alter the mass.
        /// This function has no effect if the body isn't dynamic.
        /// @warning The supplied rotational inertia is assumed to be relative to the center of mass.
        /// @param massData the mass properties.
        public void SetMassData(ref MassData massData)
        {
	        Debug.Assert(_world.IsLocked == false);
	        if (_world.IsLocked == true)
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

	        _mass = massData.mass;

	        if (_mass <= 0.0f)
	        {
                _mass = 1.0f;
	        }

            _invMass = 1.0f / _mass;


	        if (massData.i > 0.0f && (_flags & BodyFlags.FixedRotation) == 0)
	        {
		        _I = massData.i - _mass * Vector2.Dot(massData.center, massData.center);
		        _invI = 1.0f / _I;
	        }

	        // Move center of mass.
	        Vector2 oldCenter = _sweep.c;
	        _sweep.localCenter = massData.center;
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
		        _mass += massData.mass;
		        center += massData.mass * massData.center;
		        _I += massData.i;
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

	    /// Get the linear damping of the body.
	    public float GetLinearDamping()
        {
            return _linearDamping;
        }

	    /// Set the linear damping of the body.
	    public void SetLinearDamping(float linearDamping)
        {
            _linearDamping = linearDamping;
        }

	    /// Get the angular damping of the body.
	    public float GetAngularDamping()
        {
            return _angularDamping;
        }

	    /// Set the angular damping of the body.
	    public void SetAngularDamping(float angularDamping)
        {
            _angularDamping = angularDamping;
        }

	    /// Is this body treated like a bullet for continuous collision detection?
	    public bool IsBullet
        {
            get
            {
                return (_flags & BodyFlags.Bullet) == BodyFlags.Bullet;
            }
        }

	    /// Should this body be treated like a bullet for continuous collision detection?
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

        /// You can disable sleeping on this body. If you disable sleeping, the
	    /// body will be woken.
	    void SetSleepingAllowed(bool flag)
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

	    /// Is this body allowed to sleep
	    public bool IsSleepingAllowed
        {
            get
            {
                return (_flags & BodyFlags.AutoSleep) == BodyFlags.AutoSleep;
            }
        }

	    /// You can disable sleeping on this body.
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

	    /// Set the sleep state of the body. A sleeping body has very
	    /// low CPU cost.
	    /// @param flag set to true to put body to sleep, false to wake it.
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

        /// Get the sleeping state of this body.
	    /// @return true if the body is sleeping.
	    public bool IsAwake()
        {
            return (_flags & BodyFlags.Awake) == BodyFlags.Awake;
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
                BroadPhase broadPhase = _world._contactManager._broadPhase;
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
                BroadPhase broadPhase = _world._contactManager._broadPhase;
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
                    _world._contactManager.Destroy(ce0.Contact);
                }
                _contactList = null;
            }
        }

	    /// Get the active state of the body.
	    public bool IsActive()
        {
            return (_flags & BodyFlags.Active) == BodyFlags.Active;
        }

	    /// Set this body to have fixed rotation. This causes the mass
	    /// to be reset.
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

	    /// Does this body have fixed rotation?
        public bool IsFixedRotation()
        {
            return (_flags & BodyFlags.FixedRotation) == BodyFlags.FixedRotation;
        }

	    public Fixture GetFixtureList()
        {
            return _fixtureList;
        }

	    /// Get the list of all joints attached to this body.
	    public JointEdge GetJointList()
        {
            return _jointList;
        }

	    /// Get the list of all contacts attached to this body.
	    /// @warning this list changes during the time step and you may
	    /// miss some collisions if you don't use ContactListener.
	    public ContactEdge GetContactList()
        {
            return _contactList;
        }

        /// Get the next body in the world's body list.
	    public Body GetNext()
        {
            return _next;
        }

	    /// Get the user data pointer that was provided in the body definition.
	    public object GetUserData()
        {
            return _userData;
        }

	    /// Set the user data. Use this to store your application specific data.
	    public void SetUserData(object data)
        {
            _userData = data;
        }

	    /// Get the parent world of this body.
	    public World GetWorld()
        {
            return _world;
        }

        internal Body(BodyDef bd, World world)
        {
            Debug.Assert(bd.position.IsValid());
            Debug.Assert(bd.linearVelocity.IsValid());
            Debug.Assert(MathUtils.IsValid(bd.angle));
            Debug.Assert(MathUtils.IsValid(bd.angularVelocity));
            Debug.Assert(MathUtils.IsValid(bd.inertiaScale) && bd.inertiaScale >= 0.0f);
            Debug.Assert(MathUtils.IsValid(bd.angularDamping) && bd.angularDamping >= 0.0f);
            Debug.Assert(MathUtils.IsValid(bd.linearDamping) && bd.linearDamping >= 0.0f);


	        if (bd.bullet)
	        {
		        _flags |= BodyFlags.Bullet;
	        }
	        if (bd.fixedRotation)
	        {
		        _flags |= BodyFlags.FixedRotation;
	        }
	        if (bd.allowSleep)
	        {
		        _flags |= BodyFlags.AutoSleep;
	        }
            if (bd.awake)
            {
                _flags |= BodyFlags.Awake;
            }
            if (bd.active)
            {
                _flags |= BodyFlags.Active;
            }

	        _world = world;

	        _xf.Position = bd.position;
	        _xf.R.Set(bd.angle);

	        _sweep.t0 = 1.0f;
	        _sweep.a0 = _sweep.a = bd.angle;
	        _sweep.c0 = _sweep.c = MathUtils.Multiply(ref _xf, _sweep.localCenter);

	        _linearVelocity = bd.linearVelocity;
	        _angularVelocity = bd.angularVelocity;

	        _linearDamping = bd.linearDamping;
	        _angularDamping = bd.angularDamping;
            
            _type = bd.type;
            if (_type == BodyType.Dynamic)
            {
                _mass = 1.0f;
                _invMass = 1.0f;
            }

            _intertiaScale = bd.inertiaScale;
	        _userData = bd.userData;
        }

        internal void SynchronizeFixtures()
        {
            Transform xf1 = new Transform();
	        xf1.R.Set(_sweep.a0);
	        xf1.Position = _sweep.c0 - MathUtils.Multiply(ref xf1.R, _sweep.localCenter);

	        BroadPhase broadPhase = _world._contactManager._broadPhase;
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
                    if (jn.Joint._collideConnected == false)
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
        internal BodyType _type;

        internal int _islandIndex;

        internal Transform _xf;		// the body origin transform
        internal Sweep _sweep;	// the swept motion for CCD

        internal Vector2 _linearVelocity;
        internal float _angularVelocity;

        internal Vector2 _force;
        internal float _torque;

        internal World _world;
        internal Body _prev;
        internal Body _next;

        internal Fixture _fixtureList;
        internal int _fixtureCount;

        internal JointEdge _jointList;
        internal ContactEdge _contactList;

        internal float _mass, _invMass;
        internal float _I, _invI;

        internal float _linearDamping;
        internal float _angularDamping;

        internal float _sleepTime;

        internal object _userData;

        internal float _intertiaScale;
    }
}
