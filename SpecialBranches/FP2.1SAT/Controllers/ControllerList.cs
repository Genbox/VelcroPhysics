using System.Collections.Generic;

namespace FarseerGames.FarseerPhysics.Controllers
{
    /// <summary>
    /// Provides an implementation of a strongly typed List with Controller
    /// </summary>
    public class ControllerList : List<Controller>
    {
        #region Delegates

        public delegate void ContentsChangedEventHandler(Controller controller);

        #endregion

        private List<Controller> _markedForRemovalList = new List<Controller>();

        public ContentsChangedEventHandler Added;
        public ContentsChangedEventHandler Removed;

        /// <summary>
        /// Adds the specified controller.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public new void Add(Controller controller)
        {
            base.Add(controller);
            if (Added != null)
            {
                Added(controller);
            }
        }

        /// <summary>
        /// Removes the specified controller.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public new void Remove(Controller controller)
        {
            base.Remove(controller);
            if (Removed != null)
            {
                Removed(controller);
            }
        }

        /// <summary>
        /// Removes the disposed controllers.
        /// </summary>
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