/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com

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
using Box2DX.Common;
using Math = Box2DX.Common.Math;

namespace Box2DX.Collision
{
    /// <summary>
    /// A circle shape.
    /// </summary>
    public class CircleShape : Shape
    {
        // Local position in parent body
        public Vec2 LocalPosition;

        public CircleShape()
        {
            Type = ShapeType.CircleShape;
        }

        public override bool TestPoint(ref XForm transform, ref Vec2 p)
        {
            Vec2 center = transform.Position + Math.Mul(transform.R, LocalPosition);
            Vec2 d = p - center;
            return Vec2.Dot(d, d) <= Radius * Radius;
        }

        // Collision Detection in Interactive 3D Environments by Gino van den Bergen
        // From Section 3.1.2
        // x = s + a * r
        // norm(x) = radius
        public override SegmentCollide TestSegment(ref XForm transform, out float lambda, out Vec2 normal, ref Segment segment, float maxLambda)
        {
            // Needed in the C# version as all variables marked "out" must be inialized
            lambda = 0f;
            normal = Vec2.Zero;

            Vec2 position = transform.Position + Math.Mul(transform.R, LocalPosition);
            Vec2 s = segment.P1 - position;
            float b = Vec2.Dot(s, s) - Radius * Radius;

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
            if (sigma < 0.0f || rr < Settings.FLT_EPSILON)
            {
                return SegmentCollide.MissCollide;
            }

            // Find the point of intersection of the line with the circle.
            float a = -(c + Math.Sqrt(sigma));

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

        public override void ComputeAABB(out AABB aabb, ref XForm transform)
        {
            aabb = new AABB();

            Vec2 p = transform.Position + Math.Mul(transform.R, LocalPosition);
            aabb.LowerBound.Set(p.X - Radius, p.Y - Radius);
            aabb.UpperBound.Set(p.X + Radius, p.Y + Radius);
        }

        public override void ComputeMass(out MassData massData, float density)
        {
            massData = new MassData();

            massData.Mass = density * Settings.Pi * Radius * Radius;
            massData.Center = LocalPosition;

            // inertia about the local origin
            massData.I = massData.Mass * (0.5f * Radius * Radius + Vec2.Dot(LocalPosition, LocalPosition));
        }

        public override float ComputeSubmergedArea(ref Vec2 normal, float offset, ref XForm xf, out Vec2 c)
        {
            // Needed in the C# version as all variables marked "out" must be inialized
            c = new Vec2();

            Vec2 p = Math.Mul(xf, LocalPosition);
            float l = -(Vec2.Dot(normal, p) - offset);
            if (l < -Radius + Settings.FLT_EPSILON)
            {
                //Completely dry
                return 0;
            }
            if (l > Radius)
            {
                //Completely wet
                c = p;
                return Settings.Pi * Radius * Radius;
            }

            //Magic
            float r2 = Radius * Radius;
            float l2 = l * l;
            float area = r2 * ((float)System.Math.Asin(l / Radius) + Settings.Pi / 2) + l * Math.Sqrt(r2 - l2);
            float com = -2.0f / 3.0f * (float)System.Math.Pow(r2 - l2, 1.5f) / area;

            c.X = p.X + normal.X * com;
            c.Y = p.Y + normal.Y * com;

            return area;
        }

        //Note: Not needed by CircleShape. It is a leftover from hacking C++ generics into C#
        public override int GetSupport(ref XForm xf, ref Vec2 d)
        {
            throw new NotImplementedException();
        }

        public override int GetSupport(Vec2 d)
        {
            //B2_NOT_USED(d);
            return 0;
        }

        public override Vec2 GetSupportVertex(ref Vec2 d)
        {
            //B2_NOT_USED(d);
            return LocalPosition;
        }

        public override Vec2 GetVertex(int index)
        {
            //B2_NOT_USED(index);
            Box2DXDebug.Assert(index == 0);
            return LocalPosition;
        }

        public override float ComputeSweepRadius(ref Vec2 pivot)
        {
            return Vec2.Distance(LocalPosition, pivot);
        }
    }
}