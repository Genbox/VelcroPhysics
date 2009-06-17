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
		public static int GJKIterations = 0;

		// GJK using Voronoi regions (Christer Ericson) and region selection
		// optimizations (Casey Muratori).

		// The origin is either in the region of points[1] or in the edge region. The origin is
		// not in region of points[0] because that is the old point.
		public static int ProcessTwo(out Vec2 x1, out Vec2 x2, ref Vec2[] p1s, ref Vec2[] p2s,
			ref Vec2[] points)
		{
			// If in point[1] region
			Vec2 r = -points[1];
			Vec2 d = points[0] - points[1];
			float length = d.Normalize();
			float lambda = Vec2.Dot(r, d);
			if (lambda <= 0.0f || length < Common.Settings.FLT_EPSILON)
			{
				// The simplex is reduced to a point.
				x1 = p1s[1];
				x2 = p2s[1];
				p1s[0] = p1s[1];
				p2s[0] = p2s[1];
				points[0] = points[1];
				return 1;
			}

			// Else in edge region
			lambda /= length;
			x1 = p1s[1] + lambda * (p1s[0] - p1s[1]);
			x2 = p2s[1] + lambda * (p2s[0] - p2s[1]);
			return 2;
		}

		// Possible regions:
		// - points[2]
		// - edge points[0]-points[2]
		// - edge points[1]-points[2]
		// - inside the triangle
		public static int ProcessThree(out Vec2 x1, out Vec2 x2, ref Vec2[] p1s, ref Vec2[] p2s,
			ref Vec2[] points)
		{
			Vec2 a = points[0];
			Vec2 b = points[1];
			Vec2 c = points[2];

			Vec2 ab = b - a;
			Vec2 ac = c - a;
			Vec2 bc = c - b;

			float sn = -Vec2.Dot(a, ab), sd = Vec2.Dot(b, ab);
			float tn = -Vec2.Dot(a, ac), td = Vec2.Dot(c, ac);
			float un = -Vec2.Dot(b, bc), ud = Vec2.Dot(c, bc);

			// In vertex c region?
			if (td <= 0.0f && ud <= 0.0f)
			{
				// Single point
				x1 = p1s[2];
				x2 = p2s[2];
				p1s[0] = p1s[2];
				p2s[0] = p2s[2];
				points[0] = points[2];
				return 1;
			}

			// Should not be in vertex a or b region.

			//B2_NOT_USED(sd);
			//B2_NOT_USED(sn);			
			Box2DXDebug.Assert(sn > 0.0f || tn > 0.0f);
			Box2DXDebug.Assert(sd > 0.0f || un > 0.0f);

			float n = Vec2.Cross(ab, ac);

#if TARGET_FLOAT32_IS_FIXED
				n = (n < 0.0f) ? -1.0f : ((n > 0.0f) ? 1.0f : 0.0f);
#endif

			// Should not be in edge ab region.
			float vc = n * Vec2.Cross(a, b);
			Box2DXDebug.Assert(vc > 0.0f || sn > 0.0f || sd > 0.0f);

			// In edge bc region?
			float va = n * Vec2.Cross(b, c);
			if (va <= 0.0f && un >= 0.0f && ud >= 0.0f && (un + ud) > 0.0f)
			{
				Box2DXDebug.Assert(un + ud > 0.0f);
				float lambda = un / (un + ud);
				x1 = p1s[1] + lambda * (p1s[2] - p1s[1]);
				x2 = p2s[1] + lambda * (p2s[2] - p2s[1]);
				p1s[0] = p1s[2];
				p2s[0] = p2s[2];
				points[0] = points[2];
				return 2;
			}

			// In edge ac region?
			float vb = n * Vec2.Cross(c, a);
			if (vb <= 0.0f && tn >= 0.0f && td >= 0.0f && (tn + td) > 0.0f)
			{
				Box2DXDebug.Assert(tn + td > 0.0f);
				float lambda = tn / (tn + td);
				x1 = p1s[0] + lambda * (p1s[2] - p1s[0]);
				x2 = p2s[0] + lambda * (p2s[2] - p2s[0]);
				p1s[1] = p1s[2];
				p2s[1] = p2s[2];
				points[1] = points[2];
				return 2;
			}

			// Inside the triangle, compute barycentric coordinates
			float denom = va + vb + vc;
			Box2DXDebug.Assert(denom > 0.0f);
			denom = 1.0f / denom;
#if TARGET_FLOAT32_IS_FIXED
			x1 = denom * (va * p1s[0] + vb * p1s[1] + vc * p1s[2]);
			x2 = denom * (va * p2s[0] + vb * p2s[1] + vc * p2s[2]);
#else
			float u = va * denom;
			float v = vb * denom;
			float w = 1.0f - u - v;
			x1 = u * p1s[0] + v * p1s[1] + w * p1s[2];
			x2 = u * p2s[0] + v * p2s[1] + w * p2s[2];
#endif
			return 3;
		}

		public static bool InPoints(Vec2 w, Vec2[] points, int pointCount)
		{
			float k_tolerance = 100.0f * Common.Settings.FLT_EPSILON;
			for (int i = 0; i < pointCount; ++i)
			{
				Vec2 d = Common.Math.Abs(w - points[i]);
				Vec2 m = Common.Math.Max(Common.Math.Abs(w), Common.Math.Abs(points[i]));

				if (d.X < k_tolerance * (m.X + 1.0f) &&
					d.Y < k_tolerance * (m.Y + 1.0f))
				{
					return true;
				}
			}

			return false;
		}

		public interface IGenericShape
		{
			Vec2 Support(XForm xf, Vec2 v);
			Vec2 GetFirstVertex(XForm xf);
		}

		public static float DistanceGeneric<T1, T2>(out Vec2 x1, out Vec2 x2,
						   T1 shape1_, XForm xf1, T2 shape2_, XForm xf2)
		{
			IGenericShape shape1 = shape1_ as IGenericShape;
			IGenericShape shape2 = shape2_ as IGenericShape;

			if (shape1 == null || shape2 == null)
				Box2DXDebug.Assert(false, "Can not cast some parameters to IGenericShape");

			Vec2[] p1s = new Vec2[3], p2s = new Vec2[3];
			Vec2[] points = new Vec2[3];
			int pointCount = 0;

			x1 = shape1.GetFirstVertex(xf1);
			x2 = shape2.GetFirstVertex(xf2);

			float vSqr = 0.0f;
			int maxIterations = 20;

			for (int iter = 0; iter < maxIterations; ++iter)
			{
				Vec2 v = x2 - x1;
				Vec2 w1 = shape1.Support(xf1, v);
				Vec2 w2 = shape2.Support(xf2, -v);

				vSqr = Vec2.Dot(v, v);
				Vec2 w = w2 - w1;
				float vw = Vec2.Dot(v, w);
				if (vSqr - vw <= 0.01f * vSqr || Collision.InPoints(w, points, pointCount)) // or w in points
				{
					if (pointCount == 0)
					{
						x1 = w1;
						x2 = w2;
					}
					Collision.GJKIterations = iter;
					return Common.Math.Sqrt(vSqr);
				}

				switch (pointCount)
				{
					case 0:
						p1s[0] = w1;
						p2s[0] = w2;
						points[0] = w;
						x1 = p1s[0];
						x2 = p2s[0];
						++pointCount;
						break;

					case 1:
						p1s[1] = w1;
						p2s[1] = w2;
						points[1] = w;
						pointCount = Collision.ProcessTwo(out x1, out x2, ref p1s, ref p2s, ref points);
						break;

					case 2:
						p1s[2] = w1;
						p2s[2] = w2;
						points[2] = w;
						pointCount = Collision.ProcessThree(out x1, out x2, ref p1s, ref p2s, ref points);
						break;
				}

				// If we have three points, then the origin is in the corresponding triangle.
				if (pointCount == 3)
				{
					Collision.GJKIterations = iter;
					return 0.0f;
				}

				float maxSqr = -Common.Settings.FLT_MAX;
				for (int i = 0; i < pointCount; ++i)
				{
					maxSqr = Common.Math.Max(maxSqr, Vec2.Dot(points[i], points[i]));
				}

#if TARGET_FLOAT32_IS_FIXED
				if (pointCount == 3 || vSqr <= 5.0*Common.Settings.FLT_EPSILON * maxSqr)
#else
				if (vSqr <= 100.0f * Common.Settings.FLT_EPSILON * maxSqr)
#endif
				{
					Collision.GJKIterations = iter;
					v = x2 - x1;
					vSqr = Vec2.Dot(v, v);

					return Common.Math.Sqrt(vSqr);
				}
			}

			Collision.GJKIterations = maxIterations;
			return Common.Math.Sqrt(vSqr);
		}

		public static float DistanceCC(out Vec2 x1, out Vec2 x2,
			CircleShape circle1, XForm xf1, CircleShape circle2, XForm xf2)
		{
			Vec2 p1 = Common.Math.Mul(xf1, circle1.GetLocalPosition());
			Vec2 p2 = Common.Math.Mul(xf2, circle2.GetLocalPosition());

			Vec2 d = p2 - p1;
			float dSqr = Vec2.Dot(d, d);
			float r1 = circle1.GetRadius() - Settings.ToiSlop;
			float r2 = circle2.GetRadius() - Settings.ToiSlop;
			float r = r1 + r2;
			if (dSqr > r * r)
			{
				float dLen = d.Normalize();
				float distance = dLen - r;
				x1 = p1 + r1 * d;
				x2 = p2 - r2 * d;
				return distance;
			}
			else if (dSqr > Common.Settings.FLT_EPSILON * Common.Settings.FLT_EPSILON)
			{
				d.Normalize();
				x1 = p1 + r1 * d;
				x2 = x1;
				return 0.0f;
			}

			x1 = p1;
			x2 = x1;
			return 0.0f;
		}

#warning "CAS"
		// This is used for polygon-vs-circle distance.
		public class Point : Collision.IGenericShape
		{
			public Vec2 p;

			public Vec2 Support(XForm xf, Vec2 v)
			{
				return p;
			}

			public Vec2 GetFirstVertex(XForm xf)
			{
				return p;
			}
		}

		// GJK is more robust with polygon-vs-point than polygon-vs-circle.
		// So we convert polygon-vs-circle to polygon-vs-point.
		public static float DistancePC(out Vec2 x1, out Vec2 x2,
			PolygonShape polygon, XForm xf1, CircleShape circle, XForm xf2)
		{
			Point point = new Point();
			point.p = Common.Math.Mul(xf2, circle.GetLocalPosition());

			float distance = DistanceGeneric<PolygonShape, Point>(out x1, out x2, polygon, xf1, point, XForm.Identity);

			float r = circle.GetRadius() - Settings.ToiSlop;

			if (distance > r)
			{
				distance -= r;
				Vec2 d = x2 - x1;
				d.Normalize();
				x2 -= r * d;
			}
			else
			{
				distance = 0.0f;
				x2 = x1;
			}

			return distance;
		}

		public static float Distance(out Vec2 x1, out Vec2 x2,
			Shape shape1, XForm xf1, Shape shape2, XForm xf2)
		{
			x1 = new Vec2();
			x2 = new Vec2();

			ShapeType type1 = shape1.GetType();
			ShapeType type2 = shape2.GetType();

			if (type1 == ShapeType.CircleShape && type2 == ShapeType.CircleShape)
			{
				return DistanceCC(out x1, out x2, (CircleShape)shape1, xf1, (CircleShape)shape2, xf2);
			}

			if (type1 == ShapeType.PolygonShape && type2 == ShapeType.CircleShape)
			{
				return DistancePC(out x1, out x2, (PolygonShape)shape1, xf1, (CircleShape)shape2, xf2);
			}

			if (type1 == ShapeType.CircleShape && type2 == ShapeType.PolygonShape)
			{
				return DistancePC(out x2, out x1, (PolygonShape)shape2, xf2, (CircleShape)shape1, xf1);
			}

			if (type1 == ShapeType.PolygonShape && type2 == ShapeType.PolygonShape)
			{
				return DistanceGeneric(out x1, out x2, (PolygonShape)shape1, xf1, (PolygonShape)shape2, xf2);
			}

			return 0.0f;
		}
	}
}