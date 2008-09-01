using System.Collections.Generic;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public class BodyList : List<Body>
    {
        #region Delegates

        public delegate void ContentsChangedEventHandler(Body body);

        #endregion

        private readonly List<Body> markedForRemovalList = new List<Body>();

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
                    markedForRemovalList.Add(this[i]);
                }
            }
            for (int j = 0; j < markedForRemovalList.Count; j++)
            {
                Remove(markedForRemovalList[j]);
            }
            markedForRemovalList.Clear();
        }


        internal static bool IsDisposed(Body a)
        {
            return a.IsDisposed;
        }
    }
}