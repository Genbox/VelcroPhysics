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
            JointCapacity = jointCapacity;

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

                if (b.BodyType != BodyType.Dynamic)
                {
                    continue;
                }

                // Integrate velocities. Only apply gravity if the body wants it.
                if (b.IgnoreGravity)
                {
                    b._linearVelocity += step.DeltaTime * (b._invMass * b._force);
                    b._angularVelocity += step.DeltaTime * b._invI * b._torque;
                }
                else
                {
                    b._linearVelocity += step.DeltaTime * (gravity + b._invMass * b._force);
                    b._angularVelocity += step.DeltaTime * b._invI * b._torque;
                }

                // Apply damping.
                // ODE: dv/dt + c * v = 0
                // Solution: v(t) = v0 * exp(-c * t)
                // Time step: v(t + dt) = v0 * exp(-c * (t + dt)) = v0 * exp(-c * t) * exp(-c * dt) = v * exp(-c * dt)
                // v2 = exp(-c * dt) * v1
                // Taylor expansion:
                // v2 = (1.0f - c * dt) * v1
                b._linearVelocity *= MathUtils.Clamp(1.0f - step.DeltaTime * b.LinearDamping, 0.0f, 1.0f);
                b._angularVelocity *= MathUtils.Clamp(1.0f - step.DeltaTime * b._angularDamping, 0.0f, 1.0f);
            }

            // Partition contacts so that contacts with static bodies are solved last.
            int i1 = -1;
            for (int i2 = 0; i2 < ContactCount; ++i2)
            {
                Fixture fixtureA = Contacts[i2].FixtureA;
                Fixture fixtureB = Contacts[i2].FixtureB;
                Body bodyA = fixtureA.Body;
                Body bodyB = fixtureB.Body;
                bool nonStatic = bodyA.BodyType != BodyType.Static && bodyB.BodyType != BodyType.Static;
                if (nonStatic)
                {
                    ++i1;
                    //b2Swap(_contacts[i1], _contacts[i2]);
                    Contact temp = Contacts[i1];
                    Contacts[i1] = Contacts[i2];
                    Contacts[i2] = temp;
                }
            }

            // Initialize velocity constraints.
            _contactSolver.Reset(Contacts, ContactCount, step.DtRatio);
            _contactSolver.WarmStart();

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
                Vector2 translation = step.DeltaTime * b._linearVelocity;
                if (Vector2.Dot(translation, translation) > Settings.MaxTranslationSquared)
                {
                    float ratio = Settings.MaxTranslation / translation.Length();
                    b._linearVelocity *= ratio;
                }

                float rotation = step.DeltaTime * b._angularVelocity;
                if (rotation * rotation > Settings.MaxRotationSquared)
                {
                    float ratio = Settings.MaxRotation / Math.Abs(rotation);
                    b._angularVelocity *= ratio;
                }

                // Store positions for continuous collision.
                b._sweep.Center0 = b._sweep.Center;
                b._sweep.Angle0 = b._sweep.Angle;

                // Integrate
                b._sweep.Center += step.DeltaTime * b._linearVelocity;
                b._sweep.Angle += step.DeltaTime * b._angularVelocity;

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
                    bool jointOkay = Joints[j].SolvePositionConstraints();
                    jointsOkay = jointsOkay && jointOkay;
                }

                if (contactsOkay && jointsOkay)
                {
                    // Exit early if the position errors are small.
                    break;
                }
            }

            //We only report using the postsolve delegate. If no one wants the data, we don't collect it.
            if (_contactManager.PostSolve != null)
                Report(_contactSolver.Constraints);

            if (allowSleep)
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

                    if (b._invMass == 0.0f)
                    {
                        continue;
                    }

                    if ((b._flags & BodyFlags.AllowSleep) == 0)
                    {
                        b._sleepTime = 0.0f;
                        minSleepTime = 0.0f;
                    }

                    if ((b._flags & BodyFlags.AllowSleep) == 0 ||
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
                        b.Awake = false;
                    }
                }
            }
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
            Debug.Assert(JointCount < JointCapacity);
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

                //The Report method does not get run unless someone is subscribed to the PostSolve delegate
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
        public int JointCapacity;
    }
}
