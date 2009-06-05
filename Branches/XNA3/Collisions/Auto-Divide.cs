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
        /// <returns>A list of divided vertices</returns>
        public static List<Vertices> TriangulateVertices(Vertices vertices)
        {
            List<Vertices> split = new List<Vertices>();   // a list holding all vertices as they're split
            Vector2[] triangles;
            short[] indices;
            int j = 0;
            Vertices temp = new Vertices();

            // split the vertices up into a set of triangles
            Vertices.Triangulate(vertices.ToArray(), Vertices.WindingOrder.CounterClockwise, out triangles, out indices);

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

            return split;
        }

        /// <summary>
        /// Creates a list of geoms, each of which are a
        /// triangle, from a set of vertices.
        /// </summary>
        /// <param name="vertices">The set of vertices. Must be a closed set.</param>
        /// <param name="body">The body you want the geoms to belong too.</param>
        /// <returns>A list of divided geometries</returns>
        public static List<Geom> TriangulateGeom(Vertices vertices, Body body)
        {
            List<Geom> geomsList = new List<Geom>();

            List<Vertices> verts = TriangulateVertices(vertices);

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

        /// <summary>
        /// Decomposes a set of vertices into a set of vertices.
        /// </summary>
        /// <param name="vertices">Vertices to decompose.</param>
        /// <param name="maxPolysToFind">Maximum Vertices to return.</param>
        /// <returns>A list of Vertices.</returns>
        public static List<Vertices> DecomposeVertices(Vertices vertices, int maxPolysToFind)
        {
            Vertices[] verts = Polygon.DecomposeVertices(vertices, maxPolysToFind);

            return new List<Vertices>(verts);
        }


        /// <summary>
        /// Decomposes a set of vertices into a set of Geoms all
        /// attached to one body.
        /// </summary>
        /// <param name="vertices">Vertices to decompose.</param>
        /// <param name="body">Body to attach too.</param>
        /// <param name="maxPolysToFind">Maximum Geoms to return.</param>
        /// <returns>A list of Geoms.</returns>
        public static List<Geom> DecomposeGeom(Vertices vertices, Body body, int maxPolysToFind)
        {
            Vertices[] verts = Polygon.DecomposeVertices(vertices, maxPolysToFind);

            List<Geom> geomList = new List<Geom>();

            Vector2 mainCentroid = vertices.GetCentroid();

            foreach (Vertices v in verts)
            {
                Vector2 subCentroid = v.GetCentroid();
                geomList.Add(new Geom(body, v, -mainCentroid, 0, 1.0f));
            }

            return geomList;
        }
    }
}