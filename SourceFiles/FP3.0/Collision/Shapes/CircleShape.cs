/*
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
    /// <summary>
    /// Shape that represents a circle
    /// </summary>
    public class CircleShape : Shape
    {
        /// <summary>
        /// Position of the shape
        /// </summary>
        public Vector2 Position;

        private CircleShape()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CircleShape"/> class.
        /// </summary>
        /// <param name="radius">The radius.</param>
        /// <param name="density">The density.</param>
        public CircleShape(float radius, float density)
            : base(radius, density)
        {
            ShapeType = ShapeType.Circle;
            ComputeProperties();
        }

        /// <summary>
        /// Clones the shape.
        /// </summary>
        /// <returns>A clone of the shape</returns>
        public override Shape Clone()
        {
            CircleShape shape = new CircleShape();
            shape.Radius = Radius;
            shape.Density = Density;
            shape.ShapeType = ShapeType;
            shape.Position = Position;
            shape.Area = Area;
            shape.Mass = Mass;
            shape.Center = Center;
            shape.Inertia = Inertia;

            return shape;
        }

        public override int GetChildCount()
        {
            return 1;
        }

        /// <summary>
        /// Tests if the supplied point is inside the shape.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        public override bool TestPoint(ref Transform transform, Vector2 point)
        {
            Vector2 center = transform.Position + MathUtils.Multiply(ref transform.R, Position);
            Vector2 d = point - center;
            return Vector2.Dot(d, d) <= (Radius * Radius);
        }

        /// <summary>
        /// Cast a ray against this shape.
        /// </summary>
        /// <param name="output">the ray-cast results.</param>
        /// <param name="input">the ray-cast input parameters.</param>
        /// <param name="transform">the transform to be applied to the shape.</param>
        /// <returns>True if the raycast hit something</returns>
        public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex)
        {
            // Collision Detection in Interactive 3D Environments by Gino van den Bergen
            // From Section 3.1.2
            // x = s + a * r
            // norm(x) = radius

            output = new RayCastOutput();

            Vector2 position = transform.Position + MathUtils.Multiply(ref transform.R, Position);
            Vector2 s = input.Point1 - position;
            float b = Vector2.Dot(s, s) - (Radius * Radius);

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
            float a = -(c + (float)Math.Sqrt(sigma));

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
        /// Given a transform, compute the associated axis aligned bounding box for this shape.
        /// </summary>
        /// <param name="aabb">returns the axis aligned box.</param>
        /// <param name="transform">the world transform of the shape.</param>
        public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
        {
#if XBOX360
            aabb = new AABB();
#endif

            Vector2 p = transform.Position + MathUtils.Multiply(ref transform.R, Position);

            //aabb.LowerBound = new Vector2(p.X - Radius, p.Y - Radius);
            aabb.LowerBound.X = p.X - Radius;
            aabb.LowerBound.Y = p.Y - Radius;

            //aabb.UpperBound = new Vector2(p.X + Radius, p.Y + Radius);
            aabb.UpperBound.X = p.X + Radius;
            aabb.UpperBound.Y = p.Y + Radius;
        }

        /// <summary>
        /// Computes the properties of the shape
        /// The following properties are computed:
        /// - Area
        /// - Mass
        /// - Center of shape
        /// - Interia
        /// </summary>
        protected override sealed void ComputeProperties()
        {
            float area = Settings.Pi * (Radius * Radius);
            Area = area;
            Mass = Density * area;
            Center = Position;

            // inertia about the local origin
            Inertia = Mass * (0.5f * (Radius * Radius) + Vector2.Dot(Position, Position));
        }

        /// <summary>
        /// Gets the vertices of the shape. If the shape is not already represented by vertices
        /// an approximation will be made.
        /// </summary>
        /// <returns></returns>
        public override Vertices GetVertices()
        {
            return PolygonTools.CreateCircle(Radius, 16);
        }
    }
}