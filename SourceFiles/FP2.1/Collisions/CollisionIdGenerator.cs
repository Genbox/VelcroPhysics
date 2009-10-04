namespace FarseerGames.FarseerPhysics.Collisions
{
    public class CollisionIdGenerator
    {
        private int _currentId;

        public int NextCollisionId()
        {
            return _currentId++;
        }
    }
}
