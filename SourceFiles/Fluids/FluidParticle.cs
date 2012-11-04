using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FarseerPhysics.Fluids
{
    public class FluidParticle
    {
        public Vector2 Position;
        public Vector2 PreviousPosition;

        public Vector2 Velocity;
        public Vector2 Acceleration;

        internal FluidParticle(Vector2 position)
        {
            Neighbours = new List<FluidParticle>();
            IsActive = true;
            MoveTo(position);

            Damping = 0.0f;
            Mass = 1.0f;
        }

        public bool IsActive { get; set; }

        public List<FluidParticle> Neighbours { get; private set; }

        // For gameplay purposes
        public float Density { get; internal set; }
        public float Pressure { get; internal set; }

        // Other properties
        public int Index { get; internal set; }

        // Physics properties
        public float Damping { get; set; }
        public float Mass { get; set; }

        public void MoveTo(Vector2 p)
        {
            Position = p;
            PreviousPosition = p;

            Velocity = Vector2.Zero;
            Acceleration = Vector2.Zero;
        }

        public void ApplyForce(Vector2 force)
        {
            ApplyForce(ref force);
        }

        public void ApplyForce(ref Vector2 force)
        {
            Acceleration += force * Mass;
        }

        public void ApplyImpulse(Vector2 impulse)
        {
            ApplyImpulse(ref impulse);
        }

        public void ApplyImpulse(ref Vector2 impulse)
        {
            Velocity += impulse;
        }

        public void Update(float timeStep)
        {
            Velocity += Acceleration * timeStep;

            Vector2 delta = (1.0f - Damping) * Velocity * timeStep;

            PreviousPosition = Position;
            Position += delta;

            Acceleration = Vector2.Zero;
        }

        public void UpdateVelocity(float timeStep)
        {
            Velocity = (Position - PreviousPosition) / timeStep;
        }
    }
}
