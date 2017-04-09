using VelcroPhysics.Collision.Narrowphase;

namespace VelcroPhysics.Collision.TOI
{
    /// <summary>
    /// Input parameters for CalculateTimeOfImpact
    /// </summary>
    public class TOIInput
    {
        public DistanceProxy ProxyA = new DistanceProxy();
        public DistanceProxy ProxyB = new DistanceProxy();
        public Sweep SweepA;
        public Sweep SweepB;
        public float TMax; // defines sweep interval [0, tMax]
    }
}