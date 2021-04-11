using Genbox.VelcroPhysics.Collision.ContactSystem;
using Genbox.VelcroPhysics.Dynamics;

namespace Genbox.VelcroPhysics.Collision.Handlers
{
    public delegate void OnSeparationHandler(Fixture fixtureA, Fixture fixtureB, Contact contact);
}