using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Interfaces;

namespace FarseerGames.FarseerPhysics.Collisions
{
    public sealed class SelectiveSweepCollider : IBroadPhaseCollider
    {
        private static readonly StubComparer comparer = new StubComparer();

        private readonly PhysicsSimulator _physicsSimulator;
        private readonly List<Wrapper> _wrappers;
        private readonly List<Stub> _xStubs;
        private readonly List<Stub> _yStubs;

        public SelectiveSweepCollider(PhysicsSimulator physicsSimulator)
        {
            _physicsSimulator = physicsSimulator;
            _wrappers = new List<Wrapper>();
            _xStubs = new List<Stub>();
            _yStubs = new List<Stub>();
        }

        #region IBroadPhaseCollider Members

        public void Add(Geom item)
        {
            Wrapper wrapper = new Wrapper(item);
            _wrappers.Add(wrapper);
            wrapper.AddStubs(_xStubs, _yStubs);
        }

        public void ProcessRemovedGeoms()
        {
            if (_wrappers.RemoveAll(WrapperIsRemoved) > 0)
            {
                _xStubs.RemoveAll(StubIsRemoved);
                _yStubs.RemoveAll(StubIsRemoved);
            }
        }

        public void ProcessDisposedGeoms()
        {
            if (_wrappers.RemoveAll(WrapperIsDisposed) > 0)
            {
                _xStubs.RemoveAll(StubIsDisposed);
                _yStubs.RemoveAll(StubIsDisposed);
            }
        }

        public void Update()
        {
            InternalUpdate();
            DetectInternal(ShouldDoX());
        }

        #endregion

        public void Clear()
        {
            _wrappers.Clear();
            _xStubs.Clear();
            _yStubs.Clear();
        }

        private static bool WrapperIsDisposed(Wrapper wrapper)
        {
            return wrapper.geom.IsDisposed;
        }

        private static bool StubIsDisposed(Stub stub)
        {
            return stub.wrapper.geom.IsDisposed;
        }

        private static bool WrapperIsRemoved(Wrapper wrapper)
        {
            return wrapper.geom.IsRemoved;
        }

        private static bool StubIsRemoved(Stub stub)
        {
            return stub.wrapper.geom.IsRemoved;
        }

        /// <summary>
        /// updates all the nodes to their new values and sorts the lists
        /// </summary>
        public void InternalUpdate()
        {
            for (int index = 0; index < _wrappers.Count; ++index)
            {
                _wrappers[index].Update();
            }
            _xStubs.Sort(comparer);
            _yStubs.Sort(comparer);
        }

        /// <summary>
        /// Finds how many collisions there are on the x and y and returns if
        /// the x axis has the least
        /// </summary>
        private bool ShouldDoX()
        {
            int xCount = 0;
            int xdepth = 0;
            int yCount = 0;
            int ydepth = 0;
            for (int index = 0; index < _xStubs.Count; index++)
            {
                if (_xStubs[index].begin)
                {
                    xCount += xdepth++;
                }
                else
                {
                    xdepth--;
                }

                if (_yStubs[index].begin)
                {
                    yCount += ydepth++;
                }
                else
                {
                    ydepth--;
                }
            }
            return xCount < yCount;
        }

        private void DetectInternal(bool doX)
        {
            List<Stub> stubs = (doX) ? (_xStubs) : (_yStubs);
            LinkedList<Wrapper> currentBodies = new LinkedList<Wrapper>();
            for (int index = 0; index < stubs.Count; index++)
            {
                Stub stub = stubs[index];
                Wrapper wrapper1 = stub.wrapper;
                if (stub.begin)
                {
                    //set the min and max values
                    if (doX)
                    {
                        wrapper1.SetY();
                    }
                    else
                    {
                        wrapper1.SetX();
                    }

                    Geom geom1 = wrapper1.geom;
                    for (LinkedListNode<Wrapper> node = currentBodies.First;
                         node != null;
                         node = node.Next)
                    {
                        Wrapper wrapper2 = node.Value;
                        Geom geom2 = wrapper2.geom;
                        if (wrapper1.min <= wrapper2.max && //tests the other axis
                            wrapper2.min <= wrapper1.max)
                        {
                            OnCollision(geom1, geom2);
                        }
                    }
                    if (wrapper1.shouldAddNode)
                    {
                        currentBodies.AddLast(wrapper1.node);
                    }
                }
                else
                {
                    if (wrapper1.shouldAddNode)
                    {
                        currentBodies.Remove(wrapper1.node);
                    }
                }
            }
        }

        private void OnCollision(Geom geom1, Geom geom2)
        {
            if (!geom1.Body.enabled || !geom2.Body.enabled)
            {
                return;
            }

            if ((geom1.CollisionGroup == geom2.CollisionGroup) &&
                geom1.CollisionGroup != 0 &&
                geom2.CollisionGroup != 0)
            {
                return;
            }

            if (!geom1.CollisionEnabled || !geom2.CollisionEnabled)
            {
                return;
            }

            if (geom1.Body.isStatic && geom2.Body.isStatic)
            {
                //don't collide two static bodies
                return;
            }

            if (((geom1.CollisionCategories & geom2.CollidesWith) == Enums.CollisionCategories.None) &
                ((geom2.CollisionCategories & geom1.CollidesWith) == Enums.CollisionCategories.None))
            {
                return;
            }

            Arbiter arbiter = _physicsSimulator.arbiterPool.Fetch();
            arbiter.ConstructArbiter(geom1, geom2, _physicsSimulator);

            if (!_physicsSimulator.ArbiterList.Contains(arbiter))
            {
                _physicsSimulator.ArbiterList.Add(arbiter);
            }
            else
            {
                _physicsSimulator.arbiterPool.Release(arbiter);
            }
        }

        #region Nested type: Stub

        private sealed class Stub
        {
            public readonly bool begin;
            public readonly Wrapper wrapper;
            public float value;

            public Stub(Wrapper wrapper, bool begin)
            {
                this.wrapper = wrapper;
                this.begin = begin;
            }
        }

        #endregion

        #region Nested type: StubComparer

        private sealed class StubComparer : IComparer<Stub>
        {
            #region IComparer<Stub> Members

            public int Compare(Stub left, Stub right)
            {
                if (left.value < right.value)
                {
                    return -1;
                }
                if (left.value > right.value)
                {
                    return 1;
                }
                return ((left == right) ? (0) : ((left.begin) ? (-1) : (1)));
            }

            #endregion
        }

        #endregion

        #region Nested type: Wrapper

        private sealed class Wrapper
        {
            public readonly Geom geom;
            public readonly LinkedListNode<Wrapper> node;
            private readonly Stub xBegin;
            private readonly Stub xEnd;
            private readonly Stub yBegin;
            private readonly Stub yEnd;
            public float max;
            public float min;
            public bool shouldAddNode;

            public Wrapper(Geom body)
            {
                geom = body;
                node = new LinkedListNode<Wrapper>(this);
                xBegin = new Stub(this, true);
                xEnd = new Stub(this, false);
                yBegin = new Stub(this, true);
                yEnd = new Stub(this, false);
            }

            public void AddStubs(ICollection<Stub> xStubs, ICollection<Stub> yStubs)
            {
                xStubs.Add(xBegin);
                xStubs.Add(xEnd);

                yStubs.Add(yBegin);
                yStubs.Add(yEnd);
            }

            public void Update()
            {
                AABB rect = geom.AABB;
                //if it is a single point in space
                //then dont even add it to the link list.
                shouldAddNode = rect.Min.X != rect.Max.X || rect.Min.Y != rect.Max.Y;

                xBegin.value = rect.Min.X;
                xEnd.value = rect.Max.X;

                yBegin.value = rect.Min.Y;
                yEnd.value = rect.Max.Y;
            }

            public void SetX()
            {
                min = xBegin.value;
                max = xEnd.value;
            }

            public void SetY()
            {
                min = yBegin.value;
                max = yEnd.value;
            }
        }

        #endregion
    }
}