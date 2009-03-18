using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Interfaces;

namespace FarseerGames.FarseerPhysics
{
    public class GenericList<T> : List<T> where T : IIsDisposable
    {
        public delegate void ContentsChangedEventHandler(T element);
        private List<T> _markedForRemovalList = new List<T>();
        private int _numberDisposed;
        private int _count;
        public ContentsChangedEventHandler Added;
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
