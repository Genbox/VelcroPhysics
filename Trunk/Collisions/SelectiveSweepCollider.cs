using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Interfaces;

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <summary>
    /// A broad phase collider that uses the Selective Sweep algorithm
    /// </summary>
    public sealed class SelectiveSweepCollider : IBroadPhaseCollider
    {
        private static StubComparer _comparer = new StubComparer();

        private PhysicsSimulator _physicsSimulator;
        private List<Wrapper> _wrappers;
        private List<Stub> _xStubs;
        private List<Stub> _yStubs;
        private LinkedList<Wrapper> _currentBodies = new LinkedList<Wrapper>();
        public event BroadPhaseCollisionHandler OnBroadPhaseCollision;

        public SelectiveSweepCollider(PhysicsSimulator physicsSimulator)
        {
            _physicsSimulator = physicsSimulator;
            _wrappers = new List<Wrapper>();
            _xStubs = new List<Stub>();
            _yStubs = new List<Stub>();
        }

        #region IBroadPhaseCollider Members

        public void Add(Geom geom)
        {
            Wrapper wrapper = new Wrapper(geom);
            _wrappers.Add(wrapper);
            wrapper.AddStubs(_xStubs, _yStubs);
        }

#if (!SILVERLIGHT)
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
#else
        public void ProcessRemovedGeoms()
        {
            bool match = false;
            for (int i = 0; i < _wrappers.Count; i++)
            {
                if (WrapperIsRemoved(_wrappers[i]))
                {
                    match = true;
                    _wrappers.RemoveAt(i);
                    i--;
                }
            }

            if (match)
            {
                for (int j = 0; j < _xStubs.Count; j++)
                {
                    if (StubIsRemoved(_xStubs[j]))
                    {
                        _xStubs.RemoveAt(j);
                        j--;
                    }
                }
                for (int j = 0; j < _yStubs.Count; j++)
                {
                    if (StubIsRemoved(_yStubs[j]))
                    {
                        _yStubs.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        public void ProcessDisposedGeoms()
        {
            bool match = false;
            for (int i = 0; i < _wrappers.Count; i++)
            {
                if (WrapperIsDisposed(_wrappers[i]))
                {
                    match = true;
                    _wrappers.RemoveAt(i);
                    i--;
                }
            }

            if (match)
            {
                for (int j = 0; j < _xStubs.Count; j++)
                {
                    if (StubIsDisposed(_xStubs[j]))
                    {
                        _xStubs.RemoveAt(j);
                        j--;
                    }
                }
                for (int j = 0; j < _yStubs.Count; j++)
                {
                    if (StubIsDisposed(_yStubs[j]))
                    {
                        _yStubs.RemoveAt(j);
                        j--;
                    }
                }
            }
        }
#endif

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
            return wrapper.geom.isRemoved;
        }

        private static bool StubIsRemoved(Stub stub)
        {
            return stub.wrapper.geom.isRemoved;
        }

        /// <summary>
        /// Updates all the nodes to their new values and sorts the lists
        /// </summary>
        public void InternalUpdate()
        {
            for (int index = 0; index < _wrappers.Count; ++index)
            {
                _wrappers[index].Update();
            }
            _xStubs.Sort(_comparer);
            _yStubs.Sort(_comparer);
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
            _currentBodies.Clear();
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
                    for (LinkedListNode<Wrapper> node = _currentBodies.First;
                         node != null;
                         node = node.Next)
                    {
                        Wrapper wrapper2 = node.Value;
                        Geom geom2 = wrapper2.geom;
                        if (wrapper1.min <= wrapper2.max && //tests the other axis
                            wrapper2.min <= wrapper1.max)
                        {
                            //Call the OnBroadPhaseCollision event first. If the user aborts the collision
                            //it will not create an arbiter
                            if (OnBroadPhaseCollision != null)
                            {
                                if (OnBroadPhaseCollision(geom1, geom2))
                                    OnCollision(geom1, geom2);
                            }
                            else
                            {
                                OnCollision(geom1, geom2);
                            }
                        }
                    }
                    if (wrapper1.shouldAddNode)
                    {
                        _currentBodies.AddLast(wrapper1.node);
                    }
                }
                else
                {
                    if (wrapper1.shouldAddNode)
                    {
                        _currentBodies.Remove(wrapper1.node);
                    }
                }
            }
        }

        private void OnCollision(Geom geom1, Geom geom2)
        {
            if (!geom1.body.enabled || !geom2.body.enabled)
                return;

            if ((geom1.collisionGroup == geom2.collisionGroup) &&
                geom1.collisionGroup != 0 && geom2.collisionGroup != 0)
                return;

            if (!geom1.collisionEnabled || !geom2.collisionEnabled)
                return;

            if (geom1.body.isStatic && geom2.body.isStatic)
                return;

            if (geom1.body == geom2.body)
                return;

            if (((geom1.collisionCategories & geom2.collidesWith) ==
                 CollisionCategory.None) & ((geom2.collisionCategories &
                                               geom1.collidesWith) == CollisionCategory.None))
                return;

            Arbiter arbiter = _physicsSimulator.arbiterPool.Fetch();
            arbiter.ConstructArbiter(geom1, geom2, _physicsSimulator);
            
            //TODO: Since we insert all arbiters that is already in the arbiterList into the pool
            //should we not restrict the size of the pool to a fixed number? A large simulation
            //that runs for some time might accumulate A LOT of arbiters in the pool.
            if (!_physicsSimulator.arbiterList.Contains(arbiter))
                _physicsSimulator.arbiterList.Add(arbiter);
            else
                _physicsSimulator.arbiterPool.Insert(arbiter);
        }

        #region Nested type: Stub

        private sealed class Stub
        {
            public bool begin;
            public float value;
            public Wrapper wrapper;

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
            private Stub _xBegin;
            private Stub _xEnd;
            private Stub _yBegin;
            private Stub _yEnd;
            public Geom geom;
            public float max;
            public float min;
            public LinkedListNode<Wrapper> node;
            public bool shouldAddNode;

            public Wrapper(Geom body)
            {
                geom = body;
                node = new LinkedListNode<Wrapper>(this);
                _xBegin = new Stub(this, true);
                _xEnd = new Stub(this, false);
                _yBegin = new Stub(this, true);
                _yEnd = new Stub(this, false);
            }

            public void AddStubs(List<Stub> xStubs, List<Stub> yStubs)
            {
                xStubs.Add(_xBegin);
                xStubs.Add(_xEnd);

                yStubs.Add(_yBegin);
                yStubs.Add(_yEnd);
            }

            public void Update()
            {
                AABB rect = geom.AABB;
                //if it is a single point in space
                //then dont even add it to the link list.
                shouldAddNode = rect.Min.X != rect.Max.X || rect.Min.Y != rect.Max.Y;

                _xBegin.value = rect.Min.X;
                _xEnd.value = rect.Max.X;

                _yBegin.value = rect.Min.Y;
                _yEnd.value = rect.Max.Y;
            }

            public void SetX()
            {
                min = _xBegin.value;
                max = _xEnd.value;
            }

            public void SetY()
            {
                min = _yBegin.value;
                max = _yEnd.value;
            }
        }

        #endregion
    }
}