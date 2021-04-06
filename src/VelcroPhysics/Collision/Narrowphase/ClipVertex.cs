using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Primitives;

namespace VelcroPhysics.Collision.Narrowphase
{
    /// <summary>
    /// Used for computing contact manifolds.
    /// </summary>
    internal struct ClipVertex
    {
        public ContactID ID;
        public Vector2 V;
    }
}