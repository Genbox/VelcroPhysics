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
                if (this[i].ContainsInvalidGeom())
                {
                    _markedForRemovalList.Add(this[i]);
                }
            }
            for (int j = 0; j < _markedForRemovalList.Count; j++)
            {
                Remove(_markedForRemovalList[j]);
                arbiterPool.Insert(_markedForRemovalList[j]);
            }
            _markedForRemovalList.Clear();
        }
    }
}