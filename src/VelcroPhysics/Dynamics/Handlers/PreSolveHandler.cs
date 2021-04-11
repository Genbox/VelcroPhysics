using Genbox.VelcroPhysics.Collision.ContactSystem;
using Genbox.VelcroPhysics.Collision.Narrowphase;

namespace Genbox.VelcroPhysics.Dynamics.Handlers
{
    public delegate void PreSolveHandler(Contact contact, ref Manifold oldManifold);
}