using FarseerGames.FarseerPhysics.Collisions;

namespace FarseerGames.FarseerPhysics.Interfaces
{
    public interface INarrowPhaseCollider
    {
        void Collide(Geom geomA, Geom geomB, ContactList contactList);
    }
}
