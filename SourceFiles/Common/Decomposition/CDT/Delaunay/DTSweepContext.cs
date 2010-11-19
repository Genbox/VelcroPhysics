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

using System.Collections.Generic;
using FarseerPhysics.Common.Decomposition.CDT.Polygon;

namespace FarseerPhysics.Common.Decomposition.CDT.Delaunay
{
    public class DTSweepContext
    {
        // Inital triangle factor, seed triangle will extend 30% of 
        // PointSet width to both left and right.
        private const float ALPHA = 0.3f;
        public readonly List<PolygonPoint> Points = new List<PolygonPoint>(200);
        public readonly List<DelaunayTriangle> Triangles = new List<DelaunayTriangle>();
        public List<DelaunayTriangle> trianglesCleaned = new List<DelaunayTriangle>();
        private Polygon.Polygon Polygon { get; set; }

        public DTSweepBasin Basin = new DTSweepBasin();
        public DTSweepEdgeEvent EdgeEvent = new DTSweepEdgeEvent();
        public AdvancingFront Front;

        private DTSweepPointComparator _comparator = new DTSweepPointComparator();

        public DTSweepContext()
        {
            Clear();
        }
        public int StepCount { get; private set; }

        public void Done()
        {
            StepCount++;
        }

        public PolygonPoint Head { get; set; }
        public PolygonPoint Tail { get; set; }

        public void MeshClean(DelaunayTriangle triangle)
        {
            MeshCleanReq(triangle);
        }

        private void MeshCleanReq(DelaunayTriangle triangle)
        {
            if (triangle != null && !triangle.IsInterior)
            {
                triangle.IsInterior = true;
                trianglesCleaned.Add(triangle);

                for (int i = 0; i < 3; i++)
                    if (!triangle.EdgeIsConstrained[i])
                    {
                        MeshCleanReq(triangle.Neighbors[i]);
                    }
            }
        }

        private void Clear()
        {
            Points.Clear();
            StepCount = 0;

            Triangles.Clear();
        }

        public AdvancingFrontNode LocateNode(PolygonPoint point)
        {
            return Front.LocateNode(point);
        }

        public void CreateAdvancingFront()
        {
            AdvancingFrontNode head, tail, middle;
            // Initial triangle
            DelaunayTriangle iTriangle = new DelaunayTriangle(Points[0], Tail, Head);
            Triangles.Add(iTriangle);

            head = new AdvancingFrontNode(iTriangle.Points[1]);
            head.Triangle = iTriangle;
            middle = new AdvancingFrontNode(iTriangle.Points[0]);
            middle.Triangle = iTriangle;
            tail = new AdvancingFrontNode(iTriangle.Points[2]);

            Front = new AdvancingFront(head, tail);

            // TODO: I think it would be more intuitive if head is middles next and not previous
            //       so swap head and tail
            Front.Head.Next = middle;
            middle.Next = Front.Tail;
            middle.Prev = Front.Head;
            Front.Tail.Prev = middle;
        }

        /// <summary>
        /// Try to map a node to all sides of this triangle that don't have 
        /// a neighbor.
        /// </summary>
        public void MapTriangleToNodes(DelaunayTriangle t)
        {
            for (int i = 0; i < 3; i++)
                if (t.Neighbors[i] == null)
                {
                    AdvancingFrontNode n = Front.LocatePoint(t.PointCWFrom(t.Points[i]));
                    if (n != null) n.Triangle = t;
                }
        }

        public void PrepareTriangulation(Polygon.Polygon t)
        {
            Polygon = t;

            // Outer constraints
            for (int i = 0; i < Polygon.Points.Count - 1; i++)
            {
                DTSweep.CreateSweepConstraint(Polygon.Points[i], Polygon.Points[i + 1]);
            }
            DTSweep.CreateSweepConstraint(Polygon.Points[0], Polygon.Points[Polygon.Points.Count - 1]);

            Points.AddRange(Polygon.Points);

            // Hole constraints
            if (Polygon.Holes != null)
            {
                foreach (Polygon.Polygon p in Polygon.Holes)
                {
                    for (int i = 0; i < p.Points.Count - 1; i++)
                        DTSweep.CreateSweepConstraint(p.Points[i], p.Points[i + 1]);

                    DTSweep.CreateSweepConstraint(p.Points[0], p.Points[p.Points.Count - 1]);
                    Points.AddRange(p.Points);
                }
            }

            double xmax, xmin;
            double ymax, ymin;

            xmax = xmin = Points[0].X;
            ymax = ymin = Points[0].Y;

            // Calculate bounds. Should be combined with the sorting
            foreach (PolygonPoint p in Points)
            {
                if (p.X > xmax) xmax = p.X;
                if (p.X < xmin) xmin = p.X;
                if (p.Y > ymax) ymax = p.Y;
                if (p.Y < ymin) ymin = p.Y;
            }

            double deltaX = ALPHA * (xmax - xmin);
            double deltaY = ALPHA * (ymax - ymin);
            PolygonPoint p1 = new PolygonPoint(xmax + deltaX, ymin - deltaY);
            PolygonPoint p2 = new PolygonPoint(xmin - deltaX, ymin - deltaY);

            Head = p1;
            Tail = p2;

            // Sort the points along y-axis
            Points.Sort(_comparator);
        }
    }
}