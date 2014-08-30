/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
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
using FarseerPhysics.Collision;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Testbed.Tests
{
    public class DynamicTreeTest : Test
    {
        private const int ActorCount = 128;
        private Actor[] _actors = new Actor[ActorCount];
        private bool _automated;
        private float _proxyExtent;
        private AABB _queryAABB;
        private Actor _rayActor = new Actor();
        private RayCastInput _rayCastInput;
        private RayCastOutput _rayCastOutput;
        private DynamicTree<Actor> _tree = new DynamicTree<Actor>();
        private float _worldExtent;

        private DynamicTreeTest()
        {
            _worldExtent = 15.0f;
            _proxyExtent = 0.5f;

            Rand.Random = new Random(888);

            for (int i = 0; i < ActorCount; ++i)
            {
                _actors[i] = new Actor();

                Actor actor = _actors[i];
                GetRandomAABB(out actor.AABB);
                actor.ProxyId = _tree.AddProxy(ref actor.AABB, actor);
            }

            float h = _worldExtent;
            _queryAABB.LowerBound = new Vector2(-3.0f, -4.0f + h);
            _queryAABB.UpperBound = new Vector2(5.0f, 6.0f + h);

            _rayCastInput.Point1 = new Vector2(-5.0f, 5.0f + h);
            _rayCastInput.Point2 = new Vector2(7.0f, -4.0f + h);
            //_rayCastInput.p1 = new Vector2(0.0f, 2.0f + h);
            //_rayCastInput.p2 = new Vector2(0.0f, -2.0f + h);
            _rayCastInput.MaxFraction = 1.0f;

            _automated = false;
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            _rayActor = null;
            for (int i = 0; i < ActorCount; ++i)
            {
                _actors[i].Fraction = 1.0f;
                _actors[i].Overlap = false;
            }

            if (_automated)
            {
                int actionCount = Math.Max(1, ActorCount >> 2);

                for (int i = 0; i < actionCount; ++i)
                {
                    Action();
                }
            }

            Query();
            RayCast();

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
            for (int i = 0; i < ActorCount; ++i)
            {
                Actor actor = _actors[i];
                if (actor.ProxyId == -1)
                    continue;

                Color ca = new Color(0.9f, 0.9f, 0.9f);
                if (actor == _rayActor && actor.Overlap)
                {
                    ca = new Color(0.9f, 0.6f, 0.6f);
                }
                else if (actor == _rayActor)
                {
                    ca = new Color(0.6f, 0.9f, 0.6f);
                }
                else if (actor.Overlap)
                {
                    ca = new Color(0.6f, 0.6f, 0.9f);
                }

                DebugView.DrawAABB(ref actor.AABB, ca);
            }

            Color c = new Color(0.7f, 0.7f, 0.7f);
            DebugView.DrawAABB(ref _queryAABB, c);

            DebugView.DrawSegment(_rayCastInput.Point1, _rayCastInput.Point2, c);

            Color c1 = new Color(0.2f, 0.9f, 0.2f);
            Color c2 = new Color(0.9f, 0.2f, 0.2f);
            DebugView.DrawPoint(_rayCastInput.Point1, 0.1f, c1);
            DebugView.DrawPoint(_rayCastInput.Point2, 0.1f, c2);

            if (_rayActor != null)
            {
                Color cr = new Color(0.2f, 0.2f, 0.9f);
                Vector2 p = _rayCastInput.Point1 + _rayActor.Fraction * (_rayCastInput.Point2 - _rayCastInput.Point1);
                DebugView.DrawPoint(p, 0.1f, cr);
            }
            DebugView.EndCustomDraw();

            int height = _tree.Height;
            DrawString("Dynamic tree height = " + height);
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.A))
            {
                _automated = !_automated;
            }
            if (keyboardManager.IsNewKeyPress(Keys.C))
            {
                CreateProxy();
            }
            if (keyboardManager.IsNewKeyPress(Keys.D))
            {
                DestroyProxy();
            }
            if (keyboardManager.IsNewKeyPress(Keys.M))
            {
                MoveProxy();
            }
        }

        private bool QueryCallback(int proxyid)
        {
            Actor actor = _tree.GetUserData(proxyid);
            actor.Overlap = AABB.TestOverlap(ref _queryAABB, ref actor.AABB);
            return true;
        }

        private float RayCastCallback(RayCastInput input, int proxyid)
        {
            Actor actor = _tree.GetUserData(proxyid);

            RayCastOutput output;
            bool hit = actor.AABB.RayCast(out output, ref input);

            if (hit)
            {
                _rayCastOutput = output;
                _rayActor = actor;
                actor.Fraction = output.Fraction;
                return output.Fraction;
            }

            return input.MaxFraction;
        }

        private void GetRandomAABB(out AABB aabb)
        {
            aabb = new AABB();

            Vector2 w = new Vector2(2.0f * _proxyExtent, 2.0f * _proxyExtent);
            //aabb.LowerBound.x = -_proxyExtent;
            //aabb.LowerBound.y = -_proxyExtent + _worldExtent;
            aabb.LowerBound.X = Rand.RandomFloat(-_worldExtent, _worldExtent);
            aabb.LowerBound.Y = Rand.RandomFloat(0.0f, 2.0f * _worldExtent);
            aabb.UpperBound = aabb.LowerBound + w;
        }

        private void MoveAABB(ref AABB aabb)
        {
            Vector2 d = Vector2.Zero;
            d.X = Rand.RandomFloat(-0.5f, 0.5f);
            d.Y = Rand.RandomFloat(-0.5f, 0.5f);
            //d.x = 2.0f;
            //d.y = 0.0f;
            aabb.LowerBound += d;
            aabb.UpperBound += d;

            Vector2 c0 = 0.5f * (aabb.LowerBound + aabb.UpperBound);
            Vector2 min = new Vector2(-_worldExtent, 0.0f);
            Vector2 max = new Vector2(_worldExtent, 2.0f * _worldExtent);
            Vector2 c = Vector2.Clamp(c0, min, max);

            aabb.LowerBound += c - c0;
            aabb.UpperBound += c - c0;
        }

        private void CreateProxy()
        {
            for (int i = 0; i < ActorCount; ++i)
            {
                int j = Rand.Random.Next() % ActorCount;
                Actor actor = _actors[j];
                if (actor.ProxyId == -1)
                {
                    GetRandomAABB(out actor.AABB);
                    actor.ProxyId = _tree.AddProxy(ref actor.AABB, actor);
                    return;
                }
            }
        }

        private void DestroyProxy()
        {
            for (int i = 0; i < ActorCount; ++i)
            {
                int j = Rand.Random.Next() % ActorCount;
                Actor actor = _actors[j];
                if (actor.ProxyId != -1)
                {
                    _tree.RemoveProxy(actor.ProxyId);
                    actor.ProxyId = -1;
                    return;
                }
            }
        }

        private void MoveProxy()
        {
            for (int i = 0; i < ActorCount; ++i)
            {
                int j = Rand.Random.Next() % ActorCount;
                Actor actor = _actors[j];
                if (actor.ProxyId == -1)
                {
                    continue;
                }

                AABB aabb0 = actor.AABB;
                MoveAABB(ref actor.AABB);
                Vector2 displacement = actor.AABB.Center - aabb0.Center;
                _tree.MoveProxy(actor.ProxyId, ref actor.AABB, displacement);
                return;
            }
        }

        private void Action()
        {
            int choice = Rand.Random.Next() % 20;

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

            for (int i = 0; i < ActorCount; ++i)
            {
                if (_actors[i].ProxyId == -1)
                {
                    continue;
                }

                bool overlap = AABB.TestOverlap(ref _queryAABB, ref _actors[i].AABB);
                Debug.Assert(overlap == _actors[i].Overlap);
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
            RayCastOutput bruteOutput = new RayCastOutput();
            for (int i = 0; i < ActorCount; ++i)
            {
                if (_actors[i].ProxyId == -1)
                {
                    continue;
                }

                RayCastOutput output;
                bool hit = _actors[i].AABB.RayCast(out output, ref input);
                if (hit)
                {
                    bruteActor = _actors[i];
                    bruteOutput = output;
                    input.MaxFraction = output.Fraction;
                }
            }

            if (bruteActor != null)
            {
                Debug.Assert(bruteOutput.Fraction == _rayCastOutput.Fraction);
            }
        }

        #region Nested type: Actor

        private sealed class Actor
        {
            internal AABB AABB;
            internal float Fraction;
            internal bool Overlap;
            internal int ProxyId;
        }

        #endregion

        internal static Test Create()
        {
            return new DynamicTreeTest();
        }
    }
}