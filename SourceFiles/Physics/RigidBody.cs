using FarseerPhysics.Dynamics;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Physics.Collisions;
using Microsoft.Xna.Framework;
using FarseerPhysics.Common;
using System;
using System.Diagnostics;

namespace FarseerPhysics.Physics
{
    public class RigidBody
    {
        internal struct State
        {
            public Vector2 Force;
            public float Torque;

            public Transform Xf;
            public Sweep Sweep;

            public Vector2 LinearVelocity;
            public float AngularVelocity;

            public bool Awake;

            public void Save(Body body)
            {
                Force = body.Force;
                Torque = body.Torque;
                Xf = body.Xf;
                Sweep = body.Sweep;
                LinearVelocity = body.LinearVelocity;
                AngularVelocity = body.AngularVelocity;
                Awake = body.Awake;
            }

            public void Restore(Body body)
            {
                body.Force = Force;
                body.Torque = Torque;
                body.Xf = Xf;
                body.Sweep = Sweep;
                body.LinearVelocity = LinearVelocity;
                body.AngularVelocity = AngularVelocity;
                body.Awake = Awake;
            }
        }

        public object UserData { get; set; }

        internal State state = new State();
        internal Vector2 bufferForce;
        internal float bufferTorque;

        public RigidBody(Body b)
        {
            Collisions = new List<Collisions.Collision>();
            Body = b;
            UserData = Body.UserData;
            Body.UserData = this;

            for (int i = 0; i < Body.FixtureList.Count; ++i)
            {
                AddFixture(Body.FixtureList[i]);
            }
        }

        public List<Collisions.Collision> Collisions { get; private set; }
        public Body Body { get; private set; }

        public void AddFixture(Fixture fixture)
        {
            switch (fixture.ShapeType)
            {
                case ShapeType.Circle:
                    Collisions.Add(new CircleCollision(this, fixture));
                    break;
                case ShapeType.Polygon:
                    Collisions.Add(new PolygonCollision(this, fixture));
                    break;
            }
        }

        public void RemoveFixture(Fixture fixture)
        {
            for (int i = 0; i < Collisions.Count; ++i)
            {
                if (Collisions[i].Fixture == fixture)
                {
                    Collisions.RemoveAt(i);
                    break;
                }
            }
        }

        internal void SaveState()
        {
            state.Save(Body);
        }

        internal void RestoreState()
        {
            state.Restore(Body);
        }

        internal void AddForce(ref Vector2 force, ref Vector2 point)
        {
            bufferForce += force;
            bufferTorque += (point.X - Body.Sweep.C.X) * force.Y - (point.Y - Body.Sweep.C.Y) * force.X;
        }

        internal void Advance(float dt, ref Vector2 gravity)
        {
            Body b = Body;

            if (b.BodyType != BodyType.Dynamic)
            {
                return;
            }

            // Integrate velocities.
            // FPE 3 only - Only apply gravity if the body wants it.
            if (b.IgnoreGravity)
            {
                b.LinearVelocityInternal.X += dt * (b.InvMass * b.Force.X);
                b.LinearVelocityInternal.Y += dt * (b.InvMass * b.Force.Y);
                b.AngularVelocityInternal += dt * b.InvI * b.Torque;
            }
            else
            {
                b.LinearVelocityInternal.X += dt * (gravity.X + b.InvMass * b.Force.X);
                b.LinearVelocityInternal.Y += dt * (gravity.Y + b.InvMass * b.Force.Y);
                b.AngularVelocityInternal += dt * b.InvI * b.Torque;
            }

            // Apply damping.
            // ODE: dv/dt + c * v = 0
            // Solution: v(t) = v0 * exp(-c * t)
            // Time step: v(t + dt) = v0 * exp(-c * (t + dt)) = v0 * exp(-c * t) * exp(-c * dt) = v * exp(-c * dt)
            // v2 = exp(-c * dt) * v1
            // Taylor expansion:
            // v2 = (1.0f - c * dt) * v1
            b.LinearVelocityInternal *= MathUtils.Clamp(1.0f - dt * b.LinearDamping, 0.0f, 1.0f);
            b.AngularVelocityInternal *= MathUtils.Clamp(1.0f - dt * b.AngularDamping, 0.0f, 1.0f);

            if (b.BodyType == BodyType.Static)
            {
                return;
            }

            // Check for large velocities.
            float translationX = dt * b.LinearVelocityInternal.X;
            float translationY = dt * b.LinearVelocityInternal.Y;
            float result = translationX * translationX + translationY * translationY;

            if (result > Settings.MaxTranslationSquared)
            {
                float sq = (float)Math.Sqrt(result);

                float ratio = Settings.MaxTranslation / sq;
                b.LinearVelocityInternal.X *= ratio;
                b.LinearVelocityInternal.Y *= ratio;
            }

            float rotation = dt * b.AngularVelocityInternal;
            if (rotation * rotation > Settings.MaxRotationSquared)
            {
                float ratio = Settings.MaxRotation / Math.Abs(rotation);
                b.AngularVelocityInternal *= ratio;
            }

            // Store positions for continuous collision.
            b.Sweep.C0.X = b.Sweep.C.X;
            b.Sweep.C0.Y = b.Sweep.C.Y;
            b.Sweep.A0 = b.Sweep.A;

            // Integrate
            b.Sweep.C.X += dt * b.LinearVelocityInternal.X;
            b.Sweep.C.Y += dt * b.LinearVelocityInternal.Y;
            b.Sweep.A += dt * b.AngularVelocityInternal;

            b.SynchronizeTransform();
        }

        public bool Intersect(Collisions.Collision collision, ref Vector2 point, ref Vector2 previousPoint, out Feature result)
        {
            Debug.Assert(collision.RigidBody == this);

            Vector2 localPoint = Body.GetLocalPoint(ref point);
            Vector2 localPreviousPoint = Body.GetLocalPoint(ref previousPoint);

            bool intersects = collision.Intersect(ref localPoint, ref localPreviousPoint, out result);

            if (intersects)
            {
                result.Position = Body.GetWorldPoint(ref result.Position);
                result.Normal = Body.GetWorldVector(ref result.Normal);
            }

            return intersects;
        }

        internal void Reset()
        {
            bufferForce.X = 0.0f;
            bufferForce.Y = 0.0f;
            bufferTorque = 0.0f;

            for (int i = 0; i < Collisions.Count; ++i)
            {
                Collisions[i].Reset();
            }
        }
    }
}