using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FarseerPhysics.Fluids
{
    //TODO: Could be struct?
    public class SpringHash : IEqualityComparer<SpringHash>
    {
        public FluidParticle P0;
        public FluidParticle P1;

        public bool Equals(SpringHash lhs, SpringHash rhs)
        {
            return (lhs.P0.Index == rhs.P0.Index && lhs.P1.Index == rhs.P1.Index)
                || (lhs.P0.Index == rhs.P1.Index && lhs.P1.Index == rhs.P0.Index);
        }

        public int GetHashCode(SpringHash s)
        {
            return (s.P0.Index * 73856093) ^ (s.P1.Index * 19349663) ^ (s.P0.Index * 19349663) ^ (s.P1.Index * 73856093);
        }
    }

    public class Spring
    {
        public FluidParticle P0;
        public FluidParticle P1;

        public Spring(FluidParticle p0, FluidParticle p1)
        {
            Active = true;
            P0 = p0;
            P1 = p1;
        }

        public bool Active { get; set; }
        public float RestLength { get; set; }

        public void Update(float timeStep, float kSpring, float influenceRadius)
        {
            if (!Active)
                return;

            Vector2 dir = P1.Position - P0.Position;
            float distance = dir.Length();
            dir.Normalize();

            // This is to avoid imploding simulation with really springy fluids
            if (distance < 0.5f * influenceRadius)
            {
                Active = false;
                return;
            }
            if (RestLength > influenceRadius)
            {
                Active = false;
                return;
            }
            
            //Algorithm 3
            float displacement = timeStep * timeStep * kSpring * (1.0f - RestLength / influenceRadius) * (RestLength - distance) * 0.5f;

            dir *= displacement;

            P0.Position -= dir;
            P1.Position += dir;
        }
    }
}
