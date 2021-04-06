namespace VelcroPhysics.Tools.Triangulation.TriangulationBase
{
    public enum TriangulationAlgorithm
    {
        /// <summary>
        /// Convex decomposition algorithm using ear clipping
        /// Properties:
        /// - Only works on simple polygons.
        /// - Does not support holes.
        /// - Running time is O(n^2), n = number of vertices.
        /// </summary>
        Earclip,

        /// <summary>
        /// Convex decomposition algorithm created by Mark Bayazit (http://mnbayazit.com/)
        /// Properties:
        /// - Tries to decompose using polygons instead of triangles.
        /// - Tends to produce optimal results with low processing time.
        /// - Running time is O(nr), n = number of vertices, r = reflex vertices.
        /// - Does not support holes.
        /// </summary>
        Bayazit,

        /// <summary>
        /// Convex decomposition algorithm created by unknown
        /// Properties:
        /// - No support for holes
        /// - Very fast
        /// - Only works on simple polygons
        /// - Only works on counter clockwise polygons
        /// </summary>
        Flipcode,

        /// <summary>
        /// Convex decomposition algorithm created by Raimund Seidel
        /// Properties:
        /// - Decompose the polygon into trapezoids, then triangulate.
        /// - To use the trapezoid data, use ConvexPartitionTrapezoid()
        /// - Generate a lot of garbage due to incapsulation of the Poly2Tri library.
        /// - Running time is O(n log n), n = number of vertices.
        /// - Running time is almost linear for most simple polygons.
        /// - Does not care about winding order.
        /// </summary>
        Seidel,
        SeidelTrapezoids,

        /// <summary>
        /// 2D constrained Delaunay triangulation algorithm.
        /// Based on the paper "Sweep-line algorithm for constrained Delaunay triangulation" by V. Domiter and and B. Zalik
        /// Properties:
        /// - Creates triangles with a large interior angle.
        /// - Supports holes
        /// - Generate a lot of garbage due to incapsulation of the Poly2Tri library.
        /// - Running time is O(n^2), n = number of vertices.
        /// - Does not care about winding order.
        /// </summary>
        Delauny
    }
}