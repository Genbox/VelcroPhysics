/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2007 Erin Catto http://www.gphysics.com

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

using Box2DX.Common;

namespace Box2DX.Collision
{
    public partial class Collision
    {
        // GJK using Voronoi regions (Christer Ericson) and Barycentric coordinates.
        public static int GjkCalls, GjkIters, GjkMaxIters;

        /// A distance proxy is used by the GJK algorithm.
        /// It encapsulates any shape.
        public class DistanceProxy
        {
            public DistanceProxy()
            {
            }

            /// Initialize the proxy using the given shape. The shape
            /// must remain in scope while the proxy is in use.
            public void Set(Shape shape)
            {
                switch (shape.GetType())
                {
                    case ShapeType.CircleShape:
                        {
                            CircleShape circle = (CircleShape)shape;
                            _vertices = new Vec2[1];
                            _vertices[0] = circle._p;
                            _count = 1;
                            _radius = circle._radius;
                        }
                        break;

                    case ShapeType.PolygonShape:
                        {
                            PolygonShape polygon = (PolygonShape)shape;
                            _vertices = polygon.Vertices;
                            _count = polygon.VertexCount;
                            _radius = polygon._radius;
                        }
                        break;

                    default:
                        Box2DXDebug.Assert(false);
                        break;
                }
            }

            /// Get the supporting vertex index in the given direction.
            public int GetSupport(Vec2 d)
            {
                int bestIndex = 0;
                float bestValue = Vec2.Dot(_vertices[0], d);
                for (int i = 1; i < _count; ++i)
                {
                    float value = Vec2.Dot(_vertices[i], d);
                    if (value > bestValue)
                    {
                        bestIndex = i;
                        bestValue = value;
                    }
                }

                return bestIndex;
            }

            /// Get the supporting vertex in the given direction.
            public Vec2 GetSupportVertex(Vec2 d)
            {
                int bestIndex = 0;
                float bestValue = Vec2.Dot(_vertices[0], d);
                for (int i = 1; i < _count; ++i)
                {
                    float value = Vec2.Dot(_vertices[i], d);
                    if (value > bestValue)
                    {
                        bestIndex = i;
                        bestValue = value;
                    }
                }

                return _vertices[bestIndex];
            }

            /// Get the vertex count.
            public int GetVertexCount()
            {
                return _count;
            }

            /// Get a vertex by index. Used by b2Distance.
            public Vec2 GetVertex(int index)
            {
                Box2DXDebug.Assert(0 <= index && index < _count);
                return _vertices[index];
            }

            public Vec2[] _vertices;
            public int _count;
            public float _radius;
        };

        /// <summary>
        /// Used to warm start b2Distance.
        /// Set count to zero on first call.
        /// </summary>
        public class SimplexCache
        {
            public float Metric;	// length or area
            public ushort Count;
            public byte[] IndexA = new byte[3];	// vertices on shape A
            public byte[] IndexB = new byte[3];	// vertices on shape B
        };

        /// <summary>
        /// Input for b2Distance.
        /// You have to option to use the shape radii
        /// in the computation. Even 
        /// </summary>
        public class DistanceInput
        {
            public DistanceProxy proxyA = new DistanceProxy();
            public DistanceProxy proxyB = new DistanceProxy();
            public Transform TransformA;
            public Transform TransformB;
            public bool UseRadii;
        };

        /// <summary>
        /// Output for b2Distance.
        /// </summary>
        public struct DistanceOutput
        {
            public Vec2 PointA;		// closest point on shapeA
            public Vec2 PointB;		// closest point on shapeB
            public float Distance;
            public int Iterations;	// number of GJK iterations used
        };

        public class SimplexVertex
        {
            public Vec2 WA;		// support point in shapeA
            public Vec2 WB;		// support point in shapeB
            public Vec2 W;		// wB - wA
            public float A;		// barycentric coordinate for closest point
            public int IndexA;	// wA index
            public int IndexB;	// wB index
        };

        public class Simplex
        {
            public Simplex()
            {
                for (int i = 0; i < 3; i++)
                {
                    Vertices[i] = new SimplexVertex();
                }
            }

            public void ReadCache(SimplexCache cache,
                            DistanceProxy shapeA, ref Transform transformA,
                            DistanceProxy shapeB, ref Transform transformB)
            {
                Box2DXDebug.Assert(0 <= cache.Count && cache.Count <= 3);

                // Copy data from cache.
                Count = cache.Count;

                for (int i = 0; i < Count; ++i)
                {
                    SimplexVertex v = Vertices[i];
                    v.IndexA = cache.IndexA[i];
                    v.IndexB = cache.IndexB[i];
                    Vec2 wALocal = shapeA.GetVertex(v.IndexA);
                    Vec2 wBLocal = shapeB.GetVertex(v.IndexB);
                    v.WA = Math.Mul(transformA, wALocal);
                    v.WB = Math.Mul(transformB, wBLocal);
                    v.W = v.WB - v.WA;
                    v.A = 0.0f;
                }

                // Compute the new simplex metric, if it is substantially different than
                // old metric then flush the simplex.
                if (Count > 1)
                {
                    float metric1 = cache.Metric;
                    float metric2 = GetMetric();
                    if (metric2 < 0.5f * metric1 || 2.0f * metric1 < metric2 || metric2 < Settings.FLT_EPSILON)
                    {
                        // Reset the simplex.
                        Count = 0;
                    }
                }

                // If the cache is empty or invalid ...
                if (Count == 0)
                {
                    SimplexVertex v = Vertices[0];
                    v.IndexA = 0;
                    v.IndexB = 0;
                    Vec2 wALocal = shapeA.GetVertex(0);
                    Vec2 wBLocal = shapeB.GetVertex(0);
                    v.WA = Math.Mul(transformA, wALocal);
                    v.WB = Math.Mul(transformB, wBLocal);
                    v.W = v.WB - v.WA;
                    Count = 1;
                }
            }

            public void WriteCache(SimplexCache cache)
            {
                cache.Metric = GetMetric();
                cache.Count = (ushort)Count;
                for (int i = 0; i < Count; ++i)
                {
                    cache.IndexA[i] = (byte)Vertices[i].IndexA;
                    cache.IndexB[i] = (byte)Vertices[i].IndexB;
                }
            }

            public Vec2 GetSearchDirection()
            {
                switch (Count)
                {
                    case 1:
                        return -Vertices[0].W;
                    case 2:
                        {
                            Vec2 e12 = Vertices[1].W - Vertices[0].W;
                            float sgn = Vec2.Cross(e12, -Vertices[0].W);
                            if (sgn > 0.0f)
                            {
                                // Origin is left of e12.
                                return Vec2.Cross(1.0f, e12);
                            }
                            else
                            {
                                // Origin is right of e12.
                                return Vec2.Cross(e12, 1.0f);
                            }
                        }
                    default:
                        Box2DXDebug.Assert(false);
                        return Vec2.Zero;
                }
            }

            public Vec2 GetClosestPoint()
            {
                switch (Count)
                {
                    case 0:
                        Box2DXDebug.Assert(false);
                        return Vec2.Zero;

                    case 1:
                        return Vertices[0].W;

                    case 2:
                        return Vertices[0].A * Vertices[0].W + Vertices[1].A * Vertices[1].W;

                    case 3:
                        return Vec2.Zero;

                    default:
                        Box2DXDebug.Assert(false);
                        return Vec2.Zero;
                }
            }

            public void GetWitnessPoints(out Vec2 pA, out Vec2 pB)
            {
                pA = Vec2.Zero;
                pB = Vec2.Zero;

                switch (Count)
                {
                    case 0:
                        Box2DXDebug.Assert(false);
                        break;

                    case 1:
                        pA = Vertices[0].WA;
                        pB = Vertices[0].WB;
                        break;

                    case 2:
                        pA = Vertices[0].A * Vertices[0].WA + Vertices[1].A * Vertices[1].WA;
                        pB = Vertices[0].A * Vertices[0].WB + Vertices[1].A * Vertices[1].WB;
                        break;

                    case 3:
                        pA = Vertices[0].A * Vertices[0].WA + Vertices[1].A * Vertices[1].WA + Vertices[2].A * Vertices[2].WA;
                        pB = pA;
                        break;

                    default:
                        Box2DXDebug.Assert(false);
                        break;
                }
            }

            public float GetMetric()
            {
                switch (Count)
                {
                    case 0:
                        Box2DXDebug.Assert(false);
                        return 0.0f;

                    case 1:
                        return 0.0f;

                    case 2:
                        return Vec2.Distance(Vertices[0].W, Vertices[1].W);

                    case 3:
                        return Vec2.Cross(Vertices[1].W - Vertices[0].W, Vertices[2].W - Vertices[0].W);

                    default:
                        Box2DXDebug.Assert(false);
                        return 0.0f;
                }
            }

            /// <summary>
            /// Solve a line segment using barycentric coordinates.
            ///
            /// p = a1 * w1 + a2 * w2
            /// a1 + a2 = 1
            ///
            /// The vector from the origin to the closest point on the line is
            /// perpendicular to the line.
            /// e12 = w2 - w1
            /// dot(p, e) = 0
            /// a1 * dot(w1, e) + a2 * dot(w2, e) = 0
            ///
            /// 2-by-2 linear system
            /// [1      1     ][a1] = [1]
            /// [w1.e12 w2.e12][a2] = [0]
            ///
            /// Define
            /// d12_1 =  dot(w2, e12)
            /// d12_2 = -dot(w1, e12)
            /// d12 = d12_1 + d12_2
            ///
            /// Solution
            /// a1 = d12_1 / d12
            ///  a2 = d12_2 / d12
            /// </summary>
            public void Solve2()
            {
                Vec2 w1 = Vertices[0].W;
                Vec2 w2 = Vertices[1].W;
                Vec2 e12 = w2 - w1;

                // w1 region
                float d12_2 = -Vec2.Dot(w1, e12);
                if (d12_2 <= 0.0f)
                {
                    // a2 <= 0, so we clamp it to 0
                    Vertices[0].A = 1.0f;
                    Count = 1;
                    return;
                }

                // w2 region
                float d12_1 = Vec2.Dot(w2, e12);
                if (d12_1 <= 0.0f)
                {
                    // a1 <= 0, so we clamp it to 0
                    Vertices[1].A = 1.0f;
                    Count = 1;
                    Vertices[0] = Vertices[1];
                    return;
                }

                // Must be in e12 region.
                float inv_d12 = 1.0f / (d12_1 + d12_2);
                Vertices[0].A = d12_1 * inv_d12;
                Vertices[1].A = d12_2 * inv_d12;
                Count = 2;
            }

            /// <summary>
            /// Possible regions:
            /// - points[2]
            /// - edge points[0]-points[2]
            /// - edge points[1]-points[2]
            /// - inside the triangle
            /// </summary>
            public void Solve3()
            {
                Vec2 w1 = Vertices[0].W;
                Vec2 w2 = Vertices[1].W;
                Vec2 w3 = Vertices[2].W;

                // Edge12
                // [1      1     ][a1] = [1]
                // [w1.e12 w2.e12][a2] = [0]
                // a3 = 0
                Vec2 e12 = w2 - w1;
                float w1e12 = Vec2.Dot(w1, e12);
                float w2e12 = Vec2.Dot(w2, e12);
                float d12_1 = w2e12;
                float d12_2 = -w1e12;

                // Edge13
                // [1      1     ][a1] = [1]
                // [w1.e13 w3.e13][a3] = [0]
                // a2 = 0
                Vec2 e13 = w3 - w1;
                float w1e13 = Vec2.Dot(w1, e13);
                float w3e13 = Vec2.Dot(w3, e13);
                float d13_1 = w3e13;
                float d13_2 = -w1e13;

                // Edge23
                // [1      1     ][a2] = [1]
                // [w2.e23 w3.e23][a3] = [0]
                // a1 = 0
                Vec2 e23 = w3 - w2;
                float w2e23 = Vec2.Dot(w2, e23);
                float w3e23 = Vec2.Dot(w3, e23);
                float d23_1 = w3e23;
                float d23_2 = -w2e23;

                // Triangle123
                float n123 = Vec2.Cross(e12, e13);

                float d123_1 = n123 * Vec2.Cross(w2, w3);
                float d123_2 = n123 * Vec2.Cross(w3, w1);
                float d123_3 = n123 * Vec2.Cross(w1, w2);

                // w1 region
                if (d12_2 <= 0.0f && d13_2 <= 0.0f)
                {
                    Vertices[0].A = 1.0f;
                    Count = 1;
                    return;
                }

                // e12
                if (d12_1 > 0.0f && d12_2 > 0.0f && d123_3 <= 0.0f)
                {
                    float inv_d12 = 1.0f / (d12_1 + d12_2);
                    Vertices[0].A = d12_1 * inv_d12;
                    Vertices[1].A = d12_1 * inv_d12;
                    Count = 2;
                    return;
                }

                // e13
                if (d13_1 > 0.0f && d13_2 > 0.0f && d123_2 <= 0.0f)
                {
                    float inv_d13 = 1.0f / (d13_1 + d13_2);
                    Vertices[0].A = d13_1 * inv_d13;
                    Vertices[2].A = d13_2 * inv_d13;
                    Count = 2;
                    Vertices[1] = Vertices[2];
                    return;
                }

                // w2 region
                if (d12_1 <= 0.0f && d23_2 <= 0.0f)
                {
                    Vertices[1].A = 1.0f;
                    Count = 1;
                    Vertices[0] = Vertices[1];
                    return;
                }

                // w3 region
                if (d13_1 <= 0.0f && d23_1 <= 0.0f)
                {
                    Vertices[2].A = 1.0f;
                    Count = 1;
                    Vertices[0] = Vertices[2];
                    return;
                }

                // e23
                if (d23_1 > 0.0f && d23_2 > 0.0f && d123_1 <= 0.0f)
                {
                    float inv_d23 = 1.0f / (d23_1 + d23_2);
                    Vertices[1].A = d23_1 * inv_d23;
                    Vertices[2].A = d23_2 * inv_d23;
                    Count = 2;
                    Vertices[0] = Vertices[2];
                    return;
                }

                // Must be in triangle123
                float inv_d123 = 1.0f / (d123_1 + d123_2 + d123_3);
                Vertices[0].A = d123_1 * inv_d123;
                Vertices[1].A = d123_2 * inv_d123;
                Vertices[2].A = d123_3 * inv_d123;
                Count = 3;
            }

            public SimplexVertex[] Vertices = new SimplexVertex[3];
            public int Count;
        };

        /// <summary>
        /// Compute the closest points between two shapes. Supports any combination of:
        /// b2CircleShape, b2PolygonShape, b2EdgeShape. The simplex cache is input/output.
        /// On the first call set b2SimplexCache.count to zero.
        /// </summary>
        public static void Distance(out DistanceOutput output,
                        SimplexCache cache,
                        DistanceInput input)
        {
            ++GjkCalls;

            DistanceProxy proxyA = input.proxyA;
            DistanceProxy proxyB = input.proxyB;

            Transform transformA = input.TransformA;
            Transform transformB = input.TransformB;

            // Initialize the simplex.
            Simplex simplex = new Simplex();
            simplex.ReadCache(cache, proxyA, ref  transformA, proxyB, ref transformB);

            // Get simplex vertices as an array.
            SimplexVertex[] vertices = simplex.Vertices;
            const int k_maxIters = 20;

            // These store the vertices of the last simplex so that we
            // can check for duplicates and prevent cycling.
            int[] saveA = new int[3], saveB = new int[3];
            int saveCount = 0;

            Vec2 closestPoint = simplex.GetClosestPoint();
            float distanceSqr1 = closestPoint.LengthSquared();
            float distanceSqr2 = distanceSqr1;

            // Main iteration loop.
            int iter = 0;
            while (iter < k_maxIters)
            {
                // Copy simplex so we can identify duplicates.
                saveCount = simplex.Count;
                for (int i = 0; i < saveCount; ++i)
                {
                    saveA[i] = vertices[i].IndexA;
                    saveB[i] = vertices[i].IndexB;
                }

                switch (simplex.Count)
                {
                    case 1:
                        break;

                    case 2:
                        simplex.Solve2();
                        break;

                    case 3:
                        simplex.Solve3();
                        break;

                    default:
                        Box2DXDebug.Assert(false);
                        break;
                }

                // If we have 3 points, then the origin is in the corresponding triangle.
                if (simplex.Count == 3)
                {
                    break;
                }

                // Compute closest point.
                Vec2 p = simplex.GetClosestPoint();
                float distanceSqr = p.LengthSquared();

                // Ensure progress
                if (distanceSqr2 >= distanceSqr1)
                {
                    //break;
                }
                distanceSqr1 = distanceSqr2;

                // Get search direction.
                Vec2 d = simplex.GetSearchDirection();

                // Ensure the search direction is numerically fit.
                if (d.LengthSquared() < Settings.FLT_EPSILON * Settings.FLT_EPSILON)
                {
                    // The origin is probably contained by a line segment
                    // or triangle. Thus the shapes are overlapped.

                    // We can't return zero here even though there may be overlap.
                    // In case the simplex is a point, segment, or triangle it is difficult
                    // to determine if the origin is contained in the CSO or very close to it.
                    break;
                }
                // Compute a tentative new simplex vertex using support points.
                SimplexVertex vertex = vertices[simplex.Count];
                vertex.IndexA = proxyA.GetSupport(Math.MulT(transformA.R, -d));
                vertex.WA = Math.Mul(transformA, proxyA.GetVertex(vertex.IndexA));
                Vec2 wBLocal;
                vertex.IndexB = proxyB.GetSupport(Math.MulT(transformB.R, d));
                vertex.WB = Math.Mul(transformB, proxyB.GetVertex(vertex.IndexB));
                vertex.W = vertex.WB - vertex.WA;

                // Iteration count is equated to the number of support point calls.
                ++iter;
                ++GjkIters;

                // Check for duplicate support points. This is the main termination criteria.
                bool duplicate = false;
                for (int i = 0; i < saveCount; ++i)
                {
                    if (vertex.IndexA == saveA[i] && vertex.IndexB == saveB[i])
                    {
                        duplicate = true;
                        break;
                    }
                }

                // If we found a duplicate support point we must exit to avoid cycling.
                if (duplicate)
                {
                    break;
                }

                // New vertex is ok and needed.
                ++simplex.Count;
            }

            GjkMaxIters = Math.Max(GjkMaxIters, iter);

            // Prepare output.
            simplex.GetWitnessPoints(out output.PointA, out output.PointB);
            output.Distance = Vec2.Distance(output.PointA, output.PointB);
            output.Iterations = iter;

            // Cache the simplex.
            simplex.WriteCache(cache);

            // Apply radii if requested.
            if (input.UseRadii)
            {
                float rA = proxyA._radius;
                float rB = proxyB._radius;

                if (output.Distance > rA + rB && output.Distance > Settings.FLT_EPSILON)
                {
                    // Shapes are still no overlapped.
                    // Move the witness points to the outer surface.
                    output.Distance -= rA + rB;
                    Vec2 normal = output.PointB - output.PointA;
                    normal.Normalize();
                    output.PointA += rA * normal;
                    output.PointB -= rB * normal;
                }
                else
                {
                    // Shapes are overlapped when radii are considered.
                    // Move the witness points to the middle.
                    Vec2 p = 0.5f * (output.PointA + output.PointB);
                    output.PointA = p;
                    output.PointB = p;
                    output.Distance = 0.0f;
                }
            }
        }
    }
}