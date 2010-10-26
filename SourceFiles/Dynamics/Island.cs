/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
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
    /// <summary>
    /// This is an internal class.
    /// </summary>
    internal class Island
    {
        public Body[] Bodies;
        public int BodyCount;
        public int ContactCount;
        public int JointCount;
        private int _bodyCapacity;
        private int _contactCapacity;
        private ContactManager _contactManager;
        private ContactSolver _contactSolver = new ContactSolver();
        private Contact[] _contacts;
        private int _jointCapacity;
        private Joint[] _joints;
        private int _positionIterationCount;

        public void Reset(int bodyCapacity, int contactCapacity, int jointCapacity, ContactManager contactManager)
        {
            _bodyCapacity = bodyCapacity;
            _contactCapacity = contactCapacity;
            _jointCapacity = jointCapacity;

            BodyCount = 0;
            ContactCount = 0;
            JointCount = 0;

            _contactManager = contactManager;

            if (Bodies == null || Bodies.Length < bodyCapacity)
            {
                Bodies = new Body[bodyCapacity];
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
            BodyCount = 0;
            ContactCount = 0;
            JointCount = 0;
        }

        public void Solve(ref TimeStep step, Vector2 gravity)
        {
            // Integrate velocities and apply damping.
            for (int i = 0; i < BodyCount; ++i)
            {
                Body b = Bodies[i];

                if (b.BodyType != BodyType.Dynamic)
                {
                    continue;
                }

                // Integrate velocities. Only apply gravity if the body wants it.
                if (b.IgnoreGravity)
                {
                    b.LinearVelocityInternal += step.dt * (b.InvMass * b.Force);
                    b.AngularVelocityInternal += step.dt * b.InvI * b.Torque;
                }
                else
                {
                    b.LinearVelocityInternal += step.dt * (gravity + b.InvMass * b.Force);
                    b.AngularVelocityInternal += step.dt * b.InvI * b.Torque;
                }

                // Apply damping.
                // ODE: dv/dt + c * v = 0
                // Solution: v(t) = v0 * exp(-c * t)
                // Time step: v(t + dt) = v0 * exp(-c * (t + dt)) = v0 * exp(-c * t) * exp(-c * dt) = v * exp(-c * dt)
                // v2 = exp(-c * dt) * v1
                // Taylor expansion:
                // v2 = (1.0f - c * dt) * v1
                b.LinearVelocityInternal *= MathUtils.Clamp(1.0f - step.dt * b.LinearDamping, 0.0f, 1.0f);
                b.AngularVelocityInternal *= MathUtils.Clamp(1.0f - step.dt * b.AngularDamping, 0.0f, 1.0f);
            }

            // Partition contacts so that contacts with static bodies are solved last.
            int i1 = -1;
            for (int i2 = 0; i2 < ContactCount; ++i2)
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
            _contactSolver.Reset(_contacts, ContactCount, step.dtRatio);
            _contactSolver.InitializeVelocityConstraints();
            _contactSolver.WarmStart();

            for (int i = 0; i < JointCount; ++i)
            {
                _joints[i].InitVelocityConstraints(ref step);
            }

            // Solve velocity constraints.
            for (int i = 0; i < Settings.VelocityIterations; ++i)
            {
                for (int j = 0; j < JointCount; ++j)
                {
                    _joints[j].SolveVelocityConstraints(ref step);
                }

                _contactSolver.SolveVelocityConstraints();
            }

            // Post-solve (store impulses for warm starting).
            _contactSolver.StoreImpulses();

            // Integrate positions.
            for (int i = 0; i < BodyCount; ++i)
            {
                Body b = Bodies[i];

                if (b.BodyType == BodyType.Static)
                {
                    continue;
                }

                // Check for large velocities.
                Vector2 translation = step.dt * b.LinearVelocityInternal;
                if (Vector2.Dot(translation, translation) > Settings.MaxTranslationSquared)
                {
                    float ratio = Settings.MaxTranslation / translation.Length();
                    b.LinearVelocityInternal *= ratio;
                }

                float rotation = step.dt * b.AngularVelocityInternal;
                if (rotation * rotation > Settings.MaxRotationSquared)
                {
                    float ratio = Settings.MaxRotation / Math.Abs(rotation);
                    b.AngularVelocityInternal *= ratio;
                }

                // Store positions for continuous collision.
                b.Sweep.c0 = b.Sweep.c;
                b.Sweep.a0 = b.Sweep.a;

                // Integrate
                b.Sweep.c += step.dt * b.LinearVelocityInternal;
                b.Sweep.a += step.dt * b.AngularVelocityInternal;

                // Compute new transform
                b.SynchronizeTransform();

                // Note: shapes are synchronized later.
            }

            // Iterate over constraints.
            for (int i = 0; i < Settings.PositionIterations; ++i)
            {
                bool contactsOkay = _contactSolver.SolvePositionConstraints(Settings.ContactBaumgarte);

                bool jointsOkay = true;
                for (int j = 0; j < JointCount; ++j)
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

            if (_contactManager.PostSolve != null)
                Report(_contactSolver.Constraints);

            if (Settings.AllowSleep)
            {
                float minSleepTime = Settings.MaxFloat;

                const float linTolSqr = Settings.LinearSleepTolerance * Settings.LinearSleepTolerance;
                const float angTolSqr = Settings.AngularSleepTolerance * Settings.AngularSleepTolerance;

                for (int i = 0; i < BodyCount; ++i)
                {
                    Body b = Bodies[i];
                    if (b.BodyType == BodyType.Static)
                    {
                        continue;
                    }

                    if ((b.Flags & BodyFlags.AutoSleep) == 0)
                    {
                        b.SleepTime = 0.0f;
                        minSleepTime = 0.0f;
                    }

                    if ((b.Flags & BodyFlags.AutoSleep) == 0 ||
                        b.AngularVelocityInternal * b.AngularVelocityInternal > angTolSqr ||
                        Vector2.Dot(b.LinearVelocityInternal, b.LinearVelocityInternal) > linTolSqr)
                    {
                        b.SleepTime = 0.0f;
                        minSleepTime = 0.0f;
                    }
                    else
                    {
                        b.SleepTime += step.dt;
                        minSleepTime = Math.Min(minSleepTime, b.SleepTime);
                    }
                }

                if (minSleepTime >= Settings.TimeToSleep)
                {
                    for (int i = 0; i < BodyCount; ++i)
                    {
                        Body b = Bodies[i];
                        b.Awake = false;
                    }
                }
            }
        }

        internal void SolveTOI(TimeStep subStep, Body bodyA, Body bodyB)
        {
            _contactSolver.Reset(_contacts, ContactCount, subStep.dtRatio);

            // Solve position constraints.
            const float k_toiBaumgarte = 0.75f;

            for (int i = 0; i < Settings.PositionIterations; ++i)
            {
                bool contactsOkay = _contactSolver.SolvePositionConstraintsTOI(k_toiBaumgarte, bodyA, bodyB);
                if (contactsOkay)
                {
                    break;
                }

                if (i == 5 - 1)
                {
                    i += 0;
                }
            }
            /*
            #if 0
                // Is the new position really safe?
                for (int32 i = 0; i < m_contactCount; ++i)
                {
                    b2Contact* c = m_contacts[i];
                    b2Fixture* fA = c->GetFixtureA();
                    b2Fixture* fB = c->GetFixtureB();

                    b2Body* bA = fA->GetBody();
                    b2Body* bB = fB->GetBody();

                    int32 indexA = c->GetChildIndexA();
                    int32 indexB = c->GetChildIndexB();

                    b2DistanceInput input;
                    input.proxyA.Set(fA->GetShape(), indexA);
                    input.proxyB.Set(fB->GetShape(), indexB);
                    input.transformA = bA->GetTransform();
                    input.transformB = bB->GetTransform();
                    input.useRadii = false;

                    b2DistanceOutput output;
                    b2SimplexCache cache;
                    cache.count = 0;
                    b2Distance(&output, &cache, &input);

                    if (output.distance == 0 || cache.count == 3)
                    {
                        cache.count += 0;
                    }
                }
            #endif
             */

            // Leap of faith to new safe state.
            for (int i = 0; i < BodyCount; ++i)
            {
                Bodies[i].Sweep.a0 = Bodies[i].Sweep.a;
                Bodies[i].Sweep.c0 = Bodies[i].Sweep.c;
            }

            // No warm starting is needed for TOI events because warm
            // starting impulses were applied in the discrete solver.
            _contactSolver.InitializeVelocityConstraints();

            // Solve velocity constraints.
            for (int i = 0; i < Settings.VelocityIterations; ++i)
            {
                _contactSolver.SolveVelocityConstraints();
            }

            // Don't store the TOI contact forces for warm starting
            // because they can be quite large.

            // Integrate positions.
            for (int i = 0; i < BodyCount; ++i)
            {
                Body b = Bodies[i];

                if (b.BodyType == BodyType.Static)
                {
                    continue;
                }

                // Check for large velocities.
                Vector2 translation = subStep.dt * b.LinearVelocity;
                if (Vector2.Dot(translation, translation) > Settings.MaxTranslationSquared)
                {
                    translation.Normalize();
                    b.LinearVelocity = (Settings.MaxTranslation * subStep.inv_dt) * translation;
                }

                float rotation = subStep.dt * b.AngularVelocity;
                if (rotation * rotation > Settings.MaxRotationSquared)
                {
                    if (rotation < 0.0)
                    {
                        b.AngularVelocity = -subStep.inv_dt * Settings.MaxRotation;
                    }
                    else
                    {
                        b.AngularVelocity = subStep.inv_dt * Settings.MaxRotation;
                    }
                }

                // Integrate
                b.Sweep.c += subStep.dt * b.LinearVelocity;
                b.Sweep.a += subStep.dt * b.AngularVelocity;

                // Compute new transform
                b.SynchronizeTransform();

                // Note: shapes are synchronized later.
            }

            Report(_contactSolver.Constraints);
        }

        public void Add(Body body)
        {
            Debug.Assert(BodyCount < _bodyCapacity);
            Bodies[BodyCount++] = body;
        }

        public void Add(Contact contact)
        {
            Debug.Assert(ContactCount < _contactCapacity);
            _contacts[ContactCount++] = contact;
        }

        public void Add(Joint joint)
        {
            Debug.Assert(JointCount < _jointCapacity);
            _joints[JointCount++] = joint;
        }

        private void Report(ContactConstraint[] constraints)
        {
            if (_contactManager == null)
            {
                return;
            }

            for (int i = 0; i < ContactCount; ++i)
            {
                Contact c = _contacts[i];

                ContactConstraint cc = constraints[i];

                ContactImpulse impulse = new ContactImpulse();
                for (int j = 0; j < cc.PointCount; ++j)
                {
                    impulse.NormalImpulses[j] = cc.Points[j].NormalImpulse;
                    impulse.TangentImpulses[j] = cc.Points[j].TangentImpulse;
                }

                _contactManager.PostSolve(c, ref impulse);
            }
        }
    }
}
