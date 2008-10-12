using System.Collections.Generic;

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <summary>
    /// Provides an implementation of a strongly typed List with Geom
    /// </summary>
    public class GeomList : List<Geom>
    {
        #region Delegates

        /// <summary>
        /// Occurs when the content is changed of the list
        /// </summary>
        public delegate void ContentsChangedEventHandler(Geom geom);

        #endregion

        private List<Geom> _markedForRemovalList = new List<Geom>();

        private int _numberDisposed;
        public ContentsChangedEventHandler Added;
        public ContentsChangedEventHandler Removed;

        /// <summary>
        /// Adds the specified geom.
        /// </summary>
        /// <param name="geom">The geom.</param>
        public new void Add(Geom geom)
        {
            base.Add(geom);
            if (Added != null)
            {
                Added(geom);
            }
        }

        /// <summary>
        /// Removes the specified geom.
        /// </summary>
        /// <param name="geom">The geom.</param>
        public new void Remove(Geom geom)
        {
            base.Remove(geom);
            if (Removed != null)
            {
                Removed(geom);
            }
        }

        /// <summary>
        /// Removes the disposed geoms
        /// </summary>
        /// <returns></returns>
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