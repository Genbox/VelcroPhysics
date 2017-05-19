using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Collision.Narrowphase;

namespace VelcroPhysics.Dynamics.Handlers
{
    public delegate void PreSolveHandler(Contact contact, ref Manifold oldManifold);
}