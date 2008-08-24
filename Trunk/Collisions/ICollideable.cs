using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerPhysics.Collisions {
    interface ICollideable<T> {
        void Collide(T t, ContactList contactList);
    }
}
