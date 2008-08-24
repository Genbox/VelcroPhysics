using System.Collections.Generic;

namespace FarseerGames.FarseerPhysics.Collisions
{
    public class ContactList : List<Contact>
    {
        private int _index = -1;

        public ContactList(int capacity) : base(capacity)
        {
        }

        public ContactList(ContactList contactList) : base(contactList)
        {
        }

        public int IndexOfSafe(Contact contact)
        {
            _index = -1;
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == contact)
                {
                    _index = i;
                    break;
                }
            }
            return _index;
        }
    }
}