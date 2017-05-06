using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Dynamics.Solver;

namespace VelcroPhysics.Dynamics.Delegates {
    public delegate void PostSolveDelegate(Contact contact, ContactVelocityConstraint impulse);
}