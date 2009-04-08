using FarseerGames.FarseerPhysics.Collisions;

#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Interfaces
{
    public interface INarrowPhaseCollider
    {
        bool Intersect(ref Vector2 vector, out Feature feature);
        INarrowPhaseCollider Clone();
        void Prepare(Geom geometry, object data);
    }
}