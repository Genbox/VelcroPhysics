/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
*/

using System.Collections.Generic;
using FarseerPhysics.Common.Decomposition.CDT;
using FarseerPhysics.Common.Decomposition.CDT.Delaunay;
using FarseerPhysics.Common.Decomposition.CDT.Delaunay.Sweep;
using FarseerPhysics.Common.Decomposition.CDT.Polygon;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.Decomposition
{
    /// <summary>
    /// 2D constrained Delaunay triangulation algorithm.
    /// </summary>
    /// <remarks>
    /// Based on the paper "Sweep-line algorithm for constrained Delaunay triangulation" by V. Domiter and and B. Zalik
    /// Source: http://code.google.com/p/poly2tri/
    /// </remarks>
    public static class CDTDecomposer
    {
        /// <summary>
        /// Creates a list of triangles based on the given polygon.
        /// You can use output from TextureConverter to 
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static List<Vertices> ConvexPartition(Vertices vertices)
        {
            Polygon poly = new Polygon();

            foreach (Vector2 vertex in vertices)
                poly.Points.Add(new TriangulationPoint(vertex.X, vertex.Y));

            if (vertices.Holes != null)
            {
                foreach (Vertices holeVertices in vertices.Holes)
                {
                    Polygon hole = new Polygon();

                    foreach (Vector2 vertex in holeVertices)
                        hole.Points.Add(new TriangulationPoint(vertex.X, vertex.Y));

                    poly.AddHole(hole);
                }
            }

            DTSweepContext tcx = new DTSweepContext();
            tcx.PrepareTriangulation(poly);
            DTSweep.Triangulate(tcx);

            List<Vertices> results = new List<Vertices>();

            foreach (DelaunayTriangle triangle in poly.Triangles)
            {
                Vertices v = new Vertices();
                foreach (TriangulationPoint p in triangle.Points)
                {
                    v.Add(new Vector2((float)p.X, (float)p.Y));
                }
                results.Add(v);
            }

            return results;
        }

        public static List<Vertices> ConvexPartition(List<Vertices> vertices)
        {
            List<Vertices> result = new List<Vertices>();

            foreach (Vertices e in vertices)
                result.AddRange(ConvexPartition(e));

            return result;
        }
    }
}