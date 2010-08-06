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
        internal float AngularVelocityInternal;
        internal int FixtureCount;
        internal BodyFlags Flags;
        internal Vector2 Force;
        internal float I;
        internal float InvI;
        internal float InvMass;
        internal Vector2 LinearVelocityInternal;
        internal float SleepTime;
        internal Sweep Sweep; // the swept motion for CCD
        internal float Torque;
        internal BodyType Type;
        internal World World;
        internal Transform Xf; // the body origin transform

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

        /// Get the type of this body.
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

        /// Set the linear velocity of the center of mass.
        /// @param v the new linear velocity of the center of mass.
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

        /// Set the angular velocity.
        /// @param omega the new angular velocity in radians/second.
        public float AngularVelocity
        {
            set
            {
                if (Type == BodyType.Static)
                {
                    return;
                }

                if (value*value > 0.0f)
                {
                    Awake = true;
                }

                AngularVelocityInternal = value;
            }
            get { return AngularVelocityInternal; }
        }

        /// Get the total mass of the body.
        /// @return the mass, usually in kilograms (kg).
        public float Mass { get; internal set; }

        /// Get the rotational inertia of the body about the local origin.
        /// @return the rotational inertia, usually in kg-m^2.
        public float Inertia
        {
            get { return I + Mass*Vector2.Dot(Sweep.localCenter, Sweep.localCenter); }
        }

        /// Get the linear damping of the body.
        public float LinearDamping { get; set; }

        /// Get the angular damping of the body.
        public float AngularDamping { get; set; }

        /// Should this body be treated like a bullet for continuous collision detection?
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

        /// You can disable sleeping on this body. If you disable sleeping, the
        /// body will be woken.
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

        /// Set the sleep state of the body. A sleeping body has very
        /// low CPU cost.
        /// @param flag set to true to put body to sleep, false to wake it.
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

        /// Set this body to have fixed rotation. This causes the mass
        /// to be reset.
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

        public Fixture FixtureList { get; set; }

        /// Get the list of all joints attached to this body.
        public JointEdge JointList { get; internal set; }

        /// Get the list of all contacts attached to this body.
        /// @warning this list changes during the time step and you may
        /// miss some collisions if you don't use ContactListener.
        public ContactEdge ContactList { get; internal set; }

        /// Set the user data. Use this to store your application specific data.
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

        public bool IsStatic
        {
            get { return Type == BodyType.Static; }
            set
            {
                if (value)
                    Type = BodyType.Static;
            }
        }

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

        /// Get the angle in radians.
        /// @return the current world rotation angle in radians.
        public float Angle
        {
            get { return Sweep.a; }
        }

        /// Get the world position of the center of mass.
        public Vector2 WorldCenter
        {
            get { return Sweep.c; }
        }

        /// Get the local position of the center of mass.
        public Vector2 LocalCenter
        {
            get { return Sweep.localCenter; }
        }

        /// Get the next body in the world's body list.
        public Body Next { get; internal set; }

        public Body Prev { get; internal set; }

        /// Creates a fixture and attach it to this body. Use this function if you need
        /// to set some fixture parameters, like friction. Otherwise you can create the
        /// fixture directly from a shape.
        /// If the density is non-zero, this function automatically updates the mass of the body.
        /// Contacts are not created until the next time step.
        /// @param def the fixture definition.
        /// @warning This function is locked during callbacks.
        public Fixture CreateFixture(Shape shape, float density)
        {
            Debug.Assert(World.IsLocked == false);
            if (World.IsLocked)
            {
                return null;
            }

            Fixture fixture = new Fixture();
            fixture.Create(this, shape, density);

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

        /// Destroy a fixture. This removes the fixture from the broad-phase and
        /// destroys all contacts associated with this fixture. This will	
        /// automatically adjust the mass of the body if the body is dynamic and the
        /// fixture has positive density.
        /// All fixtures attached to a body are implicitly destroyed when the body is destroyed.
        /// @param fixture the fixture to be removed.
        /// @warning This function is locked during callbacks.
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

        /// Set the position of the body's origin and rotation.
        /// This breaks any contacts and wakes the other bodies.
        /// Manipulating a body's transform may cause non-physical behavior.
        /// @param position the world position of the body's local origin.
        /// @param angle the world rotation in radians.
        public void SetTransform(Vector2 position, float angle)
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

            World.ContactManager.FindNewContacts();
        }

        // For teleporting a body without considering new contacts immediately.
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

        /// Get the body transform for the body's origin.
        /// @return the world transform of the body's origin.
        public void GetTransform(out Transform xf)
        {
            xf = Xf;
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
            ApplyForce(ref force, ref Xf.Position);
        }

        public void ApplyForce(Vector2 force)
        {
            ApplyForce(ref force, ref Xf.Position);
        }

        /// Apply a force at a world point. If the force is not
        /// applied at the center of mass, it will generate a torque and
        /// affect the angular velocity. This wakes up the body.
        /// @param force the world force vector, usually in Newtons (N).
        /// @param point the world position of the point of application.
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

        /// Apply a torque. This affects the angular velocity
        /// without affecting the linear velocity of the center of mass.
        /// This wakes up the body.
        /// @param torque about the z-axis (out of the screen), usually in N-m.
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

        /// Apply an impulse at a point. This immediately modifies the velocity.
        /// It also modifies the angular velocity if the point of application
        /// is not at the center of mass. This wakes up the body.
        /// @param impulse the world impulse vector, usually in N-seconds or kg-m/s.
        /// @param point the world position of the point of application.
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
            LinearVelocityInternal += InvMass*impulse;
            AngularVelocityInternal += InvI*MathUtils.Cross(point - Sweep.c, impulse);
        }

        /// Apply an angular impulse.  
        /// @param impulse the angular impulse in units of kg*m*m/s  
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

            AngularVelocityInternal += InvI*impulse;
        }

        /// Get the mass data of the body.
        /// @return a struct containing the mass, inertia and center of the body.
        public void GetMassData(out MassData massData)
        {
            massData = new MassData();
            massData.Mass = Mass;
            massData.Inertia = I + Mass*Vector2.Dot(Sweep.localCenter, Sweep.localCenter);
            massData.Center = Sweep.localCenter;
        }

        /// Set the mass properties to override the mass properties of the fixtures.
        /// Note that this changes the center of mass position.
        /// Note that creating or destroying fixtures can also alter the mass.
        /// This function has no effect if the body isn't dynamic.
        /// @param massData the mass properties.
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
            I = 0.0f;
            InvI = 0.0f;

            Mass = massData.Mass;

            if (Mass <= 0.0f)
            {
                Mass = 1.0f;
            }

            InvMass = 1.0f/Mass;


            if (massData.Inertia > 0.0f && (Flags & BodyFlags.FixedRotation) == 0)
            {
                I = massData.Inertia - Mass*Vector2.Dot(massData.Center, massData.Center);
                Debug.Assert(I > 0.0f);
                InvI = 1.0f/I;
            }

            // Move center of mass.
            Vector2 oldCenter = Sweep.c;
            Sweep.localCenter = massData.Center;
            Sweep.c0 = Sweep.c = MathUtils.Multiply(ref Xf, Sweep.localCenter);

            // Update center of mass velocity.
            LinearVelocityInternal += MathUtils.Cross(AngularVelocityInternal, Sweep.c - oldCenter);
        }

        /// This resets the mass properties to the sum of the mass properties of the fixtures.
        /// This normally does not need to be called unless you called SetMassData to override
        /// the mass and you later want to reset the mass.
        public void ResetMassData()
        {
            // Compute mass data from shapes. Each shape has its own density.
            Mass = 0.0f;
            InvMass = 0.0f;
            I = 0.0f;
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
                if (f.Density == 0.0f)
                {
                    continue;
                }

                MassData massData;
                f.GetMassData(out massData);
                Mass += massData.Mass;
                center += massData.Mass*massData.Center;
                I += massData.Inertia;
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
                InvMass = 1.0f/Mass;
                center *= InvMass;
            }
            else
            {
                // Force all dynamic bodies to have a positive mass.
                Mass = 1.0f;
                InvMass = 1.0f;
            }

            if (I > 0.0f && (Flags & BodyFlags.FixedRotation) == 0)
            {
                // Center the inertia about the center of mass.
                I -= Mass*Vector2.Dot(center, center);

                Debug.Assert(I > 0.0f);
                InvI = 1.0f/I;
            }
            else
            {
                I = 0.0f;
                InvI = 0.0f;
            }

            // Move center of mass.
            Vector2 oldCenter = Sweep.c;
            Sweep.localCenter = center;
            Sweep.c0 = Sweep.c = MathUtils.Multiply(ref Xf, Sweep.localCenter);

            // Update center of mass velocity.
            LinearVelocityInternal += MathUtils.Cross(AngularVelocityInternal, Sweep.c - oldCenter);
        }

        /// Get the world coordinates of a point given the local coordinates.
        /// @param localPoint a point on the body measured relative the the body's origin.
        /// @return the same point expressed in world coordinates.
        public Vector2 GetWorldPoint(Vector2 localPoint)
        {
            return MathUtils.Multiply(ref Xf, localPoint);
        }

        /// Get the world coordinates of a vector given the local coordinates.
        /// @param localVector a vector fixed in the body.
        /// @return the same vector expressed in world coordinates.
        public Vector2 GetWorldVector(Vector2 localVector)
        {
            return MathUtils.Multiply(ref Xf.R, localVector);
        }

        /// Gets a local point relative to the body's origin given a world point.
        /// @param a point in world coordinates.
        /// @return the corresponding local point relative to the body's origin.
        public Vector2 GetLocalPoint(Vector2 worldPoint)
        {
            return MathUtils.MultiplyT(ref Xf, worldPoint);
        }

        /// Gets a local vector given a world vector.
        /// @param a vector in world coordinates.
        /// @return the corresponding local vector.
        public Vector2 GetLocalVector(Vector2 worldVector)
        {
            return MathUtils.MultiplyT(ref Xf.R, worldVector);
        }

        /// Get the world linear velocity of a world point attached to this body.
        /// @param a point in world coordinates.
        /// @return the world velocity of a point.
        public Vector2 GetLinearVelocityFromWorldPoint(Vector2 worldPoint)
        {
            return LinearVelocityInternal + MathUtils.Cross(AngularVelocityInternal, worldPoint - Sweep.c);
        }

        /// Get the world velocity of a local point.
        /// @param a point in local coordinates.
        /// @return the world velocity of a point.
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

        // This is used to prevent connected bodies from colliding.
        // It may lie, depending on the collideConnected flag.
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