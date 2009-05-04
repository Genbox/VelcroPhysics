using System;
using System.Collections.Generic;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    /// <summary>
    /// Provides an implementation of a strongly typed List with Arbiter
    /// </summary>
    public class ArbiterList : List<Arbiter>
    {
        private List<Arbiter> _markedForRemovalList;

        public ArbiterList()
        {
            _markedForRemovalList = new List<Arbiter>();
        }

        public void ForEachSafe(Action<Arbiter> action)
        {
            for (int i = 0; i < Count; i++)
            {
                action(this[i]);
            }
        }

        public void RemoveAllSafe(Predicate<Arbiter> match)
        {
            for (int i = 0; i < Count; i++)
            {
                if (match(this[i]))
                {
                    _markedForRemovalList.Add(this[i]);
                }
            }
            for (int j = 0; j < _markedForRemovalList.Count; j++)
            {
                Remove(_markedForRemovalList[j]);
                _markedForRemovalList[j].Reset();
            }
            _markedForRemovalList.Clear();
        }

        public void RemoveContactCountEqualsZero(Pool<Arbiter> arbiterPool)
        {
            for (int i = 0; i < Count; i++)
            {
                //If they don't have any contacts associated with them. Remove them.
                if (this[i].ContactCount == 0)
                {
                    _markedForRemovalList.Add(this[i]);
                }
            }

            int count = _markedForRemovalList.Count;
            for (int j = 0; j < count; j++)
            {
                Remove(_markedForRemovalList[j]);
                arbiterPool.Insert(_markedForRemovalList[j]);

                //No contacts exist between the two geometries, fire the OnSeperation event.
                if (_markedForRemovalList[j].GeometryA.OnSeparation != null)
                {
                    _markedForRemovalList[j].GeometryA.OnSeparation(_markedForRemovalList[j].GeometryA,
                                                                    _markedForRemovalList[j].GeometryB);
                }

                if (_markedForRemovalList[j].GeometryB.OnSeparation != null)
                {
                    _markedForRemovalList[j].GeometryB.OnSeparation(_markedForRemovalList[j].GeometryB,
                                                                    _markedForRemovalList[j].GeometryA);
                }
            }
            _markedForRemovalList.Clear();
        }

        public void CleanArbiterList(Pool<Arbiter> arbiterPool)
        {
            for (int i = 0; i < Count; i++)
            {
                //Remove those arbiters that have disposed geometries
                if (this[i].GeometryA.IsDisposed || this[i].GeometryB.IsDisposed)
                {
                    _markedForRemovalList.Add(this[i]);
                }

                //Remove the arbiter where both bodies are static
                if (this[i].GeometryA.body.isStatic && this[i].GeometryB.body.isStatic)
                {
                    _markedForRemovalList.Add(this[i]);
                }

                //Remove the arbiter if one of the geometries are not collision enabled
                if (!this[i].GeometryA.CollisionEnabled || !this[i].GeometryB.CollisionEnabled)
                {
                    _markedForRemovalList.Add(this[i]);
                }
            }

            int count = _markedForRemovalList.Count;
            for (int j = 0; j < count; j++)
            {
                //Remove the arbiters from the list
                Remove(_markedForRemovalList[j]);

                //And insert them back into the pool
                arbiterPool.Insert(_markedForRemovalList[j]);
            }

            //Clear the temp arbiter removal list
            _markedForRemovalList.Clear();
        }
    }
}