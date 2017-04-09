using VelcroPhysics.Dynamics;

namespace VelcroPhysics.Common.PhysicsLogic
{
    internal struct ShapeData
    {
        public Body Body;
        public float Max;
        public float Min; // absolute angles
    }
}