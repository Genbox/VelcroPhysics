using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Interfaces;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public class ItemList<T> : List<T>
    {
        #region Delegates

        public delegate void ContentsChangedEventHandler(T item);

        #endregion

        private readonly List<T> _markedForRemovalList = new List<T>();

        private int _numberDisposed;
        public ContentsChangedEventHandler Added;
        public ContentsChangedEventHandler Removed;

        public new void Add(T item)
        {
            base.Add(item);
            if (Added != null)
            {
                Added(item);
            }
        }

        public new void Remove(T item)
        {
            base.Remove(item);
            if (Removed != null)
            {
                Removed(item);
            }
        }

        public int RemoveDisposed()
        {
            for (int i = 0; i < Count; i++)
            {
                if (IsDisposed(this[i]))
                {
                    _markedForRemovalList.Add(this[i]);
                }
            }
            for (int j = 0; j < _markedForRemovalList.Count; j++)
            {
                Remove(_markedForRemovalList[j]);
            }
            _numberDisposed = _markedForRemovalList.Count;
            _markedForRemovalList.Clear();
            return _numberDisposed;
        }

        internal static bool IsDisposed(T item)
        {
            return ((IIsDisposable) item).IsDisposed;
        }
    }
}