using System.Collections.Generic;

namespace FarseerGames.FarseerPhysics.Collisions
{
    public class ContactList : List<Contact>
    {
        private int index = -1;

        public ContactList(int capacity) : base(capacity)
        {
        }

        public ContactList(ContactList contactList) : base(contactList)
        {
        }

        public int IndexOfSafe(Contact contact)
        {
            index = -1;
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == contact)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
    }
}