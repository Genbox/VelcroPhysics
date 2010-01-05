/*
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace FarseerPhysics
{
    /// A dynamic AABB tree broad-phase, inspired by Nathanael Presson's btDbvt.

    public delegate float RayCastCallback(ref RayCastInput input, int userData);

    public delegate float WorldRayCastCallback(Fixture fixture, Vector2 point, Vector2 normal, float fraction);

    /// <summary>
    /// A node in the dynamic tree. The client does not interact with this directly.
    /// </summary>
    internal struct DynamicTreeNode
    {
        internal bool IsLeaf()
        {
            return Child1 == DynamicTree.NullNode;
        }

        /// This is the fattened AABB.
        internal AABB Aabb;
        internal object UserData;

        internal int ParentOrNext;

        internal int Child1;
        internal int Child2;
    }

    /// <summary>
    /// A dynamic tree arranges data in a binary tree to accelerate
    /// queries such as volume queries and ray casts. Leafs are proxies
    /// with an AABB. In the tree we expand the proxy AABB by Settings.b2_fatAABBFactor
    /// so that the proxy AABB is bigger than the client object. This allows the client
    /// object to move by small amounts without triggering a tree update.
    /// Nodes are pooled and relocatable, so we use node indices rather than pointers.
    /// </summary>
    public class DynamicTree
    {
        public const int NullNode = -1;

        /// constructing the tree initializes the node pool.
        public DynamicTree()
        {
            _root = NullNode;

            _nodeCapacity = 16;
            _nodeCount = 0;
            _nodes = new DynamicTreeNode[_nodeCapacity];

            // Build a linked list for the free list.
            for (int i = 0; i < _nodeCapacity - 1; ++i)
            {
                _nodes[i].ParentOrNext = i + 1;
            }
            _nodes[_nodeCapacity - 1].ParentOrNext = NullNode;
            _freeList = 0;
        }

        /// <summary>
        /// Create a proxy. Provide a tight fitting AABB and a userData pointer.
        /// </summary>
        /// <param name="aabb">The aabb.</param>
        /// <param name="userData">The user data.</param>
        /// <returns></returns>
        public int CreateProxy(ref AABB aabb, object userData)
        {
            int proxyId = AllocateNode();

            // Fatten the aabb.
            Vector2 r = new Vector2(Settings.AabbExtension, Settings.AabbExtension);
            _nodes[proxyId].Aabb.LowerBound = aabb.LowerBound - r;
            _nodes[proxyId].Aabb.UpperBound = aabb.UpperBound + r;
            _nodes[proxyId].UserData = userData;

            InsertLeaf(proxyId);

            return proxyId;
        }

        /// <summary>
        /// Destroy a proxy. This asserts if the id is invalid.
        /// </summary>
        /// <param name="proxyId">The proxy id.</param>
        public void DestroyProxy(int proxyId)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            Debug.Assert(_nodes[proxyId].IsLeaf());

            RemoveLeaf(proxyId);
            FreeNode(proxyId);
        }

        /// <summary>
        /// Move a proxy with a swepted AABB. If the proxy has moved outside of its fattened AABB,
        /// then the proxy is removed from the tree and re-inserted. Otherwise
        /// the function returns immediately.
        /// </summary>
        /// <param name="proxyId">The proxy id.</param>
        /// <param name="aabb">The aabb.</param>
        /// <param name="displacement">The displacement.</param>
        /// <returns>true if the proxy was re-inserted.</returns>
        public bool MoveProxy(int proxyId, ref AABB aabb, Vector2 displacement)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            Debug.Assert(_nodes[proxyId].IsLeaf());

            if (_nodes[proxyId].Aabb.Contains(ref aabb))
            {
                return false;
            }

            RemoveLeaf(proxyId);

            // Extend AABB.
            AABB b = aabb;

            Vector2 r = new Vector2(Settings.AabbExtension, Settings.AabbExtension);
            b.LowerBound = b.LowerBound - r;
            b.UpperBound = b.UpperBound + r;

            // Predict AABB displacement.
            Vector2 d = Settings.AabbMultiplier * displacement;

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

        /// <summary>
        /// Perform some iterations to re-balance the tree.
        /// </summary>
        /// <param name="iterations">The iterations.</param>
        public void Rebalance(int iterations)
        {
            if (_root == NullNode)
            {
                return;
            }

            for (int i = 0; i < iterations; ++i)
            {
                int node = _root;

                int bit = 0;
                while (_nodes[node].IsLeaf() == false)
                {
                    node = ((_path >> bit) & 1) == 0 ? _nodes[node].Child1 : _nodes[node].Child2;
                    bit = (bit + 1) & (8 * sizeof(uint) - 1);
                }
                ++_path;

                RemoveLeaf(node);
                InsertLeaf(node);
            }
        }

        /// <summary>
        /// Get proxy user data.
        /// </summary>
        /// <param name="proxyId">The proxy id.</param>
        /// <returns>the proxy user data or 0 if the id is invalid.</returns>
        public T GetUserData<T>(int proxyId)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            return (T)_nodes[proxyId].UserData;
        }

        /// <summary>
        /// Get the fat AABB for a proxy.
        /// </summary>
        /// <param name="proxyId">The proxy id.</param>
        /// <param name="fatAABB">The fat AABB.</param>
        public void GetFatAABB(int proxyId, out AABB fatAABB)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            fatAABB = _nodes[proxyId].Aabb;
        }

        /// Query an AABB for overlapping proxies. The callback class
        /// is called for each proxy that overlaps the supplied AABB.
        public void Query(Func<int, bool> callback, ref AABB aabb)
        {
            int count = 0;
            _stack[count++] = _root;

            while (count > 0)
            {
                int nodeId = _stack[--count];
                if (nodeId == NullNode)
                {
                    continue;
                }

                DynamicTreeNode node = _nodes[nodeId];

                if (AABB.TestOverlap(ref node.Aabb, ref aabb))
                {
                    if (node.IsLeaf())
                    {
                        bool proceed = callback(nodeId);
                        if (!proceed)
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (count < k_stackSize)
                        {
                            _stack[count++] = node.Child1;
                        }

                        if (count < k_stackSize)
                        {
                            _stack[count++] = node.Child2;
                        }
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
        public void RayCast(RayCastCallback callback, ref RayCastInput input)
        {
            Vector2 p1 = input.Point1;
            Vector2 p2 = input.Point2;
            Vector2 r = p2 - p1;
            Debug.Assert(r.LengthSquared() > 0.0f);
            r.Normalize();

            // v is perpendicular to the segment.
            Vector2 v = MathUtils.Cross(1.0f, r);
            Vector2 abs_v = MathUtils.Abs(v);

            // Separating axis for segment (Gino, p80).
            // |dot(v, p1 - c)| > dot(|v|, h)

            float maxFraction = input.MaxFraction;

            // Build a bounding box for the segment.
            AABB segmentAABB = new AABB();
            {
                Vector2 t = p1 + maxFraction * (p2 - p1);
                segmentAABB.LowerBound = Vector2.Min(p1, t);
                segmentAABB.UpperBound = Vector2.Max(p1, t);
            }

            int count = 0;
            _stack[count++] = _root;

            while (count > 0)
            {
                int nodeId = _stack[--count];
                if (nodeId == NullNode)
                {
                    continue;
                }

                DynamicTreeNode node = _nodes[nodeId];

                if (AABB.TestOverlap(ref node.Aabb, ref segmentAABB) == false)
                {
                    continue;
                }

                // Separating axis for segment (Gino, p80).
                // |dot(v, p1 - c)| > dot(|v|, h)
                Vector2 c = node.Aabb.Center;
                Vector2 h = node.Aabb.Extents;
                float separation = Math.Abs(Vector2.Dot(v, p1 - c)) - Vector2.Dot(abs_v, h);
                if (separation > 0.0f)
                {
                    continue;
                }

                if (node.IsLeaf())
                {
                    RayCastInput subInput;
                    subInput.Point1 = input.Point1;
                    subInput.Point2 = input.Point2;
                    subInput.MaxFraction = maxFraction;

                    maxFraction = callback(ref subInput, nodeId);

                    if (maxFraction == 0.0f)
                    {
                        return;
                    }

                    // Update segment bounding box.
                    {
                        Vector2 t = p1 + maxFraction * (p2 - p1);
                        segmentAABB.LowerBound = Vector2.Min(p1, t);
                        segmentAABB.UpperBound = Vector2.Max(p1, t);
                    }
                }
                else
                {
                    if (count < k_stackSize)
                    {
                        _stack[count++] = node.Child1;
                    }

                    if (count < k_stackSize)
                    {
                        _stack[count++] = node.Child2;
                    }
                }
            }
        }

        private int AllocateNode()
        {
            // Expand the node pool as needed.
            if (_freeList == NullNode)
            {
                Debug.Assert(_nodeCount == _nodeCapacity);

                // The free list is empty. Rebuild a bigger pool.
                DynamicTreeNode[] oldNodes = _nodes;
                _nodeCapacity *= 2;
                _nodes = new DynamicTreeNode[_nodeCapacity];
                Array.Copy(oldNodes, _nodes, _nodeCount);

                // Build a linked list for the free list. The parent
                // pointer becomes the "next" pointer.
                for (int i = _nodeCount; i < _nodeCapacity - 1; ++i)
                {
                    _nodes[i].ParentOrNext = i + 1;
                }
                _nodes[_nodeCapacity - 1].ParentOrNext = NullNode;
                _freeList = _nodeCount;
            }

            // Peel a node off the free list.
            int nodeId = _freeList;
            _freeList = _nodes[nodeId].ParentOrNext;
            _nodes[nodeId].ParentOrNext = NullNode;
            _nodes[nodeId].Child1 = NullNode;
            _nodes[nodeId].Child2 = NullNode;
            ++_nodeCount;
            return nodeId;
        }

        private void FreeNode(int nodeId)
        {
            Debug.Assert(0 <= nodeId && nodeId < _nodeCapacity);
            Debug.Assert(0 < _nodeCount);
            _nodes[nodeId].ParentOrNext = _freeList;
            _freeList = nodeId;
            --_nodeCount;
        }

        private void InsertLeaf(int leaf)
        {
            ++_insertionCount;

            if (_root == NullNode)
            {
                _root = leaf;
                _nodes[_root].ParentOrNext = NullNode;
                return;
            }

            // Find the best sibling for this node.
            Vector2 center = _nodes[leaf].Aabb.Center;
            int sibling = _root;
            if (_nodes[sibling].IsLeaf() == false)
            {
                do
                {
                    int child1 = _nodes[sibling].Child1;
                    int child2 = _nodes[sibling].Child2;

                    Vector2 delta1 = MathUtils.Abs(_nodes[child1].Aabb.Center - center);
                    Vector2 delta2 = MathUtils.Abs(_nodes[child2].Aabb.Center - center);

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
            int node1 = _nodes[sibling].ParentOrNext;
            int node2 = AllocateNode();
            _nodes[node2].ParentOrNext = node1;
            //_nodes[node2].UserData = 0;
            _nodes[node2].Aabb.Combine(ref _nodes[leaf].Aabb, ref _nodes[sibling].Aabb);

            if (node1 != NullNode)
            {
                if (_nodes[_nodes[sibling].ParentOrNext].Child1 == sibling)
                {
                    _nodes[node1].Child1 = node2;
                }
                else
                {
                    _nodes[node1].Child2 = node2;
                }

                _nodes[node2].Child1 = sibling;
                _nodes[node2].Child2 = leaf;
                _nodes[sibling].ParentOrNext = node2;
                _nodes[leaf].ParentOrNext = node2;

                do
                {
                    if (_nodes[node1].Aabb.Contains(ref _nodes[node2].Aabb))
                    {
                        break;
                    }

                    _nodes[node1].Aabb.Combine(ref _nodes[_nodes[node1].Child1].Aabb, ref _nodes[_nodes[node1].Child2].Aabb);
                    node2 = node1;
                    node1 = _nodes[node1].ParentOrNext;
                }
                while (node1 != NullNode);
            }
            else
            {
                _nodes[node2].Child1 = sibling;
                _nodes[node2].Child2 = leaf;
                _nodes[sibling].ParentOrNext = node2;
                _nodes[leaf].ParentOrNext = node2;
                _root = node2;
            }
        }

        private void RemoveLeaf(int leaf)
        {
            if (leaf == _root)
            {
                _root = NullNode;
                return;
            }

            int node2 = _nodes[leaf].ParentOrNext;
            int node1 = _nodes[node2].ParentOrNext;
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
                _nodes[sibling].ParentOrNext = node1;
                FreeNode(node2);

                // Adjust ancestor bounds.
                while (node1 != NullNode)
                {
                    AABB oldAABB = _nodes[node1].Aabb;
                    _nodes[node1].Aabb.Combine(ref _nodes[_nodes[node1].Child1].Aabb, ref _nodes[_nodes[node1].Child2].Aabb);

                    if (oldAABB.Contains(ref _nodes[node1].Aabb))
                    {
                        break;
                    }

                    node1 = _nodes[node1].ParentOrNext;
                }
            }
            else
            {
                _root = sibling;
                _nodes[sibling].ParentOrNext = NullNode;
                FreeNode(node2);
            }
        }

        private const int k_stackSize = 128;
        private static int[] _stack = new int[k_stackSize];
        private int _root;
        private DynamicTreeNode[] _nodes;
        private int _nodeCount;
        private int _nodeCapacity;
        private int _freeList;
        private int _insertionCount;

        /// This is used incrementally traverse the tree for re-balancing.
        private int _path;
    }
}
