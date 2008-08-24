using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

#if (XNA)
using Microsoft.Xna.Framework; 
#endif

namespace FarseerGames.FarseerPhysics.Collisions
{
    public class ContactList : List<Contact>
    {
        public ContactList(int capacity) : base(capacity)
        { 

        }

        public ContactList(ContactList contactList) : base(contactList) {
        }

        int index = -1;
        public int IndexOfSafe(Contact contact) {
            index = -1;
            for (int i = 0; i < Count; i++) {
                if (this[i] == contact) {
                    index = i;
                    break;
                }
            }
            return index;
        }
    }
}
