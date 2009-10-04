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
        private LinkedList<Wrapper> _currentBodies = new LinkedList<Wrapper>();

        private PhysicsSimulator _physicsSimulator;
        private List<Wrapper> _wrappers;
        private List<Stub> _xStubs;
        private List<Stub> _yStubs;

        public SelectiveSweepCollider(PhysicsSimulator physicsSimulator)
        {
            _physicsSimulator = physicsSimulator;
            _wrappers = new List<Wrapper>();
            _xStubs = new List<Stub>();
            _yStubs = new List<Stub>();
        }

        #region IBroadPhaseCollider Members

        /// <summary>
        /// Fires when a broad phase collision occurs
        /// </summary>
        public event BroadPhaseCollisionHandler OnBroadPhaseCollision;

        /// <summary>
        /// Adds the specified geom.
        /// </summary>
        /// <param name="geom">The geom.</param>
        public void Add(Geom geom)
        {
            Wrapper wrapper = new Wrapper(geom);
            _wrappers.Add(wrapper);
            wrapper.AddStubs(_xStubs, _yStubs);
        }

#if (!SILVERLIGHT)
        /// <summary>
        /// Processes the removed geoms.
        /// </summary>
        public void ProcessRemovedGeoms()
        {
            if (_wrappers.RemoveAll(WrapperIsRemoved) > 0)
            {
                _xStubs.RemoveAll(StubIsRemoved);
                _yStubs.RemoveAll(StubIsRemoved);
            }
        }

        /// <summary>
        /// Processes the disposed geoms.
        /// </summary>
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

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public void Update()
        {
            InternalUpdate();
            DetectInternal(ShouldDoX());
        }

        #endregion

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _wrappers.Clear();
            _xStubs.Clear();
            _yStubs.Clear();
        }

        private static bool WrapperIsDisposed(Wrapper wrapper)
        {
            return wrapper.Geom.IsDisposed;
        }

        private static bool StubIsDisposed(Stub stub)
        {
            return stub.Wrapper.Geom.IsDisposed;
        }

        private static bool WrapperIsRemoved(Wrapper wrapper)
        {
            return !wrapper.Geom.InSimulation;
        }

        private static bool StubIsRemoved(Stub stub)
        {
            return !stub.Wrapper.Geom.InSimulation;
        }

        /// <summary>
        /// Updates all the nodes to their new values and sorts the lists
        /// </summary>
        private void InternalUpdate()
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
            int xDepth = 0;
            int yCount = 0;
            int yDepth = 0;
            for (int index = 0; index < _xStubs.Count; index++)
            {
                if (_xStubs[index].Begin)
                {
                    xCount += xDepth++;
                }
                else
                {
                    xDepth--;
                }

                if (_yStubs[index].Begin)
                {
                    yCount += yDepth++;
                }
                else
                {
                    yDepth--;
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
                Wrapper wrapper1 = stub.Wrapper;
                if (stub.Begin)
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

                    Geom geometryA = wrapper1.Geom;

                    for (LinkedListNode<Wrapper> node = _currentBodies.First; node != null; node = node.Next)
                    {
                        Wrapper wrapper2 = node.Value;
                        Geom geometryB = wrapper2.Geom;

                        if (wrapper1.Min <= wrapper2.Max && wrapper2.Min <= wrapper1.Max)
                        {
                            if (!geometryA.body.Enabled || !geometryB.body.Enabled)
                                continue;

                            if ((geometryA.CollisionGroup == geometryB.CollisionGroup) &&
                                geometryA.CollisionGroup != 0 && geometryB.CollisionGroup != 0)
                                continue;

                            if (!geometryA.CollisionEnabled || !geometryB.CollisionEnabled)
                                continue;

                            if (geometryA.body.isStatic && geometryB.body.isStatic)
                                continue;

                            if (geometryA.body == geometryB.body)
                                continue;

                            if (((geometryA.CollisionCategories & geometryB.CollidesWith) == CollisionCategory.None) &
                                ((geometryB.CollisionCategories & geometryA.CollidesWith) == CollisionCategory.None))
                                continue;

                            if (geometryA.IsGeometryIgnored(geometryB) || geometryB.IsGeometryIgnored(geometryA))
                            {
                                continue;
                            }

                            bool intersection = true;

                            //Call the OnBroadPhaseCollision event first. If the user aborts the collision
                            //it will not create an arbiter
                            if (OnBroadPhaseCollision != null)
                                intersection = OnBroadPhaseCollision(geometryA, geometryB);

                            if (!intersection)
                                continue;

                            _physicsSimulator.ArbiterList.AddArbiterForGeomPair(_physicsSimulator, geometryA, geometryB);
                        }
                    }
                    if (wrapper1.ShouldAddNode)
                    {
                        _currentBodies.AddLast(wrapper1.Node);
                    }
                }
                else
                {
                    if (wrapper1.ShouldAddNode)
                    {
                        _currentBodies.Remove(wrapper1.Node);
                    }
                }
            }
        }

        #region Nested type: Stub

        private sealed class Stub
        {
            public bool Begin;
            public float Value;
            public Wrapper Wrapper;

            public Stub(Wrapper wrapper, bool begin)
            {
                Wrapper = wrapper;
                Begin = begin;
            }
        }

        #endregion

        #region Nested type: StubComparer

        private sealed class StubComparer : IComparer<Stub>
        {
            #region IComparer<Stub> Members

            public int Compare(Stub left, Stub right)
            {
                if (left.Value < right.Value)
                {
                    return -1;
                }
                if (left.Value > right.Value)
                {
                    return 1;
                }
                return ((left == right) ? (0) : ((left.Begin) ? (-1) : (1)));
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
            public Geom Geom;
            public float Max;
            public float Min;
            public LinkedListNode<Wrapper> Node;
            public bool ShouldAddNode;

            public Wrapper(Geom body)
            {
                Geom = body;
                Node = new LinkedListNode<Wrapper>(this);
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
                AABB rect = Geom.AABB;
                //if it is a single point in space
                //then dont even add it to the link list.
                ShouldAddNode = rect.Min.X != rect.Max.X || rect.Min.Y != rect.Max.Y;

                _xBegin.Value = rect.Min.X;
                _xEnd.Value = rect.Max.X;

                _yBegin.Value = rect.Min.Y;
                _yEnd.Value = rect.Max.Y;
            }

            public void SetX()
            {
                Min = _xBegin.Value;
                Max = _xEnd.Value;
            }

            public void SetY()
            {
                Min = _yBegin.Value;
                Max = _yEnd.Value;
            }
        }

        #endregion
    }
}