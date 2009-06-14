#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

using FarseerGames.FarseerPhysics.Collisions;

namespace FarseerGames.FarseerPhysics.Interfaces
{
    public interface INarrowPhaseCollider
    {
        void Collide(Geom geomA, Geom geomB, ContactList contactList);
        //INarrowPhaseCollider Clone();
        bool Intersect(Geom geom, ref Vector2 point);
    }
}
