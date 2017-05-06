using VelcroPhysics.Collision.ContactSystem;

namespace VelcroPhysics.Dynamics.Delegates {
    /// <summary>
    /// This delegate is called when a contact is created
    /// </summary>
    public delegate bool BeginContactDelegate(Contact contact);
}