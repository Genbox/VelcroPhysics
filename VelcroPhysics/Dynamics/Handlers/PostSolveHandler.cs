using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Dynamics.Solver;

namespace VelcroPhysics.Dynamics.Handlers
{
    public delegate void PostSolveHandler(Contact contact, ContactVelocityConstraint impulse);
}