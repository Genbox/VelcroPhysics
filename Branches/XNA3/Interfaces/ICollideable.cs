using FarseerGames.FarseerPhysics.Collisions;

namespace FarseerGames.FarseerPhysics.Interfaces
{
    internal interface ICollideable<T>
    {
        void Collide(T t, ContactList contactList);
    }
}