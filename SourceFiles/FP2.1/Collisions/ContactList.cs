using System.Collections.Generic;

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <summary>
    /// Provides an implementation of a strongly typed List with Contact
    /// </summary>
    public class ContactList : List<Contact>
    {
        private int _index = -1;

        public ContactList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// A safe way of getting an index
        /// </summary>
        /// <param name="contact">The contact.</param>
        /// <returns></returns>
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