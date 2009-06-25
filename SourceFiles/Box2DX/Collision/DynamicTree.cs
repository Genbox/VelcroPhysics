using System;
using Box2DX.Common;
using Math = Box2DX.Common.Math;

namespace Box2DX.Collision
{
    ///<summary>
    /// A node in the dynamic tree. The client does not interact with this directly.
    /// 4 + 8 + 6 = 18 bytes on a 32bit machine.
    /// </summary>
    public struct DynamicTreeNode
    {
        public bool IsLeaf()
        {
            return Child1 == DynamicTree.NullNode;
        }

        public object UserData;
        public AABB Aabb;
        public ushort Parent;
        public ushort Child1;
        public ushort Child2;
    }
    /// <summary>
    /// A callback for AABB queries.
    /// </summary>
    public abstract class QueryCallback
    {
        /// This function is called for each overlapping AABB.
        /// @return true if the query should continue.
        public abstract bool Process(object userData);
    }

    /// <summary>
    /// A callback for ray casts.
    /// </summary>
    public abstract class RayCastCallback
    {
        /// Process a ray-cast. This allows the client to perform an exact ray-cast
        /// against their object (found from the proxyUserData pointer).
        /// @param input the original ray-cast segment with an adjusted maxFraction.
        /// @param maxFraction the clipping parameter, the ray extends from p1 to p1 + maxFraction * (p2 - p1).
        /// @param userData user data associated with the current proxy.
        /// @return the new max fraction. Return 0 to end the ray-cast. Return the input maxFraction to
        /// continue the ray cast. Return a value less than maxFraction to clip the ray-cast.
        public abstract float Process(RayCastInput input, object userData);
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
        public static readonly ushort NullNode = Math.USHRT_MAX;

        /// Constructing the tree initializes the node pool.
        public DynamicTree()
        {
            _root = NullNode;
            _nodeCount = Math.Max(Settings.NodePoolSize, 1);
            _nodes = new DynamicTreeNode[_nodeCount];

            //m_nodes = (b2DynamicTreeNode*)b2Alloc(m_nodeCount * sizeof(b2DynamicTreeNode));
            //memset(m_nodes, 0, m_nodeCount * sizeof(b2DynamicTreeNode));

            // Build a linked list for the free list. The parent
            // pointer becomes the "next" pointer.
            for (int i = 0; i < _nodeCount - 1; ++i)
            {
                _nodes[i].Parent = (ushort)(i + 1);
            }
            _nodes[_nodeCount - 1].Parent = NullNode;
            _freeList = 0;

            _path = 0;
        }

        /// Create a proxy. Provide a tight fitting AABB and a userData pointer.
        public ushort CreateProxy(AABB aabb, object userData)
        {
            ushort node = AllocateNode();

            // Fatten the aabb.
            Vec2 center = aabb.GetCenter();
            Vec2 extents = Settings.FatAABBFactor * aabb.GetExtents();
            _nodes[node].Aabb.LowerBound = center - extents;
            _nodes[node].Aabb.UpperBound = center + extents;
            _nodes[node].UserData = userData;

            InsertLeaf(node);

            return node;
        }

        /// Destroy a proxy. This asserts if the id is invalid.
        public void DestroyProxy(ushort proxyId)
        {
            Box2DXDebug.Assert(proxyId < _nodeCount);
            Box2DXDebug.Assert(_nodes[proxyId].IsLeaf());

            RemoveLeaf(proxyId);
            FreeNode(proxyId);
        }

        /// Move a proxy. If the proxy has moved outside of its fattened AABB,
        /// then the proxy is removed from the tree and re-inserted. Otherwise
        /// the function returns immediately.
        public void MoveProxy(ushort proxyId, AABB aabb)
        {
            Box2DXDebug.Assert(proxyId < _nodeCount);

            Box2DXDebug.Assert(_nodes[proxyId].IsLeaf());

            if (_nodes[proxyId].Aabb.Contains(aabb))
            {
                return;
            }

            RemoveLeaf(proxyId);

            Vec2 center = aabb.GetCenter();
            Vec2 extents = Settings.FatAABBFactor * aabb.GetExtents();

            _nodes[proxyId].Aabb.LowerBound = center - extents;
            _nodes[proxyId].Aabb.UpperBound = center + extents;

            InsertLeaf(proxyId);
        }

        /// Perform some iterations to re-balance the tree.
        public void Rebalance(int iterations)
        {
            if (_root == NullNode)
            {
                return;
            }

            for (int i = 0; i < iterations; ++i)
            {
                ushort node = _root;

                uint bit = 0;
                while (_nodes[node].IsLeaf() == false)
                {
#warning "more pointer-used-as-an-array crap"
                    ushort children = _nodes[node].Child1;
                    node = children[(_path >> bit) & 1];
                    bit = (bit + 1) & (8 * sizeof(uint) - 1);
                }
                ++_path;

                RemoveLeaf(node);
                InsertLeaf(node);
            }
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

        /// Query an AABB for overlapping proxies. The callback class
        /// is called for each proxy that overlaps the supplied AABB.
#warning "C++ callback. Guess we create a delegate in C#?"
        public void Query(T callback, AABB aabb)
        {
            if (_root == null)
            {
                return;
            }

            const int k_stackSize = 32;
            ushort[] stack = new ushort[k_stackSize];

            int count = 0;
            stack[count++] = _root;

            while (count > 0)
            {
                DynamicTreeNode node = _nodes + stack[--count];

                if (Collision.TestOverlap(node.Aabb, aabb))
                {
                    if (node.IsLeaf())
                    {
                        callback.QueryCallback(aabb, node.UserData);
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
#warning "C++ callback. Guess we create a delegate in C#?"
        public void RayCast(T callback, RayCastInput input)
        {
            if (_root == null)
            {
                return;
            }

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
            AABB segmentAABB;
            {
                Vec2 t = p1 + maxFraction * (p2 - p1);
                segmentAABB.LowerBound = Math.Min(p1, t);
                segmentAABB.UpperBound = Math.Max(p1, t);
            }

            const int k_stackSize = 32;
            ushort[] stack = new ushort[k_stackSize];

            int count = 0;
            stack[count++] = _root;

            while (count > 0)
            {
                DynamicTreeNode node = _nodes + stack[--count];

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

                    RayCastOutput output;

                    callback.RayCastCallback(out output, subInput, node.UserData);

                    if (output.Hit)
                    {
                        // Early exit.
                        if (output.Fraction == 0.0f)
                        {
                            return;
                        }

                        maxFraction = output.Fraction;

                        // Update segment bounding box.
                        {
                            Vec2 t = p1 + maxFraction * (p2 - p1);
                            segmentAABB.LowerBound = Math.Min(p1, t);
                            segmentAABB.UpperBound = Math.Max(p1, t);
                        }
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

        private ushort AllocateNode()
        {
            ushort node;

            // Peel a node off the free list.
            if (_freeList != NullNode)
            {
                node = _freeList;
                _freeList = _nodes[node].Parent;
                _nodes[node].Parent = NullNode;
                _nodes[node].Child1 = NullNode;
                _nodes[node].Child2 = NullNode;
                return node;
            }

            // The free list is empty. Rebuild a bigger pool.
            int newPoolCount = Math.Min(2 * _nodeCount, Math.USHRT_MAX - 1);
            Box2DXDebug.Assert(newPoolCount > _nodeCount);
            DynamicTreeNode[] newPool = new DynamicTreeNode[newPoolCount];
            //b2DynamicTreeNode* newPool = (b2DynamicTreeNode*)b2Alloc(newPoolCount * sizeof(b2DynamicTreeNode));
            //memcpy(newPool, m_nodes, m_nodeCount * sizeof(b2DynamicTreeNode));
            //memset(newPool + m_nodeCount, 0, (newPoolCount - m_nodeCount) * sizeof(b2DynamicTreeNode));

            // Build a linked list for the free list. The parent
            // pointer becomes the "next" pointer.
            for (int i = _nodeCount; i < newPoolCount - 1; ++i)
            {
                newPool[i].Parent = (ushort)(i + 1);
            }
            newPool[newPoolCount - 1].Parent = NullNode;
            _freeList = (ushort)_nodeCount;

            _nodes = null;
            _nodes = newPool;
            _nodeCount = newPoolCount;

            // Finally peel a node off the new free list.
            node = _freeList;
            _freeList = _nodes[node].Parent;
            return node;
        }
        private void FreeNode(ushort node)
        {
            Box2DXDebug.Assert(node < Math.USHRT_MAX);
            _nodes[node].Parent = _freeList;
            _freeList = node;
        }

        private void InsertLeaf(ushort leaf)
        {
            if (_root == NullNode)
            {
                _root = leaf;
                _nodes[_root].Parent = NullNode;
                return;
            }

            // Find the best sibling for this node.
            Vec2 center = _nodes[leaf].Aabb.GetCenter();
            ushort sibling = _root;
            if (_nodes[sibling].IsLeaf() == false)
            {
                do
                {
                    ushort child1 = _nodes[sibling].Child1;
                    ushort child2 = _nodes[sibling].Child2;

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
            ushort node1 = _nodes[sibling].Parent;
            ushort node2 = AllocateNode();
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

        private void RemoveLeaf(ushort leaf)
        {
            if (leaf == _root)
            {
                _root = NullNode;
                return;
            }

            ushort node2 = _nodes[leaf].Parent;
            ushort node1 = _nodes[node2].Parent;
            ushort sibling;
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

        private ushort _root;

        private DynamicTreeNode[] _nodes;
        private int _nodeCount;

        private ushort _freeList;

        /// This is used incrementally traverse the tree for re-balancing.
        private ushort _path;

        #region IDisposable Members

        public void Dispose()
        {
            _nodes = null;
        }

        #endregion
    }
}
