using System.Collections.Generic;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    /// <summary>
    /// Provides an implementation of a strongly typed List with Body
    /// </summary>
    public class BodyList : List<Body>
    {
        #region Delegates

        public delegate void ContentsChangedEventHandler(Body body);

        #endregion

        private List<Body> _markedForRemovalList = new List<Body>();

        public ContentsChangedEventHandler Added;
        public ContentsChangedEventHandler Removed;

        public new void Add(Body body)
        {
            base.Add(body);
            if (Added != null)
            {
                Added(body);
            }
        }

        public new void Remove(Body body)
        {
            base.Remove(body);
            if (Removed != null)
            {
                Removed(body);
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


        internal static bool IsDisposed(Body a)
        {
            return a.IsDisposed;
        }
    }
}