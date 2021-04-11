using Genbox.VelcroPhysics.Dynamics;

namespace Genbox.VelcroPhysics.Collision.Handlers
{
    public delegate void BroadphaseHandler(ref FixtureProxy proxyA, ref FixtureProxy proxyB);
}