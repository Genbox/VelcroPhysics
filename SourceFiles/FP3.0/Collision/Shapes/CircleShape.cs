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

using Microsoft.Xna.Framework;
using System;

namespace FarseerPhysics
{
    /// <summary>
    /// Shape that represents a circle
    /// </summary>
    public class CircleShape : Shape
    {
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

        public override Shape Clone()
        {
            CircleShape shape = new CircleShape();
            shape._radius = _radius;
            shape._radius2 = _radius2;
            shape.Density = Density;
            shape.ShapeType = ShapeType;
            shape.Position = Position;
            shape.MassData = MassData;

            return shape;
        }

        public override bool TestPoint(ref Transform transform, Vector2 point)
        {
            Vector2 center = transform.Position + MathUtils.Multiply(ref transform.R, Position);
            Vector2 d = point - center;
            return Vector2.Dot(d, d) <= _radius2;
        }

        public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform)
        {
            // Collision Detection in Interactive 3D Environments by Gino van den Bergen
            // From Section 3.1.2
            // x = s + a * r
            // norm(x) = radius

            output = new RayCastOutput();

            Vector2 position = transform.Position + MathUtils.Multiply(ref transform.R, Position);
            Vector2 s = input.Point1 - position;
            float b = Vector2.Dot(s, s) - _radius2;

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

        public override void ComputeAABB(out AABB aabb, ref Transform transform)
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

        protected override sealed void ComputeProperties()
        {
            MassData data = new MassData();
            float area = Settings.Pi * _radius2;
            data.Area = area;
            data.Mass = Density * area;
            data.Center = Position;

            // inertia about the local origin
            data.Inertia = data.Mass * (0.5f * _radius2 + Vector2.Dot(Position, Position));

            MassData = data;
        }

        public override float ComputeSubmergedArea(ref Vector2 normal, float offset, ref Transform transform, out Vector2 centroid)
        {
            centroid = Vector2.Zero;

            Vector2 p = MathUtils.Multiply(ref transform, Position);
            float l = -(Vector2.Dot(normal, p) - offset);
            if (l < -_radius + Settings.Epsilon)
            {
                //Completely dry
                return 0;
            }
            if (l > _radius)
            {
                //Completely wet
                centroid = p;
                return MathHelper.Pi * _radius2;
            }

            //Magic
            float l2 = l * l;
            float area = _radius2 * ((float)Math.Asin(l / _radius) + MathHelper.Pi / 2) + l * (float)Math.Sqrt(_radius2 - l2);
            float com = -2.0f / 3.0f * (float)Math.Pow(_radius2 - l2, 1.5f) / area;

            centroid.X = p.X + normal.X * com;
            centroid.Y = p.Y + normal.Y * com;

            return area;
        }


        //TODO: Get rid of this. Use Body.Position
        /// <summary>
        /// Position of the shape
        /// </summary>
        public Vector2 Position;
    }
}
