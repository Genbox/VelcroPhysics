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
	/// <summary>
	/// This structure is used to build circle shapes.
	/// </summary>
	public class CircleDef : ShapeDef
	{
		public Vec2 LocalPosition;
		public float Radius;

		public CircleDef()
		{
			Type = ShapeType.CircleShape;
			LocalPosition = Vec2.Zero;
			Radius = 1.0f;
		}
	}

	/// <summary>
	/// A circle shape.
	/// </summary>
	public class CircleShape : Shape
	{
		// Local position in parent body
		private Vec2 _localPosition;
		// Radius of this circle.
		private float _radius;

		internal CircleShape(ShapeDef def)
			: base(def)
		{
			Box2DXDebug.Assert(def.Type == ShapeType.CircleShape);
			CircleDef circleDef = (CircleDef)def;

			_type = ShapeType.CircleShape;
			_localPosition = circleDef.LocalPosition;
			_radius = circleDef.Radius;
		}

		internal override void UpdateSweepRadius(Vec2 center)
		{
			// Update the sweep radius (maximum radius) as measured from
			// a local center point.
			Vec2 d = _localPosition - center;
			_sweepRadius = d.Length() + _radius - Settings.ToiSlop;
		}

		public override bool TestPoint(XForm transform, Vec2 p)
		{
			Vec2 center = transform.Position + Common.Math.Mul(transform.R, _localPosition);
			Vec2 d = p - center;
			return Vec2.Dot(d, d) <= _radius * _radius;
		}

		// Collision Detection in Interactive 3D Environments by Gino van den Bergen
		// From Section 3.1.2
		// x = s + a * r
		// norm(x) = radius
		public override SegmentCollide TestSegment(XForm transform, out float lambda, out Vec2 normal, Segment segment, float maxLambda)
		{
			lambda = 0f;
			normal = Vec2.Zero;

			Vec2 position = transform.Position + Common.Math.Mul(transform.R, _localPosition);
			Vec2 s = segment.P1 - position;
			float b = Vec2.Dot(s, s) - _radius * _radius;

			// Does the segment start inside the circle?
			if (b < 0.0f)
			{
				lambda = 0f;
				return SegmentCollide.StartInsideCollide;
			}

			// Solve quadratic equation.
			Vec2 r = segment.P2 - segment.P1;
			float c = Vec2.Dot(s, r);
			float rr = Vec2.Dot(r, r);
			float sigma = c * c - rr * b;

			// Check for negative discriminant and short segment.
			if (sigma < 0.0f || rr < Common.Settings.FLT_EPSILON)
			{
				return SegmentCollide.MissCollide;
			}

			// Find the point of intersection of the line with the circle.
			float a = -(c + Common.Math.Sqrt(sigma));

			// Is the intersection point on the segment?
			if (0.0f <= a && a <= maxLambda * rr)
			{
				a /= rr;
				lambda = a;
				normal = s + a * r;
				normal.Normalize();
				return SegmentCollide.HitCollide;
			}

			return SegmentCollide.MissCollide;
		}

		public override void ComputeAABB(out AABB aabb, XForm transform)
		{
			aabb = new AABB();

			Vec2 p = transform.Position + Common.Math.Mul(transform.R, _localPosition);
			aabb.LowerBound.Set(p.X - _radius, p.Y - _radius);
			aabb.UpperBound.Set(p.X + _radius, p.Y + _radius);
		}

		public override void ComputeSweptAABB(out AABB aabb, XForm transform1, XForm transform2)
		{
			aabb = new AABB();

			Vec2 p1 = transform1.Position + Common.Math.Mul(transform1.R, _localPosition);
			Vec2 p2 = transform2.Position + Common.Math.Mul(transform2.R, _localPosition);
			Vec2 lower = Common.Math.Min(p1, p2);
			Vec2 upper = Common.Math.Max(p1, p2);

			aabb.LowerBound.Set(lower.X - _radius, lower.Y - _radius);
			aabb.UpperBound.Set(upper.X + _radius, upper.Y + _radius);
		}

		public override void ComputeMass(out MassData massData)
		{
			massData = new MassData();

			massData.Mass = _density * Settings.Pi * _radius * _radius;
			massData.Center = _localPosition;

			// inertia about the local origin
			massData.I = massData.Mass * (0.5f * _radius * _radius + Vec2.Dot(_localPosition, _localPosition));
		}

		public override float ComputeSubmergedArea(Vec2 normal, float offset, XForm xf, out Vec2 c)
		{
			Vec2 p = Box2DX.Common.Math.Mul(xf, _localPosition);
			float l = -(Vec2.Dot(normal, p) - offset);
			if (l < -_radius + Box2DX.Common.Settings.FLT_EPSILON)
			{
				//Completely dry
				c = new Vec2();
				return 0;
			}
			if (l > _radius)
			{
				//Completely wet
				c = p;
				return Box2DX.Common.Settings.Pi * _radius * _radius;
			}

			//Magic
			float r2 = _radius * _radius;
			float l2 = l * l;
			float area = r2 * ((float)System.Math.Asin(l / _radius) + Box2DX.Common.Settings.Pi / 2) +
				l * Box2DX.Common.Math.Sqrt(r2 - l2);
			float com = -2.0f / 3.0f * (float)System.Math.Pow(r2 - l2, 1.5f) / area;

			c.X = p.X + normal.X * com;
			c.Y = p.Y + normal.Y * com;

			return area;
		}

		/// <summary>
		/// Get the local position of this circle in its parent body.
		/// </summary>
		/// <returns></returns>
		public Vec2 GetLocalPosition()
		{
			return _localPosition;
		}

		/// <summary>
		/// Get the radius of this circle.
		/// </summary>
		/// <returns></returns>
		public float GetRadius()
		{
			return _radius;
		}
	}
}