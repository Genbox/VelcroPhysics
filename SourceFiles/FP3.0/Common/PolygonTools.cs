using Microsoft.Xna.Framework;

namespace FarseerPhysics
{
    public static class PolygonTools
    {
        /// <summary>
        /// Build vertices to represent an axis-aligned box.
        /// </summary>
        /// <param name="hx">the half-width.</param>
        /// <param name="hy">the half-height.</param>
        /// <param name="vertices">The vertices.</param>
        public static Vertices CreateBox(float hx, float hy)
        {
            Vertices vertices = new Vertices(4);
            vertices[0] = new Vector2(-hx, -hy);
            vertices[1] = new Vector2(hx, -hy);
            vertices[2] = new Vector2(hx, hy);
            vertices[3] = new Vector2(-hx, hy);

            return vertices;
        }

        /// <summary>
        /// Build vertices to represent an oriented box.
        /// </summary>
        /// <param name="hx">the half-width.</param>
        /// <param name="hy">the half-height.</param>
        /// <param name="center">the center of the box in local coordinates.</param>
        /// <param name="angle">the rotation of the box in local coordinates.</param>
        /// <param name="vertices">The vertices.</param>
        public static Vertices CreateBox(float hx, float hy, Vector2 center, float angle)
        {
            Vertices vertices = CreateBox(hx, hy);

            Transform xf = new Transform();
            xf.Position = center;
            xf.R.Set(angle);

            // Transform vertices and normals.
            for (int i = 0; i < 4; ++i)
            {
                vertices[i] = MathUtils.Multiply(ref xf, vertices[i]);
            }

            return vertices;
        }

        /// <summary>
        /// Set this as a single edge.
        /// </summary>
        /// <param name="v1">The first point.</param>
        /// <param name="v2">The second point.</param>
        /// <param name="vertices">The vertices.</param>
        public static Vertices CreateEdge(Vector2 v1, Vector2 v2)
        {
            Vertices vertices = new Vertices(2);
            vertices[0] = v1;
            vertices[1] = v2;

            return vertices;
        }
    }
}
