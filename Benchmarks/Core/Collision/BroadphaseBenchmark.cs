using System;
using BenchmarkDotNet.Attributes;
using Benchmarks.Utilities;
using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.Broadphase;
using VelcroPhysics.Collision.RayCast;
using VelcroPhysics.Shared;

namespace Benchmarks.Core.Collision
{
    [MemoryDiagnoser]
    public class BroadphaseBenchmark
    {
        private float _proxyExtent;
        private Random _random;
        private float _worldExtent;

        private DynamicTree<Actor> _tree;
        private AABB _queryAABB;
        private RayCastInput _rayCastInput;
        private Actor[] _actors;

        [Setup]
        public void Setup()
        {
            _worldExtent = 15.0f;
            _proxyExtent = 0.5f;

            _random = new Random(888);

            float h = _worldExtent;
            _queryAABB.LowerBound = new Vector2(-3.0f, -4.0f + h);
            _queryAABB.UpperBound = new Vector2(5.0f, 6.0f + h);

            _rayCastInput.Point1 = new Vector2(-5.0f, 5.0f + h);
            _rayCastInput.Point2 = new Vector2(7.0f, -4.0f + h);
            _rayCastInput.MaxFraction = 1.0f;

            _tree = new DynamicTree<Actor>();
            _actors = new Actor[100];

            for (int i = 0; i < _actors.Length; i++)
            {
                Actor a = new Actor();
                GetRandomAABB(out AABB randAabb);
                a.AABB = randAabb;
                _actors[i] = a;
            }
        }

        [Benchmark]
        public void CreateMoveDelete()
        {
            for (int i = 0; i < _actors.Length; i++)
            {
                _actors[i].ProxyId = _tree.AddProxy(ref _actors[i].AABB, null);
            }

            foreach (Actor a in _actors)
            {
                if (a.ProxyId == -1)
                    continue;

                AABB aabb0 = a.AABB;
                RandomMoveAABB(ref a.AABB);
                Vector2 displacement = a.AABB.Center - aabb0.Center;
                _tree.MoveProxy(a.ProxyId, ref a.AABB, displacement);
            }

            foreach (Actor a in _actors)
            {
                if (a.ProxyId == -1)
                    continue;

                _tree.RemoveProxy(a.ProxyId);
            }
        }

        [Benchmark]
        public void Query()
        {
            _tree.Query(QueryCallback, ref _queryAABB);
        }

        [Benchmark]
        public void RayCast()
        {
            RayCastInput input = _rayCastInput;
            _tree.RayCast(RayCastCallback, ref input);
        }

        private bool QueryCallback(int proxyId)
        {
            Actor actor = _tree.GetUserData(proxyId);
            AABB.TestOverlap(ref _queryAABB, ref actor.AABB);
            return true;
        }

        private float RayCastCallback(RayCastInput input, int proxyId)
        {
            Actor actor = _tree.GetUserData(proxyId);

            RayCastOutput output;
            bool hit = actor.AABB.RayCast(out output, ref input);
            return hit ? output.Fraction : input.MaxFraction;
        }

        private void GetRandomAABB(out AABB aabb)
        {
            aabb = new AABB();

            Vector2 w = new Vector2(2.0f * _proxyExtent, 2.0f * _proxyExtent);
            aabb.LowerBound.X = _random.RandomFloat(-_worldExtent, _worldExtent);
            aabb.LowerBound.Y = _random.RandomFloat(0.0f, 2.0f * _worldExtent);
            aabb.UpperBound = aabb.LowerBound + w;
        }

        private void RandomMoveAABB(ref AABB aabb)
        {
            Vector2 d = Vector2.Zero;
            d.X = _random.RandomFloat(-0.5f, 0.5f);
            d.Y = _random.RandomFloat(-0.5f, 0.5f);

            aabb.LowerBound += d;
            aabb.UpperBound += d;

            Vector2 c0 = 0.5f * (aabb.LowerBound + aabb.UpperBound);
            Vector2 min = new Vector2(-_worldExtent, 0.0f);
            Vector2 max = new Vector2(_worldExtent, 2.0f * _worldExtent);
            Vector2 c = Vector2.Clamp(c0, min, max);

            aabb.LowerBound += c - c0;
            aabb.UpperBound += c - c0;
        }

        private sealed class Actor
        {
            internal AABB AABB;
            internal int ProxyId;
        }
    }
}