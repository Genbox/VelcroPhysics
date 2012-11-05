using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Physics;
using FarseerPhysics.Physics.Collisions;
using Microsoft.Xna.Framework;
using System;

namespace FarseerPhysics.Fluids
{
    public class FluidManager
    {
        private World _world;
        private List<RigidBody> _bodies = new List<RigidBody>();
        private List<Fixture> _fluidCollisions = new List<Fixture>();

        public FluidManager(World w)
        {
            Fluid = new FluidSystem();
            _world = w;

            _world.BodyAdded += OnBodyAdded;
            _world.BodyRemoved += OnBodyRemoved;

            for (int i = 0; i < _world.BodyList.Count; ++i)
            {
                OnBodyAdded(_world.BodyList[i]);
            }
        }

        public FluidSystem Fluid { get; private set; }

        private void OnBodyAdded(Body body)
        {
            _bodies.Add(new RigidBody(body));
        }

        private void OnBodyRemoved(Body body)
        {
            for (int i = 0; i < _bodies.Count; ++i)
            {
                if (_bodies[i].Body == body)
                {
                    _bodies.RemoveAt(i);
                    break;
                }
            }
        }

        private bool FluidQueryCallback(Fixture fixture)
        {
            _fluidCollisions.Add(fixture);

            return true;
        }

        public void Clear()
        {
            _world.Clear();
            Fluid.Clear();
        }

        public void Step(float dt)
        {
            Fluid.Gravity = _world.Gravity;
            Fluid.Update(dt);

            Collision.AABB fluidAABB = new Collision.AABB();

            // Compute fluid AABB
            for (int i = 0; i < Fluid.Particles.Count; ++i)
            {
                FluidParticle p = Fluid.Particles[i];
                if (p.IsActive)
                {
                    fluidAABB.LowerBound = Vector2.Min(p.Position, fluidAABB.LowerBound);
                    fluidAABB.UpperBound = Vector2.Max(p.Position, fluidAABB.UpperBound);
                }
            }

            fluidAABB.LowerBound -= new Vector2(Fluid.Definition.InfluenceRadius, Fluid.Definition.InfluenceRadius);
            fluidAABB.UpperBound += new Vector2(Fluid.Definition.InfluenceRadius, Fluid.Definition.InfluenceRadius);

            // Query the world for fixtures near fluid
            _fluidCollisions.Clear();
            _world.QueryAABB(FluidQueryCallback, ref fluidAABB);

            // Save states
            for (int i = 0; i < _bodies.Count; ++i)
            {
                RigidBody rb = _bodies[i];
                if (rb.Body.BodyType == BodyType.Dynamic)
                {
                    rb.Reset();
                    rb.SaveState();
                    rb.Advance(dt, ref _world.Gravity);
                }
            }

            // Fluid > Bodies
            for (int ic = 0; ic < _fluidCollisions.Count; ++ic)
            {
                Fixture fixture = _fluidCollisions[ic];

                if (fixture.RigidBody.Body.BodyType != BodyType.Dynamic)
                    continue;

                if (fixture.IsSensor)
                    continue;

                for (int ip = 0; ip < Fluid.Particles.Count; ++ip)
                {
                    FluidParticle particle = Fluid.Particles[ip];

                    if (!particle.IsActive)
                        continue;

                    Feature result;

                    if (!fixture.RigidBody.Intersect(fixture, ref particle.Position, out result))
                        continue;

                    Vector2 impulse = new Vector2();

                    if (result.Distance < 0.0f) // Inside
                    {
                        Vector2 bodyVelocity = fixture.RigidBody.Body.GetLinearVelocityFromWorldPoint(ref particle.Position);

                        Vector2 velocity = (particle.Position - particle.PreviousPosition) - bodyVelocity * dt;

                        Vector2 velocityNormal = Vector2.Dot(velocity, result.Normal) * result.Normal;
                        Vector2 velocityTangent = velocity - velocityNormal;

                        impulse = velocityNormal + fixture.FluidProperties.Slip * velocityTangent;
                    }
                    else if (fixture.FluidProperties.IsSticky && result.Distance < fixture.FluidProperties.StickDistance) // Outside
                    {
                        impulse = -dt * fixture.FluidProperties.StickForce * result.Distance * (1.0f - result.Distance / fixture.FluidProperties.StickDistance) * result.Normal;
                    }

                    fixture.RigidBody.AddForce(ref impulse, ref particle.Position);
                }
            }

            // Apply buffers to bodies
            for (int i = 0; i < _bodies.Count; ++i)
            {
                RigidBody rb = _bodies[i];
                if (rb.Body.BodyType == BodyType.Dynamic)
                {
                    rb.RestoreState();

                    rb.bufferForce /= dt;
                    rb.bufferTorque /= dt;

                    if (Math.Abs(rb.bufferForce.X) > 0.0f && Math.Abs(rb.bufferForce.Y) > 0.0f)
                    {
                        rb.Body.ApplyLinearImpulse(ref rb.bufferForce);
                    }
                    if (Math.Abs(rb.bufferTorque) > 0.0f)
                    {
                        rb.Body.ApplyAngularImpulse(rb.bufferTorque);
                    }
                }
            }

            // Physics step
            _world.Step(dt);

            // Bodies > Fluids
            for (int ip = 0; ip < Fluid.Particles.Count; ++ip)
            {
                FluidParticle particle = Fluid.Particles[ip];

                if (!particle.IsActive)
                {
                    continue;
                }

                for (int ic = 0; ic < _fluidCollisions.Count; ++ic)
                {
                    Fixture fixture = _fluidCollisions[ic];

                    if (fixture.IsSensor)
                        continue;

                    Feature result;
                    if (!fixture.RigidBody.Intersect(fixture, ref particle.Position, out result))
                        continue;

                    Vector2 impulse = new Vector2();

                    if (result.Distance < 0.0f) // Inside
                    {
                        Vector2 bodyVelocity = fixture.RigidBody.Body.GetLinearVelocityFromWorldPoint(ref particle.Position);

                        Vector2 velocity = (particle.Position - particle.PreviousPosition) - bodyVelocity * dt;

                        Vector2 velocityNormal = Vector2.Dot(velocity, result.Normal) * result.Normal;
                        Vector2 velocityTangent = velocity - velocityNormal;

                        impulse = velocityNormal + fixture.FluidProperties.Slip * velocityTangent;
                    }
                    else if (fixture.FluidProperties.IsSticky && result.Distance < fixture.FluidProperties.StickDistance) // Outside
                    {
                        impulse = -dt * fixture.FluidProperties.StickForce * result.Distance * (1.0f - result.Distance / fixture.FluidProperties.StickDistance) * result.Normal;
                    }

                    particle.Position -= impulse;

                    if (fixture.RigidBody.Intersect(fixture, ref particle.Position, out result))
                    {
                        if (result.Distance < 0.0f) // Still inside
                        {
                            particle.ApplyImpulse((result.Position - particle.Position) / dt);
                            particle.Position = result.Position;
                        }
                    }
                }
            }

            // Correct velocities
            Fluid.UpdateVelocities(dt);
        }
    }
}