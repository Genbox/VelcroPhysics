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
        public Vec2 _p;

        public CircleShape()
        {
            Type = ShapeType.CircleShape;
            _radius = 0.0f;
            _p.SetZero();
        }

        public override Shape Clone()
        {
            CircleShape shape = new CircleShape();
            shape._p = this._p;
            shape.Type = this.Type;
            shape._radius = this._radius;
            return shape;
        }

        public override bool TestPoint(Transform transform, Vec2 p)
        {
            Vec2 center = transform.Position + Math.Mul(transform.R, _p);
            Vec2 d = p - center;
            return Vec2.Dot(d, d) <= _radius * _radius;
        }

        // Collision Detection in Interactive 3D Environments by Gino van den Bergen
        // From Section 3.1.2
        // x = s + a * r
        // norm(x) = radius
        public override void RayCast(out RayCastOutput output, ref RayCastInput input, Transform transform)
        {
            output = new RayCastOutput();

            Vec2 position = transform.Position + Math.Mul(transform.R, _p);
            Vec2 s = input.P1 - position;
            float b = Vec2.Dot(s, s) - _radius * _radius;

            // Solve quadratic equation.
            Vec2 r = input.P2 - input.P1;
            float c = Vec2.Dot(s, r);
            float rr = Vec2.Dot(r, r);
            float sigma = c * c - rr * b;

            // Check for negative discriminant and short segment.
            if (sigma < 0.0f || rr < Settings.FLT_EPSILON)
            {
                output.Hit = false;
                return;
            }

            // Find the point of intersection of the line with the circle.
            float a = -(c + Math.Sqrt(sigma));

            // Is the intersection point on the segment?
            if (0.0f <= a && a <= input.MaxFraction * rr)
            {
                a /= rr;
                output.Hit = true;
                output.Fraction = a;
                output.Normal = s + a*r;
                output.Normal.Normalize();
                return;
            }

            output.Hit = false;
            return;
        }

        public override void ComputeAABB(out AABB aabb, ref Transform transform)
        {
            aabb = new AABB();

            Vec2 p = transform.Position + Math.Mul(transform.R, _p);
            aabb.LowerBound.Set(p.X - _radius, p.Y - _radius);
            aabb.UpperBound.Set(p.X + _radius, p.Y + _radius);
        }

        public override void ComputeMass(out MassData massData, float density)
        {
            massData = new MassData();

            massData.Mass = density * Settings.PI * _radius * _radius;
            massData.Center = _p;

            // inertia about the local origin
            massData.I = massData.Mass * (0.5f * _radius * _radius + Vec2.Dot(_p, _p));
        }

        public override int GetSupport(Vec2 d)
        {
            //B2_NOT_USED(d);
            return 0;
        }

        public override Vec2 GetSupportVertex(ref Vec2 d)
        {
            //B2_NOT_USED(d);
            return _p;
        }

        /// Get the vertex count.
        public int GetVertexCount()
        {
            return 1;
        }

        public override Vec2 GetVertex(int index)
        {
            //B2_NOT_USED(index);
            Box2DXDebug.Assert(index == 0);
            return _p;
        }
    }
}