using Genbox.VelcroPhysics.Collision.ContactSystem;
using Genbox.VelcroPhysics.Dynamics.Solver;

namespace Genbox.VelcroPhysics.Dynamics.Handlers
{
    public delegate void PostSolveHandler(Contact contact, ContactVelocityConstraint impulse);
}