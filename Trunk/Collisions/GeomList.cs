using System.Collections.Generic;

namespace FarseerGames.FarseerPhysics.Collisions
{
    public class GeomList : List<Geom>
    {
        #region Delegates

        public delegate void ContentsChangedEventHandler(Geom geom);

        #endregion

        private List<Geom> _markedForRemovalList = new List<Geom>();

        private int _numberDisposed;
        public ContentsChangedEventHandler Added;
        public ContentsChangedEventHandler Removed;

        public new void Add(Geom geom)
        {
            base.Add(geom);
            if (Added != null)
            {
                Added(geom);
            }
        }

        public new void Remove(Geom geom)
        {
            base.Remove(geom);
            if (Removed != null)
            {
                Removed(geom);
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

        internal static bool IsDisposed(Geom a)
        {
            return a.IsDisposed;
        }
    }
}