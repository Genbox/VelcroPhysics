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

using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace FarseerPhysics
{
    /// <summary>
    /// This is an internal class.
    /// </summary>
    internal class Island
    {
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

            if (Contacts == null || Contacts.Length < contactCapacity)
            {
                Contacts = new Contact[contactCapacity * 2];
            }

            if (Joints == null || Joints.Length < jointCapacity)
            {
                Joints = new Joint[jointCapacity * 2];
            }
        }

        public void Clear()
        {
            BodyCount = 0;
            ContactCount = 0;
            JointCount = 0;
        }

        public void Solve(ref TimeStep step, Vector2 gravity, bool allowSleep)
        {
            // Integrate velocities and apply damping.
            for (int i = 0; i < BodyCount; ++i)
            {
                Body b = Bodies[i];

                if (b.GetBodyType() != BodyType.Dynamic)
                {
                    continue;
                }

                // Integrate velocities.
                b._linearVelocity += step.DeltaTime * (gravity + b._invMass * b._force);
                b._angularVelocity += step.DeltaTime * b._invI * b._torque;

                // Apply damping.
                // ODE: dv/dt + c * v = 0
                // Solution: v(t) = v0 * exp(-c * t)
                // Time step: v(t + dt) = v0 * exp(-c * (t + dt)) = v0 * exp(-c * t) * exp(-c * dt) = v * exp(-c * dt)
                // v2 = exp(-c * dt) * v1
                // Taylor expansion:
                // v2 = (1.0f - c * dt) * v1
                b._linearVelocity *= MathUtils.Clamp(1.0f - step.DeltaTime * b._linearDamping, 0.0f, 1.0f);
                b._angularVelocity *= MathUtils.Clamp(1.0f - step.DeltaTime * b._angularDamping, 0.0f, 1.0f);
            }

            _contactSolver.Reset(Contacts, ContactCount);

            // Initialize velocity constraints.
            _contactSolver.InitVelocityConstraints(ref step);

            for (int i = 0; i < JointCount; ++i)
            {
                Joints[i].InitVelocityConstraints(ref step);
            }

            // Solve velocity constraints.
            for (int i = 0; i < step.VelocityIterations; ++i)
            {
                for (int j = 0; j < JointCount; ++j)
                {
                    Joints[j].SolveVelocityConstraints(ref step);
                }

                _contactSolver.SolveVelocityConstraints();
            }

            // Post-solve (store impulses for warm starting).
            for (int j = 0; j < JointCount; ++j)
            {
                Joints[j].FinalizeVelocityConstraints();
            }

            _contactSolver.FinalizeVelocityConstraints();

            // Integrate positions.
            for (int i = 0; i < BodyCount; ++i)
            {
                Body b = Bodies[i];

                if (b.GetBodyType() == BodyType.Static)
                {
                    continue;
                }

                // Check for large velocities.
                Vector2 translation = step.DeltaTime * b._linearVelocity;
                if (Vector2.Dot(translation, translation) > Settings.MaxTranslationSquared)
                {
                    translation.Normalize();
                    b._linearVelocity = (Settings.MaxTranslation * step.Inv_DeltaTime) * translation;
                }

                float rotation = step.DeltaTime * b._angularVelocity;
                if (rotation * rotation > Settings.MaxRotationSquared)
                {
                    if (rotation < 0.0)
                    {
                        b._angularVelocity = -step.Inv_DeltaTime * Settings.MaxRotation;
                    }
                    else
                    {
                        b._angularVelocity = step.Inv_DeltaTime * Settings.MaxRotation;
                    }
                }

                // Store positions for continuous collision.
                b._sweep.c0 = b._sweep.c;
                b._sweep.a0 = b._sweep.a;

                // Integrate
                b._sweep.c += step.DeltaTime * b._linearVelocity;
                b._sweep.a += step.DeltaTime * b._angularVelocity;

                // Compute new transform
                b.SynchronizeTransform();

                // Note: shapes are synchronized later.
            }

            // Iterate over constraints.
            for (int i = 0; i < step.PositionIterations; ++i)
            {
                bool contactsOkay = _contactSolver.SolvePositionConstraints(Settings.ContactBaumgarte);

                bool jointsOkay = true;
                for (int j = 0; j < JointCount; ++j)
                {
                    bool jointOkay = Joints[j].SolvePositionConstraints(Settings.ContactBaumgarte);
                    jointsOkay = jointsOkay && jointOkay;
                }

                if (contactsOkay && jointsOkay)
                {
                    // Exit early if the position errors are small.
                    break;
                }
            }

            Report(_contactSolver.Constraints);

            if (allowSleep)
            {
                float minSleepTime = Settings.MaxFloat;

                const float linTolSqr = Settings.LinearSleepTolerance * Settings.LinearSleepTolerance;
                const float angTolSqr = Settings.AngularSleepTolerance * Settings.AngularSleepTolerance;

                for (int i = 0; i < BodyCount; ++i)
                {
                    Body b = Bodies[i];
                    if (b.GetBodyType() == BodyType.Static)
                    {
                        continue;
                    }

                    if (b._invMass == 0.0f)
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
                        b._sleepTime += step.DeltaTime;
                        minSleepTime = Math.Min(minSleepTime, b._sleepTime);
                    }
                }

                if (minSleepTime >= Settings.TimeToSleep)
                {
                    for (int i = 0; i < BodyCount; ++i)
                    {
                        Body b = Bodies[i];
                        b.SetAwake(false);
                    }
                }
            }
        }

        public void SolveTOI(ref TimeStep subStep)
        {
            _contactSolver.Reset(Contacts, ContactCount);

            // No warm starting is needed for TOI events because warm
            // starting impulses were applied in the discrete solver.

            // Warm starting for joints is off for now, but we need to
            // call this function to compute Jacobians.
            for (int i = 0; i < JointCount; ++i)
            {
                Joints[i].InitVelocityConstraints(ref subStep);
            }

            // Solve velocity constraints.
            for (int i = 0; i < subStep.VelocityIterations; ++i)
            {
                _contactSolver.SolveVelocityConstraints();
                for (int j = 0; j < JointCount; ++j)
                {
                    Joints[j].SolveVelocityConstraints(ref subStep);
                }
            }

            // Don't store the TOI contact forces for warm starting
            // because they can be quite large.

            // Integrate positions.
            for (int i = 0; i < BodyCount; ++i)
            {
                Body b = Bodies[i];

                if (b.GetBodyType() == BodyType.Static)
                {
                    continue;
                }

                // Check for large velocities.
                Vector2 translation = subStep.DeltaTime * b._linearVelocity;
                if (Vector2.Dot(translation, translation) > Settings.MaxTranslationSquared)
                {
                    translation.Normalize();
                    b._linearVelocity = (Settings.MaxTranslation * subStep.Inv_DeltaTime) * translation;
                }

                float rotation = subStep.DeltaTime * b._angularVelocity;
                if (rotation * rotation > Settings.MaxRotationSquared)
                {
                    if (rotation < 0.0)
                    {
                        b._angularVelocity = -subStep.Inv_DeltaTime * Settings.MaxRotation;
                    }
                    else
                    {
                        b._angularVelocity = subStep.Inv_DeltaTime * Settings.MaxRotation;
                    }
                }

                // Store positions for continuous collision.
                b._sweep.c0 = b._sweep.c;
                b._sweep.a0 = b._sweep.a;

                // Integrate
                b._sweep.c += subStep.DeltaTime * b._linearVelocity;
                b._sweep.a += subStep.DeltaTime * b._angularVelocity;

                // Compute new transform
                b.SynchronizeTransform();

                // Note: shapes are synchronized later.
            }


            // Solve position constraints.
            const float k_toiBaumgarte = 0.75f;
            for (int i = 0; i < subStep.PositionIterations; ++i)
            {
                bool contactsOkay = _contactSolver.SolvePositionConstraints(k_toiBaumgarte);
                bool jointsOkay = true;
                for (int j = 0; j < JointCount; ++j)
                {
                    bool jointOkay = Joints[j].SolvePositionConstraints(k_toiBaumgarte);
                    jointsOkay = jointsOkay && jointOkay;
                }

                if (contactsOkay && jointsOkay)
                {
                    break;
                }
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
            Contacts[ContactCount++] = contact;
        }

        public void Add(Joint joint)
        {
            Debug.Assert(JointCount < _jointCapacity);
            Joints[JointCount++] = joint;
        }

        private void Report(ContactConstraint[] constraints)
        {
            for (int i = 0; i < ContactCount; ++i)
            {
                Contact c = Contacts[i];

                ContactConstraint cc = constraints[i];

                ContactImpulse impulse = new ContactImpulse();
                for (int j = 0; j < cc.PointCount; ++j)
                {
                    impulse.normalImpulses[j] = cc.Points[j].NormalImpulse;
                    impulse.tangentImpulses[j] = cc.Points[j].TangentImpulse;
                }

                if (_contactManager.PostSolve != null)
                    _contactManager.PostSolve(c, ref impulse);
            }
        }

        private ContactSolver _contactSolver = new ContactSolver();
        private ContactManager _contactManager;

        public Body[] Bodies;
        public Contact[] Contacts;
        public Joint[] Joints;

        public int BodyCount;
        public int ContactCount;
        public int JointCount;

        private int _bodyCapacity;
        public int _contactCapacity;
        public int _jointCapacity;
    }
}
