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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.Broadphase;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Collision.Filtering;
using VelcroPhysics.Collision.Handlers;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Collision.TOI;
using VelcroPhysics.Dynamics.Joints;
using VelcroPhysics.Extensions.Controllers.ControllerBase;
using VelcroPhysics.Extensions.PhysicsLogics.PhysicsLogicBase;
using VelcroPhysics.Shared;
using VelcroPhysics.Templates;
using VelcroPhysics.Utilities;

namespace VelcroPhysics.Dynamics
{
    public class Body
    {
        private BodyType _type;
        private float _inertia;
        private float _mass;

        internal BodyFlags _flags;
        internal float _invI;
        internal float _invMass;
        internal Vector2 _force;
        internal Vector2 _linearVelocity;
        internal float _angularVelocity;
        internal Sweep _sweep; // the swept motion for CCD
        internal float _torque;
        internal World _world;
        public Transform _xf; // the body origin transform

        internal Body(World world, BodyTemplate template)
        {
            FixtureList = new List<Fixture>(1);

            if (template.AllowCCD)
                _flags |= BodyFlags.BulletFlag;
            if (template.AllowRotation)
                _flags |= BodyFlags.FixedRotationFlag;
            if (template.AllowSleep)
                _flags |= BodyFlags.AutoSleepFlag;
            if (template.Awake)
                _flags |= BodyFlags.AwakeFlag;
            if (template.Active)
                _flags |= BodyFlags.Enabled;

            _world = world;

            _xf.p = template.Position;
            _xf.q.Set(template.Angle);

            _sweep.C0 = _xf.p;
            _sweep.C = _xf.p;
            _sweep.A0 = template.Angle;
            _sweep.A = template.Angle;

            _linearVelocity = template.LinearVelocity;
            _angularVelocity = template.AngularVelocity;

            LinearDamping = template.LinearDamping;
            AngularDamping = template.AngularDamping;
            GravityScale = 1.0f;

            _type = template.Type;

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

            UserData = template.UserData;
        }

        public ControllerFilter ControllerFilter { get; set; }

        public PhysicsLogicFilter PhysicsLogicFilter { get; set; }

        /// <summary>
        /// A unique id for this body.
        /// </summary>
        public int BodyId { get; internal set; }

        public float SleepTime { get; set; }

        public int IslandIndex { get; set; }

        /// <summary>
        /// Scale the gravity applied to this body.
        /// Defaults to 1. A value of 2 means double the gravity is applied to this body.
        /// </summary>
        public float GravityScale { get; set; }

        /// <summary>
        /// Set the user data. Use this to store your application specific data.
        /// </summary>
        /// <value>The user data.</value>
        public object UserData { get; set; }

        /// <summary>
        /// Gets the total number revolutions the body has made.
        /// </summary>
        /// <value>The revolutions.</value>
        public float Revolutions => Rotation / (float)Math.PI;

        /// <summary>
        /// Gets or sets the body type.
        /// Warning: Calling this mid-update might cause a crash.
        /// </summary>
        /// <value>The type of body.</value>
        public BodyType BodyType
        {
            get { return _type; }
            set
            {
                if (value == _type)
                    return;

                _type = value;

                ResetMassData();

                if (_type == BodyType.Static)
                {
                    _linearVelocity = Vector2.Zero;
                    _angularVelocity = 0.0f;
                    _sweep.A0 = _sweep.A;
                    _sweep.C0 = _sweep.C;
                    SynchronizeFixtures();
                }

                Awake = true;

                _force = Vector2.Zero;
                _torque = 0.0f;

                // Delete the attached contacts.
                ContactEdge ce = ContactList;
                while (ce != null)
                {
                    ContactEdge ce0 = ce;
                    ce = ce.Next;
                    _world.ContactManager.Destroy(ce0.Contact);
                }

                ContactList = null;

                // Touch the proxies so that new contacts will be created (when appropriate)
                IBroadPhase broadPhase = _world.ContactManager.BroadPhase;
                foreach (Fixture fixture in FixtureList)
                {
                    int proxyCount = fixture.ProxyCount;
                    for (int j = 0; j < proxyCount; j++)
                    {
                        broadPhase.TouchProxy(fixture.Proxies[j].ProxyId);
                    }
                }
            }
        }

        /// <summary>
        /// Get or sets the linear velocity of the center of mass.
        /// </summary>
        /// <value>The linear velocity.</value>
        public Vector2 LinearVelocity
        {
            get { return _linearVelocity; }
            set
            {
                Debug.Assert(!float.IsNaN(value.X) && !float.IsNaN(value.Y));

                if (_type == BodyType.Static)
                    return;

                if (Vector2.Dot(value, value) > 0.0f)
                    Awake = true;

                _linearVelocity = value;
            }
        }

        /// <summary>
        /// Gets or sets the angular velocity. Radians/second.
        /// </summary>
        /// <value>The angular velocity.</value>
        public float AngularVelocity
        {
            get { return _angularVelocity; }
            set
            {
                Debug.Assert(!float.IsNaN(value));

                if (_type == BodyType.Static)
                    return;

                if (value * value > 0.0f)
                    Awake = true;

                _angularVelocity = value;
            }
        }

        /// <summary>
        /// Gets or sets the linear damping.
        /// </summary>
        /// <value>The linear damping.</value>
        public float LinearDamping { get; set; }

        /// <summary>
        /// Gets or sets the angular damping.
        /// </summary>
        /// <value>The angular damping.</value>
        public float AngularDamping { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this body should be included in the CCD solver.
        /// </summary>
        /// <value><c>true</c> if this instance is included in CCD; otherwise, <c>false</c>.</value>
        public bool IsBullet
        {
            get { return (_flags & BodyFlags.BulletFlag) == BodyFlags.BulletFlag; }
            set
            {
                if (value)
                    _flags |= BodyFlags.BulletFlag;
                else
                    _flags &= ~BodyFlags.BulletFlag;
            }
        }

        /// <summary>
        /// You can disable sleeping on this body. If you disable sleeping, the
        /// body will be woken.
        /// </summary>
        /// <value><c>true</c> if sleeping is allowed; otherwise, <c>false</c>.</value>
        public bool SleepingAllowed
        {
            get { return (_flags & BodyFlags.AutoSleepFlag) == BodyFlags.AutoSleepFlag; }
            set
            {
                if (value)
                    _flags |= BodyFlags.AutoSleepFlag;
                else
                {
                    _flags &= ~BodyFlags.AutoSleepFlag;
                    Awake = true;
                }
            }
        }

        /// <summary>
        /// Set the sleep state of the body. A sleeping body has very
        /// low CPU cost.
        /// </summary>
        /// <value><c>true</c> if awake; otherwise, <c>false</c>.</value>
        public bool Awake
        {
            get { return (_flags & BodyFlags.AwakeFlag) == BodyFlags.AwakeFlag; }
            set
            {
                if (value)
                {
                    _flags |= BodyFlags.AwakeFlag;
                    SleepTime = 0.0f;
                }
                else
                {
                    _flags &= ~BodyFlags.AwakeFlag;
                    ResetDynamics();
                    SleepTime = 0.0f;
                }
            }
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
        /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
        public bool Enabled
        {
            get { return (_flags & BodyFlags.Enabled) == BodyFlags.Enabled; }

            set
            {
                if (value == Enabled)
                    return;

                if (value)
                {
                    _flags |= BodyFlags.Enabled;

                    // Create all proxies.
                    IBroadPhase broadPhase = _world.ContactManager.BroadPhase;
                    for (int i = 0; i < FixtureList.Count; i++)
                    {
                        FixtureList[i].CreateProxies(broadPhase, ref _xf);
                    }

                    // Contacts are created the next time step.
                }
                else
                {
                    _flags &= ~BodyFlags.Enabled;

                    // Destroy all proxies.
                    IBroadPhase broadPhase = _world.ContactManager.BroadPhase;

                    for (int i = 0; i < FixtureList.Count; i++)
                    {
                        FixtureList[i].DestroyProxies(broadPhase);
                    }

                    // Destroy the attached contacts.
                    ContactEdge ce = ContactList;
                    while (ce != null)
                    {
                        ContactEdge ce0 = ce;
                        ce = ce.Next;
                        _world.ContactManager.Destroy(ce0.Contact);
                    }
                    ContactList = null;
                }
            }
        }

        /// <summary>
        /// Set this body to have fixed rotation. This causes the mass
        /// to be reset.
        /// </summary>
        /// <value><c>true</c> if it has fixed rotation; otherwise, <c>false</c>.</value>
        public bool FixedRotation
        {
            get { return (_flags & BodyFlags.FixedRotationFlag) == BodyFlags.FixedRotationFlag; }
            set
            {
                if (value == FixedRotation)
                    return;

                if (value)
                    _flags |= BodyFlags.FixedRotationFlag;
                else
                    _flags &= ~BodyFlags.FixedRotationFlag;

                _angularVelocity = 0f;
                ResetMassData();
            }
        }

        /// <summary>
        /// Gets all the fixtures attached to this body.
        /// </summary>
        /// <value>The fixture list.</value>
        public List<Fixture> FixtureList { get; internal set; }

        /// <summary>
        /// Get the list of all joints attached to this body.
        /// </summary>
        /// <value>The joint list.</value>
        public JointEdge JointList { get; internal set; }

        /// <summary>
        /// Get the list of all contacts attached to this body.
        /// Warning: this list changes during the time step and you may
        /// miss some collisions if you don't use ContactListener.
        /// </summary>
        /// <value>The contact list.</value>
        public ContactEdge ContactList { get; internal set; }

        /// <summary>
        /// Get the world body origin position.
        /// </summary>
        /// <returns>Return the world position of the body's origin.</returns>
        public Vector2 Position
        {
            get { return _xf.p; }
            set
            {
                Debug.Assert(!float.IsNaN(value.X) && !float.IsNaN(value.Y));

                SetTransform(ref value, Rotation);
            }
        }

        /// <summary>
        /// Get the angle in radians.
        /// </summary>
        /// <returns>Return the current world rotation angle in radians.</returns>
        public float Rotation
        {
            get { return _sweep.A; }
            set
            {
                Debug.Assert(!float.IsNaN(value));

                SetTransform(ref _xf.p, value);
            }
        }

        //Velcro: We don't add a setter here since it requires a branch, and we only use it internally
        internal bool IsIsland => (_flags & BodyFlags.IslandFlag) == BodyFlags.IslandFlag;

        public bool IsStatic => _type == BodyType.Static;

        public bool IsKinematic => _type == BodyType.Kinematic;

        public bool IsDynamic => _type == BodyType.Dynamic;

        /// <summary>
        /// Gets or sets a value indicating whether this body ignores gravity.
        /// </summary>
        /// <value><c>true</c> if it ignores gravity; otherwise, <c>false</c>.</value>
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

        /// <summary>
        /// Get the world position of the center of mass.
        /// </summary>
        /// <value>The world position.</value>
        public Vector2 WorldCenter => _sweep.C;

        /// <summary>
        /// Get the local position of the center of mass.
        /// </summary>
        /// <value>The local position.</value>
        public Vector2 LocalCenter
        {
            get { return _sweep.LocalCenter; }
            set
            {
                if (_type != BodyType.Dynamic)
                    return;

                //Velcro: We support setting the mass independently

                // Move center of mass.
                Vector2 oldCenter = _sweep.C;
                _sweep.LocalCenter = value;
                _sweep.C0 = _sweep.C = MathUtils.Mul(ref _xf, ref _sweep.LocalCenter);

                // Update center of mass velocity.
                Vector2 a = _sweep.C - oldCenter;
                _linearVelocity += new Vector2(-_angularVelocity * a.Y, _angularVelocity * a.X);
            }
        }

        /// <summary>
        /// Gets or sets the mass. Usually in kilograms (kg).
        /// </summary>
        /// <value>The mass.</value>
        public float Mass
        {
            get { return _mass; }
            set
            {
                Debug.Assert(!float.IsNaN(value));

                if (_type != BodyType.Dynamic)
                    return;

                //Velcro: We support setting the mass independently
                _mass = value;

                if (_mass <= 0.0f)
                    _mass = 1.0f;

                _invMass = 1.0f / _mass;
            }
        }

        /// <summary>
        /// Get or set the rotational inertia of the body about the local origin. usually in kg-m^2.
        /// </summary>
        /// <value>The inertia.</value>
        public float Inertia
        {
            get { return _inertia + _mass * Vector2.Dot(_sweep.LocalCenter, _sweep.LocalCenter); }
            set
            {
                Debug.Assert(!float.IsNaN(value));

                if (_type != BodyType.Dynamic)
                    return;

                //Velcro: We support setting the inertia independently
                if (value > 0.0f && !FixedRotation)
                {
                    _inertia = value - _mass * Vector2.Dot(_sweep.LocalCenter, _sweep.LocalCenter);
                    Debug.Assert(_inertia > 0.0f);
                    _invI = 1.0f / _inertia;
                }
            }
        }

        public float Restitution
        {
            set
            {
                for (int i = 0; i < FixtureList.Count; i++)
                {
                    Fixture f = FixtureList[i];
                    f.Restitution = value;
                }
            }
        }

        public float Friction
        {
            set
            {
                for (int i = 0; i < FixtureList.Count; i++)
                {
                    Fixture f = FixtureList[i];
                    f.Friction = value;
                }
            }
        }

        public Category CollisionCategories
        {
            set
            {
                for (int i = 0; i < FixtureList.Count; i++)
                {
                    Fixture f = FixtureList[i];
                    f.CollisionCategories = value;
                }
            }
        }

        public Category CollidesWith
        {
            set
            {
                for (int i = 0; i < FixtureList.Count; i++)
                {
                    Fixture f = FixtureList[i];
                    f.CollidesWith = value;
                }
            }
        }

        /// <summary>
        /// Body objects can define which categories of bodies they wish to ignore CCD with.
        /// This allows certain bodies to be configured to ignore CCD with objects that
        /// aren't a penetration problem due to the way content has been prepared.
        /// This is compared against the other Body's fixture CollisionCategories within World.SolveTOI().
        /// </summary>
        public Category IgnoreCCDWith
        {
            set
            {
                for (int i = 0; i < FixtureList.Count; i++)
                {
                    Fixture f = FixtureList[i];
                    f.IgnoreCCDWith = value;
                }
            }
        }

        public short CollisionGroup
        {
            set
            {
                for (int i = 0; i < FixtureList.Count; i++)
                {
                    Fixture f = FixtureList[i];
                    f.CollisionGroup = value;
                }
            }
        }

        public bool IsSensor
        {
            set
            {
                for (int i = 0; i < FixtureList.Count; i++)
                {
                    Fixture f = FixtureList[i];
                    f.IsSensor = value;
                }
            }
        }

        public bool IgnoreCCD
        {
            get { return (_flags & BodyFlags.IgnoreCCD) == BodyFlags.IgnoreCCD; }
            set
            {
                if (value)
                    _flags |= BodyFlags.IgnoreCCD;
                else
                    _flags &= ~BodyFlags.IgnoreCCD;
            }
        }

        /// <summary>
        /// Resets the dynamics of this body.
        /// Sets torque, force and linear/angular velocity to 0
        /// </summary>
        public void ResetDynamics()
        {
            _torque = 0;
            _angularVelocity = 0;
            _force = Vector2.Zero;
            _linearVelocity = Vector2.Zero;
        }

        /// <summary>
        /// Creates a fixture and attach it to this body.
        /// If the density is non-zero, this function automatically updates the mass of the body.
        /// Contacts are not created until the next time step.
        /// Warning: This function is locked during callbacks.
        /// </summary>
        public Fixture CreateFixture(FixtureTemplate template)
        {
            Fixture f = new Fixture(this, template);
            f.FixtureId = _world._fixtureIdCounter++;
            return f;
        }

        public Fixture CreateFixture(Shape shape, object userData = null)
        {
            FixtureTemplate template = new FixtureTemplate();
            template.Shape = shape;
            template.UserData = userData;

            return CreateFixture(template);
        }

        /// <summary>
        /// Destroy a fixture. This removes the fixture from the broad-phase and
        /// destroys all contacts associated with this fixture. This will
        /// automatically adjust the mass of the body if the body is dynamic and the
        /// fixture has positive density.
        /// All fixtures attached to a body are implicitly destroyed when the body is destroyed.
        /// Warning: This function is locked during callbacks.
        /// </summary>
        /// <param name="fixture">The fixture to be removed.</param>
        public void DestroyFixture(Fixture fixture)
        {
            if (fixture == null)
                return;

            Debug.Assert(fixture.Body == this);

            // Remove the fixture from this body's singly linked list.
            Debug.Assert(FixtureList.Count > 0);

            // You tried to remove a fixture that not present in the fixturelist.
            Debug.Assert(FixtureList.Contains(fixture));

            // Destroy any contacts associated with the fixture.
            ContactEdge edge = ContactList;
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
                    _world.ContactManager.Destroy(c);
                }
            }

            if (Enabled)
            {
                IBroadPhase broadPhase = _world.ContactManager.BroadPhase;
                fixture.DestroyProxies(broadPhase);
            }

            FixtureList.Remove(fixture);
            fixture.Destroy();
            fixture.Body = null;

            ResetMassData();
        }

        /// <summary>
        /// Set the position of the body's origin and rotation.
        /// This breaks any contacts and wakes the other bodies.
        /// Manipulating a body's transform may cause non-physical behavior.
        /// </summary>
        /// <param name="position">The world position of the body's local origin.</param>
        /// <param name="rotation">The world rotation in radians.</param>
        public void SetTransform(ref Vector2 position, float rotation)
        {
            SetTransformIgnoreContacts(ref position, rotation);

            //Velcro: We check for new contacts after a body has been moved.
            _world.ContactManager.FindNewContacts();
        }

        /// <summary>
        /// Set the position of the body's origin and rotation.
        /// This breaks any contacts and wakes the other bodies.
        /// Manipulating a body's transform may cause non-physical behavior.
        /// </summary>
        /// <param name="position">The world position of the body's local origin.</param>
        /// <param name="rotation">The world rotation in radians.</param>
        public void SetTransform(Vector2 position, float rotation)
        {
            SetTransform(ref position, rotation);
        }

        /// <summary>
        /// For teleporting a body without considering new contacts immediately.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="angle">The angle.</param>
        public void SetTransformIgnoreContacts(ref Vector2 position, float angle)
        {
            _xf.q.Set(angle);
            _xf.p = position;

            _sweep.C = MathUtils.Mul(ref _xf, _sweep.LocalCenter);
            _sweep.A = angle;

            _sweep.C0 = _sweep.C;
            _sweep.A0 = angle;

            IBroadPhase broadPhase = _world.ContactManager.BroadPhase;
            for (int i = 0; i < FixtureList.Count; i++)
            {
                FixtureList[i].Synchronize(broadPhase, ref _xf, ref _xf);
            }
        }

        /// <summary>
        /// Get the body transform for the body's origin.
        /// </summary>
        /// <param name="transform">The transform of the body's origin.</param>
        public void GetTransform(out Transform transform)
        {
            transform = _xf;
        }

        /// <summary>
        /// Apply a force at a world point. If the force is not
        /// applied at the center of mass, it will generate a torque and
        /// affect the angular velocity. This wakes up the body.
        /// </summary>
        /// <param name="force">The world force vector, usually in Newtons (N).</param>
        /// <param name="point">The world position of the point of application.</param>
        public void ApplyForce(Vector2 force, Vector2 point)
        {
            ApplyForce(ref force, ref point);
        }

        /// <summary>
        /// Applies a force at the center of mass.
        /// </summary>
        /// <param name="force">The force.</param>
        public void ApplyForce(ref Vector2 force)
        {
            ApplyForce(ref force, ref _xf.p);
        }

        /// <summary>
        /// Applies a force at the center of mass.
        /// </summary>
        /// <param name="force">The force.</param>
        public void ApplyForce(Vector2 force)
        {
            ApplyForce(ref force, ref _xf.p);
        }

        /// <summary>
        /// Apply a force at a world point. If the force is not
        /// applied at the center of mass, it will generate a torque and
        /// affect the angular velocity. This wakes up the body.
        /// </summary>
        /// <param name="force">The world force vector, usually in Newtons (N).</param>
        /// <param name="point">The world position of the point of application.</param>
        public void ApplyForce(ref Vector2 force, ref Vector2 point)
        {
            Debug.Assert(!float.IsNaN(force.X));
            Debug.Assert(!float.IsNaN(force.Y));
            Debug.Assert(!float.IsNaN(point.X));
            Debug.Assert(!float.IsNaN(point.Y));

            if (_type != BodyType.Dynamic)
                return;

            //Velcro: We always wake the body. You told it to move.
            if (Awake == false)
                Awake = true;

            _force += force;
            _torque += MathUtils.Cross(point - _sweep.C, force);
        }

        /// <summary>
        /// Apply a torque. This affects the angular velocity
        /// without affecting the linear velocity of the center of mass.
        /// </summary>
        /// <param name="torque">The torque about the z-axis (out of the screen), usually in N-m.</param>
        public void ApplyTorque(float torque)
        {
            Debug.Assert(!float.IsNaN(torque));

            if (BodyType != BodyType.Dynamic)
                return;

            //Velcro: We always wake the body. You told it to move.
            if (Awake == false)
                Awake = true;

            _torque += torque;
        }

        /// <summary>
        /// Apply an impulse at a point. This immediately modifies the velocity.
        /// This wakes up the body.
        /// </summary>
        /// <param name="impulse">The world impulse vector, usually in N-seconds or kg-m/s.</param>
        public void ApplyLinearImpulse(Vector2 impulse)
        {
            ApplyLinearImpulse(ref impulse);
        }

        /// <summary>
        /// Apply an impulse at a point. This immediately modifies the velocity.
        /// It also modifies the angular velocity if the point of application
        /// is not at the center of mass.
        /// This wakes up the body.
        /// </summary>
        /// <param name="impulse">The world impulse vector, usually in N-seconds or kg-m/s.</param>
        /// <param name="point">The world position of the point of application.</param>
        public void ApplyLinearImpulse(Vector2 impulse, Vector2 point)
        {
            ApplyLinearImpulse(ref impulse, ref point);
        }

        /// <summary>
        /// Apply an impulse at a point. This immediately modifies the velocity.
        /// This wakes up the body.
        /// </summary>
        /// <param name="impulse">The world impulse vector, usually in N-seconds or kg-m/s.</param>
        public void ApplyLinearImpulse(ref Vector2 impulse)
        {
            if (_type != BodyType.Dynamic)
                return;

            //Velcro: We always wake the body. You told it to move.
            if (Awake == false)
                Awake = true;

            _linearVelocity += _invMass * impulse;
        }

        /// <summary>
        /// Apply an impulse at a point. This immediately modifies the velocity.
        /// It also modifies the angular velocity if the point of application
        /// is not at the center of mass.
        /// This wakes up the body.
        /// </summary>
        /// <param name="impulse">The world impulse vector, usually in N-seconds or kg-m/s.</param>
        /// <param name="point">The world position of the point of application.</param>
        public void ApplyLinearImpulse(ref Vector2 impulse, ref Vector2 point)
        {
            if (_type != BodyType.Dynamic)
                return;

            //Velcro: We always wake the body. You told it to move.
            if (Awake == false)
                Awake = true;

            _linearVelocity += _invMass * impulse;
            _angularVelocity += _invI * MathUtils.Cross(point - _sweep.C, impulse);
        }

        /// <summary>
        /// Apply an angular impulse.
        /// </summary>
        /// <param name="impulse">The angular impulse in units of kg*m*m/s.</param>
        public void ApplyAngularImpulse(float impulse)
        {
            if (_type != BodyType.Dynamic)
                return;

            //Velcro: We always wake the body. You told it to move.
            if (Awake == false)
                Awake = true;

            _angularVelocity += _invI * impulse;
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
            _inertia = 0.0f;
            _invI = 0.0f;
            _sweep.LocalCenter = Vector2.Zero;

            //Velcro: We have mass on static bodies to support attaching joints to them
            // Kinematic bodies have zero mass.
            if (BodyType == BodyType.Kinematic)
            {
                _sweep.C0 = _xf.p;
                _sweep.C = _xf.p;
                _sweep.A0 = _sweep.A;
                return;
            }

            Debug.Assert(BodyType == BodyType.Dynamic || BodyType == BodyType.Static);

            // Accumulate mass over all fixtures.
            Vector2 localCenter = Vector2.Zero;
            foreach (Fixture f in FixtureList)
            {
                if (f.Shape._density == 0.0f)
                    continue;

                MassData massData = f.Shape.MassData;
                _mass += massData.Mass;
                localCenter += massData.Mass * massData.Centroid;
                _inertia += massData.Inertia;
            }

            //Velcro: Static bodies only have mass, they don't have other properties. A little hacky tho...
            if (BodyType == BodyType.Static)
            {
                _sweep.C0 = _sweep.C = _xf.p;
                return;
            }

            // Compute center of mass.
            if (_mass > 0.0f)
            {
                _invMass = 1.0f / _mass;
                localCenter *= _invMass;
            }
            else
            {
                // Force all bodies to have a positive mass.
                _mass = 1.0f;
                _invMass = 1.0f;
            }

            if (_inertia > 0.0f && (_flags & BodyFlags.FixedRotationFlag) == 0)
            {
                // Center the inertia about the center of mass.
                _inertia -= _mass * Vector2.Dot(localCenter, localCenter);

                Debug.Assert(_inertia > 0.0f);
                _invI = 1.0f / _inertia;
            }
            else
            {
                _inertia = 0.0f;
                _invI = 0.0f;
            }

            // Move center of mass.
            Vector2 oldCenter = _sweep.C;
            _sweep.LocalCenter = localCenter;
            _sweep.C0 = _sweep.C = MathUtils.Mul(ref _xf, ref _sweep.LocalCenter);

            // Update center of mass velocity.
            Vector2 a = _sweep.C - oldCenter;
            _linearVelocity += new Vector2(-_angularVelocity * a.Y, _angularVelocity * a.X);
        }

        /// <summary>
        /// Get the world coordinates of a point given the local coordinates.
        /// </summary>
        /// <param name="localPoint">A point on the body measured relative the body's origin.</param>
        /// <returns>The same point expressed in world coordinates.</returns>
        public Vector2 GetWorldPoint(ref Vector2 localPoint)
        {
            return MathUtils.Mul(ref _xf, ref localPoint);
        }

        /// <summary>
        /// Get the world coordinates of a point given the local coordinates.
        /// </summary>
        /// <param name="localPoint">A point on the body measured relative the body's origin.</param>
        /// <returns>The same point expressed in world coordinates.</returns>
        public Vector2 GetWorldPoint(Vector2 localPoint)
        {
            return GetWorldPoint(ref localPoint);
        }

        /// <summary>
        /// Get the world coordinates of a vector given the local coordinates.
        /// Note that the vector only takes the rotation into account, not the position.
        /// </summary>
        /// <param name="localVector">A vector fixed in the body.</param>
        /// <returns>The same vector expressed in world coordinates.</returns>
        public Vector2 GetWorldVector(ref Vector2 localVector)
        {
            return MathUtils.Mul(ref _xf.q, localVector);
        }

        /// <summary>
        /// Get the world coordinates of a vector given the local coordinates.
        /// </summary>
        /// <param name="localVector">A vector fixed in the body.</param>
        /// <returns>The same vector expressed in world coordinates.</returns>
        public Vector2 GetWorldVector(Vector2 localVector)
        {
            return GetWorldVector(ref localVector);
        }

        /// <summary>
        /// Gets a local point relative to the body's origin given a world point.
        /// Note that the vector only takes the rotation into account, not the position.
        /// </summary>
        /// <param name="worldPoint">A point in world coordinates.</param>
        /// <returns>The corresponding local point relative to the body's origin.</returns>
        public Vector2 GetLocalPoint(ref Vector2 worldPoint)
        {
            return MathUtils.MulT(ref _xf, worldPoint);
        }

        /// <summary>
        /// Gets a local point relative to the body's origin given a world point.
        /// </summary>
        /// <param name="worldPoint">A point in world coordinates.</param>
        /// <returns>The corresponding local point relative to the body's origin.</returns>
        public Vector2 GetLocalPoint(Vector2 worldPoint)
        {
            return GetLocalPoint(ref worldPoint);
        }

        /// <summary>
        /// Gets a local vector given a world vector.
        /// Note that the vector only takes the rotation into account, not the position.
        /// </summary>
        /// <param name="worldVector">A vector in world coordinates.</param>
        /// <returns>The corresponding local vector.</returns>
        public Vector2 GetLocalVector(ref Vector2 worldVector)
        {
            return MathUtils.MulT(_xf.q, worldVector);
        }

        /// <summary>
        /// Gets a local vector given a world vector.
        /// Note that the vector only takes the rotation into account, not the position.
        /// </summary>
        /// <param name="worldVector">A vector in world coordinates.</param>
        /// <returns>The corresponding local vector.</returns>
        public Vector2 GetLocalVector(Vector2 worldVector)
        {
            return GetLocalVector(ref worldVector);
        }

        /// <summary>
        /// Get the world linear velocity of a world point attached to this body.
        /// </summary>
        /// <param name="worldPoint">A point in world coordinates.</param>
        /// <returns>The world velocity of a point.</returns>
        public Vector2 GetLinearVelocityFromWorldPoint(Vector2 worldPoint)
        {
            return GetLinearVelocityFromWorldPoint(ref worldPoint);
        }

        /// <summary>
        /// Get the world linear velocity of a world point attached to this body.
        /// </summary>
        /// <param name="worldPoint">A point in world coordinates.</param>
        /// <returns>The world velocity of a point.</returns>
        public Vector2 GetLinearVelocityFromWorldPoint(ref Vector2 worldPoint)
        {
            return _linearVelocity + MathUtils.Cross(_angularVelocity, worldPoint - _sweep.C);
        }

        /// <summary>
        /// Get the world velocity of a local point.
        /// </summary>
        /// <param name="localPoint">A point in local coordinates.</param>
        /// <returns>The world velocity of a point.</returns>
        public Vector2 GetLinearVelocityFromLocalPoint(Vector2 localPoint)
        {
            return GetLinearVelocityFromLocalPoint(ref localPoint);
        }

        /// <summary>
        /// Get the world velocity of a local point.
        /// </summary>
        /// <param name="localPoint">A point in local coordinates.</param>
        /// <returns>The world velocity of a point.</returns>
        public Vector2 GetLinearVelocityFromLocalPoint(ref Vector2 localPoint)
        {
            return GetLinearVelocityFromWorldPoint(GetWorldPoint(ref localPoint));
        }

        internal void SynchronizeFixtures()
        {
            Transform xf1 = new Transform();
            xf1.q.Set(_sweep.A0);
            xf1.p = _sweep.C0 - MathUtils.Mul(xf1.q, _sweep.LocalCenter);

            IBroadPhase broadPhase = _world.ContactManager.BroadPhase;
            for (int i = 0; i < FixtureList.Count; i++)
            {
                FixtureList[i].Synchronize(broadPhase, ref xf1, ref _xf);
            }
        }

        internal void SynchronizeTransform()
        {
            _xf.q.Set(_sweep.A);
            _xf.p = _sweep.C - MathUtils.Mul(_xf.q, _sweep.LocalCenter);
        }

        /// <summary>
        /// This is used to prevent connected bodies from colliding.
        /// It may lie, depending on the collideConnected flag.
        /// </summary>
        /// <param name="other">The other body.</param>
        internal bool ShouldCollide(Body other)
        {
            // At least one body should be dynamic.
            if (_type != BodyType.Dynamic && other._type != BodyType.Dynamic)
            {
                return false;
            }

            // Does a joint prevent collision?
            for (JointEdge jn = JointList; jn != null; jn = jn.Next)
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

        internal void Advance(float alpha)
        {
            // Advance to the new safe time. This doesn't sync the broad-phase.
            _sweep.Advance(alpha);
            _sweep.C = _sweep.C0;
            _sweep.A = _sweep.A0;
            _xf.q.Set(_sweep.A);
            _xf.p = _sweep.C - MathUtils.Mul(_xf.q, _sweep.LocalCenter);
        }

        public event OnCollisionHandler OnCollision
        {
            add
            {
                foreach (Fixture f in FixtureList)
                {
                    f.OnCollision += value;
                }
            }
            remove
            {
                foreach (Fixture f in FixtureList)
                {
                    f.OnCollision -= value;
                }
            }
        }

        public event OnSeparationHandler OnSeparation
        {
            add
            {
                foreach (Fixture f in FixtureList)
                {
                    f.OnSeparation += value;
                }
            }
            remove
            {
                foreach (Fixture f in FixtureList)
                {
                    f.OnSeparation -= value;
                }
            }
        }
    }
}