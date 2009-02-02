using FarseerGames.FarseerPhysics.Collisions;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Interfaces
{
    public interface INarrowPhaseCollider
    {
        bool Intersect(ref Vector2 vector, out Feature feature);
        INarrowPhaseCollider Clone();
        void Prepare(Geom geometry, ColliderData data);
    }
}