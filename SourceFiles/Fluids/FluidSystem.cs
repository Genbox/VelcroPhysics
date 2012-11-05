using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Fluids
{
    public class FluidSystem
    {
        private float _influenceRadiusSquared;
        private HashGrid _hashGrid = new HashGrid();
        private Dictionary<SpringHash, Spring> _springs = new Dictionary<SpringHash, Spring>();
        private List<SpringHash> _springsToRemove = new List<SpringHash>();
        private Vector2 _totalForce;

        public FluidSystem()
        {
            Gravity = new Vector2(0.0f, 10.0f);
            Particles = new List<FluidParticle>();
            DefaultDefinition();
        }

        public FluidDefinition Definition { get; private set; }

        public List<FluidParticle> Particles { get; private set; }

        public Vector2 Gravity { get; set; }

        public void DefaultDefinition()
        {
            SetDefinition(FluidDefinition.Default);
        }

        public void SetDefinition(FluidDefinition def)
        {
            Definition = def;
            Definition.Check();
            _influenceRadiusSquared = Definition.InfluenceRadius * Definition.InfluenceRadius;
        }

        public FluidParticle AddParticle(Vector2 p)
        {
            FluidParticle particle = new FluidParticle(p) { Index = Particles.Count };
            Particles.Add(particle);
            return particle;
        }

        public void Clear()
        {

        }

        public void ApplyForce(Vector2 f)
        {
            _totalForce += f;
        }

        private void ApplyForces()
        {
            Vector2 f = Gravity + _totalForce;

            for (int i = 0; i < Particles.Count; ++i)
            {
                Particles[i].ApplyForce(f);
            }

            _totalForce = Vector2.Zero;
        }

        private void ApplyViscosity(FluidParticle p, float timeStep)
        {
            for (int i = 0; i < p.Neighbours.Count; ++i)
            {
                FluidParticle neighbour = p.Neighbours[i];

                if (p.Index >= neighbour.Index)
                {
                    continue;
                }

                float q;
                Vector2.DistanceSquared(ref p.Position, ref neighbour.Position, out q);

                if (q > _influenceRadiusSquared)
                {
                    continue;
                }

                Vector2 direction;
                Vector2.Subtract(ref neighbour.Position, ref p.Position, out direction);

                if (direction.LengthSquared() < float.Epsilon)
                {
                    continue;
                }

                direction.Normalize();

                Vector2 deltaVelocity;
                Vector2.Subtract(ref p.Velocity, ref neighbour.Velocity, out deltaVelocity);

                float u;
                Vector2.Dot(ref deltaVelocity, ref direction, out u);

                if (u > 0.0f)
                {
                    q = 1.0f - (float)Math.Sqrt(q) / Definition.InfluenceRadius;

                    float impulseFactor = 0.5f * timeStep * q * (u * (Definition.ViscositySigma + Definition.ViscosityBeta * u));

                    Vector2 impulse;

                    Vector2.Multiply(ref direction, -impulseFactor, out impulse);
                    p.ApplyImpulse(ref impulse);

                    Vector2.Multiply(ref direction, impulseFactor, out impulse);
                    neighbour.ApplyImpulse(ref impulse);
                }
            }
        }

        private void DoubleDensityRelaxation(FluidParticle p, float timeStep)
        {
            float density = 0.0f;
            float densityNear = 0.0f;

            for (int i = 0; i < p.Neighbours.Count; ++i)
            {
                FluidParticle neighbour = p.Neighbours[i];

                if (p.Index == neighbour.Index)
                {
                    continue;
                }

                float q;
                Vector2.DistanceSquared(ref p.Position, ref neighbour.Position, out q);

                if (q > _influenceRadiusSquared)
                {
                    continue;
                }

                q = 1.0f - (float)Math.Sqrt(q) / Definition.InfluenceRadius;

                float densityDelta = q * q;
                density += densityDelta;
                densityNear += densityDelta * q;
            }

            float pressure = Definition.Stiffness * (density - Definition.DensityRest);
            float pressureNear = Definition.StiffnessNear * densityNear;

            // For gameplay purposes
            p.Density = density + densityNear;
            p.Pressure = pressure + pressureNear;

            Vector2 delta = Vector2.Zero;

            for (int i = 0; i < p.Neighbours.Count; ++i)
            {
                FluidParticle neighbour = p.Neighbours[i];

                if (p.Index == neighbour.Index)
                {
                    continue;
                }

                float q;
                Vector2.DistanceSquared(ref p.Position, ref neighbour.Position, out q);

                if (q > _influenceRadiusSquared)
                {
                    continue;
                }

                q = 1.0f - (float)Math.Sqrt(q) / Definition.InfluenceRadius;

                float dispFactor = 0.5f * timeStep * timeStep * (q * (pressure + pressureNear * q));

                Vector2 direction;
                Vector2.Subtract(ref neighbour.Position, ref p.Position, out direction);

                if (direction.LengthSquared() < float.Epsilon)
                {
                    continue;
                }

                direction.Normalize();

                Vector2 disp;

                Vector2.Multiply(ref direction, dispFactor, out disp);
                Vector2.Add(ref neighbour.Position, ref disp, out neighbour.Position);

                Vector2.Multiply(ref direction, -dispFactor, out disp);
                Vector2.Add(ref delta, ref disp, out delta);
            }

            Vector2.Add(ref p.Position, ref delta, out p.Position);
        }

        private void CreateSprings(FluidParticle p)
        {
            for (int i = 0; i < p.Neighbours.Count; ++i)
            {
                FluidParticle neighbour = p.Neighbours[i];

                if (p.Index >= neighbour.Index)
                    continue;

                float q;
                Vector2.DistanceSquared(ref p.Position, ref neighbour.Position, out q);

                if (q > _influenceRadiusSquared)
                    continue;

                SpringHash hash = new SpringHash { P0 = p, P1 = neighbour };

                if (!_springs.ContainsKey(hash))
                {
                    //TODO: Use pool?
                    Spring spring = new Spring(p, neighbour) { RestLength = (float)Math.Sqrt(q) };
                    _springs.Add(hash, spring);
                }
            }
        }

        private void AdjustSprings(float timeStep)
        {
            foreach (var pair in _springs)
            {
                Spring spring = pair.Value;

                spring.Update(timeStep, Definition.KSpring, Definition.InfluenceRadius);

                if (spring.Active)
                {
                    float L = spring.RestLength;
                    float distance;
                    Vector2.Distance(ref spring.P0.Position, ref spring.P1.Position, out distance);

                    if (distance > (L + (Definition.YieldRatioStretch * L)))
                    {
                        spring.RestLength += timeStep * Definition.Plasticity * (distance - L - (Definition.YieldRatioStretch * L));
                    }
                    else if (distance < (L - (Definition.YieldRatioCompress * L)))
                    {
                        spring.RestLength -= timeStep * Definition.Plasticity * (L - (Definition.YieldRatioCompress * L) - distance);
                    }
                }
                else
                {
                    _springsToRemove.Add(pair.Key);
                }
            }

            for (int i = 0; i < _springsToRemove.Count; ++i)
            {
                _springs.Remove(_springsToRemove[i]);
            }
        }

        private void ComputeNeighbours()
        {
            _hashGrid.GridSize = Definition.InfluenceRadius;
            _hashGrid.Clear();

            for (int i = 0; i < Particles.Count; ++i)
            {
                FluidParticle p = Particles[i];

                if (p.IsActive)
                {
                    _hashGrid.Add(p);
                }
            }

            for (int i = 0; i < Particles.Count; ++i)
            {
                FluidParticle p = Particles[i];
                p.Neighbours.Clear();
                _hashGrid.Find(ref p.Position, p.Neighbours);
            }
        }

        public void Update(float timeStep)
        {
            ComputeNeighbours();
            ApplyForces();

            if (Definition.UseViscosity)
            {
                for (int i = 0; i < Particles.Count; ++i)
                {
                    FluidParticle p = Particles[i];
                    if (p.IsActive)
                    {
                        ApplyViscosity(p, timeStep);
                    }
                }
            }

            for (int i = 0; i < Particles.Count; ++i)
            {
                FluidParticle p = Particles[i];
                if (p.IsActive)
                {
                    p.Update(timeStep);
                }
            }

            for (int i = 0; i < Particles.Count; ++i)
            {
                FluidParticle p = Particles[i];
                if (p.IsActive)
                {
                    DoubleDensityRelaxation(p, timeStep);
                }
            }

            if (Definition.UsePlasticity)
            {
                for (int i = 0; i < Particles.Count; ++i)
                {
                    FluidParticle p = Particles[i];
                    if (p.IsActive)
                    {
                        CreateSprings(p);
                    }
                }
            }

            AdjustSprings(timeStep);

            UpdateVelocities(timeStep);
        }

        internal void UpdateVelocities(float timeStep)
        {
            for (int i = 0; i < Particles.Count; ++i)
            {
                Particles[i].UpdateVelocity(timeStep);
            }
        }
    }
}
