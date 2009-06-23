/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2007 Erin Catto http://www.gphysics.com

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections.Generic;
using System.Text;

using Box2DX;
using Box2DX.Common;
using Box2DX.Collision;
using Box2DX.Dynamics;

namespace TestBed
{
	using Box2DXMath = Box2DX.Common.Math;
	using SystemMath = System.Math;

	public class Actor
	{
		public AABB aabb;
		public int overlapCount;
		public ushort proxyId;
	}

	public class Callback : PairCallback
	{
		public override object PairAdded(object proxyUserData1, object proxyUserData2)
		{
			Actor actor1 = proxyUserData1 as Actor;
			Actor actor2 = proxyUserData2 as Actor;

			int id1 = Array.IndexOf(_test._actors, actor1);
			int id2 = Array.IndexOf(_test._actors, actor2);
			Box2DXDebug.Assert(id1 < BroadPhaseTest.k_actorCount);
			Box2DXDebug.Assert(id2 < BroadPhaseTest.k_actorCount);

			Box2DXDebug.Assert(_test._overlaps[id1][id2] == false);
			_test._overlaps[id1][id2] = true;
			_test._overlaps[id2][id1] = true;
			++_test._overlapCount;

			++actor1.overlapCount;
			++actor2.overlapCount;

			return null;
		}

		public override void PairRemoved(object proxyUserData1, object proxyUserData2, object pairUserData)
		{
			//B2_NOT_USED(pairUserData);

			Actor actor1 = proxyUserData1 as Actor;
			Actor actor2 = proxyUserData2 as Actor;

			// The pair may have been removed by destroying a proxy.
			int id1 = Array.IndexOf(_test._actors, actor1);
			int id2 = Array.IndexOf(_test._actors, actor2);
			Box2DXDebug.Assert(id1 < BroadPhaseTest.k_actorCount);
			Box2DXDebug.Assert(id2 < BroadPhaseTest.k_actorCount);

			_test._overlaps[id1][id2] = false;
			_test._overlaps[id2][id1] = false;
			--_test._overlapCount;

			--actor1.overlapCount;
			--actor2.overlapCount;
		}

		public BroadPhaseTest _test;
	}

	public class BroadPhaseTest : Test
	{
		public static readonly int k_actorCount = 256;
		public static readonly float k_extent = 15.0f;
		public static readonly float k_width = 1.0f;

		internal int _overlapCount;
		private int _overlapCountExact;

		private Callback _callback = new Callback();
		private BroadPhase _broadPhase;
		internal Actor[] _actors = new Actor[k_actorCount];
		internal bool[][] _overlaps = new bool[k_actorCount][/*k_actorCount*/];
		private bool _automated;
		private int _stepCount;

		public static Test Create()
		{
			return new BroadPhaseTest();
		}

		public BroadPhaseTest()
		{
			BroadPhase.IsValidate = true;

			//srand(888);

			AABB worldAABB = new AABB();
			worldAABB.LowerBound.Set(-5.0f * k_extent, -5.0f * k_extent);
			worldAABB.UpperBound.Set(5.0f * k_extent, 5.0f * k_extent);

			_overlapCount = 0;
			_overlapCountExact = 0;
			_callback._test = this;

			_broadPhase = new BroadPhase(worldAABB, _callback);

			for (int i = 0; i < k_actorCount; i++)
				_overlaps[i] = new bool[k_actorCount];
			for (int i = 0; i < k_actorCount; i++)
				for (int j = 0; j < k_actorCount; j++)
					_overlaps[i][j] = false;

			for (int i = 0; i < k_actorCount; i++)
				_actors[i] = new Actor();

			for (int i = 0; i < k_actorCount; ++i)
			{
				bool s = false;
				if (i == 91)
					s = true;
				Actor actor = _actors[i];
				GetRandomAABB(ref actor.aabb);
				//actor->aabb.minVertex.Set(0.0f, 0.0f);
				//actor->aabb.maxVertex.Set(k_width, k_width);
				actor.proxyId = _broadPhase.CreateProxy(actor.aabb, actor);
				actor.overlapCount = 0;
				_broadPhase.Validate();
			}

			_automated = true;
			_stepCount = 0;
		}

		protected override void Dispose(bool state)
		{
			if (state)
			{
				BroadPhase.IsValidate = false;
				_broadPhase = null;
			}
		}

		public float GetExtent() { return 1.5f * k_extent; }

		public override void Step(Settings settings)
		{
			//B2_NOT_USED(settings);

			if (_automated == true)
			{
				int actionCount = Box2DXMath.Max(1, k_actorCount >> 2);

				for (int i = 0; i < actionCount; ++i)
				{
					Action();
				}
			}

			_broadPhase.Commit();

			for (int i = 0; i < k_actorCount; ++i)
			{
				Actor actor = _actors[i];
				if (actor.proxyId == PairManager.NullProxy)
					continue;

				Color c = new Color();
				switch (actor.overlapCount)
				{
					case 0:
						c.R = 0.9f; c.G = 0.9f; c.B = 0.9f;
						break;

					case 1:
						c.R = 0.6f; c.G = 0.9f; c.B = 0.6f;
						break;

					default:
						c.R = 0.9f; c.G = 0.6f; c.B = 0.6f;
						break;
				}

				OpenGLDebugDraw.DrawAABB(actor.aabb, c);
			}

			StringBuilder strBld = new StringBuilder();
			strBld.AppendFormat("overlaps = {0}, exact = {1}, diff = {2}",
				new object[] { _overlapCount, _overlapCountExact, _overlapCount - _overlapCountExact });
			OpenGLDebugDraw.DrawString(5, 30, strBld.ToString());
			Validate();

			++_stepCount;
		}

		public override void Keyboard(System.Windows.Forms.Keys key)
		{
			switch (key)
			{
				case System.Windows.Forms.Keys.A:
					_automated = !_automated;
					break;

				case System.Windows.Forms.Keys.C:
					CreateProxy();
					break;

				case System.Windows.Forms.Keys.D:
					DestroyProxy();
					break;

				case System.Windows.Forms.Keys.M:
					MoveProxy();
					break;
			}
		}

		private void GetRandomAABB(ref AABB aabb)
		{
			Vec2 w = new Vec2(); w.Set(k_width, k_width);
			aabb.LowerBound.X = Box2DXMath.Random(-k_extent, k_extent);
			aabb.LowerBound.Y = Box2DXMath.Random(0.0f, 2.0f * k_extent);
			aabb.UpperBound = aabb.LowerBound + w;
		}

		private void MoveAABB(ref AABB aabb)
		{
			Vec2 d = new Vec2();
			d.X = Box2DXMath.Random(-0.5f, 0.5f);
			d.Y = Box2DXMath.Random(-0.5f, 0.5f);
			//d.x = 2.0f;
			//d.y = 0.0f;
			aabb.LowerBound += d;
			aabb.UpperBound += d;

			Vec2 c0 = 0.5f * (aabb.LowerBound + aabb.UpperBound);
			Vec2 min = new Vec2(); min.Set(-k_extent, 0.0f);
			Vec2 max = new Vec2(); max.Set(k_extent, 2.0f * k_extent);
			Vec2 c = Box2DXMath.Clamp(c0, min, max);

			aabb.LowerBound += c - c0;
			aabb.UpperBound += c - c0;
		}

		private void CreateProxy()
		{
			Random rnd = new Random(888);
			for (int i = 0; i < k_actorCount; ++i)
			{
				int j = (rnd.Next() % k_actorCount);
				Actor actor = _actors[j];
				if (actor.proxyId == PairManager.NullProxy)
				{
					actor.overlapCount = 0;
					GetRandomAABB(ref actor.aabb);
					actor.proxyId = _broadPhase.CreateProxy(actor.aabb, actor);
					return;
				}
			}
		}
		private void DestroyProxy()
		{
			Random rnd = new Random(888);
			for (int i = 0; i < k_actorCount; ++i)
			{
				int j = (rnd.Next() % k_actorCount);
				Actor actor = _actors[j];
				if (actor.proxyId != PairManager.NullProxy)
				{
					_broadPhase.DestroyProxy(actor.proxyId);
					actor.proxyId = PairManager.NullProxy;
					actor.overlapCount = 0;
					return;
				}
			}
		}

		private void MoveProxy()
		{
			Random rnd = new Random(888);
			for (int i = 0; i < k_actorCount; ++i)
			{
				int j = (rnd.Next() % k_actorCount);
				//int32 j = 1;
				Actor actor = _actors[j];
				if (actor.proxyId == PairManager.NullProxy)
				{
					continue;
				}

				MoveAABB(ref actor.aabb);
				_broadPhase.MoveProxy(actor.proxyId, actor.aabb);
				return;
			}
		}

		private void Action()
		{
			int choice = (int)Box2DXMath.Random(0,2);

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

		private void Validate()
		{
			_broadPhase.Validate();

			_overlapCountExact = 0;

			for (int i = 0; i < k_actorCount; ++i)
			{
				Actor actor1 = _actors[i];
				if (actor1.proxyId == PairManager.NullProxy)
					continue;

				for (int j = i + 1; j < k_actorCount; ++j)
				{
					Actor actor2 = _actors[j];
					if (actor2.proxyId == PairManager.NullProxy)
						continue;

					bool overlap = Collision.TestOverlap(actor1.aabb, actor2.aabb);
					if (overlap) ++_overlapCountExact;

					if (overlap)
					{
						Box2DXDebug.Assert(_overlaps[Array.IndexOf(_actors, actor1)][Array.IndexOf(_actors, actor2)] == true);
					}
				}
			}

			Box2DXDebug.Assert(_overlapCount >= _overlapCountExact);
		}
	}
}
