using Microsoft.Xna.Framework;

namespace VelcroPhysics.Dynamics.Solver
{
    public sealed class VelocityConstraintPoint
    {
        public float normalImpulse;
        public float normalMass;
        public Vector2 rA;
        public Vector2 rB;
        public float tangentImpulse;
        public float tangentMass;
        public float velocityBias;
    }
}