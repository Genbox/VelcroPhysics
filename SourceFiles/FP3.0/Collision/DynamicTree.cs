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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision
{
    /// A dynamic AABB tree broad-phase, inspired by Nathanael Presson's btDbvt.
    public delegate float RayCastCallbackInternal(ref RayCastInput input, int userData);

    /// A node in the dynamic tree. The client does not interact with this directly.
    internal struct DynamicTreeNode
    {
        /// This is the fattened AABB.
        internal AABB aabb;

        internal int child1;
        internal int child2;

        internal int leafCount;
        internal int parentOrNext;
        internal object userData;

        internal bool IsLeaf()
        {
            return child1 == DynamicTree.NullNode;
        }
    }

    /// A dynamic tree arranges data in a binary tree to accelerate
    /// queries such as volume queries and ray casts. Leafs are proxies
    /// with an AABB. In the tree we expand the proxy AABB by Settings.b2_fatAABBFactor
    /// so that the proxy AABB is bigger than the client object. This allows the client
    /// object to move by small amounts without triggering a tree update.
    ///
    /// Nodes are pooled and relocatable, so we use node indices rather than pointers.
    public class DynamicTree
    {
        internal static int NullNode = -1;
        private static Stack<int> stack = new Stack<int>(256);
        private int _freeList;
        private int _insertionCount;
        private int _nodeCapacity;
        private int _nodeCount;
        private DynamicTreeNode[] _nodes;

        /// This is used incrementally traverse the tree for re-balancing.
        private int _path;

        private int _root;

        /// ructing the tree initializes the node pool.
        public DynamicTree()
        {
            _root = NullNode;

            _nodeCapacity = 16;
            _nodeCount = 0;
            _nodes = new DynamicTreeNode[_nodeCapacity];

            // Build a linked list for the free list.
            for (int i = 0; i < _nodeCapacity - 1; ++i)
            {
                _nodes[i].parentOrNext = i + 1;
            }
            _nodes[_nodeCapacity - 1].parentOrNext = NullNode;
            _freeList = 0;

            _path = 0;
        }

        /// Create a proxy. Provide a tight fitting AABB and a userData pointer.
        public int CreateProxy(ref AABB aabb, object userData)
        {
            int proxyId = AllocateNode();

            // Fatten the aabb.
            Vector2 r = new Vector2(Settings.AABBExtension, Settings.AABBExtension);
            _nodes[proxyId].aabb.LowerBound = aabb.LowerBound - r;
            _nodes[proxyId].aabb.UpperBound = aabb.UpperBound + r;
            _nodes[proxyId].userData = userData;
            _nodes[proxyId].leafCount = 1;

            InsertLeaf(proxyId);

            // Rebalance if necessary.
            int iterationCount = _nodeCount >> 4;
            int tryCount = 0;
            int height = ComputeHeight();
            while (height > 64 && tryCount < 10)
            {
                Rebalance(iterationCount);
                height = ComputeHeight();
                ++tryCount;
            }

            return proxyId;
        }

        /// Destroy a proxy. This asserts if the id is invalid.
        public void DestroyProxy(int proxyId)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            Debug.Assert(_nodes[proxyId].IsLeaf());

            RemoveLeaf(proxyId);
            FreeNode(proxyId);
        }

        /// Move a proxy with a swepted AABB. If the proxy has moved outside of its fattened AABB,
        /// then the proxy is removed from the tree and re-inserted. Otherwise
        /// the function returns immediately.
        /// @return true if the proxy was re-inserted.
        public bool MoveProxy(int proxyId, ref AABB aabb, Vector2 displacement)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);

            Debug.Assert(_nodes[proxyId].IsLeaf());

            if (_nodes[proxyId].aabb.Contains(ref aabb))
            {
                return false;
            }

            RemoveLeaf(proxyId);

            // Extend AABB.
            AABB b = aabb;

            Vector2 r = new Vector2(Settings.AABBExtension, Settings.AABBExtension);
            b.LowerBound = b.LowerBound - r;
            b.UpperBound = b.UpperBound + r;

            // Predict AABB displacement.
            Vector2 d = Settings.AABBMultiplier * displacement;

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

            _nodes[proxyId].aabb = b;

            InsertLeaf(proxyId);

            return true;
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
                int node = _root;

                int bit = 0;
                while (_nodes[node].IsLeaf() == false)
                {
                    // Child selector based on a bit in the path
                    int selector = (_path >> bit) & 1;

                    // Select the child nod
                    node = (selector == 0) ? _nodes[node].child1 : _nodes[node].child2;

                    // Keep bit between 0 and 31 because _path has 32 bits
                    // bit = (bit + 1) % 31
                    bit = (bit + 1) & 0x1F;
                }
                ++_path;

                RemoveLeaf(node);
                InsertLeaf(node);
            }
        }

        /// Get proxy user data.
        /// @return the proxy user data or 0 if the id is invalid.
        public T GetUserData<T>(int proxyId)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            return (T) _nodes[proxyId].userData;
        }

        /// Get the fat AABB for a proxy.
        public void GetFatAABB(int proxyId, out AABB fatAABB)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            fatAABB = _nodes[proxyId].aabb;
        }

        /// Compute the height of the binary tree in O(N) time. Should not be
        /// called often.
        public int ComputeHeight()
        {
            return ComputeHeight(_root);
        }

        /// Query an AABB for overlapping proxies. The callback class
        /// is called for each proxy that overlaps the supplied AABB.
        public void Query(Func<int, bool> callback, ref AABB aabb)
        {
            stack.Clear();
            stack.Push(_root);

            while (stack.Count > 0)
            {
                int nodeId = stack.Pop();
                if (nodeId == NullNode)
                {
                    continue;
                }

                DynamicTreeNode node = _nodes[nodeId];

                if (AABB.TestOverlap(ref node.aabb, ref aabb))
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
                        stack.Push(node.child1);
                        stack.Push(node.child2);
                    }
                }
            }
        }

        /// Ray-cast against the proxies in the tree. This relies on the callback
        /// to perform a exact ray-cast in the case were the proxy contains a Shape.
        /// The callback also performs the any collision filtering. This has performance
        /// roughly equal to k * log(n), where k is the number of collisions and n is the
        /// number of proxies in the tree.
        /// @param input the ray-cast input data. The ray extends from p1 to p1 + maxFraction * (p2 - p1).
        /// @param callback a callback class that is called for each proxy that is hit by the ray.
        public void RayCast(RayCastCallbackInternal callback, ref RayCastInput input)
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

            stack.Clear();
            stack.Push(_root);

            while (stack.Count > 0)
            {
                int nodeId = stack.Pop();
                if (nodeId == NullNode)
                {
                    continue;
                }

                DynamicTreeNode node = _nodes[nodeId];

                if (AABB.TestOverlap(ref node.aabb, ref segmentAABB) == false)
                {
                    continue;
                }

                // Separating axis for segment (Gino, p80).
                // |dot(v, p1 - c)| > dot(|v|, h)
                Vector2 c = node.aabb.GetCenter();
                Vector2 h = node.aabb.GetExtents();
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

                    float value = callback(ref subInput, nodeId);

                    if (value == 0.0f)
                    {
                        // the client has terminated the raycast.
                        return;
                    }

                    if (value > 0.0f)
                    {
                        // Update segment bounding box.
                        maxFraction = value;
                        Vector2 t = p1 + maxFraction * (p2 - p1);
                        segmentAABB.LowerBound = Vector2.Min(p1, t);
                        segmentAABB.UpperBound = Vector2.Max(p1, t);
                    }
                }
                else
                {
                    stack.Push(node.child1);
                    stack.Push(node.child2);
                }
            }
        }

        private int CountLeaves(int nodeId)
        {
            if (nodeId == NullNode)
            {
                return 0;
            }

            Debug.Assert(0 <= nodeId && nodeId < _nodeCapacity);
            DynamicTreeNode node = _nodes[nodeId];

            if (node.IsLeaf())
            {
                Debug.Assert(node.leafCount == 1);
                return 1;
            }

            int count1 = CountLeaves(node.child1);
            int count2 = CountLeaves(node.child2);
            int count = count1 + count2;
            Debug.Assert(count == node.leafCount);
            return count;
        }

        private void Validate()
        {
            CountLeaves(_root);
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
                    _nodes[i].parentOrNext = i + 1;
                }
                _nodes[_nodeCapacity - 1].parentOrNext = NullNode;
                _freeList = _nodeCount;
            }

            // Peel a node off the free list.
            int nodeId = _freeList;
            _freeList = _nodes[nodeId].parentOrNext;
            _nodes[nodeId].parentOrNext = NullNode;
            _nodes[nodeId].child1 = NullNode;
            _nodes[nodeId].child2 = NullNode;
            _nodes[nodeId].leafCount = 0;
            ++_nodeCount;
            return nodeId;
        }

        private void FreeNode(int nodeId)
        {
            Debug.Assert(0 <= nodeId && nodeId < _nodeCapacity);
            Debug.Assert(0 < _nodeCount);
            _nodes[nodeId].parentOrNext = _freeList;
            _freeList = nodeId;
            --_nodeCount;
        }

        private void InsertLeaf(int leaf)
        {
            ++_insertionCount;

            if (_root == NullNode)
            {
                _root = leaf;
                _nodes[_root].parentOrNext = NullNode;
                return;
            }

            // Find the best sibling for this node
            AABB leafAABB = _nodes[leaf].aabb;
            Vector2 leafCenter = leafAABB.GetCenter();
            int sibling = _root;
            while (_nodes[sibling].IsLeaf() == false)
            {
                // Expand the node's AABB.
                _nodes[sibling].aabb.Combine(ref leafAABB);
                _nodes[sibling].leafCount += 1;

                int child1 = _nodes[sibling].child1;
                int child2 = _nodes[sibling].child2;

#if false
    // This seems to create imbalanced trees
		        Vector2 delta1 = Math.Abs(_nodes[child1].aabb.GetCenter() - leafCenter);
		        Vector2 delta2 = Math.Abs(_nodes[child2].aabb.GetCenter() - leafCenter);

		        float norm1 = delta1.x + delta1.y;
		        float norm2 = delta2.x + delta2.y;
#else
                // Surface area heuristic
                AABB aabb1 = new AABB();
                AABB aabb2 = new AABB();
                aabb1.Combine(ref leafAABB, ref _nodes[child1].aabb);
                aabb2.Combine(ref leafAABB, ref _nodes[child2].aabb);
                float norm1 = (_nodes[child1].leafCount + 1) * aabb1.GetPerimeter();
                float norm2 = (_nodes[child2].leafCount + 1) * aabb2.GetPerimeter();
#endif

                if (norm1 < norm2)
                {
                    sibling = child1;
                }
                else
                {
                    sibling = child2;
                }
            }

            // Create a new parent for the siblings.
            int oldParent = _nodes[sibling].parentOrNext;
            int newParent = AllocateNode();
            _nodes[newParent].parentOrNext = oldParent;
            _nodes[newParent].userData = null;
            _nodes[newParent].aabb.Combine(ref leafAABB, ref _nodes[sibling].aabb);
            _nodes[newParent].leafCount = _nodes[sibling].leafCount + 1;

            if (oldParent != NullNode)
            {
                // The sibling was not the root.
                if (_nodes[oldParent].child1 == sibling)
                {
                    _nodes[oldParent].child1 = newParent;
                }
                else
                {
                    _nodes[oldParent].child2 = newParent;
                }

                _nodes[newParent].child1 = sibling;
                _nodes[newParent].child2 = leaf;
                _nodes[sibling].parentOrNext = newParent;
                _nodes[leaf].parentOrNext = newParent;
            }
            else
            {
                // The sibling was the root.
                _nodes[newParent].child1 = sibling;
                _nodes[newParent].child2 = leaf;
                _nodes[sibling].parentOrNext = newParent;
                _nodes[leaf].parentOrNext = newParent;
                _root = newParent;
            }
        }

        private void RemoveLeaf(int leaf)
        {
            if (leaf == _root)
            {
                _root = NullNode;
                return;
            }

            int parent = _nodes[leaf].parentOrNext;
            int grandParent = _nodes[parent].parentOrNext;
            int sibling;
            if (_nodes[parent].child1 == leaf)
            {
                sibling = _nodes[parent].child2;
            }
            else
            {
                sibling = _nodes[parent].child1;
            }

            if (grandParent != NullNode)
            {
                // Destroy parent and connect sibling to grandParent.
                if (_nodes[grandParent].child1 == parent)
                {
                    _nodes[grandParent].child1 = sibling;
                }
                else
                {
                    _nodes[grandParent].child2 = sibling;
                }
                _nodes[sibling].parentOrNext = grandParent;
                FreeNode(parent);

                // Adjust ancestor bounds.
                parent = grandParent;
                while (parent != NullNode)
                {
                    AABB oldAABB = _nodes[parent].aabb;
                    _nodes[parent].aabb.Combine(ref _nodes[_nodes[parent].child1].aabb,
                                                ref _nodes[_nodes[parent].child2].aabb);

                    Debug.Assert(_nodes[parent].leafCount > 0);
                    _nodes[parent].leafCount -= 1;

                    parent = _nodes[parent].parentOrNext;
                }
            }
            else
            {
                _root = sibling;
                _nodes[sibling].parentOrNext = NullNode;
                FreeNode(parent);
            }
        }

        private int ComputeHeight(int nodeId)
        {
            if (nodeId == NullNode)
            {
                return 0;
            }

            Debug.Assert(0 <= nodeId && nodeId < _nodeCapacity);
            DynamicTreeNode node = _nodes[nodeId];
            int height1 = ComputeHeight(node.child1);
            int height2 = ComputeHeight(node.child2);
            return 1 + Math.Max(height1, height2);
        }
    }
}