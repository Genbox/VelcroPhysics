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
    public class CircleShape : Shape
    {
        public CircleShape(float radius)
        {
            ShapeType = ShapeType.Circle;
            Radius = radius;
            Position = Vector2.Zero;
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

            return shape;
        }

        public override int GetChildCount()
        {
            return 1;
        }

        public override bool TestPoint(ref Transform transform, Vector2 p)
        {
            Vector2 center = transform.Position + MathUtils.Multiply(ref transform.R, Position);
            Vector2 d = p - center;
            return Vector2.Dot(d, d) <= Radius * Radius;
        }

        // Collision Detection in Interactive 3D Environments by Gino van den Bergen
        // From Section 3.1.2
        // x = s + a * r
        // norm(x) = radius
        public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex)
        {
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
            float a = -(c + (float)Math.Sqrt((double)sigma));

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

        /// @see Shape.ComputeAABB
        public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
        {
            Vector2 p = transform.Position + MathUtils.Multiply(ref transform.R, Position);
            aabb.LowerBound = new Vector2(p.X - Radius, p.Y - Radius);
            aabb.UpperBound = new Vector2(p.X + Radius, p.Y + Radius);
        }

        /// @see Shape.ComputeMass
        public override void ComputeMass(out MassData massData, float density)
        {
            massData.Mass = density * Settings.Pi * Radius * Radius;
            massData.Center = Position;

            // inertia about the local origin
            massData.Inertia = massData.Mass * (0.5f * Radius * Radius + Vector2.Dot(Position, Position));
        }

        /// Position
        public Vector2 Position;
    }
}
