/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
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

using System.Diagnostics;
using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.RayCast;
using VelcroPhysics.Shared;
using VelcroPhysics.Tools.ConvexHull.GiftWrap;
using VelcroPhysics.Utilities;

namespace VelcroPhysics.Collision.Shapes
{
    /// <summary>
    /// Represents a simple non-self intersecting convex polygon.
    /// Create a convex hull from the given array of points.
    /// </summary>
    public class PolygonShape : Shape
    {
        private Vertices _normals;
        private Vertices _vertices;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolygonShape" /> class.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="density">The density.</param>
        public PolygonShape(Vertices vertices, float density) : base(ShapeType.Polygon, Settings.PolygonRadius, density)
        {
            Vertices = vertices; //This assignment will call ComputeProperties()
        }

        /// <summary>
        /// Create a new PolygonShape with the specified density.
        /// </summary>
        /// <param name="density">The density.</param>
        public PolygonShape(float density) : base(ShapeType.Polygon, Settings.PolygonRadius, density) { }

        internal PolygonShape() : base(ShapeType.Polygon, Settings.PolygonRadius) { }

        /// <summary>
        /// Create a convex hull from the given array of local points.
        /// The number of vertices must be in the range [3, Settings.MaxPolygonVertices].
        /// Warning: the points may be re-ordered, even if they form a convex polygon
        /// Warning: collinear points are handled but not removed. Collinear points may lead to poor stacking behavior.
        /// </summary>
        public Vertices Vertices
        {
            get { return _vertices; }
            set
            {
                Debug.Assert(value.Count >= 3 && value.Count <= Settings.MaxPolygonVertices);

                if (Settings.UseConvexHullPolygons)
                {
                    //Velcro: This check is required as the GiftWrap algorithm early exits on triangles
                    //So instead of giftwrapping a triangle, we just force it to be clock wise.
                    if (value.Count <= 3)
                    {
                        _vertices = new Vertices(value);
                        _vertices.ForceCounterClockWise();
                    }
                    else
                        _vertices = GiftWrap.GetConvexHull(value);
                }
                else
                {
                    _vertices = new Vertices(value);
                }

                _normals = new Vertices(_vertices.Count);

                // Compute normals. Ensure the edges have non-zero length.
                for (int i = 0; i < _vertices.Count; ++i)
                {
                    int i1 = i;
                    int i2 = i + 1 < _vertices.Count ? i + 1 : 0;
                    Vector2 edge = _vertices[i2] - _vertices[i1];
                    Debug.Assert(edge.LengthSquared() > Settings.Epsilon * Settings.Epsilon);
                    Vector2 temp = MathUtils.Cross(edge, 1.0f);
                    temp.Normalize();
                    _normals.Add(temp);
                }

                // Compute the polygon mass data
                ComputeProperties();
            }
        }

        public Vertices Normals => _normals;

        public override int ChildCount => 1;

        protected sealed override void ComputeProperties()
        {
            // Polygon mass, centroid, and inertia.
            // Let rho be the polygon density in mass per unit area.
            // Then:
            // mass = rho * int(dA)
            // centroid.x = (1/mass) * rho * int(x * dA)
            // centroid.y = (1/mass) * rho * int(y * dA)
            // I = rho * int((x*x + y*y) * dA)
            //
            // We can compute these integrals by summing all the integrals
            // for each triangle of the polygon. To evaluate the integral
            // for a single triangle, we make a change of variables to
            // the (u,v) coordinates of the triangle:
            // x = x0 + e1x * u + e2x * v
            // y = y0 + e1y * u + e2y * v
            // where 0 <= u && 0 <= v && u + v <= 1.
            //
            // We integrate u from [0,1-v] and then v from [0,1].
            // We also need to use the Jacobian of the transformation:
            // D = cross(e1, e2)
            //
            // Simplification: triangle centroid = (1/3) * (p1 + p2 + p3)
            //
            // The rest of the derivation is handled by computer algebra.

            Debug.Assert(Vertices.Count >= 3);

            //Velcro optimization: Early exit as polygons with 0 density does not have any properties.
            if (_density <= 0)
                return;

            //Velcro optimization: Consolidated the calculate centroid and mass code to a single method.
            Vector2 center = Vector2.Zero;
            float area = 0.0f;
            float I = 0.0f;

            //Velcro: We change the reference point to be inside the polygon

            // pRef is the reference point for forming triangles.
            // It's location doesn't change the result (except for rounding error).
            Vector2 s = Vector2.Zero;

            // This code would put the reference point inside the polygon.
            for (int i = 0; i < Vertices.Count; ++i)
            {
                s += Vertices[i];
            }
            s *= 1.0f / Vertices.Count;

            const float k_inv3 = 1.0f / 3.0f;

            for (int i = 0; i < Vertices.Count; ++i)
            {
                // Triangle vertices.
                Vector2 e1 = Vertices[i] - s;
                Vector2 e2 = i + 1 < Vertices.Count ? Vertices[i + 1] - s : Vertices[0] - s;

                float D = MathUtils.Cross(e1, e2);

                float triangleArea = 0.5f * D;
                area += triangleArea;

                // Area weighted centroid
                center += triangleArea * k_inv3 * (e1 + e2);

                float ex1 = e1.X, ey1 = e1.Y;
                float ex2 = e2.X, ey2 = e2.Y;

                float intx2 = ex1 * ex1 + ex2 * ex1 + ex2 * ex2;
                float inty2 = ey1 * ey1 + ey2 * ey1 + ey2 * ey2;

                I += (0.25f * k_inv3 * D) * (intx2 + inty2);
            }

            //The area is too small for the engine to handle.
            Debug.Assert(area > Settings.Epsilon);

            // We save the area
            MassData.Area = area;

            // Total mass
            MassData.Mass = _density * area;

            // Center of mass
            center *= 1.0f / area;
            MassData.Centroid = center + s;

            // Inertia tensor relative to the local origin (point s).
            MassData.Inertia = _density * I;

            // Shift to center of mass then to original body origin.
            MassData.Inertia += MassData.Mass * (Vector2.Dot(MassData.Centroid, MassData.Centroid) - Vector2.Dot(center, center));
        }

        public override bool TestPoint(ref Transform transform, ref Vector2 point)
        {
            return TestPointHelper.TestPointPolygon(_vertices, _normals, ref point, ref transform);
        }

        public override bool RayCast(ref RayCastInput input, ref Transform transform, int childIndex, out RayCastOutput output)
        {
            return RayCastHelper.RayCastPolygon(_vertices, _normals, ref input, ref transform, out output);
        }

        /// <summary>
        /// Given a transform, compute the associated axis aligned bounding box for a child shape.
        /// </summary>
        /// <param name="transform">The world transform of the shape.</param>
        /// <param name="childIndex">The child shape index.</param>
        /// <param name="aabb">The AABB results.</param>
        public override void ComputeAABB(ref Transform transform, int childIndex, out AABB aabb)
        {
            AABBHelper.ComputePolygonAABB(_vertices, ref transform, out aabb);
        }

        public bool CompareTo(PolygonShape shape)
        {
            if (Vertices.Count != shape.Vertices.Count)
                return false;

            for (int i = 0; i < Vertices.Count; i++)
            {
                if (Vertices[i] != shape.Vertices[i])
                    return false;
            }

            return Radius == shape.Radius && MassData == shape.MassData;
        }

        public override Shape Clone()
        {
            PolygonShape clone = new PolygonShape();
            clone.ShapeType = ShapeType;
            clone._radius = _radius;
            clone._density = _density;
            clone._vertices = new Vertices(_vertices);
            clone._normals = new Vertices(_normals);
            clone.MassData = MassData;
            return clone;
        }
    }
}