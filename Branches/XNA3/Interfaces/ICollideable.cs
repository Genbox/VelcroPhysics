namespace FarseerGames.FarseerPhysics.Collisions
{
    internal interface ICollideable<T>
    {
        void Collide(T t, ContactList contactList);
    }
}