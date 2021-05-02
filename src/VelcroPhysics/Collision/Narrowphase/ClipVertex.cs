using Genbox.VelcroPhysics.Collision.ContactSystem;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Collision.Narrowphase
{
    /// <summary>Used for computing contact manifolds.</summary>
    internal struct ClipVertex
    {
        public ContactId Id;
        public Vector2 V;
    }
}