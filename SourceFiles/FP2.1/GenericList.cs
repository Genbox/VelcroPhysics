using System.Collections.Generic;
using System.Xml.Serialization;
using FarseerGames.FarseerPhysics.Interfaces;

#if(XNA)
using Microsoft.Xna.Framework.Content;
#endif

namespace FarseerGames.FarseerPhysics
{
    public class GenericList<T> : List<T> where T : IIsDisposable
    {
        public delegate void ContentsChangedEventHandler(T element);
        private List<T> _markedForRemovalList = new List<T>();
        private int _numberDisposed;
        private int _count;

        public GenericList(int capacity)
        {
            Capacity = capacity;
        }

#if(XNA)
        [ContentSerializerIgnore]
#endif
        [XmlIgnore]
        public ContentsChangedEventHandler Added;

#if(XNA)
        [ContentSerializerIgnore]
#endif
        [XmlIgnore]
        public ContentsChangedEventHandler Removed;

        public new void Add(T element)
        {
            base.Add(element);
            if (Added != null)
            {
                Added(element);
            }
        }

        public new void Remove(T element)
        {
            base.Remove(element);
            if (Removed != null)
            {
                Removed(element);
            }
        }

        /// <summary>
        /// Removes the disposed items
        /// It determines if it is disposed by checking the
        /// IsDisposed property on the item.
        /// </summary>
        /// <returns>The number of items that was removed</returns>
        public int RemoveDisposed()
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].IsDisposed)
                {
                    _markedForRemovalList.Add(this[i]);
                }
            }

            _count = _markedForRemovalList.Count;
            for (int j = 0; j < _count; j++)
            {
                Remove(_markedForRemovalList[j]);
            }
            _numberDisposed = _markedForRemovalList.Count;
            _markedForRemovalList.Clear();
            return _numberDisposed;
        }
    }
}
