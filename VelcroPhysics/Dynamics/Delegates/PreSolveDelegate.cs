using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Collision.Narrowphase;

namespace VelcroPhysics.Dynamics.Delegates {
    public delegate void PreSolveDelegate(Contact contact, ref Manifold oldManifold);
}