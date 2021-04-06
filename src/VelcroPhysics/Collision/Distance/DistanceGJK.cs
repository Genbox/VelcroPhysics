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

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.Narrowphase;
using VelcroPhysics.Shared.Optimization;
using VelcroPhysics.Utilities;

namespace VelcroPhysics.Collision.Distance
{
    /// <summary>
    /// The Gilbert–Johnson–Keerthi distance algorithm that provides the distance between shapes.
    /// Using Voronoi regions (Christer Ericson) and Barycentric coordinates.
    /// </summary>
    public static class DistanceGJK
    {
        /// <summary>
        /// The number of calls made to the ComputeDistance() function.
        /// Note: This is only activated when Settings.EnableDiagnostics = true
        /// </summary>
        [ThreadStatic]
        public static int GJKCalls;

        /// <summary>
        /// The number of iterations that was made on the last call to ComputeDistance().
        /// Note: This is only activated when Settings.EnableDiagnostics = true
        /// </summary>
        [ThreadStatic]
        public static int GJKIters;

        /// <summary>
        /// The maximum number of iterations calls to the CompteDistance() function.
        /// Note: This is only activated when Settings.EnableDiagnostics = true
        /// </summary>
        [ThreadStatic]
        public static int GJKMaxIters;

        public static void ComputeDistance(ref DistanceInput input, out DistanceOutput output, out SimplexCache cache)
        {
            cache = new SimplexCache();

            if (Settings.EnableDiagnostics) //Velcro: We only gather diagnostics when enabled
                ++GJKCalls;

            // Initialize the simplex.
            Simplex simplex = new Simplex();
            simplex.ReadCache(ref cache, ref input.ProxyA, ref input.TransformA, ref input.ProxyB, ref input.TransformB);

            // These store the vertices of the last simplex so that we
            // can check for duplicates and prevent cycling.
            FixedArray3<int> saveA = new FixedArray3<int>();
            FixedArray3<int> saveB = new FixedArray3<int>();

            //Velcro: This code was not used anyway.
            //float distanceSqr1 = Settings.MaxFloat;

            // Main iteration loop.
            int iter = 0;
            while (iter < Settings.MaxGJKIterations)
            {
                // Copy simplex so we can identify duplicates.
                int saveCount = simplex.Count;
                for (int i = 0; i < saveCount; ++i)
                {
                    saveA[i] = simplex.V[i].IndexA;
                    saveB[i] = simplex.V[i].IndexB;
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
                        Debug.Assert(false);
                        break;
                }

                // If we have 3 points, then the origin is in the corresponding triangle.
                if (simplex.Count == 3)
                {
                    break;
                }

                // Get search direction.
                Vector2 d = simplex.GetSearchDirection();

                // Ensure the search direction is numerically fit.
                if (d.LengthSquared() < Settings.Epsilon * Settings.Epsilon)
                {
                    // The origin is probably contained by a line segment
                    // or triangle. Thus the shapes are overlapped.

                    // We can't return zero here even though there may be overlap.
                    // In case the simplex is a point, segment, or triangle it is difficult
                    // to determine if the origin is contained in the CSO or very close to it.
                    break;
                }

                // Compute a tentative new simplex vertex using support points.
                SimplexVertex vertex = simplex.V[simplex.Count];
                vertex.IndexA = input.ProxyA.GetSupport(MathUtils.MulT(input.TransformA.q, -d));
                vertex.WA = MathUtils.Mul(ref input.TransformA, input.ProxyA.Vertices[vertex.IndexA]);

                vertex.IndexB = input.ProxyB.GetSupport(MathUtils.MulT(input.TransformB.q, d));
                vertex.WB = MathUtils.Mul(ref input.TransformB, input.ProxyB.Vertices[vertex.IndexB]);
                vertex.W = vertex.WB - vertex.WA;
                simplex.V[simplex.Count] = vertex;

                // Iteration count is equated to the number of support point calls.
                ++iter;

                if (Settings.EnableDiagnostics) //Velcro: We only gather diagnostics when enabled
                    ++GJKIters;

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

                // New vertex is OK and needed.
                ++simplex.Count;
            }

            if (Settings.EnableDiagnostics) //Velcro: We only gather diagnostics when enabled
                GJKMaxIters = Math.Max(GJKMaxIters, iter);

            // Prepare output.
            simplex.GetWitnessPoints(out output.PointA, out output.PointB);
            output.Distance = (output.PointA - output.PointB).Length();
            output.Iterations = iter;

            // Cache the simplex.
            simplex.WriteCache(ref cache);

            // Apply radii if requested.
            if (input.UseRadii)
            {
                float rA = input.ProxyA.Radius;
                float rB = input.ProxyB.Radius;

                if (output.Distance > rA + rB && output.Distance > Settings.Epsilon)
                {
                    // Shapes are still no overlapped.
                    // Move the witness points to the outer surface.
                    output.Distance -= rA + rB;
                    Vector2 normal = output.PointB - output.PointA;
                    normal.Normalize();
                    output.PointA += rA * normal;
                    output.PointB -= rB * normal;
                }
                else
                {
                    // Shapes are overlapped when radii are considered.
                    // Move the witness points to the middle.
                    Vector2 p = 0.5f * (output.PointA + output.PointB);
                    output.PointA = p;
                    output.PointB = p;
                    output.Distance = 0.0f;
                }
            }
        }
    }
}