using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Physics.Collisions;
using Microsoft.Xna.Framework;
using FarseerPhysics.Common;
using System;
using System.Diagnostics;

namespace FarseerPhysics.Physics
{
    public class RigidBody
    {
        private State _state = new State();
        internal Vector2 bufferForce;
        internal float bufferTorque;

        public RigidBody(Body b)
        {
            Body = b;
        }

        public Body Body { get; private set; }

        internal void SaveState()
        {
            _state.Save(Body);
        }

        internal void RestoreState()
        {
            _state.Restore(Body);
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

        CircleShape circle = new CircleShape(0.01f, 1);

        public bool Intersect(Fixture fixture, ref Vector2 point, out Feature result)
        {
            result = new Feature();

            Manifold manifold = new Manifold();
            Vector2 normal;
            FixedArray2<Vector2> points;

            Transform transformB = new Transform();
            transformB.Set(Vector2.Zero, 0);
            circle.Position = point;

            PolygonShape polygonShape = fixture.Shape as PolygonShape;

            switch (fixture.ShapeType)
            {
                case ShapeType.Polygon:
                    Collision.Collision.CollidePolygonAndCircle(ref manifold, polygonShape, ref fixture.Body.Xf, circle, ref transformB);
                    ContactSolver.WorldManifold.Initialize(ref manifold, ref fixture.Body.Xf, polygonShape.Radius, ref transformB, circle.Radius, out normal, out points);

                    result.Distance = (point-(fixture.Body.Position + new Vector2(1.5f, 1.5f))).Length();

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //result.Position = points[0];
            //result.Normal = normal;
            //result.Distance = 1f;


            bool intersects = manifold.PointCount >= 1;//collision.Intersect(ref localPoint,  out result);

            if (intersects)
            {
                result.Position = points[0];
                result.Normal = normal;
                result.Distance *= -1;

                //Vector2 collisionPoint = points[0];
                //result.Position = Body.GetWorldPoint(ref collisionPoint);
                //result.Normal = Body.GetWorldVector(ref normal);
            }

            return intersects;
        }

        internal void Reset()
        {
            bufferForce = Vector2.Zero;
            bufferTorque = 0.0f;
        }
    }
}