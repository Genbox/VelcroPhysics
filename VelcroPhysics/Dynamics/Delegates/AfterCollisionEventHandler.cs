using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Dynamics.Solver;

namespace VelcroPhysics.Dynamics.Delegates {
    public delegate void AfterCollisionEventHandler(Fixture fixtureA, Fixture fixtureB, Contact contact, ContactVelocityConstraint impulse);
}