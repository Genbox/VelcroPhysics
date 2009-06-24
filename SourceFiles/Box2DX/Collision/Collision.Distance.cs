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
        /// <summary>
        /// Used to warm start b2Distance.
        /// Set count to zero on first call.
        /// </summary>
        public struct SimplexCache
        {
            public float metric;	// length or area
            public ushort count;
#warning "Following two fields must be initialized with size of 3"
            public byte[] indexA;	// vertices on shape A
            public byte[] indexB;	// vertices on shape B
        };

        /// <summary>
        /// Input for b2Distance.
        /// You have to option to use the shape radii
        /// in the computation. Even 
        /// </summary>
        public struct DistanceInput
        {
            public XForm transformA;
            public XForm transformB;
            public bool useRadii;
        };

        /// <summary>
        /// Output for b2Distance.
        /// </summary>
        public struct DistanceOutput
        {
            public Vec2 pointA;		// closest point on shapeA
            public Vec2 pointB;		// closest point on shapeB
            public float distance;
            public int iterations;	// number of GJK iterations used
        };

        public struct SimplexVertex
        {
            public Vec2 wA;		// support point in shapeA
            public Vec2 wB;		// support point in shapeB
            public Vec2 w;		// wB - wA
            public float a;		// barycentric coordinate for closest point
            public int indexA;	// wA index
            public int indexB;	// wB index
        };

        public struct Simplex
        {
            public void ReadCache(SimplexCache cache,
                            Shape shapeA, XForm transformA,
                            Shape shapeB, XForm transformB)
            {
                Box2DXDebug.Assert(0 <= cache.count && cache.count <= 3);

                // Copy data from cache.
                m_count = cache.count;

                for (int i = 0; i < m_count; ++i)
                {
                    SimplexVertex v = m_vlist[i + 1];
                    v.indexA = cache.indexA[i];
                    v.indexB = cache.indexB[i];
                    Vec2 wALocal = shapeA.GetVertex(v.indexA);
                    Vec2 wBLocal = shapeB.GetVertex(v.indexB);
                    v.wA = Math.Mul(transformA, wALocal);
                    v.wB = Math.Mul(transformB, wBLocal);
                    v.w = v.wB - v.wA;
                    v.a = 0.0f;
                }

                // Compute the new simplex metric, if it is substantially different than
                // old metric then flush the simplex.
                if (m_count > 1)
                {
                    float metric1 = cache.metric;
                    float metric2 = GetMetric();
                    if (metric2 < 0.5f * metric1 || 2.0f * metric1 < metric2 || metric2 < Settings.FLT_EPSILON)
                    {
                        // Reset the simplex.
                        m_count = 0;
                    }
                }

                // If the cache is empty or invalid ...
                if (m_count == 0)
                {
                    m_vlist[0] = new SimplexVertex();
                    m_vlist[0].indexA = 0;
                    m_vlist[0].indexB = 0;
                    Vec2 wALocal = shapeA.GetVertex(0);
                    Vec2 wBLocal = shapeB.GetVertex(0);
                    m_vlist[0].wA = Math.Mul(transformA, wALocal);
                    m_vlist[0].wB = Math.Mul(transformB, wBLocal);
                    m_vlist[0].w = m_vlist[0].wB - m_vlist[0].wA;
                    m_count = 1;
                }
            }

            public void WriteCache(ref SimplexCache cache)
            {
                cache.metric = GetMetric();
                cache.count = (ushort)m_count;
                for (int i = 0; i < m_count; ++i)
                {
                    cache.indexA[i] = (byte)m_vlist[i].indexA;
                    cache.indexB[i] = (byte)m_vlist[i].indexB;
                }
            }

            public Vec2 GetClosestPoint()
            {
                switch (m_count)
                {
                    case 0:
                        Box2DXDebug.Assert(false);
                        return Vec2.Zero;

                    case 1:
                        return m_vlist[0].w;

                    case 2:
                        return m_vlist[0].a * m_vlist[0].w + m_vlist[1].a * m_vlist[1].w;

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

                switch (m_count)
                {
                    case 0:
                        Box2DXDebug.Assert(false);
                        break;

                    case 1:
                        pA = m_vlist[0].wA;
                        pB = m_vlist[0].wB;
                        break;

                    case 2:
                        pA = m_vlist[0].a * m_vlist[0].wA + m_vlist[1].a * m_vlist[1].wA;
                        pB = m_vlist[0].a * m_vlist[0].wB + m_vlist[1].a * m_vlist[1].wB;
                        break;

                    case 3:
                        pA = m_vlist[0].a * m_vlist[0].wA + m_vlist[1].a * m_vlist[1].wA + m_vlist[2].a * m_vlist[2].wA;
                        pB = pA;
                        break;

                    default:
                        Box2DXDebug.Assert(false);
                        break;
                }
            }

            public float GetMetric()
            {
                switch (m_count)
                {
                    case 0:
                        Box2DXDebug.Assert(false);
                        return 0.0f;

                    case 1:
                        return 0.0f;

                    case 2:
                        return Vec2.Distance(m_vlist[0].w, m_vlist[1].w);

                    case 3:
                        return Vec2.Cross(m_vlist[1].w - m_vlist[0].w, m_vlist[2].w - m_vlist[0].w);

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
                Vec2 w1 = m_vlist[0].w;
                Vec2 w2 = m_vlist[1].w;
                Vec2 e12 = w2 - w1;

                // w1 region
                float d12_2 = -Vec2.Dot(w1, e12);
                if (d12_2 <= 0.0f)
                {
                    // a2 <= 0, so we clamp it to 0
                    m_vlist[0].a = 1.0f;
                    m_count = 1;
                    return;
                }

                // w2 region
                float d12_1 = Vec2.Dot(w2, e12);
                if (d12_1 <= 0.0f)
                {
                    // a1 <= 0, so we clamp it to 0
                    m_vlist[1].a = 1.0f;
                    m_count = 1;
                    m_vlist[0] = m_vlist[1];
                    return;
                }

                // Must be in e12 region.
                float inv_d12 = 1.0f / (d12_1 + d12_2);
                m_vlist[0].a = d12_1 * inv_d12;
                m_vlist[1].a = d12_2 * inv_d12;
                m_count = 2;
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
                Vec2 w1 = m_vlist[0].w;
                Vec2 w2 = m_vlist[1].w;
                Vec2 w3 = m_vlist[2].w;

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
                    m_vlist[0].a = 1.0f;
                    m_count = 1;
                    return;
                }

                // e12
                if (d12_1 > 0.0f && d12_2 > 0.0f && d123_3 <= 0.0f)
                {
                    float inv_d12 = 1.0f / (d12_1 + d12_2);
                    m_vlist[0].a = d12_1 * inv_d12;
                    m_vlist[1].a = d12_1 * inv_d12;
                    m_count = 2;
                    return;
                }

                // e13
                if (d13_1 > 0.0f && d13_2 > 0.0f && d123_2 <= 0.0f)
                {
                    float inv_d13 = 1.0f / (d13_1 + d13_2);
                    m_vlist[0].a = d13_1 * inv_d13;
                    m_vlist[2].a = d13_2 * inv_d13;
                    m_count = 2;
                    m_vlist[1] = m_vlist[2];
                    return;
                }

                // w2 region
                if (d12_1 <= 0.0f && d23_2 <= 0.0f)
                {
                    m_vlist[1].a = 1.0f;
                    m_count = 1;
                    m_vlist[0] = m_vlist[1];
                    return;
                }

                // w3 region
                if (d13_1 <= 0.0f && d23_1 <= 0.0f)
                {
                    m_vlist[2].a = 1.0f;
                    m_count = 1;
                    m_vlist[0] = m_vlist[2];
                    return;
                }

                // e23
                if (d23_1 > 0.0f && d23_2 > 0.0f && d123_1 <= 0.0f)
                {
                    float inv_d23 = 1.0f / (d23_1 + d23_2);
                    m_vlist[1].a = d23_1 * inv_d23;
                    m_vlist[2].a = d23_2 * inv_d23;
                    m_count = 2;
                    m_vlist[0] = m_vlist[2];
                    return;
                }

                // Must be in triangle123
                float inv_d123 = 1.0f / (d123_1 + d123_2 + d123_3);
                m_vlist[0].a = d123_1 * inv_d123;
                m_vlist[1].a = d123_2 * inv_d123;
                m_vlist[2].a = d123_3 * inv_d123;
                m_count = 3;
            }
#warning "Must be initialized to a size of 3"
            public SimplexVertex[] m_vlist;
            public int m_count;
        };

        /// <summary>
        /// Compute the closest points between two shapes. Supports any combination of:
        /// b2CircleShape, b2PolygonShape, b2EdgeShape. The simplex cache is input/output.
        /// On the first call set b2SimplexCache.count to zero.
        /// </summary>
        public static void Distance(out DistanceOutput output,
                        SimplexCache cache,
                        DistanceInput input,
                        Shape shapeA,
                        Shape shapeB)
        {
            XForm transformA = input.transformA;
            XForm transformB = input.transformB;

            // Initialize the simplex.
            Simplex simplex = new Simplex();
            simplex.m_vlist = new SimplexVertex[3];
            simplex.ReadCache(cache, shapeA, transformA, shapeB, transformB);

            // These store the vertices of the last simplex so that we
            // can check for duplicates and prevent cycling.
            int[] lastA = new int[4];
            int[] lastB = new int[4];
            int lastCount;

            // Main iteration loop.
            int iter = 0;
            const int k_maxIterationCount = 20;
            while (iter < k_maxIterationCount)
            {
                // Copy simplex so we can identify duplicates.
                lastCount = simplex.m_count;
                for (int i = 0; i < lastCount; ++i)
                {
                    lastA[i] = simplex.m_vlist[i].indexA;
                    lastB[i] = simplex.m_vlist[i].indexB;
                }

                switch (simplex.m_count)
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
                if (simplex.m_count == 3)
                {
                    break;
                }

                // Compute closest point.
                Vec2 p = simplex.GetClosestPoint();
                float distanceSqr = p.LengthSquared();

                // Ensure the search direction is numerically fit.
                if (distanceSqr < Settings.FLT_EPSILON * Settings.FLT_EPSILON)
                {
                    // The origin is probably contained by a line segment
                    // or triangle. Thus the shapes are overlapped.

                    // We can't return zero here even though there may be overlap.
                    // In case the simplex is a point, segment, or triangle it is difficult
                    // to determine if the origin is contained in the CSO or very close to it.
                    break;
                }

                // Compute a tentative new simplex vertex using support points.
                SimplexVertex vertex = new SimplexVertex();
                vertex.indexA = shapeA.GetSupport(Math.MulT(transformA.R, p));
                vertex.wA = Math.Mul(transformA, shapeA.GetVertex(vertex.indexA));
                Vec2 wBLocal;
                vertex.indexB = shapeB.GetSupport(Math.MulT(transformB.R, -p));
                vertex.wB = Math.Mul(transformB, shapeB.GetVertex(vertex.indexB));
                vertex.w = vertex.wB - vertex.wA;
                simplex.m_vlist[simplex.m_count + 1] = vertex;

                // Iteration count is equated to the number of support point calls.
                ++iter;

                // Check for convergence.
                float lowerBound = Vec2.Dot(p, vertex.w);
                float upperBound = distanceSqr;
                const float k_relativeTolSqr = 0.01f * 0.01f;	// 1:100
                if (upperBound - lowerBound <= k_relativeTolSqr * upperBound)
                {
                    // Converged!
                    break;
                }

                // Check for duplicate support points.
                bool duplicate = false;
                for (int i = 0; i < lastCount; ++i)
                {
                    if (vertex.indexA == lastA[i] && vertex.indexB == lastB[i])
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
                ++simplex.m_count;
            }

            // Prepare output.
#warning "Check if pointer is converted correctly"
            simplex.GetWitnessPoints(out output.pointA, out output.pointB);
            output.distance = Vec2.Distance(output.pointA, output.pointB);
            output.iterations = iter;

            // Cache the simplex.
            simplex.WriteCache(ref cache);

            // Apply radii if requested.
            if (input.useRadii)
            {
                float rA = shapeA.Radius;
                float rB = shapeB.Radius;

                if (output.distance > rA + rB && output.distance > Settings.FLT_EPSILON)
                {
                    // Shapes are still no overlapped.
                    // Move the witness points to the outer surface.
                    output.distance -= rA + rB;
                    Vec2 normal = output.pointB - output.pointA;
                    normal.Normalize();
                    output.pointA += rA * normal;
                    output.pointB -= rB * normal;
                }
                else
                {
                    // Shapes are overlapped when radii are considered.
                    // Move the witness points to the middle.
                    Vec2 p = 0.5f * (output.pointA + output.pointB);
                    output.pointA = p;
                    output.pointB = p;
                    output.distance = 0.0f;
                }
            }
        }
    }
}