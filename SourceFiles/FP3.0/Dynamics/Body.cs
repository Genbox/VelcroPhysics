﻿/*
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
    /// <summary>
    /// The body type.
    /// </summary>
    public enum BodyType
    {
        /// <summary>
        /// Zero velocity, may be manually moved. Note: even static bodies have mass.
        /// </summary>
        Static,
        /// <summary>
        /// Zero mass, non-zero velocity set by user, moved by solver
        /// </summary>
        Kinematic,
        /// <summary>
        /// Positive mass, non-zero velocity determined by forces, moved by solver
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
        Toi = (1 << 6),
        IgnoreGravity = (1 << 7),
    }

    /// <summary>
    /// 
    /// </summary>
    public class Body
    {
        internal float AngularVelocityInternal;
        internal int FixtureCount;
        internal BodyFlags Flags;
        internal Vector2 Force;
        internal float InvI;
        internal float InvMass;
        internal Vector2 LinearVelocityInternal;
        internal float SleepTime;
        internal Sweep Sweep; // the swept motion for CCD
        internal float Torque;
        internal BodyType Type;
        internal World World;
        internal Transform Xf; // the body origin transform
        private float _inertia;

        internal Body(World world)
        {
            World = world;

            FixedRotation = false;
            IsBullet = false;
            SleepingAllowed = true;
            Awake = true;
            BodyType = BodyType.Static;
            Active = true;

            Xf.R.Set(0);
        }

        /// <summary>
        /// Gets the total number revolutions the body has made.
        /// </summary>
        /// <value>The revolutions.</value>
        public float Revolutions
        {
            get { return Rotation / (float)Math.PI; }
        }

        /// <summary>
        /// Gets or sets the body type.
        /// </summary>
        /// <value>The type of body.</value>
        public BodyType BodyType
        {
            get { return Type; }
            set
            {
                if (Type == value)
                {
                    return;
                }

                Type = value;

                ResetMassData();

                if (Type == BodyType.Static)
                {
                    LinearVelocityInternal = Vector2.Zero;
                    AngularVelocityInternal = 0.0f;
                }

                Awake = true;

                Force = Vector2.Zero;
                Torque = 0.0f;

                // Since the body type changed, we need to flag contacts for filtering.
                for (ContactEdge ce = ContactList; ce != null; ce = ce.Next)
                {
                    ce.Contact.FlagForFiltering();
                }
            }
        }

        /// <summary>
        /// Set the linear velocity of the center of mass.
        /// </summary>
        /// <value>The linear velocity.</value>
        public Vector2 LinearVelocity
        {
            set
            {
                if (Type == BodyType.Static)
                {
                    return;
                }

                if (Vector2.Dot(value, value) > 0.0f)
                {
                    Awake = true;
                }

                LinearVelocityInternal = value;
            }
            get { return LinearVelocityInternal; }
        }

        /// <summary>
        /// Gets or sets the angular velocity. Radians/second.
        /// </summary>
        /// <value>The angular velocity.</value>
        public float AngularVelocity
        {
            set
            {
                if (Type == BodyType.Static)
                {
                    return;
                }

                if (value * value > 0.0f)
                {
                    Awake = true;
                }

                AngularVelocityInternal = value;
            }
            get { return AngularVelocityInternal; }
        }

        /// <summary>
        /// Gets or sets the mass. Usually in kilograms (kg).
        /// </summary>
        /// <value>The mass.</value>
        public float Mass { get; internal set; }

        /// <summary>
        /// Get or set the rotational inertia of the body about the local origin. usually in kg-m^2.
        /// </summary>
        /// <value>The inertia.</value>
        public float Inertia
        {
            get { return _inertia + Mass * Vector2.Dot(Sweep.localCenter, Sweep.localCenter); }
            set { _inertia = value; }
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
            set
            {
                if (value)
                {
                    Flags |= BodyFlags.Bullet;
                }
                else
                {
                    Flags &= ~BodyFlags.Bullet;
                }
            }
            get { return (Flags & BodyFlags.Bullet) == BodyFlags.Bullet; }
        }

        /// <summary>
        /// You can disable sleeping on this body. If you disable sleeping, the
        /// body will be woken.
        /// </summary>
        /// <value><c>true</c> if sleeping is allowed; otherwise, <c>false</c>.</value>
        public bool SleepingAllowed
        {
            set
            {
                if (value)
                {
                    Flags |= BodyFlags.AutoSleep;
                }
                else
                {
                    Flags &= ~BodyFlags.AutoSleep;
                    Awake = true;
                }
            }
            get { return (Flags & BodyFlags.AutoSleep) == BodyFlags.AutoSleep; }
        }

        /// <summary>
        /// Set the sleep state of the body. A sleeping body has very
        /// low CPU cost.
        /// </summary>
        /// <value><c>true</c> if awake; otherwise, <c>false</c>.</value>
        public bool Awake
        {
            set
            {
                if (value)
                {
                    if ((Flags & BodyFlags.Awake) == 0)
                    {
                        Flags |= BodyFlags.Awake;
                        SleepTime = 0.0f;
                    }
                }
                else
                {
                    Flags &= ~BodyFlags.Awake;
                    SleepTime = 0.0f;
                    LinearVelocityInternal = Vector2.Zero;
                    AngularVelocityInternal = 0.0f;
                    Force = Vector2.Zero;
                    Torque = 0.0f;
                }
            }
            get { return (Flags & BodyFlags.Awake) == BodyFlags.Awake; }
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
                    Flags |= BodyFlags.Active;

                    // Create all proxies.
                    BroadPhase broadPhase = World.ContactManager.BroadPhase;
                    for (Fixture f = FixtureList; f != null; f = f.Next)
                    {
                        f.CreateProxies(broadPhase, ref Xf);
                    }

                    // Contacts are created the next time step.
                }
                else
                {
                    Flags &= ~BodyFlags.Active;

                    // Destroy all proxies.
                    BroadPhase broadPhase = World.ContactManager.BroadPhase;
                    for (Fixture f = FixtureList; f != null; f = f.Next)
                    {
                        f.DestroyProxies(broadPhase);
                    }

                    // Destroy the attached contacts.
                    ContactEdge ce = ContactList;
                    while (ce != null)
                    {
                        ContactEdge ce0 = ce;
                        ce = ce.Next;
                        World.ContactManager.Destroy(ce0.Contact);
                    }
                    ContactList = null;
                }
            }
            get { return (Flags & BodyFlags.Active) == BodyFlags.Active; }
        }

        /// <summary>
        /// Set this body to have fixed rotation. This causes the mass
        /// to be reset.
        /// </summary>
        /// <value><c>true</c> if it has fixed rotation; otherwise, <c>false</c>.</value>
        public bool FixedRotation
        {
            set
            {
                if (value)
                {
                    Flags |= BodyFlags.FixedRotation;
                }
                else
                {
                    Flags &= ~BodyFlags.FixedRotation;
                }

                ResetMassData();
            }
            get { return (Flags & BodyFlags.FixedRotation) == BodyFlags.FixedRotation; }
        }

        /// <summary>
        /// Gets all the fixtures attached to this body.
        /// </summary>
        /// <value>The fixture list.</value>
        public Fixture FixtureList { get; internal set; }

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
        /// Set the user data. Use this to store your application specific data.
        /// </summary>
        /// <value>The user data.</value>
        public object UserData { get; set; }

        /// <summary>
        /// Get the world body origin position.
        /// </summary>
        /// <returns>Return the world position of the body's origin.</returns>
        public Vector2 Position
        {
            get { return Xf.Position; }
            set { SetTransform(value, Rotation); }
        }

        /// <summary>
        /// Get the angle in radians.
        /// </summary>
        /// <returns>Return the current world rotation angle in radians.</returns>
        public float Rotation
        {
            get { return Sweep.a; }
            set { SetTransform(Position, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this body is static.
        /// </summary>
        /// <value><c>true</c> if this instance is static; otherwise, <c>false</c>.</value>
        public bool IsStatic
        {
            get { return Type == BodyType.Static; }
            set
            {
                if (value)
                    Type = BodyType.Static;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this body ignores gravity.
        /// </summary>
        /// <value><c>true</c> if  it ignores gravity; otherwise, <c>false</c>.</value>
        public bool IgnoreGravity
        {
            get { return (Flags & BodyFlags.IgnoreGravity) == BodyFlags.IgnoreGravity; }
            set
            {
                if (value)
                    Flags |= BodyFlags.IgnoreGravity;
                else
                    Flags &= ~BodyFlags.IgnoreGravity;
            }
        }

        /// <summary>
        /// Get the world position of the center of mass.
        /// </summary>
        /// <value>The world position.</value>
        public Vector2 WorldCenter
        {
            get { return Sweep.c; }
        }

        /// <summary>
        /// Get the local position of the center of mass.
        /// </summary>
        /// <value>The local position.</value>
        public Vector2 LocalCenter
        {
            get { return Sweep.localCenter; }
        }

        /// <summary>
        /// Get the next body in the world's body list.
        /// </summary>
        /// <value>The next.</value>
        public Body Next { get; internal set; }

        /// <summary>
        /// Get the previous body in the world's body list.
        /// </summary>
        /// <value>The prev.</value>
        public Body Prev { get; internal set; }

        /// <summary>
        /// Creates a fixture and attach it to this body.
        /// If the density is non-zero, this function automatically updates the mass of the body.
        /// Contacts are not created until the next time step.
        /// Warning: This function is locked during callbacks.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <param name="density">The density.</param>
        /// <returns></returns>
        public Fixture CreateFixture(Shape shape, float density)
        {
            Debug.Assert(World.IsLocked == false);
            if (World.IsLocked)
            {
                return null;
            }

            Fixture fixture = new Fixture(this, shape, density);

            if ((Flags & BodyFlags.Active) == BodyFlags.Active)
            {
                BroadPhase broadPhase = World.ContactManager.BroadPhase;
                fixture.CreateProxies(broadPhase, ref Xf);
            }

            fixture.Next = FixtureList;
            FixtureList = fixture;
            ++FixtureCount;

            fixture.Body = this;


            // Adjust mass properties if needed.
            if (fixture.Density > 0.0f)
            {
                ResetMassData();
            }

            // Let the world know we have a new fixture. This will cause new contacts
            // to be created at the beginning of the next time step.
            World.Flags |= WorldFlags.NewFixture;

            if (World.FixtureAdded != null)
            {
                World.FixtureAdded(fixture);
            }

            return fixture;
        }

        /// <summary>
        /// Creates a fixture with the supplied shape.
        /// Note: Default density is 1
        /// </summary>
        /// <param name="shape">The shape</param>
        /// <returns></returns>
        public Fixture CreateFixture(Shape shape)
        {
            return CreateFixture(shape, 1);
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
            Debug.Assert(World.IsLocked == false);
            if (World.IsLocked)
            {
                return;
            }

            Debug.Assert(fixture.Body == this);

            // Remove the fixture from this body's singly linked list.
            Debug.Assert(FixtureCount > 0);

#if DEBUG
            Fixture node = FixtureList;
            bool found = false;
            while (node != null)
            {
                if (node == fixture)
                {
                    FixtureList = fixture.Next;
                    found = true;
                    break;
                }

                node = node.Next;
            }

            // You tried to remove a shape that is not attached to this body.
            Debug.Assert(found);
#endif

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
                    World.ContactManager.Destroy(c);
                }
            }

            if ((Flags & BodyFlags.Active) == BodyFlags.Active)
            {
                BroadPhase broadPhase = World.ContactManager.BroadPhase;
                fixture.DestroyProxies(broadPhase);
            }

            fixture.Destroy();
            fixture.Body = null;
            fixture.Next = null;

            --FixtureCount;

            ResetMassData();
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
            Debug.Assert(World.IsLocked == false);
            if (World.IsLocked)
            {
                return;
            }

            Xf.R.Set(rotation);
            Xf.Position = position;

            Sweep.c0 = Sweep.c = MathUtils.Multiply(ref Xf, Sweep.localCenter);
            Sweep.a0 = Sweep.a = rotation;

            BroadPhase broadPhase = World.ContactManager.BroadPhase;
            for (Fixture f = FixtureList; f != null; f = f.Next)
            {
                f.Synchronize(broadPhase, ref Xf, ref Xf);
            }

            World.ContactManager.FindNewContacts();
        }

        /// <summary>
        // For teleporting a body without considering new contacts immediately.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="angle">The angle.</param>
        public void SetTransformIgnoreContacts(Vector2 position, float angle)
        {
            Debug.Assert(World.IsLocked == false);
            if (World.IsLocked)
            {
                return;
            }

            Xf.R.Set(angle);
            Xf.Position = position;

            Sweep.c0 = Sweep.c = MathUtils.Multiply(ref Xf, Sweep.localCenter);
            Sweep.a0 = Sweep.a = angle;

            BroadPhase broadPhase = World.ContactManager.BroadPhase;
            for (Fixture f = FixtureList; f != null; f = f.Next)
            {
                f.Synchronize(broadPhase, ref Xf, ref Xf);
            }
        }

        /// <summary>
        /// Get the body transform for the body's origin.
        /// </summary>
        /// <param name="transform">The transform of the body's origin.</param>
        public void GetTransform(out Transform transform)
        {
            transform = Xf;
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
            ApplyForce(ref force, ref Xf.Position);
        }

        /// <summary>
        /// Applies a force at the center of mass.
        /// </summary>
        /// <param name="force">The force.</param>
        public void ApplyForce(Vector2 force)
        {
            ApplyForce(ref force, ref Xf.Position);
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
            if (Type == BodyType.Dynamic)
            {
                if (Awake == false)
                {
                    Awake = true;
                }

                Force += force;
                Torque += MathUtils.Cross(point - Sweep.c, force);
            }
        }

        /// <summary>
        /// Apply a torque. This affects the angular velocity
        /// without affecting the linear velocity of the center of mass.
        /// This wakes up the body.
        /// </summary>
        /// <param name="torque">The torque about the z-axis (out of the screen), usually in N-m.</param>
        public void ApplyTorque(float torque)
        {
            if (Type == BodyType.Dynamic)
            {
                if (Awake == false)
                {
                    Awake = true;
                }

                Torque += torque;
            }
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
            if (Type != BodyType.Dynamic)
            {
                return;
            }
            if (Awake == false)
            {
                Awake = true;
            }
            LinearVelocityInternal += InvMass * impulse;
            AngularVelocityInternal += InvI * MathUtils.Cross(point - Sweep.c, impulse);
        }

        /// <summary>
        /// Apply an angular impulse.
        /// </summary>
        /// <param name="impulse">The angular impulse in units of kg*m*m/s.</param>
        public void ApplyAngularImpulse(float impulse)
        {
            if (Type != BodyType.Dynamic)
            {
                return;
            }

            if (Awake == false)
            {
                Awake = true;
            }

            AngularVelocityInternal += InvI * impulse;
        }

        /// <summary>
        /// Get the mass data of the body.
        /// </summary>
        /// <param name="massData">A struct containing the mass, inertia and center of the body.</param>
        public void GetMassData(out MassData massData)
        {
            massData = new MassData();
            massData.Mass = Mass;
            massData.Inertia = _inertia + Mass * Vector2.Dot(Sweep.localCenter, Sweep.localCenter);
            massData.Center = Sweep.localCenter;
        }

        /// <summary>
        /// Set the mass properties to override the mass properties of the fixtures.
        /// Note that this changes the center of mass position.
        /// Note that creating or destroying fixtures can also alter the mass.
        /// This function has no effect if the body isn't dynamic.
        /// </summary>
        /// <param name="massData">The mass data.</param>
        public void SetMassData(ref MassData massData)
        {
            Debug.Assert(World.IsLocked == false);
            if (World.IsLocked)
            {
                return;
            }

            if (Type != BodyType.Dynamic)
            {
                return;
            }

            InvMass = 0.0f;
            _inertia = 0.0f;
            InvI = 0.0f;

            Mass = massData.Mass;

            if (Mass <= 0.0f)
            {
                Mass = 1.0f;
            }

            InvMass = 1.0f / Mass;


            if (massData.Inertia > 0.0f && (Flags & BodyFlags.FixedRotation) == 0)
            {
                _inertia = massData.Inertia - Mass * Vector2.Dot(massData.Center, massData.Center);
                Debug.Assert(_inertia > 0.0f);
                InvI = 1.0f / _inertia;
            }

            // Move center of mass.
            Vector2 oldCenter = Sweep.c;
            Sweep.localCenter = massData.Center;
            Sweep.c0 = Sweep.c = MathUtils.Multiply(ref Xf, Sweep.localCenter);

            // Update center of mass velocity.
            LinearVelocityInternal += MathUtils.Cross(AngularVelocityInternal, Sweep.c - oldCenter);
        }

        /// <summary>
        /// This resets the mass properties to the sum of the mass properties of the fixtures.
        /// This normally does not need to be called unless you called SetMassData to override
        /// the mass and you later want to reset the mass.
        /// </summary>
        public void ResetMassData()
        {
            // Compute mass data from shapes. Each shape has its own density.
            Mass = 0.0f;
            InvMass = 0.0f;
            _inertia = 0.0f;
            InvI = 0.0f;
            Sweep.localCenter = Vector2.Zero;

            // Kinematic bodies have zero mass.
            if (BodyType == BodyType.Kinematic)
            {
                Sweep.c0 = Sweep.c = Xf.Position;
                return;
            }

            Debug.Assert(BodyType == BodyType.Dynamic || BodyType == BodyType.Static);

            // Accumulate mass over all fixtures.
            Vector2 center = Vector2.Zero;
            for (Fixture f = FixtureList; f != null; f = f.Next)
            {
                if (MathUtils.FloatEquals(f.Density, 0))
                {
                    continue;
                }

                MassData massData;
                f.GetMassData(out massData);
                Mass += massData.Mass;
                center += massData.Mass * massData.Center;
                _inertia += massData.Inertia;
            }

            //Static bodies only have mass, they don't have other properties. A little hacky tho...
            if (BodyType == BodyType.Static)
            {
                Sweep.c0 = Sweep.c = Xf.Position;
                return;
            }

            // Compute center of mass.
            if (Mass > 0.0f)
            {
                InvMass = 1.0f / Mass;
                center *= InvMass;
            }
            else
            {
                // Force all dynamic bodies to have a positive mass.
                Mass = 1.0f;
                InvMass = 1.0f;
            }

            if (_inertia > 0.0f && (Flags & BodyFlags.FixedRotation) == 0)
            {
                // Center the inertia about the center of mass.
                _inertia -= Mass * Vector2.Dot(center, center);

                Debug.Assert(_inertia > 0.0f);
                InvI = 1.0f / _inertia;
            }
            else
            {
                _inertia = 0.0f;
                InvI = 0.0f;
            }

            // Move center of mass.
            Vector2 oldCenter = Sweep.c;
            Sweep.localCenter = center;
            Sweep.c0 = Sweep.c = MathUtils.Multiply(ref Xf, Sweep.localCenter);

            // Update center of mass velocity.
            LinearVelocityInternal += MathUtils.Cross(AngularVelocityInternal, Sweep.c - oldCenter);
        }

        /// <summary>
        /// Get the world coordinates of a point given the local coordinates.
        /// </summary>
        /// <param name="localPoint">A point on the body measured relative the the body's origin.</param>
        /// <returns>The same point expressed in world coordinates.</returns>
        public Vector2 GetWorldPoint(Vector2 localPoint)
        {
            return MathUtils.Multiply(ref Xf, localPoint);
        }

        /// <summary>
        /// Get the world coordinates of a vector given the local coordinates.
        /// </summary>
        /// <param name="localVector">A vector fixed in the body.</param>
        /// <returns>The same vector expressed in world coordinates.</returns>
        public Vector2 GetWorldVector(Vector2 localVector)
        {
            return MathUtils.Multiply(ref Xf.R, localVector);
        }

        /// <summary>
        /// Gets a local point relative to the body's origin given a world point.
        /// </summary>
        /// <param name="worldPoint">A point in world coordinates.</param>
        /// <returns>The corresponding local point relative to the body's origin.</returns>
        public Vector2 GetLocalPoint(Vector2 worldPoint)
        {
            return MathUtils.MultiplyT(ref Xf, worldPoint);
        }

        /// <summary>
        /// Gets a local vector given a world vector.
        /// </summary>
        /// <param name="worldVector">A vector in world coordinates.</param>
        /// <returns>The corresponding local vector.</returns>
        public Vector2 GetLocalVector(Vector2 worldVector)
        {
            return MathUtils.MultiplyT(ref Xf.R, worldVector);
        }

        /// <summary>
        /// Get the world linear velocity of a world point attached to this body.
        /// </summary>
        /// <param name="worldPoint">A point in world coordinates.</param>
        /// <returns>The world velocity of a point.</returns>
        public Vector2 GetLinearVelocityFromWorldPoint(Vector2 worldPoint)
        {
            return LinearVelocityInternal + MathUtils.Cross(AngularVelocityInternal, worldPoint - Sweep.c);
        }

        /// <summary>
        /// Get the world velocity of a local point.
        /// </summary>
        /// <param name="localPoint">A point in local coordinates.</param>
        /// <returns>The world velocity of a point.</returns>
        public Vector2 GetLinearVelocityFromLocalPoint(Vector2 localPoint)
        {
            return GetLinearVelocityFromWorldPoint(GetWorldPoint(localPoint));
        }

        internal void SynchronizeFixtures()
        {
            Transform xf1 = new Transform();
            xf1.R.Set(Sweep.a0);
            xf1.Position = Sweep.c0 - MathUtils.Multiply(ref xf1.R, Sweep.localCenter);

            BroadPhase broadPhase = World.ContactManager.BroadPhase;
            for (Fixture f = FixtureList; f != null; f = f.Next)
            {
                f.Synchronize(broadPhase, ref xf1, ref Xf);
            }
        }

        internal void SynchronizeTransform()
        {
            Xf.R.Set(Sweep.a);
            Xf.Position = Sweep.c - MathUtils.Multiply(ref Xf.R, Sweep.localCenter);
        }

        /// <summary>
        // This is used to prevent connected bodies from colliding.
        // It may lie, depending on the collideConnected flag.
        /// </summary>
        /// <param name="other">The other body.</param>
        /// <returns></returns>
        internal bool ShouldCollide(Body other)
        {
            // At least one body should be dynamic.
            if (Type != BodyType.Dynamic && other.Type != BodyType.Dynamic)
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

        internal void Advance(float t)
        {
            // Advance to the new safe time.
            Sweep.Advance(t);
            Sweep.c = Sweep.c0;
            Sweep.a = Sweep.a0;
            SynchronizeTransform();
        }
    }
}