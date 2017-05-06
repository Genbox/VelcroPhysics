using VelcroPhysics.Collision.ContactSystem;

namespace VelcroPhysics.Dynamics.Delegates {
    public delegate void OnSeparationEventHandler(Fixture fixtureA, Fixture fixtureB, Contact contact);
}