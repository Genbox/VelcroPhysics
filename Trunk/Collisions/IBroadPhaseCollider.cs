namespace FarseerGames.FarseerPhysics.Collisions
{
    public interface IBroadPhaseCollider
    {
        void ProcessRemovedGeoms();
        void ProcessDisposedGeoms();
        void Add(Geom geom);
        void Update();
    }
}