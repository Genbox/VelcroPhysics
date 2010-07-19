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
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics
{
    /// This is an internal class.
    internal class Island
    {
        public Body[] _bodies;
        public int _bodyCapacity;
        public int _bodyCount;
        public int _contactCapacity;
        public int _contactCount;
        private ContactSolver _contactSolver = new ContactSolver();
        public Contact[] _contacts;
        public int _jointCapacity;
        public int _jointCount;
        public Joint[] _joints;
        public ContactManager _listener;
        public int _positionIterationCount;

        public void Reset(int bodyCapacity, int contactCapacity, int jointCapacity, ContactManager listener)
        {
            _bodyCapacity = bodyCapacity;
            _contactCapacity = contactCapacity;
            _jointCapacity = jointCapacity;
            _bodyCount = 0;
            _jointCount = 0;

            _listener = listener;

            if (_bodies == null || _bodies.Length < bodyCapacity)
            {
                _bodies = new Body[bodyCapacity];
            }

            if (_contacts == null || _contacts.Length < contactCapacity)
            {
                _contacts = new Contact[contactCapacity * 2];
            }

            if (_joints == null || _joints.Length < jointCapacity)
            {
                _joints = new Joint[jointCapacity * 2];
            }
        }

        public void Clear()
        {
            _bodyCount = 0;
            _contactCount = 0;
            _jointCount = 0;
        }

        public void Solve(ref TimeStep step, Vector2 gravity)
        {
            // Integrate velocities and apply damping.
            for (int i = 0; i < _bodyCount; ++i)
            {
                Body b = _bodies[i];

                if (b.BodyType != BodyType.Dynamic)
                {
                    continue;
                }

                // Integrate velocities. Only apply gravity if the body wants it.
                if (b.IgnoreGravity)
                {
                    b._linearVelocity += step.dt * (b._invMass * b._force);
                    b._angularVelocity += step.dt * b._invI * b._torque;
                }
                else
                {
                    b._linearVelocity += step.dt * (gravity + b._invMass * b._force);
                    b._angularVelocity += step.dt * b._invI * b._torque;
                }

                // Apply damping.
                // ODE: dv/dt + c * v = 0
                // Solution: v(t) = v0 * exp(-c * t)
                // Time step: v(t + dt) = v0 * exp(-c * (t + dt)) = v0 * exp(-c * t) * exp(-c * dt) = v * exp(-c * dt)
                // v2 = exp(-c * dt) * v1
                // Taylor expansion:
                // v2 = (1.0f - c * dt) * v1
                b._linearVelocity *= MathUtils.Clamp(1.0f - step.dt * b._linearDamping, 0.0f, 1.0f);
                b._angularVelocity *= MathUtils.Clamp(1.0f - step.dt * b._angularDamping, 0.0f, 1.0f);
            }

            // Partition contacts so that contacts with static bodies are solved last.
            int i1 = -1;
            for (int i2 = 0; i2 < _contactCount; ++i2)
            {
                Fixture fixtureA = _contacts[i2].FixtureA;
                Fixture fixtureB = _contacts[i2].FixtureB;
                Body bodyA = fixtureA.Body;
                Body bodyB = fixtureB.Body;
                bool nonStatic = bodyA.BodyType != BodyType.Static && bodyB.BodyType != BodyType.Static;
                if (nonStatic)
                {
                    ++i1;
                    //b2Swap(_contacts[i1], _contacts[i2]);
                    Contact temp = _contacts[i1];
                    _contacts[i1] = _contacts[i2];
                    _contacts[i2] = temp;
                }
            }

            // Initialize velocity constraints.
            _contactSolver.Reset(_contacts, _contactCount, step.dtRatio);
            _contactSolver.WarmStart();

            for (int i = 0; i < _jointCount; ++i)
            {
                _joints[i].InitVelocityConstraints(ref step);
            }

            // Solve velocity constraints.
            for (int i = 0; i < Settings.VelocityIterations; ++i)
            {
                for (int j = 0; j < _jointCount; ++j)
                {
                    _joints[j].SolveVelocityConstraints(ref step);
                }

                _contactSolver.SolveVelocityConstraints();
            }

            // Post-solve (store impulses for warm starting).
            _contactSolver.StoreImpulses();

            // Integrate positions.
            for (int i = 0; i < _bodyCount; ++i)
            {
                Body b = _bodies[i];

                if (b.BodyType == BodyType.Static)
                {
                    continue;
                }

                // Check for large velocities.
                Vector2 translation = step.dt * b._linearVelocity;
                if (Vector2.Dot(translation, translation) > Settings.MaxTranslationSquared)
                {
                    float ratio = Settings.MaxTranslation / translation.Length();
                    b._linearVelocity *= ratio;
                }

                float rotation = step.dt * b._angularVelocity;
                if (rotation * rotation > Settings.MaxRotationSquared)
                {
                    float ratio = Settings.MaxRotation / Math.Abs(rotation);
                    b._angularVelocity *= ratio;
                }

                // Store positions for continuous collision.
                b._sweep.c0 = b._sweep.c;
                b._sweep.a0 = b._sweep.a;

                // Integrate
                b._sweep.c += step.dt * b._linearVelocity;
                b._sweep.a += step.dt * b._angularVelocity;

                // Compute new transform
                b.SynchronizeTransform();

                // Note: shapes are synchronized later.
            }

            // Iterate over constraints.
            for (int i = 0; i < Settings.PositionIterations; ++i)
            {
                bool contactsOkay = _contactSolver.SolvePositionConstraints(Settings.ContactBaumgarte);

                bool jointsOkay = true;
                for (int j = 0; j < _jointCount; ++j)
                {
                    bool jointOkay = _joints[j].SolvePositionConstraints();
                    jointsOkay = jointsOkay && jointOkay;
                }

                if (contactsOkay && jointsOkay)
                {
                    // Exit early if the position errors are small.
                    break;
                }
            }

            Report(_contactSolver._constraints);

            if (Settings.AllowSleep)
            {
                float minSleepTime = Settings.MaxFloat;

                const float linTolSqr = Settings.LinearSleepTolerance * Settings.LinearSleepTolerance;
                const float angTolSqr = Settings.AngularSleepTolerance * Settings.AngularSleepTolerance;

                for (int i = 0; i < _bodyCount; ++i)
                {
                    Body b = _bodies[i];
                    if (b.BodyType == BodyType.Static)
                    {
                        continue;
                    }

                    if ((b._flags & BodyFlags.AutoSleep) == 0)
                    {
                        b._sleepTime = 0.0f;
                        minSleepTime = 0.0f;
                    }

                    if ((b._flags & BodyFlags.AutoSleep) == 0 ||
                        b._angularVelocity * b._angularVelocity > angTolSqr ||
                        Vector2.Dot(b._linearVelocity, b._linearVelocity) > linTolSqr)
                    {
                        b._sleepTime = 0.0f;
                        minSleepTime = 0.0f;
                    }
                    else
                    {
                        b._sleepTime += step.dt;
                        minSleepTime = Math.Min(minSleepTime, b._sleepTime);
                    }
                }

                if (minSleepTime >= Settings.TimeToSleep)
                {
                    for (int i = 0; i < _bodyCount; ++i)
                    {
                        Body b = _bodies[i];
                        b.Awake = false;
                    }
                }
            }
        }

        public void Add(Body body)
        {
            Debug.Assert(_bodyCount < _bodyCapacity);
            _bodies[_bodyCount++] = body;
        }

        public void Add(Contact contact)
        {
            Debug.Assert(_contactCount < _contactCapacity);
            _contacts[_contactCount++] = contact;
        }

        public void Add(Joint joint)
        {
            Debug.Assert(_jointCount < _jointCapacity);
            _joints[_jointCount++] = joint;
        }

        public void Report(ContactConstraint[] constraints)
        {
            if (_listener == null)
            {
                return;
            }

            for (int i = 0; i < _contactCount; ++i)
            {
                Contact c = _contacts[i];

                ContactConstraint cc = constraints[i];

                ContactImpulse impulse = new ContactImpulse();
                for (int j = 0; j < cc.pointCount; ++j)
                {
                    impulse.normalImpulses[j] = cc.points[j].normalImpulse;
                    impulse.tangentImpulses[j] = cc.points[j].tangentImpulse;
                }

                _listener.PostSolve(c, ref impulse);
            }
        }
    }
}