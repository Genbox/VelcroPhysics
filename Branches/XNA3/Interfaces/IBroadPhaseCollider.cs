using FarseerGames.FarseerPhysics.Collisions;

namespace FarseerGames.FarseerPhysics.Interfaces
{
    public interface IBroadPhaseCollider
    {
        void ProcessRemovedGeoms();
        void ProcessDisposedGeoms();
        void Add(Geom geom);
        void Update();
    }
}