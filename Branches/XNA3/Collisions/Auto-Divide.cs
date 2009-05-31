using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Dynamics;

#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <summary>
    /// Splits large geoms into multiple smaller geoms for better performance.
    /// </summary>
    public static class AutoDivide
    {
        /// <summary>
        /// Creates a list of vertices, each of which are a
        /// triangle, from a set of vertices.
        /// </summary>
        /// <param name="vertices">The set of vertices. Must be a closed set.</param>
        /// <param name="createTriangles">Create only triangles. (Must be true for now)</param>
        /// <returns>A list of divided vertices</returns>
        public static List<Vertices> DivideVertices(Vertices vertices, bool createTriangles)
        {
            List<Vertices> split = new List<Vertices>();   // a list holding all vertices as they're split
            Vector2[] triangles;
            short[] indices;
            int j = 0;
            Vertices temp = new Vertices();

            // split the vertices up into a set of triangles
            Vertices.Triangulate(vertices.ToArray(), Vertices.WindingOrder.CounterClockwise, out triangles, out indices);

            if (createTriangles)
            {
                // for each triangle
                for (int i = 0; i < indices.Length / 3; i++)
                {
                    temp.Add(triangles[indices[j]]);
                    j++;
                    temp.Add(triangles[indices[j]]);
                    j++;
                    temp.Add(triangles[indices[j]]);
                    j++;
                    
                    split.Add(new Vertices(temp));
                    temp.Clear();
                }
            }

            return split;
        }

        /// <summary>
        /// Creates a list of geoms, each of which are a
        /// triangle, from a set of vertices.
        /// </summary>
        /// <param name="vertices">The set of vertices. Must be a closed set.</param>
        /// <param name="body">The body you want the geoms to belong too.</param>
        /// <returns>A list of divided geometries</returns>
        public static List<Geom> DivideGeom(Vertices vertices, Body body)
        {
            List<Geom> geomsList = new List<Geom>();

            List<Vertices> verts = DivideVertices(vertices, true);

            Vector2 a = vertices.GetCentroid();

            foreach (Vertices v in verts)
            {
                if (v.Count > 0)
                {
                    Vector2 b = v.GetCentroid();

                    geomsList.Add(GeomFactory.Instance.CreatePolygonGeom(body, v, b - a, 0.0f, 1.0f));
                }
            }
            return geomsList;
        }
    }
}