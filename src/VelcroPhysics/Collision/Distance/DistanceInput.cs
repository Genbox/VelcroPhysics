using VelcroPhysics.Shared;

namespace VelcroPhysics.Collision.Distance
{
    /// <summary>
    /// Input for Distance.ComputeDistance().
    /// You have to option to use the shape radii in the computation.
    /// </summary>
    public struct DistanceInput
    {
        public DistanceProxy ProxyA;
        public DistanceProxy ProxyB;
        public Transform TransformA;
        public Transform TransformB;
        public bool UseRadii;
    }
}