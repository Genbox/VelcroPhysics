using System.Collections.Generic;

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    /// <summary>
    /// Provides an implementation of a strongly typed List with Joint
    /// </summary>
    public class JointList : List<Joint>
    {
        #region Delegates

        public delegate void ContentsChangedEventHandler(Joint joint);

        #endregion

        private List<Joint> _markedForRemovalList = new List<Joint>();

        public ContentsChangedEventHandler Added;
        public ContentsChangedEventHandler Removed;

        public new void Add(Joint joint)
        {
            base.Add(joint);
            if (Added != null)
            {
                Added(joint);
            }
        }

        public new void Remove(Joint joint)
        {
            base.Remove(joint);
            if (Removed != null)
            {
                Removed(joint);
            }
        }

        public void RemoveDisposed()
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].IsDisposed)
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
    }
}