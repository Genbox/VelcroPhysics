using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Dynamics;

namespace VelcroPhysics.Collision.Handlers
{
    public delegate void OnSeparationHandler(Fixture fixtureA, Fixture fixtureB, Contact contact);
}