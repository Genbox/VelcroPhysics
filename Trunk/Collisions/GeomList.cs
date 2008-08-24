using System.Collections.Generic;

namespace FarseerGames.FarseerPhysics.Collisions
{
    public class GeomList : List<Geom>
    {
        #region Delegates

        public delegate void ContentsChangedEventHandler(Geom geom);

        #endregion

        private readonly List<Geom> markedForRemovalList = new List<Geom>();

        public ContentsChangedEventHandler Added;
        private int numberDisposed;
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
                    markedForRemovalList.Add(this[i]);
                }
            }
            for (int j = 0; j < markedForRemovalList.Count; j++)
            {
                Remove(markedForRemovalList[j]);
            }
            numberDisposed = markedForRemovalList.Count;
            markedForRemovalList.Clear();
            return numberDisposed;
        }

        internal static bool IsDisposed(Geom a)
        {
            return a.IsDisposed;
        }
    }
}