using VelcroPhysics.Collision;
using VelcroPhysics.Collision.Primitives;

namespace VelcroPhysics.Dynamics
{
    /// <summary>
    /// This proxy is used internally to connect fixtures to the broad-phase.
    /// </summary>
    public struct FixtureProxy
    {
        public AABB AABB;
        public int ChildIndex;
        public Fixture Fixture;
        public int ProxyId;
    }
}