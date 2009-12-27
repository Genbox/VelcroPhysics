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

using Box2DX.Common;
using Box2DX.Collision;
using Math = Box2DX.Common.Math;

namespace Box2DX.Dynamics
{
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
            Position = new Vec2();
            Position.Set(0.0f, 0.0f);
            Angle = 0.0f;
            LinearVelocity = new Vec2(0f, 0f);
            AngularVelocity = 0.0f;
            LinearDamping = 0.0f;
            AngularDamping = 0.0f;
            AutoSleep = true;
            Awake = true;
            FixedRotation = false;
            Bullet = false;
            Type = Body.BodyType.Static;
            Active = true;
        }

        /// <summary>
        /// Use this to store application specific body data.
        /// </summary>
        public object UserData;

        /// <summary>
        /// The world position of the body. Avoid creating bodies at the origin
        /// since this can lead to many overlapping shapes.
        /// </summary>
        public Vec2 Position;

        /// <summary>
        /// The world angle of the body in radians.
        /// </summary>
        public float Angle;

        /// The linear velocity of the body in world co-ordinates.
        public Vec2 LinearVelocity;

        // The angular velocity of the body.
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
        public bool AutoSleep;

        /// <summary>
        /// Is this body initially awake or sleeping?
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

        /// The body type: static, kinematic, or dynamic.
        /// Note: if a dynamic body would have zero mass, the mass is set to one.

        /// <summary>
        /// The body type: static, kinematic, or dynamic.
        /// Note: if a dynamic body would have zero mass, the mass is set to one.
        /// </summary>
        public Body.BodyType Type;
    }

    /// <summary>
    /// A rigid body.
    /// </summary>
    public class Body : IDisposable
    {
        [Flags]
        public enum BodyFlags
        {
            IslandFlag = 0x0001,
            AwakeFlag = 0x0002,
            AutoSleepFlag = 0x004,
            BulletFlag = 0x008,
            FixedRotationFlag = 0x0010,
            ActiveFlag = 0x0020
        }

        /// The body type.
        /// static: zero mass, zero velocity, may be manually moved
        /// kinematic: zero mass, non-zero velocity set by user, moved by solver
        /// dynamic: positive mass, non-zero velocity determined by forces, moved by solver
        public enum BodyType
        {
            Static,
            Kinematic,
            Dynamic
        }

        internal BodyFlags _flags;
        private BodyType _type;

        internal int _islandIndex;

        private Transform _xf;		// the body origin transform
        internal Sweep _sweep;	// the swept motion for CCD

        internal Vec2 _linearVelocity;
        internal float _angularVelocity;

        internal Vec2 _force;
        internal float _torque;

        private World _world;
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

        private object _userData;

        internal Body(BodyDef bd, World world)
        {
            _flags = 0;

            if (bd.Bullet)
            {
                _flags |= BodyFlags.BulletFlag;
            }
            if (bd.FixedRotation)
            {
                _flags |= BodyFlags.FixedRotationFlag;
            }
            if (bd.AutoSleep)
            {
                _flags |= BodyFlags.AutoSleepFlag;
            }
            if (bd.Awake)
            {
                _flags |= BodyFlags.AwakeFlag;
            }
            if (bd.Active)
            {
                _flags |= BodyFlags.ActiveFlag;
            }

            _world = world;

            _xf.Position = bd.Position;
            _xf.R.Set(bd.Angle);

            _sweep.LocalCenter.SetZero();
            _sweep.T0 = 1.0f;
            _sweep.A0 = _sweep.A = bd.Angle;
            _sweep.C0 = _sweep.C = Math.Mul(_xf, _sweep.LocalCenter);

            _jointList = null;
            _contactList = null;
            _prev = null;
            _next = null;

            _linearVelocity = bd.LinearVelocity;
            _angularVelocity = bd.AngularVelocity;

            _linearDamping = bd.LinearDamping;
            _angularDamping = bd.AngularDamping;

            _force.SetZero();
            _torque = 0.0f;

            _sleepTime = 0.0f;

            _type = bd.Type;

            if (_type == BodyType.Dynamic)
            {
                _mass = 1.0f;
                _invMass = 1.0f;
            }
            else
            {
                _mass = 0.0f;
                _invMass = 0.0f;
            }

            _I = 0.0f;
            _invI = 0.0f;

            _userData = bd.UserData;

            _fixtureList = null;
            _fixtureCount = 0;
        }

        /// <summary>
        /// Set the type of this body. This may alter the mass and velocity.

        /// </summary>
        /// <param name="type"></param>
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
                _linearVelocity.SetZero();
                _angularVelocity = 0.0f;
            }

            SetAwake(true);

            _force.SetZero();
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
        public BodyType GetType()
        {
            return _type;
        }

        public void Dispose()
        {
            // shapes and joints are destroyed in World.Destroy
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
            Box2DXDebug.Assert(_world.IsLocked() == false);
            if (_world.IsLocked() == true)
            {
                return null;
            }

            //b2BlockAllocator* allocator = &m_world->m_blockAllocator;
            //void* memory = allocator->Allocate(sizeof(b2Fixture));

            Fixture fixture = new Fixture();
            fixture.Create(this, def);

            //NOTE: Correct?
            if ((_flags & BodyFlags.ActiveFlag) == BodyFlags.ActiveFlag)
            {
                BroadPhase broadPhase = _world._contactManager._broadPhase;
                fixture.CreateProxy(broadPhase, _xf);
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
            _world._flags |= World.WorldFlags.NewFixture;

            return fixture;
        }

        /// Creates a fixture from a shape and attach it to this body.
        /// This is a convenience function. Use b2FixtureDef if you need to set parameters
        /// like friction, restitution, user data, or filtering.
        /// If the density is non-zero, this function automatically updates the mass of the body.
        /// @param shape the shape to be cloned.
        /// @param density the shape density (set to zero for static bodies).
        /// @warning This function is locked during callbacks.
        public Fixture CreateFixture(Shape shape, float density)
        {
            FixtureDef def = new FixtureDef();
            def.Shape = shape;
            def.Density = density;

            return CreateFixture(def);
        }

        /// Destroy a fixture. This removes the fixture from the broad-phase and
        /// destroys all contacts associated with this fixture. This will
        /// automatically adjust the mass of the body if the body is dynamic and the
        /// fixture has positive density.
        /// All fixtures attached to a body are implicitly destroyed when the body is destroyed.
        /// @param fixture the fixture to be removed.
        /// @warning This function is locked during callbacks.
        public void DestroyFixture(ref Fixture fixture)
        {
            Box2DXDebug.Assert(_world.IsLocked() == false);
            if (_world.IsLocked() == true)
            {
                return;
            }

            Box2DXDebug.Assert(fixture._body == this);

            // Remove the fixture from this body's singly linked list.
            Box2DXDebug.Assert(_fixtureCount > 0);
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
            Box2DXDebug.Assert(found);

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

            //b2BlockAllocator* allocator = &m_world->m_blockAllocator;
            if ((_flags & BodyFlags.ActiveFlag) == 0)
            {
                Box2DXDebug.Assert(fixture._proxyId != BroadPhase.NullProxy);
                BroadPhase broadPhase = _world._contactManager._broadPhase;
                fixture.DestroyProxy(broadPhase);
            }
            else
            {
                Box2DXDebug.Assert(fixture._proxyId == BroadPhase.NullProxy);
            }

            fixture.Destroy();
            fixture._body = null;
            fixture._next = null;

            --_fixtureCount;
            ResetMassData();
        }

        /// Get the body transform for the body's origin.
        /// </summary>
        /// <returns>Return the world transform of the body's origin.</returns>
        public Transform GetTransform()
        {
            return _xf;
        }

        public void SetTransform(Vec2 position, float angle)
        {
            Box2DXDebug.Assert(_world.IsLocked() == false);
            if (_world.IsLocked() == true)
            {
                return;
            }

            _xf.R.Set(angle);
            _xf.Position = position;

            _sweep.C0 = _sweep.C = Math.Mul(_xf, _sweep.LocalCenter);
            _sweep.A0 = _sweep.A = angle;

            BroadPhase broadPhase = _world._contactManager._broadPhase;
            for (Fixture f = _fixtureList; f != null; f = f._next)
            {
                f.Synchronize(broadPhase, _xf, _xf);
            }

            _world._contactManager.FindNewContacts();

        }

        /// <summary>
        /// Get the world body origin position.
        /// </summary>
        /// <returns>Return the world position of the body's origin.</returns>
        public Vec2 GetPosition()
        {
            return _xf.Position;
        }

        /// <summary>
        /// Get the angle in radians.
        /// </summary>
        /// <returns>Return the current world rotation angle in radians.</returns>
        public float GetAngle()
        {
            return _sweep.A;
        }

        /// <summary>
        /// Get the world position of the center of mass.
        /// </summary>
        /// <returns></returns>
        public Vec2 GetWorldCenter()
        {
            return _sweep.C;
        }

        /// <summary>
        /// Get the local position of the center of mass.
        /// </summary>
        /// <returns></returns>
        public Vec2 GetLocalCenter()
        {
            return _sweep.LocalCenter;
        }

        /// <summary>
        /// Set the linear velocity of the center of mass.
        /// </summary>
        /// <param name="v">The new linear velocity of the center of mass.</param>
        public void SetLinearVelocity(Vec2 v)
        {
            if (_type == BodyType.Static)
            {
                return;
            }

            _linearVelocity = v;
        }

        /// <summary>
        /// Get the linear velocity of the center of mass.
        /// </summary>
        /// <returns>Return the linear velocity of the center of mass.</returns>
        public Vec2 GetLinearVelocity()
        {
            return _linearVelocity;
        }

        /// <summary>
        /// Set the angular velocity.
        /// </summary>
        /// <param name="omega">The new angular velocity in radians/second.</param>
        public void SetAngularVelocity(float w)
        {
            if (_type == BodyType.Static)
            {
                return;
            }

            _angularVelocity = w;
        }

        /// <summary>
        /// Get the angular velocity.
        /// </summary>
        /// <returns>Return the angular velocity in radians/second.</returns>
        public float GetAngularVelocity()
        {
            return _angularVelocity;
        }

        /// <summary>
        /// Apply a force at a world point. If the force is not
        /// applied at the center of mass, it will generate a torque and
        /// affect the angular velocity. This wakes up the body.
        /// </summary>
        /// <param name="force">The world force vector, usually in Newtons (N).</param>
        /// <param name="point">The world position of the point of application.</param>
        public void ApplyForce(Vec2 force, Vec2 point)
        {
            if (_type != BodyType.Dynamic)
            {
                return;
            }

            if (IsAwake() == false)
            {
                SetAwake(true);
            }

            _force += force;
            _torque += Vec2.Cross(point - _sweep.C, force);
        }

        /// <summary>
        /// Apply a torque. This affects the angular velocity
        /// without affecting the linear velocity of the center of mass.
        /// This wakes up the body.
        /// </summary>
        /// <param name="torque">About the z-axis (out of the screen), usually in N-m.</param>
        public void ApplyTorque(float torque)
        {
            if (_type != BodyType.Dynamic)
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
        /// <param name="impulse">The world impulse vector, usually in N-seconds or kg-m/s.</param>
        /// <param name="point">The world position of the point of application.</param>
        public void ApplyImpulse(Vec2 impulse, Vec2 point)
        {
            if (_type != BodyType.Dynamic)
            {
                return;
            }

            if (IsAwake() == false)
            {
                SetAwake(true);
            }

            _linearVelocity += _invMass * impulse;
            _angularVelocity += _invI * Vec2.Cross(point - _sweep.C, impulse);
        }

        /// <summary>
        /// Get the total mass of the body.
        /// </summary>
        /// <returns>Return the mass, usually in kilograms (kg).</returns>
        public float GetMass()
        {
            return _mass;
        }

        /// <summary>
        /// Get the central rotational inertia of the body.
        /// </summary>
        /// <returns>Return the rotational inertia, usually in kg-m^2.</returns>
        public float GetInertia()
        {
            return _I;
        }

        /// Get the mass data of the body. The rotational inertia is relative
        /// to the center of mass.
        /// @return a struct containing the mass, inertia and center of the body.
        public void GetMassData(out MassData data)
        {
            data.Mass = _mass;
            data.I = _I;
            data.Center = _sweep.LocalCenter;
        }

        /// Set the mass properties to override the mass properties of the fixtures.
        /// Note that this changes the center of mass position.
        /// Note that creating or destroying fixtures can also alter the mass.
        /// This function has no effect if the body isn't dynamic.
        /// @warning The supplied rotational inertia is assumed to be relative to the center of mass.
        /// @param massData the mass properties.
        public void SetMassData(MassData massData)
        {
            Box2DXDebug.Assert(_world.IsLocked() == false);
            if (_world.IsLocked() == true)
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

            if (massData.I > 0.0f && (_flags & BodyFlags.FixedRotationFlag) == 0)
            {
                _I = massData.I - _mass * Vec2.Dot(massData.Center, massData.Center);
                _invI = 1.0f / _I;
            }

            // Move center of mass.
            Vec2 oldCenter = _sweep.C;
            _sweep.LocalCenter = massData.Center;
            _sweep.C0 = _sweep.C = Math.Mul(_xf, _sweep.LocalCenter);

            // Update center of mass velocity.
            _linearVelocity += Vec2.Cross(_angularVelocity, _sweep.C - oldCenter);
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
            _sweep.LocalCenter.SetZero();

            // Static and kinematic bodies have zero mass.
            if (_type == BodyType.Static || _type == BodyType.Kinematic)
            {
                return;
            }

            Box2DXDebug.Assert(_type == BodyType.Dynamic);

            // Accumulate mass over all fixtures.
            Vec2 center = Vec2.Zero;
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
                _I += massData.I;
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

            if (_I > 0.0f && (_flags & BodyFlags.FixedRotationFlag) == 0)
            {
                // Center the inertia about the center of mass.
                _I -= _mass * Vec2.Dot(center, center);
                Box2DXDebug.Assert(_I > 0.0f);
                _invI = 1.0f / _I;
            }
            else
            {
                _I = 0.0f;
                _invI = 0.0f;
            }

            // Move center of mass.
            Vec2 oldCenter = _sweep.C;
            _sweep.LocalCenter = center;
            _sweep.C0 = _sweep.C = Math.Mul(_xf, _sweep.LocalCenter);

            // Update center of mass velocity.
            _linearVelocity += Vec2.Cross(_angularVelocity, _sweep.C - oldCenter);
        }

        /// <summary>
        /// Get the world coordinates of a point given the local coordinates.
        /// </summary>
        /// <param name="localPoint">A point on the body measured relative the the body's origin.</param>
        /// <returns>Return the same point expressed in world coordinates.</returns>
        public Vec2 GetWorldPoint(Vec2 localPoint)
        {
            return Common.Math.Mul(_xf, localPoint);
        }

        /// <summary>
        /// Get the world coordinates of a vector given the local coordinates.
        /// </summary>
        /// <param name="localVector">A vector fixed in the body.</param>
        /// <returns>Return the same vector expressed in world coordinates.</returns>
        public Vec2 GetWorldVector(Vec2 localVector)
        {
            return Common.Math.Mul(_xf.R, localVector);
        }

        /// <summary>
        /// Gets a local point relative to the body's origin given a world point.
        /// </summary>
        /// <param name="worldPoint">A point in world coordinates.</param>
        /// <returns>Return the corresponding local point relative to the body's origin.</returns>
        public Vec2 GetLocalPoint(Vec2 worldPoint)
        {
            return Common.Math.MulT(_xf, worldPoint);
        }

        /// <summary>
        /// Gets a local vector given a world vector.
        /// </summary>
        /// <param name="worldVector">A vector in world coordinates.</param>
        /// <returns>Return the corresponding local vector.</returns>
        public Vec2 GetLocalVector(Vec2 worldVector)
        {
            return Common.Math.MulT(_xf.R, worldVector);
        }

        /// <summary>
        /// Get the world linear velocity of a world point attached to this body.
        /// </summary>
        /// <param name="worldPoint">A point in world coordinates.</param>
        /// <returns>The world velocity of a point.</returns>
        public Vec2 GetLinearVelocityFromWorldPoint(Vec2 worldPoint)
        {
            return _linearVelocity + Vec2.Cross(_angularVelocity, worldPoint - _sweep.C);
        }

        /// <summary>
        /// Get the world velocity of a local point.
        /// </summary>
        /// <param name="localPoint">A point in local coordinates.</param>
        /// <returns>The world velocity of a point.</returns>
        public Vec2 GetLinearVelocityFromLocalPoint(Vec2 localPoint)
        {
            return GetLinearVelocityFromWorldPoint(GetWorldPoint(localPoint));
        }

        public float GetLinearDamping()
        {
            return _linearDamping;
        }

        public void SetLinearDamping(float linearDamping)
        {
            _linearDamping = linearDamping;
        }

        public float GetAngularDamping()
        {
            return _angularDamping;
        }

        public void SetAngularDamping(float angularDamping)
        {
            _angularDamping = angularDamping;
        }

        /// <summary>
        /// Should this body be treated like a bullet for continuous collision detection?
        /// </summary>
        /// <param name="flag"></param>
        public void SetBullet(bool flag)
        {
            if (flag)
            {
                _flags |= BodyFlags.BulletFlag;
            }
            else
            {
                _flags &= ~BodyFlags.BulletFlag;
            }
        }

        /// <summary>
        /// Is this body treated like a bullet for continuous collision detection?
        /// </summary>
        /// <returns></returns>
        public bool IsBullet()
        {
            return (_flags & BodyFlags.BulletFlag) == BodyFlags.BulletFlag;
        }

        public bool IsFixedRotation()
        {
            return (_flags & BodyFlags.FixedRotationFlag) == BodyFlags.FixedRotationFlag;
        }

        public void SetFixedRotation(bool flag)
        {
            if (flag)
            {
                _flags |= BodyFlags.FixedRotationFlag;
            }
            else
            {
                _flags &= ~BodyFlags.FixedRotationFlag;
            }

            ResetMassData();
        }

        /// <summary>
        /// Is this body static (immovable)?
        /// </summary>
        /// <returns></returns>
        public bool IsStatic()
        {
            return _type == BodyType.Static;
        }

        /// <summary>
        /// Is this body dynamic (movable)?
        /// </summary>
        /// <returns></returns>
        public bool IsDynamic()
        {
            return _type == BodyType.Dynamic;
        }

        public bool IsActive()
        {
            return (_flags & BodyFlags.ActiveFlag) == BodyFlags.ActiveFlag;
        }

        /// <summary>
        /// Is this body allowed to sleep
        /// </summary>
        /// <returns></returns>
        public bool IsSleepingAllowed()
        {
            return (_flags & BodyFlags.AutoSleepFlag) == BodyFlags.AutoSleepFlag;
        }

        /// <summary>
        /// You can disable sleeping on this body. If you disable sleeping, the
        /// body will be woken.
        /// </summary>
        /// <param name="flag"></param>
        public void SetSleepingAllowed(bool flag)
        {
            if (flag)
            {
                _flags |= BodyFlags.AutoSleepFlag;
            }
            else
            {
                _flags &= ~BodyFlags.AutoSleepFlag;
                SetAwake(true);
            }
        }

        /// <summary>
        /// Set the sleep state of the body. A sleeping body has very
        /// low CPU cost.
        /// @param flag set to true to put body to sleep, false to wake it.
        /// </summary>
        public void SetAwake(bool flag)
        {
            if (flag)
            {
                _flags |= BodyFlags.AwakeFlag;
                _sleepTime = 0.0f;
            }
            else
            {
                _flags &= ~BodyFlags.AwakeFlag;
                _sleepTime = 0.0f;
                _linearVelocity.SetZero();
                _angularVelocity = 0.0f;
                _force.SetZero();
                _torque = 0.0f;
            }
        }

        /// <summary>
        /// Is this body sleeping (not simulating).
        /// </summary>
        /// <returns></returns>
        public bool IsAwake()
        {
            return (_flags & BodyFlags.AwakeFlag) == BodyFlags.AwakeFlag;
        }

        /// <summary>
        /// Get the list of all fixtures attached to this body.
        /// </summary>
        /// <returns></returns>
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
        /// <param name="data"></param>
        public void SetUserData(object data) { _userData = data; }

        /// <summary>
        /// Get the parent world of this body.
        /// </summary>
        /// <returns></returns>
        public World GetWorld() { return _world; }

        internal void SynchronizeFixtures()
        {
            Transform xf1 = new Transform();
            xf1.R.Set(_sweep.A0);
            xf1.Position = _sweep.C0 - Math.Mul(xf1.R, _sweep.LocalCenter);

            BroadPhase broadPhase = _world._contactManager._broadPhase;
            for (Fixture f = _fixtureList; f != null; f = f._next)
            {
                f.Synchronize(broadPhase, xf1, _xf);
            }
        }

        internal void SynchronizeTransform()
        {
            _xf.R.Set(_sweep.A);
            _xf.Position = _sweep.C - Math.Mul(_xf.R, _sweep.LocalCenter);
        }

        // This is used to prevent connected bodies from colliding.
        // It may lie, depending on the collideConnected flag.
        public bool ShouldCollide(Body other)
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
            _sweep.C = _sweep.C0;
            _sweep.A = _sweep.A0;
            SynchronizeTransform();
        }

        public void SetActive(bool flag)
        {
            if (flag == IsActive())
            {
                return;
            }

            if (flag)
            {
                _flags |= BodyFlags.ActiveFlag;

                // Create all proxies.
                BroadPhase broadPhase = _world._contactManager._broadPhase;
                for (Fixture f = _fixtureList; f != null; f = f._next)
                {
                    f.CreateProxy(broadPhase, _xf);
                }

                // Contacts are created the next time step.
            }
            else
            {
                _flags &= ~BodyFlags.ActiveFlag;

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
    }
}
