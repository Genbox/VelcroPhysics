// MIT License

// Copyright (c) 2019 Erin Catto

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Diagnostics;
using Genbox.VelcroPhysics.Collision.Broadphase;
using Genbox.VelcroPhysics.Collision.RayCast;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class DynamicTreeTest : Test
    {
        private const int _actorCount = 128;
        private readonly float _worldExtent;
        private readonly float _proxyExtent;

        private DynamicTree<Actor> _tree = new DynamicTree<Actor>();
        private AABB _queryAABB;
        private readonly RayCastInput _rayCastInput;
        private RayCastOutput _rayCastOutput;
        private Actor _rayActor;
        private readonly Actor[] _actors = new Actor[_actorCount];
        private int _stepCount;
        private bool _automated;

        private readonly Random _rand = new Random(888);

        private DynamicTreeTest()
        {
            _worldExtent = 15.0f;
            _proxyExtent = 0.5f;

            for (int i = 0; i < _actorCount; ++i)
            {
                Actor actor = _actors[i] = new Actor();
                GetRandomAABB(ref actor.aabb);
                actor.proxyId = _tree.CreateProxy(ref actor.aabb, actor);
            }

            _stepCount = 0;

            float h = _worldExtent;
            _queryAABB.LowerBound = new Vector2(-3.0f, -4.0f + h);
            _queryAABB.UpperBound = new Vector2(5.0f, 6.0f + h);

            _rayCastInput.Point1 = new Vector2(-5.0f, 5.0f + h);
            _rayCastInput.Point2 = new Vector2(7.0f, -4.0f + h);

            //_rayCastInput.p1.Set(0.0f, 2.0f + h);
            //_rayCastInput.p2.Set(0.0f, -2.0f + h);
            _rayCastInput.MaxFraction = 1.0f;

            _automated = false;
        }

        internal static Test Create()
        {
            return new DynamicTreeTest();
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            //B2_NOT_USED(settings);

            _rayActor = null;
            for (int i = 0; i < _actorCount; ++i)
            {
                _actors[i].fraction = 1.0f;
                _actors[i].overlap = false;
            }

            if (_automated)
            {
                int actionCount = MathUtils.Max(1, _actorCount >> 2);

                for (int i = 0; i < actionCount; ++i)
                    Action();
            }

            Query();
            RayCast();

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);

            for (int i = 0; i < _actorCount; ++i)
            {
                Actor actor = _actors[i];
                if (actor.proxyId == DynamicTree<Actor>.NullNode)
                    continue;

                Color c = new Color(0.9f, 0.9f, 0.9f);
                if (actor == _rayActor && actor.overlap)
                    c = new Color(0.9f, 0.6f, 0.6f);
                else if (actor == _rayActor)
                    c = new Color(0.6f, 0.9f, 0.6f);
                else if (actor.overlap)
                    c = new Color(0.6f, 0.6f, 0.9f);

                DebugView.DrawAABB(ref actor.aabb, c);
            }

            {
                Color c = new Color(0.7f, 0.7f, 0.7f);
                DebugView.DrawAABB(ref _queryAABB, c);

                DebugView.DrawSegment(_rayCastInput.Point1, _rayCastInput.Point2, c);
            }
            Color c1 = new Color(0.2f, 0.9f, 0.2f);
            Color c2 = new Color(0.9f, 0.2f, 0.2f);
            DebugView.DrawPoint(_rayCastInput.Point1, 6.0f, c1);
            DebugView.DrawPoint(_rayCastInput.Point2, 6.0f, c2);

            if (_rayActor != null)
            {
                Color cr = new Color(0.2f, 0.2f, 0.9f);
                Vector2 p = _rayCastInput.Point1 + _rayActor.fraction * (_rayCastInput.Point2 - _rayCastInput.Point1);
                DebugView.DrawPoint(p, 6.0f, cr);
            }

            {
                int height = _tree.Height;
                DrawString("dynamic tree height = " + height);
            }

            DebugView.EndCustomDraw();

            ++_stepCount;
            base.Update(settings, gameTime);
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsNewKeyPress(Keys.A))
                _automated = !_automated;
            else if (keyboard.IsNewKeyPress(Keys.C))
                CreateProxy();
            else if (keyboard.IsNewKeyPress(Keys.D))
                DestroyProxy();
            else if (keyboard.IsNewKeyPress(Keys.M))
                MoveProxy();

            base.Keyboard(keyboard);
        }

        private bool QueryCallback(int proxyId)
        {
            Actor actor = _tree.GetUserData(proxyId);
            actor.overlap = AABB.TestOverlap(ref _queryAABB, ref actor.aabb);
            return true;
        }

        private float RayCastCallback(RayCastInput input, int proxyId)
        {
            Actor actor = _tree.GetUserData(proxyId);

            RayCastOutput output;
            bool hit = actor.aabb.RayCast(ref input, out output);

            if (hit)
            {
                _rayCastOutput = output;
                _rayActor = actor;
                _rayActor.fraction = output.Fraction;
                return output.Fraction;
            }

            return input.MaxFraction;
        }

        private class Actor
        {
            public AABB aabb;
            public float fraction;
            public bool overlap;
            public int proxyId;
        }

        private void GetRandomAABB(ref AABB aabb)
        {
            Vector2 w = new Vector2(2.0f * _proxyExtent, 2.0f * _proxyExtent);

            //aabb.lowerBound.x = -_proxyExtent;
            //aabb.lowerBound.y = -_proxyExtent + _worldExtent;
            aabb.LowerBound.X = Rand.RandomFloat(-_worldExtent, _worldExtent);
            aabb.LowerBound.Y = Rand.RandomFloat(0.0f, 2.0f * _worldExtent);
            aabb.UpperBound = aabb.LowerBound + w;
        }

        private void MoveAABB(ref AABB aabb)
        {
            Vector2 d;
            d.X = Rand.RandomFloat(-0.5f, 0.5f);
            d.Y = Rand.RandomFloat(-0.5f, 0.5f);

            //d.x = 2.0f;
            //d.y = 0.0f;
            aabb.LowerBound += d;
            aabb.UpperBound += d;

            Vector2 c0 = 0.5f * (aabb.LowerBound + aabb.UpperBound);
            Vector2 min = new Vector2(-_worldExtent, 0.0f);
            Vector2 max = new Vector2(_worldExtent, 2.0f * _worldExtent);
            Vector2 c = MathUtils.Clamp(c0, min, max);

            aabb.LowerBound += c - c0;
            aabb.UpperBound += c - c0;
        }

        private void CreateProxy()
        {
            for (int i = 0; i < _actorCount; ++i)
            {
                int j = _rand.Next() % _actorCount;
                Actor actor = _actors[j];
                if (actor.proxyId == DynamicTree<Actor>.NullNode)
                {
                    GetRandomAABB(ref actor.aabb);
                    actor.proxyId = _tree.CreateProxy(ref actor.aabb, actor);
                    return;
                }
            }
        }

        private void DestroyProxy()
        {
            for (int i = 0; i < _actorCount; ++i)
            {
                int j = _rand.Next() % _actorCount;
                Actor actor = _actors[j];
                if (actor.proxyId != DynamicTree<Actor>.NullNode)
                {
                    _tree.DestroyProxy(actor.proxyId);
                    actor.proxyId = DynamicTree<Actor>.NullNode;
                    return;
                }
            }
        }

        private void MoveProxy()
        {
            for (int i = 0; i < _actorCount; ++i)
            {
                int j = _rand.Next() % _actorCount;
                Actor actor = _actors[j];
                if (actor.proxyId == DynamicTree<Actor>.NullNode)
                    continue;

                AABB aabb0 = actor.aabb;
                MoveAABB(ref actor.aabb);
                Vector2 displacement = actor.aabb.Center - aabb0.Center;
                _tree.MoveProxy(actor.proxyId, ref actor.aabb, displacement);
                return;
            }
        }

        private void Action()
        {
            int choice = _rand.Next() % 20;

            switch (choice)
            {
                case 0:
                    CreateProxy();
                    break;

                case 1:
                    DestroyProxy();
                    break;

                default:
                    MoveProxy();
                    break;
            }
        }

        private void Query()
        {
            _tree.Query(QueryCallback, ref _queryAABB);

            for (int i = 0; i < _actorCount; ++i)
            {
                if (_actors[i].proxyId == DynamicTree<Actor>.NullNode)
                    continue;

                bool overlap = AABB.TestOverlap(ref _queryAABB, ref _actors[i].aabb);

                //B2_NOT_USED(overlap);
                Debug.Assert(overlap == _actors[i].overlap);
            }
        }

        private void RayCast()
        {
            _rayActor = null;

            RayCastInput input = _rayCastInput;

            // Ray cast against the dynamic tree.
            _tree.RayCast(RayCastCallback, ref input);

            // Brute force ray cast.
            Actor bruteActor = null;
            RayCastOutput bruteOutput = default;
            for (int i = 0; i < _actorCount; ++i)
            {
                if (_actors[i].proxyId == DynamicTree<Actor>.NullNode)
                    continue;

                RayCastOutput output;
                bool hit = _actors[i].aabb.RayCast(ref input, out output);
                if (hit)
                {
                    bruteActor = _actors[i];
                    bruteOutput = output;
                    input.MaxFraction = output.Fraction;
                }
            }

            if (bruteActor != null)
                Debug.Assert(bruteOutput.Fraction == _rayCastOutput.Fraction);
        }
    }
}