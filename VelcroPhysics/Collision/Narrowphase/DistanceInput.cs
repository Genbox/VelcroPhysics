using VelcroPhysics.Shared;

namespace VelcroPhysics.Collision.Narrowphase
{
    /// <summary>
    /// Input for Distance.ComputeDistance().
    /// You have to option to use the shape radii in the computation.
    /// </summary>
    public class DistanceInput
    {
        public DistanceProxy ProxyA = new DistanceProxy();
        public DistanceProxy ProxyB = new DistanceProxy();
        public Transform TransformA;
        public Transform TransformB;
        public bool UseRadii;
    }
}