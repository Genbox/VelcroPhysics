using System;
using System.Collections.Generic;
using System.Text;

using FarseerGames.FarseerPhysics.Dynamics;
#if (SILVERLIGHT)
//using FarseerGames.FarseerPhysics.Collections.Generic;
#endif

namespace FarseerGames.FarseerPhysics.Collisions
{
    public sealed class SelectiveSweepCollider : IBroadPhaseCollider
    {
        sealed class StubComparer : IComparer<Stub>
        {
            public int Compare(Stub left, Stub right)
            {
                if (left.value < right.value) { return -1; }
                if (left.value > right.value) { return 1; }
                return ((left == right) ? (0) : ((left.begin) ? (-1) : (1)));
            }
        }
        sealed class Wrapper
        {
            public LinkedListNode<Wrapper> node;
            public Geom geom;
            public bool shouldAddNode;
            public float min;
            public float max;
            Stub xBegin;
            Stub xEnd;
            Stub yBegin;
            Stub yEnd;
            public Wrapper(Geom body)
            {
                this.geom = body;
                this.node = new LinkedListNode<Wrapper>(this);
                xBegin = new Stub(this, true);
                xEnd = new Stub(this, false);
                yBegin = new Stub(this, true);
                yEnd = new Stub(this, false);
            }
            public void AddStubs(List<Stub> xStubs, List<Stub> yStubs)
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
                this.min = this.xBegin.value;
                this.max = this.xEnd.value;
            }
            public void SetY()
            {
                this.min = this.yBegin.value;
                this.max = this.yEnd.value;
            }
        }
        sealed class Stub
        {
            public Wrapper wrapper;
            public bool begin;
            public float value;
            public Stub(Wrapper wrapper, bool begin)
            {
                this.wrapper = wrapper;
                this.begin = begin;
            }
        }

        static StubComparer comparer = new StubComparer();

        PhysicsSimulator physicsSimulator;
        List<Wrapper> wrappers;
        List<Stub> xStubs;
        List<Stub> yStubs;

        public SelectiveSweepCollider(PhysicsSimulator physicsSimulator)
        {
            this.physicsSimulator = physicsSimulator;
            this.wrappers = new List<Wrapper>();
            this.xStubs = new List<Stub>();
            this.yStubs = new List<Stub>();
        }
        public void Add(Geom item)
        {
            Wrapper wrapper = new Wrapper(item);
            wrappers.Add(wrapper);
            wrapper.AddStubs(xStubs, yStubs);
        }
        public void Clear()
        {
            wrappers.Clear();
            xStubs.Clear();
            yStubs.Clear();
        }

        static bool WrapperIsDisposed(Wrapper wrapper)
        {
            return wrapper.geom.IsDisposed;
        }
        static bool StubIsDisposed(Stub stub)
        {
            return stub.wrapper.geom.IsDisposed;
        }
        static bool WrapperIsRemoved(Wrapper wrapper)
        {
            return wrapper.geom.isRemoved;
        }
        static bool StubIsRemoved(Stub stub)
        {
            return stub.wrapper.geom.isRemoved;
        }
#if (!SILVERLIGHT)
        public void ProcessRemovedGeoms()
        {
            if (wrappers.RemoveAll(WrapperIsRemoved) > 0)
            {
                xStubs.RemoveAll(StubIsRemoved);
                yStubs.RemoveAll(StubIsRemoved);
            }
        }
        public void ProcessDisposedGeoms()
        {
            if (wrappers.RemoveAll(WrapperIsDisposed) > 0)
            {
                xStubs.RemoveAll(StubIsDisposed);
                yStubs.RemoveAll(StubIsDisposed);
            }
        }
#else
        public void ProcessRemovedGeoms()
        {
            bool match = false;
            for (int i = 0; i < wrappers.Count; i++)
            {
                if (WrapperIsRemoved(wrappers[i]))
                {
                    match = true;
                    wrappers.RemoveAt(i);
                    i--;
                }
            }

            if (match)
            {
                for (int j = 0; j < xStubs.Count; j++)
                {
                    if (StubIsRemoved(xStubs[j]))
                    {
                        xStubs.RemoveAt(j);
                        j--;
                    }
                }
                for (int j = 0; j < yStubs.Count; j++)
                {
                    if (StubIsRemoved(yStubs[j]))
                    {
                        yStubs.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        public void ProcessDisposedGeoms()
        {
            bool match = false;
            for (int i = 0; i < wrappers.Count; i++)
            {
                if (WrapperIsDisposed(wrappers[i]))
                {
                    match = true;
                    wrappers.RemoveAt(i);
                    i--;
                }
            }

            if (match)
            {
                for (int j = 0; j < xStubs.Count; j++)
                {
                    if (StubIsDisposed(xStubs[j]))
                    {
                        xStubs.RemoveAt(j);
                        j--;
                    }
                }
                for (int j = 0; j < yStubs.Count; j++)
                {
                    if (StubIsDisposed(yStubs[j]))
                    {
                        yStubs.RemoveAt(j);
                        j--;
                    }
                }
            }
        }
#endif


        /// <summary>
        /// updates all the nodes to their new values and sorts the lists
        /// </summary>
        public void InternalUpdate()
        {
            for (int index = 0; index < wrappers.Count; ++index)
            {
                wrappers[index].Update();
            }
            xStubs.Sort(comparer);
            yStubs.Sort(comparer);
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
            for (int index = 0; index < xStubs.Count; index++)
            {
                if (xStubs[index].begin) { xCount += xdepth++; }
                else { xdepth--; }

                if (yStubs[index].begin) { yCount += ydepth++; }
                else { ydepth--; }
            }
            return xCount < yCount;
        }

        public void Update()
        {
            InternalUpdate();
            DetectInternal(ShouldDoX());
        }
        private void DetectInternal(bool doX)
        {
            List<Stub> stubs = (doX) ? (xStubs) : (yStubs);
            LinkedList<Wrapper> currentBodies = new LinkedList<Wrapper>();
            for (int index = 0; index < stubs.Count; index++)
            {
                Stub stub = stubs[index];
                Wrapper wrapper1 = stub.wrapper;
                if (stub.begin)
                {
                    //set the min and max values
                    if (doX) { wrapper1.SetY(); }
                    else { wrapper1.SetX(); }

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
            if (!geom1.body.enabled || !geom2.body.enabled)
            {
                return;
            }

            if ((geom1.collisionGroup == geom2.collisionGroup) &&
                geom1.collisionGroup != 0 &&
                geom2.collisionGroup != 0)
            {
                return;
            }

            if (!geom1.collisionEnabled || !geom2.collisionEnabled)
            {
                return;
            }

            if (geom1.body.isStatic && geom2.body.isStatic)
            { //don't collide two static bodies
                return;
            }

            if (((geom1.collisionCategories & geom2.collidesWith) == Enums.CollisionCategories.None) & ((geom2.collisionCategories & geom1.collidesWith) == Enums.CollisionCategories.None))
            {
                return;
            }

            Arbiter arbiter = physicsSimulator.arbiterPool.Fetch();
            arbiter.ConstructArbiter(geom1, geom2, physicsSimulator);

            if (!physicsSimulator.arbiterList.Contains(arbiter))
            {
                physicsSimulator.arbiterList.Add(arbiter);
            }
            else
            {
                physicsSimulator.arbiterPool.Release(arbiter);
            }
        }

    }

}
