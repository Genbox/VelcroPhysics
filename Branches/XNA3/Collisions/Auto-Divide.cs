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
        public static List<Vertices> DivideVertices(Vertices v, bool createTriangles)
        {
            List<Vertices> split = new List<Vertices>();   // a list holding all vertices as they're split
            Vector2[] triangles;
            short[] indices;
            int j = 0;
            Vertices temp = new Vertices();

            // split the vertices up into a set of triangles
            Vertices.Triangulate(v.ToArray(), Vertices.WindingOrder.CounterClockwise, out triangles, out indices);

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

        public static List<Geom> DivideGeom(Vertices vertices, Body body)
        {
            List<Geom> geomsList = new List<Geom>();

            List<Vertices> verts = AutoDivide.DivideVertices(vertices, true);

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