using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Dynamics;

namespace VelcroPhysics.Collision.Handlers
{
    public delegate void OnCollisionHandler(Fixture fixtureA, Fixture fixtureB, Contact contact);
}