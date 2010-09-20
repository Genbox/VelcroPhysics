/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
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
    public class CircleShape : Shape
    {
        public Vector2 Position;

        public CircleShape(float radius)
        {
            ShapeType = ShapeType.Circle;
            Radius = radius;
            Position = Vector2.Zero;
        }

        internal CircleShape(float radius, float density)
        {
            ShapeType = ShapeType.Circle;
            Radius = radius;
            Position = Vector2.Zero;
            _density = density;
        }

        public CircleShape()
        {
            ShapeType = ShapeType.Circle;
            Radius = 0.0f;
            Position = Vector2.Zero;
        }

        public override Shape Clone()
        {
            CircleShape shape = new CircleShape();
            shape.ShapeType = ShapeType;
            shape.Radius = Radius;
            shape.Position = Position;
            shape._density = _density;
            shape.MassData = MassData;
            return shape;
        }

        public override int ChildCount
        {
            get { return 1; }
        }

        /// <summary>
        /// Test a point for containment in this shape. This only works for convex shapes.
        /// </summary>
        /// <param name="transform">The shape world transform.</param>
        /// <param name="point">a point in world coordinates.</param>
        /// <returns>True if the point is inside the shape</returns>
        public override bool TestPoint(ref Transform transform, ref Vector2 point)
        {
            Vector2 center = transform.Position + MathUtils.Multiply(ref transform.R, Position);
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
        public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform,
                                     int childIndex)
        {
            // Collision Detection in Interactive 3D Environments by Gino van den Bergen
            // From Section 3.1.2
            // x = s + a * r
            // norm(x) = radius

            output = new RayCastOutput();

            Vector2 position = transform.Position + MathUtils.Multiply(ref transform.R, Position);
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
            float a = -(c + (float) Math.Sqrt(sigma));

            // Is the intersection point on the segment?
            if (0.0f <= a && a <= input.MaxFraction * rr)
            {
                a /= rr;
                output.Fraction = a;
                Vector2 norm = (s + a * r);
                norm.Normalize();
                output.Normal = norm;
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
            Vector2 p = transform.Position + MathUtils.Multiply(ref transform.R, Position);
            aabb.LowerBound = new Vector2(p.X - Radius, p.Y - Radius);
            aabb.UpperBound = new Vector2(p.X + Radius, p.Y + Radius);
        }

        /// <summary>
        /// Compute the mass properties of this shape using its dimensions and density.
        /// The inertia tensor is computed about the local origin, not the centroid.
        /// </summary>
        public override void ComputeProperties()
        {
            MassData.Mass = _density * Settings.Pi * Radius * Radius;
            MassData.Center = Position;

            // inertia about the local origin
            MassData.Inertia = MassData.Mass * (0.5f * Radius * Radius + Vector2.Dot(Position, Position));
        }
    }
}