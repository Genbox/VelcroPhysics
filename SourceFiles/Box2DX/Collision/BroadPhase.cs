/*
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
using Box2DX.Common;
using Box2DX.Dynamics;
using Box2DX.Stuff;

namespace Box2DX.Collision
{
    public class Pair
    {
        public int proxyIdA;
        public int proxyIdB;
        public int next;
    };

    /// The broad-phase is used for computing pairs and performing volume queries and ray casts.
    /// This broad-phase does not persist pairs. Instead, this reports potentially new pairs.
    /// It is up to the client to consume the new pairs and to track subsequent overlap.
    public class BroadPhase : IQueryEnabled
    {
        public const int NullProxy = -1;

        private DynamicTree _tree;

        private int _proxyCount;

        private int[] _moveBuffer;
        private int _moveCapacity;
        private int _moveCount;

        private Pair[] _pairBuffer;
        private int _pairCapacity;
        private int _pairCount;

        private int _queryProxyId;

        private PairComparer _comparer = new PairComparer();

        public BroadPhase()
        {
            _tree = new DynamicTree();

            _proxyCount = 0;

            _pairCapacity = 16;
            _pairCount = 0;
            _pairBuffer = new Pair[_pairCapacity];

            //Fill array with items
            for (int i = 0; i < _pairCapacity; i++)
            {
                _pairBuffer[i] = new Pair();
            }

            _moveCapacity = 16;
            _moveCount = 0;
            _moveBuffer = new int[_moveCapacity];
        }

        /// Create a proxy with an initial AABB. Pairs are not reported until
        /// UpdatePairs is called.
        public int CreateProxy(AABB aabb, object userData)
        {
            int proxyId = _tree.CreateProxy(aabb, userData);
            ++_proxyCount;
            BufferMove(proxyId);
            return proxyId;
        }

        /// Destroy a proxy. It is up to the client to remove any pairs.
        public void DestroyProxy(int proxyId)
        {
            UnBufferMove(proxyId);
            --_proxyCount;
            _tree.DestroyProxy(proxyId);
        }

        /// Call MoveProxy as many times as you like, then when you are done
        /// call UpdatePairs to finalized the proxy pairs (for your time step).
        public void MoveProxy(int proxyId, AABB aabb, Vec2 displacement)
        {
            bool buffer = _tree.MoveProxy(proxyId, aabb, displacement);
            if (buffer)
            {
                BufferMove(proxyId);
            }
        }

        public class PairComparer : IComparer<Pair>
        {
            public int Compare(Pair pair1, Pair pair2)
            {
                if (pair1.proxyIdA < pair2.proxyIdA)
                {
                    return 1;
                }

                if (pair1.proxyIdA > pair2.proxyIdA)
                {
                    return -1;
                }

                if (pair1.proxyIdA == pair2.proxyIdA)
                {
                    if (pair1.proxyIdB < pair2.proxyIdB)
                    {
                        return 1;
                    }

                    if (pair1.proxyIdB > pair2.proxyIdB)
                    {
                        return -1;
                    }
                }

                return 0;

            }
        }

        /// Get user data from a proxy. Returns NULL if the id is invalid.
        public object GetUserData(int proxyId)
        {
            return _tree.GetUserData(proxyId);
        }

        /// Test overlap of fat AABBs.
        public bool TestOverlap(int proxyIdA, int proxyIdB)
        {
            AABB aabbA = _tree.GetFatAABB(proxyIdA);
            AABB aabbB = _tree.GetFatAABB(proxyIdB);
            return Collision.TestOverlap(aabbA, aabbB);
        }

        /// Get the fat AABB for a proxy.
        public AABB GetFatAABB(int proxyId)
        {
            return _tree.GetFatAABB(proxyId);
        }

        /// Get the number of proxies.
        public int GetProxyCount()
        {
            return _proxyCount;
        }

        /// Compute the height of the embedded tree.
        public int ComputeHeight()
        {
            return _tree.ComputeHeight();
        }

        /// Update the pairs. This results in pair callbacks. This can only add pairs.
        public void UpdatePairs(ContactManager callback)
        {
            // Reset pair buffer
            _pairCount = 0;

            // Perform tree queries for all moving proxies.
            for (int i = 0; i < _moveCount; ++i)
            {
                _queryProxyId = _moveBuffer[i];
                if (_queryProxyId == NullProxy)
                {
                    continue;
                }

                // We have to query the tree with the fat AABB so that
                // we don't fail to create a pair that may touch later.
                AABB fatAABB = _tree.GetFatAABB(_queryProxyId);

                // Query tree, create pairs and add them pair buffer.
                _tree.Query(this, fatAABB);
            }

            // Reset move buffer
            _moveCount = 0;

            // Sort the pair buffer to expose duplicates.
            Array.Sort(_pairBuffer, 0, _pairCount, _comparer);

            // Send the pairs back to the client.
            int j = 0;
            while (j < _pairCount)
            {
                Pair primaryPair = _pairBuffer[j];
                object userDataA = _tree.GetUserData(primaryPair.proxyIdA);
                object userDataB = _tree.GetUserData(primaryPair.proxyIdB);

                callback.AddPair(userDataA, userDataB);
                ++j;

                // Skip any duplicate pairs.
                while (j < _pairCount)
                {
                    Pair pair = _pairBuffer[j];
                    if (pair.proxyIdA != primaryPair.proxyIdA || pair.proxyIdB != primaryPair.proxyIdB)
                    {
                        break;
                    }
                    ++j;
                }
            }
        }

        /// Query an AABB for overlapping proxies. The callback class
        /// is called for each proxy that overlaps the supplied AABB.
        public void Query(World.WorldQueryWrapper callback, AABB aabb)
        {
            _tree.Query(callback, aabb);
        }

        /// Ray-cast against the proxies in the tree. This relies on the callback
        /// to perform a exact ray-cast in the case were the proxy contains a shape.
        /// The callback also performs the any collision filtering. This has performance
        /// roughly equal to k * log(n), where k is the number of collisions and n is the
        /// number of proxies in the tree.
        /// @param input the ray-cast input data. The ray extends from p1 to p1 + maxFraction * (p2 - p1).
        /// @param callback a callback class that is called for each proxy that is hit by the ray.
        public void RayCast(World.WorldRayCastWrapper callback, RayCastInput input)
        {
            _tree.RayCast(callback, input);
        }

        private void BufferMove(int proxyId)
        {
            if (_moveCount == _moveCapacity)
            {
                _moveCapacity *= 2;
                Array.Resize(ref _moveBuffer, _moveCapacity);
            }

            _moveBuffer[_moveCount] = proxyId;
            ++_moveCount;
        }

        private void UnBufferMove(int proxyId)
        {
            for (int i = 0; i < _moveCount; ++i)
            {
                if (_moveBuffer[i] == proxyId)
                {
                    _moveBuffer[i] = NullProxy;
                    return;
                }
            }
        }

        // This is called from DynamicTree::Query when we are gathering pairs.
        public bool QueryCallback(int proxyId)
        {
            // A proxy cannot form a pair with itself.
            if (proxyId == _queryProxyId)
            {
                return true;
            }

            // Grow the pair buffer as needed.
            if (_pairCount == _pairCapacity)
            {
                _pairCapacity *= 2;
                Array.Resize(ref _pairBuffer, _pairCapacity);

                //Fill the array with items
                for (int i = _pairCount; i < _pairCapacity; i++)
                {
                    _pairBuffer[i] = new Pair();
                }
            }

            _pairBuffer[_pairCount].proxyIdA = Common.Math.Min(proxyId, _queryProxyId);
            _pairBuffer[_pairCount].proxyIdB = Common.Math.Max(proxyId, _queryProxyId);
            ++_pairCount;

            return true;
        }
    }
}