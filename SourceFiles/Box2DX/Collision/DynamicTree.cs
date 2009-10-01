using System;
using Box2DX.Common;
using Box2DX.Stuff;
using Math = Box2DX.Common.Math;

namespace Box2DX.Collision
{
    ///<summary>
    /// A node in the dynamic tree. The client does not interact with this directly.
    /// 4 + 8 + 6 = 18 bytes on a 32bit machine.
    /// </summary>
    public class DynamicTreeNode
    {
        public bool IsLeaf()
        {
            return Child1 == DynamicTree.NullNode;
        }

        public object UserData;
        public AABB Aabb;
        public int Parent;
        public int Next;
        public int Child1;
        public int Child2;
    }

    ///<summary>
    /// A dynamic tree arranges data in a binary tree to accelerate
    /// queries such as volume queries and ray casts. Leafs are proxies
    /// with an AABB. In the tree we expand the proxy AABB by b2_fatAABBFactor
    /// so that the proxy AABB is bigger than the client object. This allows the client
    /// object to move by small amounts without triggering a tree update.
    ///
    /// Nodes are pooled and relocatable, so we use node indices rather than pointers.
    /// </summary>
    public class DynamicTree : IDisposable
    {
        public const int NullNode = (-1);

        /// Constructing the tree initializes the node pool.
        public DynamicTree()
        {
            _root = NullNode;

            _nodeCapacity = 16;
            _nodeCount = 0;
            _nodes = new DynamicTreeNode[_nodeCapacity];

            //Fill the array with nodes:
            for (int i = 0; i < _nodeCapacity; ++i)
            {
                _nodes[i] = new DynamicTreeNode();
            }

            // Build a linked list for the free list. The parent
            // pointer becomes the "next" pointer.
            for (int i = 0; i < _nodeCapacity - 1; ++i)
            {
                _nodes[i].Next = i + 1;
            }

            _nodes[_nodeCapacity - 1].Next = NullNode;
            _freeList = 0;

            _path = 0;

            _insertionCount = 0;
        }

        /// Create a proxy. Provide a tight fitting AABB and a userData pointer.
        public int CreateProxy(AABB aabb, object userData)
        {
            int proxyId = AllocateNode();

            // Fatten the aabb.
            Vec2 r = new Vec2(Settings.AABBExtension, Settings.AABBExtension);
            _nodes[proxyId].Aabb.LowerBound = aabb.LowerBound - r;
            _nodes[proxyId].Aabb.UpperBound = aabb.UpperBound + r;
            _nodes[proxyId].UserData = userData;

            InsertLeaf(proxyId);

            return proxyId;
        }

        /// Destroy a proxy. This asserts if the id is invalid.
        public void DestroyProxy(int proxyId)
        {
            Box2DXDebug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            Box2DXDebug.Assert(_nodes[proxyId].IsLeaf());

            RemoveLeaf(proxyId);
            FreeNode(proxyId);
        }

        /// Move a proxy with a swepted AABB. If the proxy has moved outside of its fattened AABB,
        /// then the proxy is removed from the tree and re-inserted. Otherwise
        /// the function returns immediately.
        /// @return true if the proxy was re-inserted.
        public bool MoveProxy(int proxyId, AABB aabb, Vec2 displacement)
        {
            Box2DXDebug.Assert(0 <= proxyId && proxyId < _nodeCapacity);

            Box2DXDebug.Assert(_nodes[proxyId].IsLeaf());

            if (_nodes[proxyId].Aabb.Contains(aabb))
            {
                return false;
            }

            RemoveLeaf(proxyId);

            // Extend AABB.
            AABB b = aabb;
            Vec2 r = new Vec2(Settings.AABBExtension, Settings.AABBExtension);
            b.LowerBound = b.LowerBound - r;
            b.UpperBound = b.UpperBound + r;

            // Predict AABB displacement.
            Vec2 d = Settings.AABBMultiplier * displacement;

            if (d.X < 0.0f)
            {
                b.LowerBound.X += d.X;
            }
            else
            {
                b.UpperBound.X += d.X;
            }

            if (d.Y < 0.0f)
            {
                b.LowerBound.Y += d.Y;
            }
            else
            {
                b.UpperBound.Y += d.Y;
            }

            _nodes[proxyId].Aabb = b;

            InsertLeaf(proxyId);
            return true;
        }

        /// Get proxy user data.
        /// @return the proxy user data or NULL if the id is invalid.
        public object GetProxy(ushort proxyId)
        {
            if (proxyId < _nodeCount)
            {
                return _nodes[proxyId].UserData;
            }
            else
            {
                return null;
            }
        }

        public object GetUserData(int proxyId)
        {
            Box2DXDebug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            return _nodes[proxyId].UserData;
        }

        public AABB GetFatAABB(int proxyId)
        {
            Box2DXDebug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            return _nodes[proxyId].Aabb;
        }

        public void InsertLeaf(int leaf)
        {
            ++_insertionCount;

            if (_root == NullNode)
            {
                _root = leaf;
                _nodes[_root].Parent = NullNode;
                return;
            }

            // Find the best sibling for this node.
            Vec2 center = _nodes[leaf].Aabb.GetCenter();
            int sibling = _root;
            if (_nodes[sibling].IsLeaf() == false)
            {
                do
                {
                    int child1 = _nodes[sibling].Child1;
                    int child2 = _nodes[sibling].Child2;

                    Vec2 delta1 = Math.Abs(_nodes[child1].Aabb.GetCenter() - center);
                    Vec2 delta2 = Math.Abs(_nodes[child2].Aabb.GetCenter() - center);

                    float norm1 = delta1.X + delta1.Y;
                    float norm2 = delta2.X + delta2.Y;

                    if (norm1 < norm2)
                    {
                        sibling = child1;
                    }
                    else
                    {
                        sibling = child2;
                    }

                }
                while (_nodes[sibling].IsLeaf() == false);
            }

            // Create a parent for the siblings.
            int node1 = _nodes[sibling].Parent;
            int node2 = AllocateNode();
            _nodes[node2].Parent = node1;
            _nodes[node2].UserData = null;
            _nodes[node2].Aabb.Combine(_nodes[leaf].Aabb, _nodes[sibling].Aabb);

            if (node1 != NullNode)
            {
                if (_nodes[_nodes[sibling].Parent].Child1 == sibling)
                {
                    _nodes[node1].Child1 = node2;
                }
                else
                {
                    _nodes[node1].Child2 = node2;
                }

                _nodes[node2].Child1 = sibling;
                _nodes[node2].Child2 = leaf;
                _nodes[sibling].Parent = node2;
                _nodes[leaf].Parent = node2;

                do
                {
                    if (_nodes[node1].Aabb.Contains(_nodes[node2].Aabb))
                    {
                        break;
                    }

                    _nodes[node1].Aabb.Combine(_nodes[_nodes[node1].Child1].Aabb, _nodes[_nodes[node1].Child2].Aabb);
                    node2 = node1;
                    node1 = _nodes[node1].Parent;
                }
                while (node1 != NullNode);
            }
            else
            {
                _nodes[node2].Child1 = sibling;
                _nodes[node2].Child2 = leaf;
                _nodes[sibling].Parent = node2;
                _nodes[leaf].Parent = node2;
                _root = node2;
            }
        }

        public void RemoveLeaf(int leaf)
        {
            if (leaf == _root)
            {
                _root = NullNode;
                return;
            }

            int node2 = _nodes[leaf].Parent;
            int node1 = _nodes[node2].Parent;
            int sibling;
            if (_nodes[node2].Child1 == leaf)
            {
                sibling = _nodes[node2].Child2;
            }
            else
            {
                sibling = _nodes[node2].Child1;
            }

            if (node1 != NullNode)
            {
                // Destroy node2 and connect node1 to sibling.
                if (_nodes[node1].Child1 == node2)
                {
                    _nodes[node1].Child1 = sibling;
                }
                else
                {
                    _nodes[node1].Child2 = sibling;
                }
                _nodes[sibling].Parent = node1;
                FreeNode(node2);

                // Adjust ancestor bounds.
                while (node1 != NullNode)
                {
                    AABB oldAABB = _nodes[node1].Aabb;
                    _nodes[node1].Aabb.Combine(_nodes[_nodes[node1].Child1].Aabb, _nodes[_nodes[node1].Child2].Aabb);

                    if (oldAABB.Contains(_nodes[node1].Aabb))
                    {
                        break;
                    }

                    node1 = _nodes[node1].Parent;
                }
            }
            else
            {
                _root = sibling;
                _nodes[sibling].Parent = NullNode;
                FreeNode(node2);
            }
        }

        //TODO: uncomment
        //public void Rebalance(int iterations)
        //{
        //    if (_root == NullNode)
        //    {
        //        return;
        //    }

        //    for (int i = 0; i < iterations; ++i)
        //    {
        //        int node = _root;

        //        uint bit = 0;
        //        while (_nodes[node].IsLeaf() == false)
        //        {
        //            int children = _nodes[node].Child1;
        //            node = children[(_path >> bit) & 1];
        //            bit = (bit + 1) & (8 * sizeof(uint) - 1);
        //        }
        //        ++_path;

        //        RemoveLeaf(node);
        //        InsertLeaf(node);
        //    }
        //}

        // Compute the height of a sub-tree.
        public int ComputeHeight(int nodeId)
        {
            if (nodeId == NullNode)
            {
                return 0;
            }

            Box2DXDebug.Assert(0 <= nodeId && nodeId < _nodeCapacity);
            DynamicTreeNode node = _nodes[nodeId];
            int height1 = ComputeHeight(node.Child1);
            int height2 = ComputeHeight(node.Child2);
            return 1 + Math.Max(height1, height2);
        }

        public int ComputeHeight()
        {
            return ComputeHeight(_root);
        }

        /// Query an AABB for overlapping proxies. The callback class
        /// is called for each proxy that overlaps the supplied AABB.
        public void Query(IQueryEnabled callback, AABB aabb)
        {
            const int k_stackSize = 128;
            int[] stack = new int[k_stackSize];

            int count = 0;
            stack[count++] = _root;

            while (count > 0)
            {
                int nodeId = stack[--count];
                if (nodeId == NullNode)
                {
                    continue;
                }

                DynamicTreeNode node = _nodes[nodeId];

                if (Collision.TestOverlap(node.Aabb, aabb))
                {
                    if (node.IsLeaf())
                    {
                        bool proceed = callback.QueryCallback(nodeId);
                        if (proceed == false)
                        {
                            return;
                        }
                    }
                    else
                    {
                        Box2DXDebug.Assert(count + 1 < k_stackSize);
                        stack[count++] = node.Child1;
                        stack[count++] = node.Child2;
                    }
                }
            }
        }

        /// Ray-cast against the proxies in the tree. This relies on the callback
        /// to perform a exact ray-cast in the case were the proxy contains a shape.
        /// The callback also performs the any collision filtering. This has performance
        /// roughly equal to k * log(n), where k is the number of collisions and n is the
        /// number of proxies in the tree.
        /// @param input the ray-cast input data. The ray extends from p1 to p1 + maxFraction * (p2 - p1).
        /// @param callback a callback class that is called for each proxy that is hit by the ray.
        public void RayCast(IRayCastEnabled callback, RayCastInput input)
        {
            Vec2 p1 = input.P1;
            Vec2 p2 = input.P2;
            Vec2 r = p2 - p1;
            Box2DXDebug.Assert(r.LengthSquared() > 0.0f);
            r.Normalize();

            // v is perpendicular to the segment.
            Vec2 v = Vec2.Cross(1.0f, r);
            Vec2 abs_v = Math.Abs(v);

            // Separating axis for segment (Gino, p80).
            // |dot(v, p1 - c)| > dot(|v|, h)

            float maxFraction = input.MaxFraction;

            // Build a bounding box for the segment.
            AABB segmentAABB = new AABB();
            {
                Vec2 t = p1 + maxFraction * (p2 - p1);
                segmentAABB.LowerBound = Math.Min(p1, t);
                segmentAABB.UpperBound = Math.Max(p1, t);
            }

            const int k_stackSize = 128;
            int[] stack = new int[k_stackSize];

            int count = 0;
            stack[count++] = _root;

            while (count > 0)
            {
                int nodeId = stack[--count];
                if (nodeId == NullNode)
                {
                    continue;
                }

                DynamicTreeNode node = _nodes[nodeId];

                if (Collision.TestOverlap(node.Aabb, segmentAABB) == false)
                {
                    continue;
                }

                // Separating axis for segment (Gino, p80).
                // |dot(v, p1 - c)| > dot(|v|, h)
                Vec2 c = node.Aabb.GetCenter();
                Vec2 h = node.Aabb.GetExtents();
                float separation = Math.Abs(Vec2.Dot(v, p1 - c)) - Vec2.Dot(abs_v, h);
                if (separation > 0.0f)
                {
                    continue;
                }

                if (node.IsLeaf())
                {
                    RayCastInput subInput = new RayCastInput();
                    subInput.P1 = input.P1;
                    subInput.P2 = input.P2;
                    subInput.MaxFraction = maxFraction;

                    maxFraction = callback.RayCastCallback(subInput, nodeId);

                    if (maxFraction == 0.0f)
                    {
                        return;
                    }

                    // Update segment bounding box.
                    {
                        Vec2 t = p1 + maxFraction * (p2 - p1);
                        segmentAABB.LowerBound = Math.Min(p1, t);
                        segmentAABB.UpperBound = Math.Max(p1, t);
                    }
                }
                else
                {
                    Box2DXDebug.Assert(count + 1 < k_stackSize);
                    stack[count++] = node.Child1;
                    stack[count++] = node.Child2;
                }
            }
        }

        private int AllocateNode()
        {
            // Expand the node pool as needed.
            if (_freeList == NullNode)
            {
                Box2DXDebug.Assert(_nodeCount == _nodeCapacity);

                // The free list is empty. Rebuild a bigger pool.
                _nodeCapacity *= 2;
                Array.Resize(ref _nodes, _nodeCapacity);

                //Fill array with nodes
                for (int i = _nodeCount; i < _nodeCapacity; i++)
                {
                    _nodes[i] = new DynamicTreeNode();
                }

                // Build a linked list for the free list. The parent
                // pointer becomes the "next" pointer.
                for (int i = _nodeCount; i < _nodeCapacity - 1; ++i)
                {
                    _nodes[i].Next = i + 1;
                }
                _nodes[_nodeCapacity - 1].Next = NullNode;
                _freeList = _nodeCount;
            }

            // Peel a node off the free list.
            int nodeId = _freeList;
            _freeList = _nodes[nodeId].Next;
            _nodes[nodeId].Parent = NullNode;
            _nodes[nodeId].Child1 = NullNode;
            _nodes[nodeId].Child2 = NullNode;
            ++_nodeCount;
            return nodeId;
        }

        private void FreeNode(int nodeId)
        {
            Box2DXDebug.Assert(0 <= nodeId && nodeId < _nodeCapacity);
            Box2DXDebug.Assert(0 < _nodeCount);
            _nodes[nodeId].Next = _freeList;
            _freeList = nodeId;
            --_nodeCount;
        }

        private int _root;
        private DynamicTreeNode[] _nodes;
        private int _nodeCount;
        private int _nodeCapacity;
        private int _freeList;

        /// This is used incrementally traverse the tree for re-balancing.
        private int _path;

        private int _insertionCount;

        #region IDisposable Members

        public void Dispose()
        {
            _nodes = null;
        }

        #endregion
    }
}
