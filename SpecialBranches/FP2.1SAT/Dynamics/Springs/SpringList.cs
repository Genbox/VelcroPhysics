using System.Collections.Generic;

namespace FarseerGames.FarseerPhysics.Dynamics.Springs
{
    /// <summary>
    /// Provides an implementation of a strongly typed List with Spring
    /// </summary>
    public class SpringList : List<Spring>
    {
        #region Delegates

        public delegate void ContentsChangedEventHandler(Spring spring);

        #endregion

        private List<Spring> _markedForRemovalList = new List<Spring>();

        public ContentsChangedEventHandler Added;
        public ContentsChangedEventHandler Removed;

        public new void Add(Spring spring)
        {
            base.Add(spring);
            if (Added != null)
            {
                Added(spring);
            }
        }

        public new void Remove(Spring spring)
        {
            base.Remove(spring);
            if (Removed != null)
            {
                Removed(spring);
            }
        }

        public void RemoveDisposed()
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
            _markedForRemovalList.Clear();
        }

        internal static bool IsDisposed(Spring controller)
        {
            return controller.IsDisposed;
        }
    }
}