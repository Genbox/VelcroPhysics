using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Controllers;

namespace FarseerGames.FarseerPhysics.Controllers
{
    public class ControllerList : List<Controller>
    {
        #region Delegates

        public delegate void ContentsChangedEventHandler(Controller controller);

        #endregion

        private readonly List<Controller> _markedForRemovalList = new List<Controller>();

        public ContentsChangedEventHandler Added;
        public ContentsChangedEventHandler Removed;

        public new void Add(Controller controller)
        {
            base.Add(controller);
            if (Added != null)
            {
                Added(controller);
            }
        }

        public new void Remove(Controller controller)
        {
            base.Remove(controller);
            if (Removed != null)
            {
                Removed(controller);
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

        internal static bool IsDisposed(Controller controller)
        {
            return controller.IsDisposed;
        }
    }
}