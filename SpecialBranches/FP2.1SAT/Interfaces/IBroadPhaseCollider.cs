using FarseerGames.FarseerPhysics.Collisions;

namespace FarseerGames.FarseerPhysics.Interfaces
{
    //NOTE: This is public to all classes. Should have made the IBroadPhaseCollider an abstract class
    //and the methods virtual. But that would cause the msil callvir() to be called instead of native
    //method call. Can hurt performance, especially on Xbox360.
    public delegate bool BroadPhaseCollisionHandler(Geom geometry1, Geom geometry2);

    public interface IBroadPhaseCollider
    {
        void ProcessRemovedGeoms();
        void ProcessDisposedGeoms();
        void Add(Geom geom);
        void Update();
        event BroadPhaseCollisionHandler OnBroadPhaseCollision;
    }
}