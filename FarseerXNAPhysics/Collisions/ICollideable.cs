using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerXNAPhysics.Collisions {
    interface ICollideable<T> {
        void Collide(T t, ContactList contactList);
    }
}
