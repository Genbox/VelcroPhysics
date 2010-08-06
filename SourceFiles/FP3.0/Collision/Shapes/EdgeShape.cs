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


using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision.Shapes
{
    /// <summary>
    /// A line segment (edge) Shape. These can be connected in chains or loops
    /// to other edge Shapes. The connectivity information is used to ensure
    /// correct contact normals.
    /// </summary>
    public class EdgeShape : Shape
    {
        public bool _hasVertex0, _hasVertex3;

        /// Optional adjacent vertices. These are used for smooth collision.
        public Vector2 _vertex0;

        /// These are the edge vertices
        public Vector2 _vertex1, _vertex2;

        /// Optional adjacent vertices. These are used for smooth collision.
        public Vector2 _vertex3;

        public EdgeShape()
        {
            ShapeType = ShapeType.Edge;
            Radius = Settings.PolygonRadius;
            _hasVertex0 = false;
            _hasVertex3 = false;
        }

        /// Set this as an isolated edge.
        public void Set(Vector2 v1, Vector2 v2)
        {
            _vertex1 = v1;
            _vertex2 = v2;
            _hasVertex0 = false;
            _hasVertex3 = false;
        }

        public override Shape Clone()
        {
            var edge = new EdgeShape();
            edge._hasVertex0 = _hasVertex0;
            edge._hasVertex3 = _hasVertex3;
            edge.Radius = Radius;
            edge._vertex0 = _vertex0;
            edge._vertex1 = _vertex1;
            edge._vertex2 = _vertex2;
            edge._vertex3 = _vertex3;
            return edge;
        }

        public override int GetChildCount()
        {
            return 1;
        }

        public override bool TestPoint(ref Transform transform, Vector2 point)
        {
            return false;
        }

        /// Implement Shape.
        /// 
        // p = p1 + t * d
        // v = v1 + s * e
        // p1 + t * d = v1 + s * e
        // s * e - t * d = p1 - v1
        // 
        public override bool RayCast(out RayCastOutput output, ref RayCastInput input,
                                     ref Transform transform, int childIndex)
        {
            output = new RayCastOutput();

            // Put the ray into the edge's frame of reference.
            Vector2 p1 = MathUtils.MultiplyT(ref transform.R, input.Point1 - transform.Position);
            Vector2 p2 = MathUtils.MultiplyT(ref transform.R, input.Point2 - transform.Position);
            Vector2 d = p2 - p1;

            Vector2 v1 = _vertex1;
            Vector2 v2 = _vertex2;
            Vector2 e = v2 - v1;
            Vector2 normal = new Vector2(e.Y, -e.X);
            normal.Normalize();

            // q = p1 + t * d
            // dot(normal, q - v1) = 0
            // dot(normal, p1 - v1) + t * dot(normal, d) = 0
            float numerator = Vector2.Dot(normal, v1 - p1);
            float denominator = Vector2.Dot(normal, d);

            if (denominator == 0.0f)
            {
                return false;
            }

            float t = numerator/denominator;
            if (t < 0.0f || 1.0f < t)
            {
                return false;
            }

            Vector2 q = p1 + t*d;

            // q = v1 + s * r
            // s = dot(q - v1, r) / dot(r, r)
            Vector2 r = v2 - v1;
            float rr = Vector2.Dot(r, r);
            if (rr == 0.0f)
            {
                return false;
            }

            float s = Vector2.Dot(q - v1, r)/rr;
            if (s < 0.0f || 1.0f < s)
            {
                return false;
            }

            output.Fraction = t;
            if (numerator > 0.0f)
            {
                output.Normal = -normal;
            }
            else
            {
                output.Normal = normal;
            }
            return true;
        }

        public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
        {
            aabb = new AABB();
            Vector2 v1 = MathUtils.Multiply(ref transform, _vertex1);
            Vector2 v2 = MathUtils.Multiply(ref transform, _vertex2);

            Vector2 lower = Vector2.Min(v1, v2);
            Vector2 upper = Vector2.Max(v1, v2);

            Vector2 r = new Vector2(Radius, Radius);
            aabb.LowerBound = lower - r;
            aabb.UpperBound = upper + r;
        }

        public override void ComputeMass(out MassData massData, float density)
        {
            massData = new MassData();
            massData.Mass = 0.0f;
            massData.Center = 0.5f*(_vertex1 + _vertex2);
            massData.Inertia = 0.0f;
        }
    }
}