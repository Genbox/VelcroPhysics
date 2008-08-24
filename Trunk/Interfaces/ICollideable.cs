using FarseerGames.FarseerPhysics.Dynamics;

namespace FarseerGames.FarseerPhysics.Collisions
{
    internal interface ICollideable<T>
    {
        void Collide(T t, ItemList<Contact> contactList);
    }
}