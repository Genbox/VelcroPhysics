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
using System.Diagnostics;
using Microsoft.Xna.Framework;
namespace Box2D.XNA
{
    internal struct Pair : IComparable<Pair>
    {
        public int proxyIdA;
        public int proxyIdB;
        public int next;

        public int CompareTo(Pair other)
        {
            if (proxyIdA < other.proxyIdA)
            {
                return -1;
            }
            else if (proxyIdA == other.proxyIdA)
            {
                if (proxyIdB < other.proxyIdB)
                {
                    return -1;
                }
                else if (proxyIdB == other.proxyIdB)
                {
                    return 0;
                }
            }

            return 1;
        }
    };

    /// The broad-phase is used for computing pairs and performing volume queries and ray casts.
    /// This broad-phase does not persist pairs. Instead, this reports potentially new pairs.
    /// It is up to the client to consume the new pairs and to track subsequent overlap.
    public class BroadPhase
    {
        internal static int NullProxy = -1;

	    public BroadPhase()
        {
            _queryCallback = new Func<int, bool>(QueryCallback);

	        _proxyCount = 0;
	        
	        _pairCapacity = 16;
	        _pairCount = 0;
            _pairBuffer = new Pair[_pairCapacity];

	        _moveCapacity = 16;
	        _moveCount = 0;
            _moveBuffer = new int[_moveCapacity];
        }

	    /// Create a proxy with an initial AABB. Pairs are not reported until
	    /// UpdatePairs is called.
	    public int CreateProxy(ref AABB aabb, object userData)
        {
	        int proxyId = _tree.CreateProxy(ref aabb, userData);
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

	    public void MoveProxy(int proxyId, ref AABB aabb, Vector2 displacement)
        {
            bool buffer = _tree.MoveProxy(proxyId, ref aabb, displacement);
            if (buffer)
            {
                BufferMove(proxyId);
            }
        }

	    /// Get the AABB for a proxy.
	    public void GetFatAABB(int proxyId, out AABB aabb)
        {
            _tree.GetFatAABB(proxyId, out aabb);
        }

	    /// Get user data from a proxy. Returns null if the id is invalid.
	    public object GetUserData(int proxyId)
        {
            return _tree.GetUserData(proxyId);
        }

        /// Test overlap of fat AABBs.
        public bool TestOverlap(int proxyIdA, int proxyIdB)
        {
            AABB aabbA, aabbB;
            _tree.GetFatAABB(proxyIdA, out aabbA);
	        _tree.GetFatAABB(proxyIdB, out aabbB);
	        return AABB.TestOverlap(ref aabbA, ref aabbB);
        }


	    /// Get the number of proxies.
	    public int ProxyCount
        {
            get
            {
                return _proxyCount;
            }
        }

	    /// Update the pairs. This results in pair callbacks. This can only add pairs.
	    public void UpdatePairs<T>(Action<T,T> callback)
        {
            // Reset pair buffer
	        _pairCount = 0;

	        // Perform tree queries for all moving proxies.
	        for (int j = 0; j < _moveCount; ++j)
	        {
		        _queryProxyId = _moveBuffer[j];
		        if (_queryProxyId == NullProxy)
		        {
			        continue;
		        }

                // We have to query the tree with the fat AABB so that
		        // we don't fail to create a pair that may touch later.
		        AABB fatAABB;
                _tree.GetFatAABB(_queryProxyId, out fatAABB);

                // Query tree, create pairs and add them pair buffer.
                _tree.Query(_queryCallback, ref fatAABB);
	        }

	        // Reset move buffer
	        _moveCount = 0;

	        // Sort the pair buffer to expose duplicates.
            Array.Sort(_pairBuffer, 0, _pairCount);

	        // Send the pairs back to the client.
	        int i = 0;
	        while (i < _pairCount)
	        {
		        Pair primaryPair = _pairBuffer[i];
                object userDataA = _tree.GetUserData(primaryPair.proxyIdA);
                object userDataB = _tree.GetUserData(primaryPair.proxyIdB);

		        callback((T)userDataA, (T)userDataB);
		        ++i;

		        // Skip any duplicate pairs.
		        while (i < _pairCount)
		        {
			        Pair pair = _pairBuffer[i];
			        if (pair.proxyIdA != primaryPair.proxyIdA || pair.proxyIdB != primaryPair.proxyIdB)
			        {
				        break;
			        }
			        ++i;
		        }
	        }
        }

	    /// Query an AABB for overlapping proxies. The callback class
	    /// is called for each proxy that overlaps the supplied AABB.
	    public void Query(Func<int, bool> callback, ref AABB aabb)
        {
	        _tree.Query(callback, ref aabb);
        }

        public void RayCast(RayCastCallback callback, ref RayCastInput input)
        {
	        _tree.RayCast(callback, ref input);
        }

	    /// Compute the height of the embedded tree.
	    public int ComputeHeight()
        {
            return _tree.ComputeHeight();
        }

	    internal void BufferMove(int proxyId)
        {
	        if (_moveCount == _moveCapacity)
	        {
		        int[] oldBuffer = _moveBuffer;
		        _moveCapacity *= 2;
		        _moveBuffer = new int[_moveCapacity];
                Array.Copy(oldBuffer, _moveBuffer, _moveCount);
	        }

	        _moveBuffer[_moveCount] = proxyId;
	        ++_moveCount;
        }

	    internal void UnBufferMove(int proxyId)
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

        internal bool QueryCallback(int proxyId)
        {
	        // A proxy cannot form a pair with itself.
	        if (proxyId == _queryProxyId)
	        {
		        return true;
	        }

	        // Grow the pair buffer as needed.
	        if (_pairCount == _pairCapacity)
	        {
		        Pair[] oldBuffer = _pairBuffer;
		        _pairCapacity *= 2;
                _pairBuffer = new Pair[_pairCapacity];
                Array.Copy(oldBuffer, _pairBuffer, _pairCount);
	        }

	        _pairBuffer[_pairCount].proxyIdA = Math.Min(proxyId, _queryProxyId);
	        _pairBuffer[_pairCount].proxyIdB = Math.Max(proxyId, _queryProxyId);
	        ++_pairCount;

            return true;
        }

	    internal DynamicTree _tree = new DynamicTree();

        internal int _proxyCount;

	    internal int[] _moveBuffer;
	    internal int _moveCapacity;
	    internal int _moveCount;

	    internal Pair[] _pairBuffer;
	    internal int _pairCapacity;
	    internal int _pairCount;

	    internal int _queryProxyId;

        Func<int, bool> _queryCallback;
    }
}
