using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.ContactSystem;

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