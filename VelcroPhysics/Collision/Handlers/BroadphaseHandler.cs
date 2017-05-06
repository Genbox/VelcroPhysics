using VelcroPhysics.Dynamics;

namespace VelcroPhysics.Handlers
{
    public delegate void BroadphaseHandler(ref FixtureProxy proxyA, ref FixtureProxy proxyB);
}