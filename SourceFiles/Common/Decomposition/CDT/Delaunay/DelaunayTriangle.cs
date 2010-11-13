/* Poly2Tri
 * Copyright (c) 2009-2010, Poly2Tri Contributors
 * http://code.google.com/p/poly2tri/
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * * Redistributions of source code must retain the above copyright notice,
 *   this list of conditions and the following disclaimer.
 * * Redistributions in binary form must reproduce the above copyright notice,
 *   this list of conditions and the following disclaimer in the documentation
 *   and/or other materials provided with the distribution.
 * * Neither the name of Poly2Tri nor the names of its contributors may be
 *   used to endorse or promote products derived from this software without specific
 *   prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Diagnostics;
using FarseerPhysics.Common.Decomposition.CDT.Polygon;
using FarseerPhysics.Common.Decomposition.CDT.Util;

namespace FarseerPhysics.Common.Decomposition.CDT.Delaunay
{
    public class DelaunayTriangle
    {
        public FixedBitArray3 EdgeIsConstrained, EdgeIsDelaunay;
        public CDTFixedArray3<DelaunayTriangle> Neighbors;
        public CDTFixedArray3<PolygonPoint> Points;

        public DelaunayTriangle(PolygonPoint p1, PolygonPoint p2, PolygonPoint p3)
        {
            Points[0] = p1;
            Points[1] = p2;
            Points[2] = p3;
        }

        public bool IsInterior { get; set; }

        public int IndexOf(PolygonPoint p)
        {
            int i = Points.IndexOf(p);
            if (i == -1) throw new Exception("Calling index with a point that doesn't exist in triangle");
            return i;
        }

        private int IndexCCWFrom(PolygonPoint p)
        {
            return (IndexOf(p) + 1) % 3;
        }

        public bool Contains(PolygonPoint p)
        {
            return Points.Contains(p);
        }

        /// <summary>
        /// Update neighbor pointers
        /// </summary>
        /// <param name="p1">Point 1 of the shared edge</param>
        /// <param name="p2">Point 2 of the shared edge</param>
        /// <param name="t">This triangle's new neighbor</param>
        private void MarkNeighbor(PolygonPoint p1, PolygonPoint p2, DelaunayTriangle t)
        {
            int i = EdgeIndex(p1, p2);
            if (i == -1) throw new Exception("Error marking neighbors -- t doesn't contain edge p1-p2!");
            Neighbors[i] = t;
        }

        /// <summary>
        /// Exhaustive search to update neighbor pointers
        /// </summary>
        public void MarkNeighbor(DelaunayTriangle t)
        {
            // Points of this triangle also belonging to t
            bool a = t.Contains(Points[0]);
            bool b = t.Contains(Points[1]);
            bool c = t.Contains(Points[2]);

            if (b && c)
            {
                Neighbors[0] = t;
                t.MarkNeighbor(Points[1], Points[2], this);
            }
            else if (a && c)
            {
                Neighbors[1] = t;
                t.MarkNeighbor(Points[0], Points[2], this);
            }
            else if (a && b)
            {
                Neighbors[2] = t;
                t.MarkNeighbor(Points[0], Points[1], this);
            }
            else throw new Exception("Failed to mark neighbor, doesn't share an edge!");
        }

        /// <param name="t">Opposite triangle</param>
        /// <param name="p">The point in t that isn't shared between the triangles</param>
        public PolygonPoint OppositePoint(DelaunayTriangle t, PolygonPoint p)
        {
            Debug.Assert(t != this, "self-pointer error");
            return PointCWFrom(t.PointCWFrom(p));
        }

        public DelaunayTriangle NeighborCWFrom(PolygonPoint point)
        {
            return Neighbors[(Points.IndexOf(point) + 1) % 3];
        }

        public DelaunayTriangle NeighborCCWFrom(PolygonPoint point)
        {
            return Neighbors[(Points.IndexOf(point) + 2) % 3];
        }

        public DelaunayTriangle NeighborAcrossFrom(PolygonPoint point)
        {
            return Neighbors[Points.IndexOf(point)];
        }

        public PolygonPoint PointCCWFrom(PolygonPoint point)
        {
            return Points[(IndexOf(point) + 1) % 3];
        }

        public PolygonPoint PointCWFrom(PolygonPoint point)
        {
            return Points[(IndexOf(point) + 2) % 3];
        }

        private void RotateCW()
        {
            PolygonPoint t = Points[2];
            Points[2] = Points[1];
            Points[1] = Points[0];
            Points[0] = t;
        }

        /// <summary>
        /// Legalize triangle by rotating clockwise around oPoint
        /// </summary>
        /// <param name="oPoint">The origin point to rotate around</param>
        /// <param name="nPoint">???</param>
        public void Legalize(PolygonPoint oPoint, PolygonPoint nPoint)
        {
            RotateCW();
            Points[IndexCCWFrom(oPoint)] = nPoint;
        }

        public override string ToString()
        {
            return Points[0] + "," + Points[1] + "," + Points[2];
        }

        public void MarkConstrainedEdge(int index)
        {
            EdgeIsConstrained[index] = true;
        }

        /// <summary>
        /// Mark edge as constrained
        /// </summary>
        public void MarkConstrainedEdge(PolygonPoint p, PolygonPoint q)
        {
            int i = EdgeIndex(p, q);
            if (i != -1) EdgeIsConstrained[i] = true;
        }

        /// <summary>
        /// Get the index of the neighbor that shares this edge (or -1 if it isn't shared)
        /// </summary>
        /// <returns>index of the shared edge or -1 if edge isn't shared</returns>
        public int EdgeIndex(PolygonPoint p1, PolygonPoint p2)
        {
            int i1 = Points.IndexOf(p1);
            int i2 = Points.IndexOf(p2);

            // Points of this triangle in the edge p1-p2
            bool a = (i1 == 0 || i2 == 0);
            bool b = (i1 == 1 || i2 == 1);
            bool c = (i1 == 2 || i2 == 2);

            if (b && c) return 0;
            if (a && c) return 1;
            if (a && b) return 2;
            return -1;
        }

        public bool GetConstrainedEdgeCCW(PolygonPoint p)
        {
            return EdgeIsConstrained[(IndexOf(p) + 2) % 3];
        }

        public bool GetConstrainedEdgeCW(PolygonPoint p)
        {
            return EdgeIsConstrained[(IndexOf(p) + 1) % 3];
        }

        public void SetConstrainedEdgeCCW(PolygonPoint p, bool ce)
        {
            EdgeIsConstrained[(IndexOf(p) + 2) % 3] = ce;
        }

        public void SetConstrainedEdgeCW(PolygonPoint p, bool ce)
        {
            EdgeIsConstrained[(IndexOf(p) + 1) % 3] = ce;
        }

        public bool GetDelaunayEdgeCCW(PolygonPoint p)
        {
            return EdgeIsDelaunay[(IndexOf(p) + 2) % 3];
        }

        public bool GetDelaunayEdgeCW(PolygonPoint p)
        {
            return EdgeIsDelaunay[(IndexOf(p) + 1) % 3];
        }

        public void SetDelaunayEdgeCCW(PolygonPoint p, bool ce)
        {
            EdgeIsDelaunay[(IndexOf(p) + 2) % 3] = ce;
        }

        public void SetDelaunayEdgeCW(PolygonPoint p, bool ce)
        {
            EdgeIsDelaunay[(IndexOf(p) + 1) % 3] = ce;
        }
    }
}