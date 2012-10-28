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
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision.Shapes
{
    /// <summary>
    /// A circle shape.
    /// </summary>
    public class CircleShape : Shape
    {
        internal Vector2 _position;

        public CircleShape(float radius, float density)
            : base(density)
        {
            ShapeType = ShapeType.Circle;
            _radius = radius;
            _position = Vector2.Zero;
            ComputeProperties();
        }

        internal CircleShape()
            : base(0)
        {
            ShapeType = ShapeType.Circle;
            _radius = 0.0f;
            _position = Vector2.Zero;
        }

        public override int ChildCount
        {
            get { return 1; }
        }

        /// <summary>
        /// Get or set the position of the circle
        /// </summary>
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                ComputeProperties();
            }
        }

        public override Shape Clone()
        {
            CircleShape shape = new CircleShape();
            shape._radius = Radius;
            shape._density = _density;
            shape._position = _position;
            shape.ShapeType = ShapeType;
            shape.MassData = MassData;
            return shape;
        }

        /// <summary>
        /// Test a point for containment in this shape. This only works for convex shapes.
        /// </summary>
        /// <param name="transform">The shape world transform.</param>
        /// <param name="point">a point in world coordinates.</param>
        /// <returns>True if the point is inside the shape</returns>
        public override bool TestPoint(ref Transform transform, ref Vector2 point)
        {
            Vector2 center = transform.p + MathUtils.Mul(transform.q, Position);
            Vector2 d = point - center;
            return Vector2.Dot(d, d) <= Radius * Radius;
        }

        /// <summary>
        /// Cast a ray against a child shape.
        /// </summary>
        /// <param name="output">The ray-cast results.</param>
        /// <param name="input">The ray-cast input parameters.</param>
        /// <param name="transform">The transform to be applied to the shape.</param>
        /// <param name="childIndex">The child shape index.</param>
        /// <returns>True if the ray-cast hits the shape</returns>
        public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex)
        {
            // Collision Detection in Interactive 3D Environments by Gino van den Bergen
            // From Section 3.1.2
            // x = s + a * r
            // norm(x) = radius

            output = new RayCastOutput();

            Vector2 position = transform.p + MathUtils.Mul(transform.q, Position);
            Vector2 s = input.Point1 - position;
            float b = Vector2.Dot(s, s) - Radius * Radius;

            // Solve quadratic equation.
            Vector2 r = input.Point2 - input.Point1;
            float c = Vector2.Dot(s, r);
            float rr = Vector2.Dot(r, r);
            float sigma = c * c - rr * b;

            // Check for negative discriminant and short segment.
            if (sigma < 0.0f || rr < Settings.Epsilon)
            {
                return false;
            }

            // Find the point of intersection of the line with the circle.
            float a = -(c + (float)Math.Sqrt(sigma)); //TODO: Move to mathhelper?

            // Is the intersection point on the segment?
            if (0.0f <= a && a <= input.MaxFraction * rr)
            {
                a /= rr;
                output.Fraction = a;

                //TODO: Check results here
                output.Normal = s + a * r;
                output.Normal.Normalize();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Given a transform, compute the associated axis aligned bounding box for a child shape.
        /// </summary>
        /// <param name="aabb">The aabb results.</param>
        /// <param name="transform">The world transform of the shape.</param>
        /// <param name="childIndex">The child shape index.</param>
        public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
        {
            Vector2 p = transform.p + MathUtils.Mul(transform.q, Position);
            aabb.LowerBound = new Vector2(p.X - Radius, p.Y - Radius);
            aabb.UpperBound = new Vector2(p.X + Radius, p.Y + Radius);
        }

        /// <summary>
        /// Compute the mass properties of this shape using its dimensions and density.
        /// The inertia tensor is computed about the local origin, not the centroid.
        /// </summary>
        protected override sealed void ComputeProperties()
        {
            float area = Settings.Pi * Radius * Radius;
            MassData.Area = area;
            MassData.Mass = Density * area;
            MassData.Centroid = Position;

            // inertia about the local origin
            MassData.Inertia = MassData.Mass * (0.5f * Radius * Radius + Vector2.Dot(Position, Position));
        }

        /// <summary>
        /// Compare the circle to another circle
        /// </summary>
        /// <param name="shape">The other circle</param>
        /// <returns>True if the two circles are the same size and have the same position</returns>
        public bool CompareTo(CircleShape shape)
        {
            return (Radius == shape.Radius && Position == shape.Position);
        }

        /// <summary>
        /// Method used by the BuoyancyController
        /// </summary>
        public override float ComputeSubmergedArea(ref Vector2 normal, float offset, ref Transform xf, out Vector2 sc)
        {
            sc = Vector2.Zero;

            Vector2 p = MathUtils.Mul(ref xf, Position);
            float l = -(Vector2.Dot(normal, p) - offset);
            if (l < -Radius + Settings.Epsilon)
            {
                //Completely dry
                return 0;
            }
            if (l > Radius)
            {
                //Completely wet
                sc = p;
                return Settings.Pi * Radius * Radius;
            }

            //Magic
            float r2 = Radius * Radius;
            float l2 = l * l;
            float area = r2 * (float)((Math.Asin(l / Radius) + Settings.Pi / 2) + l * Math.Sqrt(r2 - l2));
            float com = -2.0f / 3.0f * (float)Math.Pow(r2 - l2, 1.5f) / area;

            sc.X = p.X + normal.X * com;
            sc.Y = p.Y + normal.Y * com;

            return area;
        }
    }
}