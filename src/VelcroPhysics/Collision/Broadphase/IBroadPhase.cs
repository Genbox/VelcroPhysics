using System;
using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.Handlers;
using VelcroPhysics.Collision.RayCast;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Shared;

namespace VelcroPhysics.Collision.Broadphase
{
    public interface IBroadPhase
    {
        int ProxyCount { get; }
        void UpdatePairs(BroadphaseHandler callback);

        bool TestOverlap(int proxyIdA, int proxyIdB);

        int AddProxy(ref FixtureProxy proxy);

        void RemoveProxy(int proxyId);

        void MoveProxy(int proxyId, ref AABB aabb, Vector2 displacement);

        FixtureProxy GetProxy(int proxyId);

        void TouchProxy(int proxyId);

        void GetFatAABB(int proxyId, out AABB aabb);

        void Query(Func<int, bool> callback, ref AABB aabb);

        void RayCast(Func<RayCastInput, int, float> callback, ref RayCastInput input);

        void ShiftOrigin(Vector2 newOrigin);
    }
}