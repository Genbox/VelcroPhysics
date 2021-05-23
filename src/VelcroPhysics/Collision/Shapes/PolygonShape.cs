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
using Genbox.VelcroPhysics.Collision.RayCast;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Tools.ConvexHull.GiftWrap;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Collision.Shapes
{
    /// <summary>Represents a simple non-self intersecting convex polygon. Create a convex hull from the given array of points.</summary>
    public class PolygonShape : Shape
    {
        internal Vertices _normals;
        internal Vertices _vertices;

        /// <summary>Initializes a new instance of the <see cref="PolygonShape" /> class.</summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="density">The density.</param>
        public PolygonShape(Vertices vertices, float density) : base(ShapeType.Polygon, Settings.PolygonRadius, density)
        {
            //Velcro: We must call the property here in order to modify the input and compute mass properties
            Vertices = vertices;
        }

        /// <summary>Initializes a new instance of the <see cref="PolygonShape" /> class.</summary>
        /// <param name="density">The density.</param>
        public PolygonShape(float density) : base(ShapeType.Polygon, Settings.PolygonRadius, density)
        {
        }

        private PolygonShape() : base(ShapeType.Polygon, Settings.PolygonRadius) { }

        /// <summary>
        /// Create a convex hull from the given array of local points. The number of vertices must be in the range [3,
        /// Settings.MaxPolygonVertices]. Warning: the points may be re-ordered, even if they form a convex polygon Warning:
        /// collinear points are handled but not removed. Collinear points may lead to poor stacking behavior.
        /// </summary>
        public Vertices Vertices
        {
            get => _vertices;
            set
            {
                Vertices vertices = value;

                Debug.Assert(vertices.Count >= 3 && vertices.Count <= Settings.MaxPolygonVertices);

                int n = MathUtils.Min(vertices.Count, Settings.MaxPolygonVertices);

                // Perform welding and copy vertices into local buffer.
                Vector2[] ps = new Vector2[n]; //Velcro: The temp buffer is n long instead of Settings.MaxPolygonVertices
                int tempCount = 0;
                for (int i = 0; i < n; ++i)
                {
                    Vector2 v = vertices[i];

                    bool unique = true;
                    for (int j = 0; j < tempCount; ++j)
                    {
                        Vector2 temp = ps[j];
                        if (MathUtils.DistanceSquared(ref v, ref temp) < (0.5f * Settings.LinearSlop) * (0.5f * Settings.LinearSlop))
                        {
                            unique = false;
                            break;
                        }
                    }

                    if (unique)
                    {
                        ps[tempCount++] = v;
                    }
                }

                n = tempCount;
                if (n < 3)
                {
                    // Polygon is degenerate.
                    _vertices = PolygonUtils.CreateRectangle(0.5f, 0.5f);
                    Debug.Assert(false);
                    goto Normals;
                }

                _vertices = GiftWrap.GetConvexHull(vertices);

                if (_vertices.Count < 3)
                {
                    // Polygon is degenerate.
                    _vertices = PolygonUtils.CreateRectangle(0.5f, 0.5f);
                    Debug.Assert(false);
                }

            Normals:

                _normals = new Vertices(_vertices.Count);

                // Compute normals. Ensure the edges have non-zero length.
                for (int i = 0; i < _vertices.Count; ++i)
                {
                    int i1 = i;
                    int i2 = i + 1 < _vertices.Count ? i + 1 : 0;
                    Vector2 edge = _vertices[i2] - _vertices[i1];
                    Debug.Assert(edge.LengthSquared() > MathConstants.Epsilon * MathConstants.Epsilon);
                    Vector2 temp = MathUtils.Cross(edge, 1.0f);
                    temp.Normalize();
                    _normals.Add(temp);
                }

                //Velcro: We compute all the mass data properties up front
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

            Debug.Assert(_vertices.Count >= 3);

            //Velcro optimization: Early exit as polygons with 0 density does not have any properties.
            if (_density <= 0)
                return;

            //Velcro optimization: Consolidated the calculate centroid and mass code to a single method.
            Vector2 centroid = Vector2.Zero;
            float area = 0.0f;
            float I = 0.0f;

            // Get a reference point for forming triangles.
            // Use the first vertex to reduce round-off errors.
            Vector2 s = _vertices[0];

            const float inv3 = 1.0f / 3.0f;

            int count = _vertices.Count;

            for (int i = 0; i < count; ++i)
            {
                // Triangle vertices.
                Vector2 p1 = _vertices[0] - s;
                Vector2 p2 = _vertices[i] - s;
                Vector2 p3 = i + 1 < count ? _vertices[i + 1] - s : _vertices[0] - s;

                Vector2 e1 = p2 - p1;
                Vector2 e2 = p3 - p1;

                float D = MathUtils.Cross(e1, e2);

                float triangleArea = 0.5f * D;
                area += triangleArea;

                // Area weighted centroid
                centroid += triangleArea * inv3 * (e1 + e2);

                float ex1 = e1.X, ey1 = e1.Y;
                float ex2 = e2.X, ey2 = e2.Y;

                float intx2 = ex1 * ex1 + ex2 * ex1 + ex2 * ex2;
                float inty2 = ey1 * ey1 + ey2 * ey1 + ey2 * ey2;

                I += 0.25f * inv3 * D * (intx2 + inty2);
            }

            //The area is too small for the engine to handle.
            Debug.Assert(area > MathConstants.Epsilon);

            // We save the area
            _massData._area = area;

            // Total mass
            _massData._mass = _density * area;

            // Center of mass
            centroid *= 1.0f / area;
            _massData._centroid = centroid + s;

            // Inertia tensor relative to the local origin (point s).
            _massData._inertia = _density * I;

            // Shift to center of mass then to original body origin.
            _massData._inertia += _massData._mass * (Vector2.Dot(_massData._centroid, _massData._centroid) - Vector2.Dot(centroid, centroid));
        }

        public override bool TestPoint(ref Transform transform, ref Vector2 point)
        {
            return TestPointHelper.TestPointPolygon(_vertices, _normals, ref point, ref transform);
        }

        public override bool RayCast(ref RayCastInput input, ref Transform transform, int childIndex, out RayCastOutput output)
        {
            return RayCastHelper.RayCastPolygon(_vertices, _normals, ref input, ref transform, out output);
        }

        /// <summary>Given a transform, compute the associated axis aligned bounding box for a child shape.</summary>
        /// <param name="transform">The world transform of the shape.</param>
        /// <param name="childIndex">The child shape index.</param>
        /// <param name="aabb">The AABB results.</param>
        public override void ComputeAABB(ref Transform transform, int childIndex, out AABB aabb)
        {
            AABBHelper.ComputePolygonAABB(_vertices, ref transform, out aabb);
        }

        public override Shape Clone()
        {
            PolygonShape clone = new PolygonShape();
            clone._shapeType = _shapeType;
            clone._radius = _radius;
            clone._density = _density;
            clone._vertices = new Vertices(_vertices);
            clone._normals = new Vertices(_normals);
            clone._massData = _massData;
            return clone;
        }
    }
}