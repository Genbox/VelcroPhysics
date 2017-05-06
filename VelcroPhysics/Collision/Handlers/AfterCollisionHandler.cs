using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Dynamics.Solver;

namespace VelcroPhysics.Handlers
{
    public delegate void AfterCollisionHandler(Fixture fixtureA, Fixture fixtureB, Contact contact, ContactVelocityConstraint impulse);
}