﻿/*
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
        /// Find the separation between poly1 and poly2 for a give edge normal on poly1.
        /// </summary>
        /// <param name="poly1"></param>
        /// <param name="xf1"></param>
        /// <param name="edge1"></param>
        /// <param name="poly2"></param>
        /// <param name="xf2"></param>
        /// <returns></returns>
        public static float EdgeSeparation(PolygonShape poly1, Transform xf1, int edge1,
            PolygonShape poly2, Transform xf2)
        {
            int count1 = poly1.VertexCount;
            Vec2[] vertices1 = poly1.Vertices;
            Vec2[] normals1 = poly1.Normals;

            int count2 = poly2.VertexCount;
            Vec2[] vertices2 = poly2.Vertices;

            Box2DXDebug.Assert(0 <= edge1 && edge1 < count1);

            // Convert normal from poly1's frame into poly2's frame.
            Vec2 normal1World = Math.Mul(xf1.R, normals1[edge1]);
            Vec2 normal1 = Math.MulT(xf2.R, normal1World);

            // Find support vertex on poly2 for -normal.
            int index = 0;
            float minDot = Settings.FLT_MAX;

            for (int i = 0; i < count2; ++i)
            {
                float dot = Vec2.Dot(vertices2[i], normal1);
                if (dot < minDot)
                {
                    minDot = dot;
                    index = i;
                }
            }

            Vec2 v1 = Math.Mul(xf1, vertices1[edge1]);
            Vec2 v2 = Math.Mul(xf2, vertices2[index]);
            float separation = Vec2.Dot(v2 - v1, normal1World);
            return separation;
        }

        /// <summary>
        /// Find the max separation between poly1 and poly2 using edge normals from poly1.
        /// </summary>
        /// <param name="edgeIndex"></param>
        /// <param name="poly1"></param>
        /// <param name="xf1"></param>
        /// <param name="poly2"></param>
        /// <param name="xf2"></param>
        /// <returns></returns>
        public static float FindMaxSeparation(out int edgeIndex,
            PolygonShape poly1, Transform xf1,
            PolygonShape poly2, Transform xf2)
        {
            int count1 = poly1.VertexCount;
            Vec2[] normals1 = poly1.Normals;

            // Vector pointing from the centroid of poly1 to the centroid of poly2.
            Vec2 d = Math.Mul(xf2, poly2.Centroid) - Math.Mul(xf1, poly1.Centroid);
            Vec2 dLocal1 = Math.MulT(xf1.R, d);

            // Find edge normal on poly1 that has the largest projection onto d.
            int edge = 0;
            float maxDot = -Settings.FLT_MAX;
            for (int i = 0; i < count1; ++i)
            {
                float dot = Vec2.Dot(normals1[i], dLocal1);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    edge = i;
                }
            }

            // Get the separation for the edge normal.
            float s = EdgeSeparation(poly1, xf1, edge, poly2, xf2);

            // Check the separation for the previous edge normal.
            int prevEdge = edge - 1 >= 0 ? edge - 1 : count1 - 1;
            float sPrev = EdgeSeparation(poly1, xf1, prevEdge, poly2, xf2);

            // Check the separation for the next edge normal.
            int nextEdge = edge + 1 < count1 ? edge + 1 : 0;
            float sNext = EdgeSeparation(poly1, xf1, nextEdge, poly2, xf2);

            // Find the best edge and the search direction.
            int bestEdge;
            float bestSeparation;
            int increment;
            if (sPrev > s && sPrev > sNext)
            {
                increment = -1;
                bestEdge = prevEdge;
                bestSeparation = sPrev;
            }
            else if (sNext > s)
            {
                increment = 1;
                bestEdge = nextEdge;
                bestSeparation = sNext;
            }
            else
            {
                edgeIndex = edge;
                return s;
            }

            // Perform a local search for the best edge normal.
            for (; ; )
            {
                if (increment == -1)
                    edge = bestEdge - 1 >= 0 ? bestEdge - 1 : count1 - 1;
                else
                    edge = bestEdge + 1 < count1 ? bestEdge + 1 : 0;

                s = EdgeSeparation(poly1, xf1, edge, poly2, xf2);

                if (s > bestSeparation)
                {
                    bestEdge = edge;
                    bestSeparation = s;
                }
                else
                {
                    break;
                }
            }

            edgeIndex = bestEdge;
            return bestSeparation;
        }

        public static void FindIncidentEdge(out ClipVertex[] c,
            PolygonShape poly1, Transform xf1, int edge1,
            PolygonShape poly2, Transform xf2)
        {
            int count1 = poly1.VertexCount;
            Vec2[] normals1 = poly1.Normals;

            int count2 = poly2.VertexCount;
            Vec2[] vertices2 = poly2.Vertices;
            Vec2[] normals2 = poly2.Normals;

            Box2DXDebug.Assert(0 <= edge1 && edge1 < count1);

            // Get the normal of the reference edge in poly2's frame.
            Vec2 normal1 = Math.MulT(xf2.R, Math.Mul(xf1.R, normals1[edge1]));

            // Find the incident edge on poly2.
            int index = 0;
            float minDot = Settings.FLT_MAX;
            for (int i = 0; i < count2; ++i)
            {
                float dot = Vec2.Dot(normal1, normals2[i]);
                if (dot < minDot)
                {
                    minDot = dot;
                    index = i;
                }
            }

            // Build the clip vertices for the incident edge.
            int i1 = index;
            int i2 = i1 + 1 < count2 ? i1 + 1 : 0;

            c = new ClipVertex[2];

            c[0].V = Math.Mul(xf2, vertices2[i1]);
            c[0].ID.Features.ReferenceEdge = (byte)edge1;
            c[0].ID.Features.IncidentEdge = (byte)i1;
            c[0].ID.Features.IncidentVertex = 0;

            c[1].V = Math.Mul(xf2, vertices2[i2]);
            c[1].ID.Features.ReferenceEdge = (byte)edge1;
            c[1].ID.Features.IncidentEdge = (byte)i2;
            c[1].ID.Features.IncidentVertex = 1;
        }

        // Find edge normal of max separation on A - return if separating axis is found
        // Find edge normal of max separation on B - return if separation axis is found
        // Choose reference edge as min(minA, minB)
        // Find incident edge
        // Clip
        // The normal points from 1 to 2
        public static void CollidePolygons(out Manifold manifold,
            PolygonShape polyA, Transform xfA,
            PolygonShape polyB, Transform xfB)
        {
            manifold = new Manifold();
            manifold.PointCount = 0;
            float totalRadius = polyA._radius + polyB._radius;

            int edgeA = 0;
            float separationA = FindMaxSeparation(out edgeA, polyA, xfA, polyB, xfB);
            if (separationA > totalRadius)
                return;

            int edgeB = 0;
            float separationB = FindMaxSeparation(out edgeB, polyB, xfB, polyA, xfA);
            if (separationB > totalRadius)
                return;

            PolygonShape poly1;	// reference poly
            PolygonShape poly2;	// incident poly
            Transform xf1, xf2;
            int edge1;		// reference edge
            byte flip;
            const float k_relativeTol = 0.98f;
            const float k_absoluteTol = 0.001f;

            if (separationB > k_relativeTol * separationA + k_absoluteTol)
            {
                poly1 = polyB;
                poly2 = polyA;
                xf1 = xfB;
                xf2 = xfA;
                edge1 = edgeB;
                manifold.Type = Manifold.ManifoldType.FaceB;
                flip = 1;
            }
            else
            {
                poly1 = polyA;
                poly2 = polyB;
                xf1 = xfA;
                xf2 = xfB;
                edge1 = edgeA;
                manifold.Type = Manifold.ManifoldType.FaceA;
                flip = 0;
            }

            ClipVertex[] incidentEdge;
            FindIncidentEdge(out incidentEdge, poly1, xf1, edge1, poly2, xf2);

            int count1 = poly1.VertexCount;
            Vec2[] vertices1 = poly1.Vertices;

            Vec2 v11 = vertices1[edge1];
            Vec2 v12 = edge1 + 1 < count1 ? vertices1[edge1 + 1] : vertices1[0];

            Vec2 localTangent = v12 - v11;
            localTangent.Normalize();

            Vec2 localNormal = Vec2.Cross(localTangent, 1.0f);
            Vec2 planePoint = 0.5f * (v11 + v12);

            Vec2 tangent = Math.Mul(xf1.R, localTangent);
            Vec2 normal = Vec2.Cross(tangent, 1.0f);

            v11 = Math.Mul(xf1, v11);
            v12 = Math.Mul(xf1, v12);

            // Face offset.
            float frontOffset = Vec2.Dot(normal, v11);

            // Side offsets, extended by polytope skin thickness.
            float sideOffset1 = -Vec2.Dot(tangent, v11) + totalRadius;
            float sideOffset2 = Vec2.Dot(tangent, v12) + totalRadius;

            // Clip incident edge against extruded edge1 side edges.
            ClipVertex[] clipPoints1;
            ClipVertex[] clipPoints2;
            int np;

            // Clip to box side 1
            np = ClipSegmentToLine(out clipPoints1, ref incidentEdge, -tangent, sideOffset1);

            if (np < 2)
            {
                return;
            }

            // Clip to negative box side 1
            np = ClipSegmentToLine(out clipPoints2, ref clipPoints1, tangent, sideOffset2);

            if (np < 2)
            {
                return;
            }

            // Now clipPoints2 contains the clipped points.
            manifold.LocalPlaneNormal = localNormal;
            manifold.LocalPoint = planePoint;

            int pointCount = 0;
            for (int i = 0; i < Settings.MaxManifoldPoints; ++i)
            {
                float separation = Vec2.Dot(normal, clipPoints2[i].V) - frontOffset;

                if (separation <= totalRadius)
                {
                    ManifoldPoint cp = manifold.Points[pointCount];
                    cp.LocalPoint = Math.MulT(xf2, clipPoints2[i].V);
                    cp.ID = clipPoints2[i].ID;
                    cp.ID.Features.Flip = flip;
                    ++pointCount;
                }
            }

            manifold.PointCount = pointCount;
        }
    }
}