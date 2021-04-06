using VelcroPhysics.Dynamics;

namespace VelcroPhysics.Collision.Handlers
{
    public delegate void BroadphaseHandler(ref FixtureProxy proxyA, ref FixtureProxy proxyB);
}