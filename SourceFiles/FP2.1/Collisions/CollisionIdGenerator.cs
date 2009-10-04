namespace FarseerGames.FarseerPhysics.Collisions
{
    public class CollisionIdGenerator
    {
        private int _currentId;

        public int nextCollisionId()
        {
            return _currentId++;
        }
    }
}
