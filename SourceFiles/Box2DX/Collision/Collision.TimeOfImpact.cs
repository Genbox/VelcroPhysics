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

using Box2DX.Common;

namespace Box2DX.Collision
{
	public partial class Collision
	{
		// This algorithm uses conservative advancement to compute the time of
		// impact (TOI) of two shapes.
		// Refs: Bullet, Young Kim
		/// <summary>
		/// Compute the time when two shapes begin to touch or touch at a closer distance.
		/// warning the sweeps must have the same time interval.
		/// </summary>
		/// <param name="shape1"></param>
		/// <param name="sweep1"></param>
		/// <param name="shape2"></param>
		/// <param name="sweep2"></param>
		/// <returns>
		/// The fraction between [0,1] in which the shapes first touch.
		/// fraction=0 means the shapes begin touching/overlapped, and fraction=1 means the shapes don't touch.
		/// </returns>
#warning: "check params"
		public static float TimeOfImpact(Shape shape1, Sweep sweep1, Shape shape2, Sweep sweep2)
		{
			float r1 = shape1.GetSweepRadius();
			float r2 = shape2.GetSweepRadius();

			Box2DXDebug.Assert(sweep1.T0 == sweep2.T0);
			Box2DXDebug.Assert(1.0f - sweep1.T0 > Common.Settings.FLT_EPSILON);

			float t0 = sweep1.T0;
			Vec2 v1 = sweep1.C - sweep1.C0;
			Vec2 v2 = sweep2.C - sweep2.C0;
			float omega1 = sweep1.A - sweep1.A0;
			float omega2 = sweep2.A - sweep2.A0;

			float alpha = 0.0f;

			Vec2 p1, p2;
			int k_maxIterations = 20;	// TODO_ERIN b2Settings
			int iter = 0;
			Vec2 normal = Vec2.Zero;
			float distance = 0.0f;
			float targetDistance = 0.0f;

			for (; ; )
			{
				float t = (1.0f - alpha) * t0 + alpha;
				XForm xf1, xf2;
				sweep1.GetXForm(out xf1, t);
				sweep2.GetXForm(out xf2, t);

				// Get the distance between shapes.
				distance = Collision.Distance(out p1, out p2, shape1, xf1, shape2, xf2);

				if (iter == 0)
				{
					// Compute a reasonable target distance to give some breathing room
					// for conservative advancement.
					if (distance > 2.0f * Settings.ToiSlop)
					{
						targetDistance = 1.5f * Settings.ToiSlop;
					}
					else
					{
						targetDistance = Common.Math.Max(0.05f * Settings.ToiSlop, distance - 0.5f * Settings.ToiSlop);
					}
				}

				if (distance - targetDistance < 0.05f * Settings.ToiSlop || iter == k_maxIterations)
				{
					break;
				}

				normal = p2 - p1;
				normal.Normalize();

				// Compute upper bound on remaining movement.
				float approachVelocityBound = Vec2.Dot(normal, v1 - v2) +
					Common.Math.Abs(omega1) * r1 + Common.Math.Abs(omega2) * r2;
				if (Common.Math.Abs(approachVelocityBound) < Common.Settings.FLT_EPSILON)
				{
					alpha = 1.0f;
					break;
				}

				// Get the conservative time increment. Don't advance all the way.
				float dAlpha = (distance - targetDistance) / approachVelocityBound;
				//float dt = (distance - 0.5f * Settings.LinearSlop) / approachVelocityBound;
				float newAlpha = alpha + dAlpha;

				// The shapes may be moving apart or a safe distance apart.
				if (newAlpha < 0.0f || 1.0f < newAlpha)
				{
					alpha = 1.0f;
					break;
				}

				// Ensure significant advancement.
				if (newAlpha < (1.0f + 100.0f * Common.Settings.FLT_EPSILON) * alpha)
				{
					break;
				}

				alpha = newAlpha;

				++iter;
			}

			return alpha;
		}
	}
}