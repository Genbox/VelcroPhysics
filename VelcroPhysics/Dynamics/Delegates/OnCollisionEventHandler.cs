using VelcroPhysics.Collision.ContactSystem;

namespace VelcroPhysics.Dynamics.Delegates {
    public delegate void OnCollisionEventHandler(Fixture fixtureA, Fixture fixtureB, Contact contact);
}