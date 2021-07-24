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

using System.Diagnostics;
using Genbox.VelcroPhysics.Collision.Broadphase;
using Genbox.VelcroPhysics.Collision.ContactSystem;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    /// <summary>This stress tests the dynamic tree broad-phase. This also shows that tile based collision is _not_ smooth due
    /// to Box2D not knowing about adjacency.</summary>
    internal class TilesTest : Test
    {
        private const int _count = 20;
        private readonly int _fixtureCount;
        private readonly float _createTime;

        private TilesTest()
        {
            _fixtureCount = 0;
            Stopwatch timer = Stopwatch.StartNew();

            {
                float a = 0.5f;
                BodyDef bd = new BodyDef();
                bd.Position = new Vector2(0, -a);
                Body ground = BodyFactory.CreateFromDef(World, bd);

#if true
                int N = 200;
                int M = 10;
                Vector2 position;
                position.Y = 0.0f;
                for (int j = 0; j < M; ++j)
                {
                    position.X = -N * a;
                    for (int i = 0; i < N; ++i)
                    {
                        PolygonShape shape = new PolygonShape(0.0f);
                        shape.SetAsBox(a, a, position, 0.0f);
                        ground.AddFixture(shape);
                        ++_fixtureCount;
                        position.X += 2.0f * a;
                    }

                    position.Y -= 2.0f * a;
                }
#else
			int N = 200;
			int M = 10;
			b2Vec2 position;
			position.x = -N * a;
			for (int i = 0; i < N; ++i)
			{
				position.y = 0.0f;
				for (int j = 0; j < M; ++j)
				{
					PolygonShape shape = new PolygonShape();
					shape.SetAsBox(a, a, position, 0.0f);
					ground.CreateFixture(shape, 0.0f);
					position.y -= 2.0f * a;
				}
				position.x += 2.0f * a;
			}
#endif
            }

            {
                float a = 0.5f;
                PolygonShape shape = new PolygonShape(5.0f);
                shape.SetAsBox(a, a);

                Vector2 x = new Vector2(-7.0f, 0.75f);
                Vector2 y;
                Vector2 deltaX = new Vector2(0.5625f, 1.25f);
                Vector2 deltaY = new Vector2(1.125f, 0.0f);

                for (int i = 0; i < _count; ++i)
                {
                    y = x;

                    for (int j = i; j < _count; ++j)
                    {
                        BodyDef bd = new BodyDef();
                        bd.Type = BodyType.Dynamic;
                        bd.Position = y;

                        //if (i == 0 && j == 0)
                        //{
                        //	bd.AllowSleep = false;
                        //}
                        //else
                        //{
                        //	bd.AllowSleep = true;
                        //}

                        Body body = BodyFactory.CreateFromDef(World, bd);
                        body.AddFixture(shape);
                        ++_fixtureCount;
                        y += deltaY;
                    }

                    x += deltaX;
                }
            }

            _createTime = timer.ElapsedMilliseconds;
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            ContactManager cm = World.ContactManager;
            DynamicTreeBroadPhase broadPhase = (DynamicTreeBroadPhase)cm.BroadPhase;
            int height = broadPhase.TreeHeight;
            int leafCount = broadPhase.ProxyCount;
            int minimumNodeCount = 2 * leafCount - 1;
            float minimumHeight = MathUtils.Ceil(MathUtils.Log(minimumNodeCount) / MathUtils.Log(2.0f));
            DrawString($"dynamic tree height = {height}, min = {(int)minimumHeight}");

            base.Update(settings, gameTime);

            DrawString($"create time = {_createTime} ms, fixture count = {_fixtureCount}");

            //b2DynamicTree* tree = &World.m_contactManager.m_broadPhase.m_tree;

            //if (m_stepCount == 400)
            //{
            //	tree.RebuildBottomUp();
            //}
        }

        internal static Test Create()
        {
            return new TilesTest();
        }
    }
}