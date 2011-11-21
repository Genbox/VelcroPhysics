/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2011 Ian Qvist
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
    public class Island
    {
        public Body[] Bodies;
        public int BodyCount;
        public int ContactCount;
        public int JointCount;

        public Velocity[] _velocities;
        public Position[] _positions;

        public int BodyCapacity;
        public int ContactCapacity;
        private ContactManager _contactManager;
        private ContactSolver _contactSolver = new ContactSolver();
        private Contact[] _contacts;
        public int JointCapacity;
        private Joint[] _joints;
        public float JointUpdateTime;

        private const float LinTolSqr = Settings.LinearSleepTolerance * Settings.LinearSleepTolerance;
        private const float AngTolSqr = Settings.AngularSleepTolerance * Settings.AngularSleepTolerance;

#if (!SILVERLIGHT)
        private Stopwatch _watch = new Stopwatch();
#endif

        public void Reset(int bodyCapacity, int contactCapacity, int jointCapacity, ContactManager contactManager)
        {
            BodyCapacity = bodyCapacity;
            ContactCapacity = contactCapacity;
            JointCapacity = jointCapacity;
            BodyCount = 0;
            ContactCount = 0;
            JointCount = 0;

            _contactManager = contactManager;

            if (Bodies == null || Bodies.Length < bodyCapacity)
            {
                Bodies = new Body[bodyCapacity];
                _velocities = new Velocity[bodyCapacity];
                _positions = new Position[bodyCapacity];
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

        private float _tmpTime;

        public void Solve(ref TimeStep step, ref Vector2 gravity)
        {
            float h = step.dt;

            // Integrate velocities and apply damping. Initialize the body state.
            for (int i = 0; i < BodyCount; ++i)
            {
                Body b = Bodies[i];

                Vector2 c = b.Sweep.C;
                float a = b.Sweep.A;
                Vector2 v = b.LinearVelocity;
                float w = b.AngularVelocity;

                // Store positions for continuous collision.
                b.Sweep.C0 = b.Sweep.C;
                b.Sweep.A0 = b.Sweep.A;

                if (b.BodyType == BodyType.Dynamic)
                {
                    // Integrate velocities.
                    v += h * (b.GravityScale * gravity + b.InvMass * b.Force);
                    w += h * b.InvI * b.Torque;

                    // Apply damping.
                    // ODE: dv/dt + c * v = 0
                    // Solution: v(t) = v0 * exp(-c * t)
                    // Time step: v(t + dt) = v0 * exp(-c * (t + dt)) = v0 * exp(-c * t) * exp(-c * dt) = v * exp(-c * dt)
                    // v2 = exp(-c * dt) * v1
                    // Taylor expansion:
                    // v2 = (1.0f - c * dt) * v1
                    v *= MathUtils.Clamp(1.0f - h * b.LinearDamping, 0.0f, 1.0f);
                    w *= MathUtils.Clamp(1.0f - h * b.AngularDamping, 0.0f, 1.0f);
                }

                _positions[i].c = c;
                _positions[i].a = a;
                _velocities[i].v = v;
                _velocities[i].w = w;
            }

            // Solver data
            SolverData solverData = new SolverData();
            solverData.step = step;
            solverData.positions = _positions;
            solverData.velocities = _velocities;

            // Initialize velocity constraints.
            //b2ContactSolverDef contactSolverDef;
            //contactSolverDef.step = step;
            //contactSolverDef.contacts = m_contacts;
            //contactSolverDef.count = m_contactCount;
            //contactSolverDef.positions = m_positions;
            //contactSolverDef.velocities = m_velocities;
            //contactSolverDef.allocator = m_allocator;

            _contactSolver.Reset(step, ContactCount, _contacts, _positions, _velocities);
            _contactSolver.InitializeVelocityConstraints();

            if (Settings.EnableWarmstarting)
            {
                _contactSolver.WarmStart();
            }

#if (!SILVERLIGHT)
            if (Settings.EnableDiagnostics)
            {
                _watch.Start();
                _tmpTime = 0;
            }
#endif

            for (int i = 0; i < JointCount; ++i)
            {
                if (_joints[i].Enabled)
                    _joints[i].InitVelocityConstraints(ref solverData);
            }

#if (!SILVERLIGHT)
            if (Settings.EnableDiagnostics)
            {
                _tmpTime += _watch.ElapsedTicks;
            }
#endif

            // Solve velocity constraints.
            for (int i = 0; i < Settings.VelocityIterations; ++i)
            {
#if (!SILVERLIGHT)
                if (Settings.EnableDiagnostics)
                    _watch.Start();
#endif
                for (int j = 0; j < JointCount; ++j)
                {
                    Joint joint = _joints[j];

                    if (!joint.Enabled)
                        continue;

                    joint.SolveVelocityConstraints(ref solverData);
                    
                    //TODO: Move up before solve?
                    joint.Validate(step.inv_dt);
                }

#if (!SILVERLIGHT)
                if (Settings.EnableDiagnostics)
                {
                    _watch.Stop();
                    _tmpTime += _watch.ElapsedTicks;
                    _watch.Reset();
                }
#endif

                _contactSolver.SolveVelocityConstraints();
            }

            // Store impulses for warm starting.
            _contactSolver.StoreImpulses();

            // Integrate positions
            for (int i = 0; i < BodyCount; ++i)
            {
                Vector2 c = _positions[i].c;
                float a = _positions[i].a;
                Vector2 v = _velocities[i].v;
                float w = _velocities[i].w;

                // Check for large velocities
                Vector2 translation = h * v;
                if (Vector2.Dot(translation, translation) > Settings.MaxTranslationSquared)
                {
                    float ratio = Settings.MaxTranslation / translation.Length();
                    v *= ratio;
                }

                float rotation = h * w;
                if (rotation * rotation > Settings.MaxRotationSquared)
                {
                    float ratio = Settings.MaxRotation / Math.Abs(rotation);
                    w *= ratio;
                }

                // Integrate
                c += h * v;
                a += h * w;

                _positions[i].c = c;
                _positions[i].a = a;
                _velocities[i].v = v;
                _velocities[i].w = w;
            }

            // Solve position constraints
            bool positionSolved = false;
            for (int i = 0; i < Settings.PositionIterations; ++i)
            {
                bool contactsOkay = _contactSolver.SolvePositionConstraints();

                bool jointsOkay = true;

#if (!SILVERLIGHT)
                if (Settings.EnableDiagnostics)
                    _watch.Start();
#endif
                for (int j = 0; j < JointCount; ++j)
                {
                    Joint joint = _joints[j];
                    if (!joint.Enabled)
                        continue;

                    bool jointOkay = joint.SolvePositionConstraints(ref solverData);
                    jointsOkay = jointsOkay && jointOkay;
                }

#if (!SILVERLIGHT)
                if (Settings.EnableDiagnostics)
                {
                    _watch.Stop();
                    _tmpTime += _watch.ElapsedTicks;
                    _watch.Reset();
                }
#endif
                if (contactsOkay && jointsOkay)
                {
                    // Exit early if the position errors are small.
                    positionSolved = true;
                    break;
                }
            }

#if (!SILVERLIGHT)
            if (Settings.EnableDiagnostics)
            {
                JointUpdateTime = _tmpTime;
            }
#endif

            // Copy state buffers back to the bodies
            for (int i = 0; i < BodyCount; ++i)
            {
                Body body = Bodies[i];
                body.Sweep.C = _positions[i].c;
                body.Sweep.A = _positions[i].a;
                body.LinearVelocity = _velocities[i].v;
                body.AngularVelocity = _velocities[i].w;
                body.SynchronizeTransform();
            }

            Report(_contactSolver._velocityConstraints);

            if (Settings.AllowSleep)
            {
                float minSleepTime = Settings.MaxFloat;

                for (int i = 0; i < BodyCount; ++i)
                {
                    Body b = Bodies[i];
                    if (b.BodyType == BodyType.Static)
                    {
                        continue;
                    }

                    if ((b.Flags & BodyFlags.AutoSleep) == 0 ||
                        b.AngularVelocityInternal * b.AngularVelocityInternal > AngTolSqr ||
                        Vector2.Dot(b.LinearVelocityInternal, b.LinearVelocityInternal) > LinTolSqr)
                    {
                        b.SleepTime = 0.0f;
                        minSleepTime = 0.0f;
                    }
                    else
                    {
                        b.SleepTime += h;
                        minSleepTime = Math.Min(minSleepTime, b.SleepTime);
                    }
                }

                if (minSleepTime >= Settings.TimeToSleep && positionSolved)
                {
                    for (int i = 0; i < BodyCount; ++i)
                    {
                        Body b = Bodies[i];
                        b.Awake = false;
                    }
                }
            }
        }

        internal void SolveTOI(ref TimeStep subStep, int toiIndexA, int toiIndexB)
        {
            Debug.Assert(toiIndexA < BodyCount);
            Debug.Assert(toiIndexB < BodyCount);

            // Initialize the body state.
            for (int i = 0; i < BodyCount; ++i)
            {
                Body b = Bodies[i];
                _positions[i].c = b.Sweep.C;
                _positions[i].a = b.Sweep.A;
                _velocities[i].v = b.LinearVelocity;
                _velocities[i].w = b.AngularVelocity;
            }

            //b2ContactSolverDef contactSolverDef;
            //contactSolverDef.contacts = _contacts;
            //contactSolverDef.count = _contactCount;
            //contactSolverDef.allocator = _allocator;
            //contactSolverDef.step = subStep;
            //contactSolverDef.positions = _positions;
            //contactSolverDef.velocities = _velocities;
            //b2ContactSolver contactSolver(&contactSolverDef);

            _contactSolver.Reset(subStep, ContactCount, _contacts, _positions, _velocities);

            // Solve position constraints.
            for (int i = 0; i < Settings.TOIPositionIterations; ++i)
            {
                bool contactsOkay = _contactSolver.SolveTOIPositionConstraints(toiIndexA, toiIndexB);
                if (contactsOkay)
                {
                    break;
                }
            }

            // Leap of faith to new safe state.
            Bodies[toiIndexA].Sweep.C0 = _positions[toiIndexA].c;
            Bodies[toiIndexA].Sweep.A0 = _positions[toiIndexA].a;
            Bodies[toiIndexB].Sweep.C0 = _positions[toiIndexB].c;
            Bodies[toiIndexB].Sweep.A0 = _positions[toiIndexB].a;

            // No warm starting is needed for TOI events because warm
            // starting impulses were applied in the discrete solver.
            _contactSolver.InitializeVelocityConstraints();

            // Solve velocity constraints.
            for (int i = 0; i < Settings.TOIVelocityIterations; ++i)
            {
                _contactSolver.SolveVelocityConstraints();
            }

            // Don't store the TOI contact forces for warm starting
            // because they can be quite large.

            float h = subStep.dt;

            // Integrate positions.
            for (int i = 0; i < BodyCount; ++i)
            {
                Vector2 c = _positions[i].c;
                float a = _positions[i].a;
                Vector2 v = _velocities[i].v;
                float w = _velocities[i].w;

                // Check for large velocities
                Vector2 translation = h * v;
                if (Vector2.Dot(translation, translation) > Settings.MaxTranslationSquared)
                {
                    float ratio = Settings.MaxTranslation / translation.Length();
                    v *= ratio;
                }

                float rotation = h * w;
                if (rotation * rotation > Settings.MaxRotationSquared)
                {
                    float ratio = Settings.MaxRotation / Math.Abs(rotation);
                    w *= ratio;
                }

                // Integrate
                c += h * v;
                a += h * w;

                _positions[i].c = c;
                _positions[i].a = a;
                _velocities[i].v = v;
                _velocities[i].w = w;

                // Sync bodies
                Body body = Bodies[i];
                body.Sweep.C = c;
                body.Sweep.A = a;
                body.LinearVelocity = v;
                body.AngularVelocity = w;
                body.SynchronizeTransform();
            }

            Report(_contactSolver._velocityConstraints);
        }

        public void Add(Body body)
        {
            Debug.Assert(BodyCount < BodyCapacity);
            body.IslandIndex = BodyCount;
            Bodies[BodyCount] = body;
            ++BodyCount;
        }

        public void Add(Contact contact)
        {
            Debug.Assert(ContactCount < ContactCapacity);
            _contacts[ContactCount++] = contact;
        }

        public void Add(Joint joint)
        {
            Debug.Assert(JointCount < JointCapacity);
            _joints[JointCount++] = joint;
        }

        private void Report(ContactVelocityConstraint[] constraints)
        {
            if (_contactManager == null)
                return;

            for (int i = 0; i < ContactCount; ++i)
            {
                Contact c = _contacts[i];

                if (c.FixtureA.AfterCollision != null)
                    c.FixtureA.AfterCollision(c.FixtureA, c.FixtureB, c);

                if (c.FixtureB.AfterCollision != null)
                    c.FixtureB.AfterCollision(c.FixtureB, c.FixtureA, c);

                if (_contactManager.PostSolve != null)
                {
                    ContactVelocityConstraint cc = constraints[i];

                    _contactManager.PostSolve(c, cc);
                }
            }
        }
    }
}